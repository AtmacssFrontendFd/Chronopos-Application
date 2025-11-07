# Recalculate Stock Ledger Balances
# This script fixes the balance column in stock_ledger entries

$dbPath = "$env:LOCALAPPDATA\ChronoPos\ChronoPos.db"

Write-Host "Recalculating stock ledger balances..." -ForegroundColor Cyan

# Get all products with stock ledger entries
$products = sqlite3 $dbPath "SELECT DISTINCT product_id FROM stock_ledger ORDER BY product_id;"

$productList = $products -split "`n" | Where-Object { $_ -ne "" }

foreach ($productId in $productList) {
    Write-Host "Processing Product ID: $productId" -ForegroundColor Yellow
    
    # Get all stock ledger entries for this product, ordered by date
    $entries = sqlite3 $dbPath "SELECT id, movement_type, qty, created_at FROM stock_ledger WHERE product_id = $productId ORDER BY created_at;"
    
    $entryList = $entries -split "`n" | Where-Object { $_ -ne "" }
    $runningBalance = 0
    
    foreach ($entry in $entryList) {
        $parts = $entry -split '\|'
        $id = $parts[0]
        $movementType = [int]$parts[1]
        $qty = [decimal]$parts[2]
        $createdAt = $parts[3]
        
        # Calculate new balance based on movement type
        # 1=Purchase (IN), 2=Sale (OUT), 3=Adjustment, 4=TransferIn (IN), 
        # 5=TransferOut (OUT), 6=Return (IN), 7=Replace, 8=Waste (OUT), 9=Opening, 10=Closing
        
        switch ($movementType) {
            1 { $runningBalance += $qty }  # Purchase (IN)
            2 { $runningBalance -= $qty }  # Sale (OUT)
            3 { $runningBalance += $qty }  # Adjustment (can be + or -)
            4 { $runningBalance += $qty }  # TransferIn (IN)
            5 { $runningBalance -= $qty }  # TransferOut (OUT)
            6 { $runningBalance += $qty }  # Return (IN)
            7 { }                          # Replace (no change)
            8 { $runningBalance -= $qty }  # Waste (OUT)
            9 { $runningBalance = $qty }   # Opening (set to qty)
            10 { $runningBalance = $qty }  # Closing (set to qty)
            default { }
        }
        
        # Update the balance in database
        sqlite3 $dbPath "UPDATE stock_ledger SET balance = $runningBalance WHERE id = $id;"
        
        Write-Host "  Entry ID $id - Movement: $movementType, Qty: $qty, New Balance: $runningBalance" -ForegroundColor Gray
    }
    
    Write-Host "Product $productId - Final Balance: $runningBalance" -ForegroundColor Green
}

Write-Host "`nRecalculation complete!" -ForegroundColor Green
Write-Host "Verifying results..." -ForegroundColor Cyan

# Show updated balances
sqlite3 $dbPath "SELECT sl.id, p.Name, sl.movement_type, sl.qty, sl.balance, sl.created_at FROM stock_ledger sl JOIN Products p ON sl.product_id = p.Id ORDER BY sl.created_at;"
