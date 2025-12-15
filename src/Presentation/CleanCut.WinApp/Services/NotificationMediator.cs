using System;
using System.Threading.Tasks;
using CleanCut.Application.DTOs;

namespace CleanCut.WinApp.Services;

public interface INotificationMediator
{
    event Func<CustomerInfo, Task>? CustomerUpdated;
    event Func<ProductInfo, Task>? ProductCreated;
    event Func<ProductInfo, Task>? ProductUpdated;
    event Func<OrderInfo, Task>? OrderCreated;
    event Func<OrderInfo, Task>? OrderUpdated;
    event Func<OrderInfo, Task>? OrderStatusChanged;
    event Func<CountryInfo, Task>? CountryCreated;
    event Func<CountryInfo, Task>? CountryUpdated;

    Task RaiseCustomerUpdated(CustomerInfo dto);
    Task RaiseProductCreated(ProductInfo dto);
    Task RaiseProductUpdated(ProductInfo dto);
    Task RaiseOrderCreated(OrderInfo dto);
    Task RaiseOrderUpdated(OrderInfo dto);
    Task RaiseOrderStatusChanged(OrderInfo dto);
    Task RaiseCountryCreated(CountryInfo dto);
    Task RaiseCountryUpdated(CountryInfo dto);
}

public class NotificationMediator : INotificationMediator
{
    public event Func<CustomerInfo, Task>? CustomerUpdated;
    public event Func<ProductInfo, Task>? ProductCreated;
    public event Func<ProductInfo, Task>? ProductUpdated;
    public event Func<OrderInfo, Task>? OrderCreated;
    public event Func<OrderInfo, Task>? OrderUpdated;
    public event Func<OrderInfo, Task>? OrderStatusChanged;
    public event Func<CountryInfo, Task>? CountryCreated;
    public event Func<CountryInfo, Task>? CountryUpdated;

    public Task RaiseCustomerUpdated(CustomerInfo dto) => CustomerUpdated?.Invoke(dto) ?? Task.CompletedTask;
    public Task RaiseProductCreated(ProductInfo dto) => ProductCreated?.Invoke(dto) ?? Task.CompletedTask;
    public Task RaiseProductUpdated(ProductInfo dto) => ProductUpdated?.Invoke(dto) ?? Task.CompletedTask;
    public Task RaiseOrderCreated(OrderInfo dto) => OrderCreated?.Invoke(dto) ?? Task.CompletedTask;
    public Task RaiseOrderUpdated(OrderInfo dto) => OrderUpdated?.Invoke(dto) ?? Task.CompletedTask;
    public Task RaiseOrderStatusChanged(OrderInfo dto) => OrderStatusChanged?.Invoke(dto) ?? Task.CompletedTask;
    public Task RaiseCountryCreated(CountryInfo dto) => CountryCreated?.Invoke(dto) ?? Task.CompletedTask;
    public Task RaiseCountryUpdated(CountryInfo dto) => CountryUpdated?.Invoke(dto) ?? Task.CompletedTask;
}
