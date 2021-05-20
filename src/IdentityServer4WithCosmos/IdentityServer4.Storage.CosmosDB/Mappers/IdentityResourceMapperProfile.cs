using System.Linq;
using AutoMapper;
using IdentityServer4.Storage.CosmosDB.Entities;
using EF = IdentityServer4.EntityFramework;

namespace IdentityServer4.Storage.CosmosDB.Mappers
{
    /// <inheritdoc />
    /// <summary>
    ///     AutoMapper configuration for identity resource
    ///     Between model and entity
    /// </summary>
    public class IdentityResourceMapperProfile : Profile
    {
        /// <summary>
        ///     <see cref="IdentityResourceMapperProfile" />
        /// </summary>
        public IdentityResourceMapperProfile()
        {
            // entity to model
            CreateMap<IdentityResource, Models.IdentityResource>(MemberList.Destination)
                .ForMember(x => x.UserClaims, opt => opt.MapFrom(src => src.UserClaims.Select(x => x.Type)));
            CreateMap<IdentityResource, EF.Entities.IdentityResource>(MemberList.Destination)
                .ForMember(x => x.UserClaims, opt => opt.MapFrom(src => src.UserClaims.Select(x => x.Type)));

            // model to entity
            CreateMap<Models.IdentityResource, IdentityResource>(MemberList.Source)
                .ForMember(x => x.UserClaims,
                    opts => opts.MapFrom(src => src.UserClaims.Select(x => new ApiResourceClaim {Type = x})));

            CreateMap<EF.Entities.IdentityResource, IdentityResource>(MemberList.Source)
                .ForMember(x => x.UserClaims,
                    opts => opts.MapFrom(src => src.UserClaims.Select(x => new ApiResourceClaim { Id = x.Id, Type = x.Type })));
        }
    }
}