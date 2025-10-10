using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Application.Common.Interfaces;

namespace CleanCut.Application.Queries.Users.GetUser;

/// <summary>
/// Query to get a user by ID
/// </summary>
public record GetUserQuery(Guid Id) : IRequest<UserDto?>, ICacheableQuery
{
    public string CacheKey => $"user:id:{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}