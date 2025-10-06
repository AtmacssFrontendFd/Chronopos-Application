@echo off
REM ChronoPos Desktop - Industrial Deployment Builder
REM This batch file creates a professional installer package

title ChronoPos Desktop - Industrial Deployment Builder

echo =============================================
echo   ChronoPos Desktop - Deployment Builder
echo =============================================
echo.

REM Check if PowerShell is available
where powershell >nul 2>nul
if errorlevel 1 (
    echo ERROR: PowerShell not found. This script requires PowerShell.
    echo Please install PowerShell or use Windows 10/11.
    pause
    exit /b 1
)

echo Starting deployment build process...
echo.

REM Run the PowerShell build script
powershell.exe -ExecutionPolicy Bypass -File "Deployment\Build-Installer.ps1"

REM Check if the build was successful
if errorlevel 1 (
    echo.
    echo BUILD FAILED! Please check the error messages above.
    echo.
    pause
    exit /b 1
)

echo.
echo =============================================
echo BUILD COMPLETED SUCCESSFULLY!
echo =============================================
echo.
echo Your installer packages are ready:
echo.
echo 1. Professional MSI Installer:
echo    Deployment\Installer\ChronoPosSetup.msi
echo.
echo 2. Portable ZIP Package:
echo    Deployment\Installer\ChronoPos-Portable.zip
echo.
echo 3. Installation Guide:
echo    Deployment\Installer\INSTALLATION_GUIDE.txt
echo.
echo You can now distribute these files to end users!
echo.

REM Ask if user wants to open the installer folder
set /p choice="Open installer folder? (Y/N): "
if /i "%choice%"=="Y" (
    explorer "Deployment\Installer"
)

echo.
echo Thank you for using ChronoPos Desktop!
pause