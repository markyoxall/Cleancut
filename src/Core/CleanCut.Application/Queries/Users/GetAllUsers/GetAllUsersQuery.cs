using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Queries.Users.GetAllUsers;

/// <summary>
/// Query to get all users
/// </summary>
public record GetAllUsersQuery() : IRequest<IReadOnlyList<UserDto>>;