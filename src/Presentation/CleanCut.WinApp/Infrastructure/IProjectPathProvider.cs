using System;

namespace CleanCut.WinApp.Infrastructure;

/// <summary>
/// Abstraction for resolving the project or repository base path used by the WinApp.
/// Implementations should perform best-effort resolution and return null when resolution fails.
/// </summary>
public interface IProjectPathProvider
{
    /// <summary>
    /// Resolve a project or repository base path suitable for placing runtime artifacts such as logs.
    /// Returns null if no reasonable base path can be determined.
    /// </summary>
    string? GetProjectBasePath();
}
