#!/usr/bin/env pwsh

Write-Host "?? Quick Functionality Verification" -ForegroundColor Cyan
Write-Host ""

# Test 1: Build verification
Write-Host "Test 1: Build Verification" -ForegroundColor Yellow
Write-Host "--------------------------" -ForegroundColor Yellow

Write-Host "Building OLD service..." -NoNewline
$oldResult = dotnet build "src/Services/CleanCut.BackgroundService" --verbosity quiet 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host " ?" -ForegroundColor Green
} else {
    Write-Host " ?" -ForegroundColor Red
}

Write-Host "Building NEW infrastructure..." -NoNewline
$infraResult = dotnet build "src/Infrastructure/CleanCut.Infrastructure.BackgroundServices" --verbosity quiet 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host " ?" -ForegroundColor Green
} else {
    Write-Host " ?" -ForegroundColor Red
}

Write-Host "Building NEW host..." -NoNewline
$hostResult = dotnet build "src/Applications/CleanCut.ProductExportHost" --verbosity quiet 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host " ?" -ForegroundColor Green
} else {
    Write-Host " ?" -ForegroundColor Red
}

Write-Host ""

# Test 2: Configuration verification
Write-Host "Test 2: Configuration Verification" -ForegroundColor Yellow
Write-Host "-----------------------------------" -ForegroundColor Yellow

$oldConfig = "src/Services/CleanCut.BackgroundService/appsettings.json"
$newConfig = "src/Applications/CleanCut.ProductExportHost/appsettings.json"

Write-Host "OLD config exists..." -NoNewline
if (Test-Path $oldConfig) {
 Write-Host " ?" -ForegroundColor Green
} else {
    Write-Host " ?" -ForegroundColor Red
}

Write-Host "NEW config exists..." -NoNewline
if (Test-Path $newConfig) {
    Write-Host " ?" -ForegroundColor Green
} else {
    Write-Host " ?" -ForegroundColor Red
}

Write-Host ""

# Test 3: Dependencies verification
Write-Host "Test 3: Dependencies Verification" -ForegroundColor Yellow
Write-Host "----------------------------------" -ForegroundColor Yellow

$oldCsproj = Get-Content "src/Services/CleanCut.BackgroundService/CleanCut.BackgroundService.csproj"
$newCsproj = Get-Content "src/Infrastructure/CleanCut.Infrastructure.BackgroundServices/CleanCut.Infrastructure.BackgroundServices.csproj"

Write-Host "OLD service has Duende.IdentityModel..." -NoNewline
if ($oldCsproj -match "Duende.IdentityModel") {
    Write-Host " ?" -ForegroundColor Green
} else {
    Write-Host " ?" -ForegroundColor Red
}

Write-Host "NEW service has Duende.IdentityModel..." -NoNewline
if ($newCsproj -match "Duende.IdentityModel") {
 Write-Host " ?" -ForegroundColor Green
} else {
    Write-Host " ?" -ForegroundColor Red
}

Write-Host "OLD service has CsvHelper..." -NoNewline
if ($oldCsproj -match "CsvHelper") {
    Write-Host " ?" -ForegroundColor Green
} else {
    Write-Host " ?" -ForegroundColor Red
}

Write-Host "NEW service has CsvHelper..." -NoNewline
if ($newCsproj -match "CsvHelper") {
    Write-Host " ?" -ForegroundColor Green
} else {
    Write-Host " ?" -ForegroundColor Red
}

Write-Host ""

# Summary
Write-Host "?? Verification Summary" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan
Write-Host "Both services should be ready for testing!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Start IdentityServer: dotnet run --project src/Infrastructure/CleanCut.Infranstructure.Identity" -ForegroundColor White
Write-Host "2. Start API: dotnet run --project src/Presentation/CleanCut.API" -ForegroundColor White
Write-Host "3. Test OLD: dotnet run --project src/Services/CleanCut.BackgroundService" -ForegroundColor White
Write-Host "4. Test NEW: dotnet run --project src/Applications/CleanCut.ProductExportHost" -ForegroundColor White
Write-Host ""