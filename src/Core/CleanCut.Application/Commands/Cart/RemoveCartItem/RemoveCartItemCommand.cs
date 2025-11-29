using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Commands.Cart.RemoveCartItem;

public record RemoveCartItemCommand(Guid OwnerId, Guid ProductId) : IRequest<CartInfo?>;
