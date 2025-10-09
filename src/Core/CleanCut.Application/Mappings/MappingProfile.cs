using AutoMapper;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Entities;

namespace CleanCut.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping between domain entities and DTOs
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.GetFullName()));

        CreateMap<Product, ProductDto>();
    }
}