using IdentityServer4.Storage.CosmosDB.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IdentityServer4.SqlDbToCosmosMigrator
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = this.Configuration["ConnectionStrings:IdentityDbConnection"];
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddIdentityServer()
                .AddOperationalStore(options => options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)))
                .AddConfigurationStore(options => options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)));

            var identityCosmos = this.Configuration.GetSection("IdentityCosmos");
            var cosmosConfig = new CosmosDbConfiguration()
            {
                EndPointUrl = identityCosmos["EndPointUrl"],
                PrimaryKey = identityCosmos["PrimaryKey"],
                DatabaseName = identityCosmos["DatabaseName"]
            };
            services.AddSingleton(typeof(CosmosDbConfiguration), cosmosConfig);
            services.AddTransient<ISqlToCosmosMigratorContract, SqlToCosmosMigrationProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseIdentityServer();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    using (var serviceScope = app.ApplicationServices.CreateScope())
                    {
                        var migratorService = (ISqlToCosmosMigratorContract)serviceScope.ServiceProvider.GetRequiredService(typeof(ISqlToCosmosMigratorContract));
                        await context.Response.WriteAsync(migratorService.Migrate() ? "Migrated Successfully" : "Failed to migrate");
                    }
                });
            });
        }
    }
}
