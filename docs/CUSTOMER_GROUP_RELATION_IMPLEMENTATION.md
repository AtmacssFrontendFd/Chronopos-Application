# Customer Group Relation Implementation

## Overview
Complete implementation for the `customers_group_relation` table, following the same structure as Brand implementation.

## Database Table
```sql
CREATE TABLE `customers_group_relation` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `customer_id` int,
  `customer_group_id` int,
  `status` varchar(20),
  `created_by` int,
  `created_at` timestamp,
  `updated_by` int,
  `updated_at` timestamp,
  `deleted_at` timestamp,
  `deleted_by` int
);
```

## Files Created

### 1. Entity
**Location:** `src/ChronoPos.Domain/Entities/CustomerGroupRelation.cs`
- Properties mapped to database columns
- Navigation properties for Customer and CustomerGroup
- Soft delete support (DeletedAt, DeletedBy)

### 2. DTOs
**Location:** `src/ChronoPos.Application/DTOs/CustomerGroupRelationDto.cs`
- `CustomerGroupRelationDto` - Main DTO with display properties
- `CreateCustomerGroupRelationDto` - For creating new relations
- `UpdateCustomerGroupRelationDto` - For updating existing relations

### 3. Repository Interface
**Location:** `src/ChronoPos.Domain/Interfaces/ICustomerGroupRelationRepository.cs`

Methods:
- `GetByCustomerIdAsync(int customerId)` - Get all relations for a customer
- `GetByCustomerGroupIdAsync(int customerGroupId)` - Get all relations for a group
- `GetActiveRelationsAsync()` - Get active relations only
- `RelationExistsAsync(int customerId, int customerGroupId, int? excludeId)` - Check if relation exists
- `GetByCustomerAndGroupAsync(int customerId, int customerGroupId)` - Get specific relation
- `GetAllWithDetailsAsync()` - Get all with navigation properties

### 4. Service Interface
**Location:** `src/ChronoPos.Application/Interfaces/ICustomerGroupRelationService.cs`

Methods:
- `GetAllAsync()` - Get all relations
- `GetActiveRelationsAsync()` - Get active relations
- `GetByIdAsync(int id)` - Get by ID
- `GetByCustomerIdAsync(int customerId)` - Get by customer
- `GetByCustomerGroupIdAsync(int customerGroupId)` - Get by group
- `CreateAsync(CreateCustomerGroupRelationDto)` - Create new relation
- `UpdateAsync(int id, UpdateCustomerGroupRelationDto)` - Update relation
- `DeleteAsync(int id)` - Soft delete relation
- `RelationExistsAsync(int customerId, int customerGroupId, int? excludeId)` - Check existence
- `GetAllWithDetailsAsync()` - Get all with details

### 5. Repository Implementation
**Location:** `src/ChronoPos.Infrastructure/Repositories/CustomerGroupRelationRepository.cs`

Features:
- Includes navigation properties (Customer, CustomerGroup) in queries
- Filters out soft-deleted records (DeletedAt == null)
- Ordered by CreatedAt descending
- Composite index support for quick lookups

### 6. Service Implementation
**Location:** `src/ChronoPos.Application/Services/CustomerGroupRelationService.cs`

Features:
- Validates relation uniqueness before creation/update
- Soft delete implementation
- Maps entities to DTOs with proper navigation properties
- Uses Customer.DisplayName (handles both business and personal customers)
- UnitOfWork pattern for transaction management

## Database Context Updates

**File:** `src/ChronoPos.Infrastructure/ChronoPosDbContext.cs`

### DbSet Added
```csharp
public DbSet<Domain.Entities.CustomerGroupRelation> CustomerGroupRelations { get; set; }
```

