using Microsoft.AspNetCore.Mvc;
using MediatR;
using CleanCut.Application.Commands.Products.CreateProduct;
using CleanCut.Application.Commands.Products.UpdateProduct;
using CleanCut.Application.Queries.Products.GetProduct;
using CleanCut.Application.Queries.Products.GetProductsByUser;
using CleanCut.Application.DTOs;

namespace CleanCut.API.Controllers;

/// <summary>
/// API Controller for Product operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductQuery(id);
        var product = await _mediator.Send(query, cancellationToken);
        
        if (product == null)
            return NotFound($"Product with ID {id} not found");
            
        return Ok(product);
    }

    /// <summary>
    /// Get all products for a specific user
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProductsByUser(Guid userId, CancellationToken cancellationToken)
    {
        var query = new GetProductsByUserQuery(userId);
        var products = await _mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, UpdateProductCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID in URL does not match ID in request body");

        try
        {
            var product = await _mediator.Send(command, cancellationToken);
            return Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}