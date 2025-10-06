# ChronoPos Stock Management - Complete Implementation Guide

## Overview
This document provides a comprehensive guide to the advanced stock management implementation in the ChronoPos system. The enhanced stock system is built around a multi-language, multi-location architecture that handles inventory tracking, stock movements, reservations, and automated alerts through various document types with complete audit trails.

## Entity Relationships and Database Schema

### Core Entity Hierarchy

```
PosItem (Abstract Base)
├── Product (Stock-tracked items)
└── ProductGroup (Categories/Folders)
    └── Items: List<PosItem> (Hierarchical structure)
```

### Stock-Related Entity Relationships

#### 1. Product Entity Relationships
```csharp
Product : PosItem
├── Id (Primary Key)
├── Group: ProductGroup (Many-to-One)
├── Taxes: TaxList (One-to-Many)
├── MeasurementUnit: MeasurementUnit (Many-to-One)
├── Package: Package (Many-to-One)
├── Barcodes: List<Barcode> (One-to-Many)
├── Cost: Decimal (For stock valuation)
├── Price: Decimal (Selling price)
└── IsService: bool (Excludes from stock tracking)
```

#### 2. Stock Management Entities
```csharp
StockItem
├── Product: Product (Many-to-One)
├── Warehouse: Warehouse (Many-to-One)
├── Quantity: Decimal (Current stock level)
└── Calculated Properties (Pricing, totals)

StockHistoryItem (Audit Trail)
├── Document: Document (Many-to-One)
├── DocumentItem: DocumentItem (Many-to-One)
├── Product: Product (via DocumentItem)
├── Warehouse: Warehouse (via Document)
├── Quantity: Decimal (Transaction quantity)
├── ChangeQuantity: Decimal (Calculated impact)
├── QuantityInStock: Decimal (Stock after transaction)
├── PreviousStockQuantity: Decimal (Stock before transaction)
└── CostPrice: Decimal (Cost at time of transaction)
```

#### 3. Document Flow Relationships
```csharp
Document
├── Id (Primary Key)
├── DocumentType: DocumentType (Many-to-One)
├── Warehouse: Warehouse (Many-to-One)
├── Customer: Customer (Many-to-One, Optional)
├── User: User (Many-to-One)
├── CashRegister: CashRegister (Many-to-One)
├── Items: List<DocumentItem> (One-to-Many)
├── Payments: List<Payment> (One-to-Many)
└── StockDate: DateTime (For stock valuation)

DocumentItem
├── Id (Primary Key)
├── Product: Product (Many-to-One)
├── Package: Package (Many-to-One, Optional)
├── Quantity: Decimal (Sale/purchase quantity)
├── ExpectedQuantity: Decimal (For inventory counts)
├── UOMQuantity: Decimal (Calculated: Quantity * Package.Quantity)
├── Taxes: TaxList (Inherited from Product, can be overridden)
└── Discount: Discount (Item-level discounts)

DocumentType
├── Id (Primary Key)
├── Code: string (Unique identifier)
├── StockDirection: StockDirection (In/Out/None)
├── EditorType: EditorType (Standard/Inventory/LossAndDamage)
└── Warehouse: Warehouse (Default warehouse)
```

#### 4. Supporting Entity Relationships
```csharp
Warehouse
├── Id (Primary Key)
├── Name: string
└── Treasury: Treasury (Financial tracking)

ProductGroup : PosItem
├── Id (Primary Key)
├── Items: List<PosItem> (Hierarchical children)
└── Parent: ProductGroup (Self-referencing hierarchy)

Tax
├── Id (Primary Key)
├── Code: string
├── Rate: Decimal (Percentage or fixed amount)
├── IsFixed: bool (Fixed amount vs percentage)
└── IsTaxOnTotal: bool (Applied to total vs line items)

TaxList : List<Tax>
├── Rate: Decimal (Combined percentage rate)
├── FixedAmount: Decimal (Combined fixed amount)
└── Calculation methods for tax amounts

MeasurementUnit
├── Id (Primary Key)
└── Name: string (e.g., "kg", "pieces", "liters")

Package
├── Id (Primary Key)
├── Name: string (e.g., "Case of 24")
└── Quantity: Decimal (Units per package)

Customer : LegalEntity
├── Id (Primary Key)
├── Code: string
├── Discounts: List<CustomerDiscount>
├── LoyaltyCards: List<string>
├── IsTaxExempt: bool
└── IsEnabled: bool
```

### Database Relationships Summary

#### Primary Stock Tables
1. **Products** - Master product data
2. **StockItems** - Current stock levels per product/warehouse
3. **StockHistory** - All stock movements audit trail
4. **Warehouses** - Storage locations

#### Transaction Tables
1. **Documents** - Transaction headers
2. **DocumentItems** - Transaction line items
3. **DocumentTypes** - Transaction type definitions

#### Reference Tables
1. **ProductGroups** - Product categorization
2. **Taxes** - Tax definitions
3. **MeasurementUnits** - Unit of measure definitions
4. **Packages** - Package size definitions
5. **Customers** - Customer master data

#### Key Foreign Key Relationships
```sql
-- Stock tracking
StockItems.ProductId → Products.Id
StockItems.WarehouseId → Warehouses.Id

-- Stock history
StockHistory.DocumentId → Documents.Id
StockHistory.DocumentItemId → DocumentItems.Id
StockHistory.ProductId → Products.Id (via DocumentItem)
StockHistory.WarehouseId → Warehouses.Id (via Document)

-- Product relationships
Products.GroupId → ProductGroups.Id
Products.MeasurementUnitId → MeasurementUnits.Id
Products.PackageId → Packages.Id

-- Document relationships
Documents.DocumentTypeId → DocumentTypes.Id
Documents.WarehouseId → Warehouses.Id
Documents.CustomerId → Customers.Id
DocumentItems.ProductId → Products.Id
DocumentItems.PackageId → Packages.Id

-- Tax relationships
ProductTaxes.ProductId → Products.Id
ProductTaxes.TaxId → Taxes.Id
DocumentItemTaxes.DocumentItemId → DocumentItems.Id
DocumentItemTaxes.TaxId → Taxes.Id
```

