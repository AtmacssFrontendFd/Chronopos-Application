# Apply Database Migration Script
# This script adds the CustomerGroups table to the existing database without losing data

Write-Host "ChronoPos Database Migration Script" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Get the local app data path
$localAppData = [Environment]::GetFolderPath('LocalApplicationData')
$chronoPosPath = Join-Path $localAppData "ChronoPos"
$databasePath = Join-Path $chronoPosPath "chronopos.db"
$sqlScriptPath = "AddCustomerGroupsTable.sql"

Write-Host "Database location: $databasePath" -ForegroundColor Yellow
Write-Host "SQL Script: $sqlScriptPath" -ForegroundColor Yellow
Write-Host ""

# Check if database exists
if (-not (Test-Path $databasePath)) {
    Write-Host "✗ Database not found at: $databasePath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please run the application first to create the database." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit
}

# Check if SQL script exists
if (-not (Test-Path $sqlScriptPath)) {
    Write-Host "✗ SQL script not found: $sqlScriptPath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Make sure the script is in the current directory." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit
}

Write-Host "✓ Database found" -ForegroundColor Green
Write-Host "✓ SQL script found" -ForegroundColor Green
Write-Host ""

Write-Warning "This will modify your database structure!"
$confirmation = Read-Host "Do you want to continue? (yes/no)"

if ($confirmation -ne 'yes') {
    Write-Host ""
    Write-Host "Operation cancelled." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit
}

try {
    Write-Host ""
    Write-Host "Applying migration..." -ForegroundColor Cyan
    
    # Load SQLite assembly
    Add-Type -Path "System.Data.SQLite.dll" -ErrorAction SilentlyContinue
    
    # Read SQL script
    $sqlScript = Get-Content $sqlScriptPath -Raw
    
    # Create connection
    $connectionString = "Data Source=$databasePath"
    $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
    $connection.Open()
    
    # Execute SQL script
    $command = $connection.CreateCommand()
    $command.CommandText = $sqlScript
    $result = $command.ExecuteNonQuery()
    
    $connection.Close()
    
    Write-Host ""
    Write-Host "✓ Migration applied successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Changes applied:" -ForegroundColor Yellow
    Write-Host "  - CustomerGroups table created" -ForegroundColor White
    Write-Host "  - CustomerGroupId column added to Customers table" -ForegroundColor White
    Write-Host "  - Indexes created for better performance" -ForegroundColor White
    Write-Host "  - Sample customer groups added (Retail, Wholesale, VIP)" -ForegroundColor White
}
catch {
    Write-Host ""
    Write-Host "✗ Error applying migration: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Alternative: Use ResetDatabase.ps1 to delete and recreate the database" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
