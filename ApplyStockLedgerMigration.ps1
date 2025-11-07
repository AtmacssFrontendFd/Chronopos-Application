# Apply Stock Ledger Migration
# This script applies the stock_ledger table migration to the ChronoPos database

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Stock Ledger Migration Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get the database path
$dbPath = "$env:LOCALAPPDATA\ChronoPos\chronopos.db"
$migrationFile = "database\migrations\001_add_stock_ledger_table.sql"

Write-Host "Database Path: $dbPath" -ForegroundColor Yellow
Write-Host "Migration File: $migrationFile" -ForegroundColor Yellow
Write-Host ""

# Check if database exists
if (-not (Test-Path $dbPath)) {
    Write-Host "ERROR: Database not found at $dbPath" -ForegroundColor Red
    Write-Host "Please run the application at least once to create the database." -ForegroundColor Red
    exit 1
}

# Check if migration file exists
if (-not (Test-Path $migrationFile)) {
    Write-Host "ERROR: Migration file not found at $migrationFile" -ForegroundColor Red
    exit 1
}

Write-Host "Applying migration..." -ForegroundColor Green

try {
    # Read the SQL migration file
    $sqlContent = Get-Content $migrationFile -Raw
    
    # Use sqlite3 command if available, otherwise use .NET SQLite
    $sqlite3Path = Get-Command sqlite3 -ErrorAction SilentlyContinue
    
    if ($sqlite3Path) {
        # Use sqlite3 command line tool
        Write-Host "Using sqlite3 command line tool..." -ForegroundColor Cyan
        $sqlContent | sqlite3 $dbPath
        Write-Host ""
        Write-Host "[SUCCESS] Migration applied successfully using sqlite3!" -ForegroundColor Green
    }
    else {
        # Use .NET SQLite (requires System.Data.SQLite package)
        Write-Host "Using .NET SQLite..." -ForegroundColor Cyan
        
        # Load SQLite assembly
        Add-Type -Path "$PSScriptRoot\src\ChronoPos.Desktop\bin\Debug\net9.0-windows\win-x64\System.Data.SQLite.dll" -ErrorAction SilentlyContinue
        
        if (-not ([System.AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.Location -like "*System.Data.SQLite*" })) {
            Write-Host "ERROR: SQLite library not found. Please install sqlite3 command line tool." -ForegroundColor Red
            Write-Host "Download from: https://www.sqlite.org/download.html" -ForegroundColor Yellow
            exit 1
        }
        
        # Create connection
        $connectionString = "Data Source=$dbPath;Version=3;"
        $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
        $connection.Open()
        
        # Execute SQL
        $command = $connection.CreateCommand()
        $command.CommandText = $sqlContent
        $command.ExecuteNonQuery() | Out-Null
        
        # Close connection
        $connection.Close()
        
        Write-Host ""
        Write-Host "[SUCCESS] Migration applied successfully using .NET SQLite!" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Stock Ledger Table Created!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "The stock_ledger table has been created with:" -ForegroundColor White
    Write-Host "  - All required columns (id, product_id, unit_id, movement_type, qty, balance, etc.)" -ForegroundColor White
    Write-Host "  - Foreign key constraints to products and product_units" -ForegroundColor White
    Write-Host "  - Performance indexes on product_id, created_at, movement_type, reference_type" -ForegroundColor White
    Write-Host ""
    Write-Host "You can now use the Stock Ledger feature in your application!" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "ERROR: Failed to apply migration" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "You can try applying the migration manually:" -ForegroundColor Yellow
    Write-Host "1. Install sqlite3: https://www.sqlite.org/download.html" -ForegroundColor Yellow
    Write-Host "2. Run: sqlite3 `"$dbPath`" < `"$migrationFile`"" -ForegroundColor Yellow
    exit 1
}
