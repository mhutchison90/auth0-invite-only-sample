using System.Configuration;
using System.Web.Http;

namespace Analystick.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MessageHandlers.Add(new JsonWebTokenValidationHandler
            {
                Audience = ConfigurationManager.AppSettings["auth0:ClientId"],
                SymmetricKey = ConfigurationManager.AppSettings["auth0:ClientSecret"]
            });

            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new {id = RouteParameter.Optional}
                );
        }
    }
}