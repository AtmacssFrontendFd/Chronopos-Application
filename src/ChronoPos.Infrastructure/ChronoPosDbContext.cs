using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Enums;

namespace ChronoPos.Infrastructure;

/// <summary>
/// Database context for ChronoPos application using SQLite
/// </summary>
public class ChronoPosDbContext : DbContext
{
    public ChronoPosDbContext(DbContextOptions<ChronoPosDbContext> options) : base(options)
    {
    }

    // DbSets for entities
    public DbSet<Domain.Entities.Product> Products { get; set; }
    public DbSet<Domain.Entities.Category> Categories { get; set; }
    public DbSet<Domain.Entities.Customer> Customers { get; set; }
    public DbSet<Domain.Entities.Sale> Sales { get; set; }
    public DbSet<Domain.Entities.SaleItem> SaleItems { get; set; }
    
    // Product related entities
    public DbSet<Domain.Entities.ProductBarcode> ProductBarcodes { get; set; }
    public DbSet<Domain.Entities.ProductComment> ProductComments { get; set; }
    public DbSet<Domain.Entities.ProductTax> ProductTaxes { get; set; }
    public DbSet<Domain.Entities.MeasurementUnit> MeasurementUnits { get; set; }
    public DbSet<Domain.Entities.Tax> Taxes { get; set; }
    
    // Stock management entities
    public DbSet<Domain.Entities.StockTransaction> StockTransactions { get; set; }
    public DbSet<Domain.Entities.StockAlert> StockAlerts { get; set; }
    public DbSet<Domain.Entities.Store> Stores { get; set; }
    public DbSet<Domain.Entities.StockLevel> StockLevels { get; set; }
    
