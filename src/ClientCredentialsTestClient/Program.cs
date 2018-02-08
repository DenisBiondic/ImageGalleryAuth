using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace ClientCredentialsTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(ServicePrincipal.GetS2SAccessTokenForProdMSAAsync().Result.AccessToken);
        }
    }

    public static class ServicePrincipal
    {
        /// <summary>
        /// The variables below are standard Azure AD terms from our various samples
        /// We set these in the Azure Portal for this app for security and to make it easy to change (you can reuse this code in other apps this way)
        /// You can name each of these what you want as long as you keep all of this straight
        /// </summary>
        private static string authority = "...";
        private static string clientId = "...d";
        private static string clientSecret = "... ";
        private static string resource = "...";

        /// <summary>
        /// wrapper that passes the above variables
        /// </summary>
        /// <returns></returns>
        static public async Task<AuthenticationResult> GetS2SAccessTokenForProdMSAAsync()
        {
            return await GetS2SAccessToken(authority, resource, clientId, clientSecret);
        }

        static async Task<AuthenticationResult> GetS2SAccessToken(string authority, string resource, string clientId, string clientSecret)
        {
            var clientCredential = new ClientCredential(clientId, clientSecret);
            AuthenticationContext context = new AuthenticationContext(authority, false);

            AuthenticationResult authenticationResult = await context.AcquireTokenAsync(
                resource,  // the resource (app) we are going to access with the token
                clientCredential);  // the client credentials
            return authenticationResult;
        }
    }
}
