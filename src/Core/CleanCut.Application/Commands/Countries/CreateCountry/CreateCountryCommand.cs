using MediatR;
using CleanCut.Application.DTOs;

namespace CleanCut.Application.Commands.Countries.CreateCountry;

/// <summary>
/// Command to create a new country
/// </summary>
public record CreateCountryCommand(
    string Name,
    string Code
) : IRequest<CountryDto>;