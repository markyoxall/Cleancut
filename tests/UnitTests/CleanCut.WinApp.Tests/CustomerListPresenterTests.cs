using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;
using CleanCut.WinApp.Presenters;
using CleanCut.WinApp.Views.Customers;
using CleanCut.Application.DTOs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using CleanCut.Application.Common.Interfaces;
using CleanCut.WinApp.Services.Caching;

namespace CleanCut.WinApp.Tests;

public class CustomerListPresenterTests
{
    [Fact]
    public async Task Initialize_ShouldLoadCustomersAndDisplay()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var mockMediator = new Mock<IMediator>();
        var sampleCustomers = new List<CustomerInfo>
        {
            new CustomerInfo { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john@example.com" }
        };

        mockMediator.Setup(m => m.Send(It.IsAny<object>(), default)).ReturnsAsync(sampleCustomers);

        services.AddSingleton<IMediator>(mockMediator.Object);

        // Create a real form implementing ICustomerListView for testing
        var form = new TestCustomerListForm();
        services.AddSingleton<ICustomerListView>(_ => form);

        // Mock cache service and cache manager
        var mockCacheService = new Mock<ICacheService>();
        mockCacheService.Setup(c => c.GetAsync<List<CustomerInfo>>(It.IsAny<string>(), default)).ReturnsAsync((List<CustomerInfo>?)null);
        mockCacheService.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<List<CustomerInfo>>(), It.IsAny<TimeSpan?>(), default)).Returns(Task.CompletedTask);
        services.AddSingleton<ICacheService>(mockCacheService.Object);

        var mockCacheManager = new Mock<ICacheManager>();
        mockCacheManager.Setup(m => m.InvalidateCustomersAsync(default)).Returns(Task.CompletedTask);
        services.AddSingleton<ICacheManager>(mockCacheManager.Object);

        services.AddTransient<CustomerListPresenter>();

        var sp = services.BuildServiceProvider();

        var presenter = ActivatorUtilities.CreateInstance<CustomerListPresenter>(sp, form);

        presenter.Initialize();

        // allow async initialization to complete
        await Task.Delay(200);

        Assert.True(form.Displayed);
    }

    [Fact]
    public async Task AddCustomer_ShouldInvalidateCache_OnSuccess()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var mockMediator = new Mock<IMediator>();
        mockMediator.Setup(m => m.Send(It.IsAny<object>(), default)).ReturnsAsync(new List<CustomerInfo>());
        services.AddSingleton<IMediator>(mockMediator.Object);

        var form = new TestCustomerListForm();
        services.AddSingleton<ICustomerListView>(_ => form);

        var mockCacheService = new Mock<ICacheService>();
        mockCacheService.Setup(c => c.GetAsync<List<CustomerInfo>>(It.IsAny<string>(), default)).ReturnsAsync((List<CustomerInfo>?)null);
        mockCacheService.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<List<CustomerInfo>>(), It.IsAny<TimeSpan?>(), default)).Returns(Task.CompletedTask);
        services.AddSingleton<ICacheService>(mockCacheService.Object);

        var mockCacheManager = new Mock<ICacheManager>();
        mockCacheManager.Setup(m => m.InvalidateCustomersAsync(default)).Returns(Task.CompletedTask).Verifiable();
        services.AddSingleton<ICacheManager>(mockCacheManager.Object);

        services.AddTransient<CustomerListPresenter>();

        var sp = services.BuildServiceProvider();

        var presenter = ActivatorUtilities.CreateInstance<CustomerListPresenter>(sp, form);

        // Simulate Add flow by invoking the private handler via the event
        presenter.Initialize();

        // Trigger AddCustomerRequested
        form.InvokeAddRequested();

        // allow async handler to run
        await Task.Delay(500);

        mockCacheManager.Verify(m => m.InvalidateCustomersAsync(default), Times.AtLeastOnce);
    }

    private class TestCustomerListForm : Form, ICustomerListView
    {
        public event EventHandler? AddCustomerRequested;
        public event EventHandler<Guid>? EditCustomerRequested;
        public event EventHandler<Guid>? DeleteCustomerRequested;
        public event EventHandler? RefreshRequested;

        public bool Displayed { get; private set; }

        public void DisplayCustomers(IEnumerable<CustomerInfo> users)
        {
            Displayed = true;
        }

        public void ClearCustomers() { }
        public Guid? GetSelectedCustomerId() => null;

        public void ShowError(string message) { }
        public void ShowInfo(string message) { }
        public void ShowSuccess(string message) { }
        public bool ShowConfirmation(string message) => true;
        public void SetLoading(bool isLoading) { }

        public void ApplyLayout(string? layout)
        {
            // No-op for test
        }

        public void SetTheme(string? theme)
        {
            // No-op for test
        }

        // Implement ApplyGridPreferences required by ICustomerListView
        public void ApplyGridPreferences(List<string>? columnOrder, Dictionary<string, int>? columnWidths)
        {
            // No-op for tests
        }

        public void InvokeAddRequested()
        {
            AddCustomerRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
