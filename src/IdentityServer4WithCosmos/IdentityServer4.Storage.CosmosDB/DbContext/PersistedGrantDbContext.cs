using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using IdentityServer4.Storage.CosmosDB.Abstracts;
using IdentityServer4.Storage.CosmosDB.Configuration;
using IdentityServer4.Storage.CosmosDB.Entities;
using IdentityServer4.Storage.CosmosDB.Extensions;
using IdentityServer4.Storage.CosmosDB.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityServer4.Storage.CosmosDB.DbContext
{
    /// <inheritdoc cref="CosmosDbContextBase" />
    /// <summary>
    ///     Persisted Grant DbContext Class.
    /// </summary>
    public class PersistedGrantDbContext : CosmosDbContextBase, IPersistedGrantDbContext
    {
        private Container _persistedGrants;

        /// <summary>
        ///     Create an instance of the PersistedGrantDbContext Class.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="clientOptions"></param>
        /// <param name="logger"></param>
        public PersistedGrantDbContext(IOptions<CosmosDbConfiguration> settings,
            CosmosClientOptions clientOptions = null,
            ILogger<ConfigurationDbContext> logger = null)
            : base(settings, clientOptions, logger)
        {
            Guard.ForNullOrDefault(settings.Value, nameof(settings));
            SetupPersistedGrants().Wait();
        }


        /// <summary>
        ///     Add new Persisted Grant.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Add(PersistedGrant entity)
        {
            entity.Id = Guid.NewGuid().ToString();
            await _persistedGrants.CreateItemAsync(entity);
        }

        /// <summary>
        ///     Remove multiple Persisted Grants.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task Remove(Expression<Func<PersistedGrant, bool>> filter)
        {
            var persistedGrants = PersistedGrants().ToList();
            foreach (var persistedGrant in persistedGrants.AsQueryable().Where(filter)) await Remove(persistedGrant);
        }

        /// <summary>
        ///     Removed expired Persisted Grants.
        /// </summary>
        /// <returns></returns>
        public async Task RemoveExpired()
        {
            foreach (var expired in PersistedGrants().ToList().Where(x => x.Expiration < DateTime.UtcNow)) await Remove(expired);
        }

        /// <summary>
        ///     Updated a Persisted Grant.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Update(PersistedGrant entity)
        {
            await _persistedGrants.ReplaceItemAsync(entity, entity.Id);
        }

        /// <summary>
        ///     Update multiple Persisted Grants.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Update(Expression<Func<PersistedGrant, bool>> filter, PersistedGrant entity)
        {
            await _persistedGrants.UpsertItemAsync(entity);
        }

        /// <summary>
        ///     Remove a Persisted Grant.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Remove(PersistedGrant entity)
        {
            await _persistedGrants.DeleteItemAsync<PersistedGrant>(entity.Id, new PartitionKey(entity.ClientId));
        }

        /// <summary>
        ///     Queryable Persisted Grants.
        /// </summary>
        public IQueryable<PersistedGrant> PersistedGrants(string partitionKey = "")
        {
            return string.IsNullOrWhiteSpace(partitionKey)
              ? _persistedGrants.GetItemLinqQueryable<PersistedGrant>(allowSynchronousQueryExecution: true)
              : _persistedGrants.GetItemLinqQueryable<PersistedGrant>(allowSynchronousQueryExecution: true, requestOptions: new QueryRequestOptions() { PartitionKey = new PartitionKey(partitionKey) });
        }

        private async Task SetupPersistedGrants()
        {
            
            var indexingPolicy = new IndexingPolicy
            {
                Automatic = true,
                IndexingMode = IndexingMode.Consistent, 
                IncludedPaths =
                {
                    new IncludedPath
                    {
                        Path = "/expiration/?",
                    },
                    new IncludedPath
                    {
                        Path = "/",
                    }
                }
            };
            Logger?.LogDebug($"Persisted Grants Indexing Policy: {indexingPolicy}");

            var uniqueKeyPolicy = new UniqueKeyPolicy
            {
                UniqueKeys =
                {
                    new UniqueKey
                    {
                        Paths =
                        {
                            "/clientId",
                            "/subjectId",
                            "/type"
                        }
                    }
                }
            };
            Logger?.LogDebug($"Persisted Grants Unique Key Policy: {uniqueKeyPolicy}");

            var containerProperties = new ContainerProperties
            {
                Id = Constants.CollectionNames.PersistedGrant,
                PartitionKeyPath = Constants.CollectionPartitionKeyPaths.PersistedGrant,
                IndexingPolicy = indexingPolicy,
                UniqueKeyPolicy = uniqueKeyPolicy
            };
            Logger?.LogDebug($"Persisted Grants Collection: {containerProperties}");

            Logger?.LogDebug($"Ensure Persisted Grants (ID:{containerProperties.Id}) collection exists...");
            var persistedGrantsResults = await this.Database.CreateContainerIfNotExistsAsync(containerProperties, GetRUsFor(CollectionName.PersistedGrants));
            Logger?.LogDebug($"{persistedGrantsResults.Container.Id} Creation Results: {persistedGrantsResults.StatusCode}");
            if (persistedGrantsResults.StatusCode.EqualsOne(HttpStatusCode.Created, HttpStatusCode.OK))
                _persistedGrants = persistedGrantsResults.Container;
        }
    }
}