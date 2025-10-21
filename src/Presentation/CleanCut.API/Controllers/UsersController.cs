using Microsoft.AspNetCore.Mvc;
using MediatR;
using CleanCut.Application.Commands.Users.CreateUser;
using CleanCut.Application.Commands.Users.UpdateUser;
using CleanCut.Application.Queries.Users.GetUser;
using CleanCut.Application.Queries.Users.GetAllUsers;
using CleanCut.Application.DTOs;

namespace CleanCut.API.Controllers;

/// <summary>
/// API Controller for User operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController :  ApiControllerBase
{
     

    public UsersController(IMediator mediator) : base(mediator)
    {
        
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserInfo>>> GetUsers(CancellationToken cancellationToken)
    {
        var query = new GetAllUsersQuery();
        var users = await  Send(query, cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserInfo>> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserQuery(id);
        var user = await  Send(query, cancellationToken);
        
        if (user == null)
            return NotFound($"User with ID {id} not found");
            
        return Ok(user);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserInfo>> CreateUser(CreateUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var user = await  Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserInfo>> UpdateUser(Guid id, UpdateUserCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID in URL does not match ID in request body");

        try
        {
            var user = await  Send(command, cancellationToken);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}