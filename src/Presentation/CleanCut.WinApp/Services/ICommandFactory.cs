using CleanCut.Application.Commands.Countries.CreateCountry;
using CleanCut.Application.Commands.Countries.UpdateCountryCommand;
using CleanCut.Application.Commands.Countries.DeleteCountry;
using CleanCut.Application.Commands.Customers.CreateCustomer;
using CleanCut.Application.Commands.Customers.UpdateCustomer;
using CleanCut.Application.Commands.Customers.DeleteCustomer;
using CleanCut.Application.Commands.Products.CreateProduct;
using CleanCut.Application.Commands.Products.UpdateProduct;
using CleanCut.Application.Commands.Products.DeleteProduct;
using CleanCut.WinApp.Views.Countries;
using CleanCut.WinApp.Views.Customers;
using CleanCut.WinApp.Views.Products;

namespace CleanCut.WinApp.Services;

/// <summary>
/// Factory for creating application commands from WinApp view models.
/// Keeps mapping/command-construction logic inside the presentation layer.
/// </summary>
public interface ICommandFactory
{

    CreateCountryCommand CreateCountryCommand(CountryEditViewModel vm);
    UpdateCountryCommand UpdateCountryCommand(Guid id, CountryEditViewModel vm);
   DeleteCountryCommand DeleteCountryCommand(Guid id);


    CreateCustomerCommand CreateCustomerCommand(CustomerEditViewModel vm);
    UpdateCustomerCommand UpdateCustomerCommand(Guid id, CustomerEditViewModel vm);
    DeleteCustomerCommand DeleteCustomerCommand(Guid id);

    CreateProductCommand CreateProductCommand(ProductEditViewModel vm);
    UpdateProductCommand UpdateProductCommand(Guid id, ProductEditViewModel vm);
    DeleteProductCommand DeleteProductCommand(Guid id);
}
