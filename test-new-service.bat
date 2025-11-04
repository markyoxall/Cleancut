@echo off
echo ================================
echo Testing New Clean Architecture Background Service
echo ================================
echo.

echo This will test ONLY the new Clean Architecture version.
echo The old service in src/Services will remain untouched.
echo.

echo ========================================
echo Step 1: Building the new infrastructure
echo ========================================
cd /d "D:\.NET STUDY\CleanCut"
echo Building Infrastructure.BackgroundServices...
dotnet build "src/Infrastructure/CleanCut.Infrastructure.BackgroundServices"
if %errorlevel% neq 0 (
    echo ERROR: Infrastructure build failed!
    pause
    exit /b 1
)

echo Building ProductExportHost...
dotnet build "src/Applications/CleanCut.ProductExportHost"
if %errorlevel% neq 0 (
    echo ERROR: ProductExportHost build failed!
    pause
    exit /b 1
)

echo.
echo ? Both projects built successfully!
echo.

echo ========================================
echo Step 2: Configuration Check
echo ========================================
echo Checking if appsettings.json exists...
if exist "src/Applications/CleanCut.ProductExportHost/appsettings.json" (
    echo ? Configuration file found
) else (
    echo ? Configuration file missing!
    pause
    exit /b 1
)

echo.
echo ========================================
echo Step 3: Prerequisites Check
echo ========================================
echo.
echo Before running the new service, ensure:
echo.
echo 1. IdentityServer is running on https://localhost:5001
echo    Command: dotnet run --project "src/Infrastructure/CleanCut.Infranstructure.Identity"
echo.
echo 2. CleanCut.API is running on https://localhost:7142  
echo    Command: dotnet run --project "src/Presentation/CleanCut.API"
echo.
echo Press any key when both services are running...
pause

echo.
echo ========================================
echo Step 4: Running New Clean Architecture Service
echo ========================================
echo.
echo Starting CleanCut.ProductExportHost...
echo (Press Ctrl+C to stop)
echo.

cd "src/Applications/CleanCut.ProductExportHost"
dotnet run

echo.
echo ========================================
echo Test Complete
echo ========================================
echo.
echo If the service ran successfully, check the 'exports' folder
echo for CSV files with product data.
echo.
echo The old service remains available at:
echo src/Services/CleanCut.BackgroundService
echo.
pause