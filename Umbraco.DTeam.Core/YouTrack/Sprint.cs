using System;
using Newtonsoft.Json;
using Umbraco.DTeam.Core.Serialization;

namespace Umbraco.DTeam.Core.YouTrack
{
    public class Sprint
    {
        public string Id { get; set; }
        public string Version { get; set; }
        [JsonConverter(typeof(EpochDateTimeConverter))]
        public DateTime Start { get; set; }
        [JsonConverter(typeof(EpochDateTimeConverter))]
        public DateTime Finish { get; set; }
    }
}
