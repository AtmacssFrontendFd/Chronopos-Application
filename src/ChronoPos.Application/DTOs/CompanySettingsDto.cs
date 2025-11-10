namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for CompanySettings entity
/// </summary>
public class CompanySettingsDto
{
    public int Id { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int CurrencyId { get; set; }
    public string? CurrencyName { get; set; }
    public string? CurrencySymbol { get; set; }
    public int? StockValueId { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? ClientBackupFrequency { get; set; }
    public string? AtmacssBackupFrequency { get; set; }
    public string? RefundType { get; set; }
    public int? PeriodOfValidity { get; set; }
    public bool AllowReturnCash { get; set; }
    public bool AllowCreditNote { get; set; }
    public bool AllowExchangeTransaction { get; set; }
    public bool HasSkuFormat { get; set; }
    public bool HasInvoiceFormat { get; set; }
    public string? CompanySubscriptionType { get; set; }
    public int? InvoiceDefaultLanguageId { get; set; }
    public string? InvoiceDefaultLanguageName { get; set; }
    public int NumberOfUsers { get; set; }
    
    // Hardware Configuration
    public string? InvoicePrinterName { get; set; }
    public string? NormalPrinterName { get; set; }
    public string? BarcodePrinterName { get; set; }
    public string? BarcodeScannerPort { get; set; }
    public string? WeighingMachinePort { get; set; }
    
    // Backup Configuration
    public string? ClientBackupPath { get; set; }
    public string? AtmacssBackupPath { get; set; }
    public DateTime? LastClientBackup { get; set; }
    public DateTime? LastAtmacssBackup { get; set; }
    
    // Deprecated fields
    public int? InvoicePrinters { get; set; }
    public int? BarcodeScanners { get; set; }
    public int? NormalPrinter { get; set; }
    public int? BarcodePrinter { get; set; }
    public int? WeighingMachine { get; set; }
    public string? SellingType { get; set; }
    public string Status { get; set; } = "Active";
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating new CompanySettings
/// </summary>
public class CreateCompanySettingsDto
{
    public int? CompanyId { get; set; }
    public int CurrencyId { get; set; }
    public int? StockValueId { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? ClientBackupFrequency { get; set; }
    public string? AtmacssBackupFrequency { get; set; }
    public string? RefundType { get; set; }
    public int? PeriodOfValidity { get; set; }
    public bool AllowReturnCash { get; set; } = false;
    public bool AllowCreditNote { get; set; } = false;
    public bool AllowExchangeTransaction { get; set; } = false;
    public bool HasSkuFormat { get; set; } = false;
    public bool HasInvoiceFormat { get; set; } = false;
    public string? CompanySubscriptionType { get; set; }
    public int? InvoiceDefaultLanguageId { get; set; }
    public int NumberOfUsers { get; set; } = 1;
    
    // Hardware Configuration
    public string? InvoicePrinterName { get; set; }
    public string? NormalPrinterName { get; set; }
    public string? BarcodePrinterName { get; set; }
    public string? BarcodeScannerPort { get; set; }
    public string? WeighingMachinePort { get; set; }
    
    // Backup Configuration
    public string? ClientBackupPath { get; set; }
    public string? AtmacssBackupPath { get; set; }
    
    // Deprecated fields
    public int? InvoicePrinters { get; set; }
    public int? BarcodeScanners { get; set; }
    public int? NormalPrinter { get; set; }
    public int? BarcodePrinter { get; set; }
    public int? WeighingMachine { get; set; }
    public string? SellingType { get; set; }
    public string Status { get; set; } = "Active";
}

/// <summary>
/// DTO for updating existing CompanySettings
/// </summary>
public class UpdateCompanySettingsDto
{
    public int? CompanyId { get; set; }
    public int CurrencyId { get; set; }
    public int? StockValueId { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? ClientBackupFrequency { get; set; }
    public string? AtmacssBackupFrequency { get; set; }
    public string? RefundType { get; set; }
    public int? PeriodOfValidity { get; set; }
    public bool AllowReturnCash { get; set; }
    public bool AllowCreditNote { get; set; }
    public bool AllowExchangeTransaction { get; set; }
    public bool HasSkuFormat { get; set; }
    public bool HasInvoiceFormat { get; set; }
    public string? CompanySubscriptionType { get; set; }
    public int? InvoiceDefaultLanguageId { get; set; }
    public int NumberOfUsers { get; set; }
    
    // Hardware Configuration
    public string? InvoicePrinterName { get; set; }
    public string? NormalPrinterName { get; set; }
    public string? BarcodePrinterName { get; set; }
    public string? BarcodeScannerPort { get; set; }
    public string? WeighingMachinePort { get; set; }
    
    // Backup Configuration
    public string? ClientBackupPath { get; set; }
    public string? AtmacssBackupPath { get; set; }
    public DateTime? LastClientBackup { get; set; }
    public DateTime? LastAtmacssBackup { get; set; }
    
    // Deprecated fields
    public int? InvoicePrinters { get; set; }
    public int? BarcodeScanners { get; set; }
    public int? NormalPrinter { get; set; }
    public int? BarcodePrinter { get; set; }
    public int? WeighingMachine { get; set; }
    public string? SellingType { get; set; }
    public string Status { get; set; } = "Active";
}
