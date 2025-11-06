using Microsoft.AspNetCore.Mvc;
using MediatR;
using CleanCut.Application.Commands.Orders.CreateOrder;
using CleanCut.Application.Commands.Orders.UpdateOrder;
using CleanCut.Application.Commands.Orders.DeleteOrder;
using CleanCut.Application.Commands.Orders.AddOrderLineItem;
using CleanCut.Application.Queries.Orders.GetOrder;
using CleanCut.Application.Queries.Orders.GetAllOrders;
using CleanCut.Application.Queries.Orders.GetOrdersByCustomer;
using CleanCut.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace CleanCut.API.Controllers;

/// <summary>
/// API Controller for Order operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class OrdersController : ApiControllerBase
{
    public OrdersController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all orders - Requires authentication
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<OrderInfo>>> GetOrders(CancellationToken cancellationToken)
    {
        var query = new GetAllOrdersQuery();
        var orders = await Send(query, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Get order by ID - Requires authentication
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderInfo>> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery(id);
        var order = await Send(query, cancellationToken);

        if (order == null)
            return NotFound($"Order with ID {id} not found");

        return Ok(order);
    }

    /// <summary>
    /// Get orders by customer ID - Requires authentication
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<OrderInfo>>> GetOrdersByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var query = new GetOrdersByCustomerQuery(customerId);
        var orders = await Send(query, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Create a new order - Requires authentication
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderInfo), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderInfo>> CreateOrder(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var order = await Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing order - Requires authentication
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OrderInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderInfo>> UpdateOrder(Guid id, UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID in URL does not match ID in request body");

        try
        {
            var order = await Send(command, cancellationToken);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Add a line item to an order - Requires authentication
    /// </summary>
    [HttpPost("{id:guid}/lineitems")]
    [ProducesResponseType(typeof(OrderInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderInfo>> AddOrderLineItem(Guid id, AddOrderLineItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new AddOrderLineItemCommand(id, request.ProductId, request.Quantity);
            var order = await Send(command, cancellationToken);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete an order - Requires Admin role
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")] // Admin-only for destructive operations
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid order ID");

        try
        {
            var command = new DeleteOrderCommand(id);
            var success = await Send(command, cancellationToken);

            if (!success)
                return NotFound($"Order with ID {id} not found");

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

// Local request models for API calls
public class AddOrderLineItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}