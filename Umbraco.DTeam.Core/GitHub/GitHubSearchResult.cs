// Decompiled with JetBrains decompiler
// Type: Umbraco.DTeam.Core.GitHub.GithubSearchResult
// Assembly: Umbraco.DTeam.Core, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 9C2125CE-8AAC-4B0A-9E34-E96211E4B131
// Assembly location: C:\Users\Shannon\Downloads\Umbraco.DTeam.Core.dll

using Newtonsoft.Json;

namespace Umbraco.DTeam.Core.GitHub
{
    public class GithubSearchResult
    {
        [JsonProperty("total_count")]
        public int TotalCount { get; set; }
    }
}
