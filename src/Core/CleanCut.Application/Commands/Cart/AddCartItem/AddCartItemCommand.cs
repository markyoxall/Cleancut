using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Commands.Cart.AddCartItem;

public record AddCartItemCommand(Guid OwnerId, Guid ProductId, int Quantity) : IRequest<CartInfo>;
