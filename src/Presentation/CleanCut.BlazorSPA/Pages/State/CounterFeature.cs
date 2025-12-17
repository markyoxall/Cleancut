using CleanCut.BlazorSPA.Pages.Models;
using Fluxor;

namespace CleanCut.BlazorSPA.Pages.State;

public class CounterFeature : Feature<CounterState>
{
    public override string GetName() => "Counter";

    protected override CounterState GetInitialState() => new CounterState(0);
}
