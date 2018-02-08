using System.Threading.Tasks;
using ImageGallery.API.Entities;
using ImageGallery.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ImageGallery.API
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["connectionStrings:imageGalleryDBConnectionString"];
            services.AddDbContext<GalleryContext>(o => o.UseSqlServer(connectionString));

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddIdentityServerAuthentication(options =>
            //    {
            //        options.Authority = "https://localhost:44308/";
            //        options.RequireHttpsMetadata = true;

            //        options.ApiName = "imagegalleryapi";
            //    });

            TokenValidationParameters tvps = new TokenValidationParameters
            {
                ValidAudience = "203a78c7-184f-4bbd-b38e-fa7938a6770d",
                ValidateAudience = true,
                ValidIssuer = "https://login.microsoftonline.com/DenisBiondichotmail.onmicrosoft.com",
                ValidateIssuer = true
            };

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://login.microsoftonline.com/DenisBiondichotmail.onmicrosoft.com";
                    options.Audience = "203a78c7-184f-4bbd-b38e-fa7938a6770d";
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = AuthenticationFailed,
                        OnTokenValidated = OnTokenValidated,
                        OnChallenge = OnChallenge,
                        OnMessageReceived = OnMessageRecieved
                    };
                    options.TokenValidationParameters = tvps;
                });

            // register the repository
            services.AddScoped<IGalleryRepository, GalleryRepository>();

            services.AddMvc();
        }

        private Task AuthenticationFailed(AuthenticationFailedContext arg)
        {
            // For debugging purposes only!
            var s = $"AuthenticationFailed: {arg.Exception.Message}";
            return Task.FromResult(0);
        }

        private Task OnTokenValidated(TokenValidatedContext arg)
        {
            // For debugging purposes only!
            var s = $"TokenValid: {arg.ToString()}";
            return Task.FromResult(0);
        }

        private Task OnChallenge(JwtBearerChallengeContext arg)
        {
            // For debugging purposes only!
            var s = $"Challenge: {arg.ToString()}";
            return Task.FromResult(0);
        }

        private Task OnMessageRecieved(MessageReceivedContext arg)
        {
            // For debugging purposes only!
            var s = $"Message: {arg.ToString()}";
            return Task.FromResult(0);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
            ILoggerFactory loggerFactory, GalleryContext galleryContext)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        // ensure generic 500 status code on fault.
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });
            }

            app.UseStaticFiles();

            AutoMapper.Mapper.Initialize(cfg =>
            {
                // Map from Image (entity) to Image, and back
                cfg.CreateMap<Image, Model.Image>().ReverseMap();

                // Map from ImageForCreation to Image
                // Ignore properties that shouldn't be mapped
                cfg.CreateMap<Model.ImageForCreation, Image>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());

                // Map from ImageForUpdate to Image
                // ignore properties that shouldn't be mapped
                cfg.CreateMap<Model.ImageForUpdate, Image>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());
            });

            AutoMapper.Mapper.AssertConfigurationIsValid();

			// ensure DB migrations are applied
            galleryContext.Database.Migrate();

            // seed the DB with data
            galleryContext.EnsureSeedDataForContext();

            app.UseAuthentication();
			
            app.UseMvc(); 
        }
    }
}
