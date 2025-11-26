<#
PowerShell helper to apply EF Core migrations to your development database (CleanCut_db_Dev).
Edit the $connection variable below if you use SQL authentication.
Usage:
  .\UpdateDatabase_Dev.ps1
#>

# TODO: Update server name or credentials if needed
$connection = 'Server=YOUR_SQL_SERVER;Database=CleanCut_db_Dev;Trusted_Connection=True;MultipleActiveResultSets=true'

Write-Host "Using dev DB connection:`n$connection"

# Export env var for this process
$env:CLEANCUT_CONNECTION = $connection

# Run EF Core database update
dotnet ef database update `
  --project src\Infrastructure\CleanCut.Infrastructure.Data\CleanCut.Infrastructure.Data.csproj `
  --startup-project src\Presentation\CleanCut.API\CleanCut.API.csproj `
  --context CleanCutDbContext

if ($LASTEXITCODE -eq 0) {
    Write-Host "EF database update completed successfully against CleanCut_db_Dev."
} else {
    Write-Error "EF database update failed with exit code $LASTEXITCODE"
}

# Optional: unset env var in this session
# Remove-Item Env:CLEANCUT_CONNECTION
