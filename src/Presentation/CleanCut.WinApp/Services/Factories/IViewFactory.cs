namespace CleanCut.WinApp.Services.Factories;

/// <summary>
/// Factory for creating view instances resolved from DI
/// </summary>
public interface IViewFactory<TView>
{
    TView Create();
}
