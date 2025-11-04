#!/usr/bin/env pwsh

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "CleanCut Product Export Host - Clean Architecture" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "This script tests the new Clean Architecture background service." -ForegroundColor Yellow
Write-Host ""

Write-Host "Step 1: Ensure IdentityServer is running" -ForegroundColor Green
Write-Host "----------------------------------------" -ForegroundColor Green
Write-Host "Start the IdentityServer project first:"
Write-Host "  cd src/Infrastructure/CleanCut.Infranstructure.Identity" -ForegroundColor White
Write-Host "  dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "Press any key when IdentityServer is running on https://localhost:5001..." -ForegroundColor Yellow
$null = Read-Host

Write-Host ""
Write-Host "Step 2: Ensure CleanCut.API is running" -ForegroundColor Green
Write-Host "--------------------------------------" -ForegroundColor Green
Write-Host "Start the API project:"
Write-Host "  cd src/Presentation/CleanCut.API" -ForegroundColor White
Write-Host "  dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "Press any key when API is running on https://localhost:7142..." -ForegroundColor Yellow
$null = Read-Host

Write-Host ""
Write-Host "Step 3: Running the Product Export Host" -ForegroundColor Green
Write-Host "--------------------------------------" -ForegroundColor Green
Write-Host "Starting the new Clean Architecture background service..." -ForegroundColor Yellow
Write-Host ""

Set-Location "src/Applications/CleanCut.ProductExportHost"
dotnet run

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Clean Architecture Test Complete" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Architecture Summary:" -ForegroundColor Green
Write-Host "? Infrastructure Layer: Authentication, API, CSV services" -ForegroundColor Green
Write-Host "? Application Layer: ProductExportHost executable" -ForegroundColor Green
Write-Host "? Clean separation of concerns" -ForegroundColor Green
Write-Host "? Dependency inversion through interfaces" -ForegroundColor Green
Write-Host ""
Write-Host "Check the 'exports' folder for CSV files." -ForegroundColor Yellow
Write-Host ""
Read-Host "Press Enter to exit"