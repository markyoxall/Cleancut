using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.Application.Commands.Users.CreateUser;

/// <summary>
/// Command to create a new user
/// </summary>
public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email
) : IRequest<UserDto>, ICacheInvalidator
{
    public IEnumerable<string> CacheKeysToInvalidate => 
        ["user:all", $"user:email:{Email.ToLowerInvariant()}"];
}