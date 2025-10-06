# Real-time log monitoring script for stock adjustment testing
Write-Host "=== ChronoPos Stock Adjustment Log Monitor ===" -ForegroundColor Green
Write-Host "Monitoring logs for save functionality testing..." -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop monitoring" -ForegroundColor Cyan
Write-Host ""

$LogFile = "$env:LOCALAPPDATA\ChronoPos\app.log"

if (Test-Path $LogFile) {
    # Show last 5 lines first for context
    Write-Host "Recent log entries:" -ForegroundColor Yellow
    Get-Content $LogFile | Select-Object -Last 5 | ForEach-Object {
        Write-Host "  $_" -ForegroundColor Gray
    }
    Write-Host ""
    Write-Host "Monitoring new entries..." -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    
    # Monitor for new entries
    Get-Content $LogFile -Wait | ForEach-Object {
        $line = $_
        
        # Highlight important save-related events
        if ($line -match "SaveAdjustProduct|CreateStockAdjustment|ERROR|SUCCESS") {
            if ($line -match "ERROR|FATAL") {
                Write-Host $line -ForegroundColor Red
            }
            elseif ($line -match "SUCCESS") {
                Write-Host $line -ForegroundColor Green
            }
            elseif ($line -match "SaveAdjustProduct.*===.*SAVE") {
                Write-Host $line -ForegroundColor Cyan
            }
            else {
                Write-Host $line -ForegroundColor Yellow
            }
        }
        else {
            Write-Host $line -ForegroundColor White
        }
    }
} else {
    Write-Host "Log file not found: $LogFile" -ForegroundColor Red
    Write-Host "Make sure the application is running." -ForegroundColor Yellow
}
