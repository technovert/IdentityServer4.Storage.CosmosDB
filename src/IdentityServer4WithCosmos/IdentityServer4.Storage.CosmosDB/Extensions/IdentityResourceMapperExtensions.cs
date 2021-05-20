using AutoMapper;
using IdentityServer4.Storage.CosmosDB.Mappers;
using IdentityServer4.Models;
using EF = IdentityServer4.EntityFramework.Entities;

namespace IdentityServer4.Storage.CosmosDB.Extensions
{
    /// <summary>
    ///     Extension methods to map IdentityServer4.Models.IdentityResource to
    ///     IdentityServer4.Storage.CosmosDB.Entities.IdentityResource.
    /// </summary>
    public static class IdentityResourceMapperExtensions
    {
        static IdentityResourceMapperExtensions()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<IdentityResourceMapperProfile>())
                .CreateMapper();
        }

        internal static IMapper Mapper { get; }

        /// <summary>
        ///     Will map all data in IdentityServer4.Storage.CosmosDB.Entities.IdentityResource to
        ///     IdentityServer4.Models.IdentityResource.
        /// </summary>
        /// <param name="entity">The IdentityServer4.Storage.CosmosDB.Entities.IdentityResource to gather data from.</param>
        /// <returns>An instance of IdentityServer4.Models.IdentityResource.</returns>
        public static IdentityResource ToModel(this Entities.IdentityResource entity)
        {
            return entity == null ? null : Mapper.Map<IdentityResource>(entity);
        }

        /// <summary>
        ///     Will map all data in IdentityServer4.Models.IdentityResource to
        ///     IdentityServer4.Storage.CosmosDB.Entities.IdentityResource.
        /// </summary>
        /// <param name="model">The IdentityServer4.Models.IdentityResource to gather data from.</param>
        /// <returns>An instance of IdentityServer4.Storage.CosmosDB.Entities.IdentityResource.</returns>
        public static Entities.IdentityResource ToEntity(this IdentityResource model)
        {
            return model == null ? null : Mapper.Map<Entities.IdentityResource>(model);
        }
        
        public static Entities.IdentityResource ToEntity(this EF.IdentityResource model)
        {
            return model == null ? null : Mapper.Map<Entities.IdentityResource>(model);
        }
    }
}