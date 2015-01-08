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

        public ActionResult Activate(string email)
        {
            var user = GetUserProfile(email);
            if (user != null)
                return View(new UserActivationModel {Email = email});
            return View("ActivationError", new UserActivationErrorModel("Error activating user, could not find an exact match for this email address."));
        }

        [HttpPost]
        public ActionResult Activate(UserActivationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = GetUserProfile(model.Email);
            if (user != null)
            {
                if (user.ExtraProperties.ContainsKey("account_activated") && (bool)user.ExtraProperties["account_activated"])
                    return View("ActivationError", new UserActivationErrorModel("Error activating user, the user is already active."));

                _client.ChangePassword(user.UserId, model.Password, true);
                _client.UpdateUserMetadata(user.UserId, new {account_activated = true});

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

        private UserProfile GetUserProfile(string email)
        {
            var users = _client.GetUsersByConnection(ConfigurationManager.AppSettings["auth0:Connection"], email).ToList();
            if (users.Count == 1)
            {
                var user = users.FirstOrDefault();
                var profile = _client.GetUser(user.UserId);
                return profile;
            }

            return null;
        }
    }
}