using System;
using Microsoft.Extensions.Configuration;

namespace ClientCredentialsTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.secret.json", true);

            var configuration = builder.Build();

            string authority = configuration["Secrets:Authority"];
            string clientId = configuration["Secrets:ClientId"];
            string clientSecret = configuration["Secrets:ClientSecret"];
            string resource = configuration["Secrets:Resource"];

            var token = TokenService.GetAccessToken(authority, resource, clientId, clientSecret).Result.AccessToken;
            Console.WriteLine(token);
        }
    }
}
