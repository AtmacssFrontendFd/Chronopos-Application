# Check Sales Data in Database
$dbPath = "$env:LOCALAPPDATA\ChronoPos\chronopos.db"

Write-Host "Database: $dbPath" -ForegroundColor Cyan
Write-Host ""

# Check if SQLite is available
$sqliteCmd = Get-Command sqlite3 -ErrorAction SilentlyContinue

if ($sqliteCmd) {
    Write-Host "Sales in Database (Last 10):" -ForegroundColor Green
    sqlite3 $dbPath "SELECT Id, datetime(SaleDate), TotalAmount, Status FROM Sales ORDER BY SaleDate DESC LIMIT 10;"
    
    Write-Host ""
    Write-Host "Sales Status Breakdown:" -ForegroundColor Green
    sqlite3 $dbPath "SELECT Status, COUNT(*) as Count, SUM(TotalAmount) as Total FROM Sales GROUP BY Status;"
    
    Write-Host ""
    Write-Host "Sales Today (2025-11-03):" -ForegroundColor Green
    sqlite3 $dbPath "SELECT Id, datetime(SaleDate), TotalAmount, Status FROM Sales WHERE date(SaleDate) = '2025-11-03';"
} else {
    Write-Host "SQLite not found. Install it or use alternative method." -ForegroundColor Yellow
    
    # Alternative: Use .NET to check
    Write-Host ""
    Write-Host "Checking using .NET..." -ForegroundColor Cyan
    
    Add-Type -Path "C:\Users\saswa\.nuget\packages\microsoft.data.sqlite\8.0.0\lib\net8.0\Microsoft.Data.Sqlite.dll" -ErrorAction SilentlyContinue
    
    $connectionString = "Data Source=$dbPath"
    $connection = New-Object Microsoft.Data.Sqlite.SqliteConnection($connectionString)
    
    try {
        $connection.Open()
        
        $command = $connection.CreateCommand()
        $command.CommandText = "SELECT COUNT(*) as Count FROM Sales WHERE date(SaleDate) >= date('2025-10-04') AND date(SaleDate) <= date('2025-11-03')"
        $count = $command.ExecuteScalar()
        
        Write-Host "Total sales in date range (2025-10-04 to 2025-11-03): $count" -ForegroundColor Green
        
        $command.CommandText = "SELECT Status, COUNT(*) FROM Sales GROUP BY Status"
        $reader = $command.ExecuteReader()
        
        Write-Host ""
        Write-Host "Sales by Status:" -ForegroundColor Green
        while ($reader.Read()) {
            Write-Host "  Status $($reader[0]): $($reader[1]) sales"
        }
        $reader.Close()
        
    } finally {
        $connection.Close()
    }
}
