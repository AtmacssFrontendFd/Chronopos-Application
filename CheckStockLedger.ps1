# Check Stock Ledger Entries
$ErrorActionPreference = "Stop"

Write-Host "=== Checking Stock Ledger Data ===" -ForegroundColor Cyan

$dbPath = ".\ChronoPos.db"

if (-not (Test-Path $dbPath)) {
    Write-Host "Database not found at: $dbPath" -ForegroundColor Red
    Write-Host "Please run this script from the project root directory" -ForegroundColor Yellow
    exit
}

# Check if stock_ledger table exists and has data
$query1 = "SELECT COUNT(*) as count FROM stock_ledger;"
$result1 = sqlite3 $dbPath $query1

Write-Host "`nTotal Stock Ledger Entries: $result1" -ForegroundColor Yellow

# Check stock ledger entries with details
$query2 = @"
SELECT 
    sl.id,
    sl.product_id,
    p.name as product_name,
    sl.movement_type,
    sl.qty,
    sl.balance,
    sl.reference_no,
    datetime(sl.created_at) as created_at
FROM stock_ledger sl
LEFT JOIN products p ON sl.product_id = p.id
ORDER BY sl.created_at DESC
LIMIT 10;
"@

Write-Host "`nRecent Stock Ledger Entries:" -ForegroundColor Cyan
sqlite3 $dbPath $query2 -header -column

# Check if sales exist
$query3 = "SELECT COUNT(*) as count FROM transactions WHERE status IN ('settled', 'billed');"
$result3 = sqlite3 $dbPath $query3

Write-Host "`nTotal Settled/Billed Transactions: $result3" -ForegroundColor Yellow

# Check transaction items
$query4 = @"
SELECT 
    t.id as trans_id,
    t.transaction_no,
    t.status,
    ti.product_id,
    p.name as product_name,
    ti.quantity,
    datetime(t.created_at) as created_at
FROM transactions t
JOIN transaction_items ti ON t.id = ti.transaction_id
LEFT JOIN products p ON ti.product_id = p.id
WHERE t.status IN ('settled', 'billed')
ORDER BY t.created_at DESC
LIMIT 10;
"@

Write-Host "`nRecent Transaction Items:" -ForegroundColor Cyan
sqlite3 $dbPath $query4 -header -column

Write-Host "`n=== Analysis ===" -ForegroundColor Green
Write-Host "If stock_ledger count is 0 but you have transactions, the stock ledger is not being updated during sales." -ForegroundColor Yellow
Write-Host "This needs to be fixed in the sales completion flow." -ForegroundColor Yellow
