#!/usr/bin/env pwsh

Write-Host "================================" -ForegroundColor Cyan
Write-Host "CleanCut Background Service Test" -ForegroundColor Cyan  
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "This script will help you test the background service step by step." -ForegroundColor Yellow
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
Write-Host "Step 3: Running the Background Service" -ForegroundColor Green
Write-Host "-------------------------------------" -ForegroundColor Green
Write-Host "Starting the background service..." -ForegroundColor Yellow
Write-Host ""

Set-Location $PSScriptRoot
dotnet run

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Background Service Test Complete" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Check the 'exports' folder for CSV files." -ForegroundColor Green
Write-Host "Check the console output for authentication and API call logs." -ForegroundColor Green
Write-Host ""
Read-Host "Press Enter to exit"