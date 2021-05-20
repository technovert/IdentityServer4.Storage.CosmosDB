using IdentityServer4.Models;
using IdentityServer4.Storage.CosmosDB.Extensions;
using IdentityServer4.Storage.CosmosDB.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleHost
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddLogging();
            services.AddIdentityServer()
            .AddOperationalStore(Configuration.GetSection("CosmosDB"))
            .AddConfigurationStore(Configuration.GetSection("CosmosDB"))
            .AddDeveloperSigningCredential()
            .AddJwtBearerClientAuthentication()
            .AddTestUsers(IdentityServerResources.GetTestUsers().ToList()); // Add other needed identity server configuration 

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = "https://localhost:44337/";
                options.Audience = "api1";
                options.RequireHttpsMetadata = true;
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("PublicSecure", policy => policy.RequireClaim("client_id", "oauthClient"));
                options.AddPolicy("ApiReader", policy => policy.RequireClaim("scope", "api1.read"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                EnsureSeedData(serviceScope.ServiceProvider.GetService<IConfigurationDbContext>());
            }

            app.UseIdentityServer();
            app.UseIdentityServerCosmosDbTokenCleanup(applicationLifetime);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Values}/{action=Index}/{id?}");
            });
        }

        private static void EnsureSeedData(IConfigurationDbContext context)
        {
            foreach (var client in IdentityServerResources.GetClients().ToList())
            {
                var dbRecords = context.Clients(client.ClientId).ToList();
                if (dbRecords.Count == 0) context.AddClient(client.ToEntity());
            }

            foreach (var resource in IdentityServerResources.GetIdentityResources().ToList())
            {
                var dbRecords = context.IdentityResources(resource.Name).ToList();
                if (dbRecords.Count == 0) context.AddIdentityResource(resource.ToEntity());
            }

            foreach (var resource in IdentityServerResources.GetApiResources().ToList())
            {
                var dbRecords = context.ApiResources(resource.Name).ToList();
                if (dbRecords.Count == 0) context.AddApiResource(resource.ToEntity());
            }
        }
    }
}
