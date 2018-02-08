using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ImageGallery.Client.Services
{
    public class ImageGalleryHttpClient : IImageGalleryHttpClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HttpClient _httpClient = new HttpClient();
        private readonly IConfiguration _configuration;

        public ImageGalleryHttpClient(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }
        
        public async Task<HttpClient> GetClient()
        {
            string authority = _configuration["Secrets:Authority"];
            string clientId = _configuration["Secrets:ClientId"];
            string clientSecret = _configuration["Secrets:ClientSecret"];
            string resource = _configuration["Secrets:Resource"];

            var token = await TokenService.GetAccessToken(authority, resource, clientId, clientSecret);

            // another possibility
            // var token = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            
            _httpClient.BaseAddress = new Uri("https://localhost:44301/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            return _httpClient;
        }
    }
}