## Enhanced Core Architecture

### 1. Document-Driven Stock Control System
The ChronoPos stock system uses a sophisticated document type system with multi-language support:

```sql
CREATE TABLE `document_types` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `code` varchar(20) UNIQUE NOT NULL,
  `name` varchar(100) NOT NULL,
  `name_ar` varchar(100),  -- Arabic support
  `stock_direction` ENUM('IN', 'OUT', 'NONE') NOT NULL DEFAULT 'NONE',
  `editor_type` ENUM('STANDARD', 'INVENTORY', 'LOSS_AND_DAMAGE') DEFAULT 'STANDARD',
  `is_auto_manufacture_enabled` boolean DEFAULT false,
  `default_warehouse_id` int,
  `status` varchar(20) DEFAULT 'Active'
);
```

### 2. Real-Time Stock Levels with Reservations
```sql
CREATE TABLE `stock_levels` (
  `product_id` int NOT NULL,
  `shop_location_id` int NOT NULL,
  `batch_id` int,
  `current_quantity` decimal(12,4) NOT NULL DEFAULT 0,
  `reserved_quantity` decimal(12,4) DEFAULT 0,
  `available_quantity` decimal(12,4) GENERATED ALWAYS AS (current_quantity - reserved_quantity) STORED,
  `reorder_level` decimal(12,4) DEFAULT 0,
  `max_stock_level` decimal(12,4)
);
```

### 3. Enhanced Domain Models

#### StockLevel (Enhanced StockItem)
The advanced inventory tracking entity with reservations:

```csharp
public class StockLevel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ShopLocationId { get; set; }
    public int? BatchId { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity => CurrentQuantity - ReservedQuantity;
    public decimal ReorderLevel { get; set; }
    public decimal? MaxStockLevel { get; set; }
    public DateTime LastUpdated { get; set; }
    
    // Navigation Properties
    public Product Product { get; set; }
    public ShopLocation ShopLocation { get; set; }
    public ProductBatch Batch { get; set; }
    
    // Calculated Properties
    public bool IsNegativeStock => CurrentQuantity < 0;
    public bool IsLowStock => CurrentQuantity <= ReorderLevel;
    public bool IsOverStock => MaxStockLevel.HasValue && CurrentQuantity > MaxStockLevel;
    public bool HasReservedStock => ReservedQuantity > 0;
}
```

#### StockHistory (Enhanced StockHistoryItem)
Comprehensive audit trail with document integration:

```csharp
public class StockHistory
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public int ShopLocationId { get; set; }
    public int? BatchId { get; set; }
    public int? DocumentId { get; set; }
    public int? DocumentItemId { get; set; }
    public int? DocumentTypeId { get; set; }
    public string MovementType { get; set; } // Purchase, Sale, Transfer, Adjustment, Waste, Return
    public decimal QuantityChanged { get; set; }
    public decimal QuantityBefore { get; set; }
    public decimal QuantityAfter { get; set; }
    public decimal? ExpectedQuantity { get; set; } // For inventory counts
    public bool IsMatchingExpected { get; set; } = true;
    public decimal? CostPrice { get; set; }
    public string ReferenceType { get; set; } // Transaction, Transfer, Adjustment
    public int ReferenceId { get; set; }
    public string Notes { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation Properties
    public Product Product { get; set; }
    public ShopLocation ShopLocation { get; set; }
    public ProductBatch Batch { get; set; }
    public Transaction Document { get; set; }
    public TransactionProduct DocumentItem { get; set; }
    public DocumentType DocumentType { get; set; }
    public User CreatedByUser { get; set; }
    
    // Calculated Properties
    public bool IsStockIncrease => QuantityChanged > 0;
    public bool IsStockDecrease => QuantityChanged < 0;
    public bool IsInventoryVariance => ExpectedQuantity.HasValue && !IsMatchingExpected;
    public decimal? VarianceQuantity => ExpectedQuantity.HasValue ? QuantityAfter - ExpectedQuantity : null;
}
```

#### StockReservation (New)
Prevents overselling with reservation system:

```csharp
public class StockReservation
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ShopLocationId { get; set; }
    public int? CustomerId { get; set; }
    public int? TransactionId { get; set; }
    public decimal ReservedQuantity { get; set; }
    public string ReservationType { get; set; } // SALE, TRANSFER, HOLD
    public DateTime? ExpiresAt { get; set; }
    public string Status { get; set; } // ACTIVE, RELEASED, EXPIRED
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public int? ReleasedBy { get; set; }
    
    // Navigation Properties
    public Product Product { get; set; }
    public ShopLocation ShopLocation { get; set; }
    public Customer Customer { get; set; }
    public Transaction Transaction { get; set; }
    public User CreatedByUser { get; set; }
    public User ReleasedByUser { get; set; }
    
    // Calculated Properties
    public bool IsExpired => ExpiresAt.HasValue && DateTime.Now > ExpiresAt;
    public bool IsActive => Status == "ACTIVE" && !IsExpired;
    public TimeSpan? TimeUntilExpiry => ExpiresAt?.Subtract(DateTime.Now);
}
```

#### StockAlert (New)
Automated stock monitoring and alerts:

```csharp
public class StockAlert
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ShopLocationId { get; set; }
    public string AlertType { get; set; } // LOW_STOCK, OUT_OF_STOCK, OVERSTOCK, EXPIRY_WARNING
    public decimal? CurrentQuantity { get; set; }
    public decimal? ThresholdQuantity { get; set; }
    public string AlertMessage { get; set; }
    public bool IsAcknowledged { get; set; } = false;
    public int? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation Properties
    public Product Product { get; set; }
    public ShopLocation ShopLocation { get; set; }
    public User AcknowledgedByUser { get; set; }
    
    // Calculated Properties
    public bool IsUrgent => AlertType == "OUT_OF_STOCK";
    public TimeSpan Age => DateTime.Now - CreatedAt;
    public bool RequiresAttention => !IsAcknowledged && Age.TotalHours > 24;
}
```

