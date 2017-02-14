using Auth0.AuthenticationApi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Analystick.Web.Controllers
{
    public class ApiTokenCache
    {
        private static AccessTokenResponse token;
        private static DateTime expirationTime;
        private static readonly object obj = new object();
        
        public async Task<string> GetToken()
        {
            if (token == null || TokenIsExpired())
            {
                var newToken = await GetNewToken();
                
                lock (obj)
                {
                    token = newToken;
                    expirationTime = DateTime.Now.Add(new TimeSpan(0, 0, token.ExpiresIn - 120));
                }
            }

            return token.AccessToken;
        }

        private bool TokenIsExpired()
        {
            return expirationTime < DateTime.Now;
        }

        private async Task<AccessTokenResponse> GetNewToken()
        {
            var authClient = new Auth0.AuthenticationApi.AuthenticationApiClient(
                ConfigurationManager.AppSettings["auth0:Domain"]);

            return await authClient.GetTokenAsync(new ClientCredentialsTokenRequest
            {
                ClientId = ConfigurationManager.AppSettings["auth0:ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["auth0:ClientSecret"],
                Audience = $"https://{ConfigurationManager.AppSettings["auth0:Domain"]}/api/v2/"
            });
        }
    }
}