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
        private string _user;
        private string _password;

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

        public Sprint GetSprint(string sprintId)
        {
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://issues.umbraco.org/rest/admin/agile/94-11/sprint/" + sprintId);
            var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            request.Headers.Accept.Add(mediaType);
            var response = httpClient.SendAsync(request).Result;
            var json = response.Content.ReadAsStringAsync().Result;
            var sprint = JsonConvert.DeserializeObject<Sprint>(json);
            return sprint;
        }

        public AgileSettings GetSettings()
        {
            var baseAddress = new Uri("http://issues.umbraco.org");
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = cookieContainer };
            var httpClient = new HttpClient(handler) { BaseAddress = baseAddress };

            var loginRequest = new HttpRequestMessage(HttpMethod.Post, "http://issues.umbraco.org/rest/user/login");
            loginRequest.Content = new FormUrlEncodedContent(new []
            {
                new KeyValuePair<string, string>("login", User),
                new KeyValuePair<string, string>("password", Password),
            });
            var loginResponse = httpClient.SendAsync(loginRequest).Result;
            if (!loginResponse.IsSuccessStatusCode)
            {
                var text = loginResponse.Content.ReadAsStringAsync().Result;
                throw new Exception("cannot login: " + text);
            }

            //var text = loginResponse.Content.ReadAsStringAsync().Result;
            //throw new Exception(text);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://issues.umbraco.org/rest/admin/agile/94-11");
            var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            request.Headers.Accept.Add(mediaType);
            var response = httpClient.SendAsync(request).Result;
            var json = response.Content.ReadAsStringAsync().Result;
            //throw new Exception(json);
            var agileSettings = JsonConvert.DeserializeObject<AgileSettings>(json);
            if (agileSettings.Sprints == null)
                throw new Exception(json);
            return agileSettings;
        }
    }
}
