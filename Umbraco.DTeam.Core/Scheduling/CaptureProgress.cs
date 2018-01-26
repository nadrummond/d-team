using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Scheduling;

namespace Umbraco.DTeam.Core.Scheduling
{
    public class CaptureProgress : RecurringTaskBase
    {
        private const int DelayMilliseconds = 10 * 1000; // 10s
        private const int PeriodMilliseconds = 10 * 60 * 1000; // 10 mins

        public CaptureProgress(BackgroundTaskRunner<IBackgroundTask> runner)
            : base(runner, DelayMilliseconds, PeriodMilliseconds)
        { }

        public override bool PerformRun()
        {
            var perco = new Percolator(ApplicationContext.Current.ApplicationCache.RuntimeCache);
            var progress = perco.CaptureProgress(); // fixme - this should be async

            // progres.DateTime is 'now' as local datetime
            // round to next 12hrs
            // so will be 12:00:00 or 00:00:00, server time
            // (everything we do is server-local time based)

            var captureDateTime = progress.DateTime.Date;
            captureDateTime = progress.DateTime.Hour < 12
                ? captureDateTime.AddHours(12)
                : captureDateTime.AddDays(1);
            progress.DateTime = captureDateTime;

            SprintProgress.Save(progress);

            Logger.Info<CaptureProgress>($"Capture Progress {captureDateTime}.");

            // repeat
            return true;
        }

        private static ILogger Logger => ApplicationContext.Current.ProfilingLogger.Logger;

        public override bool IsAsync => false;
    }
}
