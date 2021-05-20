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
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Index = Microsoft.Azure.Documents.Index;

namespace IdentityServer4.Storage.CosmosDB.DbContext
{
    /// <inheritdoc cref="CosmosDbContextBase" />
    /// <summary>
    ///     Persisted Grant DbContext Class.
    /// </summary>
    public class PersistedGrantDbContext : CosmosDbContextBase, IPersistedGrantDbContext
    {
        private DocumentCollection _persistedGrants;
        private Uri _persistedGrantsUri;

        /// <summary>
        ///     Create an instance of the PersistedGrantDbContext Class.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="connectionPolicy"></param>
        /// <param name="logger"></param>
        public PersistedGrantDbContext(IOptions<CosmosDbConfiguration> settings,
            ConnectionPolicy connectionPolicy = null,
            ILogger<PersistedGrantDbContext> logger = null)
            : base(settings, connectionPolicy, logger)
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
            await DocumentClient.CreateDocumentAsync(_persistedGrantsUri, entity);
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
            var documentUrl = UriFactory.CreateDocumentUri(Database.Id, _persistedGrants.Id, entity.ClientId);
            await DocumentClient.ReplaceDocumentAsync(documentUrl, entity);
        }

        /// <summary>
        ///     Update multiple Persisted Grants.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Update(Expression<Func<PersistedGrant, bool>> filter, PersistedGrant entity)
        {
            // TODO : This looks like its a MongoDb specific thing.  This is an attempt to match it.
            // await _persistedGrants.ReplaceOneAsync(filter, entity);
            await DocumentClient.UpsertDocumentAsync(_persistedGrantsUri, entity);
        }

        /// <summary>
        ///     Remove a Persisted Grant.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task Remove(PersistedGrant entity)
        {
            var documentUrl = UriFactory.CreateDocumentUri(Database.Id, _persistedGrants.Id, entity.Id);
            await DocumentClient.DeleteDocumentAsync(documentUrl, new RequestOptions() { PartitionKey = new PartitionKey(entity.ClientId)});
        }

        /// <summary>
        ///     Queryable Persisted Grants.
        /// </summary>
        public IQueryable<PersistedGrant> PersistedGrants(string partitionKey = "")
        {
            return string.IsNullOrWhiteSpace(partitionKey)
                ? DocumentClient.CreateDocumentQuery<PersistedGrant>(_persistedGrantsUri,
                    new FeedOptions {EnableCrossPartitionQuery = true})
                : DocumentClient.CreateDocumentQuery<PersistedGrant>(_persistedGrantsUri,
                    new FeedOptions {PartitionKey = new PartitionKey(partitionKey)});
        }

        private async Task SetupPersistedGrants()
        {
            _persistedGrantsUri =
                UriFactory.CreateDocumentCollectionUri(Database.Id, Constants.CollectionNames.PersistedGrant);
            Logger?.LogDebug($"Persisted Grants URI: {_persistedGrantsUri}");

            var partitionKeyDefinition = new PartitionKeyDefinition
                {Paths = {Constants.CollectionPartitionKeys.PersistedGrant}};
            Logger?.LogDebug($"Persisted Grants Partition Key: {partitionKeyDefinition}");

            var indexingPolicy = new IndexingPolicy
            {
                Automatic = true,
                IndexingMode = IndexingMode.Consistent, IncludedPaths =
                {
                    new IncludedPath
                    {
                        Path = "/expiration/?",
                        Indexes =
                        {
                            Index.Range(DataType.String)
                        }
                    },
                    new IncludedPath
                    {
                        Path = "/",
                        Indexes =
                        {
                            Index.Range(DataType.String)
                        }
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

            _persistedGrants = new DocumentCollection
            {
                Id = Constants.CollectionNames.PersistedGrant,
                PartitionKey = partitionKeyDefinition,
                IndexingPolicy = indexingPolicy,
                UniqueKeyPolicy = uniqueKeyPolicy
            };
            Logger?.LogDebug($"Persisted Grants Collection: {_persistedGrants}");

            var persistedGrantsRequestOptions = new RequestOptions
            {
                OfferThroughput = GetRUsFor(CollectionName.PersistedGrants)
            };
            Logger?.LogDebug($"Persisted Grants Request Options: {persistedGrantsRequestOptions}");

            Logger?.LogDebug($"Ensure Persisted Grants (ID:{_persistedGrants.Id}) collection exists...");
            var persistedGrantsResults =
                await DocumentClient.CreateDocumentCollectionIfNotExistsAsync(DatabaseUri, _persistedGrants,
                    persistedGrantsRequestOptions);
            Logger?.LogDebug($"{_persistedGrants.Id} Creation Results: {persistedGrantsResults.StatusCode}");
            if (persistedGrantsResults.StatusCode.EqualsOne(HttpStatusCode.Created, HttpStatusCode.OK))
                _persistedGrants = persistedGrantsResults.Resource;
        }
    }
}