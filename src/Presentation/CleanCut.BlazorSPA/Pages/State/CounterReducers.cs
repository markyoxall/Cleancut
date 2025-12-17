using CleanCut.BlazorSPA.Pages.Models;
using Fluxor;
using System;

namespace CleanCut.BlazorSPA.Pages.State
{
    public static class CounterReducers
    {
        [ReducerMethod]
        public static CounterState ReduceIncrement(
            CounterState state,
            IncrementCounterAction action)
        {
            Console.WriteLine($"CounterReducers.ReduceIncrement called: old={state.Count} new={state.Count + 1}");
            return state with { Count = state.Count + 1 };
        }
    }
}
