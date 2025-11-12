# Transaction System - Database Migration Guide

## Overview
This guide provides step-by-step instructions for creating and applying the database migration for the complete Transaction System implementation.

---

## Prerequisites

Before running the migration, ensure:
1. ✅ All 10 entities have been created
2. ✅ All repository interfaces and implementations exist
3. ✅ DbSets have been added to ChronoPosDbContext
4. ✅ Entity configurations are in OnModelCreating method
5. ✅ Solution builds without errors

---

## Step 1: Build the Solution

First, ensure the solution compiles successfully:

```powershell
# From solution root directory
dotnet build
```

**Expected Output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## Step 2: Navigate to Infrastructure Project

```powershell
cd src\ChronoPos.Infrastructure
```

---

## Step 3: Create the Migration

Run the following command to create a new migration:

```powershell
dotnet ef migrations add AddTransactionSystem --startup-project ..\ChronoPos.Desktop --context ChronoPosDbContext
```

**Parameters Explained:**
- `add AddTransactionSystem` - Creates a new migration named "AddTransactionSystem"
- `--startup-project ..\ChronoPos.Desktop` - Specifies the startup project (contains App.xaml.cs with DI)
- `--context ChronoPosDbContext` - Specifies which DbContext to use

**Expected Output:**
```
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

**Files Created:**
- `Migrations/[Timestamp]_AddTransactionSystem.cs` - The migration file
- `Migrations/ChronoPosDbContextModelSnapshot.cs` - Updated model snapshot

---

## Step 4: Review the Migration

Before applying, review the generated migration file to ensure it contains:

### Expected Tables to be Created:
1. `Shifts` - With columns: ShiftId, UserId, ShopLocationId, StartTime, EndTime, OpeningCash, ClosingCash, ExpectedCash, CashDifference, Status, Note, CreatedAt, UpdatedAt
2. `ServiceCharges` - With columns: Id, Name, NameArabic, IsPercentage, Value, TaxTypeId, IsActive, AutoApply, CreatedAt, UpdatedAt
3. `Transactions` - With columns: Id, ShiftId, CustomerId, UserId, ShopLocationId, TableId, ReservationId, SellingTime, TotalAmount, TotalVat, TotalDiscount, etc.
4. `TransactionProducts` - With columns: Id, TransactionId, ProductId, BuyerCost, SellingPrice, Quantity, etc.
5. `TransactionModifiers` - With columns: Id, TransactionProductId, ProductModifierId, ExtraPrice, CreatedAt
6. `TransactionServiceCharges` - With columns: Id, TransactionId, ServiceChargeId, TotalAmount, TotalVat, Status, CreatedAt
7. `RefundTransactions` - With columns: Id, SellingTransactionId, CustomerId, ShiftId, UserId, RefundTime, TotalAmount, TotalVat, Reason, Status, CreatedAt
8. `RefundTransactionProducts` - With columns: Id, RefundTransactionId, TransactionProductId, TotalQuantityReturned, TotalAmount, TotalVat, Status, CreatedAt
9. `ExchangeTransactions` - With columns: Id, SellingTransactionId, CustomerId, ShiftId, ExchangeTime, TotalExchangedAmount, TotalExchangedVat, ProductExchangedQuantity, Reason, Status, CreatedAt
10. `ExchangeTransactionProducts` - With columns: Id, ExchangeTransactionId, OriginalTransactionProductId, NewProductId, ReturnedQuantity, NewQuantity, OldProductAmount, NewProductAmount, PriceDifference, VatDifference, Status, CreatedAt

### Expected Foreign Keys:
- Shifts → Users (ON DELETE RESTRICT)
- Shifts → ShopLocations (ON DELETE RESTRICT)
- ServiceCharges → TaxTypes (ON DELETE SET NULL)
- Transactions → Shifts (ON DELETE RESTRICT)
- Transactions → Customers (ON DELETE SET NULL)
- Transactions → Users (ON DELETE RESTRICT)
- TransactionProducts → Transactions (ON DELETE CASCADE)
- TransactionProducts → Products (ON DELETE RESTRICT)
- TransactionModifiers → TransactionProducts (ON DELETE CASCADE)
- TransactionModifiers → ProductModifiers (ON DELETE RESTRICT)
- And many more...

### Expected Indexes:
- Shifts: IX_Shifts_UserId, IX_Shifts_ShopLocationId, IX_Shifts_Status, IX_Shifts_StartTime
- ServiceCharges: IX_ServiceCharges_Name (UNIQUE), IX_ServiceCharges_IsActive, IX_ServiceCharges_AutoApply
- Transactions: IX_Transactions_ShiftId, IX_Transactions_CustomerId, IX_Transactions_InvoiceNumber (UNIQUE), etc.
- And indexes on all foreign key columns...

---

## Step 5: Apply the Migration

Once you've reviewed the migration and confirmed it looks correct, apply it to the database:

```powershell
dotnet ef database update --startup-project ..\ChronoPos.Desktop --context ChronoPosDbContext
```

**Expected Output:**
```
Build started...
Build succeeded.
Applying migration '20240120123456_AddTransactionSystem'.
Done.
```

---

## Step 6: Verify the Migration

### Option A: Using SQL Database Browser
1. Open the SQLite database file (typically in `bin/Debug/net9.0/`)
2. Check that all 10 new tables exist
3. Verify foreign key constraints
4. Verify indexes

### Option B: Using EF Core
Run a simple query to verify:

```csharp
using (var context = new ChronoPosDbContext())
{
    var shiftCount = await context.Shifts.CountAsync();
    var transactionCount = await context.Transactions.CountAsync();
    Console.WriteLine($"Shifts: {shiftCount}, Transactions: {transactionCount}");
}
```

---

## Troubleshooting

### Error: "Build failed"
**Solution:** 
1. Check for syntax errors in entity files
2. Ensure all `using` statements are present
3. Run `dotnet build` to see specific errors

### Error: "No DbContext was found"
**Solution:**
1. Ensure you're in the Infrastructure project directory
2. Check that `--startup-project` points to ChronoPos.Desktop
3. Verify ChronoPosDbContext is properly configured

### Error: "Foreign key constraint failed"
**Solution:**
1. Ensure referenced tables exist (Users, Customers, Products, etc.)
2. Check OnDelete behaviors in entity configurations
3. May need to add missing data to referenced tables

### Error: "Column already exists"
**Solution:**
1. You may have an existing migration
2. Run: `dotnet ef migrations remove`
3. Or: `dotnet ef database update [PreviousMigration]` to rollback
4. Then recreate the migration

---

## Rollback Instructions

If you need to rollback the migration:

### Remove the migration (before applying):
```powershell
dotnet ef migrations remove --startup-project ..\ChronoPos.Desktop
```

### Rollback the database (after applying):
```powershell
# Rollback to previous migration
dotnet ef database update [PreviousMigrationName] --startup-project ..\ChronoPos.Desktop

