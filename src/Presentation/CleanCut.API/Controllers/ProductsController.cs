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
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace CleanCut.API.Controllers;

/// <summary>
/// API Controller for Product operations - Version 1
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // ? Require authentication for all endpoints
public class ProductsController : ApiControllerBase
{
    private readonly ILogger<ProductsController> _logger;

 public ProductsController(IMediator mediator, ILogger<ProductsController> logger) : base(mediator)
    {
    _logger = logger;
    }

    /// <summary>
    /// Get all products (v1) - Requires User or Admin role
    /// </summary>
 [HttpGet]
    [Authorize(Policy = "UserOrAdmin")] // ? Role-based authorization
    [ProducesResponseType(typeof(IReadOnlyList<ProductInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<ProductInfo>>> GetAllProducts(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all products for user: {User}", User.Identity?.Name);
     
   var query = new GetAllProductsQuery();
        var products = await Send(query, cancellationToken);
     
        _logger.LogInformation("Retrieved {Count} products", products.Count());
        return Ok(products);
    }

 /// <summary>
    /// Get product by ID (v1) - Requires User or Admin role
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "UserOrAdmin")] // ? Role-based authorization
    [ProducesResponseType(typeof(ProductInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
 public async Task<ActionResult<ProductInfo>> GetProduct(
     [Required] Guid id, 
        CancellationToken cancellationToken)
    {
        // ? Input validation
        if (id == Guid.Empty)
        {
     return Problem(
       title: "Invalid product ID",
      detail: "Product ID cannot be empty",
        statusCode: StatusCodes.Status400BadRequest,
          type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
      );
        }

  _logger.LogInformation("Getting product {ProductId} for user: {User}", id, User.Identity?.Name);

        var query = new GetProductQuery(id);
        var product = await Send(query, cancellationToken);
        
   if (product == null)
        {
      _logger.LogWarning("Product {ProductId} not found for user: {User}", id, User.Identity?.Name);
  return Problem(
  title: "Product not found",
  detail: $"Product with ID '{id}' was not found",
statusCode: StatusCodes.Status404NotFound,
      type: "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            );
        }
            
      _logger.LogInformation("Successfully retrieved product {ProductId}", id);
        return Ok(product);
    }

    /// <summary>
 /// Get all products for a specific user (v1) - Requires User or Admin role
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [Authorize(Policy = "UserOrAdmin")] // ? Role-based authorization
  [ProducesResponseType(typeof(IReadOnlyList<ProductInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
 public async Task<ActionResult<IReadOnlyList<ProductInfo>>> GetProductsByUser(
        [Required] Guid userId, 
        CancellationToken cancellationToken)
    {
        // ? Input validation
     if (userId == Guid.Empty)
  {
  return Problem(
      title: "Invalid user ID",
           detail: "User ID cannot be empty",
     statusCode: StatusCodes.Status400BadRequest,
        type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
  );
        }

     _logger.LogInformation("Getting products for user {UserId} requested by: {RequestedBy}", 
            userId, User.Identity?.Name);

        var query = new GetProductsByCustomerQuery(userId);
   var products = await Send(query, cancellationToken);
     
        _logger.LogInformation("Retrieved {Count} products for user {UserId}", products.Count(), userId);
        return Ok(products);
    }

    /// <summary>
    /// Create a new product (v1) - Requires User or Admin role
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "UserOrAdmin")] // ? Role-based authorization
 [ProducesResponseType(typeof(ProductInfo), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductInfo>> CreateProduct(
        [FromBody][Required] CreateProductCommand command, 
CancellationToken cancellationToken)
 {
        // ? Additional server-side validation
        if (command == null)
 {
      return Problem(
      title: "Invalid request",
    detail: "Request body cannot be null",
statusCode: StatusCodes.Status400BadRequest,
  type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
  );
        }

        _logger.LogInformation("Creating product {ProductName} for user: {User}", 
            command.Name, User.Identity?.Name);

        try
        {
         var product = await Send(command, cancellationToken);
         
          _logger.LogInformation("Successfully created product {ProductId} with name {ProductName}", 
  product.Id, product.Name);
            
       return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
      }
      catch (InvalidOperationException ex)
    {
      _logger.LogWarning("Business rule violation when creating product: {Error}", ex.Message);
            return Problem(
   title: "Business rule violation",
    detail: ex.Message,
                statusCode: StatusCodes.Status422UnprocessableEntity,
   type: "https://tools.ietf.org/html/rfc4918#section-11.2"
         );
  }
        catch (ArgumentException ex)
  {
      _logger.LogWarning("Invalid input when creating product: {Error}", ex.Message);
   return Problem(
       title: "Invalid input",
          detail: ex.Message,
  statusCode: StatusCodes.Status400BadRequest,
    type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            );
        }
    }

    /// <summary>
    /// Update an existing product (v1) - Requires User or Admin role
    /// </summary>
    [HttpPut("{id:guid}")]
 [Authorize(Policy = "UserOrAdmin")] // ? Role-based authorization
    [ProducesResponseType(typeof(ProductInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductInfo>> UpdateProduct(
 [Required] Guid id, 
        [FromBody][Required] UpdateProductCommand command, 
 CancellationToken cancellationToken)
    {
        // ? Enhanced validation
     if (id == Guid.Empty)
        {
      return Problem(
           title: "Invalid product ID",
detail: "Product ID cannot be empty",
         statusCode: StatusCodes.Status400BadRequest,
               type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
    );
     }

        if (command == null)
    {
            return Problem(
 title: "Invalid request",
     detail: "Request body cannot be null",
  statusCode: StatusCodes.Status400BadRequest,
   type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
    );
}

        if (id != command.Id)
        {
         _logger.LogWarning("ID mismatch: URL ID {UrlId} vs Command ID {CommandId} for user: {User}", 
           id, command.Id, User.Identity?.Name);
  return Problem(
       title: "ID mismatch",
       detail: "The ID in the URL does not match the ID in the request body",
           statusCode: StatusCodes.Status400BadRequest,
          type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
  );
        }

        _logger.LogInformation("Updating product {ProductId} for user: {User}", id, User.Identity?.Name);

    try
     {
     var product = await Send(command, cancellationToken);
      
 _logger.LogInformation("Successfully updated product {ProductId}", id);
          return Ok(product);
        }
    catch (InvalidOperationException ex)
     {
     _logger.LogWarning("Business rule violation when updating product {ProductId}: {Error}", id, ex.Message);
        return Problem(
     title: "Business rule violation",
        detail: ex.Message,
     statusCode: StatusCodes.Status422UnprocessableEntity,
      type: "https://tools.ietf.org/html/rfc4918#section-11.2"
     );
        }
  }

    /// <summary>
 /// Delete a product (v1) - Requires Admin role
    /// </summary>
    [HttpDelete("{id:guid}")]
 [Authorize(Policy = "AdminOnly")] // ? Restrict to admin only
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
 public async Task<IActionResult> DeleteProduct(
        [Required] Guid id, 
        CancellationToken cancellationToken)
 {
        // ? Input validation
   if (id == Guid.Empty)
     {
        return Problem(
                title: "Invalid product ID",
    detail: "Product ID cannot be empty",
 statusCode: StatusCodes.Status400BadRequest,
      type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
      );
        }

        _logger.LogInformation("DELETE request received for product {ProductId} by admin: {User}", 
    id, User.Identity?.Name);
        
        try
        {
            var command = new DeleteProductCommand(id);
            var success = await Send(command, cancellationToken);
       
   if (!success)
        {
            _logger.LogWarning("Product {ProductId} not found for deletion by admin: {User}", 
      id, User.Identity?.Name);
      return Problem(
   title: "Product not found",
      detail: $"Product with ID '{id}' was not found",
        statusCode: StatusCodes.Status404NotFound,
       type: "https://tools.ietf.org/html/rfc7231#section-6.5.4"
           );
     }
      
  _logger.LogInformation("Product {ProductId} deleted successfully by admin: {User}", 
     id, User.Identity?.Name);
         return NoContent();
     }
        catch (Exception ex)
        {
   _logger.LogError(ex, "Error deleting product {ProductId} by admin: {User}", 
         id, User.Identity?.Name);
  return Problem(
              title: "Internal server error",
 detail: "An error occurred while deleting the product",
            statusCode: StatusCodes.Status500InternalServerError
      );
      }
    }
}