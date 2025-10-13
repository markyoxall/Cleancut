using MediatR;

namespace CleanCut.Application.Commands.Countries.DeleteCountry;

/// <summary>
/// Command to delete a country by ID
/// </summary>
public record DeleteCountryCommand(Guid Id) : IRequest<bool>;