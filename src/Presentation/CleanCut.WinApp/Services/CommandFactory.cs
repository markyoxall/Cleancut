using CleanCut.Application.Commands.Customers.CreateCustomer;
using CleanCut.Application.Commands.Customers.UpdateCustomer;
using CleanCut.Application.Commands.Products.CreateProduct;
using CleanCut.Application.Commands.Products.UpdateProduct;
using CleanCut.WinApp.Views.Customers;
using CleanCut.WinApp.Views.Products;
using AutoMapper;
using CleanCut.Application.Commands.Countries.UpdateCountryCommand;
using CleanCut.Application.Commands.Countries.CreateCountry;
using CleanCut.WinApp.Views.Countries;
using CleanCut.Application.Commands.Countries.DeleteCountry;
using CleanCut.Application.Commands.Customers.DeleteCustomer;
using CleanCut.Application.Commands.Products.DeleteProduct;

namespace CleanCut.WinApp.Services;

/// <summary>
/// Factory for creating application commands from WinApp view models.
/// Keeps mapping/command-construction logic inside the presentation layer.
/// </summary>
public class CommandFactory : ICommandFactory
{
    private readonly IMapper _mapper;

    public CommandFactory(IMapper mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public CreateCustomerCommand CreateCustomerCommand(CustomerEditViewModel vm)
    {
        var dto = _mapper.Map<CleanCut.Application.DTOs.CustomerInfo>(vm);
        return new CreateCustomerCommand(dto.FirstName, dto.LastName, dto.Email);
    }

    public UpdateCustomerCommand UpdateCustomerCommand(Guid id, CustomerEditViewModel vm)
    {
        var dto = _mapper.Map<CleanCut.Application.DTOs.CustomerInfo>(vm);
        return new UpdateCustomerCommand(id, dto.FirstName, dto.LastName, dto.Email);
    }
    public DeleteCustomerCommand DeleteCustomerCommand(Guid id)
    {
        throw new NotImplementedException();
    }


    public UpdateCountryCommand UpdateCountryCommand(Guid id, CountryEditViewModel vm)
    {
        var dto = _mapper.Map<CleanCut.Application.DTOs.CountryInfo>(vm);
        return new UpdateCountryCommand(id, dto.Code, dto.Name);
    }
    public CreateCountryCommand CreateCountryCommand(CountryEditViewModel vm)
    {
        var dto = _mapper.Map<CleanCut.Application.DTOs.CountryInfo>(vm);
        return new CreateCountryCommand(dto.Code, dto.Name);
    }

    public DeleteCountryCommand DeleteCountryCommand(Guid id)
    {
        throw new NotImplementedException();
    }




    public CreateProductCommand CreateProductCommand(ProductEditViewModel vm)
    {
        var dto = _mapper.Map<CleanCut.Application.DTOs.ProductInfo>(vm);
        return new CreateProductCommand(dto.Name, dto.Description, dto.Price, dto.CustomerId);
    }

    public UpdateProductCommand UpdateProductCommand(Guid id, ProductEditViewModel vm)
    {
        var dto = _mapper.Map<CleanCut.Application.DTOs.ProductInfo>(vm);
        return new UpdateProductCommand(id, dto.Name, dto.Description, dto.Price);
    }

    public DeleteProductCommand DeleteProductCommand(Guid id)
    {
        throw new NotImplementedException();
    }
}
