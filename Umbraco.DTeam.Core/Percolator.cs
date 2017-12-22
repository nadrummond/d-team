using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.DTeam.Core.Models;
using Umbraco.DTeam.Core.YouTrack;
using Umbraco.Web;
using ZBuildMon.AppVeyor;

namespace Umbraco.DTeam.Core
{

    // name class randomly
    public class Percolator
    {
        private readonly IRuntimeCacheProvider _runtimeCache;

        public Percolator(IRuntimeCacheProvider runtimeCache)
        {
            _runtimeCache = runtimeCache;
        }

        private const string HomeModelCacheKey = "DTeam.HomeModel";

        private int GetSprintNumber(Sprint sprint)
        {
            if (sprint == null) return 0;
            return GetSprintNumber(sprint.Version);
        }

        private int GetSprintNumber(string sprintVersion)
        {
            if (sprintVersion.IsNullOrWhiteSpace()) return 0;

            var number = 0;
            var parts = sprintVersion.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            //a valid sprint is only 2 parts "Sprint 123" (some sprints are named invalid that we don't want to track)
            if (parts.Length != 2) return number;
            
            int.TryParse(parts[1], out number);

            return number;
        }

        private CurrentSprintModel GetYouTrack(int sprintNumber = int.MinValue)
        {
            var client = new YouTrackClient();
            client.Auth();

            var agileSettings = client.GetSettings();
            //TODO: This ordering will not work correctly because it will order by string but we really want it ordered by 
            //the sprint number
            var allSprintIds = agileSettings.Sprints.OrderByDescending(x => x.Id).ToArray();

            var now = DateTime.Now;
            Sprint currentSprint = null;
            Sprint previousSprint = null;
            Sprint nextSprint = null;
            
            // in YouTrack our sprints kinda go from monday to thursday
            // doing a bit of magic here to fix the mess...
            foreach (var sprint in allSprintIds.Select(x => client.GetSprint(x.Id)))
            {
                var sprNum = GetSprintNumber(sprint);

                //if it could not be parsed, ignore
                if (sprNum == 0)
                    continue;

                //if we are looking for a particular sprint
                if (sprintNumber != int.MinValue)
                {
                    if (sprNum == sprintNumber)
                    {
                        currentSprint = sprint;
                    }
                    else if (sprNum < sprintNumber && previousSprint == null)
                    {
                        previousSprint = sprint;
                    }
                    else if (sprNum > sprintNumber)
                    {
                        nextSprint = sprint;
                    }
                }
                else
                {
                    //we are determining the current by date

                    var start = sprint.Start.Date;
                    while (start.DayOfWeek != DayOfWeek.Monday)
                        start = start.AddDays(-1);
                    var finish = sprint.Finish.AddHours(12).Date;
                    while (finish.DayOfWeek != DayOfWeek.Sunday)
                        finish = finish.AddDays(1);
                    finish = finish.AddDays(1).AddSeconds(-1);

                    if (finish < now && (previousSprint == null || previousSprint.Start < start))
                        previousSprint = sprint;
                    if (start <= now && finish >= now)
                        currentSprint = sprint;
                    if (start > now && (nextSprint == null || nextSprint.Start > start))
                        nextSprint = sprint;
                }

                //exit if all is assigned so we don't keep looking up data
                if (previousSprint != null && currentSprint != null && nextSprint != null)
                    break;
            }

            var model = currentSprint == null ? null : new CurrentSprintModel
            {
                Name = currentSprint.Version,
                Number = GetSprintNumber(currentSprint),
                Start = currentSprint.Start.Date,
                Finish = currentSprint.Finish.AddHours(12).Date,

                PreviousSprint = previousSprint == null ? null : new SprintModel
                {
                    Name = previousSprint.Version,
                    Number = GetSprintNumber(previousSprint),
                    Start = previousSprint.Start.Date,
                    Finish = previousSprint.Finish.AddHours(12).Date
                },

                NextSprint = nextSprint == null ? null : new SprintModel
                {
                    Name = nextSprint.Version,
                    Number = GetSprintNumber(nextSprint),
                    Start = nextSprint.Start.Date,
                    Finish = nextSprint.Finish.AddHours(12).Date
                }
            };

            if (model != null)
                model.Issues = client.GetProgress(model.Number);

            return model;
        }

