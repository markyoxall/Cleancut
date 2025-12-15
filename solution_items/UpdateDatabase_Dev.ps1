<#
PowerShell helper to apply EF Core migrations to your development database (CleanCut_db_Dev).
Edit the $connection variable below if you use SQL authentication.
Usage:
  .\UpdateDatabase_Dev.ps1
#>

# Ensure we run dotnet ef from the repository root so relative project paths resolve correctly.
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptDir '..')
Write-Host "Changing working directory to repo root: $repoRoot"
Push-Location $repoRoot

try {
    # TODO: Update server name or credentials if needed
    $connection = 'Server=(localdb)\\mssqllocaldb;Database=CleanCutDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true'

    Write-Host "Using dev DB connection:`n$connection"

    # Export env var for this process
    $env:CLEANCUT_CONNECTION = $connection

    # Ensure dotnet-ef tool is restored to match SDK/runtime
    Write-Host "Restoring local dotnet tools..."
    dotnet tool restore

    # Restore NuGet packages for projects involved
    Write-Host "Restoring NuGet packages..."
    dotnet restore src\Infrastructure\CleanCut.Infrastructure.Data\CleanCut.Infrastructure.Data.csproj
    dotnet restore src\Presentation\CleanCut.API\CleanCut.API.csproj

    # Build startup project so design-time services can be resolved
    Write-Host "Building startup project (CleanCut.API)..."
    dotnet build src\Presentation\CleanCut.API\CleanCut.API.csproj --configuration Debug

    # Run EF Core database update with verbose output for diagnostics
    Write-Host "Running EF Core database update (verbose)..."
    $efArgs = @("database", "update", "--project", "src\Infrastructure\CleanCut.Infrastructure.Data\CleanCut.Infrastructure.Data.csproj", "--startup-project", "src\Presentation\CleanCut.API\CleanCut.API.csproj", "--context", "CleanCutDbContext", "-v")

    dotnet ef @efArgs

    if ($LASTEXITCODE -eq 0) {
        Write-Host "EF database update completed successfully against CleanCut_db_Dev."
    } else {
        Write-Error "EF database update failed with exit code $LASTEXITCODE"
        Write-Host "Tips:"
        Write-Host " - Verify the connection string in this script is correct and the DB server is reachable."
        Write-Host " - Confirm the startup project builds (see build output above)."
        Write-Host " - If you see 'Unable to retrieve project metadata', ensure the project files are SDK-style (first XML node: <Project Sdk=\"Microsoft.NET.Sdk\">)."
    }
}
finally {
    Pop-Location
}

# Optional: unset env var in this session
# Remove-Item Env:CLEANCUT_CONNECTION
