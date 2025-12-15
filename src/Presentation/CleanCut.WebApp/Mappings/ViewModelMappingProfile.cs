using AutoMapper;
using CleanCut.Application.DTOs;
using CleanCut.WebApp.Models.Customers;
using CleanCut.WebApp.Models.Products;

namespace CleanCut.WebApp.Mappings;

/// <summary>
/// AutoMapper profile for converting between DTOs and ViewModels
/// </summary>
public class ViewModelMappingProfile : Profile
{
    public ViewModelMappingProfile()
    {
        // Customer mappings
        CreateMap<CustomerInfo, CustomerEditViewModel>();
        CreateMap<CustomerEditViewModel, CustomerInfo>();
        
        // Product mappings
        CreateMap<ProductInfo, ProductEditViewModel>();
        CreateMap<ProductEditViewModel, ProductInfo>();
    }
}