using System;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Web.Scheduling;

namespace Umbraco.DTeam.Core.Scheduling
{
    // this is how we should do it, however we'd have to wait for 7.6
    // as the task scheduling plumbing (IBackgroundTask etc) is not public
    // in 7.5 - what a shame
    //
    // also: need to create the runner & add the task, in component
    // also: need to complement the task so it's a recurring task

    /*
    public class CaptureProgress : IBackgroundTask
    {
        public Task RunAsync(CancellationToken token)
        {
            var perco = new Percolator();
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

            return Task.CompletedTask;
        }

        public bool IsAsync => true;

        public void Run()
        {
            throw new InvalidOperationException("This task is async.");
        }
    }
    */
}
