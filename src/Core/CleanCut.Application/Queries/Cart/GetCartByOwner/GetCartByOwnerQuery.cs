using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Queries.Cart.GetCartByOwner;

public record GetCartByOwnerQuery(Guid OwnerId) : IRequest<CartInfo?>;
