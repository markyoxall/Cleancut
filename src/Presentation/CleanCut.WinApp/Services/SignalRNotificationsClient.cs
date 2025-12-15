using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using CleanCut.Application.DTOs;

namespace CleanCut.WinApp.Services;

public class SignalRNotificationsClient : INotificationsClient, IDisposable
{
    private readonly HubConnection _connection;
    private readonly ILogger<SignalRNotificationsClient> _logger;

    public event Func<CustomerInfo, Task>? CustomerUpdated;
    public event Func<ProductInfo, Task>? ProductCreated;
    public event Func<ProductInfo, Task>? ProductUpdated;
    public event Func<OrderInfo, Task>? OrderCreated;
    public event Func<OrderInfo, Task>? OrderUpdated;
    public event Func<OrderInfo, Task>? OrderStatusChanged;
    public event Func<CountryInfo, Task>? CountryCreated;
    public event Func<CountryInfo, Task>? CountryUpdated;

    public SignalRNotificationsClient(string hubUrl, ILogger<SignalRNotificationsClient> logger)
    {
        _logger = logger;
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        RegisterHandlers();
    }

    private void RegisterHandlers()
    {
        _connection.On<CustomerInfo>("CustomerUpdated", async dto => { try { if (CustomerUpdated != null) await CustomerUpdated(dto); } catch (Exception ex) { _logger.LogWarning(ex, "CustomerUpdated handler failed"); } });
        _connection.On<ProductInfo>("ProductCreated", async dto => { try { if (ProductCreated != null) await ProductCreated(dto); } catch (Exception ex) { _logger.LogWarning(ex, "ProductCreated handler failed"); } });
        _connection.On<ProductInfo>("ProductUpdated", async dto => { try { if (ProductUpdated != null) await ProductUpdated(dto); } catch (Exception ex) { _logger.LogWarning(ex, "ProductUpdated handler failed"); } });
        _connection.On<OrderInfo>("OrderCreated", async dto => { try { if (OrderCreated != null) await OrderCreated(dto); } catch (Exception ex) { _logger.LogWarning(ex, "OrderCreated handler failed"); } });
        _connection.On<OrderInfo>("OrderUpdated", async dto => { try { if (OrderUpdated != null) await OrderUpdated(dto); } catch (Exception ex) { _logger.LogWarning(ex, "OrderUpdated handler failed"); } });
        _connection.On<OrderInfo>("OrderStatusChanged", async dto => { try { if (OrderStatusChanged != null) await OrderStatusChanged(dto); } catch (Exception ex) { _logger.LogWarning(ex, "OrderStatusChanged handler failed"); } });
        _connection.On<CountryInfo>("CountryCreated", async dto => { try { if (CountryCreated != null) await CountryCreated(dto); } catch (Exception ex) { _logger.LogWarning(ex, "CountryCreated handler failed"); } });
        _connection.On<CountryInfo>("CountryUpdated", async dto => { try { if (CountryUpdated != null) await CountryUpdated(dto); } catch (Exception ex) { _logger.LogWarning(ex, "CountryUpdated handler failed"); } });
    }

    public async Task StartAsync()
    {
        try { await _connection.StartAsync(); } catch (Exception ex) { _logger.LogWarning(ex, "Failed to start SignalR connection"); }
    }

    public async Task StopAsync()
    {
        try { await _connection.StopAsync(); } catch (Exception ex) { _logger.LogWarning(ex, "Failed to stop SignalR connection"); }
    }

    public void Dispose()
    {
        try { _connection.DisposeAsync().AsTask().Wait(); } catch { }
    }
}
