using MediatR;
using CleanCut.Application.DTOs;
using System;

namespace CleanCut.Application.Queries.Countries.GetCountry;

/// <summary>
/// Query to get a country by ID
/// </summary>
public record GetCountryQuery(Guid Id) : IRequest<CountryInfo?>;