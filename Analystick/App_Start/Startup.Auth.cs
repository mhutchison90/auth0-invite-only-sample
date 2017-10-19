using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Auth0.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System.Web.Helpers;

namespace Analystick.Web
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            //AntiForgeryConfig.UniqueClaimTypeIdentifier = "sub";

            // Set Cookies as default authentication type
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                LoginPath = new PathString("/Account/Login")
                // LoginPath property informs the middleware that it should change an outgoing 401 Unauthorized status code into a 302 redirection onto the given login path
                // More info: http://msdn.microsoft.com/en-us/library/microsoft.owin.security.cookies.cookieauthenticationoptions.loginpath(v=vs.111).aspx
            });

            // Use Auth0
            var provider = new Auth0AuthenticationProvider
            {
                OnReturnEndpoint = context =>
                {
                    // xsrf validation
                    if (context.Request.Query["state"] != null && context.Request.Query["state"].Contains("xsrf="))
                    {
                        NameValueCollection state = HttpUtility.ParseQueryString(context.Request.Query["state"]);
                        if (state["xsrf"] != "your_xsrf_random_string")
                        {
                            throw new HttpException(400, "invalid xsrf");
                        }
                    }

                    return Task.FromResult(0);
                },
                OnAuthenticated = context =>
                {
                    if (context.User["activation_pending"] != null)
                    {
                        var pending = context.User.Value<bool>("activation_pending");
                        if (!pending)
                        {
                            context.Identity.AddClaim(new Claim(ClaimTypes.Role, "Member"));
                        }
                    }

                    // context.User is a JObject with the original user object from Auth0
                    if (context.User["admin"] != null)
                    {
                        context.Identity.AddClaim(new Claim("admin", context.User.Value<string>("admin")));
                    }

                    context.Identity.AddClaim(
                        new Claim(
                            "friendly_name",
                            string.Format("{0}, {1}", context.User["family_name"], context.User["given_name"])));
                    const string identityProviderClaim = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";
                    if (!context.Identity.HasClaim(c => c.Type == identityProviderClaim))
                    {
                        // This claim is required for the ASP.NET Anti-Forgery Token to function.
                        // See http://msdn.microsoft.com/en-us/library/system.web.helpers.antiforgeryconfig.uniqueclaimtypeidentifier(v=vs.111).aspx
                        context.Identity.AddClaim(new Claim(identityProviderClaim, "Auth0"));
                    }
                    // NOTE: uncomment this if you send an array of roles (i.e.: ['sales','marketing','hr'])
                    //context.User["roles"].ToList().ForEach(r =>
                    //{
                    //    context.Identity.AddClaim(new Claim(ClaimTypes.Role, r.ToString()));
                    //});

                    return Task.FromResult(0);
                }
            };

            var options = new Auth0AuthenticationOptions()
            {
                Domain = ConfigurationManager.AppSettings["auth0:Domain"],
                ClientId = ConfigurationManager.AppSettings["auth0:ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["auth0:ClientSecret"],
                Provider = provider
            };
            app.UseAuth0Authentication(options);
        }
    }
}