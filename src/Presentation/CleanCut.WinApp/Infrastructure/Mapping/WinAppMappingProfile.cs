using AutoMapper;
using CleanCut.Application.DTOs;
using CleanCut.WinApp.Views.Customers;
using CleanCut.WinApp.Views.Products;

namespace CleanCut.WinApp.Infrastructure.Mapping;

public class WinAppMappingProfile : Profile
{
    public WinAppMappingProfile()
    {
        // Customer mappings
        CreateMap<CustomerEditViewModel, CustomerInfo>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? Guid.Empty))
            .ForMember(dest => dest.FullName, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<CustomerInfo, CustomerEditViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        // Product mappings
        CreateMap<ProductEditViewModel, ProductInfo>()
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.UserId));

        CreateMap<ProductInfo, ProductEditViewModel>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.CustomerId));




    }
}
