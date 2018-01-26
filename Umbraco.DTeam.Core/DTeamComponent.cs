using Umbraco.Core;
using Umbraco.DTeam.Core.Auth;
using Umbraco.DTeam.Core.Scheduling;
using Umbraco.DTeam.Core.Storage;
using Umbraco.Web;
using Umbraco.Web.Scheduling;

namespace Umbraco.DTeam.Core
{
    public class DTeamComponent : ApplicationEventHandler
    {
        private BackgroundTaskRunner<IBackgroundTask> _runner;

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // run our own migrations
            var migrations = new Migrations(applicationContext);
            migrations.Run();

            UmbracoDefaultOwinStartup.MiddlewareConfigured += (sender, args) =>
            {
                args.AppBuilder.ConfigureBearerTokenAuthentication();
            };

            // somehow if we restrict ourselves to BackgroundTaskRunner<CaptureProgress> the RecurringTaskBase
            // constructor is not happy - probably some generic variance issue in Core - ok with IBackgroundTask
            _runner = new BackgroundTaskRunner<IBackgroundTask>("CaptureProgress", applicationContext.ProfilingLogger.Logger);
            _runner.TryAdd(new CaptureProgress(_runner));
        }
    }
}
