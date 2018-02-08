using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace ClientCredentialsTestClient
{
    public static class TokenService
    {
        public static async Task<AuthenticationResult> GetAccessToken(string authority, string resource, string clientId, string clientSecret)
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