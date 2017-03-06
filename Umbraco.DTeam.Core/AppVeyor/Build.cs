using System;
using System.Collections.Generic;

namespace ZBuildMon.AppVeyor
{
    public class Build
    {
        public int BuildId { get; set; }
        public int BuildNumber { get; set; }
        public string Version { get; set; }
        public string Message { get; set; }
        public string Branch { get; set; }
        public List<Job> Jobs { get; set; }
        public DateTime Finished { get; set; }
        public string Status { get; set; }
        public bool IsTag { get; set; }
        public string CommitId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorUsername { get; set; }
        public string CommitterName { get; set; }
        public string CommitterUsername { get; set; }
        public string Committed { get; set; }
        public string PullRequestId { get; set; }
        public string PullRequestName { get; set; }
        public List<string> Messages { get; set; }
        public DateTime Started { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
