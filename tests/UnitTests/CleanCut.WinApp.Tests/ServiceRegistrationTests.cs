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
        var nav = sp.GetService<CleanCut.WinApp.Services.Navigation.INavigationService>();
        var customerViewFactory = sp.GetService<CleanCut.WinApp.Services.Factories.IViewFactory<CleanCut.WinApp.Views.Customers.ICustomerListView>>();

        Assert.NotNull(mainForm);
        Assert.NotNull(nav);
        // View factory for customer list may or may not be registered; ensure DI doesn't throw
    }
}
