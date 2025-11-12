using System.Linq;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

public class CompanySettingsService : ICompanySettingsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanySettingsRepository _companySettingsRepository;

    public CompanySettingsService(IUnitOfWork unitOfWork, ICompanySettingsRepository companySettingsRepository)
    {
        _unitOfWork = unitOfWork;
        _companySettingsRepository = companySettingsRepository;
    }

    public async Task<IEnumerable<CompanySettingsDto>> GetAllAsync()
    {
        var settings = await _companySettingsRepository.GetAllAsync();
        return settings
            .Where(s => s.DeletedAt == null)
            .Select(MapToDto);
    }

    public async Task<IEnumerable<CompanySettingsDto>> GetActiveAsync()
    {
        var settings = await _companySettingsRepository.GetActiveSettingsAsync();
        return settings.Select(MapToDto);
    }

    public async Task<CompanySettingsDto?> GetByIdAsync(int id)
    {
        var settings = await _companySettingsRepository.GetByIdAsync(id);
        return settings != null && settings.DeletedAt == null ? MapToDto(settings) : null;
    }

    public async Task<CompanySettingsDto?> GetByCompanyIdAsync(int companyId)
    {
        var settings = await _companySettingsRepository.GetByCompanyIdAsync(companyId);
        return settings != null ? MapToDto(settings) : null;
    }

    public async Task<CompanySettingsDto> CreateAsync(CreateCompanySettingsDto createDto, int createdBy)
    {
        var settings = new CompanySettings
        {
            CompanyId = createDto.CompanyId,
            CurrencyId = createDto.CurrencyId,
            StockValueId = createDto.StockValueId,
            PrimaryColor = createDto.PrimaryColor,
            SecondaryColor = createDto.SecondaryColor,
            ClientBackupFrequency = createDto.ClientBackupFrequency,
            AtmacssBackupFrequency = createDto.AtmacssBackupFrequency,
            RefundType = createDto.RefundType,
            PeriodOfValidity = createDto.PeriodOfValidity,
            AllowReturnCash = createDto.AllowReturnCash,
            AllowCreditNote = createDto.AllowCreditNote,
            AllowExchangeTransaction = createDto.AllowExchangeTransaction,
            HasSkuFormat = createDto.HasSkuFormat,
            HasInvoiceFormat = createDto.HasInvoiceFormat,
            CompanySubscriptionType = createDto.CompanySubscriptionType,
            InvoiceDefaultLanguageId = createDto.InvoiceDefaultLanguageId,
            NumberOfUsers = createDto.NumberOfUsers,
            InvoicePrinters = createDto.InvoicePrinters,
            BarcodeScanners = createDto.BarcodeScanners,
            NormalPrinter = createDto.NormalPrinter,
            BarcodePrinter = createDto.BarcodePrinter,
            WeighingMachine = createDto.WeighingMachine,
            // New hardware fields
            InvoicePrinterName = createDto.InvoicePrinterName,
            NormalPrinterName = createDto.NormalPrinterName,
            BarcodePrinterName = createDto.BarcodePrinterName,
            BarcodeScannerPort = createDto.BarcodeScannerPort,
            WeighingMachinePort = createDto.WeighingMachinePort,
            // New backup fields
            ClientBackupPath = createDto.ClientBackupPath,
            AtmacssBackupPath = createDto.AtmacssBackupPath,
            SellingType = createDto.SellingType,
            Status = createDto.Status,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = createdBy,
            UpdatedAt = DateTime.UtcNow
        };

        await _companySettingsRepository.AddAsync(settings);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(settings);
    }

    public async Task<CompanySettingsDto> UpdateAsync(int id, UpdateCompanySettingsDto updateDto, int updatedBy)
    {
        var settings = await _companySettingsRepository.GetByIdAsync(id);
        if (settings == null || settings.DeletedAt != null)
        {
            throw new ArgumentException("Company settings not found.");
        }

        settings.CompanyId = updateDto.CompanyId;
        settings.CurrencyId = updateDto.CurrencyId;
        settings.StockValueId = updateDto.StockValueId;
        settings.PrimaryColor = updateDto.PrimaryColor;
        settings.SecondaryColor = updateDto.SecondaryColor;
        settings.ClientBackupFrequency = updateDto.ClientBackupFrequency;
        settings.AtmacssBackupFrequency = updateDto.AtmacssBackupFrequency;
        settings.RefundType = updateDto.RefundType;
        settings.PeriodOfValidity = updateDto.PeriodOfValidity;
        settings.AllowReturnCash = updateDto.AllowReturnCash;
        settings.AllowCreditNote = updateDto.AllowCreditNote;
        settings.AllowExchangeTransaction = updateDto.AllowExchangeTransaction;
        settings.HasSkuFormat = updateDto.HasSkuFormat;
        settings.HasInvoiceFormat = updateDto.HasInvoiceFormat;
        settings.CompanySubscriptionType = updateDto.CompanySubscriptionType;
        settings.InvoiceDefaultLanguageId = updateDto.InvoiceDefaultLanguageId;
        settings.NumberOfUsers = updateDto.NumberOfUsers;
        settings.InvoicePrinters = updateDto.InvoicePrinters;
        settings.BarcodeScanners = updateDto.BarcodeScanners;
        settings.NormalPrinter = updateDto.NormalPrinter;
        settings.BarcodePrinter = updateDto.BarcodePrinter;
        settings.WeighingMachine = updateDto.WeighingMachine;
        // New hardware fields
        settings.InvoicePrinterName = updateDto.InvoicePrinterName;
        settings.NormalPrinterName = updateDto.NormalPrinterName;
        settings.BarcodePrinterName = updateDto.BarcodePrinterName;
        settings.BarcodeScannerPort = updateDto.BarcodeScannerPort;
        settings.WeighingMachinePort = updateDto.WeighingMachinePort;
        // New backup fields
        settings.ClientBackupPath = updateDto.ClientBackupPath;
        settings.AtmacssBackupPath = updateDto.AtmacssBackupPath;
        settings.SellingType = updateDto.SellingType;
        settings.Status = updateDto.Status;
        settings.UpdatedBy = updatedBy;
        settings.UpdatedAt = DateTime.UtcNow;

        await _companySettingsRepository.UpdateAsync(settings);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(settings);
    }

    public async Task<bool> DeleteAsync(int id, int deletedBy)
    {
        var settings = await _companySettingsRepository.GetByIdAsync(id);
        if (settings == null || settings.DeletedAt != null)
        {
            return false;
        }

        // Soft delete
        settings.DeletedBy = deletedBy;
        settings.DeletedAt = DateTime.UtcNow;
        settings.Status = "Inactive";

        await _companySettingsRepository.UpdateAsync(settings);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static CompanySettingsDto MapToDto(CompanySettings settings)
    {
        return new CompanySettingsDto
        {
            Id = settings.Id,
            CompanyId = settings.CompanyId,
            CompanyName = settings.Company?.CompanyName,
            CurrencyId = settings.CurrencyId,
            CurrencyName = settings.Currency?.CurrencyName,
            CurrencySymbol = settings.Currency?.Symbol,
            StockValueId = settings.StockValueId,
            PrimaryColor = settings.PrimaryColor,
            SecondaryColor = settings.SecondaryColor,
            ClientBackupFrequency = settings.ClientBackupFrequency,
            AtmacssBackupFrequency = settings.AtmacssBackupFrequency,
            RefundType = settings.RefundType,
            PeriodOfValidity = settings.PeriodOfValidity,
            AllowReturnCash = settings.AllowReturnCash,
            AllowCreditNote = settings.AllowCreditNote,
            AllowExchangeTransaction = settings.AllowExchangeTransaction,
            HasSkuFormat = settings.HasSkuFormat,
            HasInvoiceFormat = settings.HasInvoiceFormat,
            CompanySubscriptionType = settings.CompanySubscriptionType,
            InvoiceDefaultLanguageId = settings.InvoiceDefaultLanguageId,
            InvoiceDefaultLanguageName = settings.InvoiceDefaultLanguage?.LanguageName,
            NumberOfUsers = settings.NumberOfUsers,
            InvoicePrinters = settings.InvoicePrinters,
            BarcodeScanners = settings.BarcodeScanners,
            NormalPrinter = settings.NormalPrinter,
            BarcodePrinter = settings.BarcodePrinter,
            WeighingMachine = settings.WeighingMachine,
            // New hardware fields
            InvoicePrinterName = settings.InvoicePrinterName,
            NormalPrinterName = settings.NormalPrinterName,
            BarcodePrinterName = settings.BarcodePrinterName,
            BarcodeScannerPort = settings.BarcodeScannerPort,
            WeighingMachinePort = settings.WeighingMachinePort,
            // New backup fields
            ClientBackupPath = settings.ClientBackupPath,
            AtmacssBackupPath = settings.AtmacssBackupPath,
            LastClientBackup = settings.LastClientBackup,
            LastAtmacssBackup = settings.LastAtmacssBackup,
            SellingType = settings.SellingType,
            Status = settings.Status,
            CreatedBy = settings.CreatedBy,
            CreatedAt = settings.CreatedAt,
            UpdatedBy = settings.UpdatedBy,
            UpdatedAt = settings.UpdatedAt
        };
    }
}
