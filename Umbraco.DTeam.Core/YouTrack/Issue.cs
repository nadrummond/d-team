using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;

namespace Umbraco.DTeam.Core.YouTrack
{
    public class Issue
    {
        public string Id { get; set; }
        public string EntityId { get; set; }

        [JsonProperty("field")]
        public List<Field> Fields { get; set; }

        [JsonProperty("tag")]
        public List<Tag> Tags { get; set; }

        [JsonIgnore]
        public string Summary
        {
            get { return Fields.FirstOrDefault(x => x.Name.InvariantEquals("summary")).Value.ToString(); }
        }

        [JsonIgnore]
        public string Type
        {
            get
            {
                var value = Fields.FirstOrDefault(x => x.Name.InvariantEquals("type")).Value;
                var a = value as JArray;
                if (a != null) return a[0].ToString();
                return value.ToString();
            }
        }

        [JsonIgnore]
        public string State
        {
            get
            {
                var value = Fields.FirstOrDefault(x => x.Name.InvariantEquals("state")).Value;
                var a = value as JArray;
                if (a != null) return a[0].ToString();
                return value.ToString();
            }
        }

        public bool HasTag(string tag)
        {
            return Tags.Any(x => x.Name.InvariantEquals(tag));
        }

        public class Field
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }

        public class Tag
        {
            [JsonProperty("cssClass")]
            public string CssClass { get; set; }

            [JsonProperty("value")]
            public string Name { get; set; }
        }
    }
}
