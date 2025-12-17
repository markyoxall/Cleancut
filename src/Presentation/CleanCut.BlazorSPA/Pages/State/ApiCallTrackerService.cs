using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CleanCut.BlazorSPA.Pages.State;

public class ApiCallTrackerService : IApiCallTracker
{
    private readonly List<string> _messages = new();

    public int Count => _messages.Count;

    public IReadOnlyList<string> Messages => new ReadOnlyCollection<string>(_messages);

    public event Action? Changed;

    public void Record(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        _messages.Add(message);
        Changed?.Invoke();
    }

    public void Clear()
    {
        _messages.Clear();
        Changed?.Invoke();
    }
}
