using MediatR;
using CleanCut.Application.DTOs;
using CleanCut.Application.Common.Interfaces;
using System;
using System.Collections.Generic;

namespace CleanCut.Application.Queries.Countries.GetAllCountries;

/// <summary>
/// Query to get all countries
/// </summary>
public record GetAllCountriesQuery() : IRequest<List<CountryInfo>>, ICacheableQuery
{
    public string CacheKey => "countries:all";
    public TimeSpan? Expiration => TimeSpan.FromHours(1);
}
