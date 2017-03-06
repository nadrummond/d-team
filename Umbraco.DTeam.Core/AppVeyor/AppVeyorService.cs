using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RestSharp;
using Umbraco.Core.IO;

namespace ZBuildMon.AppVeyor
{
    public class AppVeyorService
    {
        private string _apiToken;

        private readonly RestClient _client;

        private string ApiToken
        {
            get
            {
                if (_apiToken == null)
                {
                    using (var stream = File.OpenRead(IOHelper.MapPath("~/App_Data/appveyor.cfg")))
                    using (var reader = new StreamReader(stream))
                    {
                        _apiToken = reader.ReadLine();
                    }
                }
                return _apiToken;
            }
        }

        public AppVeyorService()
        {
            _client = new RestClient(new Uri("https://ci.appveyor.com/api"));
        }

        public List<Project> GetProjects()
        {
            var request = new RestRequest("projects", Method.GET);

            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {ApiToken}");

            IRestResponse<List<Project>> response = _client.Execute<List<Project>>(request);

            if (response.ErrorException != null) throw response.ErrorException;
            return response.Data;
        }

        public BuildRoot GetProject(string branch = null)
        {
            // get project last build
            var request = new RestRequest("projects/Umbraco/umbraco-cms-hs8dx" + (branch == null ? "" : $"/branch/{branch}"), Method.GET);

            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {ApiToken}");

            var response = _client.Execute<BuildRoot>(request);

            // http://stackoverflow.com/questions/15683858/restsharp-print-raw-request-and-response-headers
            //System.Console.WriteLine(response.Content);

            // fixme - we're getting a proper JSON response that's not properly deserialized?

            if (response.ErrorException != null) throw response.ErrorException;
            return response.Data;
        }

        // no! not an api
        public object GetProjects2()
        {
            // get project last build
            var request = new RestRequest("projects/Umbraco", Method.GET);

            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {ApiToken}");

            var response = _client.Execute<object>(request);

            // http://stackoverflow.com/questions/15683858/restsharp-print-raw-request-and-response-headers
            System.Console.WriteLine(response.Content);

            // fixme - we're getting a proper JSON response that's not properly deserialized?

            if (response.ErrorException != null) throw response.ErrorException;
            return response.Data;
        }

        public BuildsRoot GetHistory(int count)
        {
            var request = new RestRequest($"projects/Umbraco/umbraco-cms-hs8dx/history?recordsNumber={count}", Method.GET);

            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {ApiToken}");

            var response = _client.Execute<BuildsRoot>(request);

            // http://stackoverflow.com/questions/15683858/restsharp-print-raw-request-and-response-headers
            //System.Console.WriteLine(response.Content);

            if (response.ErrorException != null) throw response.ErrorException;
            return response.Data;
        }

        public BuildsRoot GetBranchHistory(string branch, int count)
        {
            var request = new RestRequest($"projects/Umbraco/umbraco-cms-hs8dx/history?recordsNumber={count}&branch={branch}", Method.GET);

            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", $"Bearer {ApiToken}");

            var response = _client.Execute<BuildsRoot>(request);

            // http://stackoverflow.com/questions/15683858/restsharp-print-raw-request-and-response-headers
            //System.Console.WriteLine(response.Content);

            if (response.ErrorException != null) throw response.ErrorException;
            return response.Data;
        }

        // history:
        // GET /api/projects/{accountName}/{projectSlug}/history?recordsNumber={records-per-page}[&startBuildId={buildId}&branch={branch}]
        // then
        // get history
        // get well-known (configured) branches
        // merge
        // determine status + report
        //   blue flash = working
        //   red = well-known branch is dead
        //   orange = other (eg PR) branch is dead
        //   green = all working


        public Dictionary<string, bool?> GetBuilds(bool allBuilds, int count, params string[] knownBranches)
        {
            var builds = new List<Build>();

            var history = GetHistory(count);
            builds.AddRange(history.Builds.Where(x => (x.Status == "success" || x.Status == "failed") && x.PullRequestId == null));

            foreach (var branch in knownBranches)
            {
                if (builds.Any(x => x.Branch == branch /*&& x.PullRequestId == null*/)) continue;

                var projectHistory = GetBranchHistory(branch, count);

                var build = projectHistory.Builds.FirstOrDefault(x => (x.Status == "success" || x.Status == "failed") && x.PullRequestId == null);
                if (build == null) continue; // not going up in history
                builds.Add(build);
            }
            builds.Sort((b1, b2) =>
            {
                var d1 = b1.Finished == default(DateTime) ? DateTime.Now : b1.Finished;
                var d2 = b2.Finished == default(DateTime) ? DateTime.Now : b2.Finished;
                return d2.CompareTo(d1);
            });
            var result = new Dictionary<string, bool?>();
            foreach (var build in builds)
            {
                if (result.ContainsKey(build.Branch)) continue;
                result[build.Branch] = build.Status == "success";
            }
            foreach (var branch in knownBranches)
            {
                if (result.ContainsKey(branch)) continue;
                result[branch] = null;
            }
            result = result.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            return result;
        }
    }

    // use http://json2csharp.com/
}