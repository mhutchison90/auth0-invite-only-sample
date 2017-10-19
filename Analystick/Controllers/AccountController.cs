using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Analystick.Web.Models;
using Auth0.ManagementApi;
using System.Threading.Tasks;
using Auth0.Core;
using Auth0.ManagementApi.Models;
using Auth0.AuthenticationApi;
using System;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Web;

namespace Analystick.Web.Controllers
{
    public class AccountController : Controller
    {
        private ManagementApiClient _client;

        private async Task<ManagementApiClient> GetApiClient()
        {
            if (_client == null)
            {
                var token = await (new ApiTokenCache()).GetToken();
                _client = new ManagementApiClient(
                    token,
                    ConfigurationManager.AppSettings["auth0:Domain"]);
            }

            return _client;
        }

        public ActionResult ActivationRequired()
        {
            return View();
        }

        public ActionResult Login(string returnUrl)
        {
            if (!Url.IsLocalUrl(returnUrl))
            {
                returnUrl = null;
            }

            return new ChallengeResult("Auth0", returnUrl ?? Url.Action("Index", "Home"));
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff(string returnUrl)
        {
            var appTypes = AuthenticationManager.GetAuthenticationTypes().Select(at => at.AuthenticationType).ToArray();
            AuthenticationManager.SignOut(appTypes);

            var absoluteReturnUrl = string.IsNullOrEmpty(returnUrl) ?
                this.Url.Action("Index", "Home", new { }, this.Request.Url.Scheme) :
                this.Url.IsLocalUrl(returnUrl) ?
                    new Uri(this.Request.Url, returnUrl).AbsoluteUri : returnUrl;

            // remove this line and uncomment the next redirect
            // if you want to clear Auth0's session as well
            return Redirect(absoluteReturnUrl);

            //return Redirect(
            //    string.Format("https://{0}/v2/logout?client_id={1}&returnTo={2}",
            //        ConfigurationManager.AppSettings["auth0:Domain"],
            //        ConfigurationManager.AppSettings["auth0:ClientId"],
            //        absoluteReturnUrl));
        }

        /// <summary>
        /// GET Account/Activate?userToken=xxx
        /// </summary>
        /// <param name="userToken"></param>
        /// <returns></returns>
        public async Task<ActionResult> Activate(string userToken)
        {
            dynamic metadata = JWT.JsonWebToken.DecodeToObject(userToken, 
                ConfigurationManager.AppSettings["analystick:signingKey"]);
            var user = await GetUserProfile(metadata["id"]);
            if (user != null)
                return View(new UserActivationModel { Email = user.Email, UserToken = userToken });
            return View("ActivationError", 
                new UserActivationErrorModel("Error activating user, could not find an exact match for this email address."));
        }

        /// <summary>
        /// POST Account/Activate
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Activate(UserActivationModel model)
        {
            dynamic metadata = JWT.JsonWebToken.DecodeToObject(model.UserToken, 
                ConfigurationManager.AppSettings["analystick:signingKey"], true);
            if (metadata == null)
            {
                return View("ActivationError", 
                    new UserActivationErrorModel("Unable to find the token."));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User user = await GetUserProfile(metadata["id"]);
            if (user != null)
            {
                if (user.AppMetadata["activation_pending"] != null && !((bool)user.AppMetadata["activation_pending"]))
                    return View("ActivationError", new UserActivationErrorModel("Error activating user, the user is already active."));

                var client = await GetApiClient();
                await client.Users.UpdateAsync(user.UserId, new UserUpdateRequest {
                    Password = model.Password
                });
                await client.Users.UpdateAsync(user.UserId, new UserUpdateRequest
                {
                    AppMetadata = new { activation_pending = false }
                });

                return View("Activated");
            }

            return View("ActivationError", 
                new UserActivationErrorModel("Error activating user, could not find an exact match for this email address."));
        }
        
        private async Task<User> GetUserProfile(string id)
        {
            var client = await GetApiClient();
            return await client.Users.GetAsync(id);
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            private const string XsrfKey = "XsrfId";

            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
    }
}