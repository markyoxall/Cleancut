using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CleanCut.BlazorSPA.Pages.Models;
using Microsoft.JSInterop;

namespace CleanCut.BlazorSPA.Pages.State;

public class InMemoryProductState : IProductState
{
    private const string StorageKey = "cleanCut.products.v1";

    private readonly IJSRuntime _js;
    private readonly List<SimpleProduct> _products = new();
    private bool _loaded;

    public InMemoryProductState(IJSRuntime js)
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
                var items = JsonSerializer.Deserialize<List<SimpleProduct>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (items != null)
                {
                    _products.Clear();
                    _products.AddRange(items);
                }
            }
        }
        catch
        {
            // ignore JS interop errors and fall back to seeded data
        }

        if (!_products.Any())
        {
            // seed demo data if storage empty
            _products.Add(new SimpleProduct { Id = Guid.NewGuid(), Name = "Widget A", Sku = "WIDGET-A", Description = "High-quality widget", Category = ProductCategory.Tools, Price = new Money(9.99m, "USD"), QuantityInStock = 100, IsAvailable = true, Tags = new List<string>{ "tool", "widget" } });
            _products.Add(new SimpleProduct { Id = Guid.NewGuid(), Name = "Gadget B", Sku = "GADGET-B", Description = "Multi-purpose gadget", Category = ProductCategory.Electronics, Price = new Money(24.5m, "USD"), QuantityInStock = 42, IsAvailable = true, Tags = new List<string>{ "gadget" } });
            _products.Add(new SimpleProduct { Id = Guid.NewGuid(), Name = "Tool C", Sku = "TOOL-C", Description = "Durable tool", Category = ProductCategory.Tools, Price = new Money(5.0m, "USD"), QuantityInStock = 0, IsAvailable = false, Tags = new List<string>{ "cheap", "tool" } });

            await SaveAsync();
        }

        _loaded = true;
    }

    private async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_products);
            await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }
        catch
        {
            // ignore storage failures for demo
        }
    }

    private static SimpleProduct Clone(SimpleProduct src)
    {
        if (src is null) return null!;

        return new SimpleProduct
        {
            Id = src.Id,
            Name = src.Name,
            Sku = src.Sku,
            Description = src.Description,
            Category = src.Category,
            Price = src.Price is null ? null : new Money(src.Price.Amount, src.Price.Currency),
            QuantityInStock = src.QuantityInStock,
            IsAvailable = src.IsAvailable,
            IsTaxable = src.IsTaxable,
            WeightKg = src.WeightKg,
            Size = src.Size is null ? null : new Dimensions { Length = src.Size.Length, Width = src.Size.Width, Height = src.Size.Height },
            SupplierId = src.SupplierId,
            Tags = src.Tags is null ? new List<string>() : new List<string>(src.Tags),
            Rating = src.Rating,
            ReleaseDate = src.ReleaseDate,
            DiscontinuedDate = src.DiscontinuedDate,
            ImageFileName = src.ImageFileName,
            ImageFileSize = src.ImageFileSize,
            Notes = src.Notes,
            CreatedAt = src.CreatedAt,
            UpdatedAt = src.UpdatedAt
        };
    }

    public async Task CreateAsync(SimpleProduct product)
    {
        await EnsureLoadedAsync();
        var copy = Clone(product);
        if (copy.Id == Guid.Empty)
            copy.Id = Guid.NewGuid();

        _products.Add(copy);
        await SaveAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await EnsureLoadedAsync();
        var removed = _products.RemoveAll(p => p.Id == id) > 0;
        if (removed)
            await SaveAsync();
        return removed;
    }

    public async Task<List<SimpleProduct>> GetAllAsync()
    {
        await EnsureLoadedAsync();
        return _products.Select(Clone).ToList();
    }

    public async Task<SimpleProduct?> GetByIdAsync(Guid id)
    {
        await EnsureLoadedAsync();
        var found = _products.FirstOrDefault(p => p.Id == id);
        return found is null ? null : Clone(found);
    }

    public async Task UpdateAsync(SimpleProduct product)
    {
        await EnsureLoadedAsync();
        var idx = _products.FindIndex(p => p.Id == product.Id);
        if (idx >= 0)
            _products[idx] = Clone(product);
        await SaveAsync();
    }
}
