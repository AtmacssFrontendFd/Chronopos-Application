using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Enums;

namespace ChronoPos.Infrastructure;

/// <summary>
/// Database context for ChronoPos application using SQLite
/// </summary>
public class ChronoPosDbContext : DbContext, IChronoPosDbContext
{
    public ChronoPosDbContext(DbContextOptions<ChronoPosDbContext> options) : base(options)
    {
    }

    // DbSets for entities
    public DbSet<Domain.Entities.Product> Products { get; set; }
    public DbSet<Domain.Entities.Category> Categories { get; set; }
    public DbSet<Domain.Entities.Customer> Customers { get; set; }
    public DbSet<Domain.Entities.CustomerGroup> CustomerGroups { get; set; }
    public DbSet<Domain.Entities.CustomerGroupRelation> CustomerGroupRelations { get; set; }
    public DbSet<Domain.Entities.BusinessType> BusinessTypes { get; set; }
    public DbSet<Domain.Entities.CustomerAddress> CustomerAddresses { get; set; }
    public DbSet<Domain.Entities.Supplier> Suppliers { get; set; }
    public DbSet<Domain.Entities.Sale> Sales { get; set; }
    public DbSet<Domain.Entities.SaleItem> SaleItems { get; set; }
    
    // Product related entities
    public DbSet<Domain.Entities.ProductBarcode> ProductBarcodes { get; set; }
    public DbSet<Domain.Entities.ProductComment> ProductComments { get; set; }
    public DbSet<Domain.Entities.ProductTax> ProductTaxes { get; set; }
    public DbSet<Domain.Entities.ProductUnit> ProductUnits { get; set; }
    public DbSet<Domain.Entities.ProductBatch> ProductBatches { get; set; }
    public DbSet<Domain.Entities.TaxType> TaxTypes { get; set; }
    public DbSet<Domain.Entities.Brand> Brands { get; set; }
    public DbSet<Domain.Entities.ProductImage> ProductImages { get; set; }
    public DbSet<Domain.Entities.Currency> Currencies { get; set; }
    
    // Stock management entities
    public DbSet<Domain.Entities.StockTransaction> StockTransactions { get; set; }
    public DbSet<Domain.Entities.StockAlert> StockAlerts { get; set; }
    public DbSet<Domain.Entities.Store> Stores { get; set; }
    public DbSet<Domain.Entities.StockLevel> StockLevels { get; set; }
    
    // Language system entities
    public DbSet<Domain.Entities.Language> Languages { get; set; }
    public DbSet<Domain.Entities.LanguageKeyword> LanguageKeywords { get; set; }
    public DbSet<Domain.Entities.LabelTranslation> LabelTranslations { get; set; }
    
    // Stock Management entities
    public DbSet<Domain.Entities.StockAdjustment> StockAdjustments { get; set; }
    public DbSet<Domain.Entities.StockAdjustmentItem> StockAdjustmentItems { get; set; }
    public DbSet<Domain.Entities.StockAdjustmentReason> StockAdjustmentReasons { get; set; }
    public DbSet<Domain.Entities.StockMovement> StockMovements { get; set; }
    public DbSet<Domain.Entities.StockTransfer> StockTransfers { get; set; }
    public DbSet<Domain.Entities.StockTransferItem> StockTransferItems { get; set; }
    public DbSet<Domain.Entities.GoodsReturn> GoodsReturns { get; set; }
    public DbSet<Domain.Entities.GoodsReturnItem> GoodsReturnItems { get; set; }
    public DbSet<Domain.Entities.GoodsReplace> GoodsReplaces { get; set; }
    public DbSet<Domain.Entities.GoodsReplaceItem> GoodsReplaceItems { get; set; }
    
    // Goods Received entities
    public DbSet<Domain.Entities.GoodsReceived> GoodsReceived { get; set; }
    public DbSet<Domain.Entities.GoodsReceivedItem> GoodsReceivedItems { get; set; }
    public DbSet<Domain.Entities.User> Users { get; set; }
    public DbSet<Domain.Entities.ShopLocation> ShopLocations { get; set; }
    public DbSet<Domain.Entities.UnitOfMeasurement> UnitsOfMeasurement { get; set; }
    
    // Discount system entities
    public DbSet<Domain.Entities.Discount> Discounts { get; set; }
    public DbSet<Domain.Entities.ProductDiscount> ProductDiscounts { get; set; }
    public DbSet<Domain.Entities.CategoryDiscount> CategoryDiscounts { get; set; }
    public DbSet<Domain.Entities.CustomerDiscount> CustomerDiscounts { get; set; }
    
    // Selling Price Types
    public DbSet<Domain.Entities.SellingPriceType> SellingPriceTypes { get; set; }

    // Payment Types
    public DbSet<Domain.Entities.PaymentType> PaymentTypes { get; set; }

