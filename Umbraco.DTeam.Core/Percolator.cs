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
        private const string YoutrackCacheKey = "DTeam.YouTrack";
        private const string AppVeyorCacheKey = "DTeam.AppVeyor";

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
            {
                model.Issues = client.GetProgress(model.Number);

                double unscheduled = 0;
                double points = 0;
                foreach (var issue in model.Issues)
                {
                    if (issue.Type.InvariantStartsWith("feature (planned)"))
                        continue;

                    points += issue.Points;
                    if (issue.HasTag("Unscheduled"))
                        unscheduled += issue.Points;
                }
                model.UnscheduledPoints = unscheduled;
                model.TotalPoints = points;
            }

            return model;
        }

        private static void CalculateProgress(CurrentSprintModel model)
        {
            if (model == null) return;

            var progressPoints = new Dictionary<string, double>();
            var progressPercent = new Dictionary<string, int>();

            foreach (var issue in model.Issues)
            {
                if (issue.Type.InvariantStartsWith("feature (planned)"))
                    continue;

                double points;
                var state = issue.State;
                progressPoints.TryGetValue(state, out points);
                points += issue.Points;
                progressPoints[state] = points;
            }

            var countPercent = 0;
            double countPoints = 0;
            foreach (var state in new[] { "Open", "In Progress", "Review", "Reopened", "Fixed" })
            {
                if (!progressPoints.ContainsKey(state))
                    progressPoints[state] = 0;
                countPercent += progressPercent[state] = Convert.ToInt32(Math.Floor(progressPoints[state] * 100 / model.TotalPoints));
                countPoints += progressPoints[state];
            }

            const int totalPercent = 100;
            progressPercent["Other"] = totalPercent - countPercent;        
            progressPoints["Other"] = model.TotalPoints - countPoints;

            model.ProgressPercent = progressPercent;
            model.ProgressPoints = progressPoints;

            model.Percent = Convert.ToInt32(Math.Floor(100 * (DateTime.Now - model.Start).TotalMilliseconds / (model.Finish - model.Start).TotalMilliseconds));
            if (model.Percent < 5) model.Percent = 5;
            if (model.Percent > 100) model.Percent = 100;
        }

        private Dictionary<string, bool?> GetAppVeyor()
        {
            var appVeyor = new AppVeyorService();
            return appVeyor.GetBuilds(true, 24, "dev-v7", "dev-v7.6", "dev-v8");
        }

        // gets the home model
        public HomeModel GetHomeModel(IEnumerable<ContentModels.Sprint> sprints, bool caching = true)
        {
            var cache = ApplicationContext.Current.ApplicationCache.RuntimeCache;

            var model = caching
                ? (CurrentSprintModel) cache.GetCacheItem(YoutrackCacheKey, GetYouTrack, TimeSpan.FromMinutes(5))
                : GetYouTrack();

            if (model == null) return null;

            model.Content = sprints.FirstOrDefault(x => x.SprintId == model.Number);

            CalculateProgress(model);

            var builds = caching
                ? (Dictionary<string, bool?>) cache.GetCacheItem(AppVeyorCacheKey, GetAppVeyor, TimeSpan.FromMinutes(5))
                : GetAppVeyor();

            return new HomeModel
            {
                CurrentSprint = model,
                Builds = builds
            };
        }
    }
}
