# 📦 Stock Adjustment Development Plan - ChronoPos

## 📋 Overview

This document outlines the complete development plan for implementing the **Stock Adjustment** functionality in ChronoPos, as the first module in the comprehensive stock management system. Based on the Figma design analysis and database schema, this plan covers all CRUD operations, UI components, and database interactions.

---

## 🎯 Project Scope

### **Phase 1: Stock Adjustment Core Functionality**
- ✅ **Stock Adjustment List View** - Display all adjustment records with pagination
- ✅ **Add/Edit Stock Adjustment** - Right sidebar panel for item adjustments  
- ✅ **Delete Stock Adjustment** - Remove adjustment records
- ✅ **Stock Movement Tracking** - Complete audit trail
- ✅ **Multi-language Support** - Arabic/English translations
- ✅ **Search & Filter** - Find adjustments by product, date, reason
- ✅ **Reason Management** - Predefined and custom adjustment reasons

### **Future Phases** (Referenced for consistency)
- 🔄 **Stock Transfer** - Between locations/stores
- 📥 **Goods Received** - Incoming inventory from suppliers
- 📤 **Goods Return** - Return inventory to suppliers

---

## 🎨 Figma Design Analysis

### **Stock Management Main Screen**
Based on the attached Figma screenshot:

#### **Top Module Buttons (Already Implemented)**
- ✅ **Stock Adjustment** (125 items) - *Default selected - YELLOW background*
- ⏳ **Stock Transfer** (20 items) - *Future implementation*
- ⏳ **Goods Received** (15 items) - *Future implementation*  
- ⏳ **Goods Replaced** (10 items) - *Future implementation*
- ⏳ **Goods Return** (18 items) - *Future implementation*

