using EternalSolutions.Samples.B2C.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace EternalSolutions.Samples.B2C.Client.NotesServiceClient
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public static IConfigurationRoot Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromSeconds(100);
                options.CookieHttpOnly = true;
            });

            // Add framework services.
            services.AddMvc();

            services.AddAuthentication(
                sharedOptions => sharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseSession();

            app.UseCookieAuthentication();

            var options = new OpenIdConnectOptions
            {
                Authority = string.Format("https://login.microsoftonline.com/tfp/{0}/{1}", Configuration["Authentication:AzureAdB2C:TenantName"], Configuration["Authentication:AzureAdB2C:SignInPolicyName"]),
                MetadataAddress = string.Format(Configuration["Authentication:AzureAdB2C:MetadataEndpointUrlTemplate"], Configuration["Authentication:AzureAdB2C:TenantName"], Configuration["Authentication:AzureAdB2C:SignInPolicyName"]),
                ClientId = Configuration["Authentication:AzureAdB2C:ClientId"],
                ClientSecret = Configuration["Authentication:AzureAdB2C:ClientSecret"],
                Events = new OpenIdConnectEvents
                {
                    OnAuthorizationCodeReceived = OnAuthorizationCodeReceived,
                    OnAuthenticationFailed = OnAuthenticationFailed
                },
                ResponseType = OpenIdConnectResponseType.CodeIdToken,
                SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme
            };
            options.Scope.Add($"{Constants.Scopes.NotesServiceAppIdUri}{Constants.Scopes.NotesServiceReadNotesScope}");
            options.Scope.Add($"{Constants.Scopes.NotesServiceAppIdUri}{Constants.Scopes.NotesServiceWriteNotesScope}");
            options.Scope.Add("offline_access");
            app.UseOpenIdConnectAuthentication(options);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private Task OnAuthenticationFailed(AuthenticationFailedContext authenticationFailedContext)
        {
            authenticationFailedContext.HandleResponse();
            return Task.CompletedTask;
        }

        private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext authorizationCodeReceivedContext)
        {
            var code = authorizationCodeReceivedContext.ProtocolMessage.Code;
            string signedInUserID = authorizationCodeReceivedContext.Ticket.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            TokenCache userTokenCache = new MSALSessionCache(signedInUserID, authorizationCodeReceivedContext.HttpContext).GetMsalCacheInstance();
            ConfidentialClientApplication cca = new ConfidentialClientApplication(
                Configuration["Authentication:AzureAdB2C:ClientId"],
                string.Format("https://login.microsoftonline.com/tfp/{0}/{1}", Configuration["Authentication:AzureAdB2C:TenantName"], Configuration["Authentication:AzureAdB2C:SignInPolicyName"]),
                Configuration["Authentication:AzureAdB2C:CallbackPath"], 
                new ClientCredential(Configuration["Authentication:AzureAdB2C:ClientSecret"]), 
                userTokenCache, userTokenCache);
            string[] scopes =
            {
               $"{Constants.Scopes.NotesServiceAppIdUri}{Constants.Scopes.NotesServiceReadNotesScope}",
               $"{Constants.Scopes.NotesServiceAppIdUri}{Constants.Scopes.NotesServiceWriteNotesScope}"
            };
            AuthenticationResult result = await cca.AcquireTokenByAuthorizationCodeAsync(code, scopes);
        }
    }
}
