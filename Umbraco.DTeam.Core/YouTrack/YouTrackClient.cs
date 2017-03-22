using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Umbraco.Core.IO;

namespace Umbraco.DTeam.Core.YouTrack
{
    public class YouTrackClient
    {
        private readonly HttpClient _httpClient;
        private string _user;
        private string _password;

        public YouTrackClient()
        {
            var baseAddress = new Uri("http://issues.umbraco.org");
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = cookieContainer };
            _httpClient = new HttpClient(handler) { BaseAddress = baseAddress };
        }

        private string User
        {
            get
            {
                if (_user == null) ReadConfig();
                return _user;
            }
        }

        private string Password
        {
            get
            {
                if (_password == null) ReadConfig();
                return _password;
            }
        }

        private void ReadConfig()
        {
            using (var stream = File.OpenRead(IOHelper.MapPath("~/App_Data/youtrack.cfg")))
            using (var reader = new StreamReader(stream))
            {
                _user = reader.ReadLine();
                _password = reader.ReadLine();
            }
        }

        public void Auth()
        {
            var loginRequest = new HttpRequestMessage(HttpMethod.Post, "http://issues.umbraco.org/rest/user/login");
            loginRequest.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("login", User),
                new KeyValuePair<string, string>("password", Password),
            });
            var loginResponse = _httpClient.SendAsync(loginRequest).Result;
            if (!loginResponse.IsSuccessStatusCode)
            {
                var text = loginResponse.Content.ReadAsStringAsync().Result;
                throw new Exception("cannot login: " + text);
            }
        }

        public List<Issue> GetProgress(int sprintId)
        {
            var url = "http://issues.umbraco.org/rest/issue?filter=Sprint:%20{{Sprint%20{0}}}&max=256&with=type&with=state&with=summary&with=tag&with=story%20points";
            var request = new HttpRequestMessage(HttpMethod.Get, string.Format(url, sprintId));
            var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            request.Headers.Accept.Add(mediaType);
            var response = _httpClient.SendAsync(request).Result;
            var json = response.Content.ReadAsStringAsync().Result;
            var issues = JsonConvert.DeserializeObject<IssueList>(json);
            return issues.Issues;
        }

        public Sprint GetSprint(string sprintId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://issues.umbraco.org/rest/admin/agile/94-11/sprint/" + sprintId);
            var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            request.Headers.Accept.Add(mediaType);
            var response = _httpClient.SendAsync(request).Result;
            var json = response.Content.ReadAsStringAsync().Result;
            var sprint = JsonConvert.DeserializeObject<Sprint>(json);
            return sprint;
        }

        public AgileSettings GetSettings()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://issues.umbraco.org/rest/admin/agile/94-11");
            var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            request.Headers.Accept.Add(mediaType);
            var response = _httpClient.SendAsync(request).Result;
            var json = response.Content.ReadAsStringAsync().Result;
            //throw new Exception(json);
            var agileSettings = JsonConvert.DeserializeObject<AgileSettings>(json);
            if (agileSettings.Sprints == null)
                throw new Exception(json);
            return agileSettings;
        }
    }
}