#### Product (Enhanced)
Advanced product entity with comprehensive stock control:

```csharp
public class Product
{
    public int Id { get; set; }
    public string Type { get; set; } // Physical, Digital, Service
    public bool IsService { get; set; } = false; // Services excluded from stock
    public bool TrackStock { get; set; } = true; // Enable/disable stock tracking
    public bool AllowNegativeStock { get; set; } = false; // Per-product negative stock policy
    public int StockValuationMethodId { get; set; } = 1; // FIFO/LIFO/Average
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation Properties
    public ProductInfo ProductInfo { get; set; }
    public StockValuationMethod StockValuationMethod { get; set; }
    public List<StockLevel> StockLevels { get; set; }
    public List<StockHistory> StockHistory { get; set; }
    public List<StockReservation> StockReservations { get; set; }
    public List<StockAlert> StockAlerts { get; set; }
    public List<ProductBatch> Batches { get; set; }
    
    // Calculated Properties
    public bool RequiresStockTracking => !IsService && TrackStock;
    public decimal TotalStockAcrossLocations => StockLevels?.Sum(s => s.CurrentQuantity) ?? 0;
    public decimal TotalReservedStock => StockLevels?.Sum(s => s.ReservedQuantity) ?? 0;
    public decimal TotalAvailableStock => StockLevels?.Sum(s => s.AvailableQuantity) ?? 0;
    public bool HasLowStockAlerts => StockAlerts?.Any(a => a.AlertType == "LOW_STOCK" && !a.IsAcknowledged) ?? false;
}
```

#### DocumentItem
Individual line items that trigger stock movements:

```csharp
public class DocumentItem : TaxableItemBase, ICloneable
{
    public Product Product { get; set; }
    public Package Package { get; set; }
    public Decimal ExpectedQuantity { get; set; }  // For inventory counts
    
    // UOM (Unit of Measure) calculations
    public Decimal UOMQuantity
    {
        get
        {
            Decimal quantity = Quantity;
            Decimal packageQty = Package?.Quantity ?? 1M;
            return quantity * packageQty;
        }
    }
    
    public bool IsManufactured { get; set; }
}
```

#### Document
Container for stock transactions:

```csharp
public class Document : ICloneable, ITaxable
{
    public DocumentType DocumentType { get; set; }
    public Warehouse Warehouse { get; set; }
    public List<DocumentItem> Items { get; set; }
    public DateTime StockDate { get; set; } = DateTime.Now;
    
    // Special document types
    public bool IsInventoryCount => 
        DocumentType?.EditorType == EditorType.Inventory;
    public bool IsLossAndDamage => 
        DocumentType?.EditorType == EditorType.LossAndDamage;
}
```

#### DocumentType
Defines how documents affect stock:

```csharp
public class DocumentType
{
    public string Code { get; set; }
    public string Name { get; set; }
    public StockDirection StockDirection { get; set; }
    public Warehouse Warehouse { get; set; }
    public EditorType EditorType { get; set; }
    
    public static DocumentType Default => new DocumentType()
    {
        StockDirection = StockDirection.Out,
        Code = "200"
    };
}
```

#### EditorType
Defines special handling for different document types:

```csharp
public enum EditorType
{
    Standard,        // Normal sales/purchase documents
    Inventory,       // Stock count/adjustment documents
    LossAndDamage    // Loss and damage documents
}
```

## Enhanced Stock Service Interface

The `IStockService` provides comprehensive stock management with advanced features:

```csharp
public interface IStockService
{
    // Core stock operations
    Task UpdateStockQuantityAsync(int productId, int shopLocationId, decimal quantity, 
        int? batchId = null, string movementType = "Manual", int? documentId = null, 
        int? documentItemId = null, decimal? costPrice = null, string notes = null);
    
    Task<decimal> GetCurrentStockAsync(int productId, int shopLocationId, int? batchId = null);
    Task<decimal> GetAvailableStockAsync(int productId, int shopLocationId, int? batchId = null);
    Task<decimal> GetReservedStockAsync(int productId, int shopLocationId, int? batchId = null);
    
    // Stock levels management
    Task<StockLevel> GetStockLevelAsync(int productId, int shopLocationId, int? batchId = null);
    Task<List<StockLevel>> GetStockLevelsAsync(StockFilter filter);
    Task<bool> CanReduceStockAsync(int productId, int shopLocationId, decimal quantity, int? batchId = null);
    
    // Stock reservations
    Task<StockReservation> ReserveStockAsync(int productId, int shopLocationId, decimal quantity, 
        int? customerId = null, int? transactionId = null, string reservationType = "SALE", 
        DateTime? expiresAt = null);
    Task ReleaseReservationAsync(int reservationId, int releasedBy);
    Task<List<StockReservation>> GetActiveReservationsAsync(int productId, int shopLocationId);
    
    // Stock history and audit
    Task<List<StockHistory>> GetStockHistoryAsync(int productId, int shopLocationId, 
        DateTime? startDate = null, DateTime? endDate = null, int? batchId = null);
    Task<StockHistory> GetLastInventoryCountAsync(int productId, int shopLocationId);
    
    // Stock alerts
    Task<List<StockAlert>> GetActiveAlertsAsync(int? shopLocationId = null);
    Task AcknowledgeAlertAsync(int alertId, int acknowledgedBy);
    Task CheckAndCreateAlertsAsync(int productId, int shopLocationId);
    
    // Inventory count sessions
    Task<StockCountSession> CreateCountSessionAsync(int shopLocationId, string countType, 
        List<int> productIds = null);
    Task<StockCountSession> StartCountSessionAsync(int sessionId, int startedBy);
    Task<StockCountSession> CompleteCountSessionAsync(int sessionId, int completedBy);
    Task UpdateCountItemAsync(int sessionId, int productId, decimal countedQuantity, 
        int countedBy, string notes = null);
    
    // Batch management
    Task<ProductBatch> CreateBatchAsync(int productId, string batchNo, DateTime? manufactureDate, 
        DateTime? expiryDate, decimal quantity, decimal? costPrice);
    Task<List<ProductBatch>> GetExpiringBatchesAsync(int daysAhead = 30);
    
    // Stock valuation
    Task<decimal> GetStockValueAsync(int productId, int shopLocationId, string valuationMethod = "FIFO");
    Task<StockValuationReport> GetStockValuationReportAsync(int shopLocationId, DateTime? asOfDate = null);
    
    // Multi-location operations
    Task TransferStockAsync(int productId, int fromLocationId, int toLocationId, 
        decimal quantity, int? batchId = null, string notes = null);
    Task<List<StockLevel>> GetStockAcrossLocationsAsync(int productId);
    
    // Bulk operations
    Task BulkUpdateStockAsync(List<StockUpdateRequest> updates);
    Task RecalculateStockLevelsAsync(int? productId = null, int? shopLocationId = null);
    
    // Reporting
    Task<StockMovementReport> GetMovementReportAsync(DateTime startDate, DateTime endDate, 
        int? productId = null, int? shopLocationId = null);
    Task<LowStockReport> GetLowStockReportAsync(int? shopLocationId = null);
    Task<StockAgeingReport> GetStockAgeingReportAsync(int shopLocationId);
}
```

