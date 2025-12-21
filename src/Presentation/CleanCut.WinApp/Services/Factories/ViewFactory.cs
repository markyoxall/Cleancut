using Microsoft.Extensions.DependencyInjection;

namespace CleanCut.WinApp.Services.Factories;

/// <summary>
/// Concrete factory that resolves views from the DI container.
/// 
/// SOLID notes:
/// - SRP: single responsibility to create view instances.
/// - DIP: depends on IServiceProvider abstraction for resolution.
/// - ISP: implements the small IViewFactory interface.
/// </summary>
public class ViewFactory<TView> : IViewFactory<TView>
{
    private readonly IServiceProvider _serviceProvider;

    public ViewFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Create an instance of the view by resolving it from the DI container.
    /// </summary>
    public TView Create()
    {
        return _serviceProvider.GetRequiredService<TView>();
    }
}
