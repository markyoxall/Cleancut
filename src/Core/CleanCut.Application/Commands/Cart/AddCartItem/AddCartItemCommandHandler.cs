using MediatR;
using AutoMapper;
using CleanCut.Domain.Repositories;
using CleanCut.Application.DTOs;
using CleanCut.Domain.Entities;
using CleanCut.Domain.Exceptions;

namespace CleanCut.Application.Commands.Cart.AddCartItem;

public class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand, CartInfo>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddCartItemCommandHandler(ICartRepository cartRepository, IProductRepository productRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CartInfo> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null) throw new EntityNotFoundException("Product", request.ProductId);

        var cart = await _cartRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
        if (cart == null)
        {
            cart = new CleanCut.Domain.Entities.Cart(request.OwnerId);
            await _cartRepository.AddAsync(cart, cancellationToken);
        }

        cart.AddItem(request.ProductId, product.Name, product.Price, request.Quantity);
        await _cartRepository.UpdateAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var info = _mapper.Map<CartInfo>(cart);
        info.Total = cart.GetTotal();
        return info;
    }
}
