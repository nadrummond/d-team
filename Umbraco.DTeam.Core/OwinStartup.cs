using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using Hangfire;
using Hangfire.SqlServer;
using Owin;
using Umbraco.Web;

namespace Umbraco.DTeam.Core
{
    // comes from Our

    //public class OwinStartup : UmbracoDefaultOwinStartup
    //{
    //    public override void Configuration(IAppBuilder app)
    //    {
    //        // default
    //        base.Configuration(app);

    //        //var clientId = WebConfigurationManager.AppSettings["GoogleOAuthClientID"];
    //        //var secret = WebConfigurationManager.AppSettings["GoogleOAuthSecret"];
    //        //app.ConfigureBackOfficeGoogleAuth(clientId, secret);

    //        //if (string.Equals(ConfigurationManager.AppSettings["HangFireEnabled"], "true", StringComparison.InvariantCultureIgnoreCase) == false)
    //        //    return;

    //        // configure hangfire
    //        var options = new SqlServerStorageOptions { PrepareSchemaIfNecessary = true };
    //        var connectionString = Umbraco.Core.ApplicationContext.Current.DatabaseContext.ConnectionString;
    //        GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString, options);

    //        var dashboardOptions = new DashboardOptions { Authorization = new[] { new UmbracoAuthorizationFilter() } };
    //        app.UseHangfireDashboard("/hangfire", dashboardOptions);
    //        app.UseHangfireServer();

    //        // schedule jobs
    //        var scheduler = new ScheduleHangfireJobs();
    //        scheduler.MarkAsSolvedReminder();
    //    }
    //}
}
