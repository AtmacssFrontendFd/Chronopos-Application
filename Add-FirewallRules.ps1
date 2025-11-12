# ChronoPos - Add Windows Firewall Rules for Host Discovery
# Run this script as ADMINISTRATOR on BOTH host and client devices

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "ChronoPos Firewall Configuration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$isAdmin = $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click on PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    Write-Host ""
    pause
    exit 1
}

Write-Host "✅ Running with Administrator privileges" -ForegroundColor Green
Write-Host ""

# Get the ChronoPos executable path
$exePath = Join-Path $PSScriptRoot "src\ChronoPos.Desktop\bin\Release\net9.0-windows\win-x64\publish\ChronoPos.Desktop.exe"

if (-not (Test-Path $exePath)) {
    Write-Host "⚠️  Published executable not found at: $exePath" -ForegroundColor Yellow
    Write-Host "⚠️  Will create rules for any ChronoPos.Desktop.exe" -ForegroundColor Yellow
    $exePath = "%ProgramFiles%\ChronoPos\ChronoPos.Desktop.exe"
}

Write-Host "Adding firewall rules for UDP port 42099..." -ForegroundColor Cyan
Write-Host ""

# Remove existing rules if they exist
Write-Host "Removing any existing ChronoPos firewall rules..." -ForegroundColor Yellow
netsh advfirewall firewall delete rule name="ChronoPos UDP Inbound" 2>$null | Out-Null
netsh advfirewall firewall delete rule name="ChronoPos UDP Outbound" 2>$null | Out-Null
netsh advfirewall firewall delete rule name="ChronoPos Multicast Inbound" 2>$null | Out-Null
netsh advfirewall firewall delete rule name="ChronoPos Multicast Outbound" 2>$null | Out-Null

# Add inbound rule for UDP port 42099
Write-Host "1. Adding UDP Inbound rule (port 42099)..." -ForegroundColor Cyan
$result1 = netsh advfirewall firewall add rule `
    name="ChronoPos UDP Inbound" `
    dir=in `
    action=allow `
    protocol=UDP `
    localport=42099 `
    profile=private,domain `
    description="Allow ChronoPos host discovery UDP inbound traffic"

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Inbound rule added successfully" -ForegroundColor Green
} else {
    Write-Host "   ❌ Failed to add inbound rule" -ForegroundColor Red
}

# Add outbound rule for UDP port 42099
Write-Host "2. Adding UDP Outbound rule (port 42099)..." -ForegroundColor Cyan
$result2 = netsh advfirewall firewall add rule `
    name="ChronoPos UDP Outbound" `
    dir=out `
    action=allow `
    protocol=UDP `
    localport=42099 `
    profile=private,domain `
    description="Allow ChronoPos host discovery UDP outbound traffic"

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Outbound rule added successfully" -ForegroundColor Green
} else {
    Write-Host "   ❌ Failed to add outbound rule" -ForegroundColor Red
}

# Add multicast inbound rule
Write-Host "3. Adding Multicast Inbound rule (239.255.42.99)..." -ForegroundColor Cyan
$result3 = netsh advfirewall firewall add rule `
    name="ChronoPos Multicast Inbound" `
    dir=in `
    action=allow `
    protocol=UDP `
    remoteip=239.255.42.99 `
    localport=42099 `
    profile=private,domain `
    description="Allow ChronoPos multicast inbound traffic"

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Multicast inbound rule added successfully" -ForegroundColor Green
} else {
    Write-Host "   ❌ Failed to add multicast inbound rule" -ForegroundColor Red
}

# Add multicast outbound rule
Write-Host "4. Adding Multicast Outbound rule (239.255.42.99)..." -ForegroundColor Cyan
$result4 = netsh advfirewall firewall add rule `
    name="ChronoPos Multicast Outbound" `
    dir=out `
    action=allow `
    protocol=UDP `
    remoteip=239.255.42.99 `
    localport=42099 `
    profile=private,domain `
    description="Allow ChronoPos multicast outbound traffic"

if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Multicast outbound rule added successfully" -ForegroundColor Green
} else {
    Write-Host "   ❌ Failed to add multicast outbound rule" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Firewall Configuration Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "IMPORTANT:" -ForegroundColor Yellow
Write-Host "1. Run this script on BOTH the host and client devices" -ForegroundColor Yellow
Write-Host "2. Make sure both devices are on the same WiFi network" -ForegroundColor Yellow
Write-Host "3. Restart ChronoPos application on both devices" -ForegroundColor Yellow
Write-Host "4. If still not working, check your router's AP Isolation settings" -ForegroundColor Yellow
Write-Host ""

# Display current rules
Write-Host "Current ChronoPos firewall rules:" -ForegroundColor Cyan
netsh advfirewall firewall show rule name=all | Select-String "ChronoPos"

Write-Host ""
pause
