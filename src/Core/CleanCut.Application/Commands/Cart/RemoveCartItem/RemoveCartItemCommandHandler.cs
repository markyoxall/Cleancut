using MediatR;
using AutoMapper;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Repositories;
using CleanCut.Domain.Exceptions;

namespace CleanCut.Application.Commands.Cart.RemoveCartItem;

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, CartInfo?>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RemoveCartItemCommandHandler(ICartRepository cartRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CartInfo?> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
        if (cart == null) return null;

        cart.RemoveItem(request.ProductId);
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var info = _mapper.Map<CartInfo>(cart);
        info.Total = cart.GetTotal();
        return info;
    }
}
