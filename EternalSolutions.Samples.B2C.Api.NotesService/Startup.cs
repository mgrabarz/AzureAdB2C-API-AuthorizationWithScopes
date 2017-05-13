using EternalSolutions.Samples.B2C.Api.NotesService.Notes;
using EternalSolutions.Samples.B2C.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EternalSolutions.Samples.B2C.Api.NotesService
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

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Add database context (InMemory)
            services.AddDbContext<NotesContext>(options =>
                options.UseInMemoryDatabase());

            //Configure authorization policies that use scopes and claims.
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ReadNotes", policy =>
                    policy.RequireScope(Constants.Scopes.NotesServiceReadNotesScope));

                options.AddPolicy("WriteNotes", policy =>
                    policy.RequireScopesAll(new[]
                    {
                        Constants.Scopes.NotesServiceReadNotesScope,
                        Constants.Scopes.NotesServiceWriteNotesScope
                    })
                    .RequireClaim("Name"));//We need this claim to record name of person who created note.

                options.AddPolicy("DeleteNotes", policy =>
                    policy.RequireScopesAll(new[]
                    {
                        Constants.Scopes.NotesServiceReadNotesScope,
                        Constants.Scopes.NotesServiceWriteNotesScope
                    })
                    .RequireClaim(Constants.IdentityProviderClaim, "twitter.com"));
            });

            // Removes the need for empty [Authorize] attribute
            services.AddMvc(options =>
            {
                var requireAuthenticatedUserPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser() 
                        .Build();
                options.Filters.Add(new AuthorizeFilter(requireAuthenticatedUserPolicy));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                Authority = string.Format("https://login.microsoftonline.com/tfp/{0}/{1}", Configuration["Authentication:AzureAdB2C:TenantName"], Configuration["Authentication:AzureAdB2C:SignInPolicyName"]),
                Audience = Configuration["Authentication:AzureAdB2C:ClientId"],
                MetadataAddress = string.Format(Configuration["Authentication:AzureAdB2C:MetadataEndpointUrlTemplate"], Configuration["Authentication:AzureAdB2C:TenantName"], Configuration["Authentication:AzureAdB2C:SignInPolicyName"])
            });

            app.UseMvc();
        }
    }
}
