# Test Sales Report - Debug Script
# This script helps diagnose issues with the Sales Report

Write-Host "=== ChronoPos Sales Report Debug Tool ===" -ForegroundColor Cyan
Write-Host ""

# 1. Check if database file exists
Write-Host "1. Checking Database..." -ForegroundColor Yellow
$dbPath = "$env:LOCALAPPDATA\ChronoPos\chronopos.db"
if (Test-Path $dbPath) {
    $dbInfo = Get-Item $dbPath
    Write-Host "   ✓ Database found: $dbPath" -ForegroundColor Green
    Write-Host "   Size: $([math]::Round($dbInfo.Length / 1MB, 2)) MB" -ForegroundColor Gray
    Write-Host "   Last Modified: $($dbInfo.LastWriteTime)" -ForegroundColor Gray
} else {
    Write-Host "   ✗ Database NOT found at: $dbPath" -ForegroundColor Red
    Write-Host "   The application may not have created the database yet." -ForegroundColor Yellow
}
Write-Host ""

# 2. Check log files
Write-Host "2. Checking Log Files..." -ForegroundColor Yellow
$logDir = "$env:LOCALAPPDATA\ChronoPos\logs"
if (Test-Path $logDir) {
    Write-Host "   ✓ Log directory found: $logDir" -ForegroundColor Green
    $logFiles = Get-ChildItem $logDir -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 5
    if ($logFiles) {
        Write-Host "   Recent log files:" -ForegroundColor Gray
        foreach ($log in $logFiles) {
            Write-Host "   - $($log.Name) ($([math]::Round($log.Length / 1KB, 2)) KB)" -ForegroundColor Gray
        }
        
        # Check for sales_report logs
        $salesReportLogs = Get-ChildItem $logDir -Filter "sales_report*.log"
        if ($salesReportLogs) {
            Write-Host ""
            Write-Host "   Sales Report Logs:" -ForegroundColor Cyan
            $latestSalesLog = $salesReportLogs | Sort-Object LastWriteTime -Descending | Select-Object -First 1
            Write-Host "   Latest: $($latestSalesLog.Name)" -ForegroundColor Gray
            Write-Host "   Last 10 lines:" -ForegroundColor Gray
            Get-Content $latestSalesLog.FullName -Tail 10 | ForEach-Object { Write-Host "     $_" -ForegroundColor DarkGray }
        } else {
            Write-Host "   ⚠ No sales_report logs found yet" -ForegroundColor Yellow
        }
    } else {
        Write-Host "   ⚠ No log files found" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ✗ Log directory NOT found: $logDir" -ForegroundColor Red
}
Write-Host ""

# 3. Check if SQLite is available
Write-Host "3. Testing Database Connection..." -ForegroundColor Yellow
try {
    # Try to load System.Data.SQLite if available
    Add-Type -Path "$env:USERPROFILE\.nuget\packages\system.data.sqlite.core\1.0.118\lib\netstandard2.0\System.Data.SQLite.dll" -ErrorAction Stop
    Write-Host "   ✓ SQLite library loaded" -ForegroundColor Green
    
    if (Test-Path $dbPath) {
        $connString = "Data Source=$dbPath;Version=3;"
        $conn = New-Object System.Data.SQLite.SQLiteConnection($connString)
        $conn.Open()
        
        # Query sales count
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = 'SELECT COUNT(*) FROM Sales'
        $salesCount = $cmd.ExecuteScalar()
        
        Write-Host "   ✓ Database connection successful" -ForegroundColor Green
        Write-Host "   Total Sales Records: $salesCount" -ForegroundColor Cyan
        
        if ($salesCount -gt 0) {
            # Get sales by status
            $cmd.CommandText = 'SELECT Status, COUNT(*) as Count FROM Sales GROUP BY Status'
            $reader = $cmd.ExecuteReader()
            Write-Host "   Sales by Status:" -ForegroundColor Gray
            while ($reader.Read()) {
                Write-Host "   - Status $($reader['Status']): $($reader['Count']) records" -ForegroundColor Gray
            }
            $reader.Close()
            
            # Get date range
            $cmd.CommandText = 'SELECT MIN(SaleDate) as MinDate, MAX(SaleDate) as MaxDate FROM Sales'
            $reader = $cmd.ExecuteReader()
            if ($reader.Read()) {
                Write-Host "   Date Range: $($reader['MinDate']) to $($reader['MaxDate'])" -ForegroundColor Gray
            }
            $reader.Close()
        } else {
            Write-Host "   ⚠ No sales records found in database!" -ForegroundColor Yellow
            Write-Host "   This is why the report shows 0 transactions." -ForegroundColor Yellow
        }
        
        $conn.Close()
    }
}
catch {
    Write-Host "   ⚠ Could not query database directly" -ForegroundColor Yellow
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor DarkGray
    Write-Host "   (This is normal if SQLite assembly is not available)" -ForegroundColor DarkGray
}
Write-Host ""

# 4. Recommendations
Write-Host "=== Recommendations ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "To debug further:" -ForegroundColor White
Write-Host "1. Run the application and navigate to Reports > Sales Report" -ForegroundColor Gray
Write-Host "2. Watch for debug message boxes showing:" -ForegroundColor Gray
Write-Host "   - Filter date range" -ForegroundColor Gray
Write-Host "   - Total sales amount and transaction count" -ForegroundColor Gray
Write-Host "3. Check the logs at: $logDir" -ForegroundColor Gray
Write-Host "4. If no sales data exists, create a test sale first" -ForegroundColor Gray
Write-Host ""

Write-Host "Common Issues:" -ForegroundColor White
Write-Host "• Date Filter: Report defaults to TODAY only - change to 'Last 7 Days' or 'This Month'" -ForegroundColor Yellow
Write-Host "• No Data: Create some test sales transactions first" -ForegroundColor Yellow
Write-Host "• Status Filter: Make sure sales are in 'Settled' status (Status = 3)" -ForegroundColor Yellow
Write-Host ""

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
