# ChronoPos Portable Build Script
# Creates a self-contained, shareable application package

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "ChronoPos Portable Build" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$ProjectPath = "src\ChronoPos.Desktop\ChronoPos.Desktop.csproj"
$OutputPath = "Deployment\ChronoPos-Portable"
$ZipPath = "Deployment\ChronoPos-Portable.zip"

# Step 1: Clean old builds
Write-Host "[1/5] Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
}
if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}
Write-Host "  OK - Cleaned" -ForegroundColor Green
Write-Host ""

# Step 2: Restore packages
Write-Host "[2/5] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore ChronoPos.sln --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Error "Restore failed!"
    exit 1
}
Write-Host "  OK - Packages restored" -ForegroundColor Green
Write-Host ""

# Step 3: Build
Write-Host "[3/5] Building application..." -ForegroundColor Yellow
dotnet build ChronoPos.sln -c $Configuration --no-restore --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed!"
    exit 1
}
Write-Host "  OK - Build successful" -ForegroundColor Green
Write-Host ""

# Step 4: Publish self-contained
Write-Host "[4/5] Publishing self-contained package..." -ForegroundColor Yellow
dotnet publish $ProjectPath `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -o $OutputPath `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed!"
    exit 1
}

# Check main executable
$ExePath = Join-Path $OutputPath "ChronoPos.Desktop.exe"
if (-not (Test-Path $ExePath)) {
    Write-Error "Main executable not found!"
    exit 1
}

$FileCount = (Get-ChildItem $OutputPath -Recurse -File).Count
$TotalSizeMB = [math]::Round((Get-ChildItem $OutputPath -Recurse -File | Measure-Object Length -Sum).Sum / 1MB, 2)

Write-Host "  OK - Published successfully" -ForegroundColor Green
Write-Host "  Files: $FileCount" -ForegroundColor Gray
Write-Host "  Size: $TotalSizeMB MB" -ForegroundColor Gray
Write-Host ""

# Step 5: Create ZIP package
Write-Host "[5/5] Creating portable ZIP..." -ForegroundColor Yellow
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($OutputPath, $ZipPath, 'Optimal', $false)

$ZipSizeMB = [math]::Round((Get-Item $ZipPath).Length / 1MB, 2)
Write-Host "  OK - ZIP created" -ForegroundColor Green
Write-Host "  Package: ChronoPos-Portable.zip" -ForegroundColor Gray
Write-Host "  Size: $ZipSizeMB MB" -ForegroundColor Gray
Write-Host ""

# Create README
$ReadmePath = Join-Path "Deployment" "README.txt"
$ReadmeContent = @"
ChronoPos Desktop POS System
============================

QUICK START:
1. Extract ChronoPos-Portable.zip to any folder
2. Run ChronoPos.Desktop.exe
3. Login with: admin / admin123

SYSTEM REQUIREMENTS:
- Windows 10 (1809+) or Windows 11
- 2 GB RAM minimum
- 500 MB disk space
- No .NET installation needed (self-contained)

PORTABLE MODE:
- Can run from USB drive
- Can run from any folder
- No installation required
- Data stored in: %LocalAppData%\ChronoPos\

FEATURES:
- Complete POS system
- Inventory management
- Sales tracking
- Customer management
- Multi-language support
- Offline capable

BUILD INFO:
- Build Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm')
- Configuration: $Configuration
- Runtime: $Runtime
- Package Size: $ZipSizeMB MB

DISTRIBUTION:
This package is ready to share. Users can:
- Download and extract anywhere
- Run immediately without installation
- No admin rights required
- Works on any compatible Windows PC

SUPPORT:
For technical support, contact your administrator.
"@

$ReadmeContent | Out-File -FilePath $ReadmePath -Encoding UTF8
Write-Host "  README.txt created" -ForegroundColor Gray
Write-Host ""

# Success summary
Write-Host "=====================================" -ForegroundColor Green
Write-Host "BUILD COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "PORTABLE PACKAGE READY:" -ForegroundColor White
Write-Host "  Location: Deployment\ChronoPos-Portable.zip" -ForegroundColor Cyan
Write-Host "  Size: $ZipSizeMB MB" -ForegroundColor Cyan
Write-Host "  Files: $FileCount files" -ForegroundColor Cyan
Write-Host ""
Write-Host "HOW TO SHARE:" -ForegroundColor White
Write-Host "  1. Upload ChronoPos-Portable.zip to cloud/server" -ForegroundColor Gray
Write-Host "  2. Share download link with users" -ForegroundColor Gray
Write-Host "  3. Users extract and run ChronoPos.Desktop.exe" -ForegroundColor Gray
Write-Host ""
Write-Host "FEATURES:" -ForegroundColor White
Write-Host "  [X] Self-contained (no .NET install needed)" -ForegroundColor Green
Write-Host "  [X] Portable (run from any location)" -ForegroundColor Green
Write-Host "  [X] Single ZIP file for easy distribution" -ForegroundColor Green
Write-Host "  [X] Works on any Windows 10/11 PC" -ForegroundColor Green
Write-Host ""
