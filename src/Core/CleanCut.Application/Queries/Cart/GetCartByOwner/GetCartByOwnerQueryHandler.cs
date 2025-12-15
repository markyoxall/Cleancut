using MediatR;
using AutoMapper;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Repositories;

namespace CleanCut.Application.Queries.Cart.GetCartByOwner;

public class GetCartByOwnerQueryHandler : IRequestHandler<GetCartByOwnerQuery, CartInfo?>
{
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;

    public GetCartByOwnerQueryHandler(ICartRepository cartRepository, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _mapper = mapper;
    }

    public async Task<CartInfo?> Handle(GetCartByOwnerQuery request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
        if (cart == null) return null;
        var info = _mapper.Map<CartInfo>(cart);
        info.Total = cart.GetTotal();
        return info;
    }
}
