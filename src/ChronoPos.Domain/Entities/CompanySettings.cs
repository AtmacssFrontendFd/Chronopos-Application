using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents configuration settings for a company in the POS system
/// </summary>
public class CompanySettings
{
    public int Id { get; set; }
    
    public int? CompanyId { get; set; }
    
    [Required]
    public int CurrencyId { get; set; }
    
    public int? StockValueId { get; set; }
    
    [StringLength(20)]
    public string? PrimaryColor { get; set; }
    
    [StringLength(20)]
    public string? SecondaryColor { get; set; }
    
    [StringLength(50)]
    public string? ClientBackupFrequency { get; set; }
    
    [StringLength(50)]
    public string? AtmacssBackupFrequency { get; set; }
    
    [StringLength(50)]
    public string? RefundType { get; set; }
    
    public int? PeriodOfValidity { get; set; } // Days for validity
    
    public bool AllowReturnCash { get; set; } = false;
    
    public bool AllowCreditNote { get; set; } = false;
    
    public bool AllowExchangeTransaction { get; set; } = false;
    
    public bool HasSkuFormat { get; set; } = false;
    
    public bool HasInvoiceFormat { get; set; } = false;
    
    [StringLength(50)]
    public string? CompanySubscriptionType { get; set; }
    
    public int? InvoiceDefaultLanguageId { get; set; }
    
    public int NumberOfUsers { get; set; } = 1;
    
    // Hardware Configuration - Printer Names
    [StringLength(500)]
    public string? InvoicePrinterName { get; set; }
    
    [StringLength(500)]
    public string? NormalPrinterName { get; set; }
    
    [StringLength(500)]
    public string? BarcodePrinterName { get; set; }
    
    // Hardware Configuration - Scanner/Device Ports
    [StringLength(100)]
    public string? BarcodeScannerPort { get; set; }
    
    [StringLength(100)]
    public string? WeighingMachinePort { get; set; }
    
    // Backup Configuration
    [StringLength(1000)]
    public string? ClientBackupPath { get; set; }
    
    [StringLength(1000)]
    public string? AtmacssBackupPath { get; set; }
    
    public DateTime? LastClientBackup { get; set; }
    
    public DateTime? LastAtmacssBackup { get; set; }
    
    // Deprecated fields (keeping for backward compatibility)
    public int? InvoicePrinters { get; set; }
    
    public int? BarcodeScanners { get; set; }
    
    public int? NormalPrinter { get; set; }
    
    public int? BarcodePrinter { get; set; }
    
    public int? WeighingMachine { get; set; }
    
    [StringLength(50)]
    public string? SellingType { get; set; }
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    // Navigation Properties
    public virtual Company? Company { get; set; }
    
    public virtual Currency Currency { get; set; } = null!;
    
    public virtual Language? InvoiceDefaultLanguage { get; set; }
    
    public virtual User? Creator { get; set; }
    
    public virtual User? Updater { get; set; }
    
    public virtual User? Deleter { get; set; }
}
