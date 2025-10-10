using MediatR;

namespace CleanCut.Application.Commands.Products.DeleteProduct;

/// <summary>
/// Command to delete a product
/// </summary>
public record DeleteProductCommand(Guid Id) : IRequest<bool>;