# ChronoPos Desktop - Simple Build Script
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

Write-Host "Starting ChronoPos Desktop Build Process..." -ForegroundColor Cyan
Write-Host ""

# Check .NET SDK
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
Write-Host "Using .NET SDK Version: $dotnetVersion" -ForegroundColor Green
Write-Host ""

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean ChronoPos.sln -c $Configuration -v minimal
Write-Host "Clean completed" -ForegroundColor Green
Write-Host ""

# Create output directories
$OutputPath = "Deployment\Output"
$InstallerPath = "Deployment\Installer"

if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
}
if (Test-Path $InstallerPath) {
    Remove-Item $InstallerPath -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
New-Item -ItemType Directory -Path $InstallerPath -Force | Out-Null

Write-Host "Output directories created" -ForegroundColor Green
Write-Host ""

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore ChronoPos.sln
Write-Host "Packages restored" -ForegroundColor Green
Write-Host ""

# Build solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build ChronoPos.sln -c $Configuration --no-restore -v minimal
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit 1
}
Write-Host "Build completed successfully" -ForegroundColor Green
Write-Host ""

# Publish application
Write-Host "Publishing self-contained application..." -ForegroundColor Yellow
dotnet publish src\ChronoPos.Desktop -c $Configuration -r $Runtime --self-contained true --no-restore --no-build -o $OutputPath -v minimal

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed"
    exit 1
}

Write-Host "Application published successfully" -ForegroundColor Green

# Get file information
$mainExe = Join-Path $OutputPath "ChronoPos.Desktop.exe"
if (Test-Path $mainExe) {
    $fileInfo = Get-Item $mainExe
    $fileSize = [math]::Round($fileInfo.Length / 1MB, 2)
    Write-Host "Main executable: $($fileInfo.Name) - Size: $fileSize MB" -ForegroundColor Green
}

$publishedFiles = Get-ChildItem $OutputPath -Recurse -File
$totalSize = [math]::Round(($publishedFiles | Measure-Object -Property Length -Sum).Sum / 1MB, 2)
Write-Host "Total files: $($publishedFiles.Count) - Total size: $totalSize MB" -ForegroundColor Green
Write-Host ""

# Create portable package
Write-Host "Creating portable ZIP package..." -ForegroundColor Yellow
Add-Type -AssemblyName System.IO.Compression.FileSystem

$portableZip = Join-Path $InstallerPath "ChronoPos-Portable.zip"
[System.IO.Compression.ZipFile]::CreateFromDirectory($OutputPath, $portableZip)

$zipInfo = Get-Item $portableZip
$zipSize = [math]::Round($zipInfo.Length / 1MB, 2)
Write-Host "Portable package created: $($zipInfo.Name) - Size: $zipSize MB" -ForegroundColor Green
Write-Host ""

# Create documentation
Write-Host "Creating documentation..." -ForegroundColor Yellow

$installGuide = Join-Path $InstallerPath "INSTALLATION_GUIDE.txt"
$guideContent = @"
ChronoPos Desktop POS System - Installation Guide
===============================================

SYSTEM REQUIREMENTS:
- Windows 10 or Windows 11 (64-bit)
- 2 GB RAM minimum
- 500 MB free disk space

INSTALLATION:
1. Extract ChronoPos-Portable.zip to a folder
2. Run ChronoPos.Desktop.exe
3. First run will create database automatically

DEFAULT LOGIN:
Username: admin
Password: admin123

SUPPORT:
Contact your system administrator for help.
"@

$guideContent | Out-File -FilePath $installGuide -Encoding UTF8

$versionFile = Join-Path $InstallerPath "VERSION.txt"
$buildDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$versionContent = @"
ChronoPos Desktop POS System
Version: 1.0.0.0
Build Date: $buildDate
Runtime: $Runtime
Configuration: $Configuration
.NET Version: $dotnetVersion
Files: $($publishedFiles.Count)
Size: $totalSize MB
"@

$versionContent | Out-File -FilePath $versionFile -Encoding UTF8

Write-Host "Documentation created" -ForegroundColor Green
Write-Host ""

# Final summary
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "DEPLOYMENT COMPLETED SUCCESSFULLY" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "FILES CREATED:" -ForegroundColor Green
Write-Host "Portable Package: $InstallerPath\ChronoPos-Portable.zip" -ForegroundColor White
Write-Host "Installation Guide: $InstallerPath\INSTALLATION_GUIDE.txt" -ForegroundColor White
Write-Host "Version Info: $InstallerPath\VERSION.txt" -ForegroundColor White
Write-Host "Application Files: $OutputPath\" -ForegroundColor White
Write-Host ""
Write-Host "READY FOR DISTRIBUTION!" -ForegroundColor Green
Write-Host "Users can download and extract the ZIP file to run the application." -ForegroundColor Yellow