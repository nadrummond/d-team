using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.DTeam.Core.Models;
using Umbraco.DTeam.Core.YouTrack;
using ZBuildMon.AppVeyor;

namespace Umbraco.DTeam.Core
{
    // name class randomly
    public class Percolator
    {
        private const string HomeModelCacheKey = "DTeam.HomeModel";

        private int GetSprintNumber(Sprint sprint)
        {
            if (sprint == null) return 0;

            var number = 0;
            var pos = sprint.Version.LastIndexOf(" ", StringComparison.OrdinalIgnoreCase);

            if (pos > 0)
                int.TryParse(sprint.Version.Substring(pos + 1), out number);

            return number;
        }

        private CurrentSprintModel GetYouTrack()
        {
            var client = new YouTrackClient();
            client.Auth();

            var agileSettings = client.GetSettings();
            var allSprintIds = agileSettings.Sprints.OrderByDescending(x => x.Id);

            var now = DateTime.Now;
            Sprint currentSprint = null;
            Sprint previousSprint = null;
            Sprint nextSprint = null;

            // in YouTrack our sprints kinda go from monday to thursday
            // doing a bit of magic here to fix the mess...

            foreach (var sprint in allSprintIds.Select(x => client.GetSprint(x.Id)))
            {
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
        public HomeModel GetHomeModel(IEnumerable<ContentModels.Sprint> sprints, bool caching = true)
        {
            var cache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
            return caching
                ? (HomeModel) cache.GetCacheItem(HomeModelCacheKey, () => GetHomeModelNoCache(sprints), TimeSpan.FromMinutes(5))
                : GetHomeModelNoCache(sprints);
        }

        private HomeModel GetHomeModelNoCache(IEnumerable<ContentModels.Sprint> sprints)
        {
            var model = GetYouTrack();
            if (model == null) return null;

            model.Content = sprints.FirstOrDefault(x => x.SprintId == model.Number);
            model.Progress = new List<SprintProgress>();

            var progress = GetProgress(model.Number, model.Issues);

            var history = SprintProgress.Get(model.Number).ToArray();

            var d = model.Start;
            var finish = model.NextSprint?.Start ?? model.Finish.AddDays(1);
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
