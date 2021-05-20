using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Storage.CosmosDB.Entities;

namespace IdentityServer4.Storage.CosmosDB.Interfaces
{
    public interface IConfigurationDbContext : IDisposable
    {
        IQueryable<Client> Clients(string partitionKey = "");

        IQueryable<IdentityResource> IdentityResources(string partitionKey = "");

        IQueryable<ApiResource> ApiResources(string partitionKey = "");

        Task AddClient(Client entity);

        Task AddIdentityResource(IdentityResource entity);

        Task AddApiResource(ApiResource entity);
    }
}