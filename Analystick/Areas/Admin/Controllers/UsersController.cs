using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using Analystick.Web.Areas.Admin.Models;
using Auth0;
using JWT;

namespace Analystick.Web.Areas.Admin.Controllers
{
    public class UsersController : Controller
    {
        private readonly Client _client;

        public UsersController()
        {
            _client = new Client(
                ConfigurationManager.AppSettings["auth0:ClientId"],
                ConfigurationManager.AppSettings["auth0:ClientSecret"],
                ConfigurationManager.AppSettings["auth0:Domain"]);
        }

        public ActionResult Index()
        {
            return View(_client.GetUsersByConnection(ConfigurationManager.AppSettings["auth0:Connection"])
                .Select(u => new UserModel { UserId = u.UserId, GivenName = u.GivenName, FamilyName = u.FamilyName, Email = u.Email }).ToList());
        }

        public ActionResult New()
        {
            return View(Enumerable.Range(0, 5).Select(i => new UserModel()).ToList());
        }

        [HttpPost]
        public ActionResult New(IEnumerable<UserModel> users)
        {
            if (users != null)
            {
                foreach (var user in users.Where(u => !String.IsNullOrEmpty(u.Email)))
                {
                    var randomPassword = Guid.NewGuid().ToString();
                    var metadata = new
                    {
                        user.GivenName,
                        user.FamilyName,
                        activation_pending = true
                    };

                    var profile = _client.CreateUser(user.Email, randomPassword,
                      ConfigurationManager.AppSettings["auth0:Connection"], false, metadata);

                    var userToken = JWT.JsonWebToken.Encode(
                      new { id = profile.UserId, email = profile.Email },
                        ConfigurationManager.AppSettings["analystick:signingKey"],
                          JwtHashAlgorithm.HS256);

                    var verificationUrl = _client.GenerateVerificationTicket(profile.UserId,
                        Url.Action("Activate", "Account", new { area = "", userToken }, Request.Url.Scheme));

                    var body = "Hello {0}, " +
                      "Great that you're using our application. " +
                      "Please click <a href='{1}'>ACTIVATE</a> to activate your account." +
                      "The Analystick team!";

                    var fullName = String.Format("{0} {1}", user.GivenName, user.FamilyName).Trim();
                    var mail = new MailMessage("app@auth0.com", user.Email, "Hello there!",
                        String.Format(body, fullName, verificationUrl));
                    mail.IsBodyHtml = true;

                    var mailClient = new SmtpClient();
                    mailClient.Send(mail);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!String.IsNullOrEmpty(id))
                _client.DeleteUser(id);
            return RedirectToAction("Index");
        }
    }
}