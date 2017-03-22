using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Umbraco.DTeam.Core.Auth
{
    public class BearerTokenAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var principal = actionContext.ControllerContext.RequestContext.Principal;
            var identity = principal?.Identity;

            if (identity == null)
                return false;

            if (identity.IsAuthenticated == false || identity.AuthenticationType != "Umbraco.DTeamApi")
                return false;

            var claims = (ClaimsIdentity) identity;
            return claims.HasClaim("Umbraco.DTeamApi", "yes");
        }
    }
}
