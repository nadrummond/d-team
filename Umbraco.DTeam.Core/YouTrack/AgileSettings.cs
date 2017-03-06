using System.Collections.Generic;
using Newtonsoft.Json;

namespace Umbraco.DTeam.Core.YouTrack
{
    public class AgileSettings
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Sprint> Sprints { get; set; }

        public class Sprint
        {
            public string Id { get; set; }
        }
    }
}