## Stock Calculation Examples with Entity Relationships

### Example 1: Simple Product Sale
```csharp
// Product: Beer Bottle
// Package: Individual (Quantity = 1)
// Sale: 5 bottles
// Stock Direction: Out (-1 multiplier)

Product product = new Product 
{
    Id = 1,
    Code = "BEER001",
    Name = "Premium Beer",
    Cost = 2.50M,
    Price = 4.99M,
    Package = new Package { Quantity = 1M },
    IsService = false
};

DocumentItem saleItem = new DocumentItem 
{
    Product = product,
    Quantity = 5M,  // 5 bottles sold
    Package = product.Package
};

// Stock calculation:
Decimal uomQuantity = saleItem.Quantity * saleItem.Package.Quantity; // 5 * 1 = 5
Decimal stockChange = uomQuantity * -1; // -5 (StockDirection.Out)
// Result: Stock decreases by 5 units
```

### Example 2: Package-Based Sale
```csharp
// Product: Beer Bottle (tracked individually)
// Package: Case of 24 bottles
// Sale: 2 cases
// Stock Impact: 48 individual bottles

Product product = new Product 
{
    Id = 1,
    Code = "BEER001",
    MeasurementUnit = new MeasurementUnit { Name = "Bottle" },
    Package = new Package { Name = "Case", Quantity = 24M }
};

DocumentItem saleItem = new DocumentItem 
{
    Product = product,
    Quantity = 2M,  // 2 cases sold
    Package = product.Package
};

// Stock calculation:
Decimal uomQuantity = 2M * 24M; // 48 bottles
Decimal stockChange = 48M * -1; // -48 bottles
// Result: Stock decreases by 48 individual bottles
```

### Example 3: Inventory Count Adjustment
```csharp
// Current Stock: 100 bottles
// Physical Count: 95 bottles
// Expected: 100 bottles
// Adjustment: -5 bottles

Document inventoryDoc = new Document 
{
    DocumentType = new DocumentType 
    {
        EditorType = EditorType.Inventory,
        StockDirection = StockDirection.Out
    },
    IsInventoryCount = true
};

DocumentItem countItem = new DocumentItem 
{
    Product = product,
    Quantity = 95M,        // Actual counted quantity
    ExpectedQuantity = 100M // System expected quantity
};

// Stock calculation for inventory:
Decimal actualQuantity = countItem.Quantity; // 95
Decimal expectedQuantity = countItem.ExpectedQuantity; // 100
Decimal adjustment = actualQuantity - expectedQuantity; // -5
// Result: Stock decreases by 5 units (shortage found)
```

### Example 4: Multi-Tax Product with Customer Discount
```csharp
// Complex product with multiple taxes and customer-specific pricing

TaxList productTaxes = new TaxList 
{
    new Tax { Code = "VAT", Rate = 20M, IsFixed = false }, // 20% VAT
    new Tax { Code = "EXCISE", Rate = 1.50M, IsFixed = true } // $1.50 excise
};

Product product = new Product 
{
    Price = 10.00M,
    Cost = 6.00M,
    Taxes = productTaxes,
    IsTaxInclusivePrice = false
};

Customer customer = new Customer 
{
    Code = "CUST001",
    IsTaxExempt = false,
    Discounts = new List<CustomerDiscount> 
    {
        new CustomerDiscount { Rate = 10M } // 10% customer discount
    }
};

// Price calculations:
Decimal basePrice = 10.00M;
Decimal taxRate = productTaxes.Rate; // 0.20 (20%)
Decimal fixedTax = productTaxes.FixedAmount; // 1.50
Decimal priceAfterTax = basePrice * (1 + taxRate) + fixedTax; // 10.00 * 1.20 + 1.50 = 13.50

// Stock valuation:
StockItem stockItem = new StockItem 
{
    Product = product,
    Quantity = 50M,
    // TotalCost = 50 * 6.00 = 300.00
    // TotalAfterTax = 50 * 13.50 = 675.00
};
```

## Automated Stock Management System

### Database Triggers for Real-Time Updates

ChronoPos uses MySQL triggers for automatic stock management:

