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
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

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

        // Seed Products
        modelBuilder.Entity<Domain.Entities.Product>().HasData(
            new Domain.Entities.Product { Id = 1, Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", Price = 25.99m, CategoryId = 1, Stock = 50, CreatedAt = baseDate, UpdatedAt = baseDate },
            new Domain.Entities.Product { Id = 2, Name = "Bluetooth Headphones", Description = "Noise-cancelling headphones", Price = 89.99m, CategoryId = 1, Stock = 30, CreatedAt = baseDate, UpdatedAt = baseDate },
            new Domain.Entities.Product { Id = 3, Name = "Cotton T-Shirt", Description = "100% cotton comfortable t-shirt", Price = 19.99m, CategoryId = 2, Stock = 100, CreatedAt = baseDate, UpdatedAt = baseDate },
            new Domain.Entities.Product { Id = 4, Name = "Coffee Beans", Description = "Premium arabica coffee beans", Price = 12.99m, CategoryId = 3, Stock = 75, CreatedAt = baseDate, UpdatedAt = baseDate },
            new Domain.Entities.Product { Id = 5, Name = "Programming Guide", Description = "Complete C# programming guide", Price = 39.99m, CategoryId = 4, Stock = 25, CreatedAt = baseDate, UpdatedAt = baseDate }
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
            new Domain.Entities.LanguageKeyword { Id = 2, Key = "nav.products", Description = "Products navigation item" },
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
            new Domain.Entities.LanguageKeyword { Id = 36, Key = "font.large", Description = "Large font size" }
        );

        // Seed Label Translations - English
        modelBuilder.Entity<Domain.Entities.LabelTranslation>().HasData(
            // Navigation - English
            new Domain.Entities.LabelTranslation { Id = 1, LanguageId = 1, TranslationKey = "nav.dashboard", Value = "Dashboard", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 2, LanguageId = 1, TranslationKey = "nav.products", Value = "Products", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
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

            // Navigation - Urdu
            new Domain.Entities.LabelTranslation { Id = 50, LanguageId = 2, TranslationKey = "nav.dashboard", Value = "ڈیش بورڈ", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 51, LanguageId = 2, TranslationKey = "nav.products", Value = "مصنوعات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 52, LanguageId = 2, TranslationKey = "nav.customers", Value = "گاہک", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 53, LanguageId = 2, TranslationKey = "nav.sales", Value = "فروخت", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 54, LanguageId = 2, TranslationKey = "nav.settings", Value = "ترتیبات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 55, LanguageId = 2, TranslationKey = "nav.logout", Value = "لاگ آؤٹ", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Buttons - Urdu
            new Domain.Entities.LabelTranslation { Id = 56, LanguageId = 2, TranslationKey = "btn.save", Value = "محفوظ کریں", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 57, LanguageId = 2, TranslationKey = "btn.cancel", Value = "منسوخ", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 58, LanguageId = 2, TranslationKey = "btn.edit", Value = "ترمیم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 59, LanguageId = 2, TranslationKey = "btn.delete", Value = "حذف", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 60, LanguageId = 2, TranslationKey = "btn.add", Value = "شامل کریں", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 61, LanguageId = 2, TranslationKey = "btn.search", Value = "تلاش", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Settings - Urdu
            new Domain.Entities.LabelTranslation { Id = 62, LanguageId = 2, TranslationKey = "settings.language", Value = "🌐 زبان کی ترتیبات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 63, LanguageId = 2, TranslationKey = "settings.theme", Value = "🎨 تھیم کی ترتیبات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 64, LanguageId = 2, TranslationKey = "settings.color_scheme", Value = "🎨 رنگ سکیم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 65, LanguageId = 2, TranslationKey = "settings.layout_direction", Value = "📱 لے آؤٹ کی سمت", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 66, LanguageId = 2, TranslationKey = "settings.font", Value = "🔤 فونٹ کی ترتیبات", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 67, LanguageId = 2, TranslationKey = "settings.actions", Value = "⚙️ عمل", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Products - Urdu
            new Domain.Entities.LabelTranslation { Id = 68, LanguageId = 2, TranslationKey = "products.title", Value = "مصنوعات کا انتظام", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 69, LanguageId = 2, TranslationKey = "products.name", Value = "مصنوع کا نام", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 70, LanguageId = 2, TranslationKey = "products.price", Value = "قیمت", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 71, LanguageId = 2, TranslationKey = "products.category", Value = "قسم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 72, LanguageId = 2, TranslationKey = "products.stock", Value = "اسٹاک", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },

            // Common Labels - Urdu
            new Domain.Entities.LabelTranslation { Id = 73, LanguageId = 2, TranslationKey = "label.current", Value = "موجودہ", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 74, LanguageId = 2, TranslationKey = "label.ready", Value = "تیار", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 75, LanguageId = 2, TranslationKey = "theme.light", Value = "ہلکا تھیم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 76, LanguageId = 2, TranslationKey = "theme.dark", Value = "گہرا تھیم", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 77, LanguageId = 2, TranslationKey = "layout.ltr", Value = "بائیں سے دائیں (LTR)", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 78, LanguageId = 2, TranslationKey = "layout.rtl", Value = "دائیں سے بائیں (RTL)", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 79, LanguageId = 2, TranslationKey = "font.small", Value = "چھوٹا", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 80, LanguageId = 2, TranslationKey = "font.medium", Value = "درمیانہ", Status = "Active", CreatedBy = "System", CreatedAt = baseDate },
            new Domain.Entities.LabelTranslation { Id = 81, LanguageId = 2, TranslationKey = "font.large", Value = "بڑا", Status = "Active", CreatedBy = "System", CreatedAt = baseDate }
        );
    }
}
