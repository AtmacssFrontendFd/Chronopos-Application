# ChronoPos Complete Package Builder
# This script creates a professional installer with Inno Setup

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  ChronoPos Complete Package Builder" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Check for Inno Setup
$InnoSetupPath = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $InnoSetupPath)) {
    Write-Host "ERROR: Inno Setup 6 not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install Inno Setup 6 from:" -ForegroundColor Yellow
    Write-Host "https://jrsoftware.org/isdl.php" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "After installation, run this script again." -ForegroundColor Yellow
    Write-Host ""
    pause
    exit 1
}

Write-Host "[OK] Inno Setup 6 found" -ForegroundColor Green
Write-Host ""

# Step 1: Build Portable Version First
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "STEP 1: Building Portable Version" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""

if (-not (Test-Path ".\BuildPortable.ps1")) {
    Write-Error "BuildPortable.ps1 not found!"
    exit 1
}

Write-Host "Running BuildPortable.ps1..." -ForegroundColor Cyan
& .\BuildPortable.ps1 -Configuration $Configuration -Runtime $Runtime

if ($LASTEXITCODE -ne 0) {
    Write-Error "Portable build failed!"
    exit 1
}

Write-Host ""
Write-Host "[OK] Portable version built successfully" -ForegroundColor Green
Write-Host ""

# Verify portable build exists
$PortablePath = "Deployment\ChronoPos-Portable"
if (-not (Test-Path $PortablePath)) {
    Write-Error "Portable build not found at: $PortablePath"
    exit 1
}

# Check for logo.ico
$LogoPath = "src\ChronoPos.Desktop\Images\LogoImage.ico"
if (-not (Test-Path $LogoPath)) {
    Write-Host "WARNING: LogoImage.ico not found at $LogoPath" -ForegroundColor Yellow
    Write-Host "Installer will proceed but may not have a custom icon" -ForegroundColor Yellow
}

# Step 2: Build Installer with Inno Setup
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "STEP 2: Building Installer" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""

$InnoScriptPath = "Installer\ChronoPosSetup.iss"
if (-not (Test-Path $InnoScriptPath)) {
    Write-Error "Inno Setup script not found: $InnoScriptPath"
    exit 1
}

Write-Host "Compiling installer with Inno Setup..." -ForegroundColor Cyan
Write-Host "Script: $InnoScriptPath" -ForegroundColor Gray
Write-Host ""

# Create Installer output directory
$InstallerOutputDir = "Deployment\Installer"
if (-not (Test-Path $InstallerOutputDir)) {
    New-Item -ItemType Directory -Path $InstallerOutputDir -Force | Out-Null
}

# Run Inno Setup compiler
& $InnoSetupPath $InnoScriptPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "Inno Setup compilation failed!"
    exit 1
}

Write-Host ""
Write-Host "[OK] Installer built successfully" -ForegroundColor Green
Write-Host ""

# Step 3: Verify outputs
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "STEP 3: Verifying Outputs" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""

$SetupExe = "Deployment\Installer\ChronoPosSetup.exe"
$PortableZip = "Deployment\ChronoPos-Portable.zip"

$allGood = $true

# Check setup exe
if (Test-Path $SetupExe) {
    $setupSize = [math]::Round((Get-Item $SetupExe).Length / 1MB, 2)
    Write-Host "[OK] Installer EXE: ChronoPosSetup.exe ($setupSize MB)" -ForegroundColor Green
} else {
    Write-Host "[ERROR] Installer EXE not found!" -ForegroundColor Red
    $allGood = $false
}

# Check portable zip
if (Test-Path $PortableZip) {
    $zipSize = [math]::Round((Get-Item $PortableZip).Length / 1MB, 2)
    Write-Host "[OK] Portable ZIP: ChronoPos-Portable.zip ($zipSize MB)" -ForegroundColor Green
} else {
    Write-Host "[ERROR] Portable ZIP not found!" -ForegroundColor Red
    $allGood = $false
}

# Check documentation
$ReadmePath = "Deployment\README.txt"
if (Test-Path $ReadmePath) {
    Write-Host "[OK] Documentation: README.txt" -ForegroundColor Green
} else {
    Write-Host "[WARN] README.txt not found" -ForegroundColor Yellow
}

