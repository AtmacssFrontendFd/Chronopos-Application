# Product Grouping System Implementation

## 📋 Overview
Comprehensive implementation of a Product Grouping System that allows flexible grouping, bundling, and management of products for combos, kits, and bundles.

**Implementation Approach**: Step-by-step incremental development with testing after each phase  
**Current Status**: Phase 1 Complete ✅  
**Last Updated**: Phase 1 - Domain & Infrastructure

---

## 🎯 Core Objectives

✅ Allow creation of Product Groups (logical or sellable bundles)  
⏳ Allow linking of multiple products or product units/variants to a group  
⏳ Define quantity, pricing adjustments, and default taxes/discounts  
⏳ Enable automatic SKU generation, stock management, and pricing rules  
⏳ Support both non-inventory groups (combos/kits) and inventory-tracked packs  

---

## ✅ Phase 1: Domain & Infrastructure (COMPLETED)

### 1.1 Domain Entities Created

#### ProductGroup Entity ✅
**File**: `src/ChronoPos.Domain/Entities/ProductGroup.cs`

**Properties**:
- `Id` (int) - Primary key
- `Name` (string, required) - Group name (e.g., "Burger Combo")
- `NameAr` (string, nullable) - Arabic name
- `Description` (string, nullable) - Detailed description
- `DescriptionAr` (string, nullable) - Arabic description
- `DiscountId` (int, nullable) - Optional default discount for all items
- `TaxTypeId` (int, nullable) - Optional default tax rule
- `PriceTypeId` (long, nullable) - Price type reference (MRP, Retail, etc.)
- `SkuPrefix` (string, nullable) - Prefix for auto-generating SKUs
- `Status` (string) - Active/Inactive
- `CreatedDate` (DateTime) - Audit field
- `ModifiedDate` (DateTime, nullable) - Audit field
- `CreatedBy` (int, nullable) - User who created
- `ModifiedBy` (int, nullable) - User who modified
- `IsDeleted` (bool) - Soft delete flag
- `DeletedDate` (DateTime, nullable) - Deletion timestamp
- `DeletedBy` (int, nullable) - User who deleted

**Navigation Properties**:
- `Discount` → Discount entity
- `TaxType` → TaxType entity
- `PriceType` → SellingPriceType entity
- `CreatedByUser` → User entity
- `ModifiedByUser` → User entity
- `DeletedByUser` → User entity
- `ProductGroupItems` → Collection of ProductGroupItem

#### ProductGroupItem Entity ✅
**File**: `src/ChronoPos.Domain/Entities/ProductGroupItem.cs`

**Properties**:
- `Id` (int) - Primary key
- `ProductGroupId` (int) - Foreign key to ProductGroup
- `ProductId` (int) - Foreign key to Product
- `ProductUnitId` (int, nullable) - Optional specific variant/unit
- `Quantity` (decimal) - Quantity of product in group
- `DisplayOrder` (int) - Sort order in group
- `IsRequired` (bool) - Whether item is required
- `PriceAdjustment` (decimal, nullable) - Price override (+ or -)
- `DiscountId` (int, nullable) - Item-specific discount override
- `CreatedDate` (DateTime) - When item was added
- `ModifiedDate` (DateTime, nullable) - When item was modified

**Navigation Properties**:
- `ProductGroup` → ProductGroup entity
- `Product` → Product entity
- `Discount` → Discount entity

### 1.2 Database Configuration ✅

#### DbContext Updates
**File**: `src/ChronoPos.Infrastructure/ChronoPosDbContext.cs`

**DbSets Added**:
```csharp
public DbSet<ProductGroup> ProductGroups { get; set; }
public DbSet<ProductGroupItem> ProductGroupItems { get; set; }
```

**Entity Configuration**:

**ProductGroup**:
- Primary Key: Id
- Required fields: Name (max 200), CreatedDate
- Optional fields with max lengths: NameAr (200), Description (500), DescriptionAr (500), SkuPrefix (20), Status (20)
- Foreign Keys:
  - DiscountId → Discounts (SetNull on delete)
  - TaxTypeId → TaxTypes (SetNull on delete)
  - PriceTypeId → SellingPriceTypes (SetNull on delete)
  - CreatedBy, ModifiedBy, DeletedBy → Users (SetNull on delete)
- Indexes:
  - Name (for quick lookup)
  - Status (for filtering)
  - IsDeleted (for soft delete queries)

**ProductGroupItem**:
- Primary Key: Id
- Required fields: ProductGroupId, ProductId, Quantity (precision 18,3), CreatedDate
- Optional: PriceAdjustment (precision 18,2)
- Defaults: DisplayOrder (0), IsRequired (true)
- Foreign Keys:
  - ProductGroupId → ProductGroups (Cascade delete)
  - ProductId → Products (Restrict delete)
  - DiscountId → Discounts (SetNull on delete)
- Indexes:
  - ProductGroupId (for group lookups)
  - ProductId (for product lookups)

### 1.3 Database Schema

