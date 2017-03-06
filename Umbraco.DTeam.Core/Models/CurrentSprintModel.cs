using System;
using System.Collections.Generic;
using Umbraco.DTeam.Core.YouTrack;
using Sprint = Umbraco.DTeam.ContentModels.Sprint;

namespace Umbraco.DTeam.Core.Models
{
    public class CurrentSprintModel
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
        public Sprint Content { get; set; }
        public List<Issue> Issues { get; set; }
    }
}
