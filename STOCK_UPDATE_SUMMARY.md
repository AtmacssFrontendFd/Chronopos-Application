# ✅ Goods Replace - Stock Update Summary

## Stock Update Strategy (Verified Implementation)

When a Goods Replace is saved with **Status = "Posted"**, the system performs **DIRECT UPDATES** to:

### 1️⃣ Product Table
```sql
UPDATE Products 
SET StockQuantity = StockQuantity + @quantity,
    InitialStock = InitialStock + @quantity
WHERE Id = @productId
```

### 2️⃣ ProductBatch Table
```sql
-- If batch exists:
UPDATE ProductBatches 
SET Quantity = Quantity + @quantity,
    CostPrice = @rate,
    ExpiryDate = @expiryDate
WHERE ProductId = @productId AND BatchNo = @batchNo

-- If batch doesn't exist:
INSERT INTO ProductBatches (ProductId, BatchNo, Quantity, CostPrice, ExpiryDate, ...)
VALUES (@productId, @batchNo, @quantity, @rate, @expiryDate, ...)
```

---

## Code Flow

```csharp
// CreateGoodsReplaceAsync() - Main method
foreach (var item in dto.Items)
{
    if (dto.Status == "Posted")  // ✅ Only if posted
    {
        // 1. Update Product stock directly
        await IncreaseProductStockAsync(item.ProductId, item.Quantity);
        
        // 2. Update Batch stock (if batch specified)
        if (!string.IsNullOrEmpty(item.BatchNo))
        {
            await AddOrUpdateProductBatchAsync(
                item.ProductId, 
                item.BatchNo, 
                item.ExpiryDate, 
                item.Quantity, 
                item.Rate);
        }
    }
    
    // 3. Save replace item record
    _context.GoodsReplaceItems.Add(new GoodsReplaceItem { ... });
}

await _context.SaveChangesAsync();
```

---

## What Gets Updated

| Entity | Field | Logic |
|--------|-------|-------|
| **Product** | StockQuantity (int) | `+= (int)quantity` |
| **Product** | InitialStock (decimal) | `+= quantity` |
| **ProductBatch** | Quantity (decimal) | `+= quantity` (existing)<br>`= quantity` (new) |
| **ProductBatch** | CostPrice | `= rate` |
| **ProductBatch** | ExpiryDate | `= expiryDate` |

---

## What Does NOT Get Updated

❌ **NO StockLevel table** - Not used at all  
❌ **NO StockMovement table** - Not used  
❌ **NO complex stock transactions** - Simple direct updates only

---

## Example

**Before:**
- Product (ID: 100): StockQuantity = 50, InitialStock = 50.00
- ProductBatch (ProductId: 100, Batch: "B001"): Quantity = 20.00

**Goods Replace Posted:**
- ProductId: 100, BatchNo: "B001", Quantity: 10

**After:**
- Product (ID: 100): StockQuantity = **60** ✅, InitialStock = **60.00** ✅
- ProductBatch (ProductId: 100, Batch: "B001"): Quantity = **30.00** ✅

---

## Status Impact

| Status | Product Stock | Batch Stock | Records Saved |
|--------|--------------|-------------|---------------|
| **Posted** | ✅ Increased | ✅ Increased | ✅ Yes |
| **Draft** | ❌ No change | ❌ No change | ✅ Yes |
| **Cancelled** | ❌ No change | ❌ No change | ✅ Yes |

---

## Verification

✅ **Confirmed:** Direct Product & ProductBatch updates only  
✅ **No StockLevel dependency**  
✅ **Simple, straightforward logic**  
✅ **Well-logged for audit trail**

---

**Implementation:** Already working correctly in `GoodsReplaceService.cs`  
**Methods:** `IncreaseProductStockAsync()` & `AddOrUpdateProductBatchAsync()`
