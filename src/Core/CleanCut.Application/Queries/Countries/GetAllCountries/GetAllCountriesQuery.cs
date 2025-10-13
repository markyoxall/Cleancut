using MediatR;
using CleanCut.Application.DTOs;
using System.Collections.Generic;

namespace CleanCut.Application.Queries.Countries.GetAllCountries;

/// <summary>
/// Query to get all countries
/// </summary>
public record GetAllCountriesQuery() : IRequest<List<CountryDto>>;