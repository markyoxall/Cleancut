using System;
using System.Threading.Tasks;
using CleanCut.BlazorSPA.Pages.Models;
using CleanCut.BlazorSPA.Pages.State;
using Microsoft.JSInterop;

namespace CleanCut.BlazorSPA.Tests;

public class InMemoryCustomerStateSanity
{
    public static async Task RunSanityAsync()
    {
        var js = new NullJsRuntime();
        var state = new InMemoryCustomerState(js);

        var c = new SimpleCustomer { FirstName = "T", LastName = "U", Email = "t@u.com" };
        await state.CreateAsync(c);
        var all = await state.GetAllAsync();
        if (all.Count == 0) throw new Exception("Sanity failed: no customers");

        var fetched = await state.GetByIdAsync(all[0].Id);
        if (fetched is null) throw new Exception("Sanity failed: get by id returned null");

        var deleted = await state.DeleteAsync(all[0].Id);
        if (!deleted) throw new Exception("Sanity failed: delete returned false");
    }

    // Minimal IJSRuntime that returns null for getItem calls
    private sealed class NullJsRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            return new ValueTask<TValue>(default(TValue)!);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            return new ValueTask<TValue>(default(TValue)!);
        }
    }
}