```sql
-- Trigger: Auto-update stock levels after movement
CREATE TRIGGER `update_stock_levels_after_movement`
AFTER INSERT ON `stock_movement`
FOR EACH ROW
BEGIN
  DECLARE current_stock DECIMAL(12,4) DEFAULT 0;
  
  -- Get current stock
  SELECT COALESCE(current_quantity, 0) INTO current_stock
  FROM stock_levels 
  WHERE product_id = NEW.product_id 
    AND shop_location_id = NEW.location_id 
    AND (batch_id = NEW.batch_id OR (batch_id IS NULL AND NEW.batch_id IS NULL));
  
  -- Update or insert stock level
  INSERT INTO stock_levels (product_id, shop_location_id, batch_id, current_quantity)
  VALUES (NEW.product_id, NEW.location_id, NEW.batch_id, NEW.quantity)
  ON DUPLICATE KEY UPDATE 
    current_quantity = current_quantity + NEW.quantity,
    last_updated = CURRENT_TIMESTAMP;
    
  -- Create stock history record
  INSERT INTO stock_history (
    product_id, shop_location_id, batch_id, movement_type,
    quantity_changed, quantity_before, quantity_after,
    reference_type, reference_id, created_by
  ) VALUES (
    NEW.product_id, NEW.location_id, NEW.batch_id, NEW.movement_type,
    NEW.quantity, current_stock, current_stock + NEW.quantity,
    NEW.reference_type, NEW.reference_id, NEW.created_by
  );
  
  -- Check for stock alerts
  CALL check_stock_alerts(NEW.product_id, NEW.location_id);
END;

-- Trigger: Auto-create stock movement from transactions
CREATE TRIGGER `create_stock_movement_from_transaction`
AFTER INSERT ON `transaction_products`
FOR EACH ROW
BEGIN
  DECLARE doc_stock_direction VARCHAR(10);
  DECLARE movement_qty DECIMAL(12,4);
  DECLARE is_service_product BOOLEAN DEFAULT FALSE;
  
  -- Check if product is a service
  SELECT is_service INTO is_service_product
  FROM product WHERE id = NEW.product_id;
  
  -- Only process if not a service
  IF NOT is_service_product THEN
    -- Get document stock direction
    SELECT dt.stock_direction INTO doc_stock_direction
    FROM transactions t
    JOIN document_types dt ON t.document_type_id = dt.id
    WHERE t.id = NEW.transaction_id;
    
    -- Calculate movement quantity based on direction
    SET movement_qty = CASE 
      WHEN doc_stock_direction = 'OUT' THEN -NEW.uom_quantity
      WHEN doc_stock_direction = 'IN' THEN NEW.uom_quantity
      ELSE 0
    END;
    
    -- Create stock movement if quantity is not zero
    IF movement_qty != 0 THEN
      INSERT INTO stock_movement (
        product_id, batch_id, uom_id, movement_type, quantity,
        reference_type, reference_id, location_id, document_id, document_item_id,
        previous_quantity, new_quantity, cost_price, created_by
      ) VALUES (
        NEW.product_id, NEW.batch_id, NEW.product_unit_id, 
        CASE WHEN doc_stock_direction = 'OUT' THEN 'Sale' ELSE 'Purchase' END,
        movement_qty, 'Transaction', NEW.transaction_id, NEW.shop_location_id,
        NEW.transaction_id, NEW.id, 0, 0, NEW.cost_price, NEW.created_by
      );
    END IF;
  END IF;
END;
```

### Stored Functions for Stock Validation

```sql
-- Function: Get available stock
CREATE FUNCTION `get_available_stock`(p_product_id INT, p_location_id INT) 
RETURNS DECIMAL(12,4)
READS SQL DATA
DETERMINISTIC
BEGIN
  DECLARE available_qty DECIMAL(12,4) DEFAULT 0;
  
  SELECT COALESCE(available_quantity, 0) INTO available_qty
  FROM stock_levels 
  WHERE product_id = p_product_id AND shop_location_id = p_location_id;
  
  RETURN available_qty;
END;

-- Function: Check if stock reduction is allowed
CREATE FUNCTION `can_reduce_stock`(p_product_id INT, p_location_id INT, p_quantity DECIMAL(12,4))
RETURNS BOOLEAN
READS SQL DATA
DETERMINISTIC
BEGIN
  DECLARE current_stock DECIMAL(12,4) DEFAULT 0;
  DECLARE allow_negative BOOLEAN DEFAULT FALSE;
  
  -- Get current stock and negative stock setting
  SELECT 
    COALESCE(sl.available_quantity, 0),
    COALESCE(p.allow_negative_stock, FALSE)
  INTO current_stock, allow_negative
  FROM product p
  LEFT JOIN stock_levels sl ON p.id = sl.product_id AND sl.shop_location_id = p_location_id
  WHERE p.id = p_product_id;
  
  -- Allow if sufficient stock or negative stock is allowed
  RETURN (current_stock >= p_quantity) OR allow_negative;
END;
```

## Enhanced Stock Update Workflow

### Multi-Language Document Processing

The enhanced stock system processes documents with multi-language support and advanced features:

