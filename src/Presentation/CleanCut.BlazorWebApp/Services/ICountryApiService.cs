using CleanCut.Application.DTOs;

namespace CleanCut.BlazorWebApp.Services;

public interface ICountryApiService
{
    Task<List<CountryInfo>> GetAllCountriesAsync(CancellationToken cancellationToken = default);
    Task<CountryInfo?> GetCountryByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