# Then remove the migration
dotnet ef migrations remove --startup-project ..\ChronoPos.Desktop
```

---

## Post-Migration Tasks

After successfully applying the migration:

### 1. Test Database Operations
Create a simple test to verify CRUD operations work:

```csharp
// Test Shift creation
var shift = new Shift 
{ 
    UserId = 1, 
    StartTime = DateTime.Now, 
    OpeningCash = 100, 
    Status = "Open" 
};
await shiftRepository.AddAsync(shift);
await unitOfWork.SaveChangesAsync();
```

### 2. Seed Initial Data (Optional)
Consider seeding:
- Default service charges (Delivery Fee, Service Charge, etc.)
- Sample shifts for testing
- Test transactions

### 3. Update Database Documentation
Document the new tables in your database schema documentation.

### 4. Commit Changes
```bash
git add .
git commit -m "feat: Add complete Transaction System with 10 entities, repositories, services, and EF configurations"
```

---

## Migration Summary

### What This Migration Creates:
- **10 new tables** for transaction management
- **50+ foreign key constraints** for data integrity
- **30+ indexes** for query performance
- **Cascade delete** rules for dependent data
- **Precision specifications** for decimal/money columns
- **String length constraints** for text columns
- **Unique constraints** for business rules (invoice number, service charge name)
- **Default values** for status fields

### Database Size Impact:
- **Empty tables:** ~100 KB
- **With sample data (1000 transactions):** ~5-10 MB
- **Production (100K transactions):** ~500 MB - 1 GB

### Performance Considerations:
- All foreign keys are indexed automatically
- Additional indexes on frequently queried columns (Status, Date columns)
- Composite indexes where appropriate
- Proper delete cascade rules to maintain referential integrity

---

## Verification Checklist

After migration, verify:
- [ ] All 10 tables created successfully
- [ ] Foreign keys are properly configured
- [ ] Indexes exist on all foreign key columns
- [ ] Unique constraints work (try inserting duplicate invoice numbers)
- [ ] Cascade deletes work (delete transaction → products deleted)
- [ ] Restrict deletes work (cannot delete shift with transactions)
- [ ] Set null deletes work (delete customer → transaction.CustomerId = null)
- [ ] Decimal precision is correct (12,2 for money, 10,3 for quantities)
- [ ] Default values are applied (Status = "Open" for new shifts)

---

## Next Steps

Once migration is successful:
1. ✅ Database schema is ready
2. ⏭️ Start UI development (ViewModels + Views)
3. ⏭️ Create unit tests for services
4. ⏭️ Create integration tests for repositories
5. ⏭️ Implement POS screens
6. ⏭️ Implement reporting

---

## Additional Resources

### EF Core Migration Commands Reference:
```powershell
# List all migrations
dotnet ef migrations list

# Generate SQL script (without applying)
dotnet ef migrations script

# Update to specific migration
dotnet ef database update [MigrationName]

# Remove last migration
dotnet ef migrations remove

# Drop database (BE CAREFUL!)
dotnet ef database drop
```

### Database Connection String:
Check your `appsettings.json` or `App.xaml.cs` for the SQLite database location:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=chronopos.db"
  }
}
```

---

## Support

If you encounter issues:
1. Check the error message carefully
2. Verify all entities compile successfully
3. Ensure DbContext configurations are correct
4. Check that startup project has all dependencies
5. Review migration file for any anomalies

For additional help, refer to:
- EF Core Documentation: https://learn.microsoft.com/en-us/ef/core/
- SQLite Documentation: https://www.sqlite.org/docs.html
