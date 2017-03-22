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
            var perco = new Percolator();
            var progress = perco.CaptureProgress();

            var captureDateTime = progress.DateTime.Date;
            captureDateTime = progress.DateTime.Hour < 12 
                ? captureDateTime.AddHours(12) 
                : captureDateTime.AddDays(1);
            progress.DateTime = captureDateTime;

            SprintProgress.Save(progress);

            return Request.CreateResponse(HttpStatusCode.OK, captureDateTime);
        }
    }
}
