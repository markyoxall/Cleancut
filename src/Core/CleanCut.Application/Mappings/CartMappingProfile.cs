using AutoMapper;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Entities;

namespace CleanCut.Application.Mappings;

public class CartMappingProfile : Profile
{
    public CartMappingProfile()
    {
        CreateMap<CartItem, CartItemDto>();
        CreateMap<Cart, CartInfo>();
    }
}
