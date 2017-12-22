using System;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Scheduling;

namespace Umbraco.DTeam.Core.Scheduling
{
    // also: need to complement the task so it's a recurring task

    public class CaptureProgress : IBackgroundTask
    {
        private readonly BackgroundTaskRunner<CaptureProgress> _runner;

        public CaptureProgress(BackgroundTaskRunner<CaptureProgress> runner)
        {
            _runner = runner;
        }

        public async Task RunAsync(CancellationToken token)
        {
            // first wait
            // then run...
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                return;
            }

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

            // then wait for 10 minutes and re-schedule
            // if the site goes down this will be cancelled
            // fixme - ugly but latched tasks are core-internal
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(10), token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                return;
            }
            _runner.TryAdd(new CaptureProgress(_runner)); // fixme then why can't we loop on 1 task?
        }

        private static ILogger Logger => ApplicationContext.Current.ProfilingLogger.Logger;

        public bool IsAsync => true;

        public void Run()
        {
            throw new InvalidOperationException("This task is async.");
        }

        public void Dispose()
        { }
    }
}
