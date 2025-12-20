namespace CleanCut.WinApp;

public record ManagementModuleConfig(
    string Id,
    string? Title,
    string? ViewType,
    string? PresenterType,
    bool Enabled
);
