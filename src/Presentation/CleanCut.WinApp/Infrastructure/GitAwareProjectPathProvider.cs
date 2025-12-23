using System;
using System.IO;

namespace CleanCut.WinApp.Infrastructure;

/// <summary>
/// Project path resolver that looks for repository root (.git), solution file or the project folder name.
/// This encapsulates the previous ResolveProjectBasePath logic in a testable, injectable type.
/// </summary>
public class GitAwareProjectPathProvider : IProjectPathProvider
{
    private readonly string _projectFolderName = "CleanCut.WinApp";

    /// <summary>
    /// Resolve a project or repository base path suitable for placing runtime artifacts such as logs.
    /// Returns null if no reasonable base path can be determined.
    /// </summary>
    public string? GetProjectBasePath()
    {
        try
        {
            var startDir = new DirectoryInfo(AppContext.BaseDirectory ?? Directory.GetCurrentDirectory());
            DirectoryInfo? dir = startDir;
            while (dir != null)
            {
                // prefer repository root detection via .git
                if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                    return dir.FullName;

                var sln = dir.GetFiles("*.sln");
                if (sln.Length > 0)
                    return dir.FullName;

                // fallback: look for this project folder name
                if (string.Equals(dir.Name, _projectFolderName, StringComparison.OrdinalIgnoreCase))
                    return dir.FullName;

                // look for any csproj to treat as project root
                var csproj = dir.GetFiles("*.csproj");
                if (csproj.Length > 0)
                    return dir.FullName;

                dir = dir.Parent;
            }
        }
        catch
        {
            // swallow; resolution is best-effort only
        }

        return null;
    }
}
