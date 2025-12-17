using Fluxor;
using System;
using System.Threading.Tasks;

namespace CleanCut.BlazorSPA.Pages.State
{
    public class IncrementCounterEffect : Effect<IncrementCounterAction>
    {
        public override Task HandleAsync(IncrementCounterAction action, IDispatcher dispatcher)
        {
            Console.WriteLine("IncrementCounterEffect.HandleAsync called");
            return Task.CompletedTask;
        }
    }
}
