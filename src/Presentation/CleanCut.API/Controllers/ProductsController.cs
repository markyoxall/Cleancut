using Microsoft.AspNetCore.Mvc;
using MediatR;
using CleanCut.Application.Commands.Products.CreateProduct;
using CleanCut.Application.Commands.Products.UpdateProduct;
using CleanCut.Application.Queries.Products.GetProduct;
using CleanCut.Application.Queries.Products.GetProductsByUser;
using CleanCut.Application.DTOs;

namespace CleanCut.API.Controllers;

/// <summary>
/// API Controller for Product operations - Version 1
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all products (v1) - Added for testing purposes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAllProducts(CancellationToken cancellationToken)
    {
        // For demonstration, we'll get products for the first seeded user
        // In a real API, you might want pagination or require authentication
        var seededUserId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // This should match your seeded data
        
        var query = new GetProductsByUserQuery(seededUserId);
        var products = await _mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Get product by ID (v1)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductQuery(id);
        var product = await _mediator.Send(query, cancellationToken);
        
        if (product == null)
            return Problem(
                title: "Product not found",
                detail: $"Product with ID '{id}' was not found",
                statusCode: StatusCodes.Status404NotFound,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            );
            
        return Ok(product);
    }

    /// <summary>
    /// Get all products for a specific user (v1)
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProductsByUser(Guid userId, CancellationToken cancellationToken)
    {
        var query = new GetProductsByUserQuery(userId);
        var products = await _mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Create a new product (v1)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                title: "Business rule violation",
                detail: ex.Message,
                statusCode: StatusCodes.Status422UnprocessableEntity,
                type: "https://tools.ietf.org/html/rfc4918#section-11.2"
            );
        }
        catch (ArgumentException ex)
        {
            return Problem(
                title: "Invalid input",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            );
        }
    }

    /// <summary>
    /// Update an existing product (v1)
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, UpdateProductCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return Problem(
                title: "ID mismatch",
                detail: "The ID in the URL does not match the ID in the request body",
                statusCode: StatusCodes.Status400BadRequest,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            );

        try
        {
            var product = await _mediator.Send(command, cancellationToken);
            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                title: "Business rule violation",
                detail: ex.Message,
                statusCode: StatusCodes.Status422UnprocessableEntity,
                type: "https://tools.ietf.org/html/rfc4918#section-11.2"
            );
        }
    }
}