#### product_groups Table
```sql
CREATE TABLE product_groups (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    name_ar TEXT,
    description TEXT,
    description_ar TEXT,
    discount_id INTEGER,
    tax_type_id INTEGER,
    price_type_id INTEGER,
    sku_prefix TEXT,
    status TEXT DEFAULT 'Active',
    created_date TEXT NOT NULL,
    modified_date TEXT,
    created_by INTEGER,
    modified_by INTEGER,
    is_deleted INTEGER DEFAULT 0,
    deleted_date TEXT,
    deleted_by INTEGER,
    FOREIGN KEY (discount_id) REFERENCES discounts(id) ON DELETE SET NULL,
    FOREIGN KEY (tax_type_id) REFERENCES tax_types(id) ON DELETE SET NULL,
    FOREIGN KEY (price_type_id) REFERENCES selling_price_types(id) ON DELETE SET NULL,
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE SET NULL,
    FOREIGN KEY (modified_by) REFERENCES users(id) ON DELETE SET NULL,
    FOREIGN KEY (deleted_by) REFERENCES users(id) ON DELETE SET NULL
);

CREATE INDEX ix_product_groups_name ON product_groups(name);
CREATE INDEX ix_product_groups_status ON product_groups(status);
CREATE INDEX ix_product_groups_is_deleted ON product_groups(is_deleted);
```

#### product_group_items Table
```sql
CREATE TABLE product_group_items (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    product_group_id INTEGER NOT NULL,
    product_id INTEGER NOT NULL,
    product_unit_id INTEGER,
    quantity REAL NOT NULL DEFAULT 1.0,
    display_order INTEGER DEFAULT 0,
    is_required INTEGER DEFAULT 1,
    price_adjustment REAL,
    discount_id INTEGER,
    created_date TEXT NOT NULL,
    modified_date TEXT,
    FOREIGN KEY (product_group_id) REFERENCES product_groups(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    FOREIGN KEY (discount_id) REFERENCES discounts(id) ON DELETE SET NULL
);

CREATE INDEX ix_product_group_items_product_group_id ON product_group_items(product_group_id);
CREATE INDEX ix_product_group_items_product_id ON product_group_items(product_id);
```

---

## 📊 Build Status

✅ **Phase 1 Build**: Successful  
- Domain entities compiled successfully
- Infrastructure configuration compiled successfully
- No errors detected
- Database schema ready for creation

---

## 🔄 Next Steps - Phase 2: Application Layer

### 2.1 DTOs to Create
- [ ] `ProductGroupDto` - For display/listing
- [ ] `CreateProductGroupDto` - For creating new groups
- [ ] `UpdateProductGroupDto` - For updating groups
- [ ] `ProductGroupItemDto` - For group item details
- [ ] `ProductGroupDetailDto` - Full group with items

### 2.2 Service Interface
- [ ] `IProductGroupService` interface with methods:
  - GetAllAsync()
  - GetByIdAsync(int id)
  - GetActiveAsync()
  - CreateAsync(CreateProductGroupDto dto)
  - UpdateAsync(UpdateProductGroupDto dto)
  - DeleteAsync(int id)
  - SearchAsync(string searchTerm)
  - GetGroupItemsAsync(int groupId)
  - AddItemToGroupAsync(int groupId, ProductGroupItemDto item)
  - RemoveItemFromGroupAsync(int itemId)
  - UpdateItemAsync(ProductGroupItemDto item)

### 2.3 Service Implementation
- [ ] `ProductGroupService` class with:
  - CRUD operations
  - Validation logic
  - Business rules
  - Soft delete support

### 2.4 Repository Pattern
- [ ] Update IUnitOfWork with ProductGroups repository
- [ ] Update UnitOfWork implementation
- [ ] Add ProductGroupItems repository if needed

---

## 🎯 Future Phases

### Phase 3: Presentation Layer
- [ ] ProductGroupsViewModel
- [ ] ProductGroupsView (list page)
- [ ] ProductGroupSidePanelViewModel
- [ ] ProductGroupSidePanelControl (Add/Edit form)
- [ ] Navigation integration

### Phase 4: Advanced Features
- [ ] Automatic SKU generation
- [ ] Stock tracking for grouped products
- [ ] Pricing calculation with adjustments
- [ ] Bulk operations
- [ ] Import/Export functionality
- [ ] Reports and analytics

---

## 📝 Design Decisions

### 1. Flexible Discount System
- Group-level discount (applies to all items)
- Item-level discount override (for specific products)
- Supports both percentage and fixed amount discounts

### 2. Price Adjustments
- Item-level price adjustments (PriceAdjustment field)
- Can be positive (markup) or negative (discount)
- Allows fine-grained control over bundle pricing

### 3. Soft Delete Pattern
- Groups can be soft-deleted (IsDeleted flag)
- Maintains data integrity and audit trail
- Allows restoration if needed

### 4. SKU Management
- SkuPrefix for automatic SKU generation
- Future: Auto-generate SKUs for bundled products
- Maintains unique identification

### 5. Hierarchical Structure
- ProductGroup → ProductGroupItem → Product
- One-to-many relationship with cascade delete
- Maintains data consistency

### 6. Audit Trail
- CreatedBy, ModifiedBy, DeletedBy user tracking
- Timestamps for all changes
- Full accountability

---

## ✅ Completed Checklist

- [x] ProductGroup entity created
- [x] ProductGroupItem entity created
- [x] DbContext updated with new DbSets
- [x] Entity configurations added
- [x] Foreign key relationships configured
- [x] Indexes created for performance
- [x] Soft delete support implemented
- [x] Audit fields added
- [x] Build successful with no errors

---

## 🚀 Ready for Phase 2

Domain and Infrastructure layers are complete and tested. Ready to proceed with:
1. Creating DTOs for data transfer
2. Implementing service layer with business logic
3. Setting up repository pattern
4. Registering services in DI container

**Status**: ✅ Phase 1 Complete - Proceed to Phase 2
