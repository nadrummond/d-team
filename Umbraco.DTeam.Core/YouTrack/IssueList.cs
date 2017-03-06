using System.Collections.Generic;
using Newtonsoft.Json;

namespace Umbraco.DTeam.Core.YouTrack
{
    public class IssueList
    {
        [JsonProperty("issue")]
        public List<Issue> Issues { get; set; }
    }
}
