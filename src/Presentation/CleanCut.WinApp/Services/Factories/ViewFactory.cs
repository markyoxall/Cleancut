using Microsoft.Extensions.DependencyInjection;

namespace CleanCut.WinApp.Services.Factories;

internal class ViewFactory<TView> : IViewFactory<TView>
{
    private readonly IServiceProvider _serviceProvider;

    public ViewFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public TView Create()
    {
        return _serviceProvider.GetRequiredService<TView>();
    }
}
