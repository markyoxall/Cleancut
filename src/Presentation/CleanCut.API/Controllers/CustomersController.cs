using Microsoft.AspNetCore.Mvc;
using MediatR;
using CleanCut.Application.Commands.Customers.CreateCustomer;
using CleanCut.Application.Commands.Customers.UpdateCustomer;
using CleanCut.Application.Queries.Customers.GetCustomer;
using CleanCut.Application.Queries.Customers.GetAllCustomers;
using CleanCut.Application.DTOs;

namespace CleanCut.API.Controllers;

/// <summary>
/// API Controller for Customer operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ApiControllerBase
{
    public CustomersController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
  /// Get all customers
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerInfo>>> GetCustomers(CancellationToken cancellationToken)
    {
        var query = new GetAllCustomersQuery();
  var customers = await Send(query, cancellationToken);
        return Ok(customers);
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
 [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerInfo>> GetCustomer(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCustomerQuery(id);
 var customer = await Send(query, cancellationToken);

        if (customer == null)
      return NotFound($"Customer with ID {id} not found");

        return Ok(customer);
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
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
    /// Update an existing customer
    /// </summary>
 [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerInfo>> UpdateCustomer(Guid id, UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
      return BadRequest("ID in URL does not match ID in request body");

        try
        {
     var customer = await Send(command, cancellationToken);
    return Ok(customer);
  }
  catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
    }
    }
}