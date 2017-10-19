using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace Analystick.Web
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ActivationRequiredAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            bool skipAuthorization = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true)
                                     || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true);

            if (skipAuthorization)
            {
                return;
            }

            if (filterContext.HttpContext.User.Identity.IsAuthenticated
                && !filterContext.HttpContext.User.IsInRole("Member"))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { Controller = "Account", Action = "ActivationRequired" }));
            }
        }
    }
}