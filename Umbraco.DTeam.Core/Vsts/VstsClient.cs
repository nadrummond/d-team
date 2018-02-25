using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.DTeam.Core.Vsts
{
    // see https://www.visualstudio.com/en-us/docs/integrate/api/build/builds
    // see https://umbraco.visualstudio.com/

    public class VstsClient
    {
        // version 3.0 is the current latest
        // https://www.visualstudio.com/en-us/docs/integrate/get-started/rest/basics
        private const string ApiVersion = "3.0";
        private const int Top = 32;

        // this should come from the CMS really
        private static readonly string[] Projects = {
            "Umbraco Cms",
            "Umbraco Deploy",
            "Umbraco Deploy Contrib",
            "Umbraco Forms",
            "Umbraco Duck",
        };

        private readonly HttpClient _httpClient;
        private string _token;

        public VstsClient()
        {
            var baseAddress = new Uri("https://umbraco.VisualStudio.com");
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = cookieContainer };
            _httpClient = new HttpClient(handler) { BaseAddress = baseAddress };
        }

        private string Token
        {
            get
            {
                if (_token == null) ReadConfig();
                return _token;
            }
        }

        private void ReadConfig()
        {
            using (var stream = File.OpenRead(IOHelper.MapPath("~/App_Data/vsts.cfg")))
            using (var reader = new StreamReader(stream))
            {
                _token = reader.ReadLine();
            }
        }

        public BuildsModel[] BuildsModels
        {
            get
            {
                if (!_available.HasValue)
                    _available = IsAvailable;
                return _buildsModels;
            }
        }

        private BuildsModel[] _buildsModels;
        private bool? _available;

        public bool IsAvailable
        {
            get
            {
                if (_available.HasValue) return _available.Value;
                try
                {
                    _buildsModels = Projects.Select(GetProject).ToArray();
                    _available = true;
                }
                catch (Exception ex)
                {
                    LogHelper.WarnWithException<VstsClient>("Failed to talk to VSTS.", ex);
                    _available = false;
                }
                return _available.Value;
            }
        }

        private BuildsModel GetProject(string name)
        {
            //var url = $"https://umbraco.VisualStudio.com/DefaultCollection/_apis/projects/e31cd6bd-f85b-4b61-81d9-eb7fa77eec9e?api-version={ApiVersion}";
            var url = $"https://umbraco.VisualStudio.com/DefaultCollection/{name}/_apis/build/builds?api-version={ApiVersion}&$top={Top}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            request.Headers.Accept.Add(mediaType);
            var auth = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + Token)));
            request.Headers.Authorization = auth;
            var response = _httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            var buildsModel = JsonConvert.DeserializeObject<BuildsModel>(json); // fixme or directly read as ... ?!
            buildsModel.Project = name;
            return buildsModel;
        }

        public class BuildsModel
        {
            public string Project { get; set; }

            public int Count { get; set; }

            [JsonProperty("value")]
            public List<Build> Builds { get; set; }

            [JsonIgnore]
            public IEnumerable<Build> FilteredBuilds => Builds
                .DistinctBy(x => x.SourceBranch.TrimStart("refs/heads/"))
                .Where(x => !x.SourceBranch.StartsWith("refs/pull/"))
                .OrderBy(x => x.SourceBranch);
        }

        public class Build
        {
            public string BuildNumber { get; set; }
            public string SourceBranch { get; set; }
            public string Status { get; set; }
            public string Result { get; set; }
        }
    }
}