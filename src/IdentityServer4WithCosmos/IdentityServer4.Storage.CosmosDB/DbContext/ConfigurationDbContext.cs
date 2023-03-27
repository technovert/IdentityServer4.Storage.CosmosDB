using System;
using System.Linq;
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
    ///     Configuration DbContext Class.
    /// </summary>
    public class ConfigurationDbContext : CosmosDbContextBase, IConfigurationDbContext
    {
        private Container _apiResources;
        private Container _clients;
        private Container _identityResources;

        /// <summary>
        ///     Create an instance of the ConfigurationDbContext Class.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="clientOptions"></param>
        /// <param name="logger"></param>
        public ConfigurationDbContext(IOptions<CosmosDbConfiguration> settings,
            CosmosClientOptions clientOptions = null,
            ILogger<ConfigurationDbContext> logger = null)
            : base(settings, clientOptions, logger)
        {
            Guard.ForNullOrDefault(settings.Value, nameof(settings));
            EnsureClientsCollectionCreated().Wait();
            EnsureIdentityResourcesCollectionCreated().Wait();
            EnsureApiResourcesCollectionCreated().Wait();
        }

        /// <summary>
        ///     Add a new Client.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task AddClient(Client entity)
        {
            entity.Id = Guid.NewGuid().ToString();
            await _clients.CreateItemAsync(entity);
        }

        /// <summary>
        ///     Add a new Identity Resource.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task AddIdentityResource(IdentityResource entity)
        {
            entity.Id = Guid.NewGuid().ToString();
            await _identityResources.CreateItemAsync(entity);
        }

        /// <summary>
        ///     Add a new API Resource.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task AddApiResource(ApiResource entity)
        {
            entity.Id = Guid.NewGuid().ToString();
            await _apiResources.CreateItemAsync(entity);
        }

        /// <summary>
        ///     Queryable Identity Resources.
        /// </summary>
        public IQueryable<IdentityResource> IdentityResources(string partitionKey = "")
        {
            return string.IsNullOrWhiteSpace(partitionKey)
              ? _identityResources.GetItemLinqQueryable<IdentityResource>(allowSynchronousQueryExecution: true)
              : _identityResources.GetItemLinqQueryable<IdentityResource>(allowSynchronousQueryExecution: true, requestOptions: new QueryRequestOptions() { PartitionKey = new PartitionKey(partitionKey) });
        }

        /// <summary>
        ///     Queryable API Resources.
        /// </summary>
        public IQueryable<ApiResource> ApiResources(string partitionKey = "")
        {
            return string.IsNullOrWhiteSpace(partitionKey)
               ? _apiResources.GetItemLinqQueryable<ApiResource>(allowSynchronousQueryExecution: true)
               : _apiResources.GetItemLinqQueryable<ApiResource>(allowSynchronousQueryExecution: true, requestOptions: new QueryRequestOptions() { PartitionKey = new PartitionKey(partitionKey) });
        }

        /// <summary>
        ///     Queryable Clients.
        /// </summary>
        public IQueryable<Client> Clients(string partitionKey = "")
        {
            return string.IsNullOrWhiteSpace(partitionKey)
                ? _clients.GetItemLinqQueryable<Client>(allowSynchronousQueryExecution: true)
                : _clients.GetItemLinqQueryable<Client>(allowSynchronousQueryExecution: true, requestOptions: new QueryRequestOptions() { PartitionKey = new PartitionKey(partitionKey) });
        }

        private async Task EnsureClientsCollectionCreated()
        {
            var indexingPolicy = new IndexingPolicy
            {
                Automatic = true,
                IndexingMode = IndexingMode.Consistent
            };
            Logger?.LogDebug($"Clients Indexing Policy: {indexingPolicy}");

            var containerProperties = new ContainerProperties()
            {
                Id = Constants.CollectionNames.Client,
                PartitionKeyPath = Constants.CollectionPartitionKeyPaths.Client,
                IndexingPolicy = indexingPolicy
            };
            Logger?.LogDebug($"Clients Collection: {containerProperties}");

            Logger?.LogDebug($"Ensure Clients (ID:{containerProperties.Id}) collection exists...");
            var clientResults = await this.Database.CreateContainerIfNotExistsAsync(containerProperties, GetRUsFor(CollectionName.Clients));
            Logger?.LogDebug($"{clientResults.Container.Id} Creation Results: {clientResults.StatusCode}");
            if (clientResults.StatusCode.EqualsOne(HttpStatusCode.Created, HttpStatusCode.OK))
                _clients = clientResults.Container;
        }

        private async Task EnsureIdentityResourcesCollectionCreated()
        {
            var indexingPolicy = new IndexingPolicy
            {
                Automatic = true,
                IndexingMode = IndexingMode.Consistent
            };
            Logger?.LogDebug($"Identity Resources Indexing Policy: {indexingPolicy}");

            var containerProperties = new ContainerProperties()
            {
                Id = Constants.CollectionNames.IdentityResource,
                PartitionKeyPath = Constants.CollectionPartitionKeyPaths.IdentityResource,
                IndexingPolicy = indexingPolicy
            };
            Logger?.LogDebug($"Identity Resources Collection: {containerProperties}");

            Logger?.LogDebug($"Ensure Identity Resources (ID:{containerProperties.Id}) collection exists...");
            var clientResults = await this.Database.CreateContainerIfNotExistsAsync(containerProperties, GetRUsFor(CollectionName.IdentityResources));
            Logger?.LogDebug($"{clientResults.Container.Id} Creation Results: {clientResults.StatusCode}");
            if (clientResults.StatusCode.EqualsOne(HttpStatusCode.Created, HttpStatusCode.OK))
                _identityResources = clientResults.Container;
        }

        private async Task EnsureApiResourcesCollectionCreated()
        {
            var indexingPolicy = new IndexingPolicy
            {
                Automatic = true,
                IndexingMode = IndexingMode.Consistent
            };
            Logger?.LogDebug($"API Resources Index Policy: {indexingPolicy}");

            var containerProperties = new ContainerProperties()
            {
                Id = Constants.CollectionNames.ApiResource,
                PartitionKeyPath = Constants.CollectionPartitionKeyPaths.ApiResource,
                IndexingPolicy = indexingPolicy
            };
            Logger?.LogDebug($"API Resources Collection: {containerProperties}");

            Logger?.LogDebug($"Ensure API Resources (ID:{containerProperties.Id}) collection exists...");
            var apiResourceResults = await this.Database.CreateContainerIfNotExistsAsync(containerProperties, GetRUsFor(CollectionName.ApiResources));
            Logger?.LogDebug($"{apiResourceResults.Container.Id} Creation Results: {apiResourceResults.StatusCode}");
            if (apiResourceResults.StatusCode.EqualsOne(HttpStatusCode.Created, HttpStatusCode.OK))
                _apiResources = apiResourceResults.Container;
        }
    }
}