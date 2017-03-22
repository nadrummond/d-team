using Umbraco.Core;
using Umbraco.DTeam.Core.Auth;
using Umbraco.DTeam.Core.Storage;
using Umbraco.Web;

namespace Umbraco.DTeam.Core
{
    public class DTeamComponent : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // run our own migrations
            var migrations = new Migrations(applicationContext);
            migrations.Run();

            UmbracoDefaultOwinStartup.MiddlewareConfigured += (sender, args) =>
            {
                args.AppBuilder.ConfigureBearerTokenAuthentication();
            };
        }
    }
}