```csharp
public class UpdateStockQuantitiesTask : DocumentTaskBase
{
    public override void Execute()
    {
        IStockService stockService = AroniumContainer.Resolve<IStockService>();
        
        if (stockService != null && 
            Document.DocumentType.StockDirection != StockDirection.None)
        {
            // Process each document item
            foreach (DocumentItem item in Document.Items
                .Where(x => x.Id != null && !x.Product.IsService))
            {
                ProcessDocumentItem(item, stockService);
            }
            
            // Handle document updates (removed items)
            if (Context.Action == WorkflowAction.Update && 
                ExistingDocument.Items.Count > 0)
            {
                ProcessRemovedItems(stockService);
            }
        }
        
        ChooseTransition();
    }
    
    private void ProcessDocumentItem(DocumentItem item, IStockService stockService)
    {
        // Calculate quantity change based on stock direction
        Decimal quantity = item.UOMQuantity * 
            (Document.DocumentType.StockDirection == StockDirection.Out ? -1 : 1);
        
        // Special handling for inventory counts
        if (Document.IsInventoryCount)
        {
            quantity -= item.ExpectedQuantity;
        }
        
        // Handle different workflow actions
        switch (Context.Action)
        {
            case WorkflowAction.Update:
                HandleUpdateAction(item, ref quantity);
                break;
                
            case WorkflowAction.Delete:
                quantity = -quantity;
                break;
        }
        
        // Apply stock change if non-zero
        if (quantity != 0M)
        {
            stockService.UpdateStockQuantity(
                item.Product, 
                Document.Warehouse, 
                quantity);
        }
    }
    
    private void HandleUpdateAction(DocumentItem item, ref Decimal quantity)
    {
        DocumentItem existingItem = ExistingDocument.Items
            .FirstOrDefault(x => x.Id.Equals(item.Id));
            
        if (existingItem != null)
        {
            if (!Document.IsInventoryCount)
            {
                // Regular document update - subtract previous quantity
                quantity -= existingItem.UOMQuantity * 
                    (Document.DocumentType.StockDirection == StockDirection.Out ? -1 : 1);
            }
            else
            {
                // Inventory count update - recalculate from scratch
                quantity = item.Quantity - existingItem.UOMQuantity * 
                    (Document.DocumentType.StockDirection == StockDirection.Out ? -1 : 1);
            }
        }
    }
    
    private void ProcessRemovedItems(IStockService stockService)
    {
        List<DocumentItem> currentItems = Document.Items;
        
        foreach (DocumentItem removedItem in ExistingDocument.Items
            .Where(x => !x.Product.IsService && 
                       !currentItems.Any(i => x.Id.Equals(i.Id))))
        {
            // Reverse the stock impact of removed items
            Decimal quantity = removedItem.UOMQuantity * 
                (Document.DocumentType.StockDirection == StockDirection.Out ? 1 : -1);
                
            stockService.UpdateStockQuantity(
                removedItem.Product, 
                Document.Warehouse, 
                quantity);
        }
    }
}
```

## Document Workflow Integration

### Document Processing Pipeline

The stock update is integrated into the document processing workflow:

1. **Document Creation/Update** → `CreateDocumentTask`
2. **Stock Quantity Update** → `UpdateStockQuantitiesTask`
3. **Average Price Update** → `UpdateAveragePriceTask`
4. **Manufacturing** → `CreateManufacturesTask` (if applicable)

### Workflow Actions

The system handles three main workflow actions:

#### 1. Create (WorkflowAction.Create)
- New document creation
- Stock quantities adjusted based on document direction
- Inventory counts calculate difference from expected quantity

#### 2. Update (WorkflowAction.Update)
- Existing document modification
- Previous stock impact is reversed
- New stock impact is applied
- Handles added, modified, and removed items

#### 3. Delete (WorkflowAction.Delete)
- Document deletion
- All stock impacts are reversed
- Quantities are restored to pre-document state

## Stock Filtering and Querying

### StockFilter
Provides comprehensive filtering options for stock queries:

```csharp
public class StockFilter
{
    // Product filtering
    public IEnumerable<ProductGroup> ProductGroups { get; set; }
    public string Barcode { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Wildcard { get; set; }
    
    // Quantity filtering
    public bool NegativeQuantities { get; set; }
    public bool NonZeroQuantities { get; set; }
    public bool ZeroQuantities { get; set; }
    
    // Helper properties
    public bool HasGroups => ProductGroups?.Any() == true;
}
```

## Special Document Types

### 1. Inventory Count Documents
- **Purpose**: Physical stock counting and adjustment
- **EditorType**: `Inventory`
- **Stock Calculation**: `ActualQuantity - ExpectedQuantity`
- **Key Features**:
  - Uses `ExpectedQuantity` field for comparison
  - Calculates variance automatically
  - Can handle both positive and negative adjustments

### 2. Loss and Damage Documents
- **Purpose**: Recording stock losses, damages, theft
- **EditorType**: `LossAndDamage`
- **Stock Direction**: Typically `Out`
- **Key Features**:
  - Reduces stock quantities
  - Tracks reasons for losses
  - Maintains audit trail

### 3. Standard Sales/Purchase Documents
- **Purpose**: Regular business transactions
- **EditorType**: `Standard`
- **Stock Direction**: `Out` for sales, `In` for purchases
- **Key Features**:
  - Standard quantity calculations
  - Package/UOM conversions
  - Tax and pricing calculations

## Package and UOM Handling

### Unit of Measure Conversion
The system handles complex package structures:

```csharp
// Example: Selling by case but tracking by individual units
// Product: Beer bottles
// Package: 24 bottles per case
// Sale: 2 cases
// Stock Impact: 48 individual bottles

public Decimal UOMQuantity
{
    get
    {
        Decimal saleQuantity = 2M;        // 2 cases
        Decimal packageQuantity = 24M;    // 24 bottles per case
        return saleQuantity * packageQuantity; // 48 bottles
    }
}
```

### Package Structure
```csharp
public class Package
{
    public string Name { get; set; }
    public Decimal Quantity { get; set; }  // Units per package
    public MeasurementUnit MeasurementUnit { get; set; }
}
```

## Error Handling and Validation

### Stock Validation Points

1. **Service Exclusion**: Services (`Product.IsService = true`) are excluded from stock calculations
2. **Null Checks**: Items without IDs are skipped
3. **Zero Quantity**: Zero quantity changes are ignored
4. **Negative Stock**: System tracks but allows negative stock with warnings

### Exception Handling
```csharp
try
{
    stockService.UpdateStockQuantity(product, warehouse, quantity);
}
catch (Exception ex)
{
    logger.Error($"Stock update failed for product {product.Code}", ex);
    // Handle gracefully - don't break document processing
}
```

## Performance Considerations

### Batch Processing
- Stock updates are processed in batches per document
- Single transaction per document ensures consistency
- Workflow engine manages task sequencing

### Caching Strategy
- Product information cached during document processing
- Warehouse data cached at session level
- Stock quantities retrieved on-demand