Write-Host ""

if (-not $allGood) {
    Write-Error "Some outputs are missing!"
    exit 1
}

# Step 4: Create deployment guide
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "STEP 4: Creating Deployment Guide" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""

$CurrentDate = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'

$DeploymentGuide = "ChronoPos Desktop - Deployment Guide`r`n"
$DeploymentGuide += "=====================================" + "`r`n`r`n"
$DeploymentGuide += "BUILD INFORMATION:`r`n"
$DeploymentGuide += "- Build Date: $CurrentDate`r`n"
$DeploymentGuide += "- Configuration: $Configuration`r`n"
$DeploymentGuide += "- Runtime: $Runtime`r`n"
$DeploymentGuide += "- Installer Size: $setupSize MB`r`n"
$DeploymentGuide += "- Portable Size: $zipSize MB`r`n`r`n"

$DeploymentGuide += "DISTRIBUTION FILES:`r`n"
$DeploymentGuide += "==================`r`n`r`n"

$DeploymentGuide += "1. PROFESSIONAL INSTALLER (Recommended for clients)`r`n"
$DeploymentGuide += "   File: Deployment\Installer\ChronoPosSetup.exe`r`n"
$DeploymentGuide += "   Size: $setupSize MB`r`n`r`n"

$DeploymentGuide += "2. PORTABLE VERSION (For testing or USB deployment)`r`n"
$DeploymentGuide += "   File: Deployment\ChronoPos-Portable.zip`r`n"
$DeploymentGuide += "   Size: $zipSize MB`r`n`r`n"

$DeploymentGuide += "For complete deployment instructions, see Installer\README.md`r`n"

$DeploymentGuide | Out-File -FilePath "Deployment\DEPLOYMENT_GUIDE.txt" -Encoding UTF8
Write-Host "[OK] Deployment guide created" -ForegroundColor Green
Write-Host ""

# Step 5: Success summary
Write-Host "=============================================" -ForegroundColor Green
Write-Host "  BUILD COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""

Write-Host "PROFESSIONAL INSTALLER:" -ForegroundColor White
Write-Host "  File: Deployment\Installer\ChronoPosSetup.exe" -ForegroundColor Cyan
Write-Host "  Size: $setupSize MB" -ForegroundColor Cyan
Write-Host "  Ready to distribute to clients!" -ForegroundColor Green
Write-Host ""

Write-Host "PORTABLE VERSION:" -ForegroundColor White
Write-Host "  File: Deployment\ChronoPos-Portable.zip" -ForegroundColor Cyan
Write-Host "  Size: $zipSize MB" -ForegroundColor Cyan
Write-Host "  For testing or USB deployment" -ForegroundColor Gray
Write-Host ""

Write-Host "FEATURES:" -ForegroundColor White
Write-Host "  [X] Professional installation wizard" -ForegroundColor Green
Write-Host "  [X] Custom installation path selection" -ForegroundColor Green
Write-Host "  [X] Automatic database initialization" -ForegroundColor Green
Write-Host "  [X] Desktop & Start menu shortcuts" -ForegroundColor Green
Write-Host "  [X] Windows uninstaller integration" -ForegroundColor Green
Write-Host "  [X] Self-contained (.NET included)" -ForegroundColor Green
Write-Host "  [X] Data preservation on uninstall option" -ForegroundColor Green
Write-Host ""

Write-Host "NEXT STEPS:" -ForegroundColor White
Write-Host "  1. Test installer on clean Windows machine" -ForegroundColor Yellow
Write-Host "  2. Verify database initialization works" -ForegroundColor Yellow
Write-Host "  3. Test license activation process" -ForegroundColor Yellow
Write-Host "  4. Upload to distribution server/cloud" -ForegroundColor Yellow
Write-Host "  5. Share with clients along with sales key" -ForegroundColor Yellow
Write-Host ""

$openFolder = Read-Host "Open Deployment folder? (Y/N)"
if ($openFolder -eq 'Y' -or $openFolder -eq 'y') {
    explorer "Deployment\Installer"
}

Write-Host ""
Write-Host "Thank you for using ChronoPos Build System!" -ForegroundColor Cyan
Write-Host ""
