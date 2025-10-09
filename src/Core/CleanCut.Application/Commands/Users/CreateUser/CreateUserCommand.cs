using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Commands.Users.CreateUser;

/// <summary>
/// Command to create a new user
/// </summary>
public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email
) : IRequest<UserDto>;