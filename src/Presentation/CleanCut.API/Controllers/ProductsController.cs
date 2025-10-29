using Microsoft.AspNetCore.Mvc;
using MediatR;
using CleanCut.Application.Commands.Products.CreateProduct;
using CleanCut.Application.Commands.Products.UpdateProduct;
using CleanCut.Application.Commands.Products.DeleteProduct;
using CleanCut.Application.Queries.Products.GetProduct;
using CleanCut.Application.Queries.Products.GetProductsByCustomer;
using CleanCut.Application.DTOs;
using Microsoft.Extensions.Logging;
using CleanCut.Application.Queries.Products.GetAllProducts;

namespace CleanCut.API.Controllers;

/// <summary>
/// API Controller for Product operations - Version 1
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ApiControllerBase
{
    public ProductsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all products (v1) -  
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductInfo>>> GetAllProducts(CancellationToken cancellationToken)
    {
         
        var query = new GetAllProductsQuery();
        var products = await  Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Get product by ID (v1)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductInfo>> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductQuery(id);
        var product = await  Send(query, cancellationToken);
        
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
    [ProducesResponseType(typeof(IReadOnlyList<ProductInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProductInfo>>> GetProductsByUser(Guid userId, CancellationToken cancellationToken)
    {
        var query = new GetProductsByCustomerQuery(userId);
        var products = await  Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Create a new product (v1)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductInfo), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductInfo>> CreateProduct(CreateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var product = await  Send(command, cancellationToken);
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
    [ProducesResponseType(typeof(ProductInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductInfo>> UpdateProduct(Guid id, UpdateProductCommand command, CancellationToken cancellationToken)
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
            var product = await  Send(command, cancellationToken);
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

    /// <summary>
    /// Delete a product (v1)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ProductsController>>();
        logger.LogInformation("DELETE request received for product {ProductId}", id);
        
        try
        {
            var command = new DeleteProductCommand(id);
            var success = await  Send(command, cancellationToken);
            
            if (!success)
            {
                logger.LogWarning("Product {ProductId} not found for deletion", id);
                return Problem(
                    title: "Product not found",
                    detail: $"Product with ID '{id}' was not found",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "https://tools.ietf.org/html/rfc7231#section-6.5.4"
                );
            }
            
            logger.LogInformation("Product {ProductId} deleted successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting product {ProductId}", id);
            return Problem(
                title: "Internal server error",
                detail: "An error occurred while deleting the product",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}