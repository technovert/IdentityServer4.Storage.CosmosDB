using System.Linq;
using AutoMapper;
using IdentityServer4.Storage.CosmosDB.Entities;
using IdentityServer4.Models;
using ApiResource = IdentityServer4.Storage.CosmosDB.Entities.ApiResource;
using Secret = IdentityServer4.Models.Secret;
using EF = IdentityServer4.EntityFramework;

namespace IdentityServer4.Storage.CosmosDB.Mappers
{
    /// <inheritdoc />
    /// <summary>
    ///     AutoMapper configuration for API resource
    ///     Between model and entity
    /// </summary>
    public class ApiResourceMapperProfile : Profile
    {
        /// <summary>
        ///     <see cref="ApiResourceMapperProfile" />
        /// </summary>
        public ApiResourceMapperProfile()
        {
            // entity to model
            CreateMap<ApiResource, Models.ApiResource>(MemberList.Destination)
                .ForMember(x => x.ApiSecrets, opt => opt.MapFrom(src => src.Secrets.Select(x => x)))
                .ForMember(x => x.Scopes, opt => opt.MapFrom(src => src.Scopes.Select(x => x)))
                .ForMember(x => x.UserClaims, opts => opts.MapFrom(src => src.UserClaims.Select(x => x.Type)));
            CreateMap<ApiSecret, Secret>(MemberList.Destination);
            CreateMap<ApiScope, Scope>(MemberList.Destination)
                .ForMember(x => x.UserClaims, opt => opt.MapFrom(src => src.UserClaims.Select(x => x.Type)));

            // model to entity
            CreateMap<Models.ApiResource, ApiResource>(MemberList.Source)
                .ForMember(x => x.Secrets, opts => opts.MapFrom(src => src.ApiSecrets.Select(x => x)))
                .ForMember(x => x.Scopes, opts => opts.MapFrom(src => src.Scopes.Select(x => x)))
                .ForMember(x => x.UserClaims,
                    opts => opts.MapFrom(src => src.UserClaims.Select(x => new ApiResourceClaim {Type = x})));

            CreateMap<EF.Entities.ApiResource, ApiResource>(MemberList.Source)
                .ForMember(x => x.Secrets, opts => opts.MapFrom(src => src.Secrets.Select(x => x)))
                .ForMember(x => x.Scopes, opts => opts.MapFrom(src => src.Scopes.Select(x => x)))
                .ForMember(x => x.UserClaims,
                    opts => opts.MapFrom(src => src.UserClaims.Select(x => new ApiResourceClaim { Id = x.Id, Type = x.Type })));

            CreateMap<Secret, ApiSecret>(MemberList.Source);
            CreateMap<EF.Entities.Secret, ApiSecret>(MemberList.Source);

            CreateMap<Scope, ApiScope>(MemberList.Source)
                .ForMember(x => x.UserClaims,
                    opts => opts.MapFrom(src => src.UserClaims.Select(x => new ApiScopeClaim {Type = x})));

            CreateMap<EF.Entities.ApiScope, ApiScope>(MemberList.Source)
                .ForMember(x => x.UserClaims,
                    opts => opts.MapFrom(src => src.UserClaims.Select(x => new ApiScopeClaim {Id = x.Id, Type = x.Type})));
        }
    }
}