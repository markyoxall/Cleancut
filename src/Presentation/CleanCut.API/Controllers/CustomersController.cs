using Microsoft.AspNetCore.Mvc;
using MediatR;
using CleanCut.Application.Commands.Customers.CreateCustomer;
using CleanCut.Application.Commands.Customers.UpdateCustomer;
using CleanCut.Application.Commands.Customers.DeleteCustomer;
using CleanCut.Application.Queries.Customers.GetCustomer;
using CleanCut.Application.Queries.Customers.GetAllCustomers;
using CleanCut.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CleanCut.API.Hubs;

namespace CleanCut.API.Controllers;

/// <summary>
/// API Controller for Customer operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // ? CRITICAL FIX: Require authentication for all endpoints
public class CustomersController : ApiControllerBase
{
    public CustomersController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
  /// Get all customers - Requires authentication
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<CustomerInfo>>> GetCustomers(CancellationToken cancellationToken)
    {
   var query = new GetAllCustomersQuery();
   var customers = await Send(query, cancellationToken);
        return Ok(customers);
    }

    /// <summary>
    /// Get customer by ID - Requires authentication
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  public async Task<ActionResult<CustomerInfo>> GetCustomer(Guid id, CancellationToken cancellationToken)
    {
   var query = new GetCustomerQuery(id);
        var customer = await Send(query, cancellationToken);

        if (customer == null)
 return NotFound($"Customer with ID {id} not found");

   return Ok(customer);
    }

    /// <summary>
    /// Create a new customer - Requires authentication
    /// </summary>
[HttpPost]
    [ProducesResponseType(typeof(CustomerInfo), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CustomerInfo>> CreateCustomer(CreateCustomerCommand command, CancellationToken cancellationToken)
{
        try
        {
            var customer = await Send(command, cancellationToken);
      return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }
     catch (InvalidOperationException ex)
        {
      return BadRequest(ex.Message);
    }
    }

    /// <summary>
/// Update an existing customer - Requires authentication
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
 public async Task<ActionResult<CustomerInfo>> UpdateCustomer(Guid id, UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
  return BadRequest("ID in URL does not match ID in request body");

    try
        {
 var customer = await Send(command, cancellationToken);

            // Notifications (SignalR, cache invalidation, integration publish) are handled by the domain event pipeline

      return Ok(customer);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete a customer - Requires Admin role
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")] // ? Enterprise security: Admin-only for destructive operations
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
          return BadRequest("Invalid customer ID");

 try
  {
        var command = new DeleteCustomerCommand(id);
          var success = await Send(command, cancellationToken);
            
    if (!success)
 return NotFound($"Customer with ID {id} not found");

            return NoContent();
     }
        catch (InvalidOperationException ex)
        {
         return BadRequest(ex.Message);
        }
    }
}
