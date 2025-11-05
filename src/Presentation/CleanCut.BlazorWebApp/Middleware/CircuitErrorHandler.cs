using Microsoft.AspNetCore.Components.Server.Circuits;

namespace CleanCut.BlazorWebApp.Middleware;

public class CircuitErrorHandler : CircuitHandler
{
    private readonly ILogger<CircuitErrorHandler> _logger;

    public CircuitErrorHandler(ILogger<CircuitErrorHandler> logger)
{
        _logger = logger;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
   _logger.LogInformation("Circuit {CircuitId} opened", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
   _logger.LogInformation("Circuit {CircuitId} closed", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
    _logger.LogWarning("Circuit {CircuitId} connection down", circuit.Id);
  return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit {CircuitId} connection restored", circuit.Id);
  return Task.CompletedTask;
    }
}