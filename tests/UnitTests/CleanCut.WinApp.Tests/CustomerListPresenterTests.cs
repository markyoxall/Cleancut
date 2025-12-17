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

namespace CleanCut.WinApp.Tests;

public class CustomerListPresenterTests
{
    [StaFact]
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

        services.AddTransient<CustomerListPresenter>();

        var sp = services.BuildServiceProvider();

        var presenter = ActivatorUtilities.CreateInstance<CustomerListPresenter>(sp, form);

        presenter.Initialize();

        // allow async initialization to complete
        await Task.Delay(200);

        Assert.True(form.Displayed);
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
    }
}
