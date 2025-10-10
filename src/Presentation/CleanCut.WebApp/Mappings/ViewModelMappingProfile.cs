using AutoMapper;
using CleanCut.Application.DTOs;
using CleanCut.WebApp.Models.Users;
using CleanCut.WebApp.Models.Products;

namespace CleanCut.WebApp.Mappings;

/// <summary>
/// AutoMapper profile for converting between DTOs and ViewModels
/// </summary>
public class ViewModelMappingProfile : Profile
{
    public ViewModelMappingProfile()
    {
        // User mappings
        CreateMap<UserDto, UserEditViewModel>();
        CreateMap<UserEditViewModel, UserDto>();
        
        // Product mappings
        CreateMap<ProductDto, ProductEditViewModel>();
        CreateMap<ProductEditViewModel, ProductDto>();
    }
}