#### **Stock Items Table Section** 
- **Section Title**: "Stock Items"
- **Action Button**: "Adjust Stock Item" (Yellow background)
- **Table Features**:
  - ✅ Checkbox column for multi-select
  - ✅ Product image thumbnail
  - ✅ Product name with description
  - ✅ Item ID (e.g., #22314644)
  - ✅ Current stock count
  - ✅ Category name
  - ✅ Reason field
  - ✅ Shop location
  - ✅ Edit/Delete action icons per row
- **Navigation**: ✅ Pagination at bottom
- **Scrolling**: ✅ Scrollable table content

#### **Right Sidebar Panel - "Adjust Product"**
- **Product Icon**: Pizza placeholder with "Change Icon" link
- **Product Name**: Search input with magnifier icon
- **Current Stock**: Read-only display field
- **Adjustment Type**: Dropdown (Increase/Decrease)
- **Quantity**: Numeric input for adjustment amount
- **Reason**: Dropdown/input for adjustment reason
- **Actions**: Cancel (link) and Save (yellow button)

---

## 🗄️ Database Schema Analysis

Based on the comprehensive database schema, the following tables are relevant for Stock Adjustment:

### **Core Tables Used**

#### **1. `stock_adjustment`** - Main adjustment header
```sql
adjustment_id (PK)           -- Unique adjustment record ID
adjustment_no               -- Human-readable adjustment number
adjustment_date             -- Date of adjustment
store_location_id           -- Location where adjustment occurred
reason_id                   -- Link to adjustment reason
status                      -- Pending/Approved/Cancelled
remarks                     -- Additional notes
created_by, created_at      -- Audit fields
updated_by, updated_at      -- Audit fields
```

#### **2. `stock_adjustment_item`** - Individual product adjustments  
```sql
id (PK)                     -- Line item ID
adjustment_id (FK)          -- Link to main adjustment
product_id (FK)             -- Product being adjusted
uom_id (FK)                 -- Unit of measurement
batch_no                    -- Batch tracking (if applicable)
expiry_date                 -- Expiry tracking (if applicable)
quantity_before             -- Stock before adjustment
quantity_after              -- Stock after adjustment  
difference_qty              -- Calculated difference
reason_line                 -- Line-specific reason
remarks_line                -- Line-specific notes
```

#### **3. `stock_adjustment_reasons`** - Predefined reasons
```sql
stock_adjustment_reasons_id (PK)
name                        -- Reason name (e.g., "Damaged", "Theft")
description                 -- Detailed description
status                      -- Active/Inactive
created_by, created_at      -- Audit fields
```

#### **4. `stock_movement`** - Audit trail for all stock changes
```sql
id (PK)                     -- Movement record ID
product_id (FK)             -- Product affected
movement_type               -- 'Adjustment' for our use case
quantity                    -- Quantity changed (+/-)
reference_type              -- 'Adjustment'
reference_id                -- Links to stock_adjustment.adjustment_id
location_id                 -- Store location
notes                       -- Additional notes
created_by, created_at      -- Audit fields
```

### **Supporting Tables**

#### **5. `product` & `product_info`** - Product master data
```sql
-- Product basic info
product.id, product.status, product.type

-- Detailed product information  
product_info.product_name, product_info.sku, product_info.category_id
product_info.supplier_id, product_info.shop_location_id
product_info.reorder_level, product_info.store_location
```

#### **6. `shop_locations`** - Store/location master
```sql
id, shop_id, location_name, can_sell, status
```

#### **7. `uom`** - Units of measurement
```sql  
id, name, abbreviation, conversion_factor
```

---

## 🏗️ Architecture Implementation

### **Domain Layer (ChronoPos.Domain)**

#### **New Entities to Create**

**1. StockAdjustment.cs**
```csharp
public class StockAdjustment
{
    public int AdjustmentId { get; set; }
    public string AdjustmentNo { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public int StoreLocationId { get; set; }
    public int ReasonId { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Cancelled
    public string? Remarks { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public ShopLocation? StoreLocation { get; set; }
    public StockAdjustmentReason? Reason { get; set; }
    public User? Creator { get; set; }
    public ICollection<StockAdjustmentItem> Items { get; set; } = new List<StockAdjustmentItem>();
}
```

**2. StockAdjustmentItem.cs**
```csharp
public class StockAdjustmentItem  
{
    public int Id { get; set; }
    public int AdjustmentId { get; set; }
    public int ProductId { get; set; }
    public int UomId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal QuantityBefore { get; set; }
    public decimal QuantityAfter { get; set; }
    public decimal DifferenceQty { get; set; }
    public string? ReasonLine { get; set; }
    public string? RemarksLine { get; set; }
    
    // Navigation Properties
    public StockAdjustment? Adjustment { get; set; }
    public Product? Product { get; set; }
    public UnitOfMeasurement? Uom { get; set; }
}
```

**3. StockAdjustmentReason.cs**
```csharp
public class StockAdjustmentReason
{
    public int StockAdjustmentReasonsId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Active";
    public int? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation Properties
    public User? Creator { get; set; }
    public ICollection<StockAdjustment> Adjustments { get; set; } = new List<StockAdjustment>();
}
```

**4. StockMovement.cs** 
```csharp
public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? BatchId { get; set; }
    public int UomId { get; set; }
    public string MovementType { get; set; } = string.Empty; // 'Adjustment', 'Sale', 'Purchase', etc.
    public decimal Quantity { get; set; } // Can be positive or negative
    public string ReferenceType { get; set; } = string.Empty; // 'Adjustment', 'Sale', etc. 
    public int ReferenceId { get; set; }
    public int? LocationId { get; set; }
    public string? Notes { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation Properties
    public Product? Product { get; set; }
    public UnitOfMeasurement? Uom { get; set; }
    public ShopLocation? Location { get; set; }
    public User? Creator { get; set; }
}
```

### **Application Layer (ChronoPos.Application)**

#### **DTOs to Create**

**1. StockAdjustmentDto.cs**
```csharp
public class StockAdjustmentDto
{
    public int AdjustmentId { get; set; }
    public string AdjustmentNo { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public int StoreLocationId { get; set; }
    public string StoreLocationName { get; set; } = string.Empty;
    public int ReasonId { get; set; }
    public string ReasonName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<StockAdjustmentItemDto> Items { get; set; } = new();
}
```

**2. StockAdjustmentItemDto.cs**
```csharp
public class StockAdjustmentItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal QuantityBefore { get; set; }
    public decimal QuantityAfter { get; set; }
    public decimal DifferenceQty { get; set; }
    public string AdjustmentType { get; set; } = string.Empty; // "Increase" or "Decrease"
    public string UomName { get; set; } = string.Empty;
    public string? ReasonLine { get; set; }
    public string? RemarksLine { get; set; }
    public string ShopLocation { get; set; } = string.Empty;
}
```

**3. CreateStockAdjustmentDto.cs**
```csharp
public class CreateStockAdjustmentDto
{
    public DateTime AdjustmentDate { get; set; }
    public int StoreLocationId { get; set; }
    public int ReasonId { get; set; }
    public string? Remarks { get; set; }
    public List<CreateStockAdjustmentItemDto> Items { get; set; } = new();
}

public class CreateStockAdjustmentItemDto
{
    public int ProductId { get; set; }
    public int UomId { get; set; }
    public decimal QuantityAfter { get; set; }
    public string? ReasonLine { get; set; }
    public string? RemarksLine { get; set; }
}
```

#### **Service Interfaces**

**1. IStockAdjustmentService.cs**
```csharp
public interface IStockAdjustmentService
{
    Task<PagedResult<StockAdjustmentDto>> GetStockAdjustmentsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? searchTerm = null,
        int? storeLocationId = null,
        int? reasonId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
        
    Task<StockAdjustmentDto?> GetStockAdjustmentByIdAsync(int adjustmentId);
    Task<StockAdjustmentDto> CreateStockAdjustmentAsync(CreateStockAdjustmentDto dto);
    Task<StockAdjustmentDto> UpdateStockAdjustmentAsync(int adjustmentId, CreateStockAdjustmentDto dto);
    Task<bool> DeleteStockAdjustmentAsync(int adjustmentId);
    Task<bool> ApproveStockAdjustmentAsync(int adjustmentId);
    Task<bool> CancelStockAdjustmentAsync(int adjustmentId);
    Task<List<StockAdjustmentReasonDto>> GetAdjustmentReasonsAsync();
    Task<List<ProductStockInfoDto>> GetProductsForAdjustmentAsync(string? searchTerm = null, int? categoryId = null);
}
```

### **Infrastructure Layer (ChronoPos.Infrastructure)**

#### **DbContext Updates**

**Update ChronoPosDbContext.cs**
```csharp
public DbSet<StockAdjustment> StockAdjustments { get; set; }
public DbSet<StockAdjustmentItem> StockAdjustmentItems { get; set; }
public DbSet<StockAdjustmentReason> StockAdjustmentReasons { get; set; }
public DbSet<StockMovement> StockMovements { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Configure StockAdjustment
    modelBuilder.Entity<StockAdjustment>(entity =>
    {
        entity.ToTable("stock_adjustment");
        entity.HasKey(e => e.AdjustmentId);
        entity.Property(e => e.AdjustmentId).HasColumnName("adjustment_id");
        entity.Property(e => e.AdjustmentNo).HasColumnName("adjustment_no").HasMaxLength(30);
        entity.Property(e => e.AdjustmentDate).HasColumnName("adjustment_date");
        entity.Property(e => e.StoreLocationId).HasColumnName("store_location_id");
        entity.Property(e => e.ReasonId).HasColumnName("reason_id");
        entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
        entity.Property(e => e.Remarks).HasColumnName("remarks");
        entity.Property(e => e.CreatedBy).HasColumnName("created_by");
        entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        
        // Configure relationships
        entity.HasOne(d => d.StoreLocation)
            .WithMany()
            .HasForeignKey(d => d.StoreLocationId);
            
        entity.HasOne(d => d.Reason)
            .WithMany(p => p.Adjustments)
            .HasForeignKey(d => d.ReasonId);
    });
    
    // Configure StockAdjustmentItem
    modelBuilder.Entity<StockAdjustmentItem>(entity =>
    {
        entity.ToTable("stock_adjustment_item");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.AdjustmentId).HasColumnName("adjustment_id");
        entity.Property(e => e.ProductId).HasColumnName("product_id");
        entity.Property(e => e.UomId).HasColumnName("uom_id");
        entity.Property(e => e.BatchNo).HasColumnName("batch_no").HasMaxLength(50);
        entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
        entity.Property(e => e.QuantityBefore).HasColumnName("quantity_before").HasPrecision(10, 3);
        entity.Property(e => e.QuantityAfter).HasColumnName("quantity_after").HasPrecision(10, 3);
        entity.Property(e => e.DifferenceQty).HasColumnName("difference_qty").HasPrecision(10, 3);
        entity.Property(e => e.ReasonLine).HasColumnName("reason_line").HasMaxLength(100);
        entity.Property(e => e.RemarksLine).HasColumnName("remarks_line");
        
        // Configure relationships
        entity.HasOne(d => d.Adjustment)
            .WithMany(p => p.Items)
            .HasForeignKey(d => d.AdjustmentId);
            
        entity.HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.ProductId);
    });
    
    // Configure StockAdjustmentReason  
    modelBuilder.Entity<StockAdjustmentReason>(entity =>
    {
        entity.ToTable("stock_adjustment_reasons");
        entity.HasKey(e => e.StockAdjustmentReasonsId);
        entity.Property(e => e.StockAdjustmentReasonsId).HasColumnName("stock_adjustment_reasons_id");
        entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
        entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255);
        entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(255);
        entity.Property(e => e.CreatedBy).HasColumnName("created_by");
        entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
    });
    
    // Configure StockMovement
    modelBuilder.Entity<StockMovement>(entity =>
    {
        entity.ToTable("stock_movement");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.ProductId).HasColumnName("product_id");
        entity.Property(e => e.BatchId).HasColumnName("batch_id");
        entity.Property(e => e.UomId).HasColumnName("uom_id");
        entity.Property(e => e.MovementType).HasColumnName("movement_type").HasMaxLength(20);
        entity.Property(e => e.Quantity).HasColumnName("quantity").HasPrecision(12, 4);
        entity.Property(e => e.ReferenceType).HasColumnName("reference_type").HasMaxLength(50);
        entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
        entity.Property(e => e.LocationId).HasColumnName("location_id");
        entity.Property(e => e.Notes).HasColumnName("notes");
        entity.Property(e => e.CreatedBy).HasColumnName("created_by");
        entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        
        // Configure relationships
        entity.HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.ProductId);
            
        entity.HasOne(d => d.Location)
            .WithMany()
            .HasForeignKey(d => d.LocationId);
    });
}
```

---

## 🖥️ Desktop Application Implementation

### **ViewModels**

#### **1. StockAdjustmentListViewModel.cs**
```csharp
public partial class StockAdjustmentListViewModel : ObservableObject, IDisposable
{
    private readonly IStockAdjustmentService _stockAdjustmentService;
    private readonly IDatabaseLocalizationService _localizationService;
    private readonly INavigationService _navigationService;
    
    [ObservableProperty] private ObservableCollection<StockAdjustmentItemDto> _stockItems = new();
    [ObservableProperty] private ObservableCollection<StockAdjustmentDto> _adjustments = new();
    [ObservableProperty] private ObservableCollection<StockAdjustmentReasonDto> _reasons = new();
    [ObservableProperty] private StockAdjustmentItemDto? _selectedItem;
    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private bool _isSidebarOpen = false;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _totalItems = 0;
    
    // Sidebar form properties
    [ObservableProperty] private string _productName = string.Empty;
    [ObservableProperty] private decimal _currentStock = 0;
    [ObservableProperty] private string _adjustmentType = "Increase";
    [ObservableProperty] private decimal _adjustmentQuantity = 0;
    [ObservableProperty] private int _selectedReasonId = 0;
    
    // Commands
    [RelayCommand] private async Task LoadDataAsync();
    [RelayCommand] private async Task SearchAsync();
    [RelayCommand] private void OpenAdjustmentSidebar(StockAdjustmentItemDto item);
    [RelayCommand] private void CloseSidebar();
    [RelayCommand] private async Task SaveAdjustmentAsync();
    [RelayCommand] private async Task DeleteAdjustmentAsync(int adjustmentId);
    [RelayCommand] private async Task NextPageAsync();
    [RelayCommand] private async Task PreviousPageAsync();
    [RelayCommand] private async Task GoToPageAsync(int page);
}
```

#### **2. StockAdjustmentSidebarViewModel.cs**
```csharp
public partial class StockAdjustmentSidebarViewModel : ObservableObject
{
    private readonly IStockAdjustmentService _stockAdjustmentService;
    private readonly IDatabaseLocalizationService _localizationService;
    
    [ObservableProperty] private StockAdjustmentItemDto? _currentItem;
    [ObservableProperty] private string _productIconUrl = "/Images/product-placeholder.png";
    [ObservableProperty] private string _productName = string.Empty;
    [ObservableProperty] private decimal _currentStock = 0;
    [ObservableProperty] private string _adjustmentType = "Increase";
    [ObservableProperty] private decimal _adjustmentQuantity = 0;
    [ObservableProperty] private int _selectedReasonId = 0;
    [ObservableProperty] private string _customReason = string.Empty;
    [ObservableProperty] private ObservableCollection<StockAdjustmentReasonDto> _reasons = new();
    
    [RelayCommand] private void ChangeProductIcon();
    [RelayCommand] private async Task SearchProductAsync(string searchTerm);
    [RelayCommand] private async Task SaveAsync();
    [RelayCommand] private void Cancel();
    
    public event EventHandler<StockAdjustmentItemDto>? AdjustmentSaved;
    public event EventHandler? Cancelled;
}
```

### **Views**

#### **1. StockAdjustmentView.xaml**
```xml
<UserControl x:Class="ChronoPos.Desktop.Views.StockAdjustmentView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="clr-namespace:ChronoPos.Desktop.ViewModels"
             d:DataContext="{d:DesignInstance Type=vm:StockAdjustmentListViewModel}">

    <Grid Background="{DynamicResource BackgroundBrush}">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="Auto" />
        </Grid.ColumnDefinitions>
        
        <!-- Main Content Area -->
        <Grid Grid.Column="0" Margin="20">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto" />
                <RowDefinition Height="Auto" />
                <RowDefinition Height="*" />
                <RowDefinition Height="Auto" />
            </Grid.RowDefinitions>
            
            <!-- Header Section -->
            <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,20">
                <TextBlock Text="Stock Items" 
                          Style="{DynamicResource HeaderTextStyle}"
                          VerticalAlignment="Center" />
                <Button Content="Adjust Stock Item"
                       Background="{DynamicResource PrimaryBrush}"
                       Foreground="White"
                       Padding="15,8"
                       Margin="20,0,0,0"
                       Command="{Binding OpenAdjustmentSidebarCommand}"
                       CommandParameter="{Binding SelectedItem}" />
            </StackPanel>
            
            <!-- Search Section -->
            <Grid Grid.Row="1" Margin="0,0,0,15">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="Auto" />
                </Grid.ColumnDefinitions>
                
                <TextBox Grid.Column="0"
                        Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
                        Tag="🔍 Search products..."
                        Style="{DynamicResource SearchTextBoxStyle}" />
                        
                <Button Grid.Column="1"
                       Content="Search"
                       Command="{Binding SearchCommand}"
                       Margin="10,0,0,0"
                       Padding="15,8"
                       Background="{DynamicResource SecondaryBrush}"
                       Foreground="White" />
            </Grid>
            
            <!-- Data Table -->
            <DataGrid Grid.Row="2"
                     ItemsSource="{Binding StockItems}"
                     SelectedItem="{Binding SelectedItem}"
                     AutoGenerateColumns="False"
                     CanUserAddRows="False"
                     CanUserDeleteRows="False"
                     GridLinesVisibility="Horizontal"
                     HeadersVisibility="Column"
                     Style="{DynamicResource DataGridStyle}">
                     
                <DataGrid.Columns>
                    <!-- Checkbox Column -->
                    <DataGridCheckBoxColumn Header="" Width="40" />
                    
                    <!-- Product Image -->
                    <DataGridTemplateColumn Header="Product" Width="80">
                        <DataGridTemplateColumn.CellTemplate>
                            <DataTemplate>
                                <Image Source="{Binding ProductImageUrl, TargetNullValue='/Images/product-placeholder.png'}"
                                      Width="50" Height="50"
                                      Stretch="UniformToFill" />
                            </DataTemplate>
                        </DataGridTemplateColumn.CellTemplate>
                    </DataGridTemplateColumn>
                    
                    <!-- Product Name with Description -->
                    <DataGridTemplateColumn Header="Product Name" Width="250">
                        <DataGridTemplateColumn.CellTemplate>
                            <DataTemplate>
                                <StackPanel>
                                    <TextBlock Text="{Binding ProductName}" 
                                              FontWeight="SemiBold" />
                                    <TextBlock Text="{Binding ProductDescription}" 
                                              FontSize="11"
                                              Foreground="Gray"
                                              TextTrimming="CharacterEllipsis" />
                                </StackPanel>
                            </DataTemplate>
                        </DataGridTemplateColumn.CellTemplate>
                    </DataGridTemplateColumn>
                    
                    <!-- Item ID -->
                    <DataGridTextColumn Header="Item ID" 
                                       Binding="{Binding ProductSku}" 
                                       Width="120" />
                    
                    <!-- Stock -->
                    <DataGridTextColumn Header="Stock" 
                                       Binding="{Binding QuantityBefore, StringFormat='{}{0} items'}" 
                                       Width="100" />
                    
                    <!-- Category -->
                    <DataGridTextColumn Header="Category" 
                                       Binding="{Binding CategoryName}" 
                                       Width="120" />
                    
                    <!-- Reason -->
                    <DataGridTextColumn Header="Reason" 
                                       Binding="{Binding ReasonLine}" 
                                       Width="120" />
                    
                    <!-- Shop Location -->
                    <DataGridTextColumn Header="Shop Location" 
                                       Binding="{Binding ShopLocation}" 
                                       Width="130" />
                    
                    <!-- Actions -->
                    <DataGridTemplateColumn Header="Actions" Width="80">
                        <DataGridTemplateColumn.CellTemplate>
                            <DataTemplate>
                                <StackPanel Orientation="Horizontal" HorizontalAlignment="Center">
                                    <Button Content="✏️" 
                                           Command="{Binding DataContext.OpenAdjustmentSidebarCommand, RelativeSource={RelativeSource AncestorType=DataGrid}}"
                                           CommandParameter="{Binding}"
                                           Style="{DynamicResource IconButtonStyle}"
                                           ToolTip="Edit" />
                                    <Button Content="🗑️" 
                                           Command="{Binding DataContext.DeleteAdjustmentCommand, RelativeSource={RelativeSource AncestorType=DataGrid}}"
                                           CommandParameter="{Binding.AdjustmentId}"
                                           Style="{DynamicResource IconButtonStyle}"
                                           ToolTip="Delete"
                                           Margin="5,0,0,0" />
                                </StackPanel>
                            </DataTemplate>
                        </DataGridTemplateColumn.CellTemplate>
                    </DataGridTemplateColumn>
                </DataGrid.Columns>
            </DataGrid>
            
            <!-- Pagination -->
            <StackPanel Grid.Row="3" 
                       Orientation="Horizontal" 
                       HorizontalAlignment="Center"
                       Margin="0,15,0,0">
                       
                <Button Content="← Previous" 
                       Command="{Binding PreviousPageCommand}"
                       IsEnabled="{Binding CanGoPrevious}"
                       Margin="0,0,10,0" />
                       
                <TextBlock Text="{Binding CurrentPage}" 
                          VerticalAlignment="Center"
                          Margin="10,0" />
                          
                <TextBlock Text="of" 
                          VerticalAlignment="Center"
                          Margin="5,0" />
                          
                <TextBlock Text="{Binding TotalPages}" 
                          VerticalAlignment="Center"
                          Margin="0,0,10,0" />
                          
                <Button Content="Next →" 
                       Command="{Binding NextPageCommand}"
                       IsEnabled="{Binding CanGoNext}"
                       Margin="10,0,0,0" />
            </StackPanel>
        </Grid>
        
        <!-- Right Sidebar Panel -->
        <Border Grid.Column="1"
               Background="{DynamicResource SidebarBackgroundBrush}"
               BorderBrush="{DynamicResource BorderBrush}"
               BorderThickness="1,0,0,0"
               Width="350"
               Visibility="{Binding IsSidebarOpen, Converter={StaticResource BooleanToVisibilityConverter}}">
               
            <ScrollViewer VerticalScrollBarVisibility="Auto">
                <StackPanel Margin="20">
                    
                    <!-- Header -->
                    <TextBlock Text="Adjust Product" 
                              Style="{DynamicResource SidebarHeaderStyle}"
                              Margin="0,0,0,20" />
                    
                    <!-- Product Icon -->
                    <StackPanel Margin="0,0,0,20">
                        <Image Source="{Binding ProductIconUrl}"
                              Width="80" Height="80"
                              Stretch="UniformToFill"
                              HorizontalAlignment="Center" />
                        <TextBlock Text="Change Icon"
                                  HorizontalAlignment="Center"
                                  Foreground="{DynamicResource PrimaryBrush}"
                                  Cursor="Hand"
                                  Margin="0,5,0,0">
                            <TextBlock.InputBindings>
                                <MouseBinding Command="{Binding ChangeProductIconCommand}" MouseAction="LeftClick" />
                            </TextBlock.InputBindings>
                        </TextBlock>
                    </StackPanel>
                    
                    <!-- Product Name Search -->
                    <Label Content="Product Name" Style="{DynamicResource FormLabelStyle}" />
                    <Grid Margin="0,0,0,15">
                        <TextBox Text="{Binding ProductName, UpdateSourceTrigger=PropertyChanged}"
                                Style="{DynamicResource FormTextBoxStyle}" />
                        <TextBlock Text="🔍" 
                                  HorizontalAlignment="Right"
                                  VerticalAlignment="Center"
                                  Margin="0,0,10,0"
                                  Foreground="Gray" />
                    </Grid>
                    
                    <!-- Current Stock (Read-only) -->
                    <Label Content="Current Stock" Style="{DynamicResource FormLabelStyle}" />
                    <TextBox Text="{Binding CurrentStock, StringFormat='{}{0} items'}"
                            IsReadOnly="True"
                            Style="{DynamicResource FormTextBoxStyle}"
                            Background="{DynamicResource DisabledBackgroundBrush}"
                            Margin="0,0,0,15" />
                    
                    <!-- Adjustment Type -->
                    <Label Content="Adjustment Type" Style="{DynamicResource FormLabelStyle}" />
                    <ComboBox SelectedValue="{Binding AdjustmentType}"
                             Style="{DynamicResource FormComboBoxStyle}"
                             Margin="0,0,0,15">
                        <ComboBoxItem Content="Increase" />
                        <ComboBoxItem Content="Decrease" />
                    </ComboBox>
                    
                    <!-- Quantity -->
                    <Label Content="Quantity" Style="{DynamicResource FormLabelStyle}" />
                    <TextBox Text="{Binding AdjustmentQuantity}"
                            Style="{DynamicResource FormTextBoxStyle}"
                            Margin="0,0,0,15" />
                    
                    <!-- Reason -->
                    <Label Content="Reason" Style="{DynamicResource FormLabelStyle}" />
                    <ComboBox ItemsSource="{Binding Reasons}"
                             SelectedValue="{Binding SelectedReasonId}"
                             DisplayMemberPath="Name"
                             SelectedValuePath="Id"
                             Style="{DynamicResource FormComboBoxStyle}"
                             Margin="0,0,0,25" />
                    
                    <!-- Footer Buttons -->
                    <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                        <TextBlock Text="Cancel"
                                  Foreground="{DynamicResource PrimaryBrush}"
                                  VerticalAlignment="Center"
                                  Cursor="Hand"
                                  Margin="0,0,15,0">
                            <TextBlock.InputBindings>
                                <MouseBinding Command="{Binding CancelCommand}" MouseAction="LeftClick" />
                            </TextBlock.InputBindings>
                        </TextBlock>
                        
                        <Button Content="Save"
                               Command="{Binding SaveAdjustmentCommand}"
                               Background="{DynamicResource PrimaryBrush}"
                               Foreground="White"
                               Padding="20,8"
                               MinWidth="80" />
                    </StackPanel>
                </StackPanel>
            </ScrollViewer>
        </Border>
    </Grid>
</UserControl>
```

### **Navigation Integration**

#### **Update StockManagementViewModel.cs**
```csharp
[RelayCommand]
private void NavigateToModule(string moduleType)
{
    switch (moduleType)
    {
        case "StockAdjustment":
            // Navigate to Stock Adjustment screen
            var adjustmentView = new StockAdjustmentView();
            var adjustmentViewModel = _serviceProvider.GetRequiredService<StockAdjustmentListViewModel>();
            adjustmentView.DataContext = adjustmentViewModel;
            adjustmentViewModel.GoBackCommand = new RelayCommand(ShowStockManagement);
            CurrentView = adjustmentView;
            break;
            
        case "StockTransfer":
            // TODO: Future implementation
            System.Diagnostics.Debug.WriteLine("Stock Transfer - Coming Soon");
            break;
            
        case "GoodsReceived":
            // TODO: Future implementation  
            System.Diagnostics.Debug.WriteLine("Goods Received - Coming Soon");
            break;
            
        case "GoodsReturn":
            // TODO: Future implementation
            System.Diagnostics.Debug.WriteLine("Goods Return - Coming Soon");
            break;
            
        default:
            System.Diagnostics.Debug.WriteLine($"Unknown module type: {moduleType}");
            break;
    }
}

private void ShowStockManagement()
{
    // Return to stock management main screen
    var stockManagementView = new StockManagementView();
    stockManagementView.DataContext = this;
    CurrentView = stockManagementView;
}
```

---

## 🌐 Localization & Multi-language Support

### **Translation Keywords to Add**

**Database Keywords (language_keyword table)**
```sql
INSERT INTO language_keyword (key, description) VALUES
('stock.adjustment', 'Stock Adjustment module title'),
('stock.adjustment.title', 'Stock Adjustment page title'),
('stock.adjustment.items', 'Stock Items section title'),
('stock.adjustment.adjust_item', 'Adjust Stock Item button text'),
('stock.adjustment.product', 'Product column header'),
('stock.adjustment.product_name', 'Product Name column header'),
('stock.adjustment.item_id', 'Item ID column header'),
('stock.adjustment.stock', 'Stock column header'),
('stock.adjustment.category', 'Category column header'),
('stock.adjustment.reason', 'Reason column header'),
('stock.adjustment.shop_location', 'Shop Location column header'),
('stock.adjustment.actions', 'Actions column header'),
('stock.adjustment.sidebar.title', 'Adjust Product sidebar title'),
('stock.adjustment.sidebar.product_name', 'Product Name field label'),
('stock.adjustment.sidebar.current_stock', 'Current Stock field label'),
('stock.adjustment.sidebar.adjustment_type', 'Adjustment Type field label'),
('stock.adjustment.sidebar.quantity', 'Quantity field label'),
('stock.adjustment.sidebar.reason', 'Reason field label'),
('stock.adjustment.sidebar.cancel', 'Cancel button text'),
('stock.adjustment.sidebar.save', 'Save button text'),
('stock.adjustment.type.increase', 'Increase adjustment type'),
('stock.adjustment.type.decrease', 'Decrease adjustment type'),
('stock.adjustment.search.placeholder', 'Search products placeholder text'),
('stock.adjustment.pagination.previous', 'Previous page button'),
('stock.adjustment.pagination.next', 'Next page button'),
('stock.adjustment.status.pending', 'Pending status'),
('stock.adjustment.status.approved', 'Approved status'),
('stock.adjustment.status.cancelled', 'Cancelled status');
```

**Translation Values (label_translation table)**
```sql
-- English translations
INSERT INTO label_translation (language_id, translation_key, value) VALUES
(1, 'stock.adjustment', 'Stock Adjustment'),
(1, 'stock.adjustment.title', 'Stock Adjustment Management'),
(1, 'stock.adjustment.items', 'Stock Items'),
(1, 'stock.adjustment.adjust_item', 'Adjust Stock Item'),
(1, 'stock.adjustment.product', 'Product'),
(1, 'stock.adjustment.product_name', 'Product Name'),
(1, 'stock.adjustment.item_id', 'Item ID'),
(1, 'stock.adjustment.stock', 'Stock'),
(1, 'stock.adjustment.category', 'Category'),
(1, 'stock.adjustment.reason', 'Reason'),
(1, 'stock.adjustment.shop_location', 'Shop Location'),
(1, 'stock.adjustment.actions', 'Actions'),
(1, 'stock.adjustment.sidebar.title', 'Adjust Product'),
(1, 'stock.adjustment.sidebar.product_name', 'Product Name'),
(1, 'stock.adjustment.sidebar.current_stock', 'Current Stock'),
(1, 'stock.adjustment.sidebar.adjustment_type', 'Adjustment Type'),
(1, 'stock.adjustment.sidebar.quantity', 'Quantity'),
(1, 'stock.adjustment.sidebar.reason', 'Reason'),
(1, 'stock.adjustment.sidebar.cancel', 'Cancel'),
(1, 'stock.adjustment.sidebar.save', 'Save'),
(1, 'stock.adjustment.type.increase', 'Increase'),
(1, 'stock.adjustment.type.decrease', 'Decrease'),
(1, 'stock.adjustment.search.placeholder', '🔍 Search products...'),
(1, 'stock.adjustment.pagination.previous', '← Previous'),
(1, 'stock.adjustment.pagination.next', 'Next →'),
(1, 'stock.adjustment.status.pending', 'Pending'),
(1, 'stock.adjustment.status.approved', 'Approved'),
(1, 'stock.adjustment.status.cancelled', 'Cancelled');

-- Arabic translations  
INSERT INTO label_translation (language_id, translation_key, value) VALUES
(2, 'stock.adjustment', 'تعديل المخزون'),
(2, 'stock.adjustment.title', 'إدارة تعديل المخزون'),
(2, 'stock.adjustment.items', 'عناصر المخزون'),
(2, 'stock.adjustment.adjust_item', 'تعديل عنصر المخزون'),
(2, 'stock.adjustment.product', 'المنتج'),
(2, 'stock.adjustment.product_name', 'اسم المنتج'),
(2, 'stock.adjustment.item_id', 'رقم العنصر'),
(2, 'stock.adjustment.stock', 'المخزون'),
(2, 'stock.adjustment.category', 'الفئة'),
(2, 'stock.adjustment.reason', 'السبب'),
(2, 'stock.adjustment.shop_location', 'موقع المتجر'),
(2, 'stock.adjustment.actions', 'الإجراءات'),
(2, 'stock.adjustment.sidebar.title', 'تعديل المنتج'),
(2, 'stock.adjustment.sidebar.product_name', 'اسم المنتج'),
(2, 'stock.adjustment.sidebar.current_stock', 'المخزون الحالي'),
(2, 'stock.adjustment.sidebar.adjustment_type', 'نوع التعديل'),
(2, 'stock.adjustment.sidebar.quantity', 'الكمية'),
(2, 'stock.adjustment.sidebar.reason', 'السبب'),
(2, 'stock.adjustment.sidebar.cancel', 'إلغاء'),
(2, 'stock.adjustment.sidebar.save', 'حفظ'),
(2, 'stock.adjustment.type.increase', 'زيادة'),
(2, 'stock.adjustment.type.decrease', 'نقص'),
(2, 'stock.adjustment.search.placeholder', '🔍 البحث عن المنتجات...'),
(2, 'stock.adjustment.pagination.previous', '← السابق'),
(2, 'stock.adjustment.pagination.next', 'التالي →'),
(2, 'stock.adjustment.status.pending', 'في الانتظار'),
(2, 'stock.adjustment.status.approved', 'معتمد'),
(2, 'stock.adjustment.status.cancelled', 'ملغي');
```

---

## 🧪 Testing Strategy

### **Unit Tests**

#### **Domain Layer Tests**
```csharp
[TestClass]
public class StockAdjustmentTests
{
    [TestMethod]
    public void CreateStockAdjustment_ValidData_Success()
    {
        // Arrange
        var adjustment = new StockAdjustment
        {
            AdjustmentDate = DateTime.Today,
            StoreLocationId = 1,
            ReasonId = 1,
            Status = "Pending"
        };
        
        // Act & Assert
        Assert.IsNotNull(adjustment);
        Assert.AreEqual("Pending", adjustment.Status);
        Assert.AreEqual(DateTime.Today, adjustment.AdjustmentDate);
    }
    
    [TestMethod]
    public void CalculateDifferenceQuantity_IncreaseType_CorrectCalculation()
    {
        // Arrange
        var item = new StockAdjustmentItem
        {
            QuantityBefore = 100,
            QuantityAfter = 150
        };
        
        // Act
        item.DifferenceQty = item.QuantityAfter - item.QuantityBefore;
        
        // Assert
        Assert.AreEqual(50, item.DifferenceQty);
    }
}
```

#### **Service Layer Tests**
```csharp
[TestClass]
public class StockAdjustmentServiceTests
{
    private Mock<IStockAdjustmentRepository> _mockRepository;
    private Mock<IStockMovementRepository> _mockMovementRepository;
    private StockAdjustmentService _service;
    
    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IStockAdjustmentRepository>();
        _mockMovementRepository = new Mock<IStockMovementRepository>();
        _service = new StockAdjustmentService(_mockRepository.Object, _mockMovementRepository.Object);
    }
    
    [TestMethod]
    public async Task CreateStockAdjustmentAsync_ValidDto_ReturnsCreatedAdjustment()
    {
        // Arrange
        var dto = new CreateStockAdjustmentDto
        {
            AdjustmentDate = DateTime.Today,
            StoreLocationId = 1,
            ReasonId = 1,
            Items = new List<CreateStockAdjustmentItemDto>
            {
                new CreateStockAdjustmentItemDto
                {
                    ProductId = 1,
                    UomId = 1,
                    QuantityAfter = 150
                }
            }
        };
        
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<StockAdjustment>()))
                      .ReturnsAsync(new StockAdjustment { AdjustmentId = 1 });
        
        // Act
        var result = await _service.CreateStockAdjustmentAsync(dto);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Items.Count);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<StockAdjustment>()), Times.Once);
    }
}
```

### **Integration Tests**
```csharp
[TestClass]
public class StockAdjustmentIntegrationTests : TestBase
{
    [TestMethod]
    public async Task FullStockAdjustmentWorkflow_EndToEnd_Success()
    {
        // Arrange
        using var context = CreateTestDbContext();
        var service = new StockAdjustmentService(
            new StockAdjustmentRepository(context),
            new StockMovementRepository(context));
        
        var createDto = new CreateStockAdjustmentDto
        {
            AdjustmentDate = DateTime.Today,
            StoreLocationId = 1,
            ReasonId = 1,
            Remarks = "Integration test adjustment",
            Items = new List<CreateStockAdjustmentItemDto>
            {
                new CreateStockAdjustmentItemDto
                {
                    ProductId = 1,
                    UomId = 1,
                    QuantityAfter = 125
                }
            }
        };
        
        // Act
        var createdAdjustment = await service.CreateStockAdjustmentAsync(createDto);
        var retrievedAdjustment = await service.GetStockAdjustmentByIdAsync(createdAdjustment.AdjustmentId);
        
        // Assert
        Assert.IsNotNull(retrievedAdjustment);
        Assert.AreEqual(1, retrievedAdjustment.Items.Count);
        Assert.AreEqual("Integration test adjustment", retrievedAdjustment.Remarks);
        
        // Verify stock movement was created
        var movements = await context.StockMovements
            .Where(m => m.ReferenceType == "Adjustment" && m.ReferenceId == createdAdjustment.AdjustmentId)
            .ToListAsync();
        Assert.AreEqual(1, movements.Count);
    }
}
```

---

## 📋 Implementation Checklist

### **Phase 1: Database & Domain Setup**
- [ ] Create migration for new stock tables
- [ ] Implement StockAdjustment entity
- [ ] Implement StockAdjustmentItem entity  
- [ ] Implement StockAdjustmentReason entity
- [ ] Implement StockMovement entity
- [ ] Configure EF Core relationships
- [ ] Seed initial adjustment reasons data

### **Phase 2: Business Logic**
- [ ] Create StockAdjustmentDto classes
- [ ] Implement IStockAdjustmentService interface
- [ ] Implement StockAdjustmentService
- [ ] Create repository interfaces
- [ ] Implement repositories
- [ ] Add validation logic
- [ ] Implement stock movement audit trail

### **Phase 3: Desktop UI**
- [ ] Create StockAdjustmentListViewModel
- [ ] Create StockAdjustmentSidebarViewModel
- [ ] Implement StockAdjustmentView.xaml
- [ ] Create reusable UI controls
- [ ] Implement pagination component
- [ ] Add search functionality
- [ ] Integrate with navigation system

### **Phase 4: Integration & Polish**
- [ ] Update StockManagementViewModel navigation
- [ ] Add localization keywords and translations
- [ ] Implement multi-language support
- [ ] Add loading indicators and error handling
- [ ] Implement data validation
- [ ] Add confirmation dialogs

### **Phase 5: Testing & Documentation**
- [ ] Write unit tests for domain entities
- [ ] Write unit tests for services
- [ ] Write integration tests
- [ ] Write UI tests
- [ ] Update user documentation
- [ ] Create technical documentation

---

## 🚀 Future Enhancements

### **Advanced Features for Later Phases**
1. **Batch/Serial Number Tracking** - For products with lot control
2. **Approval Workflow** - Multi-step approval for large adjustments
3. **Automated Adjustments** - Based on rules and thresholds
4. **Mobile App Support** - Warehouse staff mobile access
5. **Barcode Scanner Integration** - Quick product selection
6. **Audit Reports** - Detailed adjustment history reports
7. **Cost Impact Analysis** - Show financial impact of adjustments
8. **Photo Attachments** - Visual evidence for adjustments
9. **Integration with External Systems** - ERP/WMS integration
10. **AI-Powered Suggestions** - Smart adjustment recommendations

### **Performance Optimizations**
1. **Lazy Loading** - For large product lists
2. **Virtual Scrolling** - For better table performance
3. **Caching Strategy** - Frequently accessed data
4. **Background Processing** - For large bulk adjustments
5. **Database Indexing** - Optimized queries for reporting

---

## 📊 Success Metrics

### **Technical KPIs**
- ✅ Page load time < 2 seconds
- ✅ Search response time < 500ms
- ✅ 99.9% uptime for stock operations
- ✅ Zero data loss during adjustments
- ✅ Complete audit trail for all changes

### **Business KPIs**
- ✅ Reduced stock discrepancy time by 80%
- ✅ Improved inventory accuracy to 99.5%
- ✅ 50% reduction in manual adjustment errors
- ✅ Real-time stock visibility across all locations
- ✅ Compliance with audit requirements

---

**This comprehensive development plan provides the roadmap for implementing a robust, scalable, and user-friendly Stock Adjustment system that aligns perfectly with the ChronoPos architecture and design standards! 🎯**

---

## 📝 Notes

- All database field names follow the existing snake_case convention from the schema
- UI follows the established ChronoPos design patterns and theme system
- Full audit trail maintained for regulatory compliance
- Multi-language support integrated from day one
- Scalable architecture supports future stock management modules
- Complete CRUD operations with proper validation and error handling
