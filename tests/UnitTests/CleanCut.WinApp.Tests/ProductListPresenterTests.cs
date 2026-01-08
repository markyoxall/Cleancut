using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using CleanCut.WinApp.Presenters;
using CleanCut.WinApp.Views.Products;
using CleanCut.Application.DTOs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CleanCut.WinApp.Tests;

public class ProductListPresenterTests
{
    [Fact]
    public async Task Initialize_ShouldLoadProductsAndDisplay()
    {
        var services = new ServiceCollection();

        var mockMediator = new Mock<IMediator>();

        var customerId = Guid.NewGuid();
        var sampleCustomers = new List<CustomerInfo>
        {
            new CustomerInfo { Id = customerId, FirstName = "Alice", LastName = "Smith", Email = "alice@example.com", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        var sampleProducts = new List<ProductInfo>
        {
            new ProductInfo { Id = Guid.NewGuid(), Name = "Prod1", Description = "Desc", Price = 9.99m, CustomerId = customerId }
        };

        // Setup mediator for GetAllCustomersQuery and GetProductsByCustomerQuery
        mockMediator.Setup(m => m.Send(It.IsAny<CleanCut.Application.Queries.Customers.GetAllCustomers.GetAllCustomersQuery>(), default))
            .ReturnsAsync(sampleCustomers);
        mockMediator.Setup(m => m.Send(It.IsAny<CleanCut.Application.Queries.Products.GetProductsByCustomer.GetProductsByCustomerQuery>(), default))
            .ReturnsAsync(sampleProducts);

        services.AddSingleton<IMediator>(mockMediator.Object);

        // Register a test product list form
        var form = new TestProductListForm();
        services.AddSingleton<IProductListView>(_ => form);

        // Provide a simple view factory for IProductEditView
        services.AddSingleton<CleanCut.WinApp.Services.Factories.IViewFactory<IProductEditView>>(sp => new TestProductEditViewFactory());

        services.AddTransient<ProductListPresenter>();

        var sp = services.BuildServiceProvider();

        var presenter = ActivatorUtilities.CreateInstance<ProductListPresenter>(sp, form);
        presenter.Initialize();

        await Task.Delay(200);

        Assert.True(form.Displayed);
    }

    private class TestProductListForm : Form, IProductListView
    {
        public event EventHandler? AddProductRequested;
        public event EventHandler<Guid>? EditProductRequested;
        public event EventHandler<Guid>? DeleteProductRequested;
        public event EventHandler? RefreshRequested;
        public event EventHandler<Guid>? ViewProductsByCustomerRequested;

        public bool Displayed { get; private set; }

        public void DisplayProducts(IEnumerable<ProductInfo> products)
        {
            Displayed = true;
        }

        public void ClearProducts() { }
        public Guid? GetSelectedProductId() => null;
        public void SetAvailableCustomers(IEnumerable<CustomerInfo> customers) { }

        public void ShowError(string message) { }
        public void ShowInfo(string message) { }
        public void ShowSuccess(string message) { }
        public bool ShowConfirmation(string message) => true;
        public void SetLoading(bool isLoading) { }
    }

    private class TestProductEditViewFactory : CleanCut.WinApp.Services.Factories.IViewFactory<IProductEditView>
    {
        public IProductEditView Create() => new TestProductEditView();
    }

    private class TestProductEditView : Form, IProductEditView
    {
        public event EventHandler? SaveRequested;
        public event EventHandler? CancelRequested;
        public void ClearForm() { }
        public void SetAvailableCustomers(IEnumerable<CustomerInfo> customers) { }
        public void SetAvailableUsers(IEnumerable<CustomerInfo> users) { SetAvailableCustomers(users); }
        public ProductEditViewModel GetProductData() => new ProductEditViewModel();
        public void SetProductData(ProductEditViewModel model) { }
        public Dictionary<string, string> ValidateForm() => new Dictionary<string, string>();
        public void ShowError(string message) { }
        public void ShowInfo(string message) { }
        public void ShowSuccess(string message) { }
        public bool ShowConfirmation(string message) => true;
        public void SetLoading(bool isLoading) { }
    }
}
