using CleanCut.Application.DTOs;
using System.Threading;

namespace CleanCut.BlazorWebApp.State;

public interface ICountriesState
{
    IReadOnlyList<CountryDto> Countries { get; }
    event Action? StateChanged;
    event Action<List<CountryDto>>? CountriesChanged;

    bool IsLoading { get; }
    event Action<string, bool>? MessageChanged;

    // CancellationToken added for parity with other feature states
    Task LoadAsync(bool force = false, CancellationToken cancellationToken = default);
    void Invalidate();
}