        private static SprintProgress GetProgress(int sprintId, List<Issue> issues, DateTime dateTime = default(DateTime))
        {
            if (dateTime == default(DateTime))
                dateTime = DateTime.Now;

            var points = new Dictionary<string, double>();
            double total = 0;
            double unscheduled = 0;
            foreach (var issue in issues)
            {
                if (issue.Type.InvariantStartsWith("feature (planned)"))
                    continue;

                var state = issue.State;
                points.TryGetValue(state, out double statePoints);
                statePoints += issue.Points;
                points[state] = statePoints;

                total += issue.Points;

                if (issue.HasTag("Unscheduled"))
                    unscheduled += issue.Points;
            }

            var progress = new SprintProgress
            {
                SprintId = sprintId,
                DateTime = dateTime,
                Points = new Dictionary<string, double>(),
                TotalPoints = total,
                Unscheduled = unscheduled
            };

            foreach (var state in new[] { "Open", "In Progress", "Review", "Reopened", "Fixed" })
            {
                points.TryGetValue(state, out double statePoints);
                progress.Points[state] = statePoints;
            }

            progress.Points["Other"] = total - progress.Points.Sum(x => x.Value);

            return progress;
        }

        private static Dictionary<string, bool?> GetAppVeyor()
        {
            var appVeyor = new AppVeyorService();
            return appVeyor.GetBuilds(true, 24, "dev-v7", "dev-v7.6", "dev-v8");
        }

        // gets the home model
        public HomeModel GetHomeModel(UmbracoHelper umbraco, IEnumerable<ContentModels.Sprint> sprints, bool caching = true, int sprintNumber = int.MinValue)
        {
            var cache = _runtimeCache;
            return caching
                ? (HomeModel) cache.GetCacheItem(HomeModelCacheKey + sprintNumber, () => GetHomeModelNoCache(umbraco, sprints, sprintNumber), TimeSpan.FromMinutes(5))
                : GetHomeModelNoCache(umbraco, sprints, sprintNumber);
        }

        private HomeModel GetHomeModelNoCache(UmbracoHelper umbraco, IEnumerable<ContentModels.Sprint> sprints, int sprintNumber)
        {
            if (umbraco == null) throw new ArgumentNullException(nameof(umbraco));

            var model = GetYouTrack(sprintNumber);
            if (model == null) return null;

            sprintNumber = (sprintNumber == int.MinValue ? model.Number : sprintNumber);

            model.Content = sprints.FirstOrDefault(x => x.SprintId == sprintNumber);

            //If the content item is null we should create it
            if (model.Content == null)
            {
                var contentService = umbraco.UmbracoContext.Application.Services.ContentService;
                var container = contentService.GetChildrenByName(contentService.GetRootContent().First().Id, "Sprints").First();
                var sprintName = "Sprint " + sprintNumber;
                var sprintDoc = contentService.GetChildrenByName(container.Id, sprintName).FirstOrDefault() 
                    ?? contentService.CreateContent(sprintName, container, "sprint");

                sprintDoc.SetValue("sprintId", sprintNumber);
                var publish = contentService.PublishWithStatus(sprintDoc);
                if (!publish)
                    throw new InvalidOperationException($"Could not publish the Sprint {sprintNumber} document: {publish.Result.StatusType}", publish.Exception);
                
                model.Content = (ContentModels.Sprint)umbraco.TypedContent(sprintDoc.Id);
                if (model.Content == null)
                    throw new InvalidOperationException($"Could not get the Sprint {sprintNumber} document from cache");
            }

            model.Progress = new List<SprintProgress>();

            var finish = model.NextSprint?.Start ?? model.Finish.AddDays(1);

            var progress = GetProgress(sprintNumber, model.Issues, finish);

            var history = SprintProgress.Get(sprintNumber).ToArray();

            var d = model.Start;
            
            while (d < finish)
            {
                var nd = d.AddHours(12);
                if (progress.DateTime > d && progress.DateTime <= nd)
                {
                    model.Progress.Add(progress);
                }
                else
                {
                    var h = history.FirstOrDefault(x => x.DateTime > d && x.DateTime <= nd);
                    //add even if its null
                    model.Progress.Add(h);

                }
                d = nd;
            }

            model.TotalPoints = progress.TotalPoints;
            model.UnscheduledPoints = progress.Unscheduled;

            var builds = GetAppVeyor();

            return new HomeModel
            {
                CurrentSprint = model,
                Builds = builds
            };
        }

        public SprintProgress CaptureProgress()
        {
            var youTrack = GetYouTrack();
            if (youTrack == null) return null;

            return GetProgress(youTrack.Number, youTrack.Issues);
        }
    }
}
