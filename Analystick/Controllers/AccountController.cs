using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Analystick.Web.Models;
using Auth0;

namespace Analystick.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly Client _client;

        public AccountController()
        {
            _client = new Client(
                ConfigurationManager.AppSettings["auth0:ClientId"], 
                ConfigurationManager.AppSettings["auth0:ClientSecret"], 
                ConfigurationManager.AppSettings["auth0:Domain"]);
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Activate(string token)
        {
            dynamic metadata = JWT.JsonWebToken.DecodeToObject(token, ConfigurationManager.AppSettings["analystick:signingKey"], true);
            var user = GetUserProfile(metadata["id"]);
            if (user != null)
                return View(new UserActivationModel {Email = user.Email, Token = token });
            return View("ActivationError", new UserActivationErrorModel("Error activating user, could not find an exact match for this email address."));
        }

        [HttpPost]
        public ActionResult Activate(UserActivationModel model)
        {
            dynamic metadata = JWT.JsonWebToken.DecodeToObject(model.Token, ConfigurationManager.AppSettings["analystick:signingKey"], true);
            if (metadata == null)
            {
                return View("ActivationError", new UserActivationErrorModel("Unable to find the token."));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            UserProfile user = GetUserProfile(metadata["id"]);
            if (user != null)
            {
                if (user.ExtraProperties.ContainsKey("activation_pending") && !((bool)user.ExtraProperties["activation_pending"]))
                    return View("ActivationError", new UserActivationErrorModel("Error activating user, the user is already active."));

                _client.ChangePassword(user.UserId, model.Password, true);
                _client.UpdateUserMetadata(user.UserId, new { activation_pending = false });

                return View("ConfirmPassword");
            }
            return View("ActivationError", new UserActivationErrorModel("Error activating user, could not find an exact match for this email address."));
        }

        public ActionResult Activated(bool success, string message)
        {
            if (!success)
                return View("ActivationError", new UserActivationErrorModel(message));
            return View();
        }

        private UserProfile GetUserProfile(string id)
        {
            return _client.GetUser(id);
        }
    }
}