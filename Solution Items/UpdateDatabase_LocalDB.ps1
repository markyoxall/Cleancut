<#
PowerShell helper to apply EF Core migrations to LocalDB (development fallback).
Usage:
  .\UpdateDatabase_LocalDB.ps1
This sets the CLEANCUT_CONNECTION env var for the current process and runs dotnet ef database update.
#>

# Connection used by the design-time factory when no env var is set
$connection = 'Server=(localdb)\\MSSQLLocalDB;Database=CleanCut;Trusted_Connection=True;MultipleActiveResultSets=true'

Write-Host "Using LocalDB connection:`n$connection"

# Export env var for this process
$env:CLEANCUT_CONNECTION = $connection

# Run EF Core database update
dotnet ef database update `
  --project src\Infrastructure\CleanCut.Infrastructure.Data\CleanCut.Infrastructure.Data.csproj `
  --startup-project src\Presentation\CleanCut.API\CleanCut.API.csproj `
  --context CleanCutDbContext

if ($LASTEXITCODE -eq 0) {
    Write-Host "EF database update completed successfully against LocalDB."
} else {
    Write-Error "EF database update failed with exit code $LASTEXITCODE"
}

# Optional: unset env var in this session
# Remove-Item Env:CLEANCUT_CONNECTION
