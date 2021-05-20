using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Storage.CosmosDB.Extensions;
using IdentityServer4.Storage.CosmosDB.Interfaces;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Storage.CosmosDB.Stores
{
    /// <summary>
    /// Class PersistedGrantStore
    /// </summary>
    public class PersistedGrantStore : IPersistedGrantStore
    {
        private readonly IPersistedGrantDbContext _context;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for PersistedGrantStore
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        public PersistedGrantStore(IPersistedGrantDbContext context, ILogger<PersistedGrantStore> logger)
        {
            Guard.ForNull(context, nameof(context));
            Guard.ForNull(logger, nameof(logger));

            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Store Async
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task StoreAsync(PersistedGrant token)
        {
            try
            {
                var existing = _context.PersistedGrants().ToList().SingleOrDefault(x => x.Key == token.Key);
                if (existing == null)
                {
                    _logger.LogDebug("{persistedGrantKey} not found in database", token.Key);

                    var persistedGrant = token.ToEntity();
                    _context.Add(persistedGrant);
                }
                else
                {
                    _logger.LogDebug("{persistedGrantKey} found in database", token.Key);

                    token.UpdateEntity(existing);
                    _context.Update(x => x.Key == token.Key, existing);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "Exception storing persisted grant");
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Get Async
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<PersistedGrant> GetAsync(string key)
        {
            var persistedGrant = _context.PersistedGrants().ToList().FirstOrDefault(x => x.Key == key);
            var model = persistedGrant.ToModel();

            _logger.LogDebug($"{key} found in database: {model != null}");

            return Task.FromResult(model);
        }

        /// <summary>
        /// Get All Async
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            var persistedGrants = _context.PersistedGrants().ToList().Where(x => x.SubjectId == subjectId).ToList();
            var model = persistedGrants.Select(x => x.ToModel());

            _logger.LogDebug($"{persistedGrants.Count} persisted grants found for {subjectId}");

            return Task.FromResult(model);
        }

        /// <summary>
        /// Remover Async
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task RemoveAsync(string key)
        {
            _logger.LogDebug($"removing {key} persisted grant from database");

            _context.Remove(x => x.Key == key);

            return Task.FromResult(0);
        }

        /// <summary>
        /// Remove All Async
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public Task RemoveAllAsync(string subjectId, string clientId)
        {
            _logger.LogDebug($"removing persisted grants from database for subject {subjectId}, clientId {clientId}");

            _context.Remove(x => x.SubjectId == subjectId && x.ClientId == clientId);

            return Task.FromResult(0);
        }

        /// <summary>
        /// Remove All Async
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="clientId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            _logger.LogDebug(
                $"removing persisted grants from database for subject {subjectId}, clientId {clientId}, grantType {type}");

            _context.Remove(
                x =>
                    x.SubjectId == subjectId &&
                    x.ClientId == clientId &&
                    x.Type == type);

            return Task.FromResult(0);
        }
    }
}