using System;

namespace ZBuildMon.AppVeyor
{
    public class Job
    {
        public string JobId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime Finished { get; set; }
        public bool AllowFailure { get; set; }
        public int MessagesCount { get; set; }
        public int CompilationMessagesCount { get; set; }
        public int CompilationErrorsCount { get; set; }
        public int CompilationWarningsCount { get; set; }
        public int TestsCount { get; set; }
        public int PassedTestsCount { get; set; }
        public int FailedTestsCount { get; set; }
        public int ArtifactsCount { get; set; }
        public DateTime Started { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

    }
}