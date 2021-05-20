using IdentityServer4.Storage.CosmosDB.Configuration;
using IdentityServer4.Storage.CosmosDB.DbContext;
using IdentityServer4.Storage.CosmosDB.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EF = IdentityServer4.Storage.CosmosDB.Interfaces;

namespace IdentityServer4.SqlDbToCosmosMigrator
{
    public class SqlToCosmosMigrationProvider : ISqlToCosmosMigratorContract
    {
        public IConfigurationDbContext SQLContext { get; set; }
        public EF.IConfigurationDbContext CosmosContext { get; set; }
        public SqlToCosmosMigrationProvider(IConfigurationDbContext sqlContext, CosmosDbConfiguration cosmosConfig, ILogger<ConfigurationDbContext> logger)
        {
            this.SQLContext = sqlContext;
            this.CosmosContext = new ConfigurationDbContext(Options.Create(cosmosConfig), logger: logger);
        }
        public bool Migrate()
        {
            try
            {
                if (this.SQLContext != null)
                {
                    var clients = this.SQLContext.Clients
                        .Include(c => c.ClientSecrets)
                        .Include(c => c.Claims)
                        .Include(c => c.RedirectUris)
                        .Include(c => c.AllowedScopes)
                        .Include(c => c.Properties)
                        .Include(c => c.AllowedGrantTypes)
                        .Include(c => c.IdentityProviderRestrictions)
                        .Include(c => c.AllowedCorsOrigins)
                        .Include(c => c.PostLogoutRedirectUris);
                    clients.Select(_ => _.ToEntity()).ToList().ForEach(_ => this.CosmosContext.AddClient(_));

                    var apiResources = this.SQLContext.ApiResources
                        .Include(a => a.Scopes)
                        .Include("Scopes.UserClaims")
                        .Include(a => a.Properties)
                        .Include(a => a.Secrets)
                        .Include(a => a.UserClaims);
                    apiResources.Select(_ => _.ToEntity()).ToList().ForEach(_ => this.CosmosContext.AddApiResource(_));

                    var identityResources = this.SQLContext.IdentityResources
                        .Include(i => i.Properties)
                        .Include(i => i.UserClaims);
                    identityResources.Select(_ => _.ToEntity()).ToList().ForEach(_ => this.CosmosContext.AddIdentityResource(_));
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
