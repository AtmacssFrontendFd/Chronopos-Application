# Script to check ChronoPos database content for debugging stock adjustments
$DatabasePath = "$env:LOCALAPPDATA\ChronoPos\chronopos.db"

Write-Host "ChronoPos Database Check Script" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green
Write-Host "Database Location: $DatabasePath" -ForegroundColor Yellow

# Check if database exists
if (Test-Path $DatabasePath) {
    Write-Host "Database file exists" -ForegroundColor Green
    
    # Get file size
    $fileSize = (Get-Item $DatabasePath).Length
    Write-Host "Database size: $fileSize bytes" -ForegroundColor Cyan
    
    Write-Host "`nTo check database contents manually, use:" -ForegroundColor Yellow
    Write-Host "sqlite3 `"$DatabasePath`"" -ForegroundColor White
    Write-Host ""
    Write-Host "Useful SQL commands:" -ForegroundColor Yellow
    Write-Host "  .tables                                    # List all tables"
    Write-Host "  SELECT * FROM stock_adjustment_reasons;    # Check reasons"
    Write-Host "  SELECT * FROM stock_adjustments;           # Check adjustments"
    Write-Host "  SELECT * FROM stock_adjustment_items;      # Check adjustment items"
    Write-Host "  .exit                                      # Exit sqlite"
    
    # Try to get table count using sqlite3 if available
    try {
        $tableCount = & sqlite3 $DatabasePath ".tables" | Measure-Object -Line | Select-Object -ExpandProperty Lines
        Write-Host "`nDatabase contains $tableCount tables" -ForegroundColor Green
        
        # Check specific tables related to stock adjustments
        Write-Host "`nChecking stock adjustment related data..." -ForegroundColor Cyan
        
        $reasonCount = & sqlite3 $DatabasePath "SELECT COUNT(*) FROM stock_adjustment_reasons;"
        Write-Host "Stock adjustment reasons: $reasonCount records" -ForegroundColor White
        
        $adjustmentCount = & sqlite3 $DatabasePath "SELECT COUNT(*) FROM stock_adjustments;"
        Write-Host "Stock adjustments: $adjustmentCount records" -ForegroundColor White
        
        $itemCount = & sqlite3 $DatabasePath "SELECT COUNT(*) FROM stock_adjustment_items;"
        Write-Host "Stock adjustment items: $itemCount records" -ForegroundColor White
        
        Write-Host "`nRecent stock adjustment reasons:" -ForegroundColor Cyan
        & sqlite3 $DatabasePath "SELECT StockAdjustmentReasonsId, Name, Description, Status, CreatedAt FROM stock_adjustment_reasons ORDER BY CreatedAt DESC LIMIT 5;" | ForEach-Object {
            Write-Host "  $_" -ForegroundColor White
        }
        
        Write-Host "`nRecent stock adjustments:" -ForegroundColor Cyan
        & sqlite3 $DatabasePath "SELECT AdjustmentId, AdjustmentNo, AdjustmentDate, ReasonId, Remarks FROM stock_adjustments ORDER BY AdjustmentDate DESC LIMIT 5;" | ForEach-Object {
            Write-Host "  $_" -ForegroundColor White
        }
        
    } catch {
        Write-Host "`nSQLite3 command not available. Install SQLite3 to run database queries." -ForegroundColor Yellow
        Write-Host "Download from: https://www.sqlite.org/download.html" -ForegroundColor Yellow
    }
    
} else {
    Write-Host "Database file not found!" -ForegroundColor Red
    Write-Host "Make sure the application has been run at least once to create the database." -ForegroundColor Yellow
}

Write-Host "`nPress any key to continue..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