### Entity Configuration
```csharp
modelBuilder.Entity<Domain.Entities.CustomerGroupRelation>(entity =>
{
    entity.ToTable("customers_group_relation");
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Status).HasMaxLength(20);
    entity.Property(e => e.CreatedAt).HasColumnType("timestamp");
    entity.Property(e => e.UpdatedAt).HasColumnType("timestamp");
    entity.Property(e => e.DeletedAt).HasColumnType("timestamp");

    // Relationships
    entity.HasOne(e => e.Customer)
          .WithMany()
          .HasForeignKey(e => e.CustomerId)
          .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.CustomerGroup)
          .WithMany()
          .HasForeignKey(e => e.CustomerGroupId)
          .OnDelete(DeleteBehavior.Cascade);

    // Indexes for quick lookup
    entity.HasIndex(e => new { e.CustomerId, e.CustomerGroupId });
    entity.HasIndex(e => e.Status);
});
```

## Dependency Injection

**File:** `src/ChronoPos.Desktop/App.xaml.cs`

### Repository Registration
```csharp
services.AddTransient<ICustomerGroupRelationRepository, CustomerGroupRelationRepository>();
LogMessage("CustomerGroupRelationRepository registered as Transient");
```

### Service Registration
```csharp
services.AddTransient<ICustomerGroupRelationService, CustomerGroupRelationService>();
LogMessage("CustomerGroupRelationService registered as Transient");
```

## Key Features

### 1. Soft Delete
- Uses `DeletedAt` and `DeletedBy` fields
- Soft-deleted records are automatically filtered in queries
- Preserves data integrity and audit trail

### 2. Relationship Management
- Many-to-many relationship between Customers and CustomerGroups
- Validates uniqueness to prevent duplicate relations
- Supports status tracking (Active/Inactive)

### 3. Navigation Properties
- Customer navigation with DisplayName support
- CustomerGroup navigation with Name
- Eager loading with Include() for performance

### 4. Audit Fields
- CreatedBy, CreatedAt - Track who and when created
- UpdatedBy, UpdatedAt - Track who and when updated
- DeletedBy, DeletedAt - Track who and when soft deleted

### 5. Display Properties
- `StatusDisplay` - User-friendly status text
- `CreatedAtFormatted` - Formatted date string
- `UpdatedAtFormatted` - Formatted date string
- `IsActive` - Boolean for active status check

## Usage Examples

### Create a Relation
```csharp
var dto = new CreateCustomerGroupRelationDto
{
    CustomerId = 1,
    CustomerGroupId = 2,
    Status = "Active",
    CreatedBy = currentUserId
};

var created = await _customerGroupRelationService.CreateAsync(dto);
```

### Get Customer's Groups
```csharp
var relations = await _customerGroupRelationService.GetByCustomerIdAsync(customerId);
```

### Get Group's Customers
```csharp
var relations = await _customerGroupRelationService.GetByCustomerGroupIdAsync(groupId);
```

### Update Relation Status
```csharp
var updateDto = new UpdateCustomerGroupRelationDto
{
    Status = "Inactive",
    UpdatedBy = currentUserId
};

await _customerGroupRelationService.UpdateAsync(relationId, updateDto);
```

### Soft Delete
```csharp
var success = await _customerGroupRelationService.DeleteAsync(relationId);
```

## Testing Checklist

- [x] Entity created and mapped
- [x] DTOs created (Main, Create, Update)
- [x] Repository interface defined
- [x] Repository implementation created
- [x] Service interface defined
- [x] Service implementation created
- [x] DbContext updated with DbSet
- [x] Entity configuration added to OnModelCreating
- [x] Repository registered in DI container
- [x] Service registered in DI container
- [x] Build successful

## Next Steps

1. Create database migration if using EF Core migrations
2. Create ViewModel for UI interaction
3. Create View (XAML) for managing relations
4. Add unit tests
5. Add integration tests
6. Update documentation

## Notes

- The implementation follows the same pattern as Brand entity
- All queries filter out soft-deleted records automatically
- Composite index on (CustomerId, CustomerGroupId) ensures fast lookups
- Status index supports quick filtering by status
- Uses Customer.DisplayName to support both business and personal customers
