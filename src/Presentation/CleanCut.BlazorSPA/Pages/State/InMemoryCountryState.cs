using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CleanCut.BlazorSPA.Pages.Models;
using Microsoft.JSInterop;

namespace CleanCut.BlazorSPA.Pages.State;

public class InMemoryCountryState : ICountryState
{
    private const string StorageKey = "cleanCut.countries.v1";

    private readonly IJSRuntime _js;
    private readonly List<SimpleCountry> _countries = new();
    private bool _loaded;

    public InMemoryCountryState(IJSRuntime js)
    {
        _js = js ?? throw new ArgumentNullException(nameof(js));
    }


    private async Task EnsureLoadedAsync()
    {
        if (_loaded)
            return;

        try
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(json))
            {
                var items = JsonSerializer.Deserialize<List<SimpleCountry>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (items != null)
                {
                    _countries.Clear();
                    _countries.AddRange(items);
                }
            }
        }
        catch
        {
            // ignore JS interop errors and fall back to seeded data
        }

        if (!_countries.Any())
        {
            // seed demo data if storage empty
            _countries.Add(new SimpleCountry { Id = Guid.NewGuid(), CountryName = "Australia" });
            _countries.Add(new SimpleCountry { Id = Guid.NewGuid(), CountryName = "Brazil" });
            _countries.Add(new SimpleCountry { Id = Guid.NewGuid(), CountryName = "Canada" });

            await SaveAsync();
        }

        _loaded = true;
    }

    private async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_countries);
            await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }
        catch
        {
            // ignore storage failures for demo
        }
    }

    public async Task<List<SimpleCountry>> GetAllAsync()
    {
        await EnsureLoadedAsync();
        // return clones to avoid accidental external modification
        return _countries.Select(Clone).ToList();
    }

    public async Task<SimpleCountry?> GetByIdAsync(Guid id)
    {
        await EnsureLoadedAsync();
        var found = _countries.FirstOrDefault(c => c.Id == id);
        return found is null ? null : Clone(found);
    }

    public async Task CreateAsync(SimpleCountry customer)
    {
        await EnsureLoadedAsync();
        var copy = Clone(customer);
        if (copy.Id == Guid.Empty)
            copy.Id = Guid.NewGuid();

        _countries.Add(copy);
        await SaveAsync();
    }

    public async Task UpdateAsync(SimpleCountry customer)
    {
        await EnsureLoadedAsync();
        var idx = _countries.FindIndex(c => c.Id == customer.Id);
        if (idx >= 0)
            _countries[idx] = Clone(customer);
        await SaveAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await EnsureLoadedAsync();
        var removed = _countries.RemoveAll(c => c.Id == id) > 0;
        if (removed)
            await SaveAsync();
        return removed;
    }

    private static SimpleCountry Clone(SimpleCountry src)
    {
        return new SimpleCountry
        {
            Id = src.Id,
            CountryName = src.CountryName,
        };
    }
}
