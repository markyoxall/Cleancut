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
        CreateMap<Customer, CustomerInfo>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.GetFullName()));

        CreateMap<Product, ProductInfo>();

        CreateMap<Country, CountryInfo>();

        CreateMap<Order, OrderInfo>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TotalItemCount, opt => opt.MapFrom(src => src.GetTotalItemCount()))
            .ForMember(dest => dest.CustomerName, opt => opt.Ignore()) // Set in handler
            .ForMember(dest => dest.CustomerEmail, opt => opt.Ignore()); // Set in handler

        CreateMap<OrderLineItem, OrderLineItemInfo>()
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.GetLineTotal()));
    }
}