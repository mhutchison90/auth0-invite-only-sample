using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Analystick.Web.Areas.Admin.Models;
using Auth0;

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
                .Select(u => new UserModel {UserId = u.UserId, GivenName = u.GivenName, FamilyName = u.FamilyName, Email = u.Email}).ToList());
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
                foreach (UserModel user in users.Where(u => !String.IsNullOrEmpty(u.Email)))
                {
                    _client.CreateUser(user.Email, Guid.NewGuid().ToString(), ConfigurationManager.AppSettings["auth0:Connection"], false, new
                    {
                        user.GivenName, user.FamilyName
                    });
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