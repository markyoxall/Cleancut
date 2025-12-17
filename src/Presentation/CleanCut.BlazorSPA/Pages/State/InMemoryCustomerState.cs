using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CleanCut.BlazorSPA.Pages.Models;
using Microsoft.JSInterop;

namespace CleanCut.BlazorSPA.Pages.State;

public class InMemoryCustomerState : ICustomerState
{
    private const string StorageKey = "cleanCut.customers.v1";

    private readonly IJSRuntime _js;
    private readonly List<SimpleCustomer> _customers = new();
    private bool _loaded;

    public InMemoryCustomerState(IJSRuntime js)
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
                var items = JsonSerializer.Deserialize<List<SimpleCustomer>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (items != null)
                {
                    _customers.Clear();
                    _customers.AddRange(items);
                }
            }
        }
        catch
        {
            // ignore JS interop errors and fall back to seeded data
        }

        if (!_customers.Any())
        {
            // seed demo data if storage empty
            _customers.Add(new SimpleCustomer { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Vunder", Email = "alice@example.com", IsActive = true, Age = 34 });
            _customers.Add(new SimpleCustomer { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Builder", Email = "bob@example.com", IsActive = true, Age = 39 });
            _customers.Add(new SimpleCustomer { Id = Guid.NewGuid(), FirstName = "Charlie", LastName = "Chocolate", Email = "charlie@example.com", IsActive = false, Age = 23 });
            await SaveAsync();
        }

        _loaded = true;
    }

    private async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_customers);
            await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }
        catch
        {
            // ignore storage failures for demo
        }
    }

    public async Task<List<SimpleCustomer>> GetAllAsync()
    {
        await EnsureLoadedAsync();
        // return clones to avoid accidental external modification
        return _customers.Select(Clone).ToList();
    }

    public async Task<SimpleCustomer?> GetByIdAsync(Guid id)
    {
        await EnsureLoadedAsync();
        var found = _customers.FirstOrDefault(c => c.Id == id);
        return found is null ? null : Clone(found);
    }

    public async Task CreateAsync(SimpleCustomer customer)
    {
        await EnsureLoadedAsync();
        var copy = Clone(customer);
        if (copy.Id == Guid.Empty)
            copy.Id = Guid.NewGuid();

        _customers.Add(copy);
        await SaveAsync();
    }

    public async Task UpdateAsync(SimpleCustomer customer)
    {
        await EnsureLoadedAsync();
        var idx = _customers.FindIndex(c => c.Id == customer.Id);
        if (idx >= 0)
            _customers[idx] = Clone(customer);
        await SaveAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await EnsureLoadedAsync();
        var removed = _customers.RemoveAll(c => c.Id == id) > 0;
        if (removed)
            await SaveAsync();
        return removed;
    }

    private static SimpleCustomer Clone(SimpleCustomer src)
    {
        return new SimpleCustomer
        {
            Id = src.Id,
            FirstName = src.FirstName,
            LastName = src.LastName,
            Email = src.Email,
            IsActive = src.IsActive,
            Age = src.Age,
            BirthDate = src.BirthDate,
            Balance = src.Balance,
            Rating = src.Rating,
            Gender = src.Gender,
            PreferredContact = src.PreferredContact,
            PhoneNumber = src.PhoneNumber,
            Website = src.Website,
            PostalCode = src.PostalCode,
            MemberSince = src.MemberSince,
            LastLogin = src.LastLogin,
            PreferredContactTime = src.PreferredContactTime,
            Notes = src.Notes,
            IsSubscribed = src.IsSubscribed,
            IsPremium = src.IsPremium,
            Country = src.Country,
            FavoriteColor = src.FavoriteColor,
            Tags = src.Tags is null ? new List<string>() : new List<string>(src.Tags),
            UploadedFileName = src.UploadedFileName,
            UploadedFileSize = src.UploadedFileSize
        };
    }
}
