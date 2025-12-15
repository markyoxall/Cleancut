using System;
using System.Threading.Tasks;
using CleanCut.Application.DTOs;

namespace CleanCut.BlazorWebApp.Services.Notifications;

public interface INotificationsClient
{
    event Func<CustomerInfo, Task>? CustomerUpdated;
    event Func<ProductInfo, Task>? ProductCreated;
    event Func<ProductInfo, Task>? ProductUpdated;
    event Func<OrderInfo, Task>? OrderCreated;
    event Func<OrderInfo, Task>? OrderUpdated;
    event Func<OrderInfo, Task>? OrderStatusChanged;
    event Func<CountryInfo, Task>? CountryCreated;
    event Func<CountryInfo, Task>? CountryUpdated;

    Task StartAsync();
    Task StopAsync();
}
