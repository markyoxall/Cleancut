using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Commands.Countries.UpdateCountryCommand;

/// <summary>
/// Command to update an existing country
/// </summary>
public record UpdateCountryCommand(
    Guid Id,
    string Name,
    string Code
) : IRequest<CountryDto>;