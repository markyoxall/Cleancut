using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using CleanCut.WinApp.Infrastructure;

// Build the service provider (this uses ValidateOnBuild configured in ServiceConfiguration)
IServiceProvider sp;
try
{
    sp = ServiceConfiguration.ConfigureServices();
    Console.WriteLine("Service provider built successfully.");
}
catch (Exception ex)
{
    Console.WriteLine("Failed to build service provider: " + ex);
    return;
}

// Find ServiceConfiguration source to parse registrations
var svcConfigPath = Path.Combine("src", "Presentation", "CleanCut.WinApp", "Infrastructure", "ServiceConfiguration.cs");
if (!File.Exists(svcConfigPath))
{
    Console.WriteLine($"Could not find {svcConfigPath} to parse registrations. Doing nothing.");
    return;
}

var src = File.ReadAllText(svcConfigPath);
var candidates = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);

// Match AddSingleton<...> generic usages
foreach (Match m in Regex.Matches(src, @"AddSingleton\s*<\s*([^>]+?)\s*>"))
{
    var inner = m.Groups[1].Value;
    // Split by comma not inside <> (simple split)
    var parts = inner.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p));
    foreach (var p in parts) candidates.Add(p);
}

// Match AddSingleton(typeof(X) ... ) usages
foreach (Match m in Regex.Matches(src, @"AddSingleton\s*\(\s*typeof\s*\(\s*([^\)]+)\s*\)"))
{
    var tn = m.Groups[1].Value.Trim();
    if (!string.IsNullOrEmpty(tn)) candidates.Add(tn);
}

// Also match AddSingleton(typeof(X), typeof(Y)) - capture second type
foreach (Match m in Regex.Matches(src, @"AddSingleton\s*\(\s*typeof\s*\(\s*[^\)]+\s*\)\s*,\s*typeof\s*\(\s*([^\)]+)\s*\)"))
{
    var tn = m.Groups[1].Value.Trim();
    if (!string.IsNullOrEmpty(tn)) candidates.Add(tn);
}

Console.WriteLine($"Discovered {candidates.Count} candidate type names from ServiceConfiguration.AddSingleton calls:\n");
foreach (var c in candidates.OrderBy(x => x)) Console.WriteLine(" - " + c);

// Prepare assemblies to search types in: loaded + project bins
var loaded = AppDomain.CurrentDomain.GetAssemblies().ToList();
var solutionRoot = Directory.GetCurrentDirectory();
var srcRoot = Path.Combine(solutionRoot, "src");
if (Directory.Exists(srcRoot))
{
    var binDirs = Directory.GetDirectories(srcRoot, "bin", SearchOption.AllDirectories)
        .Where(d => d.IndexOf("Debug", StringComparison.OrdinalIgnoreCase) >= 0 || d.IndexOf("Release", StringComparison.OrdinalIgnoreCase) >= 0)
        .Distinct();
    foreach (var bin in binDirs)
    {
        try
        {
            foreach (var dll in Directory.GetFiles(bin, "*.dll"))
            {
                try
                {
                    var an = AssemblyName.GetAssemblyName(dll);
                    if (!loaded.Any(a => a.FullName == an.FullName))
                    {
                        var asm = Assembly.LoadFrom(dll);
                        loaded.Add(asm);
                    }
                }
                catch { }
            }
        }
        catch { }
    }
}

Console.WriteLine();

// Try resolve each candidate and attempt to GetService for the resolved Type
foreach (var name in candidates.OrderBy(x => x))
{
    Type? type = null;
    // Try direct Type.GetType first
    try { type = Type.GetType(name, throwOnError: false, ignoreCase: false); } catch { }

    if (type == null)
    {
        // Try to find by full name or by name in loaded assemblies
        foreach (var asm in loaded)
        {
            try
            {
                var t = asm.GetTypes().FirstOrDefault(tt => tt.FullName == name || tt.Name == name || tt.FullName?.EndsWith(", " + name) == true);
                if (t != null) { type = t; break; }
            }
            catch { }
        }
    }

    if (type == null)
    {
        Console.WriteLine($"Type not found for '{name}'");
        continue;
    }

    Console.WriteLine($"\nResolved type: {type.FullName}");

    // Print public constructors
    var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
    if (ctors.Length == 0) Console.WriteLine("  (no public constructors)");
    else
    {
        foreach (var ctor in ctors)
        {
            var ps = ctor.GetParameters();
            Console.WriteLine($"  ctor({string.Join(", ", ps.Select(p => p.ParameterType.FullName + " " + p.Name))})");
        }
    }

    // Attempt to resolve from service provider
    try
    {
        var resolved = sp.GetService(type);
        if (resolved != null)
        {
            Console.WriteLine($"  -> Successfully resolved instance of {type.FullName} from service provider. (Type: {resolved.GetType().FullName})");
        }
        else
        {
            Console.WriteLine($"  -> sp.GetService returned null for {type.FullName}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  -> Exception while resolving {type.FullName}: {ex.GetType().Name}: {ex.Message}");
    }
}

Console.WriteLine("\nAnalysis complete.");
