using System;
using System.Collections.Generic;

namespace CleanCut.BlazorSPA.Pages.State;

public interface IApiCallTracker
{
    int Count { get; }
    IReadOnlyList<string> Messages { get; }
    void Record(string message);
    void Clear();
    event Action? Changed;
}