### Database Optimization
- Indexed queries on Product ID + Warehouse ID
- Partitioned stock history by date ranges
- Optimized UOM calculations

## Entity Relationship Impact on Stock Operations

### 1. Product Hierarchy Impact
```csharp
// ProductGroup filtering affects stock queries
public IEnumerable<StockItem> GetStockByGroup(ProductGroup group)
{
    var allProducts = group.GetChildren().OfType<Product>();
    return stockService.GetStockItems(new StockFilter 
    {
        ProductGroups = new[] { group }
    });
}

// Hierarchical stock reporting
public StockSummary GetGroupStockSummary(ProductGroup group)
{
    var products = group.GetChildren().OfType<Product>();
    return new StockSummary
    {
        TotalProducts = products.Count(),
        TotalStockValue = products.Sum(p => GetStockValue(p)),
        NegativeStockItems = products.Count(p => HasNegativeStock(p))
    };
}
```

### 2. Warehouse Multi-Location Impact
```csharp
// Stock transfers between warehouses
public void TransferStock(Product product, Warehouse fromWarehouse, 
                         Warehouse toWarehouse, decimal quantity)
{
    // Create transfer document with two items
    var transferDoc = new Document
    {
        DocumentType = new DocumentType { StockDirection = StockDirection.None },
        Items = new List<DocumentItem>
        {
            // Outbound from source warehouse
            new DocumentItem 
            {
                Product = product,
                Quantity = quantity,
                // This will be processed with fromWarehouse context
            }
        }
    };
    
    // Process as two separate stock movements
    stockService.UpdateStockQuantity(product, fromWarehouse, -quantity);
    stockService.UpdateStockQuantity(product, toWarehouse, quantity);
}

// Multi-warehouse stock lookup
public decimal GetTotalStockAcrossWarehouses(Product product)
{
    return warehouses.Sum(w => stockService.GetStockQuantity(product.Id, w.Id));
}
```

### 3. Tax Relationship Impact on Stock Valuation
```csharp
// Tax changes affect stock valuation but not quantities
public void UpdateProductTaxes(Product product, TaxList newTaxes)
{
    var oldTaxes = product.Taxes;
    product.Taxes = newTaxes;
    
    // Stock quantities remain unchanged
    // But stock values are recalculated
    var stockItems = stockService.GetStockItems(new StockFilter 
    {
        Code = product.Code
    });
    
    foreach (var stockItem in stockItems)
    {
        // Recalculate stock values with new tax rates
        var oldValue = CalculateStockValue(stockItem, oldTaxes);
        var newValue = CalculateStockValue(stockItem, newTaxes);
        
        LogStockRevaluation(stockItem, oldValue, newValue);
    }
}

private decimal CalculateStockValue(StockItem stockItem, TaxList taxes)
{
    var baseValue = stockItem.Quantity * stockItem.Product.Cost;
    var taxAmount = TaxHandlerResolver.GetInstance()
        .GetTaxAmount(baseValue, taxes);
    return baseValue + taxAmount;
}
```

### 4. Package/UOM Relationship Impact
```csharp
// Package changes affect UOM calculations
public void UpdateProductPackage(Product product, Package newPackage)
{
    var oldPackage = product.Package;
    product.Package = newPackage;
    
    // Existing stock quantities remain in base units
    // But future transactions use new package size
    
    // Example: Product was sold by "Each" (1 unit)
    // Now changed to "Case" (24 units)
    // Existing stock: 100 units (remains 100)
    // New sale of 1 case = 24 units impact
}

// UOM conversion for reporting
public StockReportItem ConvertStockToDisplayUnits(StockItem stockItem)
{
    var baseQuantity = stockItem.Quantity;
    var packageSize = stockItem.Product.Package?.Quantity ?? 1M;
    
    return new StockReportItem
    {
        Product = stockItem.Product,
        BaseUnits = baseQuantity,
        PackageUnits = baseQuantity / packageSize,
        DisplayText = $"{baseQuantity / packageSize:F2} {stockItem.Product.Package?.Name ?? "units"}"
    };
}
```

### 5. Customer Relationship Impact
```csharp
// Customer-specific stock reservations
public class StockReservation
{
    public Product Product { get; set; }
    public Customer Customer { get; set; }
    public Warehouse Warehouse { get; set; }
    public decimal ReservedQuantity { get; set; }
    public DateTime ReservationDate { get; set; }
}

// Available stock calculation considering reservations
public decimal GetAvailableStock(Product product, Warehouse warehouse, 
                                Customer excludeCustomer = null)
{
    var totalStock = stockService.GetStockQuantity(product.Id, warehouse.Id);
    var reservedStock = GetReservedStock(product, warehouse, excludeCustomer);
    return totalStock - reservedStock;
}

// Customer tax exemption impact on stock valuation
public StockValuation GetStockValuationForCustomer(StockItem stockItem, Customer customer)
{
    var baseValue = stockItem.Quantity * stockItem.Product.Cost;
    
    if (customer?.IsTaxExempt == true)
    {
        return new StockValuation
        {
            BaseValue = baseValue,
            TaxAmount = 0M,
            TotalValue = baseValue
        };
    }
    
    var taxAmount = TaxHandlerResolver.GetInstance()
        .GetTaxAmount(baseValue, stockItem.Product.Taxes);
    
    return new StockValuation
    {
        BaseValue = baseValue,
        TaxAmount = taxAmount,
        TotalValue = baseValue + taxAmount
    };
}
```

