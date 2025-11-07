# Add Missing Stock Ledger Entries from Existing Transactions
$ErrorActionPreference = "Stop"

Write-Host "=== Adding Stock Ledger Entries for Existing Transactions ===" -ForegroundColor Cyan

$dbPath = "$env:LOCALAPPDATA\ChronoPos\ChronoPos.db"

if (-not (Test-Path $dbPath)) {
    Write-Host "Database not found at: $dbPath" -ForegroundColor Red
    exit
}

Write-Host "Database found: $dbPath" -ForegroundColor Green

# SQL to insert stock ledger entries from transaction items
$sql = "INSERT INTO stock_ledger (product_id, unit_id, movement_type, qty, balance, location, reference_type, reference_id, note, created_at) SELECT tp.ProductId, 1 as unit_id, 2 as movement_type, tp.Quantity as qty, 0 as balance, 'Main Store' as location, 2 as reference_type, t.Id as reference_id, 'Auto-generated from transaction' as note, t.CreatedAt FROM TransactionProducts tp JOIN Transactions t ON tp.TransactionId = t.Id WHERE t.Status IN ('settled', 'billed') AND NOT EXISTS (SELECT 1 FROM stock_ledger sl WHERE sl.reference_type = 2 AND sl.reference_id = t.Id AND sl.product_id = tp.ProductId) ORDER BY t.CreatedAt, tp.Id;"

Write-Host "`nExecuting SQL to add stock ledger entries..." -ForegroundColor Yellow
try {
    $result = sqlite3 $dbPath $sql
    Write-Host "Stock ledger entries added successfully!" -ForegroundColor Green
    
    # Show count of new entries
    $countSql = "SELECT COUNT(*) FROM stock_ledger;"
    $count = sqlite3 $dbPath $countSql
    Write-Host "`nTotal Stock Ledger Entries Now: $count" -ForegroundColor Cyan
    
    # Show sample entries
    $sampleSql = "SELECT sl.id, sl.product_id, p.name as product, sl.movement_type, sl.qty, datetime(sl.created_at) as created_at FROM stock_ledger sl LEFT JOIN products p ON sl.product_id = p.id ORDER BY sl.created_at DESC LIMIT 10;"
    
    Write-Host "`nRecent Stock Ledger Entries:" -ForegroundColor Cyan
    sqlite3 $dbPath $sampleSql -header -column
    
    Write-Host "`n✅ Stock ledger entries created successfully!" -ForegroundColor Green
    Write-Host "Now go to Inventory Report and click 'Load Report' to see the data!" -ForegroundColor Yellow
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
