using Microsoft.AspNetCore.Mvc;
using MediatR;
using CleanCut.Application.Commands.Products.CreateProduct;
using CleanCut.Application.Commands.Products.UpdateProduct;
using CleanCut.Application.Queries.Products.GetProduct;
using CleanCut.Application.Queries.Products.GetProductsByCustomer;
using CleanCut.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace CleanCut.API.Controllers.V2;

/// <summary>
/// API Controller for Product operations - Version 2
/// Enhanced with additional features and improved responses
/// </summary>
[ApiController]
[Route("api/v2/[controller]")]
[Authorize] // ? Require authentication for all endpoints
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all products (v2) - Enhanced with pagination and metadata
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> GetAllProducts(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination parameters
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            var problemDetails = new ProblemDetails
            {
                Type = "https://api.cleancut.com/problems/invalid-pagination",
                Title = "Invalid pagination parameters",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Page must be >= 1 and pageSize must be between 1 and 100",
                Instance = HttpContext.Request.Path
            };
            
            problemDetails.Extensions.Add("requestId", HttpContext.TraceIdentifier);
            problemDetails.Extensions.Add("timestamp", DateTime.UtcNow);
            problemDetails.Extensions.Add("apiVersion", "2.0");
            problemDetails.Extensions.Add("invalidValues", new { page, pageSize });
            
            return BadRequest(problemDetails);
        }

        // For demonstration, get products for the first seeded customer
        var seededCustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        
        var query = new GetProductsByCustomerQuery(seededCustomerId);
        var allProducts = await _mediator.Send(query, cancellationToken);
        
        // Apply pagination
        var pagedProducts = allProducts
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        return Ok(new {
            Data = pagedProducts,
            Pagination = new {
                Page = page,
                PageSize = pageSize,
                TotalItems = allProducts.Count,
                TotalPages = (int)Math.Ceiling(allProducts.Count / (double)pageSize)
            },
            ApiVersion = "2.0",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get product by ID (v2) - Enhanced with metadata
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductQuery(id);
        var product = await _mediator.Send(query, cancellationToken);
        
        if (product == null)
        {
            var problemDetails = new ProblemDetails
            {
                Type = "https://api.cleancut.com/problems/product-not-found",
                Title = "Product not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Product with ID '{id}' was not found",
                Instance = HttpContext.Request.Path
            };
            
   // Add V2 specific extensions
            problemDetails.Extensions.Add("requestId", HttpContext.TraceIdentifier);
            problemDetails.Extensions.Add("timestamp", DateTime.UtcNow);
         problemDetails.Extensions.Add("apiVersion", "2.0");
problemDetails.Extensions.Add("productId", id);
       
   return NotFound(problemDetails);
        }
   
  return Ok(new {
     Data = product,
   ApiVersion = "2.0",
Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get all products for a specific customer (v2) - Enhanced with pagination
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> GetProductsByCustomer(
     Guid customerId, 
  [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination parameters
 if (page < 1 || pageSize < 1 || pageSize > 100)
        {
    var problemDetails = new ProblemDetails
            {
  Type = "https://api.cleancut.com/problems/invalid-pagination",
            Title = "Invalid pagination parameters",
           Status = StatusCodes.Status400BadRequest,
             Detail = "Page must be >= 1 and pageSize must be between 1 and 100",
     Instance = HttpContext.Request.Path
      };
            
     problemDetails.Extensions.Add("requestId", HttpContext.TraceIdentifier);
   problemDetails.Extensions.Add("timestamp", DateTime.UtcNow);
            problemDetails.Extensions.Add("apiVersion", "2.0");
       problemDetails.Extensions.Add("invalidValues", new { page, pageSize });

         return BadRequest(problemDetails);
        }

   var query = new GetProductsByCustomerQuery(customerId);
        var allProducts = await _mediator.Send(query, cancellationToken);
  
  // Simple pagination for v2
        var pagedProducts = allProducts
         .Skip((page - 1) * pageSize)
   .Take(pageSize)
            .ToList();
        
        return Ok(new {
    Data = pagedProducts,
            Pagination = new {
   Page = page,
     PageSize = pageSize,
    TotalItems = allProducts.Count,
                TotalPages = (int)Math.Ceiling(allProducts.Count / (double)pageSize)
  },
 ApiVersion = "2.0",
     Timestamp = DateTime.UtcNow
  });
    }

    /// <summary>
    /// Create a new product (v2) - Enhanced response format
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
 [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<object>> CreateProduct(CreateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
          var product = await _mediator.Send(command, cancellationToken);
          
    var response = new {
     Data = product,
    Message = "Product created successfully",
       ApiVersion = "2.0",
   Timestamp = DateTime.UtcNow
            };
            
 return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, response);
        }
        catch (InvalidOperationException ex)
 {
            var problemDetails = new ProblemDetails
          {
            Type = "https://api.cleancut.com/problems/business-rule-violation",
      Title = "Business rule violation",
      Status = StatusCodes.Status422UnprocessableEntity,
  Detail = ex.Message,
   Instance = HttpContext.Request.Path
          };
            
      problemDetails.Extensions.Add("requestId", HttpContext.TraceIdentifier);
       problemDetails.Extensions.Add("timestamp", DateTime.UtcNow);
problemDetails.Extensions.Add("apiVersion", "2.0");
        
   return UnprocessableEntity(problemDetails);
        }
     catch (ArgumentException ex)
        {
     var problemDetails = new ProblemDetails
   {
         Type = "https://api.cleancut.com/problems/invalid-input",
          Title = "Invalid input",
      Status = StatusCodes.Status400BadRequest,
 Detail = ex.Message,
      Instance = HttpContext.Request.Path
      };
            
         problemDetails.Extensions.Add("requestId", HttpContext.TraceIdentifier);
            problemDetails.Extensions.Add("timestamp", DateTime.UtcNow);
            problemDetails.Extensions.Add("apiVersion", "2.0");
 
 return BadRequest(problemDetails);
    }
    }

    /// <summary>
    /// Update an existing product (v2) - Enhanced validation and response
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<object>> UpdateProduct(Guid id, UpdateProductCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
      {
var problemDetails = new ProblemDetails
       {
        Type = "https://api.cleancut.com/problems/id-mismatch",
 Title = "ID mismatch",
    Status = StatusCodes.Status400BadRequest,
       Detail = "The ID in the URL does not match the ID in the request body",
           Instance = HttpContext.Request.Path
       };
 
     problemDetails.Extensions.Add("requestId", HttpContext.TraceIdentifier);
            problemDetails.Extensions.Add("timestamp", DateTime.UtcNow);
            problemDetails.Extensions.Add("apiVersion", "2.0");
     problemDetails.Extensions.Add("urlId", id);
            problemDetails.Extensions.Add("bodyId", command.Id);
         
         return BadRequest(problemDetails);
        }

        try
        {
     var product = await _mediator.Send(command, cancellationToken);
   
  return Ok(new {
         Data = product,
       Message = "Product updated successfully",
         ApiVersion = "2.0",
        Timestamp = DateTime.UtcNow
       });
      }
        catch (InvalidOperationException ex)
        {
            var problemDetails = new ProblemDetails
       {
      Type = "https://api.cleancut.com/problems/business-rule-violation",
  Title = "Business rule violation",
 Status = StatusCodes.Status422UnprocessableEntity,
          Detail = ex.Message,
             Instance = HttpContext.Request.Path
            };
 
            problemDetails.Extensions.Add("requestId", HttpContext.TraceIdentifier);
  problemDetails.Extensions.Add("timestamp", DateTime.UtcNow);
            problemDetails.Extensions.Add("apiVersion", "2.0");
    
    return UnprocessableEntity(problemDetails);
        }
    }

    /// <summary>
    /// Get product statistics (v2 only) - New feature
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetProductStatistics(CancellationToken cancellationToken)
    {
      // This would typically call a new query handler
        // For demo purposes, we'll return mock data
        
        var stats = new {
     TotalProducts = 100,
        AvailableProducts = 85,
    UnavailableProducts = 15,
 AveragePrice = 299.99m,
         LastUpdated = DateTime.UtcNow.AddMinutes(-30)
        };
   
     return Ok(new {
            Data = stats,
         Message = "Product statistics retrieved successfully",
          ApiVersion = "2.0",
            Timestamp = DateTime.UtcNow
        });
    }
}