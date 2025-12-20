using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CleanCut.WinApp.Tests;

public class ServiceRegistrationTests
{
    [Fact]
    public void ServiceConfiguration_ShouldResolveMainServices()
    {
        var sp = CleanCut.WinApp.Infrastructure.ServiceConfiguration.ConfigureServices();

        var mainForm = sp.GetService<CleanCut.WinApp.MainForm>();
        var managementLoader = sp.GetService<CleanCut.WinApp.Services.Management.IManagementLoader>();
        var customerViewFactory = sp.GetService<CleanCut.WinApp.Services.Factories.IViewFactory<CleanCut.WinApp.Views.Customers.ICustomerListView>>();
        var commandFactory = sp.GetService<CleanCut.WinApp.Services.ICommandFactory>();

        Assert.NotNull(mainForm);
        Assert.NotNull(managementLoader);
        Assert.NotNull(commandFactory);
        // View factory for customer list may or may not be registered; ensure DI doesn't throw
    }
}
