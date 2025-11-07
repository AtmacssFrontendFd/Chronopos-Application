# Add Opening Stock Ledger Entries
# This adds opening stock entries before existing sales

$dbPath = "$env:LOCALAPPDATA\ChronoPos\ChronoPos.db"

Write-Host "Adding opening stock entries..." -ForegroundColor Cyan

# Define opening stocks (current stock + sales)
$openingStocks = @(
    @{ ProductId = 1; ProductName = "iphone 6s"; OpeningQty = 70; Date = "2025-11-01 23:00:00" },
    @{ ProductId = 2; ProductName = "chocolate"; OpeningQty = 10; Date = "2025-11-02 00:00:00" },
    @{ ProductId = 3; ProductName = "realme"; OpeningQty = 10; Date = "2025-11-06 17:00:00" }
)

foreach ($stock in $openingStocks) {
    $productId = $stock.ProductId
    $productName = $stock.ProductName
    $qty = $stock.OpeningQty
    $date = $stock.Date
    
    # Insert opening stock entry (movement_type = 9 for Opening)
    $sql = @"
INSERT INTO stock_ledger (product_id, unit_id, movement_type, qty, balance, location, reference_type, reference_id, created_at, note)
VALUES ($productId, 1, 9, $qty, $qty, 'Main Store', NULL, NULL, '$date', 'Opening Stock');
"@
    
    sqlite3 $dbPath $sql
    Write-Host "Added opening stock for $productName`: $qty units" -ForegroundColor Green
}

Write-Host "`nRecalculating balances after adding opening stock..." -ForegroundColor Cyan

# Now recalculate all balances
$products = sqlite3 $dbPath "SELECT DISTINCT product_id FROM stock_ledger ORDER BY product_id;"
$productList = $products -split "`n" | Where-Object { $_ -ne "" }

foreach ($productId in $productList) {
    Write-Host "Processing Product ID: $productId" -ForegroundColor Yellow
    
    $entries = sqlite3 $dbPath "SELECT id, movement_type, qty FROM stock_ledger WHERE product_id = $productId ORDER BY created_at;"
    $entryList = $entries -split "`n" | Where-Object { $_ -ne "" }
    $runningBalance = 0
    
    foreach ($entry in $entryList) {
        $parts = $entry -split '\|'
        $id = $parts[0]
        $movementType = [int]$parts[1]
        $qty = [decimal]$parts[2]
        
        switch ($movementType) {
            1 { $runningBalance += $qty }  # Purchase (IN)
            2 { $runningBalance -= $qty }  # Sale (OUT)
            3 { $runningBalance += $qty }  # Adjustment
            4 { $runningBalance += $qty }  # TransferIn (IN)
            5 { $runningBalance -= $qty }  # TransferOut (OUT)
            6 { $runningBalance += $qty }  # Return (IN)
            7 { }                          # Replace
            8 { $runningBalance -= $qty }  # Waste (OUT)
            9 { $runningBalance = $qty }   # Opening (set to qty)
            10 { $runningBalance = $qty }  # Closing (set to qty)
        }
        
        sqlite3 $dbPath "UPDATE stock_ledger SET balance = $runningBalance WHERE id = $id;"
        Write-Host "  Entry ID $id - Movement: $movementType, Qty: $qty, Balance: $runningBalance" -ForegroundColor Gray
    }
}

Write-Host "`nOpening stock entries added and balances recalculated!" -ForegroundColor Green
Write-Host "`nFinal stock ledger:" -ForegroundColor Cyan
sqlite3 $dbPath "SELECT sl.id, p.Name, CASE sl.movement_type WHEN 9 THEN 'Opening' WHEN 2 THEN 'Sale' ELSE 'Other' END as Type, sl.qty, sl.balance, sl.created_at FROM stock_ledger sl JOIN Products p ON sl.product_id = p.Id ORDER BY sl.created_at;"