    // Product Attribute system entities
    public DbSet<ProductAttribute> ProductAttributes { get; set; }
    public DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }
    public DbSet<ProductCombinationItem> ProductCombinationItems { get; set; }

    // Product Grouping system entities
    public DbSet<Domain.Entities.ProductGroup> ProductGroups { get; set; }
    public DbSet<Domain.Entities.ProductGroupItem> ProductGroupItems { get; set; }

    // Permission system entities
    public DbSet<Domain.Entities.Permission> Permissions { get; set; }
    public DbSet<Domain.Entities.Role> Roles { get; set; }
    public DbSet<Domain.Entities.RolePermission> RolePermissions { get; set; }
    public DbSet<Domain.Entities.UserPermissionOverride> UserPermissionOverrides { get; set; }

    // Restaurant management entities
    public DbSet<Domain.Entities.RestaurantTable> RestaurantTables { get; set; }
    public DbSet<Domain.Entities.Reservation> Reservations { get; set; }

    // Order management entities
    public DbSet<Domain.Entities.Order> Orders { get; set; }
    public DbSet<Domain.Entities.OrderItem> OrderItems { get; set; }

    // Product Modifier system entities
    public DbSet<Domain.Entities.ProductModifier> ProductModifiers { get; set; }
    public DbSet<Domain.Entities.ProductModifierGroup> ProductModifierGroups { get; set; }
    public DbSet<Domain.Entities.ProductModifierGroupItem> ProductModifierGroupItems { get; set; }
    public DbSet<Domain.Entities.ProductModifierLink> ProductModifierLinks { get; set; }

    // Transaction & Sales entities
    public DbSet<Domain.Entities.Shift> Shifts { get; set; }
    public DbSet<Domain.Entities.ServiceCharge> ServiceCharges { get; set; }
    public DbSet<Domain.Entities.Transaction> Transactions { get; set; }
    public DbSet<Domain.Entities.TransactionProduct> TransactionProducts { get; set; }
    public DbSet<Domain.Entities.TransactionModifier> TransactionModifiers { get; set; }
    public DbSet<Domain.Entities.TransactionServiceCharge> TransactionServiceCharges { get; set; }
    public DbSet<Domain.Entities.RefundTransaction> RefundTransactions { get; set; }
    public DbSet<Domain.Entities.RefundTransactionProduct> RefundTransactionProducts { get; set; }
    public DbSet<Domain.Entities.ExchangeTransaction> ExchangeTransactions { get; set; }
    public DbSet<Domain.Entities.ExchangeTransactionProduct> ExchangeTransactionProducts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Domain.Entities.Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Cost).HasPrecision(18, 2);
            entity.Property(e => e.LastPurchasePrice).HasPrecision(18, 2);
            entity.Property(e => e.Markup).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.MaxDiscount).HasPrecision(5, 2);
            entity.Property(e => e.TaxInclusivePriceValue).HasPrecision(18, 2);
            entity.Property(e => e.SKU).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.ImagePath).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Index on Code for quick lookup
            entity.HasIndex(e => e.Code).IsUnique();
            
            // Index on SKU for quick lookup
            entity.HasIndex(e => e.SKU).IsUnique();
            
            // Index on PLU for quick lookup
            entity.HasIndex(e => e.PLU).IsUnique();

            // Foreign key relationship with Category
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Foreign key relationship with UnitOfMeasurement
            entity.HasOne(p => p.UnitOfMeasurement)
                  .WithMany()
                  .HasForeignKey(p => p.UnitOfMeasurementId)
                  .OnDelete(DeleteBehavior.Restrict);

        // Foreign key relationship with PurchaseUnit (optional)
        entity.HasOne(p => p.PurchaseUnit)
            .WithMany()
            .HasForeignKey(p => p.PurchaseUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // Foreign key relationship with SellingUnit (optional)
        entity.HasOne(p => p.SellingUnit)
            .WithMany()
            .HasForeignKey(p => p.SellingUnitId)
            .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Category entity
        modelBuilder.Entity<Domain.Entities.Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        // Configure ProductBarcode entity
        modelBuilder.Entity<Domain.Entities.ProductBarcode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Barcode).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BarcodeType).IsRequired().HasMaxLength(20).HasDefaultValue("ean");
            entity.Property(e => e.CreatedAt).IsRequired();

            // Unique constraint on barcode value
            entity.HasIndex(e => e.Barcode).IsUnique();

            // Foreign key relationship with Product
            entity.HasOne(pb => pb.Product)
                  .WithMany(p => p.ProductBarcodes)
                  .HasForeignKey(pb => pb.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Optional foreign key relationship with ProductUnit
            entity.HasOne(pb => pb.ProductUnit)
                  .WithMany()
                  .HasForeignKey(pb => pb.ProductUnitId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Index on ProductId for performance
            entity.HasIndex(e => e.ProductId);
            
            // Index on ProductUnitId for performance
            entity.HasIndex(e => e.ProductUnitId);
            
            // Index on ProductGroupId for performance
            entity.HasIndex(e => e.ProductGroupId);
        });

        // Configure ProductComment entity
        modelBuilder.Entity<Domain.Entities.ProductComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(100);

            // Foreign key relationship with Product
            entity.HasOne(pc => pc.Product)
                  .WithMany(p => p.ProductComments)
                  .HasForeignKey(pc => pc.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TaxType entity
        modelBuilder.Entity<Domain.Entities.TaxType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.Property(e => e.Value).HasPrecision(15, 4);
            entity.Property(e => e.IncludedInPrice).HasDefaultValue(false);
            entity.Property(e => e.AppliesToBuying).HasDefaultValue(false);
            entity.Property(e => e.AppliesToSelling).HasDefaultValue(true);
            entity.Property(e => e.CalculationOrder).HasDefaultValue(1);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Unique constraint on name
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure SellingPriceType entity
        modelBuilder.Entity<Domain.Entities.SellingPriceType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("selling_price_types");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TypeName).IsRequired().HasMaxLength(100).HasColumnName("type_name");
            entity.Property(e => e.ArabicName).IsRequired().HasMaxLength(100).HasColumnName("arabic_name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Status).HasDefaultValue(true).HasColumnName("status");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnName("created_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

            // Unique constraint on type name
            entity.HasIndex(e => e.TypeName).IsUnique();
        });

        // Configure PaymentType entity
        modelBuilder.Entity<Domain.Entities.PaymentType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Payment_Options");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.PaymentCode).IsRequired().HasMaxLength(50).HasColumnName("payment_code");
            entity.Property(e => e.NameAr).HasMaxLength(255).HasColumnName("name_ar");
            entity.Property(e => e.Status).HasDefaultValue(true).HasColumnName("status");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

            // Unique constraints
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.PaymentCode).IsUnique();
        });

        // Configure ProductTax entity
        modelBuilder.Entity<Domain.Entities.ProductTax>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationship with Product
            entity.HasOne(pt => pt.Product)
                  .WithMany(p => p.ProductTaxes)
                  .HasForeignKey(pt => pt.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

        // Foreign key relationship with TaxType
        entity.HasOne(pt => pt.TaxType)
            .WithMany()
            .HasForeignKey(pt => pt.TaxTypeId)
            .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint on Product-TaxType combination
            entity.HasIndex(e => new { e.ProductId, e.TaxTypeId }).IsUnique();
        });

        // Configure Brand entity
        modelBuilder.Entity<Domain.Entities.Brand>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameArabic).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Unique constraint on name
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure Currency entity
        modelBuilder.Entity<Domain.Entities.Currency>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CurrencyName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CurrencyCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
            entity.Property(e => e.ExchangeRate).HasPrecision(10, 4).HasDefaultValue(1.0000m);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Unique constraint on currency code
            entity.HasIndex(e => e.CurrencyCode).IsUnique();
        });

        // Configure ProductImage entity
        modelBuilder.Entity<Domain.Entities.ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd(); // Fix: Tell EF Core that Id is auto-generated
            entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AltText).HasMaxLength(255);
            entity.Property(e => e.SortOrder).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.IsPrimary).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationship with Product
            entity.HasOne(pi => pi.Product)
                  .WithMany(p => p.ProductImages)
                  .HasForeignKey(pi => pi.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Optional foreign key relationship with ProductUnit
            entity.HasOne(pi => pi.ProductUnit)
                  .WithMany()
                  .HasForeignKey(pi => pi.ProductUnitId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Index on ProductId and SortOrder for performance
            entity.HasIndex(e => new { e.ProductId, e.SortOrder });

            // Index on ProductId and IsPrimary for primary image queries
            entity.HasIndex(e => new { e.ProductId, e.IsPrimary });
            
            // Index on ProductUnitId for performance
            entity.HasIndex(e => e.ProductUnitId);
            
            // Index on ProductGroupId for performance
            entity.HasIndex(e => e.ProductGroupId);
        });

        // Configure ProductUnit entity
        modelBuilder.Entity<Domain.Entities.ProductUnit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.UnitId).IsRequired();
            entity.Property(e => e.Sku).HasMaxLength(100).IsRequired();
            entity.Property(e => e.QtyInUnit).IsRequired().HasDefaultValue(1);
            entity.Property(e => e.CostOfUnit).IsRequired().HasPrecision(10, 2);
            entity.Property(e => e.PriceOfUnit).IsRequired().HasPrecision(10, 2);
            entity.Property(e => e.PriceType).HasMaxLength(50).HasDefaultValue("Retail");
            entity.Property(e => e.DiscountAllowed).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.IsBase).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationship with Product
            entity.HasOne(pu => pu.Product)
                  .WithMany(p => p.ProductUnits)
                  .HasForeignKey(pu => pu.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Foreign key relationship with UnitOfMeasurement
            entity.HasOne(pu => pu.Unit)
                  .WithMany()
                  .HasForeignKey(pu => pu.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint on Product-Unit combination (one record per product-unit pair)
            entity.HasIndex(e => new { e.ProductId, e.UnitId }).IsUnique();

            // Index on ProductId for performance
            entity.HasIndex(e => e.ProductId);

            // Index on SKU for uniqueness and performance
            entity.HasIndex(e => e.Sku).IsUnique();

            // Index on IsBase for finding base units quickly
            entity.HasIndex(e => new { e.ProductId, e.IsBase });
        });

        // Configure ProductBatch entity
        modelBuilder.Entity<Domain.Entities.ProductBatch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.BatchNo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ManufactureDate);
            entity.Property(e => e.ExpiryDate);
            entity.Property(e => e.Quantity).IsRequired().HasPrecision(12, 4);
            entity.Property(e => e.UomId).IsRequired();
            entity.Property(e => e.CostPrice).HasPrecision(12, 2);
            entity.Property(e => e.LandedCost).HasPrecision(12, 2);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Foreign key relationships
            entity.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Uom)
                .WithMany()
                .HasForeignKey(d => d.UomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => new { e.ProductId, e.BatchNo }).IsUnique();
            entity.HasIndex(e => e.ExpiryDate);
            entity.HasIndex(e => e.Status);
        });

        // Configure Customer entity
        modelBuilder.Entity<Domain.Entities.Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerFullName).HasMaxLength(150);
            entity.Property(e => e.BusinessFullName).HasMaxLength(150);
            entity.Property(e => e.IsBusiness).IsRequired();
            entity.Property(e => e.LicenseNo).HasMaxLength(50);
            entity.Property(e => e.TrnNo).HasMaxLength(50);
            entity.Property(e => e.MobileNo).IsRequired().HasMaxLength(20);
            entity.Property(e => e.HomePhone).HasMaxLength(20);
            entity.Property(e => e.OfficePhone).HasMaxLength(20);
            entity.Property(e => e.ContactMobileNo).HasMaxLength(20);
            entity.Property(e => e.OfficialEmail).HasMaxLength(100);
            entity.Property(e => e.KeyContactName).HasMaxLength(150);
            entity.Property(e => e.KeyContactMobile).HasMaxLength(20);
            entity.Property(e => e.KeyContactEmail).HasMaxLength(100);
            entity.Property(e => e.FinancePersonName).HasMaxLength(150);
            entity.Property(e => e.FinancePersonMobile).HasMaxLength(20);
            entity.Property(e => e.FinancePersonEmail).HasMaxLength(100);
            entity.Property(e => e.CreditReference1Name).HasMaxLength(150);
            entity.Property(e => e.CreditReference2Name).HasMaxLength(150);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Relationships
            entity.HasOne(e => e.BusinessType)
                  .WithMany(bt => bt.Customers)
                  .HasForeignKey(e => e.BusinessTypeId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.CustomerGroup)
                  .WithMany(cg => cg.Customers)
                  .HasForeignKey(e => e.CustomerGroupId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Addresses)
                  .WithOne(a => a.Customer)
                  .HasForeignKey(a => a.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.CustomerDiscounts)
                  .WithOne(cd => cd.Customer)
                  .HasForeignKey(cd => cd.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Index on mobile for quick lookup
            entity.HasIndex(e => e.MobileNo);
            entity.HasIndex(e => e.OfficialEmail);
        });

        // Configure BusinessType entity
        modelBuilder.Entity<Domain.Entities.BusinessType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BusinessTypeName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BusinessTypeNameAr).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        // Configure CustomerGroup entity
        modelBuilder.Entity<Domain.Entities.CustomerGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameAr).HasMaxLength(100);
            entity.Property(e => e.DiscountValue).HasPrecision(18, 2);
            entity.Property(e => e.DiscountMaxValue).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Relationships
            entity.HasOne(e => e.SellingPriceType)
                  .WithMany()
                  .HasForeignKey(e => e.SellingPriceTypeId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Discount)
                  .WithMany()
                  .HasForeignKey(e => e.DiscountId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Index on name for quick lookup
            entity.HasIndex(e => e.Name);
        });

        // Configure CustomerGroupRelation entity
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

            // Index for quick lookup
            entity.HasIndex(e => new { e.CustomerId, e.CustomerGroupId });
            entity.HasIndex(e => e.Status);
        });

        // Configure ProductGroup entity
        modelBuilder.Entity<Domain.Entities.ProductGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameAr).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DescriptionAr).HasMaxLength(500);
            entity.Property(e => e.SkuPrefix).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedDate).IsRequired();

            // Relationships
            entity.HasOne(e => e.Discount)
                  .WithMany()
                  .HasForeignKey(e => e.DiscountId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TaxType)
                  .WithMany()
                  .HasForeignKey(e => e.TaxTypeId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PriceType)
                  .WithMany()
                  .HasForeignKey(e => e.PriceTypeId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ModifiedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.ModifiedBy)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.DeletedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.DeletedBy)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsDeleted);
        });

        // Configure ProductGroupItem entity
        modelBuilder.Entity<Domain.Entities.ProductGroupItem>(entity =>
        {
            entity.ToTable("product_group_items");
            entity.HasKey(e => e.Id);
            
            // Properties with column mapping
            entity.Property(e => e.ProductGroupId).HasColumnName("group_id").IsRequired();
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductUnitId).HasColumnName("product_unit_id");
            entity.Property(e => e.ProductCombinationId).HasColumnName("product_combination_id");
            entity.Property(e => e.Quantity).HasPrecision(12, 4).HasDefaultValue(1).IsRequired();
            entity.Property(e => e.PriceAdjustment).HasPrecision(10, 2).HasDefaultValue(0).IsRequired();
            entity.Property(e => e.DiscountId).HasColumnName("discount_id");
            entity.Property(e => e.TaxTypeId).HasColumnName("tax_type_id");
            entity.Property(e => e.SellingPriceTypeId).HasColumnName("price_type_id");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();

            // Relationships
            entity.HasOne(e => e.ProductGroup)
                  .WithMany(g => g.ProductGroupItems)
                  .HasForeignKey(e => e.ProductGroupId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Discount)
                  .WithMany()
                  .HasForeignKey(e => e.DiscountId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TaxType)
                  .WithMany()
                  .HasForeignKey(e => e.TaxTypeId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.SellingPriceType)
                  .WithMany()
                  .HasForeignKey(e => e.SellingPriceTypeId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.ProductGroupId);
            entity.HasIndex(e => e.ProductId);
        });

        // Configure CustomerAddress entity
        modelBuilder.Entity<Domain.Entities.CustomerAddress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AddressLine1).HasMaxLength(255);
            entity.Property(e => e.AddressLine2).HasMaxLength(255);
            entity.Property(e => e.PoBox).HasMaxLength(50);
            entity.Property(e => e.Area).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Landmark).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });

        // Configure Supplier entity
        modelBuilder.Entity<Domain.Entities.Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId);
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LogoPicture).HasMaxLength(255);
            entity.Property(e => e.LicenseNumber).HasMaxLength(50);
            entity.Property(e => e.OwnerName).HasMaxLength(100);
            entity.Property(e => e.OwnerMobile).HasMaxLength(20);
            entity.Property(e => e.VatTrnNumber).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.AddressLine1).IsRequired().HasMaxLength(255);
            entity.Property(e => e.AddressLine2).HasMaxLength(255);
            entity.Property(e => e.Building).HasMaxLength(100);
            entity.Property(e => e.Area).HasMaxLength(100);
            entity.Property(e => e.PoBox).HasMaxLength(20);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Website).HasMaxLength(100);
            entity.Property(e => e.KeyContactName).HasMaxLength(100);
            entity.Property(e => e.KeyContactMobile).HasMaxLength(20);
            entity.Property(e => e.KeyContactEmail).HasMaxLength(100);
            entity.Property(e => e.Mobile).HasMaxLength(20);
            entity.Property(e => e.LocationLatitude).HasPrecision(10, 8);
            entity.Property(e => e.LocationLongitude).HasPrecision(11, 8);
            entity.Property(e => e.CompanyPhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Gstin).HasMaxLength(20);
            entity.Property(e => e.Pan).HasMaxLength(20);
            entity.Property(e => e.PaymentTerms).HasMaxLength(50);
            entity.Property(e => e.OpeningBalance).HasPrecision(12, 2);
            entity.Property(e => e.BalanceType).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Index on email for quick lookup (if provided)
            entity.HasIndex(e => e.Email);
            
            // Index on company name for quick lookup
            entity.HasIndex(e => e.CompanyName);
            
            // Index on VAT/TRN number for quick lookup
            entity.HasIndex(e => e.VatTrnNumber);
        });

        // Configure Sale entity
        modelBuilder.Entity<Domain.Entities.Sale>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMethod).IsRequired();
            entity.Property(e => e.SaleDate).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationship with Customer (optional)
            entity.HasOne(s => s.Customer)
                  .WithMany(c => c.Sales)
                  .HasForeignKey(s => s.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Index on transaction number for quick lookup
            entity.HasIndex(e => e.TransactionNumber).IsUnique();
        });

        // Configure SaleItem entity
        modelBuilder.Entity<Domain.Entities.SaleItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Ignore(e => e.TotalAmount); // Computed property

            // Foreign key relationships
            entity.HasOne(si => si.Sale)
                  .WithMany(s => s.SaleItems)
                  .HasForeignKey(si => si.SaleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(si => si.Product)
                  .WithMany()
                  .HasForeignKey(si => si.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Language entity
        modelBuilder.Entity<Domain.Entities.Language>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LanguageName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.LanguageCode).IsRequired().HasMaxLength(255);
            entity.Property(e => e.IsRtl).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(255).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();

            // Index on language code for quick lookup
            entity.HasIndex(e => e.LanguageCode).IsUnique();
        });

        // Configure LanguageKeyword entity
        modelBuilder.Entity<Domain.Entities.LanguageKeyword>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);

            // Unique constraint on key
            entity.HasIndex(e => e.Key).IsUnique();
        });

        // Configure LabelTranslation entity
        modelBuilder.Entity<Domain.Entities.LabelTranslation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TranslationKey).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(255).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationship with Language
            entity.HasOne(lt => lt.Language)
                  .WithMany(l => l.LabelTranslations)
                  .HasForeignKey(lt => lt.LanguageId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint on language_id + translation_key
            entity.HasIndex(e => new { e.LanguageId, e.TranslationKey }).IsUnique();
        });

        // Configure StockTransaction entity
        modelBuilder.Entity<Domain.Entities.StockTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MovementType).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.PreviousStock).IsRequired();
            entity.Property(e => e.NewStock).IsRequired();
            entity.Property(e => e.ReferenceNumber).HasMaxLength(200);
            entity.Property(e => e.ReferenceType).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.UnitCost).HasPrecision(18, 2);
            entity.Property(e => e.SupplierName).HasMaxLength(100);
            entity.Property(e => e.BatchNumber).HasMaxLength(50);
            entity.Property(e => e.Created).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(100);

            // Foreign key relationship with Product
            entity.HasOne(st => st.Product)
                  .WithMany(p => p.StockTransactions)
                  .HasForeignKey(st => st.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Foreign key relationship with Store
            entity.HasOne(st => st.Store)
                  .WithMany(s => s.StockTransactions)
                  .HasForeignKey(st => st.StoreId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Index on ProductId and Created for performance
            entity.HasIndex(e => new { e.ProductId, e.Created });
        });

        // Configure StockAlert entity
        modelBuilder.Entity<Domain.Entities.StockAlert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AlertType).IsRequired();
            entity.Property(e => e.Message).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CurrentStock).IsRequired();
            entity.Property(e => e.TriggerLevel).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.IsRead).IsRequired();
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.ReadBy).HasMaxLength(100);

            // Foreign key relationship with Product
            entity.HasOne(sa => sa.Product)
                  .WithMany(p => p.StockAlerts)
                  .HasForeignKey(sa => sa.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Index on ProductId and AlertType for performance
            entity.HasIndex(e => new { e.ProductId, e.AlertType, e.IsActive });
        });

        // Configure Store entity
        modelBuilder.Entity<Domain.Entities.Store>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.ManagerName).HasMaxLength(100);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.IsDefault).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Unique constraint on name
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure StockLevel entity
        modelBuilder.Entity<Domain.Entities.StockLevel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CurrentStock).HasPrecision(18, 3);
            entity.Property(e => e.ReservedStock).HasPrecision(18, 3);
            entity.Property(e => e.AverageCost).HasPrecision(18, 2);
            entity.Property(e => e.LastCost).HasPrecision(18, 2);
            entity.Property(e => e.LastUpdated).IsRequired();

            // Foreign key relationship with Product
            entity.HasOne(sl => sl.Product)
                  .WithMany(p => p.StockLevels)
                  .HasForeignKey(sl => sl.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Foreign key relationship with Store
            entity.HasOne(sl => sl.Store)
                  .WithMany(s => s.StockLevels)
                  .HasForeignKey(sl => sl.StoreId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint on Product-Store combination
            entity.HasIndex(e => new { e.ProductId, e.StoreId }).IsUnique();
        });

        // Configure StockAdjustment entity
        modelBuilder.Entity<Domain.Entities.StockAdjustment>(entity =>
        {
            entity.HasKey(e => e.AdjustmentId);
            entity.Property(e => e.AdjustmentNo).IsRequired().HasMaxLength(30);
            entity.Property(e => e.AdjustmentDate).IsRequired();
            entity.Property(e => e.StoreLocationId).IsRequired();
            entity.Property(e => e.ReasonId).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(d => d.StoreLocation)
                .WithMany()
                .HasForeignKey(d => d.StoreLocationId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(d => d.Reason)
                .WithMany(p => p.Adjustments)
                .HasForeignKey(d => d.ReasonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Creator)
                .WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Updater)
                .WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Index on adjustment number for quick lookup
            entity.HasIndex(e => e.AdjustmentNo).IsUnique();
        });

        // Configure StockAdjustmentItem entity
        modelBuilder.Entity<Domain.Entities.StockAdjustmentItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AdjustmentId).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.UomId).IsRequired();
            entity.Property(e => e.BatchNo).HasMaxLength(50);
            entity.Property(e => e.QuantityBefore).HasPrecision(10, 3);
            entity.Property(e => e.QuantityAfter).HasPrecision(10, 3);
            entity.Property(e => e.DifferenceQty).HasPrecision(10, 3);
            entity.Property(e => e.ConversionFactor).HasPrecision(10, 4).HasDefaultValue(1);
            entity.Property(e => e.ReasonLine).HasMaxLength(100);

            // Foreign key relationships
            entity.HasOne(d => d.Adjustment)
                .WithMany(p => p.Items)
                .HasForeignKey(d => d.AdjustmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Uom)
                .WithMany()
                .HasForeignKey(d => d.UomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure StockAdjustmentReason entity
        modelBuilder.Entity<Domain.Entities.StockAdjustmentReason>(entity =>
        {
            entity.HasKey(e => e.StockAdjustmentReasonsId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(255).HasDefaultValue("Active");

            entity.HasOne(d => d.Creator)
                .WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Updater)
                .WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure StockMovement entity
        modelBuilder.Entity<Domain.Entities.StockMovement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.UomId).IsRequired();
            entity.Property(e => e.MovementType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Quantity).HasPrecision(12, 4);
            entity.Property(e => e.ReferenceType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ReferenceId).IsRequired();
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(d => d.Uom)
                .WithMany()
                .HasForeignKey(d => d.UomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Location)
                .WithMany()
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Creator)
                .WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => new { e.ReferenceType, e.ReferenceId });
        });

        // Configure StockTransfer entity
        modelBuilder.Entity<Domain.Entities.StockTransfer>(entity =>
        {
            entity.HasKey(e => e.TransferId);
            entity.Property(e => e.TransferNo).IsRequired().HasMaxLength(30);
            entity.Property(e => e.TransferDate).IsRequired();
            entity.Property(e => e.FromStoreId).IsRequired();
            entity.Property(e => e.ToStoreId).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne<Domain.Entities.Store>(d => d.FromStore)
                .WithMany()
                .HasForeignKey(d => d.FromStoreId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne<Domain.Entities.Store>(d => d.ToStore)
                .WithMany()
                .HasForeignKey(d => d.ToStoreId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Creator)
                .WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Updater)
                .WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Index on transfer number for quick lookup
            entity.HasIndex(e => e.TransferNo).IsUnique();
        });

        // Configure StockTransferItem entity
        modelBuilder.Entity<Domain.Entities.StockTransferItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransferId).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.UomId).IsRequired();
            entity.Property(e => e.BatchNo).HasMaxLength(50);
            entity.Property(e => e.QuantitySent).HasPrecision(10, 3);
            entity.Property(e => e.QuantityReceived).HasPrecision(10, 3);
            entity.Property(e => e.DamagedQty).HasPrecision(10, 3);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");

            // Foreign key relationships
            entity.HasOne(d => d.Transfer)
                .WithMany(p => p.Items)
                .HasForeignKey(d => d.TransferId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Uom)
                .WithMany()
                .HasForeignKey(d => d.UomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure GoodsReturn entity
        modelBuilder.Entity<Domain.Entities.GoodsReturn>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReturnNo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SupplierId).IsRequired();
            entity.Property(e => e.StoreId).IsRequired();
            entity.Property(e => e.ReferenceGrnId);
            entity.Property(e => e.ReturnDate).IsRequired();
            entity.Property(e => e.TotalAmount).HasPrecision(12, 2).HasDefaultValue(0);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
            entity.Property(e => e.Remarks).HasMaxLength(255);
            entity.Property(e => e.IsTotallyReplaced).HasDefaultValue(false);
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(d => d.Supplier)
                .WithMany()
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Store)
                .WithMany()
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.ReferenceGrn)
                .WithMany()
                .HasForeignKey(d => d.ReferenceGrnId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Creator)
                .WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Index on return number for quick lookup
            entity.HasIndex(e => e.ReturnNo).IsUnique();
        });

        // Configure GoodsReturnItem entity
        modelBuilder.Entity<Domain.Entities.GoodsReturnItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReturnId).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.BatchId);
            entity.Property(e => e.BatchNo).HasMaxLength(50);
            entity.Property(e => e.ExpiryDate);
            entity.Property(e => e.Quantity).HasPrecision(12, 4).IsRequired();
            entity.Property(e => e.UomId).IsRequired();
            entity.Property(e => e.CostPrice).HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.LineTotal).HasPrecision(12, 2);
            entity.Property(e => e.Reason).HasMaxLength(255);
            entity.Property(e => e.AlreadyReplacedQuantity).HasPrecision(12, 4).HasDefaultValue(0);
            entity.Property(e => e.IsTotallyReplaced).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(d => d.Return)
                .WithMany(p => p.Items)
                .HasForeignKey(d => d.ReturnId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Batch)
                .WithMany()
                .HasForeignKey(d => d.BatchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Uom)
                .WithMany()
                .HasForeignKey(d => d.UomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure User entity
        modelBuilder.Entity<Domain.Entities.User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Password).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.PhoneNo).HasMaxLength(20);
            entity.Property(e => e.UaeId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Index on email for quick lookup
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure ShopLocation entity
        modelBuilder.Entity<Domain.Entities.ShopLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ShopId).IsRequired();
            entity.Property(e => e.LocationType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LocationName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AddressLine1).IsRequired().HasMaxLength(255);
            entity.Property(e => e.AddressLine2).HasMaxLength(255);
            entity.Property(e => e.Building).HasMaxLength(100);
            entity.Property(e => e.Area).HasMaxLength(100);
            entity.Property(e => e.PoBox).HasMaxLength(20);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.LandlineNumber).HasMaxLength(20);
            entity.Property(e => e.MobileNumber).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LocationLatitude).HasPrecision(10, 8);
            entity.Property(e => e.LocationLongitude).HasPrecision(11, 8);
        });

        // Configure UnitOfMeasurement entity
        modelBuilder.Entity<Domain.Entities.UnitOfMeasurement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Abbreviation).HasMaxLength(10);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CategoryTitle).HasMaxLength(50);
            entity.Property(e => e.ConversionFactor).HasPrecision(10, 4);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Unique constraint on name and abbreviation
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Abbreviation).IsUnique();

            // Self-referencing relationship for base UOM
            entity.HasOne(u => u.BaseUom)
                  .WithMany(u => u.DerivedUnits)
                  .HasForeignKey(u => u.BaseUomId)
                  .OnDelete(DeleteBehavior.Restrict);

            // User relationships
            entity.HasOne(u => u.Creator)
                  .WithMany()
                  .HasForeignKey(u => u.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(u => u.Updater)
                  .WithMany()
                  .HasForeignKey(u => u.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(u => u.Deleter)
                  .WithMany()
                  .HasForeignKey(u => u.DeletedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.CategoryTitle);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DeletedAt);
        });

        // Configure Discount entity
        modelBuilder.Entity<Domain.Entities.Discount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DiscountName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.DiscountNameAr).HasMaxLength(150);
            entity.Property(e => e.DiscountDescription).HasMaxLength(150);
            entity.Property(e => e.DiscountCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DiscountType).IsRequired();
            entity.Property(e => e.DiscountValue).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.MaxDiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.MinPurchaseAmount).HasPrecision(18, 2);
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();
            entity.Property(e => e.ApplicableOn).IsRequired();
            entity.Property(e => e.Priority).IsRequired();
            entity.Property(e => e.IsStackable).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.CurrencyCode).HasMaxLength(3);

            // Unique constraint on discount code
            entity.HasIndex(e => e.DiscountCode).IsUnique();

            // Foreign key relationships
            entity.HasOne(d => d.Store)
                  .WithMany()
                  .HasForeignKey(d => d.StoreId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(d => d.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.UpdatedByUser)
                  .WithMany()
                  .HasForeignKey(d => d.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.DeletedByUser)
                  .WithMany()
                  .HasForeignKey(d => d.DeletedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => e.DiscountType);
            entity.HasIndex(e => e.ApplicableOn);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.StartDate, e.EndDate });
        });

        // Configure ProductDiscount entity
        modelBuilder.Entity<Domain.Entities.ProductDiscount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("product_discount");
            
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.DiscountsId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Unique constraint to prevent duplicate mappings
            entity.HasIndex(e => new { e.ProductId, e.DiscountsId })
                  .IsUnique()
                  .HasDatabaseName("uq_product_discount");

            // Foreign key relationships
            entity.HasOne(d => d.Product)
                  .WithMany(p => p.ProductDiscounts)
                  .HasForeignKey(d => d.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Discount)
                  .WithMany(d => d.ProductDiscounts)
                  .HasForeignKey(d => d.DiscountsId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(d => d.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.UpdatedByUser)
                  .WithMany()
                  .HasForeignKey(d => d.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.DeletedByUser)
                  .WithMany()
                  .HasForeignKey(d => d.DeletedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.DiscountsId);
            entity.HasIndex(e => e.DeletedAt);
        });

        // Configure CategoryDiscount entity
        modelBuilder.Entity<Domain.Entities.CategoryDiscount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("category_discount");
            
            entity.Property(e => e.CategoryId).IsRequired();
            entity.Property(e => e.DiscountsId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Unique constraint to prevent duplicate mappings
            entity.HasIndex(e => new { e.CategoryId, e.DiscountsId })
                  .IsUnique()
                  .HasDatabaseName("uq_category_discount");

            // Foreign key relationships
            entity.HasOne(d => d.Category)
                  .WithMany(c => c.CategoryDiscounts)
                  .HasForeignKey(d => d.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Discount)
                  .WithMany(d => d.CategoryDiscounts)
                  .HasForeignKey(d => d.DiscountsId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(d => d.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.UpdatedByUser)
                  .WithMany()
                  .HasForeignKey(d => d.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.DeletedByUser)
                  .WithMany()
                  .HasForeignKey(d => d.DeletedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.DiscountsId);
            entity.HasIndex(e => e.DeletedAt);
        });

        // Configure CustomerDiscount entity
        modelBuilder.Entity<Domain.Entities.CustomerDiscount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("customer_discount");
            
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.DiscountsId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Unique constraint to prevent duplicate mappings
            entity.HasIndex(e => new { e.CustomerId, e.DiscountsId })
                  .IsUnique()
                  .HasDatabaseName("uq_customer_discount");

            // Foreign key relationships
            entity.HasOne(d => d.Customer)
                  .WithMany()
                  .HasForeignKey(d => d.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Discount)
                  .WithMany(d => d.CustomerDiscounts)
                  .HasForeignKey(d => d.DiscountsId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.CreatedByUser)
                  .WithMany()
                  .HasForeignKey(d => d.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.UpdatedByUser)
                  .WithMany()
                  .HasForeignKey(d => d.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.DeletedByUser)
                  .WithMany()
                  .HasForeignKey(d => d.DeletedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.DiscountsId);
            entity.HasIndex(e => e.DeletedAt);
        });

        // Configure ProductCombinationItem entity
        modelBuilder.Entity<Domain.Entities.ProductCombinationItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("product_combination_items");

            // Properties
            entity.Property(e => e.ProductUnitId).IsRequired();
            entity.Property(e => e.AttributeValueId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(d => d.ProductUnit)
                  .WithMany()
                  .HasForeignKey(d => d.ProductUnitId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.AttributeValue)
                  .WithMany()
                  .HasForeignKey(d => d.AttributeValueId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.ProductUnitId);
            entity.HasIndex(e => e.AttributeValueId);
            
            // Composite unique index to prevent duplicate combinations
            entity.HasIndex(e => new { e.ProductUnitId, e.AttributeValueId })
                  .IsUnique()
                  .HasDatabaseName("IX_ProductCombinationItem_ProductUnit_AttributeValue");
        });

        // Configure Permission entity
        modelBuilder.Entity<Domain.Entities.Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ScreenName).HasMaxLength(100);
            entity.Property(e => e.TypeMatrix).HasMaxLength(20);
            entity.Property(e => e.IsParent).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Unique constraint on code
            entity.HasIndex(e => e.Code).IsUnique();

            // Self-referencing relationship for parent permission
            entity.HasOne(p => p.ParentPermission)
                  .WithMany(p => p.ChildPermissions)
                  .HasForeignKey(p => p.ParentPermissionId)
                  .OnDelete(DeleteBehavior.Restrict);

            // User relationships
            entity.HasOne(p => p.Creator)
                  .WithMany()
                  .HasForeignKey(p => p.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Updater)
                  .WithMany()
                  .HasForeignKey(p => p.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Deleter)
                  .WithMany()
                  .HasForeignKey(p => p.DeletedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.ScreenName);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsParent);
            entity.HasIndex(e => e.DeletedAt);
        });

        // Configure Role entity
        modelBuilder.Entity<Domain.Entities.Role>(entity =>
        {
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Unique constraint on role name
            entity.HasIndex(e => e.RoleName).IsUnique();

            // User relationships
            entity.HasOne(r => r.Creator)
                  .WithMany()
                  .HasForeignKey(r => r.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Updater)
                  .WithMany()
                  .HasForeignKey(r => r.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Deleter)
                  .WithMany()
                  .HasForeignKey(r => r.DeletedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DeletedAt);
        });

        // Configure RolePermission entity
        modelBuilder.Entity<Domain.Entities.RolePermission>(entity =>
        {
            entity.HasKey(e => e.RolePermissionId);
            entity.Property(e => e.RoleId).IsRequired();
            entity.Property(e => e.PermissionId).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Unique constraint on Role-Permission combination
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();

            // Foreign key relationships
            entity.HasOne(rp => rp.Role)
                  .WithMany(r => r.RolePermissions)
                  .HasForeignKey(rp => rp.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Permission)
                  .WithMany(p => p.RolePermissions)
                  .HasForeignKey(rp => rp.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);

            // User relationships
            entity.HasOne(rp => rp.Creator)
                  .WithMany()
                  .HasForeignKey(rp => rp.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(rp => rp.Updater)
                  .WithMany()
                  .HasForeignKey(rp => rp.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(rp => rp.Deleter)
                  .WithMany()
                  .HasForeignKey(rp => rp.DeletedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.RoleId);
            entity.HasIndex(e => e.PermissionId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DeletedAt);
        });

        // Configure UserPermissionOverride entity
        modelBuilder.Entity<Domain.Entities.UserPermissionOverride>(entity =>
        {
            entity.HasKey(e => e.UserPermissionOverrideId);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.PermissionId).IsRequired();
            entity.Property(e => e.IsAllowed).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.Reason);
            entity.Property(e => e.ValidFrom);
            entity.Property(e => e.ValidTo);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Unique constraint on User-Permission combination
            entity.HasIndex(e => new { e.UserId, e.PermissionId }).IsUnique();

            // Foreign key relationships
            entity.HasOne(upo => upo.User)
                  .WithMany()
                  .HasForeignKey(upo => upo.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(upo => upo.Permission)
                  .WithMany(p => p.UserPermissionOverrides)
                  .HasForeignKey(upo => upo.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);

            // User relationships for audit
            entity.HasOne(upo => upo.Creator)
                  .WithMany()
                  .HasForeignKey(upo => upo.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(upo => upo.Updater)
                  .WithMany()
                  .HasForeignKey(upo => upo.UpdatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(upo => upo.Deleter)
                  .WithMany()
                  .HasForeignKey(upo => upo.DeletedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PermissionId);
            entity.HasIndex(e => e.ValidFrom);
            entity.HasIndex(e => e.ValidTo);
            entity.HasIndex(e => e.DeletedAt);
        });

        // Configure ProductModifier entity
        modelBuilder.Entity<Domain.Entities.ProductModifier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasPrecision(18, 4);
            entity.Property(e => e.Cost).HasPrecision(18, 4);
            entity.Property(e => e.Sku).HasMaxLength(100);
            entity.Property(e => e.Barcode).HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(pm => pm.TaxType)
                  .WithMany()
                  .HasForeignKey(pm => pm.TaxTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(pm => pm.Creator)
                  .WithMany()
                  .HasForeignKey(pm => pm.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.HasIndex(e => e.Barcode).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.TaxTypeId);
        });

        // Configure ProductModifierGroup entity
        modelBuilder.Entity<Domain.Entities.ProductModifierGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.SelectionType).IsRequired().HasMaxLength(50).HasDefaultValue("Multiple");
            entity.Property(e => e.Required).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.MinSelections).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.MaxSelections);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(pmg => pmg.Creator)
                  .WithMany()
                  .HasForeignKey(pmg => pmg.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.SelectionType);
        });

        // Configure ProductModifierGroupItem entity
        modelBuilder.Entity<Domain.Entities.ProductModifierGroupItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PriceAdjustment).HasPrecision(18, 4).HasDefaultValue(0);
            entity.Property(e => e.SortOrder).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.DefaultSelection).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(pmgi => pmgi.Group)
                  .WithMany(pmg => pmg.GroupItems)
                  .HasForeignKey(pmgi => pmgi.GroupId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pmgi => pmgi.Modifier)
                  .WithMany()
                  .HasForeignKey(pmgi => pmgi.ModifierId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint on Group-Modifier combination
            entity.HasIndex(e => new { e.GroupId, e.ModifierId }).IsUnique();

            // Indexes
            entity.HasIndex(e => e.GroupId);
            entity.HasIndex(e => e.ModifierId);
            entity.HasIndex(e => e.SortOrder);
            entity.HasIndex(e => e.Status);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Seed Units of Measurement FIRST (required by Products)
        modelBuilder.Entity<Domain.Entities.UnitOfMeasurement>().HasData(
            new Domain.Entities.UnitOfMeasurement 
            { 
                Id = 1, 
                Name = "Pieces", 
                Abbreviation = "pcs", 
                Type = "Base",
                CategoryTitle = "Count",
                IsActive = true, 
                Status = "Active",
                CreatedBy = null,
                CreatedAt = baseDate 
            },
            new Domain.Entities.UnitOfMeasurement 
            { 
                Id = 2, 
                Name = "Kilograms", 
                Abbreviation = "kg", 
                Type = "Base",
                CategoryTitle = "Weight",
                IsActive = true, 
                Status = "Active",
                CreatedBy = null,
                CreatedAt = baseDate 
            },
            new Domain.Entities.UnitOfMeasurement 
            { 
                Id = 3, 
                Name = "Dozen", 
                Abbreviation = "dz", 
                Type = "Derived",
                CategoryTitle = "Count",
                BaseUomId = 1,
                ConversionFactor = 12.0000m,
                IsActive = true, 
                Status = "Active",
                CreatedBy = null,
                CreatedAt = baseDate 
            },
            new Domain.Entities.UnitOfMeasurement 
            { 
                Id = 4, 
                Name = "Litres", 
                Abbreviation = "L", 
                Type = "Base",
                CategoryTitle = "Volume",
                IsActive = true, 
                Status = "Active",
                CreatedBy = null,
                CreatedAt = baseDate 
            },
            new Domain.Entities.UnitOfMeasurement 
            { 
                Id = 5, 
                Name = "Grams", 
                Abbreviation = "g", 
                Type = "Derived",
                CategoryTitle = "Weight",
                BaseUomId = 2,
                ConversionFactor = 0.0010m,
                IsActive = true, 
                Status = "Active",
                CreatedBy = null,
                CreatedAt = baseDate 
            },
            new Domain.Entities.UnitOfMeasurement 
            { 
                Id = 6, 
                Name = "Millilitres", 
                Abbreviation = "ml", 
                Type = "Derived",
                CategoryTitle = "Volume",
                BaseUomId = 4,
                ConversionFactor = 0.0010m,
                IsActive = true, 
                Status = "Active",
                CreatedBy = null,
                CreatedAt = baseDate 
            }
        );

        // Seed Categories
        modelBuilder.Entity<Domain.Entities.Category>().HasData(
            new Domain.Entities.Category { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories", CreatedAt = baseDate, UpdatedAt = baseDate },
            new Domain.Entities.Category { Id = 2, Name = "Clothing", Description = "Apparel and fashion items", CreatedAt = baseDate, UpdatedAt = baseDate },
            new Domain.Entities.Category { Id = 3, Name = "Food & Beverages", Description = "Food items and drinks", CreatedAt = baseDate, UpdatedAt = baseDate },
            new Domain.Entities.Category { Id = 4, Name = "Books", Description = "Books and educational materials", CreatedAt = baseDate, UpdatedAt = baseDate }
        );

        // Seed Stores
        modelBuilder.Entity<Domain.Entities.Store>().HasData(
            new Domain.Entities.Store { Id = 1, Name = "Main Store", Address = "123 Main Street", PhoneNumber = "+1234567890", Email = "main@chronopos.com", ManagerName = "John Doe", IsActive = true, IsDefault = true, CreatedAt = baseDate, UpdatedAt = baseDate },
            new Domain.Entities.Store { Id = 2, Name = "Branch Store", Address = "456 Branch Avenue", PhoneNumber = "+1234567891", Email = "branch@chronopos.com", ManagerName = "Jane Smith", IsActive = true, IsDefault = false, CreatedAt = baseDate, UpdatedAt = baseDate }
        );

        // NOTE: Shift seeding disabled because Users are not seeded (created during onboarding)
        // Shifts will be created when users start their work day
        // For now, transactions use ShiftId = 1 as a placeholder
        
        // Seed Default Shift (required for transactions) - without foreign key dependencies
        modelBuilder.Entity<Domain.Entities.Shift>().HasData(
            new Domain.Entities.Shift 
            { 
                ShiftId = 1, 
                UserId = null, // No user dependency - can be assigned later
                ShopLocationId = null,
                StartTime = baseDate,
                EndTime = null,
                OpeningCash = 0m,
                ClosingCash = 0m,
                ExpectedCash = 0m,
                CashDifference = 0m,
                Status = "Open",
                Notes = "Default shift for transactions",
                CreatedBy = null,
                CreatedAt = baseDate,
                UpdatedBy = null,
                UpdatedAt = null
            }
        );

        // Seed TaxTypes
        modelBuilder.Entity<Domain.Entities.TaxType>().HasData(
            new Domain.Entities.TaxType { Id = 1, Name = "VAT", Description = "Value Added Tax", Value = 10.0000m, IsPercentage = true, IncludedInPrice = false, AppliesToBuying = false, AppliesToSelling = true, CalculationOrder = 1, IsActive = true, CreatedAt = baseDate },
            new Domain.Entities.TaxType { Id = 2, Name = "Sales Tax", Description = "General Sales Tax", Value = 5.0000m, IsPercentage = true, IncludedInPrice = false, AppliesToBuying = false, AppliesToSelling = true, CalculationOrder = 1, IsActive = true, CreatedAt = baseDate },
            new Domain.Entities.TaxType { Id = 3, Name = "Luxury Tax", Description = "Luxury Goods Tax", Value = 15.0000m, IsPercentage = true, IncludedInPrice = false, AppliesToBuying = false, AppliesToSelling = true, CalculationOrder = 2, IsActive = true, CreatedAt = baseDate }
        );

        // Seed Selling Price Types
        modelBuilder.Entity<Domain.Entities.SellingPriceType>().HasData(
            new Domain.Entities.SellingPriceType { Id = 1, TypeName = "Retail", ArabicName = "بيع بالتجزئة", Description = "Standard retail pricing", Status = true, CreatedBy = null, CreatedAt = baseDate },
            new Domain.Entities.SellingPriceType { Id = 2, TypeName = "Wholesale", ArabicName = "بيع بالجملة", Description = "Bulk pricing for wholesale customers", Status = true, CreatedBy = null, CreatedAt = baseDate },
            new Domain.Entities.SellingPriceType { Id = 3, TypeName = "VIP", ArabicName = "في آي بي", Description = "Premium pricing for VIP customers", Status = true, CreatedBy = null, CreatedAt = baseDate },
            new Domain.Entities.SellingPriceType { Id = 4, TypeName = "Staff", ArabicName = "موظف", Description = "Employee discount pricing", Status = true, CreatedBy = null, CreatedAt = baseDate },
            new Domain.Entities.SellingPriceType { Id = 5, TypeName = "Student", ArabicName = "طالب", Description = "Student discount pricing", Status = true, CreatedBy = null, CreatedAt = baseDate }
        );

        // Seed Payment Types
        modelBuilder.Entity<Domain.Entities.PaymentType>().HasData(
            new Domain.Entities.PaymentType { Id = 1, Name = "Cash", PaymentCode = "CASH", NameAr = "نقد", Status = true, CreatedBy = null, CreatedAt = baseDate },
            new Domain.Entities.PaymentType { Id = 2, Name = "Credit Card", PaymentCode = "CC", NameAr = "بطاقة ائتمان", Status = true, CreatedBy = null, CreatedAt = baseDate },
            new Domain.Entities.PaymentType { Id = 3, Name = "Debit Card", PaymentCode = "DC", NameAr = "بطاقة خصم", Status = true, CreatedBy = null, CreatedAt = baseDate },
            new Domain.Entities.PaymentType { Id = 4, Name = "Digital Wallet", PaymentCode = "DW", NameAr = "محفظة رقمية", Status = true, CreatedBy = null, CreatedAt = baseDate },
            new Domain.Entities.PaymentType { Id = 5, Name = "Bank Transfer", PaymentCode = "BT", NameAr = "تحويل مصرفي", Status = true, CreatedBy = null, CreatedAt = baseDate },
            new Domain.Entities.PaymentType { Id = 6, Name = "Check", PaymentCode = "CHK", NameAr = "شيك", Status = true, CreatedBy = null, CreatedAt = baseDate }
        );

        // Seed Business Types
        modelBuilder.Entity<Domain.Entities.BusinessType>().HasData(
            new Domain.Entities.BusinessType { Id = 1, BusinessTypeName = "Corporation", BusinessTypeNameAr = "شركة", Status = "Active", CreatedAt = baseDate, UpdatedAt = baseDate },
            new Domain.Entities.BusinessType { Id = 2, BusinessTypeName = "Partnership", BusinessTypeNameAr = "شراكة", Status = "Active", CreatedAt = baseDate, UpdatedAt = baseDate },
            new Domain.Entities.BusinessType { Id = 3, BusinessTypeName = "Sole Proprietorship", BusinessTypeNameAr = "ملكية فردية", Status = "Active", CreatedAt = baseDate, UpdatedAt = baseDate },
            new Domain.Entities.BusinessType { Id = 4, BusinessTypeName = "LLC", BusinessTypeNameAr = "ذات مسؤولية محدودة", Status = "Active", CreatedAt = baseDate, UpdatedAt = baseDate },
            new Domain.Entities.BusinessType { Id = 5, BusinessTypeName = "Non-Profit", BusinessTypeNameAr = "غير ربحية", Status = "Active", CreatedAt = baseDate, UpdatedAt = baseDate }
        );

        // Seed Customers
        modelBuilder.Entity<Domain.Entities.Customer>().HasData(
            new Domain.Entities.Customer 
            { 
                Id = 1, 
                CustomerFullName = "John Doe", 
                IsBusiness = false,
                MobileNo = "555-0101", 
                OfficialEmail = "john.doe@email.com",
                Status = "Active",
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            },
            new Domain.Entities.Customer 
            { 
                Id = 2, 
                BusinessFullName = "Smith Corporation", 
                IsBusiness = true,
                MobileNo = "555-0102", 
                OfficialEmail = "info@smithcorp.com",
                LicenseNo = "BL123456",
                TrnNo = "TRN789012",
                Status = "Active",
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            }
        );

        // Seed Languages
        modelBuilder.Entity<Domain.Entities.Language>().HasData(
            new Domain.Entities.Language { Id = 1, LanguageName = "English", LanguageCode = "en", IsRtl = false, Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.Language { Id = 2, LanguageName = "العربية", LanguageCode = "ar", IsRtl = true, Status = "Active", CreatedBy = "System", CreatedAt = baseDate }
        );

        // Seed Language Keywords
        modelBuilder.Entity<Domain.Entities.LanguageKeyword>().HasData(
            // Main Navigation
            new Domain.Entities.LanguageKeyword { Id = 1, Key = "nav.dashboard", Description = "Dashboard navigation item" },
            new Domain.Entities.LanguageKeyword { Id = 2, Key = "nav.management", Description = "Back Office navigation item (formerly Management)" },
            new Domain.Entities.LanguageKeyword { Id = 3, Key = "nav.customers", Description = "Customers navigation item" },
            new Domain.Entities.LanguageKeyword { Id = 4, Key = "nav.sales", Description = "Sales Window navigation item (formerly Transactions)" },
            new Domain.Entities.LanguageKeyword { Id = 5, Key = "nav.settings", Description = "Settings navigation item" },
            new Domain.Entities.LanguageKeyword { Id = 6, Key = "nav.logout", Description = "Logout button" },
            new Domain.Entities.LanguageKeyword { Id = 121, Key = "nav.transaction", Description = "Transaction navigation item (NEW)" },

            // Common Buttons
            new Domain.Entities.LanguageKeyword { Id = 7, Key = "btn.save", Description = "Save button" },
            new Domain.Entities.LanguageKeyword { Id = 8, Key = "btn.cancel", Description = "Cancel button" },
            new Domain.Entities.LanguageKeyword { Id = 9, Key = "btn.edit", Description = "Edit button" },
            new Domain.Entities.LanguageKeyword { Id = 10, Key = "btn.delete", Description = "Delete button" },
            new Domain.Entities.LanguageKeyword { Id = 11, Key = "btn.add", Description = "Add button" },
            new Domain.Entities.LanguageKeyword { Id = 12, Key = "btn.search", Description = "Search button" },
            new Domain.Entities.LanguageKeyword { Id = 13, Key = "btn.reset", Description = "Reset button" },
            new Domain.Entities.LanguageKeyword { Id = 14, Key = "btn.apply", Description = "Apply button" },

            // Settings Page
            new Domain.Entities.LanguageKeyword { Id = 15, Key = "settings.language", Description = "Language Settings section" },
            new Domain.Entities.LanguageKeyword { Id = 16, Key = "settings.theme", Description = "Theme Settings section" },
            new Domain.Entities.LanguageKeyword { Id = 17, Key = "settings.color_scheme", Description = "Color Scheme section" },
            new Domain.Entities.LanguageKeyword { Id = 18, Key = "settings.layout_direction", Description = "Layout Direction section" },
            new Domain.Entities.LanguageKeyword { Id = 19, Key = "settings.font", Description = "Font Settings section" },
            new Domain.Entities.LanguageKeyword { Id = 20, Key = "settings.actions", Description = "Actions section" },

            // Products Page
            new Domain.Entities.LanguageKeyword { Id = 21, Key = "products.title", Description = "Products page title" },
            new Domain.Entities.LanguageKeyword { Id = 22, Key = "products.name", Description = "Product name field" },
            new Domain.Entities.LanguageKeyword { Id = 23, Key = "products.price", Description = "Product price field" },
            new Domain.Entities.LanguageKeyword { Id = 24, Key = "products.category", Description = "Product category field" },
            new Domain.Entities.LanguageKeyword { Id = 25, Key = "products.stock", Description = "Product stock field" },

            // Common Labels
            new Domain.Entities.LanguageKeyword { Id = 26, Key = "label.current", Description = "Current label" },
            new Domain.Entities.LanguageKeyword { Id = 27, Key = "label.status", Description = "Status label" },
            new Domain.Entities.LanguageKeyword { Id = 28, Key = "label.ready", Description = "Ready status" },
            new Domain.Entities.LanguageKeyword { Id = 29, Key = "label.loading", Description = "Loading text" },

            // Themes
            new Domain.Entities.LanguageKeyword { Id = 30, Key = "theme.light", Description = "Light theme" },
            new Domain.Entities.LanguageKeyword { Id = 31, Key = "theme.dark", Description = "Dark theme" },
            
            // Layout Direction
            new Domain.Entities.LanguageKeyword { Id = 32, Key = "layout.ltr", Description = "Left to Right" },
            new Domain.Entities.LanguageKeyword { Id = 33, Key = "layout.rtl", Description = "Right to Left" },

            // Font Sizes
            new Domain.Entities.LanguageKeyword { Id = 34, Key = "font.small", Description = "Small font size" },
            new Domain.Entities.LanguageKeyword { Id = 35, Key = "font.medium", Description = "Medium font size" },
            new Domain.Entities.LanguageKeyword { Id = 36, Key = "font.large", Description = "Large font size" },

            // Stock Management Modules
            new Domain.Entities.LanguageKeyword { Id = 37, Key = "stock.adjustment", Description = "Stock Adjustment module" },
            new Domain.Entities.LanguageKeyword { Id = 38, Key = "stock.transfer", Description = "Stock Transfer module" },
            new Domain.Entities.LanguageKeyword { Id = 39, Key = "stock.goods_received", Description = "Goods Received module" },
            new Domain.Entities.LanguageKeyword { Id = 40, Key = "stock.goods_return", Description = "Goods Return module" },
            new Domain.Entities.LanguageKeyword { Id = 41, Key = "stock.goods_replaced", Description = "Goods Replaced module" },

            // Management Modules (All 6 original modules)
            new Domain.Entities.LanguageKeyword { Id = 42, Key = "management.stock", Description = "Stock Management" },
            new Domain.Entities.LanguageKeyword { Id = 43, Key = "management.products", Description = "Products" },
            new Domain.Entities.LanguageKeyword { Id = 44, Key = "management.supplier", Description = "Supplier" },
            new Domain.Entities.LanguageKeyword { Id = 45, Key = "management.customers", Description = "Customer Module" },
            new Domain.Entities.LanguageKeyword { Id = 46, Key = "management.payment", Description = "Payment Options" },
            new Domain.Entities.LanguageKeyword { Id = 47, Key = "management.service", Description = "Service Charge" },

            // Settings Modules (NEW - for renamed/new modules)
            new Domain.Entities.LanguageKeyword { Id = 123, Key = "settings.client_settings", Description = "Client Settings module (formerly User Settings)" },
            new Domain.Entities.LanguageKeyword { Id = 124, Key = "settings.global_settings", Description = "Global Settings module (formerly Application Settings)" },
            new Domain.Entities.LanguageKeyword { Id = 125, Key = "settings.others", Description = "Others module (formerly Add Options, moved from Management)" },
            new Domain.Entities.LanguageKeyword { Id = 126, Key = "settings.services", Description = "Services module (NEW)" },

            // UI Buttons
            new Domain.Entities.LanguageKeyword { Id = 48, Key = "btn.back", Description = "Back button" },
            new Domain.Entities.LanguageKeyword { Id = 49, Key = "btn.refresh", Description = "Refresh button" },

            // Login Screen
            new Domain.Entities.LanguageKeyword { Id = 127, Key = "login.title", Description = "Login screen title" },
            new Domain.Entities.LanguageKeyword { Id = 128, Key = "login.subtitle", Description = "Login screen subtitle" },
            new Domain.Entities.LanguageKeyword { Id = 129, Key = "login.username", Description = "Username label" },
            new Domain.Entities.LanguageKeyword { Id = 130, Key = "login.password", Description = "Password label" },
            new Domain.Entities.LanguageKeyword { Id = 131, Key = "login.username_placeholder", Description = "Username placeholder text" },
            new Domain.Entities.LanguageKeyword { Id = 132, Key = "login.password_placeholder", Description = "Password placeholder text" },
            new Domain.Entities.LanguageKeyword { Id = 133, Key = "login.remember_me", Description = "Remember me checkbox" },
            new Domain.Entities.LanguageKeyword { Id = 134, Key = "login.forgot_password", Description = "Forgot password link" },
            new Domain.Entities.LanguageKeyword { Id = 135, Key = "login.button", Description = "Login button text" },
            new Domain.Entities.LanguageKeyword { Id = 136, Key = "login.error_username_required", Description = "Username required error message" },
            new Domain.Entities.LanguageKeyword { Id = 137, Key = "login.error_password_required", Description = "Password required error message" },
            new Domain.Entities.LanguageKeyword { Id = 138, Key = "login.error_invalid_credentials", Description = "Invalid credentials error message" },
            new Domain.Entities.LanguageKeyword { Id = 139, Key = "login.password_reset_success", Description = "Password reset success message" },

            // Forgot Password Screen
            new Domain.Entities.LanguageKeyword { Id = 140, Key = "forgot_password.title", Description = "Reset password screen title" },
            new Domain.Entities.LanguageKeyword { Id = 141, Key = "forgot_password.subtitle", Description = "Reset password screen subtitle" },
            new Domain.Entities.LanguageKeyword { Id = 142, Key = "forgot_password.license_file", Description = "License file label" },
            new Domain.Entities.LanguageKeyword { Id = 143, Key = "forgot_password.new_password", Description = "New password label" },
            new Domain.Entities.LanguageKeyword { Id = 144, Key = "forgot_password.confirm_password", Description = "Confirm password label" },
            new Domain.Entities.LanguageKeyword { Id = 145, Key = "forgot_password.back_to_login", Description = "Back to login button" },
            new Domain.Entities.LanguageKeyword { Id = 146, Key = "forgot_password.reset_button", Description = "Reset password button" },
            new Domain.Entities.LanguageKeyword { Id = 147, Key = "forgot_password.browse_button", Description = "Browse license file button" },
            new Domain.Entities.LanguageKeyword { Id = 148, Key = "forgot_password.success_message", Description = "Password reset success message" },
            new Domain.Entities.LanguageKeyword { Id = 149, Key = "forgot_password.error_license_required", Description = "License file required error" },
            new Domain.Entities.LanguageKeyword { Id = 150, Key = "forgot_password.error_password_required", Description = "New password required error" },
            new Domain.Entities.LanguageKeyword { Id = 151, Key = "forgot_password.error_confirm_password_required", Description = "Confirm password required error" },
            new Domain.Entities.LanguageKeyword { Id = 152, Key = "forgot_password.error_passwords_mismatch", Description = "Passwords mismatch error" },
            new Domain.Entities.LanguageKeyword { Id = 153, Key = "forgot_password.error_password_too_short", Description = "Password too short error" },
            new Domain.Entities.LanguageKeyword { Id = 154, Key = "forgot_password.error_invalid_license", Description = "Invalid license format error" },
            new Domain.Entities.LanguageKeyword { Id = 155, Key = "forgot_password.error_license_machine_mismatch", Description = "License machine mismatch error" },
            new Domain.Entities.LanguageKeyword { Id = 156, Key = "forgot_password.error_license_expired", Description = "License expired error" },
            new Domain.Entities.LanguageKeyword { Id = 157, Key = "forgot_password.error_license_sales_key_mismatch", Description = "License sales key mismatch error" },
            new Domain.Entities.LanguageKeyword { Id = 158, Key = "forgot_password.verify_license_button", Description = "Verify license button text" },
            new Domain.Entities.LanguageKeyword { Id = 159, Key = "forgot_password.license_verified_message", Description = "License verified success message" },
            new Domain.Entities.LanguageKeyword { Id = 160, Key = "forgot_password.error_license_not_verified", Description = "License not verified error message" }
        );

        // Seed Users (required by many entities with CreatedBy/UpdatedBy)
        // NOTE: User seeding disabled to allow admin creation during onboarding
        // Users will be created through the CreateAdminWindow after license activation
        /*
        modelBuilder.Entity<Domain.Entities.User>().HasData(
            new Domain.Entities.User 
            { 
                Id = 1, 
                FullName = "System Administrator", 
                Email = "admin@chronopos.com", 
                Password = "admin123", // In production, this should be hashed
                Role = "Admin", 
                PhoneNo = "+1234567890",
                UaeId = "SYS001",
                CreatedAt = baseDate 
            },
            new Domain.Entities.User 
            { 
                Id = 2, 
                FullName = "Store Manager", 
                Email = "manager@chronopos.com", 
                Password = "manager123", // In production, this should be hashed
                Role = "Manager", 
                PhoneNo = "+1234567891",
                UaeId = "MGR001",
                CreatedAt = baseDate 
            }
        );
        */

        // Seed ShopLocations (required by StockAdjustments)
        modelBuilder.Entity<Domain.Entities.ShopLocation>().HasData(
            new Domain.Entities.ShopLocation 
            { 
                Id = 1, 
                ShopId = 1,
                LocationType = "Main",
                LocationName = "Main Store Location", 
                AddressLine1 = "123 Main Street",
                AddressLine2 = "Suite 100",
                Building = "Commercial Center",
                Area = "Downtown",
                City = "Dubai",
                PoBox = "12345",
                LandlineNumber = "+971-4-1234567",
                MobileNumber = "+971-50-1234567",
                Status = "Active",
                CreatedAt = baseDate,
                LocationLatitude = 25.2048m,
                LocationLongitude = 55.2708m
            }
        );

        // Seed StockAdjustmentReasons (required by StockAdjustments)
        modelBuilder.Entity<Domain.Entities.StockAdjustmentReason>().HasData(
            new Domain.Entities.StockAdjustmentReason 
            { 
                StockAdjustmentReasonsId = 1, 
                Name = "Physical Count Adjustment", 
                Description = "Adjustment based on physical stock count",
                Status = "Active",
                CreatedBy = null,
                CreatedAt = baseDate
            },
            new Domain.Entities.StockAdjustmentReason 
            { 
                StockAdjustmentReasonsId = 2, 
                Name = "Damaged Goods", 
                Description = "Stock reduction due to damaged inventory",
                Status = "Active",
                CreatedBy = null,
                CreatedAt = baseDate
            },
            new Domain.Entities.StockAdjustmentReason 
            { 
                StockAdjustmentReasonsId = 3, 
                Name = "System Error Correction", 
                Description = "Correction of system entry errors",
                Status = "Active",
                CreatedBy = null,
                CreatedAt = baseDate
            }
        );

        // Seed Label Translations - English
        modelBuilder.Entity<Domain.Entities.LabelTranslation>().HasData(
            // Navigation - English
            new Domain.Entities.LabelTranslation { Id = 1, LanguageId = 1, TranslationKey = "nav.dashboard", Value = "Dashboard", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 2, LanguageId = 1, TranslationKey = "nav.management", Value = "Back Office", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 3, LanguageId = 1, TranslationKey = "nav.customers", Value = "Customers", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 4, LanguageId = 1, TranslationKey = "nav.sales", Value = "Sales Window", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 5, LanguageId = 1, TranslationKey = "nav.settings", Value = "Settings", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 6, LanguageId = 1, TranslationKey = "nav.logout", Value = "Logout", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 121, LanguageId = 1, TranslationKey = "nav.transaction", Value = "Transaction", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Buttons - English
            new Domain.Entities.LabelTranslation { Id = 7, LanguageId = 1, TranslationKey = "btn.save", Value = "Save", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 8, LanguageId = 1, TranslationKey = "btn.cancel", Value = "Cancel", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 9, LanguageId = 1, TranslationKey = "btn.edit", Value = "Edit", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 10, LanguageId = 1, TranslationKey = "btn.delete", Value = "Delete", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 11, LanguageId = 1, TranslationKey = "btn.add", Value = "Add", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 12, LanguageId = 1, TranslationKey = "btn.search", Value = "Search", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Settings - English
            new Domain.Entities.LabelTranslation { Id = 13, LanguageId = 1, TranslationKey = "settings.language", Value = "🌐 Language Settings", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 14, LanguageId = 1, TranslationKey = "settings.theme", Value = "🎨 Theme Settings", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 15, LanguageId = 1, TranslationKey = "settings.color_scheme", Value = "🎨 Color Scheme", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 16, LanguageId = 1, TranslationKey = "settings.layout_direction", Value = "📱 Layout Direction", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 17, LanguageId = 1, TranslationKey = "settings.font", Value = "🔤 Font Settings", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 18, LanguageId = 1, TranslationKey = "settings.actions", Value = "⚙️ Actions", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Products - English
            new Domain.Entities.LabelTranslation { Id = 19, LanguageId = 1, TranslationKey = "products.title", Value = "Products Management", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 20, LanguageId = 1, TranslationKey = "products.name", Value = "Product Name", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 21, LanguageId = 1, TranslationKey = "products.price", Value = "Price", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 22, LanguageId = 1, TranslationKey = "products.category", Value = "Category", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 23, LanguageId = 1, TranslationKey = "products.stock", Value = "Stock", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Common Labels - English
            new Domain.Entities.LabelTranslation { Id = 24, LanguageId = 1, TranslationKey = "label.current", Value = "Current", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 25, LanguageId = 1, TranslationKey = "label.ready", Value = "Ready", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 26, LanguageId = 1, TranslationKey = "theme.light", Value = "Light Theme", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 27, LanguageId = 1, TranslationKey = "theme.dark", Value = "Dark Theme", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 28, LanguageId = 1, TranslationKey = "layout.ltr", Value = "Left to Right (LTR)", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 29, LanguageId = 1, TranslationKey = "layout.rtl", Value = "Right to Left (RTL)", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 30, LanguageId = 1, TranslationKey = "font.small", Value = "Small", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 31, LanguageId = 1, TranslationKey = "font.medium", Value = "Medium", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 32, LanguageId = 1, TranslationKey = "font.large", Value = "Large", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Stock Management Modules - English
            new Domain.Entities.LabelTranslation { Id = 33, LanguageId = 1, TranslationKey = "stock.adjustment", Value = "Stock Adjustment", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 34, LanguageId = 1, TranslationKey = "stock.transfer", Value = "Stock Transfer", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 35, LanguageId = 1, TranslationKey = "stock.goods_received", Value = "Goods Received", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 36, LanguageId = 1, TranslationKey = "stock.goods_return", Value = "Goods Return", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 37, LanguageId = 1, TranslationKey = "stock.goods_replaced", Value = "Goods Replaced", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Management Modules - English (All 6 original modules)
            new Domain.Entities.LabelTranslation { Id = 38, LanguageId = 1, TranslationKey = "management.stock", Value = "Stock Management", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 39, LanguageId = 1, TranslationKey = "management.products", Value = "Products", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 40, LanguageId = 1, TranslationKey = "management.supplier", Value = "Supplier", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 41, LanguageId = 1, TranslationKey = "management.customers", Value = "Customer Module", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 42, LanguageId = 1, TranslationKey = "management.payment", Value = "Payment Options", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 43, LanguageId = 1, TranslationKey = "management.service", Value = "Service Charge", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // UI Buttons - English
            new Domain.Entities.LabelTranslation { Id = 44, LanguageId = 1, TranslationKey = "btn.back", Value = "Back", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 45, LanguageId = 1, TranslationKey = "btn.refresh", Value = "Refresh", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Navigation - Urdu
            new Domain.Entities.LabelTranslation { Id = 46, LanguageId = 2, TranslationKey = "nav.dashboard", Value = "ڈیش بورڈ", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 47, LanguageId = 2, TranslationKey = "nav.management", Value = "بیک آفس", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 48, LanguageId = 2, TranslationKey = "nav.customers", Value = "گاہک", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 49, LanguageId = 2, TranslationKey = "nav.sales", Value = "سیلز ونڈو", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 50, LanguageId = 2, TranslationKey = "nav.settings", Value = "ترتیبات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 51, LanguageId = 2, TranslationKey = "nav.logout", Value = "لاگ آؤٹ", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 122, LanguageId = 2, TranslationKey = "nav.transaction", Value = "لین دین", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Buttons - Urdu
            new Domain.Entities.LabelTranslation { Id = 52, LanguageId = 2, TranslationKey = "btn.save", Value = "محفوظ کریں", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 53, LanguageId = 2, TranslationKey = "btn.cancel", Value = "منسوخ", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 54, LanguageId = 2, TranslationKey = "btn.edit", Value = "ترمیم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 55, LanguageId = 2, TranslationKey = "btn.delete", Value = "حذف", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 56, LanguageId = 2, TranslationKey = "btn.add", Value = "شامل کریں", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 57, LanguageId = 2, TranslationKey = "btn.search", Value = "تلاش", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Settings - Urdu
            new Domain.Entities.LabelTranslation { Id = 58, LanguageId = 2, TranslationKey = "settings.language", Value = "🌐 زبان کی ترتیبات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 59, LanguageId = 2, TranslationKey = "settings.theme", Value = "🎨 تھیم کی ترتیبات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 60, LanguageId = 2, TranslationKey = "settings.color_scheme", Value = "🎨 رنگ سکیم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 61, LanguageId = 2, TranslationKey = "settings.layout_direction", Value = "📱 لے آؤٹ کی سمت", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 62, LanguageId = 2, TranslationKey = "settings.font", Value = "🔤 فونٹ کی ترتیبات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 63, LanguageId = 2, TranslationKey = "settings.actions", Value = "⚙️ عمل", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Stock Management Modules - Urdu
            new Domain.Entities.LabelTranslation { Id = 64, LanguageId = 2, TranslationKey = "stock.adjustment", Value = "اسٹاک کی تشخیص", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 65, LanguageId = 2, TranslationKey = "stock.transfer", Value = "اسٹاک ٹرانسفر", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 66, LanguageId = 2, TranslationKey = "stock.goods_received", Value = "مال کی آمد", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 67, LanguageId = 2, TranslationKey = "stock.goods_return", Value = "مال کی واپسی", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 68, LanguageId = 2, TranslationKey = "stock.goods_replaced", Value = "مال کی تبدیلی", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Management Modules - Urdu (All 6 original modules)
            new Domain.Entities.LabelTranslation { Id = 69, LanguageId = 2, TranslationKey = "management.stock", Value = "اسٹاک انتظام", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 70, LanguageId = 2, TranslationKey = "management.products", Value = "مصنوعات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 71, LanguageId = 2, TranslationKey = "management.supplier", Value = "سپلائر", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 72, LanguageId = 2, TranslationKey = "management.customers", Value = "کسٹمر ماڈیول", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 73, LanguageId = 2, TranslationKey = "management.payment", Value = "ادائیگی کے اختیارات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 74, LanguageId = 2, TranslationKey = "management.service", Value = "سروس چارج", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // UI Buttons - Urdu
            new Domain.Entities.LabelTranslation { Id = 75, LanguageId = 2, TranslationKey = "btn.back", Value = "واپس", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 76, LanguageId = 2, TranslationKey = "btn.refresh", Value = "تازہ کریں", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Products - Urdu
            new Domain.Entities.LabelTranslation { Id = 77, LanguageId = 2, TranslationKey = "products.title", Value = "مصنوعات کا انتظام", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 78, LanguageId = 2, TranslationKey = "products.name", Value = "مصنوع کا نام", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 79, LanguageId = 2, TranslationKey = "products.price", Value = "قیمت", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 80, LanguageId = 2, TranslationKey = "products.category", Value = "قسم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 81, LanguageId = 2, TranslationKey = "products.stock", Value = "اسٹاک", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Common Labels - Urdu
            new Domain.Entities.LabelTranslation { Id = 82, LanguageId = 2, TranslationKey = "label.current", Value = "موجودہ", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 83, LanguageId = 2, TranslationKey = "label.ready", Value = "تیار", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 84, LanguageId = 2, TranslationKey = "theme.light", Value = "ہلکا تھیم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 85, LanguageId = 2, TranslationKey = "theme.dark", Value = "گہرا تھیم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 86, LanguageId = 2, TranslationKey = "layout.ltr", Value = "بائیں سے دائیں (LTR)", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 87, LanguageId = 2, TranslationKey = "layout.rtl", Value = "دائیں سے بائیں (RTL)", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 88, LanguageId = 2, TranslationKey = "font.small", Value = "چھوٹا", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 89, LanguageId = 2, TranslationKey = "font.medium", Value = "درمیانہ", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 90, LanguageId = 2, TranslationKey = "font.large", Value = "بڑا", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Settings Modules - English (NEW)
            new Domain.Entities.LabelTranslation { Id = 127, LanguageId = 1, TranslationKey = "settings.client_settings", Value = "Client Settings", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 128, LanguageId = 1, TranslationKey = "settings.global_settings", Value = "Global Settings", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 129, LanguageId = 1, TranslationKey = "settings.others", Value = "Others", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 130, LanguageId = 1, TranslationKey = "settings.services", Value = "Services", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Settings Modules - Urdu (NEW)
            new Domain.Entities.LabelTranslation { Id = 131, LanguageId = 2, TranslationKey = "settings.client_settings", Value = "کلائنٹ کی ترتیبات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 132, LanguageId = 2, TranslationKey = "settings.global_settings", Value = "عالمی ترتیبات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 133, LanguageId = 2, TranslationKey = "settings.others", Value = "دیگر", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 134, LanguageId = 2, TranslationKey = "settings.services", Value = "خدمات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Login Screen - English
            new Domain.Entities.LabelTranslation { Id = 200, LanguageId = 1, TranslationKey = "login.title", Value = "Login!", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 201, LanguageId = 1, TranslationKey = "login.subtitle", Value = "Please enter your credentials below to continue", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 202, LanguageId = 1, TranslationKey = "login.username", Value = "Username", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 203, LanguageId = 1, TranslationKey = "login.password", Value = "Password", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 204, LanguageId = 1, TranslationKey = "login.username_placeholder", Value = "Enter your username", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 205, LanguageId = 1, TranslationKey = "login.password_placeholder", Value = "Enter your password", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 206, LanguageId = 1, TranslationKey = "login.remember_me", Value = "Remember me", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 207, LanguageId = 1, TranslationKey = "login.forgot_password", Value = "Forgot Password?", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 208, LanguageId = 1, TranslationKey = "login.button", Value = "Login", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 209, LanguageId = 1, TranslationKey = "login.error_username_required", Value = "Please enter your username or email.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 210, LanguageId = 1, TranslationKey = "login.error_password_required", Value = "Please enter your password.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 211, LanguageId = 1, TranslationKey = "login.error_invalid_credentials", Value = "Invalid username or password.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 224, LanguageId = 1, TranslationKey = "login.password_reset_success", Value = "Password has been reset successfully! Please login with your new password.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Login Screen - Arabic
            new Domain.Entities.LabelTranslation { Id = 212, LanguageId = 2, TranslationKey = "login.title", Value = "تسجيل الدخول!", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 213, LanguageId = 2, TranslationKey = "login.subtitle", Value = "الرجاء إدخال بيانات الاعتماد أدناه للمتابعة", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 214, LanguageId = 2, TranslationKey = "login.username", Value = "اسم المستخدم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 215, LanguageId = 2, TranslationKey = "login.password", Value = "كلمة المرور", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 216, LanguageId = 2, TranslationKey = "login.username_placeholder", Value = "أدخل اسم المستخدم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 217, LanguageId = 2, TranslationKey = "login.password_placeholder", Value = "أدخل كلمة المرور", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 218, LanguageId = 2, TranslationKey = "login.remember_me", Value = "تذكرني", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 219, LanguageId = 2, TranslationKey = "login.forgot_password", Value = "نسيت كلمة المرور؟", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 220, LanguageId = 2, TranslationKey = "login.button", Value = "تسجيل الدخول", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 221, LanguageId = 2, TranslationKey = "login.error_username_required", Value = "الرجاء إدخال اسم المستخدم أو البريد الإلكتروني.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 222, LanguageId = 2, TranslationKey = "login.error_password_required", Value = "الرجاء إدخال كلمة المرور.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 223, LanguageId = 2, TranslationKey = "login.error_invalid_credentials", Value = "اسم المستخدم أو كلمة المرور غير صحيحة.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 225, LanguageId = 2, TranslationKey = "login.password_reset_success", Value = "تم إعادة تعيين كلمة المرور بنجاح! يرجى تسجيل الدخول بكلمة المرور الجديدة.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Forgot Password Screen - English
            new Domain.Entities.LabelTranslation { Id = 226, LanguageId = 1, TranslationKey = "forgot_password.title", Value = "Reset Password", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 227, LanguageId = 1, TranslationKey = "forgot_password.subtitle", Value = "Please provide your license file and set a new password", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 228, LanguageId = 1, TranslationKey = "forgot_password.license_file", Value = "License File", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 229, LanguageId = 1, TranslationKey = "forgot_password.new_password", Value = "New Password", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 230, LanguageId = 1, TranslationKey = "forgot_password.confirm_password", Value = "Confirm Password", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 231, LanguageId = 1, TranslationKey = "forgot_password.back_to_login", Value = "← Back to Login", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 232, LanguageId = 1, TranslationKey = "forgot_password.reset_button", Value = "Reset Password", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 233, LanguageId = 1, TranslationKey = "forgot_password.browse_button", Value = "Browse...", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 234, LanguageId = 1, TranslationKey = "forgot_password.success_message", Value = "Password has been reset successfully!", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 235, LanguageId = 1, TranslationKey = "forgot_password.error_license_required", Value = "Please select a license file.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 236, LanguageId = 1, TranslationKey = "forgot_password.error_password_required", Value = "Please enter a new password.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 237, LanguageId = 1, TranslationKey = "forgot_password.error_confirm_password_required", Value = "Please confirm your new password.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 238, LanguageId = 1, TranslationKey = "forgot_password.error_passwords_mismatch", Value = "Passwords do not match.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 239, LanguageId = 1, TranslationKey = "forgot_password.error_password_too_short", Value = "Password must be at least 6 characters long.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 240, LanguageId = 1, TranslationKey = "forgot_password.error_invalid_license", Value = "Invalid license file format.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 241, LanguageId = 1, TranslationKey = "forgot_password.error_license_machine_mismatch", Value = "License is not valid for this machine.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 242, LanguageId = 1, TranslationKey = "forgot_password.error_license_expired", Value = "License has expired. Please contact support.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 243, LanguageId = 1, TranslationKey = "forgot_password.error_license_sales_key_mismatch", Value = "License does not match the sales key for this machine.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 262, LanguageId = 1, TranslationKey = "forgot_password.verify_license_button", Value = "Verify License", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 263, LanguageId = 1, TranslationKey = "forgot_password.license_verified_message", Value = "License verified successfully. You can now set a new password.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 264, LanguageId = 1, TranslationKey = "forgot_password.error_license_not_verified", Value = "Please verify your license first.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Forgot Password Screen - Arabic
            new Domain.Entities.LabelTranslation { Id = 244, LanguageId = 2, TranslationKey = "forgot_password.title", Value = "إعادة تعيين كلمة المرور", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 245, LanguageId = 2, TranslationKey = "forgot_password.subtitle", Value = "يرجى تقديم ملف الترخيص الخاص بك وتعيين كلمة مرور جديدة", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 246, LanguageId = 2, TranslationKey = "forgot_password.license_file", Value = "ملف الترخيص", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 247, LanguageId = 2, TranslationKey = "forgot_password.new_password", Value = "كلمة المرور الجديدة", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 248, LanguageId = 2, TranslationKey = "forgot_password.confirm_password", Value = "تأكيد كلمة المرور", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 249, LanguageId = 2, TranslationKey = "forgot_password.back_to_login", Value = "← العودة لتسجيل الدخول", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 250, LanguageId = 2, TranslationKey = "forgot_password.reset_button", Value = "إعادة تعيين كلمة المرور", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 251, LanguageId = 2, TranslationKey = "forgot_password.browse_button", Value = "تصفح...", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 252, LanguageId = 2, TranslationKey = "forgot_password.success_message", Value = "تم إعادة تعيين كلمة المرور بنجاح!", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 253, LanguageId = 2, TranslationKey = "forgot_password.error_license_required", Value = "يرجى تحديد ملف ترخيص.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 254, LanguageId = 2, TranslationKey = "forgot_password.error_password_required", Value = "يرجى إدخال كلمة مرور جديدة.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 255, LanguageId = 2, TranslationKey = "forgot_password.error_confirm_password_required", Value = "يرجى تأكيد كلمة المرور الجديدة.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 256, LanguageId = 2, TranslationKey = "forgot_password.error_passwords_mismatch", Value = "كلمات المرور غير متطابقة.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 257, LanguageId = 2, TranslationKey = "forgot_password.error_password_too_short", Value = "يجب أن تكون كلمة المرور 6 أحرف على الأقل.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 258, LanguageId = 2, TranslationKey = "forgot_password.error_invalid_license", Value = "تنسيق ملف ترخيص غير صالح.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 259, LanguageId = 2, TranslationKey = "forgot_password.error_license_machine_mismatch", Value = "الترخيص غير صالح لهذا الجهاز.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 260, LanguageId = 2, TranslationKey = "forgot_password.error_license_expired", Value = "انتهت صلاحية الترخيص. يرجى الاتصال بالدعم.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 261, LanguageId = 2, TranslationKey = "forgot_password.error_license_sales_key_mismatch", Value = "الترخيص لا يتطابق مع مفتاح المبيعات لهذا الجهاز.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 265, LanguageId = 2, TranslationKey = "forgot_password.verify_license_button", Value = "تحقق من الترخيص", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 266, LanguageId = 2, TranslationKey = "forgot_password.license_verified_message", Value = "تم التحقق من الترخيص بنجاح. يمكنك الآن تعيين كلمة مرور جديدة.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 267, LanguageId = 2, TranslationKey = "forgot_password.error_license_not_verified", Value = "يرجى التحقق من الترخيص الخاص بك أولاً.", Status = "Active", CreatedBy = "System", CreatedAt = baseDate }
        );

        // Seed Brands
        modelBuilder.Entity<Domain.Entities.Brand>().HasData(
            new Domain.Entities.Brand 
            { 
                Id = 1, 
                Name = "Samsung", 
                NameArabic = "سامسونج", 
                Description = "Samsung Electronics - Global technology leader",
                LogoUrl = "/images/brands/samsung-logo.png",
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            },
            new Domain.Entities.Brand 
            { 
                Id = 2, 
                Name = "Apple", 
                NameArabic = "آبل", 
                Description = "Apple Inc. - Innovation and design",
                LogoUrl = "/images/brands/apple-logo.png",
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            },
            new Domain.Entities.Brand 
            { 
                Id = 3, 
                Name = "Sony", 
                NameArabic = "سوني", 
                Description = "Sony Corporation - Electronics and entertainment",
                LogoUrl = "/images/brands/sony-logo.png",
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            },
            new Domain.Entities.Brand 
            { 
                Id = 4, 
                Name = "Generic", 
                NameArabic = "عام", 
                Description = "Generic brand for unbranded products",
                LogoUrl = "/images/brands/generic-logo.png",
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            }
        );

        // Seed Currencies
        modelBuilder.Entity<Domain.Entities.Currency>().HasData(
            new Domain.Entities.Currency 
            { 
                Id = 1, 
                CurrencyName = "US Dollar", 
                CurrencyCode = "USD", 
                Symbol = "$",
                ExchangeRate = 1.0000m,
                IsDefault = true,
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            },
            new Domain.Entities.Currency 
            { 
                Id = 2, 
                CurrencyName = "Euro", 
                CurrencyCode = "EUR", 
                Symbol = "€",
                ExchangeRate = 0.92m,
                IsDefault = false,
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            },
            new Domain.Entities.Currency 
            { 
                Id = 3, 
                CurrencyName = "British Pound", 
                CurrencyCode = "GBP", 
                Symbol = "£",
                ExchangeRate = 0.79m,
                IsDefault = false,
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            },
            new Domain.Entities.Currency 
            { 
                Id = 4, 
                CurrencyName = "UAE Dirham", 
                CurrencyCode = "AED", 
                Symbol = "د.إ",
                ExchangeRate = 3.67m,
                IsDefault = false,
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            },
            new Domain.Entities.Currency 
            { 
                Id = 5, 
                CurrencyName = "Saudi Riyal", 
                CurrencyCode = "SAR", 
                Symbol = "﷼",
                ExchangeRate = 3.75m,
                IsDefault = false,
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            },
            new Domain.Entities.Currency 
            { 
                Id = 6, 
                CurrencyName = "Indian Rupee", 
                CurrencyCode = "INR", 
                Symbol = "₹",
                ExchangeRate = 83.12m,
                IsDefault = false,
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            }
        );

        // Seed Restaurant Tables
        modelBuilder.Entity<Domain.Entities.RestaurantTable>().HasData(
            // Ground Floor Tables
            new Domain.Entities.RestaurantTable 
            { 
                Id = 1, 
                TableNumber = "T-01", 
                Capacity = 2, 
                Location = "Ground Floor", 
                Status = "available", 
                CreatedAt = baseDate 
            },
            new Domain.Entities.RestaurantTable 
            { 
                Id = 2, 
                TableNumber = "T-02", 
                Capacity = 4, 
                Location = "Ground Floor", 
                Status = "available", 
                CreatedAt = baseDate 
            },
            new Domain.Entities.RestaurantTable 
            { 
                Id = 3, 
                TableNumber = "T-03", 
                Capacity = 4, 
                Location = "Ground Floor", 
                Status = "available", 
                CreatedAt = baseDate 
            },
            new Domain.Entities.RestaurantTable 
            { 
                Id = 4, 
                TableNumber = "T-04", 
                Capacity = 6, 
                Location = "Ground Floor", 
                Status = "available", 
                CreatedAt = baseDate 
            },
            new Domain.Entities.RestaurantTable 
            { 
                Id = 5, 
                TableNumber = "T-05", 
                Capacity = 2, 
                Location = "Ground Floor", 
                Status = "available", 
                CreatedAt = baseDate 
            },
            new Domain.Entities.RestaurantTable 
            { 
                Id = 6, 
                TableNumber = "T-06", 
                Capacity = 8, 
                Location = "Ground Floor", 
                Status = "available", 
                CreatedAt = baseDate 
            },
            // First Floor Tables
            new Domain.Entities.RestaurantTable 
            { 
                Id = 7, 
                TableNumber = "T-11", 
                Capacity = 2, 
                Location = "First Floor", 
                Status = "available", 
                CreatedAt = baseDate 
            },
            new Domain.Entities.RestaurantTable 
            { 
                Id = 8, 
                TableNumber = "T-12", 
                Capacity = 4, 
                Location = "First Floor", 
                Status = "available", 
                CreatedAt = baseDate 
            },
            new Domain.Entities.RestaurantTable 
            { 
                Id = 9, 
                TableNumber = "T-13", 
                Capacity = 4, 
                Location = "First Floor", 
                Status = "available", 
                CreatedAt = baseDate 
            },
            new Domain.Entities.RestaurantTable 
            { 
                Id = 10, 
                TableNumber = "T-14", 
                Capacity = 6, 
                Location = "First Floor", 
                Status = "available", 
                CreatedAt = baseDate 
            },
            new Domain.Entities.RestaurantTable 
            { 
                Id = 11, 
                TableNumber = "T-15", 
                Capacity = 2, 
                Location = "First Floor", 
                Status = "available", 
                CreatedAt = baseDate 
            },
            // Outdoor Patio Tables
            new Domain.Entities.RestaurantTable 
            { 
                Id = 12, 
                TableNumber = "T-P1", 
                Capacity = 4, 
                Location = "Outdoor Patio", 
                Status = "available", 
                CreatedAt = baseDate 
            },
            new Domain.Entities.RestaurantTable 
            { 
                Id = 13, 
                TableNumber = "T-P2", 
                Capacity = 4, 
                Location = "Outdoor Patio", 
                Status = "available", 
                CreatedAt = baseDate 
            },
            new Domain.Entities.RestaurantTable 
            { 
                Id = 14, 
                TableNumber = "T-P3", 
                Capacity = 6, 
                Location = "Outdoor Patio", 
                Status = "available", 
                CreatedAt = baseDate 
            },
            // VIP Room Tables
            new Domain.Entities.RestaurantTable 
            { 
                Id = 15, 
                TableNumber = "T-VIP1", 
                Capacity = 10, 
                Location = "VIP Room", 
                Status = "available", 
                CreatedAt = baseDate 
            },
            new Domain.Entities.RestaurantTable 
            { 
                Id = 16, 
                TableNumber = "T-VIP2", 
                Capacity = 12, 
                Location = "VIP Room", 
                Status = "available", 
                CreatedAt = baseDate 
            }
        );

        // TODO: ProductImage seed data removed to fix auto-increment issue
        // Explicit IDs in HasData() conflict with ValueGeneratedOnAdd()
        // Consider using a data seeding service instead
        /*
        // Seed ProductImages - IDs will be auto-generated
        modelBuilder.Entity<Domain.Entities.ProductImage>().HasData(
            new Domain.Entities.ProductImage 
            { 
                ProductId = 1, 
                ImageUrl = "/images/products/mouse-wireless-main.jpg",
                AltText = "Wireless Mouse - Main View",
                SortOrder = 1,
                IsPrimary = true,
                CreatedAt = baseDate 
            },
            new Domain.Entities.ProductImage 
            { 
                ProductId = 1, 
                ImageUrl = "/images/products/mouse-wireless-side.jpg",
                AltText = "Wireless Mouse - Side View",
                SortOrder = 2,
                IsPrimary = false,
                CreatedAt = baseDate 
            },
            new Domain.Entities.ProductImage 
            { 
                ProductId = 2, 
                ImageUrl = "/images/products/headphones-bluetooth-main.jpg",
                AltText = "Bluetooth Headphones - Main View",
                SortOrder = 1,
                IsPrimary = true,
                CreatedAt = baseDate 
            },
            new Domain.Entities.ProductImage 
            { 
                ProductId = 3, 
                ImageUrl = "/images/products/tshirt-cotton-main.jpg",
                AltText = "Cotton T-Shirt - Main View",
                SortOrder = 1,
                IsPrimary = true,
                CreatedAt = baseDate 
            },
            new Domain.Entities.ProductImage 
            { 
                ProductId = 4, 
                ImageUrl = "/images/products/coffee-beans-main.jpg",
                AltText = "Coffee Beans - Package View",
                SortOrder = 1,
                IsPrimary = true,
                CreatedAt = baseDate 
            }
        );
        */

        // Configure GoodsReceived entity
        modelBuilder.Entity<Domain.Entities.GoodsReceived>(entity =>
        {
            entity.HasKey(gr => gr.Id);
            entity.ToTable("goods_received");
            
            entity.Property(gr => gr.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
                
            entity.Property(gr => gr.GrnNo)
                .HasColumnName("grn_no")
                .HasMaxLength(50)
                .IsRequired();
                
            entity.Property(gr => gr.SupplierId)
                .HasColumnName("supplier_id")
                .IsRequired();
                
            entity.Property(gr => gr.StoreId)
                .HasColumnName("store_id")
                .IsRequired();
                
            entity.Property(gr => gr.InvoiceNo)
                .HasColumnName("invoice_no")
                .HasMaxLength(50);
                
            entity.Property(gr => gr.InvoiceDate)
                .HasColumnName("invoice_date");
                
            entity.Property(gr => gr.ReceivedDate)
                .HasColumnName("received_date")
                .IsRequired()
                .HasDefaultValueSql("CURRENT_DATE");
                
            entity.Property(gr => gr.TotalAmount)
                .HasColumnName("total_amount")
                .HasColumnType("decimal(12,2)")
                .HasDefaultValue(0);
                
            entity.Property(gr => gr.Remarks)
                .HasColumnName("remarks")
                .HasMaxLength(255);
                
            entity.Property(gr => gr.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
                
            entity.Property(gr => gr.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Configure relationships
            entity.HasOne(gr => gr.Supplier)
                .WithMany()
                .HasForeignKey(gr => gr.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(gr => gr.Store)
                .WithMany()
                .HasForeignKey(gr => gr.StoreId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasMany(gr => gr.Items)
                .WithOne(gri => gri.GoodsReceived)
                .HasForeignKey(gri => gri.GrnId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure GoodsReceivedItem entity
        modelBuilder.Entity<Domain.Entities.GoodsReceivedItem>(entity =>
        {
            entity.HasKey(gri => gri.Id);
            entity.ToTable("goods_received_items");
            
            entity.Property(gri => gri.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
                
            entity.Property(gri => gri.GrnId)
                .HasColumnName("grn_id")
                .IsRequired();
                
            entity.Property(gri => gri.ProductId)
                .HasColumnName("product_id")
                .IsRequired();
                
            entity.Property(gri => gri.BatchId)
                .HasColumnName("batch_id");
                
            entity.Property(gri => gri.BatchNo)
                .HasColumnName("batch_no")
                .HasMaxLength(50);
                
            entity.Property(gri => gri.ManufactureDate)
                .HasColumnName("manufacture_date");
                
            entity.Property(gri => gri.ExpiryDate)
                .HasColumnName("expiry_date");
                
            entity.Property(gri => gri.Quantity)
                .HasColumnName("quantity")
                .HasColumnType("decimal(12,4)")
                .IsRequired();
                
            entity.Property(gri => gri.UomId)
                .HasColumnName("uom_id")
                .IsRequired();
                
            entity.Property(gri => gri.CostPrice)
                .HasColumnName("cost_price")
                .HasColumnType("decimal(12,2)")
                .IsRequired();
                
            entity.Property(gri => gri.LandedCost)
                .HasColumnName("landed_cost")
                .HasColumnType("decimal(12,2)");
                
            entity.Property(gri => gri.LineTotal)
                .HasColumnName("line_total")
                .HasColumnType("decimal(12,2)")
                .HasComputedColumnSql("quantity * cost_price", stored: true);
                
            entity.Property(gri => gri.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Configure relationships
            entity.HasOne(gri => gri.GoodsReceived)
                .WithMany(gr => gr.Items)
                .HasForeignKey(gri => gri.GrnId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(gri => gri.Product)
                .WithMany()
                .HasForeignKey(gri => gri.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(gri => gri.ProductBatch)
                .WithMany()
                .HasForeignKey(gri => gri.BatchId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(gri => gri.UnitOfMeasurement)
                .WithMany()
                .HasForeignKey(gri => gri.UomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure RestaurantTable entity
        modelBuilder.Entity<Domain.Entities.RestaurantTable>(entity =>
        {
            entity.ToTable("restaurant_tables");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TableNumber).IsRequired().HasMaxLength(10).HasColumnName("table_number");
            entity.Property(e => e.Capacity).IsRequired().HasColumnName("capacity");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("available").HasColumnName("status");
            entity.Property(e => e.Location).HasMaxLength(50).HasColumnName("location");
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");

            // Unique constraint on table number
            entity.HasIndex(e => e.TableNumber).IsUnique();

            // Index on status for quick filtering
            entity.HasIndex(e => e.Status);
        });

        // Configure Reservation entity
        modelBuilder.Entity<Domain.Entities.Reservation>(entity =>
        {
            entity.ToTable("reservation");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CustomerId).IsRequired().HasColumnName("customer_id");
            entity.Property(e => e.TableId).IsRequired().HasColumnName("table_id");
            entity.Property(e => e.NumberOfPersons).IsRequired().HasColumnName("number_of_persons");
            entity.Property(e => e.ReservationDate).IsRequired().HasColumnName("reservation_date");
            entity.Property(e => e.ReservationTime).IsRequired().HasColumnName("reservation_time");
            entity.Property(e => e.DepositFee).HasPrecision(10, 2).HasDefaultValue(0).HasColumnName("deposit_fee");
            entity.Property(e => e.PaymentTypeId).HasColumnName("payment_type_id");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("waiting").HasColumnName("status");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false).HasColumnName("is_deleted");
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");

            // Foreign key relationship with Customer
            entity.HasOne(r => r.Customer)
                  .WithMany()
                  .HasForeignKey(r => r.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Foreign key relationship with RestaurantTable
            entity.HasOne(r => r.Table)
                  .WithMany(t => t.Reservations)
                  .HasForeignKey(r => r.TableId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Foreign key relationship with PaymentType (nullable)
            entity.HasOne(r => r.PaymentType)
                  .WithMany()
                  .HasForeignKey(r => r.PaymentTypeId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes for performance
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.TableId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ReservationDate);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => new { e.TableId, e.ReservationDate });
        });

        // Configure Order entity
        modelBuilder.Entity<Domain.Entities.Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TableId).HasColumnName("table_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.ReservationId).HasColumnName("reservation_id");
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2).HasDefaultValue(0.00m).HasColumnName("total_amount");
            entity.Property(e => e.Discount).HasPrecision(10, 2).HasDefaultValue(0.00m).HasColumnName("discount");
            entity.Property(e => e.PaymentTypeId).HasColumnName("payment_type_id");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("pending").HasColumnName("status");
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");

            // Ignore computed property
            entity.Ignore(e => e.FinalAmount);

            // Foreign key relationship with RestaurantTable (nullable)
            entity.HasOne(o => o.Table)
                  .WithMany()
                  .HasForeignKey(o => o.TableId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Foreign key relationship with Customer (nullable)
            entity.HasOne(o => o.Customer)
                  .WithMany()
                  .HasForeignKey(o => o.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Foreign key relationship with Reservation (nullable)
            entity.HasOne(o => o.Reservation)
                  .WithMany()
                  .HasForeignKey(o => o.ReservationId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Foreign key relationship with PaymentType (nullable)
            entity.HasOne(o => o.PaymentType)
                  .WithMany()
                  .HasForeignKey(o => o.PaymentTypeId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes for performance
            entity.HasIndex(e => e.TableId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.ReservationId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure OrderItem entity
        modelBuilder.Entity<Domain.Entities.OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).IsRequired().HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.MenuItemId).HasColumnName("menu_item_id");
            entity.Property(e => e.Quantity).IsRequired().HasColumnName("quantity");
            entity.Property(e => e.Price).HasPrecision(10, 2).IsRequired().HasColumnName("price");
            entity.Property(e => e.Notes).HasMaxLength(255).HasColumnName("notes");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("pending").HasColumnName("status");

            // Ignore computed property
            entity.Ignore(e => e.LineTotal);

            // Foreign key relationship with Order
            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Foreign key relationship with Product (nullable)
            entity.HasOne(oi => oi.Product)
                  .WithMany()
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Note: MenuItem relationship will be added when MenuItem entity is created
            // entity.HasOne(oi => oi.MenuItem)
            //       .WithMany()
            //       .HasForeignKey(oi => oi.MenuItemId)
            //       .OnDelete(DeleteBehavior.SetNull);

            // Indexes for performance
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.MenuItemId);
            entity.HasIndex(e => e.Status);
        });

        // Configure ProductModifier entity
        modelBuilder.Entity<Domain.Entities.ProductModifier>(entity =>
        {
            entity.ToTable("product_modifiers");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price).HasPrecision(10, 2).HasDefaultValue(0.00m).HasColumnName("price");
            entity.Property(e => e.Cost).HasPrecision(10, 2).HasDefaultValue(0.00m).HasColumnName("cost");
            entity.Property(e => e.Sku).HasMaxLength(50).HasColumnName("sku");
            entity.Property(e => e.Barcode).HasMaxLength(50).HasColumnName("barcode");
            entity.Property(e => e.TaxTypeId).HasColumnName("tax_type_id");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active").HasColumnName("status");
            entity.Property(e => e.CreatedBy).IsRequired().HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");

            // Foreign key relationship with TaxType (nullable)
            entity.HasOne(m => m.TaxType)
                  .WithMany()
                  .HasForeignKey(m => m.TaxTypeId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Foreign key relationship with User (creator)
            entity.HasOne(m => m.Creator)
                  .WithMany()
                  .HasForeignKey(m => m.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.HasIndex(e => e.Barcode).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.TaxTypeId);
        });

        // Configure ProductModifierGroup entity
        modelBuilder.Entity<Domain.Entities.ProductModifierGroup>(entity =>
        {
            entity.ToTable("product_modifier_groups");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.SelectionType).IsRequired().HasMaxLength(20).HasDefaultValue("Multiple").HasColumnName("selection_type");
            entity.Property(e => e.Required).HasDefaultValue(false).HasColumnName("required");
            entity.Property(e => e.MinSelections).HasDefaultValue(0).HasColumnName("min_selections");
            entity.Property(e => e.MaxSelections).HasColumnName("max_selections");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active").HasColumnName("status");
            entity.Property(e => e.CreatedBy).IsRequired().HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");

            // Foreign key relationship with User (creator)
            entity.HasOne(g => g.Creator)
                  .WithMany()
                  .HasForeignKey(g => g.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.SelectionType);
            entity.HasIndex(e => e.Required);
            entity.HasIndex(e => e.Status);
        });

        // Configure ProductModifierGroupItem entity
        modelBuilder.Entity<Domain.Entities.ProductModifierGroupItem>(entity =>
        {
            entity.ToTable("product_modifier_group_items");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GroupId).IsRequired().HasColumnName("group_id");
            entity.Property(e => e.ModifierId).IsRequired().HasColumnName("modifier_id");
            entity.Property(e => e.PriceAdjustment).HasPrecision(10, 2).HasDefaultValue(0.00m).HasColumnName("price_adjustment");
            entity.Property(e => e.SortOrder).HasDefaultValue(0).HasColumnName("sort_order");
            entity.Property(e => e.DefaultSelection).HasDefaultValue(false).HasColumnName("default_selection");
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active").HasColumnName("status");
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");

            // Foreign key relationship with ProductModifierGroup
            entity.HasOne(i => i.Group)
                  .WithMany(g => g.GroupItems)
                  .HasForeignKey(i => i.GroupId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Foreign key relationship with ProductModifier
            entity.HasOne(i => i.Modifier)
                  .WithMany(m => m.ModifierGroupItems)
                  .HasForeignKey(i => i.ModifierId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.GroupId);
            entity.HasIndex(e => e.ModifierId);
            entity.HasIndex(e => e.SortOrder);
            entity.HasIndex(e => e.Status);
            
            // Composite unique index to prevent duplicate modifier in same group
            entity.HasIndex(e => new { e.GroupId, e.ModifierId }).IsUnique();
        });

        // Configure ProductModifierLink entity
        modelBuilder.Entity<Domain.Entities.ProductModifierLink>(entity =>
        {
            entity.ToTable("product_modifier_links");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductId).IsRequired().HasColumnName("product_id");
            entity.Property(e => e.ModifierGroupId).IsRequired().HasColumnName("modifier_group_id");
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");

            // Foreign key relationship with Product
            entity.HasOne(l => l.Product)
                  .WithMany()
                  .HasForeignKey(l => l.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Foreign key relationship with ProductModifierGroup
            entity.HasOne(l => l.ModifierGroup)
                  .WithMany()
                  .HasForeignKey(l => l.ModifierGroupId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.ModifierGroupId);
            
            // Composite unique index to prevent duplicate links
            entity.HasIndex(e => new { e.ProductId, e.ModifierGroupId }).IsUnique();
        });

        // Configure Shift entity
        modelBuilder.Entity<Domain.Entities.Shift>(entity =>
        {
            entity.HasKey(e => e.ShiftId);
            entity.Property(e => e.UserId).IsRequired(false); // Made optional - no user dependency
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.OpeningCash).HasPrecision(12, 2);
            entity.Property(e => e.ClosingCash).HasPrecision(12, 2);
            entity.Property(e => e.ExpectedCash).HasPrecision(12, 2);
            entity.Property(e => e.CashDifference).HasPrecision(12, 2);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Open");
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationships - User is optional
            entity.HasOne(s => s.User)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired(false);

            entity.HasOne(s => s.ShopLocation)
                  .WithMany()
                  .HasForeignKey(s => s.ShopLocationId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ShopLocationId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartTime);
        });

        // Configure ServiceCharge entity
        modelBuilder.Entity<Domain.Entities.ServiceCharge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NameArabic).HasMaxLength(100);
            entity.Property(e => e.IsPercentage).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.Value).IsRequired().HasPrecision(10, 4);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationship with TaxType
            entity.HasOne(sc => sc.TaxType)
                  .WithMany()
                  .HasForeignKey(sc => sc.TaxTypeId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.AutoApply);
        });

        // Configure Transaction entity
        modelBuilder.Entity<Domain.Entities.Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ShiftId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.SellingTime).IsRequired();
            entity.Property(e => e.TotalAmount).HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.TotalVat).HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.TotalDiscount).HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.TotalAppliedVat).HasPrecision(12, 2);
            entity.Property(e => e.TotalAppliedDiscountValue).HasPrecision(12, 2);
            entity.Property(e => e.AmountPaidCash).HasPrecision(12, 2);
            entity.Property(e => e.AmountCreditRemaining).HasPrecision(12, 2);
            entity.Property(e => e.DiscountValue).HasPrecision(12, 2);
            entity.Property(e => e.DiscountMaxValue).HasPrecision(12, 2);
            entity.Property(e => e.Vat).HasPrecision(12, 2);
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
            // Status values: draft, billed, settled, hold, cancelled, pending_payment, partial_payment, refunded, exchanged
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("draft");
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(t => t.Shift)
                  .WithMany(s => s.Transactions)
                  .HasForeignKey(t => t.ShiftId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Customer)
                  .WithMany()
                  .HasForeignKey(t => t.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.User)
                  .WithMany()
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.ShopLocation)
                  .WithMany()
                  .HasForeignKey(t => t.ShopLocationId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.Table)
                  .WithMany()
                  .HasForeignKey(t => t.TableId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.Reservation)
                  .WithMany()
                  .HasForeignKey(t => t.ReservationId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.Creator)
                  .WithMany()
                  .HasForeignKey(t => t.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.ShiftId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.SellingTime);
            entity.HasIndex(e => e.TableId);
        });

        // Configure TransactionProduct entity
        modelBuilder.Entity<Domain.Entities.TransactionProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionId).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.BuyerCost).HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.SellingPrice).HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.DiscountValue).HasPrecision(12, 2);
            entity.Property(e => e.DiscountMaxValue).HasPrecision(12, 2);
            entity.Property(e => e.Vat).HasPrecision(12, 2);
            entity.Property(e => e.Quantity).HasPrecision(10, 3).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("active");
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(tp => tp.Transaction)
                  .WithMany(t => t.TransactionProducts)
                  .HasForeignKey(tp => tp.TransactionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tp => tp.Product)
                  .WithMany()
                  .HasForeignKey(tp => tp.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(tp => tp.ProductUnit)
                  .WithMany()
                  .HasForeignKey(tp => tp.ProductUnitId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.TransactionId);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.Status);
        });

        // Configure TransactionModifier entity
        modelBuilder.Entity<Domain.Entities.TransactionModifier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionProductId).IsRequired();
            entity.Property(e => e.ProductModifierId).IsRequired();
            entity.Property(e => e.ExtraPrice).HasPrecision(10, 2).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(tm => tm.TransactionProduct)
                  .WithMany(tp => tp.TransactionModifiers)
                  .HasForeignKey(tm => tm.TransactionProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tm => tm.ProductModifier)
                  .WithMany()
                  .HasForeignKey(tm => tm.ProductModifierId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.TransactionProductId);
            entity.HasIndex(e => e.ProductModifierId);
        });

        // Configure TransactionServiceCharge entity
        modelBuilder.Entity<Domain.Entities.TransactionServiceCharge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionId).IsRequired();
            entity.Property(e => e.ServiceChargeId).IsRequired();
            entity.Property(e => e.TotalAmount).HasPrecision(12, 2);
            entity.Property(e => e.TotalVat).HasPrecision(12, 2);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(tsc => tsc.Transaction)
                  .WithMany(t => t.TransactionServiceCharges)
                  .HasForeignKey(tsc => tsc.TransactionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tsc => tsc.ServiceCharge)
                  .WithMany(sc => sc.TransactionServiceCharges)
                  .HasForeignKey(tsc => tsc.ServiceChargeId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.TransactionId);
            entity.HasIndex(e => e.ServiceChargeId);
        });

        // Configure RefundTransaction entity
        modelBuilder.Entity<Domain.Entities.RefundTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SellingTransactionId).IsRequired();
            entity.Property(e => e.TotalAmount).HasPrecision(12, 2);
            entity.Property(e => e.TotalVat).HasPrecision(12, 2);
            entity.Property(e => e.RefundTime).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(rt => rt.Customer)
                  .WithMany()
                  .HasForeignKey(rt => rt.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(rt => rt.SellingTransaction)
                  .WithMany(t => t.RefundTransactions)
                  .HasForeignKey(rt => rt.SellingTransactionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(rt => rt.Shift)
                  .WithMany(s => s.RefundTransactions)
                  .HasForeignKey(rt => rt.ShiftId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(rt => rt.User)
                  .WithMany()
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.SellingTransactionId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.ShiftId);
            entity.HasIndex(e => e.RefundTime);
        });

        // Configure RefundTransactionProduct entity
        modelBuilder.Entity<Domain.Entities.RefundTransactionProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RefundTransactionId).IsRequired();
            entity.Property(e => e.TransactionProductId).IsRequired();
            entity.Property(e => e.TotalQuantityReturned).HasPrecision(10, 3);
            entity.Property(e => e.TotalVat).HasPrecision(12, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(12, 2);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(rtp => rtp.RefundTransaction)
                  .WithMany(rt => rt.RefundTransactionProducts)
                  .HasForeignKey(rtp => rtp.RefundTransactionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rtp => rtp.TransactionProduct)
                  .WithMany(tp => tp.RefundTransactionProducts)
                  .HasForeignKey(rtp => rtp.TransactionProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.RefundTransactionId);
            entity.HasIndex(e => e.TransactionProductId);
        });

        // Configure ExchangeTransaction entity
        modelBuilder.Entity<Domain.Entities.ExchangeTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SellingTransactionId).IsRequired();
            entity.Property(e => e.TotalExchangedAmount).HasPrecision(12, 2);
            entity.Property(e => e.TotalExchangedVat).HasPrecision(12, 2);
            entity.Property(e => e.ProductExchangedQuantity).HasPrecision(10, 3);
            entity.Property(e => e.ExchangeTime).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(et => et.Customer)
                  .WithMany()
                  .HasForeignKey(et => et.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(et => et.SellingTransaction)
                  .WithMany(t => t.ExchangeTransactions)
                  .HasForeignKey(et => et.SellingTransactionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(et => et.Shift)
                  .WithMany(s => s.ExchangeTransactions)
                  .HasForeignKey(et => et.ShiftId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.SellingTransactionId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.ShiftId);
            entity.HasIndex(e => e.ExchangeTime);
        });

        // Configure ExchangeTransactionProduct entity
        modelBuilder.Entity<Domain.Entities.ExchangeTransactionProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExchangeTransactionId).IsRequired();
            entity.Property(e => e.ReturnedQuantity).HasPrecision(10, 3);
            entity.Property(e => e.NewQuantity).HasPrecision(10, 3);
            entity.Property(e => e.PriceDifference).HasPrecision(12, 2);
            entity.Property(e => e.OldProductAmount).HasPrecision(12, 2);
            entity.Property(e => e.NewProductAmount).HasPrecision(12, 2);
            entity.Property(e => e.VatDifference).HasPrecision(12, 2);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationships
            entity.HasOne(etp => etp.ExchangeTransaction)
                  .WithMany(et => et.ExchangeTransactionProducts)
                  .HasForeignKey(etp => etp.ExchangeTransactionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(etp => etp.OriginalTransactionProduct)
                  .WithMany(tp => tp.OriginalExchangeTransactionProducts)
                  .HasForeignKey(etp => etp.OriginalTransactionProductId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(etp => etp.NewProduct)
                  .WithMany()
                  .HasForeignKey(etp => etp.NewProductId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.ExchangeTransactionId);
            entity.HasIndex(e => e.OriginalTransactionProductId);
            entity.HasIndex(e => e.NewProductId);
        });
    }
}
