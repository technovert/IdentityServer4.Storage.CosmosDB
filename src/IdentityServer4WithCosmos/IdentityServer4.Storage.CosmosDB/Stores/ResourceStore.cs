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
    /// Class ResourceStore
    /// </summary>
    public class ResourceStore : IResourceStore
    {
        private readonly IConfigurationDbContext _context;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for ResourceStore
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        public ResourceStore(IConfigurationDbContext context, ILogger<ResourceStore> logger)
        {
            Guard.ForNull(context, nameof(context));
            Guard.ForNull(logger, nameof(logger));

            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// FindIdentityResourcesByScopeAsync
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            var scopes = scopeNames.ToArray();

            var identityResources = _context.IdentityResources().ToList();
            var resources = from identityResource in identityResources
                where scopes.Contains(identityResource.Name)
                select identityResource;

            var results = resources.ToArray();

            _logger.LogDebug("Found {scopes} identity scopes in database", results.Select(x => x.Name));

            return Task.FromResult(results.Select(x => x.ToModel()).ToArray().AsEnumerable());
        }

        /// <summary>
        /// FindApiResourcesByScopeAsync
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            var names = scopeNames.ToArray();

            var apis = from api in _context.ApiResources().ToList()
                where api.Scopes.Any(x => names.Contains(x.Name))
                select api;

            var results = apis.ToArray();
            var models = results.Select(x => x.ToModel()).ToArray();

            _logger.LogDebug("Found {scopes} API scopes in database",
                models.SelectMany(x => x.Scopes).Select(x => x));

            return Task.FromResult(models.AsEnumerable());
        }

        /// <summary>
        /// FindApiResourceAsync
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Task<ApiResource> FindApiResourceAsync(string name)
        {
            var apis = from apiResource in _context.ApiResources().ToList()
                where apiResource.Name == name
                select apiResource;

            var api = apis.FirstOrDefault();

            if (api != null)
                _logger.LogDebug($"Found {name} API resource in database");
            else
                _logger.LogDebug($"Did not find {name} API resource in database");

            return Task.FromResult(api.ToModel());
        }

        /// <summary>
        /// GetAllResourcesAsync
        /// </summary>
        /// <returns></returns>
        public Task<Resources> GetAllResourcesAsync()
        {
            var identity = _context.IdentityResources().ToList();

            var apis = _context.ApiResources().ToList();

            var result = new Resources(
                identity.Select(x => x.ToModel()).AsEnumerable(),
                apis.Select(x => x.ToModel()).AsEnumerable());

            _logger.LogDebug("Found {scopes} as all scopes in database",
                result.IdentityResources.Select(x => x.Name)
                    .Union(result.ApiResources.SelectMany(x => x.Scopes).Select(x => x.Name)));

            return Task.FromResult(result);
        }
    }
}