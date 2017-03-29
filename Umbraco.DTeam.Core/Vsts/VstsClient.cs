using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Umbraco.Core.IO;

namespace Umbraco.DTeam.Core.Vsts
{
    // see https://www.visualstudio.com/en-us/docs/integrate/api/build/builds
    // see https://umbraco.visualstudio.com/

    public class VstsClient
    {
        // version 3.0 is the current latest
        // https://www.visualstudio.com/en-us/docs/integrate/get-started/rest/basics
        private const string ApiVersion = "3.0";

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

        public BuildsModel Test()
        {
            //var url = $"https://umbraco.VisualStudio.com/DefaultCollection/_apis/projects/e31cd6bd-f85b-4b61-81d9-eb7fa77eec9e?api-version={ApiVersion}";
            var url = $"https://umbraco.VisualStudio.com/DefaultCollection/e31cd6bd-f85b-4b61-81d9-eb7fa77eec9e/_apis/build/builds?api-version={ApiVersion}&$top=20";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            request.Headers.Accept.Add(mediaType);
            var auth = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + Token)));
            request.Headers.Authorization = auth;
            var response = _httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<BuildsModel>(json); // fixme or directly read as ... ?!
        }

        public class BuildsModel
        {
            public int Count { get; set; }

            [JsonProperty("value")]
            public List<Build> Builds { get; set; }
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