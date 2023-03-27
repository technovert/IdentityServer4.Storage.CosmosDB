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
    /// Class ClientStore
    /// </summary>
    public class ClientStore : IClientStore
    {
        private readonly IConfigurationDbContext _context;
        private readonly ILogger _logger;

        /// <summary>
        /// constructor for Client Store
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        public ClientStore(IConfigurationDbContext context, ILogger<ClientStore> logger)
        {
            Guard.ForNull(context, nameof(context));
            Guard.ForNull(logger, nameof(logger));

            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Find clients by id
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>

        public Task<Client> FindClientByIdAsync(string clientId)
        {
            var clients = _context.Clients().ToList();

            var model = clients?.FirstOrDefault(_ => _.ClientId.Equals(clientId)).ToModel();

            _logger.LogDebug($"{clientId} found in database: {model != null}");

            return Task.FromResult(model);
        }
    }
}