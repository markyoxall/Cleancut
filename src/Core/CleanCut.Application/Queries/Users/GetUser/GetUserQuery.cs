using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Queries.Users.GetUser;

/// <summary>
/// Query to get a user by ID
/// </summary>
public record GetUserQuery(Guid Id) : IRequest<UserDto?>;