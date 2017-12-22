using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Umbraco.Core.Logging;

namespace Umbraco.DTeam.Core.GitHub
{
    public class GitHubClient
    {
        private readonly HttpClient _client;

        public GitHubClient()
        {
            _client = new HttpClient(new HttpClientHandler
            {
                UseProxy = true
            })
            {
                BaseAddress = new Uri("https://api.github.com/")
            };
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
        }

        public GithubSearchResult GetNumberOfOpenPullRequests()
        {
            var openPullRequestUrl = "search/issues?q=is:pr+repo:umbraco/umbraco-cms+state:open";
            var githubSearchResult = new GithubSearchResult()
            {
                TotalCount = -1
            };
            try
            {
                githubSearchResult = JsonConvert.DeserializeObject<GithubSearchResult>(_client.GetAsync(openPullRequestUrl).Result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                LogHelper.WarnWithException<GitHubClient>("Error while fetching PR count from Github via url {0}", ex, (Func<object>) (() => openPullRequestUrl));
            }
            return githubSearchResult;
        }
    }
}