    // Language system entities
    public DbSet<Domain.Entities.Language> Languages { get; set; }
    public DbSet<Domain.Entities.LanguageKeyword> LanguageKeywords { get; set; }
    public DbSet<Domain.Entities.LabelTranslation> LabelTranslations { get; set; }

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
            entity.Property(e => e.Excise).HasPrecision(18, 2);
            entity.Property(e => e.MaxDiscount).HasPrecision(5, 2);
            entity.Property(e => e.SKU).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.MeasurementUnit).HasMaxLength(10);
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
            entity.Property(e => e.Value).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Unique constraint on barcode value
            entity.HasIndex(e => e.Value).IsUnique();

            // Foreign key relationship with Product
            entity.HasOne(pb => pb.Product)
                  .WithMany(p => p.ProductBarcodes)
                  .HasForeignKey(pb => pb.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
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

        // Configure MeasurementUnit entity
        modelBuilder.Entity<Domain.Entities.MeasurementUnit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Symbol).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Unique constraint on name
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure Tax entity
        modelBuilder.Entity<Domain.Entities.Tax>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Rate).HasPrecision(5, 2);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Unique constraint on name
            entity.HasIndex(e => e.Name).IsUnique();
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

            // Foreign key relationship with Tax
            entity.HasOne(pt => pt.Tax)
                  .WithMany()
                  .HasForeignKey(pt => pt.TaxId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint on Product-Tax combination
            entity.HasIndex(e => new { e.ProductId, e.TaxId }).IsUnique();
        });

        // Configure Customer entity
        modelBuilder.Entity<Domain.Entities.Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Index on email for quick lookup
            entity.HasIndex(e => e.Email).IsUnique();
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

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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

        // Seed Products
        modelBuilder.Entity<Domain.Entities.Product>().HasData(
            new Domain.Entities.Product 
            { 
                Id = 1, 
                Code = "MOUSE001", 
                PLU = 1001,
                Name = "Wireless Mouse", 
                Description = "Ergonomic wireless mouse", 
                Price = 25.99m, 
                Cost = 15.00m,
                CategoryId = 1, 
                StockQuantity = 50, 
                MeasurementUnit = "pcs",
                IsActive = true,
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            },
            new Domain.Entities.Product 
            { 
                Id = 2, 
                Code = "HEAD001", 
                PLU = 1002,
                Name = "Bluetooth Headphones", 
                Description = "Noise-cancelling headphones", 
                Price = 89.99m, 
                Cost = 60.00m,
                CategoryId = 1, 
                StockQuantity = 30, 
                MeasurementUnit = "pcs",
                IsActive = true,
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            },
            new Domain.Entities.Product 
            { 
                Id = 3, 
                Code = "SHIRT001", 
                PLU = 1003,
                Name = "Cotton T-Shirt", 
                Description = "100% cotton comfortable t-shirt", 
                Price = 19.99m, 
                Cost = 12.00m,
                CategoryId = 2, 
                StockQuantity = 100, 
                MeasurementUnit = "pcs",
                IsActive = true,
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            },
            new Domain.Entities.Product 
            { 
                Id = 4, 
                Code = "COFFEE001", 
                PLU = 1004,
                Name = "Coffee Beans", 
                Description = "Premium arabica coffee beans", 
                Price = 12.99m, 
                Cost = 8.00m,
                CategoryId = 3, 
                StockQuantity = 75, 
                MeasurementUnit = "kg",
                IsActive = true,
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            },
            new Domain.Entities.Product 
            { 
                Id = 5, 
                Code = "BOOK001", 
                PLU = 1005,
                Name = "Programming Guide", 
                Description = "Complete C# programming guide", 
                Price = 39.99m, 
                Cost = 25.00m,
                CategoryId = 4, 
                StockQuantity = 25, 
                MeasurementUnit = "pcs",
                IsActive = true,
                CreatedAt = baseDate, 
                UpdatedAt = baseDate 
            }
        );

        // Seed MeasurementUnits
        modelBuilder.Entity<Domain.Entities.MeasurementUnit>().HasData(
            new Domain.Entities.MeasurementUnit { Id = 1, Name = "Pieces", Symbol = "pcs", IsDefault = true, IsActive = true, CreatedAt = baseDate },
            new Domain.Entities.MeasurementUnit { Id = 2, Name = "Kilograms", Symbol = "kg", IsDefault = false, IsActive = true, CreatedAt = baseDate },
            new Domain.Entities.MeasurementUnit { Id = 3, Name = "Pounds", Symbol = "lbs", IsDefault = false, IsActive = true, CreatedAt = baseDate },
            new Domain.Entities.MeasurementUnit { Id = 4, Name = "Liters", Symbol = "L", IsDefault = false, IsActive = true, CreatedAt = baseDate },
            new Domain.Entities.MeasurementUnit { Id = 5, Name = "Meters", Symbol = "m", IsDefault = false, IsActive = true, CreatedAt = baseDate }
        );

        // Seed Taxes
        modelBuilder.Entity<Domain.Entities.Tax>().HasData(
            new Domain.Entities.Tax { Id = 1, Name = "VAT", Rate = 10.00m, IsPercentage = true, IsDefault = true, IsActive = true, CreatedAt = baseDate },
            new Domain.Entities.Tax { Id = 2, Name = "Sales Tax", Rate = 5.00m, IsPercentage = true, IsDefault = false, IsActive = true, CreatedAt = baseDate },
            new Domain.Entities.Tax { Id = 3, Name = "Luxury Tax", Rate = 15.00m, IsPercentage = true, IsDefault = false, IsActive = true, CreatedAt = baseDate }
        );

        // Seed ProductBarcodes
        modelBuilder.Entity<Domain.Entities.ProductBarcode>().HasData(
            new Domain.Entities.ProductBarcode { Id = 1, ProductId = 1, Value = "1234567890123", CreatedAt = baseDate },
            new Domain.Entities.ProductBarcode { Id = 2, ProductId = 1, Value = "MOUSE001BC", CreatedAt = baseDate },
            new Domain.Entities.ProductBarcode { Id = 3, ProductId = 2, Value = "2345678901234", CreatedAt = baseDate },
            new Domain.Entities.ProductBarcode { Id = 4, ProductId = 3, Value = "3456789012345", CreatedAt = baseDate },
            new Domain.Entities.ProductBarcode { Id = 5, ProductId = 4, Value = "4567890123456", CreatedAt = baseDate },
            new Domain.Entities.ProductBarcode { Id = 6, ProductId = 5, Value = "5678901234567", CreatedAt = baseDate }
        );

        // Seed ProductComments
        modelBuilder.Entity<Domain.Entities.ProductComment>().HasData(
            new Domain.Entities.ProductComment { Id = 1, ProductId = 1, Comment = "Popular item - stock frequently", CreatedBy = "Admin", CreatedAt = baseDate },
            new Domain.Entities.ProductComment { Id = 2, ProductId = 2, Comment = "Check battery life before selling", CreatedBy = "Admin", CreatedAt = baseDate }
        );

        // Seed ProductTaxes
        modelBuilder.Entity<Domain.Entities.ProductTax>().HasData(
            new Domain.Entities.ProductTax { Id = 1, ProductId = 1, TaxId = 1, CreatedAt = baseDate }, // Mouse with VAT
            new Domain.Entities.ProductTax { Id = 2, ProductId = 2, TaxId = 1, CreatedAt = baseDate }, // Headphones with VAT
            new Domain.Entities.ProductTax { Id = 3, ProductId = 2, TaxId = 3, CreatedAt = baseDate }, // Headphones with Luxury Tax
            new Domain.Entities.ProductTax { Id = 4, ProductId = 3, TaxId = 2, CreatedAt = baseDate }, // T-Shirt with Sales Tax
            new Domain.Entities.ProductTax { Id = 5, ProductId = 4, TaxId = 1, CreatedAt = baseDate }, // Coffee with VAT
            new Domain.Entities.ProductTax { Id = 6, ProductId = 5, TaxId = 2, CreatedAt = baseDate }  // Book with Sales Tax
        );

        // Seed Customers
        modelBuilder.Entity<Domain.Entities.Customer>().HasData(
            new Domain.Entities.Customer { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@email.com", PhoneNumber = "555-0101", Address = "123 Main St, City, State", CreatedAt = baseDate, UpdatedAt = baseDate },
            new Domain.Entities.Customer { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@email.com", PhoneNumber = "555-0102", Address = "456 Oak Ave, City, State", CreatedAt = baseDate, UpdatedAt = baseDate }
        );

        // Seed Languages
        modelBuilder.Entity<Domain.Entities.Language>().HasData(
            new Domain.Entities.Language { Id = 1, LanguageName = "English", LanguageCode = "en", IsRtl = false, Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.Language { Id = 2, LanguageName = "اردو", LanguageCode = "ur", IsRtl = true, Status = "Active", CreatedBy = "System", CreatedAt = baseDate }
        );

        // Seed Language Keywords
        modelBuilder.Entity<Domain.Entities.LanguageKeyword>().HasData(
            // Main Navigation
            new Domain.Entities.LanguageKeyword { Id = 1, Key = "nav.dashboard", Description = "Dashboard navigation item" },
            new Domain.Entities.LanguageKeyword { Id = 2, Key = "nav.management", Description = "Management navigation item" },
            new Domain.Entities.LanguageKeyword { Id = 3, Key = "nav.customers", Description = "Customers navigation item" },
            new Domain.Entities.LanguageKeyword { Id = 4, Key = "nav.sales", Description = "Sales navigation item" },
            new Domain.Entities.LanguageKeyword { Id = 5, Key = "nav.settings", Description = "Settings navigation item" },
            new Domain.Entities.LanguageKeyword { Id = 6, Key = "nav.logout", Description = "Logout button" },

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

            // Management Modules (All 6 original modules)
            new Domain.Entities.LanguageKeyword { Id = 41, Key = "management.stock", Description = "Stock Management" },
            new Domain.Entities.LanguageKeyword { Id = 42, Key = "management.products", Description = "Products" },
            new Domain.Entities.LanguageKeyword { Id = 43, Key = "management.supplier", Description = "Supplier" },
            new Domain.Entities.LanguageKeyword { Id = 44, Key = "management.customers", Description = "Customer Module" },
            new Domain.Entities.LanguageKeyword { Id = 45, Key = "management.payment", Description = "Payment Options" },
            new Domain.Entities.LanguageKeyword { Id = 46, Key = "management.service", Description = "Service Charge" },

            // UI Buttons
            new Domain.Entities.LanguageKeyword { Id = 47, Key = "btn.back", Description = "Back button" },
            new Domain.Entities.LanguageKeyword { Id = 48, Key = "btn.refresh", Description = "Refresh button" }
        );

        // Seed Label Translations - English
        modelBuilder.Entity<Domain.Entities.LabelTranslation>().HasData(
            // Navigation - English
            new Domain.Entities.LabelTranslation { Id = 1, LanguageId = 1, TranslationKey = "nav.dashboard", Value = "Dashboard", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 2, LanguageId = 1, TranslationKey = "nav.management", Value = "Management", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 3, LanguageId = 1, TranslationKey = "nav.customers", Value = "Customers", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 4, LanguageId = 1, TranslationKey = "nav.sales", Value = "Sales", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 5, LanguageId = 1, TranslationKey = "nav.settings", Value = "Settings", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 6, LanguageId = 1, TranslationKey = "nav.logout", Value = "Logout", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

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

            // Management Modules - English (All 6 original modules)
            new Domain.Entities.LabelTranslation { Id = 37, LanguageId = 1, TranslationKey = "management.stock", Value = "Stock Management", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 38, LanguageId = 1, TranslationKey = "management.products", Value = "Products", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 39, LanguageId = 1, TranslationKey = "management.supplier", Value = "Supplier", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 40, LanguageId = 1, TranslationKey = "management.customers", Value = "Customer Module", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 41, LanguageId = 1, TranslationKey = "management.payment", Value = "Payment Options", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 42, LanguageId = 1, TranslationKey = "management.service", Value = "Service Charge", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // UI Buttons - English
            new Domain.Entities.LabelTranslation { Id = 43, LanguageId = 1, TranslationKey = "btn.back", Value = "Back", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 44, LanguageId = 1, TranslationKey = "btn.refresh", Value = "Refresh", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Navigation - Urdu
            new Domain.Entities.LabelTranslation { Id = 45, LanguageId = 2, TranslationKey = "nav.dashboard", Value = "ڈیش بورڈ", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 46, LanguageId = 2, TranslationKey = "nav.management", Value = "انتظام", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 47, LanguageId = 2, TranslationKey = "nav.customers", Value = "گاہک", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 48, LanguageId = 2, TranslationKey = "nav.sales", Value = "فروخت", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 49, LanguageId = 2, TranslationKey = "nav.settings", Value = "ترتیبات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 50, LanguageId = 2, TranslationKey = "nav.logout", Value = "لاگ آؤٹ", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Buttons - Urdu
            new Domain.Entities.LabelTranslation { Id = 51, LanguageId = 2, TranslationKey = "btn.save", Value = "محفوظ کریں", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 52, LanguageId = 2, TranslationKey = "btn.cancel", Value = "منسوخ", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 53, LanguageId = 2, TranslationKey = "btn.edit", Value = "ترمیم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 54, LanguageId = 2, TranslationKey = "btn.delete", Value = "حذف", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 55, LanguageId = 2, TranslationKey = "btn.add", Value = "شامل کریں", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 56, LanguageId = 2, TranslationKey = "btn.search", Value = "تلاش", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Settings - Urdu
            new Domain.Entities.LabelTranslation { Id = 57, LanguageId = 2, TranslationKey = "settings.language", Value = "🌐 زبان کی ترتیبات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 58, LanguageId = 2, TranslationKey = "settings.theme", Value = "🎨 تھیم کی ترتیبات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 59, LanguageId = 2, TranslationKey = "settings.color_scheme", Value = "🎨 رنگ سکیم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 60, LanguageId = 2, TranslationKey = "settings.layout_direction", Value = "📱 لے آؤٹ کی سمت", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 61, LanguageId = 2, TranslationKey = "settings.font", Value = "🔤 فونٹ کی ترتیبات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 62, LanguageId = 2, TranslationKey = "settings.actions", Value = "⚙️ عمل", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Stock Management Modules - Urdu
            new Domain.Entities.LabelTranslation { Id = 63, LanguageId = 2, TranslationKey = "stock.adjustment", Value = "اسٹاک کی تشخیص", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 64, LanguageId = 2, TranslationKey = "stock.transfer", Value = "اسٹاک ٹرانسفر", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 65, LanguageId = 2, TranslationKey = "stock.goods_received", Value = "مال کی آمد", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 66, LanguageId = 2, TranslationKey = "stock.goods_return", Value = "مال کی واپسی", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Management Modules - Urdu (All 6 original modules)
            new Domain.Entities.LabelTranslation { Id = 67, LanguageId = 2, TranslationKey = "management.stock", Value = "اسٹاک انتظام", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 68, LanguageId = 2, TranslationKey = "management.products", Value = "مصنوعات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 69, LanguageId = 2, TranslationKey = "management.supplier", Value = "سپلائر", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 70, LanguageId = 2, TranslationKey = "management.customers", Value = "کسٹمر ماڈیول", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 71, LanguageId = 2, TranslationKey = "management.payment", Value = "ادائیگی کے اختیارات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 72, LanguageId = 2, TranslationKey = "management.service", Value = "سروس چارج", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // UI Buttons - Urdu
            new Domain.Entities.LabelTranslation { Id = 73, LanguageId = 2, TranslationKey = "btn.back", Value = "واپس", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 74, LanguageId = 2, TranslationKey = "btn.refresh", Value = "تازہ کریں", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Products - Urdu
            new Domain.Entities.LabelTranslation { Id = 75, LanguageId = 2, TranslationKey = "products.title", Value = "مصنوعات کا انتظام", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 76, LanguageId = 2, TranslationKey = "products.name", Value = "مصنوع کا نام", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 77, LanguageId = 2, TranslationKey = "products.price", Value = "قیمت", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 78, LanguageId = 2, TranslationKey = "products.category", Value = "قسم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 79, LanguageId = 2, TranslationKey = "products.stock", Value = "اسٹاک", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Common Labels - Urdu
            new Domain.Entities.LabelTranslation { Id = 80, LanguageId = 2, TranslationKey = "label.current", Value = "موجودہ", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 81, LanguageId = 2, TranslationKey = "label.ready", Value = "تیار", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 82, LanguageId = 2, TranslationKey = "theme.light", Value = "ہلکا تھیم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 83, LanguageId = 2, TranslationKey = "theme.dark", Value = "گہرا تھیم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 84, LanguageId = 2, TranslationKey = "layout.ltr", Value = "بائیں سے دائیں (LTR)", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 85, LanguageId = 2, TranslationKey = "layout.rtl", Value = "دائیں سے بائیں (RTL)", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 86, LanguageId = 2, TranslationKey = "font.small", Value = "چھوٹا", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 87, LanguageId = 2, TranslationKey = "font.medium", Value = "درمیانہ", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 88, LanguageId = 2, TranslationKey = "font.large", Value = "بڑا", Status = "Active", CreatedBy = "System", CreatedAt = baseDate }
        );
    }
}
