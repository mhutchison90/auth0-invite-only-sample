using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using Analystick.Web.Areas.Admin.Models;
using Auth0.ManagementApi;
using JWT;
using System.Threading.Tasks;
using Auth0.AuthenticationApi.Models;
using Analystick.Web.Controllers;

namespace Analystick.Web.Areas.Admin.Controllers
{
    public class UsersController : Controller
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

        public async Task<ActionResult> Index()
        {
            var client = await GetApiClient();
            return View((await client.Users.GetAllAsync(connection: ConfigurationManager.AppSettings["auth0:Connection"]))
                .Select(u => new UserModel { UserId = u.UserId, GivenName = u.FirstName, FamilyName = u.LastName, Email = u.Email }).ToList());
        }

        public ActionResult New()
        {
            return View(Enumerable.Range(0, 5).Select(i => new UserModel()).ToList());
        }

        [HttpPost]
        public async Task<ActionResult> New(IEnumerable<UserModel> users)
        {
            if (users != null)
            {
                foreach (var user in users.Where(u => !String.IsNullOrEmpty(u.Email)))
                {
                    var randomPassword = Guid.NewGuid().ToString();
                    var metadata = new
                    {
                        activation_pending = true
                    };

                    var client = await GetApiClient();
                    var profile = await client.Users.CreateAsync(new Auth0.ManagementApi.Models.UserCreateRequest
                    {
                        Email = user.Email,
                        Password = randomPassword,
                        Connection = ConfigurationManager.AppSettings["auth0:Connection"],
                        EmailVerified = true,
                        FirstName = user.GivenName,
                        LastName = user.FamilyName,
                        AppMetadata = metadata
                    });

                    var userToken = JWT.JsonWebToken.Encode(
                      new { id = profile.UserId, email = profile.Email },
                        ConfigurationManager.AppSettings["analystick:signingKey"],
                          JwtHashAlgorithm.HS256);

                    var verificationUrlTicket = await client.Tickets.CreateEmailVerificationTicketAsync(new Auth0.ManagementApi.Models.EmailVerificationTicketRequest
                    {
                        UserId = profile.UserId,

                        ResultUrl = Url.Action("Activate", "Account", new { area = "", userToken }, Request.Url.Scheme)
                    });

                    var body = "Hello {0}, " +
                      "Great that you're using our application. " +
                      "Please click <a href='{1}'>ACTIVATE</a> to activate your account." +
                      "The Analystick team!";

                    var fullName = String.Format("{0} {1}", user.GivenName, user.FamilyName).Trim();
                    var mail = new MailMessage("app@auth0.com", user.Email, "Hello there!",
                        String.Format(body, fullName, verificationUrlTicket.Value));
                    mail.IsBodyHtml = true;

                    var mailClient = new SmtpClient();
                    mailClient.Send(mail);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                var client = await GetApiClient();
                await client.Users.DeleteAsync(id);
            }
            return RedirectToAction("Index");
        }

        private async Task<string> GetManagementApiToken()
        {
            var authClient = new Auth0.AuthenticationApi.AuthenticationApiClient(
                ConfigurationManager.AppSettings["auth0:Domain"]);

            var token = await authClient.GetTokenAsync(new ClientCredentialsTokenRequest
            {
                ClientId = ConfigurationManager.AppSettings["auth0:ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["auth0:ClientSecret"],
                Audience = $"https://{ConfigurationManager.AppSettings["auth0:Domain"]}/api/v2/"
            });
            return token.AccessToken;
        }
    }
}