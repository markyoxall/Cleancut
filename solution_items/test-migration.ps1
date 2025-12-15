#!/usr/bin/env pwsh

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Background Service Migration Test" -ForegroundColor Cyan  
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "This script helps you test both the OLD and NEW background services" -ForegroundColor Yellow
Write-Host "to ensure the new Clean Architecture version works correctly." -ForegroundColor Yellow
Write-Host ""

Write-Host "?? OLD SERVICE: src/Services/CleanCut.BackgroundService" -ForegroundColor Red
Write-Host "?? NEW SERVICE: src/Applications/CleanCut.ProductExportHost" -ForegroundColor Green
Write-Host ""

# Function to test service prerequisites
function Test-Prerequisites {
    Write-Host "?? Checking Prerequisites..." -ForegroundColor Yellow
    
    # Test IdentityServer
    try {
        $identityResponse = Invoke-WebRequest -Uri "https://localhost:5001/.well-known/openid_configuration" -SkipCertificateCheck -TimeoutSec 5
        Write-Host "? IdentityServer is running (port 5001)" -ForegroundColor Green
    }
    catch {
Write-Host "? IdentityServer is NOT running (port 5001)" -ForegroundColor Red
    Write-Host "   Start with: dotnet run --project src/Infrastructure/CleanCut.Infranstructure.Identity" -ForegroundColor White
        return $false
    }
    
    # Test API
    try {
 $apiResponse = Invoke-WebRequest -Uri "https://localhost:7142/swagger" -SkipCertificateCheck -TimeoutSec 5
        Write-Host "? CleanCut.API is running (port 7142)" -ForegroundColor Green
    }
    catch {
        Write-Host "? CleanCut.API is NOT running (port 7142)" -ForegroundColor Red
        Write-Host "   Start with: dotnet run --project src/Presentation/CleanCut.API" -ForegroundColor White
        return $false
    }
    
    return $true
}

# Function to build projects
function Build-Projects {
    Write-Host "?? Building Projects..." -ForegroundColor Yellow
    
    # Build old service
    Write-Host "Building OLD service..." -ForegroundColor Red
    $oldBuild = dotnet build "src/Services/CleanCut.BackgroundService" --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
     Write-Host "? OLD service build failed!" -ForegroundColor Red
        return $false
    }
    Write-Host "? OLD service built successfully" -ForegroundColor Green
    
    # Build new infrastructure
    Write-Host "Building NEW infrastructure..." -ForegroundColor Blue
    $infraBuild = dotnet build "src/Infrastructure/CleanCut.Infrastructure.BackgroundServices" --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? NEW infrastructure build failed!" -ForegroundColor Red
        return $false
    }
    Write-Host "? NEW infrastructure built successfully" -ForegroundColor Green
    
    # Build new host
    Write-Host "Building NEW host..." -ForegroundColor Green
    $hostBuild = dotnet build "src/Applications/CleanCut.ProductExportHost" --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? NEW host build failed!" -ForegroundColor Red
        return $false
    }
    Write-Host "? NEW host built successfully" -ForegroundColor Green
    
    return $true
}

# Main menu
do {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "Choose what to test:" -ForegroundColor White
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "1. ?? Check Prerequisites (IdentityServer + API)" -ForegroundColor White
    Write-Host "2. ?? Build Both Services" -ForegroundColor White
    Write-Host "3. ?? Test OLD Service (Original)" -ForegroundColor Red
    Write-Host "4. ?? Test NEW Service (Clean Architecture)" -ForegroundColor Green
    Write-Host "5. ?? Compare Configurations" -ForegroundColor Yellow
    Write-Host "6. ?? Exit" -ForegroundColor White
    Write-Host ""
    
    $choice = Read-Host "Enter your choice (1-6)"
    
    switch ($choice) {
      "1" {
 if (Test-Prerequisites) {
            Write-Host "? All prerequisites are running!" -ForegroundColor Green
            } else {
                Write-Host "? Please start the required services first" -ForegroundColor Red
          }
 }
        
        "2" {
      if (Build-Projects) {
          Write-Host "? All projects built successfully!" -ForegroundColor Green
  } else {
                Write-Host "? Build failed. Check the errors above." -ForegroundColor Red
 }
        }
     
    "3" {
            Write-Host ""
  Write-Host "?? Starting OLD Background Service..." -ForegroundColor Red
       Write-Host "Location: src/Services/CleanCut.BackgroundService" -ForegroundColor White
   Write-Host "Press Ctrl+C to stop the service" -ForegroundColor Yellow
   Write-Host ""
   
     Push-Location "src/Services/CleanCut.BackgroundService"
       try {
  dotnet run
            }
     finally {
    Pop-Location
   }
        }
  
        "4" {
  Write-Host ""
      Write-Host "?? Starting NEW Background Service..." -ForegroundColor Green
 Write-Host "Location: src/Applications/CleanCut.ProductExportHost" -ForegroundColor White
            Write-Host "Architecture: Clean Architecture with Infrastructure Layer" -ForegroundColor Blue
       Write-Host "Press Ctrl+C to stop the service" -ForegroundColor Yellow
   Write-Host ""
     
       Push-Location "src/Applications/CleanCut.ProductExportHost"
            try {
        dotnet run
      }
     finally {
       Pop-Location
            }
        }
        
        "5" {
            Write-Host ""
      Write-Host "?? Configuration Comparison:" -ForegroundColor Yellow
  Write-Host ""
       Write-Host "OLD CONFIG (Nested):" -ForegroundColor Red
    Write-Host "  BackgroundService.IdentityServer.Authority" -ForegroundColor White
    Write-Host "  BackgroundService.Api.BaseUrl" -ForegroundColor White
            Write-Host "  BackgroundService.Csv.OutputDirectory" -ForegroundColor White
            Write-Host ""
  Write-Host "NEW CONFIG (Flat):" -ForegroundColor Green
   Write-Host "  Authentication.Authority" -ForegroundColor White
   Write-Host "  Api.BaseUrl" -ForegroundColor White
     Write-Host "  Csv.OutputDirectory" -ForegroundColor White
            Write-Host ""
   Write-Host "? Both use the same values, just different structure" -ForegroundColor Green
      }
     
      "6" {
       Write-Host "Goodbye! ??" -ForegroundColor Cyan
            break
    }
        
        default {
            Write-Host "Invalid choice. Please enter 1-6." -ForegroundColor Red
        }
    }
    
} while ($choice -ne "6")

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Testing Summary" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "?? Both services should create CSV files in 'exports' folder" -ForegroundColor White
Write-Host "?? Compare the output to ensure identical functionality" -ForegroundColor White
Write-Host "?? Keep both until you're 100% confident in the new one" -ForegroundColor White
Write-Host ""