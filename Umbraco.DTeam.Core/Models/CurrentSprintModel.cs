using System;
using System.Collections.Generic;
using Umbraco.DTeam.Core.YouTrack;
using Sprint = Umbraco.DTeam.ContentModels.Sprint;

namespace Umbraco.DTeam.Core.Models
{
    // many classes in same file, how confusing

    public class SprintModel
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
    }

    public class CurrentSprintModel : SprintModel
    {
        public Sprint Content { get; set; }
        public List<Issue> Issues { get; set; }
        public SprintModel PreviousSprint { get; set; }
        public SprintModel NextSprint { get; set; }
        public Dictionary<string, int> Progress { get; set; }
        public int Percent { get; set; }
    }

    public class HomeModel
    {
        public CurrentSprintModel CurrentSprint { get; set; }
        public Dictionary<string, bool?> Builds { get; set; }
    }
}
