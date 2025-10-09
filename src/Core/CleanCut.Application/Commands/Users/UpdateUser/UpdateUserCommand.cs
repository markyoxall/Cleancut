using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Commands.Users.UpdateUser;

/// <summary>
/// Command to update an existing user
/// </summary>
public record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email
) : IRequest<UserDto>;