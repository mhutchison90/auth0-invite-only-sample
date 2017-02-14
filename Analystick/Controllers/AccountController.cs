using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Analystick.Web.Models;
using Auth0.ManagementApi;
using System.Threading.Tasks;
using Auth0.Core;
using Auth0.ManagementApi.Models;

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

        public ActionResult Login()
        {
            return View();
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
    }
}