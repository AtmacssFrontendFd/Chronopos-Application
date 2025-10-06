# Stock Adjustment Save Functionality - Comprehensive Logging Guide

## Overview
Comprehensive logging has been added to the stock adjustment save functionality to help debug and track the entire save process from UI interaction to database persistence.

## Logging Locations

### 1. ViewModel Layer (StockManagementViewModel.cs)
**Method: SaveAdjustProduct()**

#### Initial Logging:
- Save button click confirmation
- Service availability checks
- Form data validation logging
- All input field values (product, quantities, reason, etc.)

#### Validation Logging:
- Product selection validation
- Reason text validation  
- Quantity difference validation
- Service injection validation

#### Process Logging:
- Reason creation/retrieval process
- DTO creation with detailed field mapping
- Service call execution
- Success/failure result handling
- Form reset and data refresh

#### Error Handling:
- Comprehensive exception logging with full details
- Stack traces for debugging
- Inner exception details

### 2. Service Layer (StockAdjustmentService.cs)

#### CreateReasonIfNotExistsAsync Method:
- Input validation logging
- Database query for existing reasons
- New reason creation process
- Database save operations
- Return value confirmation

#### CreateStockAdjustmentAsync Method:
- Input DTO validation and field logging
- Database transaction management
- Adjustment number generation
- Main adjustment record creation
- Adjustment items processing
- Database save operations
- Transaction commit/rollback
- Result retrieval confirmation

#### GenerateAdjustmentNumberAsync Method:
- Prefix generation logging
- Last adjustment number lookup
- Number sequence calculation
- Final adjustment number generation

## Log Output Locations

### Application Logs
- **File**: `%LocalAppData%\ChronoPos\app.log`
- **Content**: ViewModel layer logging from LogMessage() calls

### Console Output
- **Location**: Terminal/Console where application is running
- **Content**: Service layer logging from Console.WriteLine() calls

## How to Monitor Logs

### Real-time Application Logs:
```powershell
Get-Content "$env:LOCALAPPDATA\ChronoPos\app.log" -Wait
```

### Recent Application Logs:
```powershell
Get-Content "$env:LOCALAPPDATA\ChronoPos\app.log" | Select-Object -Last 50
```

### Console Logs:
Monitor the terminal window where `dotnet run` is executed

## Testing the Save Functionality

### Steps to Test:
1. Run the application
2. Navigate to Stock Management
3. Click "Adjust Product" 
4. Search and select a product
5. Enter new quantity
6. Enter reason text
7. Click Save
8. Monitor logs for the complete save process

### Expected Log Flow:
1. **[SaveAdjustProduct] === SAVE BUTTON CLICKED ===**
2. Service and data validation logs
3. **[StockAdjustmentService] CreateReasonIfNotExistsAsync called**
4. Reason creation/retrieval process
5. **[StockAdjustmentService] === STARTING CreateStockAdjustmentAsync ===**
6. DTO validation and transaction processing
7. **[StockAdjustmentService] Generated adjustment number: ADJyyyyMM####**
8. Database save operations
9. **[SaveAdjustProduct] SUCCESS! Adjustment saved**
10. Form reset and data refresh

## Database Verification

### Check Database After Save:
```powershell
# Check reasons table
sqlite3 "$env:LOCALAPPDATA\ChronoPos\chronopos.db" "SELECT * FROM StockAdjustmentReasons ORDER BY CreatedAt DESC LIMIT 5;"

# Check adjustments table  
sqlite3 "$env:LOCALAPPDATA\ChronoPos\chronopos.db" "SELECT * FROM StockAdjustments ORDER BY AdjustmentDate DESC LIMIT 5;"

# Check adjustment items
sqlite3 "$env:LOCALAPPDATA\ChronoPos\chronopos.db" "SELECT * FROM StockAdjustmentItems ORDER BY AdjustmentItemId DESC LIMIT 5;"
```

## Key Log Markers to Watch For

### Success Indicators:
- `[SaveAdjustProduct] Validation passed`
- `[StockAdjustmentService] New reason saved with ID:`
- `[StockAdjustmentService] Transaction committed successfully`
- `[SaveAdjustProduct] SUCCESS! Adjustment saved`

### Error Indicators:
- `[SaveAdjustProduct] ERROR:`
- `[StockAdjustmentService] ERROR:`
- `Transaction rolled back`
- `FATAL ERROR:`

## Troubleshooting Guide

### If Save Fails:
1. Check application logs for validation errors
2. Check console for service layer errors
3. Verify database connectivity
4. Check database constraints and foreign keys
5. Monitor transaction rollback messages

### Common Issues to Look For:
- Service injection failures
- Database connection issues
- Constraint violations
- Transaction timeout issues
- Invalid foreign key references

## Log Level Details

### INFO Level:
- Normal operation flow
- Successful operations
- Data validation results

### ERROR Level:
- Validation failures
- Database errors
- Service exceptions
- Transaction rollbacks

### DEBUG Level:
- Detailed field values
- DTO contents
- Database query details
- Transaction states

This comprehensive logging system provides complete visibility into the stock adjustment save process, making it easy to identify and resolve any issues that may occur during testing or production use.
