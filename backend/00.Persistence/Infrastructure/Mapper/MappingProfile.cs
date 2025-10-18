using AutoMapper;
using Domain.Entity;
using Infrastructure.Dto;
using Domain.CustomEntities;

namespace Infrastructure.Mapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Users, UserDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.PasswordHash))
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.ProfileImageUrl))
            .ReverseMap();
        CreateMap<Clients, ClientDto>()
            .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.MembershipTypeId, opt => opt.Ignore())
            .ForMember(dest => dest.MembershipStatusId, opt => opt.Ignore())
            .ForMember(dest => dest.MembershipStatusName, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<MembershipTypes, MembershipTypeDto>().ReverseMap();
        CreateMap<PaymentMethods, PaymentMethodDto>().ReverseMap();
        CreateMap<Coupons, CouponDto>().ReverseMap();
        CreateMap<Roles, RoleDto>().ReverseMap();
    }
}
