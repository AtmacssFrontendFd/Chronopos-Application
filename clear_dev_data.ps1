# Clear ChronoPOS Development Data
# This script clears all persisted data for a fresh start during development

Write-Host "=== ChronoPOS Development Data Cleanup ===" -ForegroundColor Cyan
Write-Host ""

# Paths
$chronoPosPath = Join-Path $env:LOCALAPPDATA "ChronoPos"
$dbPath = "e:\WORK\ChronoPosRevised\src\ChronoPos.Infrastructure\chronopos.db"

# Clear ChronoPos AppData folder (includes license, logs, and database)
if (Test-Path $chronoPosPath) {
    Write-Host "Clearing ChronoPos data folder..." -ForegroundColor Yellow
    Remove-Item $chronoPosPath -Recurse -Force
    Write-Host "✓ ChronoPos data folder cleared" -ForegroundColor Green
    Write-Host "  - License files removed" -ForegroundColor Gray
    Write-Host "  - Database removed" -ForegroundColor Gray
    Write-Host "  - Logs removed" -ForegroundColor Gray
} else {
    Write-Host "✓ No ChronoPos data folder found" -ForegroundColor Gray
}

# Clear old database in Infrastructure folder (if exists)
if (Test-Path $dbPath) {
    Write-Host "Clearing old Infrastructure database..." -ForegroundColor Yellow
    Remove-Item $dbPath -Force
    Write-Host "✓ Old database cleared" -ForegroundColor Green
} else {
    Write-Host "✓ No old database file found" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=== Cleanup Complete ===" -ForegroundColor Cyan
Write-Host "You can now run the application for a fresh onboarding experience." -ForegroundColor White
Write-Host ""
Write-Host "Expected Flow:" -ForegroundColor Yellow
Write-Host "  1. Onboarding Window → License activation (Step 1-6)" -ForegroundColor White
Write-Host "  2. Create Admin Window → Set admin email, username, password" -ForegroundColor White
Write-Host "  3. Login Window → Enter credentials" -ForegroundColor White
Write-Host "  4. Main Dashboard → POS application" -ForegroundColor White
Write-Host ""
