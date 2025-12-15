/*
 * Products API Controller (Version 1)
 * ===================================
 * 
 * This controller demonstrates enterprise-level authorization and authentication
 * implementation for the CleanCut API. It serves as the primary example of how
 * protected API endpoints integrate with the overall authentication architecture.
 * 
 * AUTHENTICATION FLOW INTEGRATION:
 * --------------------------------
 * 1. CLIENT AUTHENTICATION:
 *  ??? Client applications obtain JWT tokens from CleanCut IdentityServer
 *    ??? Tokens contain user identity claims and "CleanCutAPI" audience
 * 
 * 2. API AUTHORIZATION:
 *    ??? This controller validates Bearer tokens on every request
 *    ??? Extracts user claims (role, name, email) from JWT payload
 *    ??? Enforces role-based policies for different operations
 * 
 * ROLE-BASED ACCESS CONTROL:
 * --------------------------
 * 
 * • Authentication Required (Most endpoints):
 *   ??? GET /api/v1/products - List all products
 *   ??? GET /api/v1/products/{id} - Get specific product
 *   ??? GET /api/v1/products/customer/{customerId} - Get customer's products
 *   ??? POST /api/v1/products - Create new product
 *   ??? PUT /api/v1/products/{id} - Update existing product
 *   ??? Authenticated Users: ? Can perform these operations
 *   ??? Anonymous: ? 401 Unauthorized
 * 
 * • AdminOnly Policy (Destructive operations):
 *   ??? DELETE /api/v1/products/{id} - Delete product
 *   ??? Regular Users: ? 403 Forbidden 
 *   ??? Admins: ? Can perform these operations
 *   ??? Anonymous: ? 401 Unauthorized
 * 
 * CLIENT APPLICATION USAGE:
 * -------------------------
 * 
 * • CleanCut.BlazorWebApp:
 *   ??? TokenService obtains Client Credentials token
 *   ??? ProductApiClient includes Bearer token in HTTP requests
 *   ??? Server-side Blazor components call these endpoints
 *   ??? UI adapts based on user roles from claims
 * 
 * • CleanCut.WebApp (MVC):
 *   ??? After user login, receives access token
 *   ??? Controllers use HttpClient with Bearer authentication
 *   ??? Views show/hide features based on user roles
 *   ??? JavaScript ajax calls include Authorization header
 * 
 * • CleanCut.WinApp (Future):
 *   ??? Desktop app would use HttpClient with Bearer tokens
 *   ??? Login flow opens browser for user authentication
 *   ??? Local storage of tokens for API calls
 * 
 * SECURITY FEATURES IMPLEMENTED:
 * ------------------------------
 * • [Authorize] attribute on controller - All endpoints require authentication
 * • Role-based policies on individual actions
 * • Comprehensive input validation with detailed error responses
 * • Security logging without exposing sensitive data
 * • IP address tracking for audit trails
 * • Rate limiting applied globally (configured in Program.cs)
 * • CORS restrictions to known client origins
 * 
 * ERROR RESPONSES:
 * ---------------
 * • 400 Bad Request - Invalid input data or malformed requests
 * • 401 Unauthorized - Missing or invalid JWT token
 * • 403 Forbidden - Valid token but insufficient role permissions
 * • 404 Not Found - Resource doesn't exist
 * • 422 Unprocessable Entity - Business rule violations
 * • 429 Too Many Requests - Rate limiting exceeded
 * • 500 Internal Server Error - Unexpected errors (minimal details exposed)
 * 
 * AUTHENTICATION DEBUGGING:
 * -------------------------
 * Use these test accounts created by SeedData:
 * 
 * • Alice (Admin): admin@cleancut.com
 *   ??? Can access ALL endpoints including DELETE operations
 *   ??? Role claim: "Admin"
 *   ??? Department: "IT", Employee ID: "EMP001"
 * 
 * • Bob (User): user@cleancut.com  
 *   ??? Can access GET, POST, PUT operations only
 *   ??? Cannot access DELETE operations (403 Forbidden)
 *   ??? Role claim: "User"
 *   ??? Department: "Sales", Employee ID: "EMP002"
 */

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
    /// Get all products (v1) - Requires authentication
    /// </summary>
    [HttpGet]
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
    /// Get product by ID (v1) - Requires authentication
 /// </summary>
    [HttpGet("{id:guid}")]
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
    /// Get all products for a specific customer (v1) - Requires authentication
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProductInfo>>> GetProductsByCustomer(
  [Required] Guid customerId, 
        CancellationToken cancellationToken)
    {
 // ? Input validation
        if (customerId == Guid.Empty)
        {
   return Problem(
  title: "Invalid customer ID",
         detail: "Customer ID cannot be empty",
 statusCode: StatusCodes.Status400BadRequest,
    type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
     );
    }

        _logger.LogInformation("Getting products for customer {CustomerId} requested by: {RequestedBy}", 
  customerId, User.Identity?.Name);

        var query = new GetProductsByCustomerQuery(customerId);
        var products = await Send(query, cancellationToken);
     
        _logger.LogInformation("Retrieved {Count} products for customer {CustomerId}", products.Count(), customerId);
        return Ok(products);
}

    /// <summary>
    /// Create a new product (v1) - Requires authentication
    /// </summary>
    [HttpPost]
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
    /// Update an existing product (v1) - Requires authentication
    /// </summary>
    [HttpPut("{id:guid}")]
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

        // If the client omitted the Id in the request body (legacy clients), accept the URL id
        var cmd = command;
        if (cmd.Id == Guid.Empty)
        {
            cmd = cmd with { Id = id };
        }

        if (id != cmd.Id)
        {
         _logger.LogWarning("ID mismatch: URL ID {UrlId} vs Command ID {CommandId} for user: {User}", 
           id, cmd.Id, User.Identity?.Name);
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
            var product = await Send(cmd, cancellationToken);
      
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
    [Authorize(Policy = "AdminOnly")] // ? Keep admin-only restriction for destructive operations
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
