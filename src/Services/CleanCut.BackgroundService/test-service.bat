@echo off
echo ================================
echo CleanCut Background Service Test
echo ================================
echo.

echo This script will help you test the background service step by step.
echo.

echo Step 1: Ensure IdentityServer is running
echo ----------------------------------------
echo Start the IdentityServer project first:
echo   cd src\Infrastructure\CleanCut.Infranstructure.Identity
echo   dotnet run
echo.
echo Press any key when IdentityServer is running on https://localhost:5001...
pause

echo.
echo Step 2: Ensure CleanCut.API is running  
echo --------------------------------------
echo Start the API project:
echo   cd src\Presentation\CleanCut.API
echo   dotnet run
echo.
echo Press any key when API is running on https://localhost:7142...
pause

echo.
echo Step 3: Running the Background Service
echo -------------------------------------
echo Starting the background service...
echo.

cd /d "%~dp0"
dotnet run

echo.
echo ================================
echo Background Service Test Complete
echo ================================
echo.
echo Check the 'exports' folder for CSV files.
echo Check the console output for authentication and API call logs.
echo.
pause