### 6. Document Type Relationship Impact
```csharp
// Different document types create different stock impacts
public void ProcessDocumentByType(Document document)
{
    switch (document.DocumentType.EditorType)
    {
        case EditorType.Standard:
            // Normal sales/purchase - use StockDirection
            ProcessStandardDocument(document);
            break;
            
        case EditorType.Inventory:
            // Inventory count - calculate variance
            ProcessInventoryDocument(document);
            break;
            
        case EditorType.LossAndDamage:
            // Loss/damage - always reduces stock
            ProcessLossAndDamageDocument(document);
            break;
    }
}

private void ProcessInventoryDocument(Document document)
{
    foreach (var item in document.Items.Where(x => !x.Product.IsService))
    {
        var currentStock = stockService.GetStockQuantity(
            item.Product.Id, document.Warehouse.Id);
        
        var variance = item.Quantity - item.ExpectedQuantity;
        
        if (variance != 0)
        {
            stockService.UpdateStockQuantity(
                item.Product, document.Warehouse, variance);
                
            // Log inventory adjustment
            LogInventoryAdjustment(item, currentStock, variance);
        }
    }
}
```

### 7. Service vs Product Impact
```csharp
// Services are excluded from stock tracking
public void ProcessDocumentItems(Document document)
{
    var stockImpactItems = document.Items
        .Where(item => !item.Product.IsService && item.Id != null)
        .ToList();
        
    var serviceItems = document.Items
        .Where(item => item.Product.IsService)
        .ToList();
    
    // Only process stock impact items
    foreach (var item in stockImpactItems)
    {
        ProcessStockImpact(item, document);
    }
    
    // Services are processed for revenue but not stock
    foreach (var service in serviceItems)
    {
        ProcessServiceRevenue(service, document);
    }
}
```

## Integration Points

### Event System
Stock changes trigger events throughout the system:

```csharp
// Stock quantity changed
EventAggregator.Instance.GetEvent<StockQuantityChangedEvent>()
    .Publish(new StockQuantityChangedPayload
    {
        Product = product,
        Warehouse = warehouse,
        OldQuantity = oldQuantity,
        NewQuantity = newQuantity,
        ChangeQuantity = quantity
    });
```

### Plugin Architecture
Stock behavior can be extended through plugins:

```csharp
public interface IStockPlugin : IPlugin
{
    void OnBeforeStockUpdate(StockUpdateContext context);
    void OnAfterStockUpdate(StockUpdateContext context);
    bool ValidateStockUpdate(StockUpdateContext context);
}
```

## Implementation Best Practices

### 1. Transaction Management
```csharp
using (var transaction = database.BeginTransaction())
{
    try
    {
        // Update document
        documentService.SaveDocument(document);
        
        // Update stock quantities
        stockService.UpdateStockQuantities(document);
        
        // Update average prices
        priceService.UpdateAveragePrices(document);
        
        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}
```

### 2. Audit Trail Maintenance
```csharp
public void UpdateStockQuantity(Product product, Warehouse warehouse, Decimal quantity)
{
    // Get current stock
    var currentStock = GetStockQuantity(product.Id, warehouse.Id);
    
    // Create history record
    var historyItem = new StockHistoryItem
    {
        Product = product,
        Warehouse = warehouse,
        PreviousStockQuantity = currentStock,
        ChangeQuantity = quantity,
        QuantityInStock = currentStock + quantity,
        Document = currentDocument,
        Timestamp = DateTime.Now
    };
    
    // Save history
    stockHistoryService.SaveHistoryItem(historyItem);
    
    // Update current stock
    UpdateCurrentStock(product.Id, warehouse.Id, quantity);
}
```

### 3. Concurrency Control
```csharp
public void UpdateStockQuantity(Product product, Warehouse warehouse, Decimal quantity)
{
    lock (GetStockLock(product.Id, warehouse.Id))
    {
        // Perform stock update with exclusive lock
        var currentQuantity = GetStockQuantityWithLock(product.Id, warehouse.Id);
        var newQuantity = currentQuantity + quantity;
        SetStockQuantity(product.Id, warehouse.Id, newQuantity);
    }
}
```

## Testing Strategy

### Unit Tests
```csharp
[Test]
public void UpdateStockQuantitiesTask_SalesDocument_DecreasesStock()
{
    // Arrange
    var document = CreateSalesDocument();
    var task = new UpdateStockQuantitiesTask();
    
    // Act
    task.Execute();
    
    // Assert
    var newQuantity = stockService.GetStockQuantity(product.Id, warehouse.Id);
    Assert.AreEqual(expectedQuantity, newQuantity);
}

[Test]
public void UpdateStockQuantitiesTask_InventoryCount_AdjustsToActual()
{
    // Arrange
    var document = CreateInventoryCountDocument();
    
    // Act & Assert
    // Test inventory adjustment logic
}
```

### Integration Tests
```csharp
[Test]
public void DocumentWorkflow_CompleteFlow_UpdatesStockCorrectly()
{
    // Test complete document processing workflow
    // Verify stock updates at each step
    // Ensure consistency across all operations
}
```

## Migration and Upgrade Considerations

### Data Migration
When upgrading stock systems:

1. **Backup existing stock data**
2. **Migrate stock history records**
3. **Recalculate current stock levels**
4. **Validate data integrity**
5. **Update workflow definitions**

### Version Compatibility
- Maintain backward compatibility for existing documents
- Handle legacy document types gracefully
- Provide migration tools for data conversion

## Monitoring and Diagnostics

### Stock Reconciliation
```csharp
public class StockReconciliationService
{
    public StockReconciliationResult ReconcileStock(Product product, Warehouse warehouse)
    {
        var calculatedStock = CalculateStockFromHistory(product, warehouse);
        var currentStock = GetCurrentStock(product, warehouse);
        
        return new StockReconciliationResult
        {
            Product = product,
            Warehouse = warehouse,
            CalculatedQuantity = calculatedStock,
            CurrentQuantity = currentStock,
            Variance = currentStock - calculatedStock,
            IsReconciled = Math.Abs(currentStock - calculatedStock) < 0.001M
        };
    }
}
```

### Performance Monitoring
- Track stock update execution times
- Monitor database query performance
- Alert on stock reconciliation failures
- Log all stock movements for audit

This comprehensive implementation guide provides the foundation for understanding and implementing stock management in the Aronium POS system. The workflow-driven architecture ensures consistency and auditability while providing flexibility for different business scenarios.