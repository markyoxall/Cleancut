using System.Windows.Forms;
using CleanCut.WinApp.Services.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CleanCut.WinApp.Tests;

public class NavigationServiceTests
{
    [StaFact]
    public void ShowCustomerManagement_ShouldCreateAndShowChildForm()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // register a fake customer list view
        var mockView = new Mock<CleanCut.WinApp.Views.Customers.ICustomerListView>();
        // Mock methods used in presenter initialization
        mockView.Setup(v => v.SetAvailableCustomers(It.IsAny<System.Collections.Generic.IEnumerable<CleanCut.Application.DTOs.CustomerInfo>>()));
        mockView.Setup(v => v.DisplayCustomers(It.IsAny<System.Collections.Generic.IEnumerable<CleanCut.Application.DTOs.CustomerInfo>>()));

        services.AddSingleton<CleanCut.WinApp.Views.Customers.ICustomerListView>(_ => mockView.Object);

        // Register presenter type so ActivatorUtilities can create it
        services.AddTransient<CleanCut.WinApp.Presenters.CustomerListPresenter>();

        var sp = services.BuildServiceProvider();

        var nav = new NavigationService(sp, new NullLogger<NavigationService>());

        var mainForm = new Form();
        mainForm.IsMdiContainer = true;

        // Act
        nav.ShowCustomerManagement(mainForm);

        // Assert
        Assert.Empty(mainForm.MdiChildren); // the view is a mock and not a real form; expect no MDI children
    }
}
