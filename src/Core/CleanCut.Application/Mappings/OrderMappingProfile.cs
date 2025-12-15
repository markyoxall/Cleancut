using AutoMapper;
using CleanCut.Domain.Entities;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping domain Order entities to OrderInfo DTOs
/// </summary>
public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<OrderLineItem, OrderLineItemInfo>()
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.GetLineTotal()));

        CreateMap<Order, OrderInfo>()
            .ForMember(dest => dest.TotalItemCount, opt => opt.MapFrom(src => src.OrderLineItems.Sum(i => i.Quantity)))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.OrderLineItems.Sum(i => i.GetLineTotal())))
            .ForMember(dest => dest.OrderLineItems, opt => opt.MapFrom(src => src.OrderLineItems))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
