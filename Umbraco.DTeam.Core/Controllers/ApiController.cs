using System.Net;
using System.Net.Http;
using Umbraco.DTeam.Core.Auth;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using HttpPost = System.Web.Http.HttpPostAttribute;

namespace Umbraco.DTeam.Core.Controllers
{
    [PluginController(ApiArea)]
    [IsBackOffice]
    [BearerTokenAuthorize]
    public class ApiController : UmbracoApiController
    {
        public const string ApiArea = "DTeam";

        // POST /Umbraco/BackOffice/DTeam/Api/CaptureProgress
        //
        [HttpPost]
        public HttpResponseMessage CaptureProgress()
        {
            var perco = new Percolator(ApplicationContext.ApplicationCache.RuntimeCache);
            var progress = perco.CaptureProgress();

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

            return Request.CreateResponse(HttpStatusCode.OK, captureDateTime.ToUniversalTime());
        }
    }
}
