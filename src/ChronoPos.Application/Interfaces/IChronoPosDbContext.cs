using ChronoPos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Application.Interfaces;

public interface IChronoPosDbContext
{
    DbSet<Product> Products { get; set; }
    DbSet<Category> Categories { get; set; }
    DbSet<Brand> Brands { get; set; }
    DbSet<ProductImage> ProductImages { get; set; }
    DbSet<Customer> Customers { get; set; }
    DbSet<Sale> Sales { get; set; }
    DbSet<SaleItem> SaleItems { get; set; }
    DbSet<StockAdjustment> StockAdjustments { get; set; }
    DbSet<StockAdjustmentItem> StockAdjustmentItems { get; set; }
    DbSet<StockAdjustmentReason> StockAdjustmentReasons { get; set; }
    DbSet<StockTransfer> StockTransfers { get; set; }
    DbSet<StockTransferItem> StockTransferItems { get; set; }
    
    // Product module entities
    DbSet<Store> Stores { get; set; }
    DbSet<StockLevel> StockLevels { get; set; }
    DbSet<StockTransaction> StockTransactions { get; set; }
    DbSet<StockAlert> StockAlerts { get; set; }
    DbSet<ProductBarcode> ProductBarcodes { get; set; }
    DbSet<ProductComment> ProductComments { get; set; }
    DbSet<ProductTax> ProductTaxes { get; set; }
    DbSet<TaxType> TaxTypes { get; set; }
    DbSet<UnitOfMeasurement> UnitsOfMeasurement { get; set; }
    
    // Language support entities
    DbSet<LanguageKeyword> LanguageKeywords { get; set; }
    DbSet<LabelTranslation> LabelTranslations { get; set; }
    DbSet<Language> Languages { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
