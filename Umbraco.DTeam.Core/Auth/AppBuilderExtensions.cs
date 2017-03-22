using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Owin;

namespace Umbraco.DTeam.Core.Auth
{
    public static class AppBuilderExtensions
    {
        internal static void ConfigureBearerTokenAuthentication(this IAppBuilder appBuilder)
        {
            appBuilder.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                Provider = new OAuthBearerAuthenticationProvider
                {
                    //This is the first callback made when OWIN starts
                    //to process the request, here we need to set the token
                    //to null if we don't want it to be processed - basically
                    //this is where we need to:
                    // * check the current request URL to see if we should auth the request (only deploy end points)
                    // * TODO: check if the current request has valid IPs
                    OnRequestToken = context =>
                    {
                        var requestPath = context.Request.Uri.CleanPathAndQuery();
                        //Only d-team endpoints should be authenticated
                        if (requestPath.StartsWith("/Umbraco/BackOffice/DTeam/Api/",
                            StringComparison.InvariantCultureIgnoreCase))
                            return Task.FromResult(0);

                        context.Token = null;
                        return Task.FromResult(0);
                    }
                },
                AccessTokenProvider = new AuthenticationTokenProvider
                {
                    //Callback used to parse the token in the request,
                    //if the token parses correctly then we should assign a ticket
                    //to the request, this is the "User" that will get assigned to
                    //the request with Claims.
                    //If the token is invalid, then don't assign a ticket and OWIN
                    //will take care of the rest (not authenticated)
                    OnReceive = context =>
                    {
                        var requestPath = context.Request.Uri.CleanPathAndQuery();
                        //this throws an exception if anything if wrong, so the ticket won't be assigned
                        DateTime timestamp;
                        Token.ValidateToken(context.Token, requestPath, out timestamp);

                        //If ok, create a ticket here with the Claims we need
                        //to check for in AuthZ - these can be anything, perhaps
                        //could assign any values that are taken from the token (if any)
                        var ticket = new AuthenticationTicket(
                            new ClaimsIdentity(
                                new List<Claim>
                                {
                                    new Claim("Umbraco.DTeamApi", "yes")
                                },

                                //The authentication type = this is important, if not set
                                //then the ticket's IsAuthenticated property will be false
                                authenticationType: "Umbraco.DTeamApi"),
                            new AuthenticationProperties
                            {
                                //Expires after 5 minutes in case there are some long running operations
                                ExpiresUtc = timestamp.AddMinutes(5)
                            });

                        context.SetTicket(ticket);
                    }
                }
            });
        }
    }
}
