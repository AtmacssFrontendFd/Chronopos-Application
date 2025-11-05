using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;
using SysApplication = System.Windows.Application;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for Company Settings screen with Edit/Save/Cancel functionality
/// </summary>
public partial class CompanySettingsViewModel : ObservableObject
{
    private readonly ICompanySettingsService _companySettingsService;
    private readonly ICurrencyService _currencyService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPrinterService _printerService;
    private readonly IBackupService _backupService;
    private readonly ICompanyService _companyService;
    private int? _currentSettingsId;
    private CompanySettingsDto? _originalSettings;

    [ObservableProperty]
    private CompanySettingsDto? _currentSettings;

    [ObservableProperty]
    private int _currencyId;

    [ObservableProperty]
    private string? _primaryColor;

    [ObservableProperty]
    private string? _secondaryColor;

    [ObservableProperty]
    private string? _clientBackupFrequency;

    [ObservableProperty]
    private string? _atmacssBackupFrequency;

    [ObservableProperty]
    private string? _refundType;

    [ObservableProperty]
    private int? _periodOfValidity;

    [ObservableProperty]
    private bool _allowReturnCash;

    [ObservableProperty]
    private bool _allowCreditNote;

    [ObservableProperty]
    private bool _allowExchangeTransaction;

    [ObservableProperty]
    private bool _hasSkuFormat;

    [ObservableProperty]
    private bool _hasInvoiceFormat;

    [ObservableProperty]
    private string? _companySubscriptionType;

    [ObservableProperty]
    private int _numberOfUsers = 1;

    [ObservableProperty]
    private int? _invoicePrinters;

    [ObservableProperty]
    private int? _barcodeScanners;

    [ObservableProperty]
    private int? _normalPrinter;

    [ObservableProperty]
    private int? _barcodePrinter;

    [ObservableProperty]
    private int? _weighingMachine;

    // Hardware configuration fields
    [ObservableProperty]
    private string? _invoicePrinterName;

    [ObservableProperty]
    private string? _normalPrinterName;

    [ObservableProperty]
    private string? _barcodePrinterName;

    [ObservableProperty]
    private string? _barcodeScannerPort;

    [ObservableProperty]
    private string? _weighingMachinePort;

    // Backup configuration fields
    [ObservableProperty]
    private string? _clientBackupPath;

    [ObservableProperty]
    private string? _atmacssBackupPath;

    [ObservableProperty]
    private DateTime? _lastClientBackup;

    [ObservableProperty]
    private DateTime? _lastAtmacssBackup;

    [ObservableProperty]
    private string? _sellingType;

    [ObservableProperty]
    private string _status = "Active";

    [ObservableProperty]
    private bool _isEditMode = false;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private ObservableCollection<CurrencyDto> _currencies = new();

    [ObservableProperty]
    private CurrencyDto? _selectedCurrency;

    // Available printers collection
    [ObservableProperty]
    private ObservableCollection<string> _availablePrinters = new();

    // Backup frequency options
    [ObservableProperty]
    private ObservableCollection<string> _backupFrequencies = new()
    {
        "Hourly",
        "Daily",
        "Weekly",
        "Monthly",
        "Manual"
    };

    public CompanySettingsViewModel(
        ICompanySettingsService companySettingsService,
        ICurrencyService currencyService,
        ICurrentUserService currentUserService,
        IPrinterService printerService,
        IBackupService backupService,
        ICompanyService companyService)
    {
        _companySettingsService = companySettingsService ?? throw new ArgumentNullException(nameof(companySettingsService));
        _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
        _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
        _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadCurrenciesAsync();
        await LoadPrintersAsync();
        await LoadCompanySettingsAsync();
        await CheckPendingRestoreAsync();
    }

    /// <summary>
    /// Load available printers
    /// </summary>
    private async Task LoadPrintersAsync()
    {
        try
        {
            var printers = await _printerService.GetAvailablePrintersAsync();
            AvailablePrinters.Clear();
            AvailablePrinters.Add(""); // Empty option for no printer selected
            foreach (var printer in printers)
            {
                AvailablePrinters.Add(printer);
            }
        }
        catch (Exception ex)
        {
            SysApplication.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Error loading printers: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    /// <summary>
    /// Load available currencies
    /// </summary>
    private async Task LoadCurrenciesAsync()
    {
        try
        {
            var currencies = await _currencyService.GetAllAsync();
            Currencies.Clear();
            foreach (var currency in currencies)
            {
                Currencies.Add(currency);
            }
            
            // Set default currency if none is selected
            if (SelectedCurrency == null && Currencies.Any())
            {
                SelectedCurrency = Currencies.First();
            }
        }
        catch (Exception ex)
        {
            SysApplication.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Error loading currencies: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    /// <summary>
    /// Load company settings
    /// </summary>
    private async Task LoadCompanySettingsAsync()
    {
        try
        {
            IsLoading = true;

            // Get the first company settings (assuming single company setup)
            var settingsList = await _companySettingsService.GetActiveAsync();
            var settings = settingsList.FirstOrDefault();

            if (settings != null)
            {
                _currentSettingsId = settings.Id;
                CurrentSettings = settings;
                _originalSettings = CloneSettings(settings);
                MapSettingsToProperties(settings);
            }
        }
        catch (Exception ex)
        {
            SysApplication.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Error loading company settings: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Map settings DTO to properties
    /// </summary>
    private void MapSettingsToProperties(CompanySettingsDto settings)
    {
        CurrencyId = settings.CurrencyId;
        SelectedCurrency = Currencies.FirstOrDefault(c => c.Id == settings.CurrencyId);
        PrimaryColor = settings.PrimaryColor;
        SecondaryColor = settings.SecondaryColor;
        ClientBackupFrequency = settings.ClientBackupFrequency;
        AtmacssBackupFrequency = settings.AtmacssBackupFrequency;
        RefundType = settings.RefundType;
        PeriodOfValidity = settings.PeriodOfValidity;
        AllowReturnCash = settings.AllowReturnCash;
        AllowCreditNote = settings.AllowCreditNote;
        AllowExchangeTransaction = settings.AllowExchangeTransaction;
        HasSkuFormat = settings.HasSkuFormat;
        HasInvoiceFormat = settings.HasInvoiceFormat;
        CompanySubscriptionType = settings.CompanySubscriptionType;
        NumberOfUsers = settings.NumberOfUsers;
        InvoicePrinters = settings.InvoicePrinters;
        BarcodeScanners = settings.BarcodeScanners;
        NormalPrinter = settings.NormalPrinter;
        BarcodePrinter = settings.BarcodePrinter;
        WeighingMachine = settings.WeighingMachine;
        // Hardware fields
        InvoicePrinterName = settings.InvoicePrinterName;
        NormalPrinterName = settings.NormalPrinterName;
        BarcodePrinterName = settings.BarcodePrinterName;
        BarcodeScannerPort = settings.BarcodeScannerPort;
        WeighingMachinePort = settings.WeighingMachinePort;
        // Backup fields
        ClientBackupPath = settings.ClientBackupPath;
        AtmacssBackupPath = settings.AtmacssBackupPath;
        LastClientBackup = settings.LastClientBackup;
        LastAtmacssBackup = settings.LastAtmacssBackup;
        SellingType = settings.SellingType;
        Status = settings.Status;
    }

    /// <summary>
    /// Clone settings for cancel operation
    /// </summary>
    private CompanySettingsDto CloneSettings(CompanySettingsDto settings)
    {
        return new CompanySettingsDto
        {
            Id = settings.Id,
            CompanyId = settings.CompanyId,
            CurrencyId = settings.CurrencyId,
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
            NumberOfUsers = settings.NumberOfUsers,
            InvoicePrinters = settings.InvoicePrinters,
            BarcodeScanners = settings.BarcodeScanners,
            NormalPrinter = settings.NormalPrinter,
            BarcodePrinter = settings.BarcodePrinter,
            WeighingMachine = settings.WeighingMachine,
            // Hardware fields
            InvoicePrinterName = settings.InvoicePrinterName,
            NormalPrinterName = settings.NormalPrinterName,
            BarcodePrinterName = settings.BarcodePrinterName,
            BarcodeScannerPort = settings.BarcodeScannerPort,
            WeighingMachinePort = settings.WeighingMachinePort,
            // Backup fields
            ClientBackupPath = settings.ClientBackupPath,
            AtmacssBackupPath = settings.AtmacssBackupPath,
            LastClientBackup = settings.LastClientBackup,
            LastAtmacssBackup = settings.LastAtmacssBackup,
            SellingType = settings.SellingType,
            Status = settings.Status
        };
    }

    /// <summary>
    /// Enable edit mode
    /// </summary>
    [RelayCommand]
    private void EnableEdit()
    {
        IsEditMode = true;
    }

    /// <summary>
    /// Save changes (Create or Update)
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            IsLoading = true;
            var userId = _currentUserService.CurrentUserId ?? 0;
            
            // Validate required fields
            var currencyId = SelectedCurrency?.Id ?? CurrencyId;
            if (currencyId == 0)
            {
                MessageBox.Show("Please select a currency.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CompanySettingsDto savedSettings;

            // If no settings exist, create new one
            if (_currentSettingsId == null)
            {
                // Get the first active company to associate with settings
                var companies = await _companyService.GetAllAsync();
                var firstCompany = companies.FirstOrDefault();
                
                if (firstCompany == null)
                {
                    MessageBox.Show("No company found. Please complete company setup first.", "Warning", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ChronoPos.Application.Logging.AppLogger.Log($"Creating company settings for CompanyId: {firstCompany.Id}");
                ChronoPos.Application.Logging.AppLogger.Log($"CurrencyId: {SelectedCurrency?.Id ?? CurrencyId}");

                var createDto = new CreateCompanySettingsDto
                {
                    CompanyId = firstCompany.Id,
                    CurrencyId = SelectedCurrency?.Id ?? CurrencyId,
                    StockValueId = null, // Make nullable to avoid FK constraint
                    PrimaryColor = PrimaryColor,
                    SecondaryColor = SecondaryColor,
                    ClientBackupFrequency = ClientBackupFrequency,
                    AtmacssBackupFrequency = AtmacssBackupFrequency,
                    RefundType = RefundType,
                    PeriodOfValidity = PeriodOfValidity,
                    AllowReturnCash = AllowReturnCash,
                    AllowCreditNote = AllowCreditNote,
                    AllowExchangeTransaction = AllowExchangeTransaction,
                    HasSkuFormat = HasSkuFormat,
                    HasInvoiceFormat = HasInvoiceFormat,
                    CompanySubscriptionType = CompanySubscriptionType,
                    InvoiceDefaultLanguageId = null, // Make nullable to avoid FK constraint
                    NumberOfUsers = NumberOfUsers,
                    InvoicePrinters = InvoicePrinters,
                    BarcodeScanners = BarcodeScanners,
                    NormalPrinter = NormalPrinter,
                    BarcodePrinter = BarcodePrinter,
                    WeighingMachine = WeighingMachine,
                    // Hardware fields
                    InvoicePrinterName = InvoicePrinterName,
                    NormalPrinterName = NormalPrinterName,
                    BarcodePrinterName = BarcodePrinterName,
                    BarcodeScannerPort = BarcodeScannerPort,
                    WeighingMachinePort = WeighingMachinePort,
                    // Backup fields
                    ClientBackupPath = ClientBackupPath,
                    AtmacssBackupPath = AtmacssBackupPath,
                    SellingType = SellingType,
                    Status = Status ?? "Active"
                };

                ChronoPos.Application.Logging.AppLogger.Log("Calling CreateAsync...");
                savedSettings = await _companySettingsService.CreateAsync(createDto, userId);
                ChronoPos.Application.Logging.AppLogger.Log($"Settings created successfully with ID: {savedSettings.Id}");
                _currentSettingsId = savedSettings.Id;
            }
            else
            {
                // Update existing settings
                var updateDto = new UpdateCompanySettingsDto
                {
                    CompanyId = CurrentSettings?.CompanyId,
                    CurrencyId = SelectedCurrency?.Id ?? CurrencyId,
                    StockValueId = CurrentSettings?.StockValueId,
                    PrimaryColor = PrimaryColor,
                    SecondaryColor = SecondaryColor,
                    ClientBackupFrequency = ClientBackupFrequency,
                    AtmacssBackupFrequency = AtmacssBackupFrequency,
                    RefundType = RefundType,
                    PeriodOfValidity = PeriodOfValidity,
                    AllowReturnCash = AllowReturnCash,
                    AllowCreditNote = AllowCreditNote,
                    AllowExchangeTransaction = AllowExchangeTransaction,
                    HasSkuFormat = HasSkuFormat,
                    HasInvoiceFormat = HasInvoiceFormat,
                    CompanySubscriptionType = CompanySubscriptionType,
                    InvoiceDefaultLanguageId = CurrentSettings?.InvoiceDefaultLanguageId,
                    NumberOfUsers = NumberOfUsers,
                    InvoicePrinters = InvoicePrinters,
                    BarcodeScanners = BarcodeScanners,
                    NormalPrinter = NormalPrinter,
                    BarcodePrinter = BarcodePrinter,
                    WeighingMachine = WeighingMachine,
                    // Hardware fields
                    InvoicePrinterName = InvoicePrinterName,
                    NormalPrinterName = NormalPrinterName,
                    BarcodePrinterName = BarcodePrinterName,
                    BarcodeScannerPort = BarcodeScannerPort,
                    WeighingMachinePort = WeighingMachinePort,
                    // Backup fields
                    ClientBackupPath = ClientBackupPath,
                    AtmacssBackupPath = AtmacssBackupPath,
                    SellingType = SellingType,
                    Status = Status
                };

                savedSettings = await _companySettingsService.UpdateAsync(
                    _currentSettingsId.Value, updateDto, userId);
            }

            // Start backup schedulers if paths and frequencies are set
            if (!string.IsNullOrWhiteSpace(ClientBackupPath) && 
                !string.IsNullOrWhiteSpace(ClientBackupFrequency) &&
                ClientBackupFrequency != "Manual")
            {
                _backupService.StartBackupScheduler(ClientBackupPath, ClientBackupFrequency);
            }

            if (!string.IsNullOrWhiteSpace(AtmacssBackupPath) && 
                !string.IsNullOrWhiteSpace(AtmacssBackupFrequency) &&
                AtmacssBackupFrequency != "Manual")
            {
                _backupService.StartBackupScheduler(AtmacssBackupPath, AtmacssBackupFrequency);
            }

            CurrentSettings = savedSettings;
            _originalSettings = CloneSettings(savedSettings);
            IsEditMode = false;

            MessageBox.Show("Company settings saved successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log($"ERROR saving company settings: {ex.Message}");
            ChronoPos.Application.Logging.AppLogger.Log($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                ChronoPos.Application.Logging.AppLogger.Log($"Inner exception: {ex.InnerException.Message}");
                ChronoPos.Application.Logging.AppLogger.Log($"Inner stack trace: {ex.InnerException.StackTrace}");
            }
            
            var errorMsg = ex.InnerException != null 
                ? $"{ex.Message}\n\nDetails: {ex.InnerException.Message}" 
                : ex.Message;
            
            MessageBox.Show($"Error saving company settings: {errorMsg}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Cancel editing and restore original values
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        if (_originalSettings != null)
        {
            MapSettingsToProperties(_originalSettings);
        }
        IsEditMode = false;
    }

    /// <summary>
    /// Refresh settings
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadCompanySettingsAsync();
        IsEditMode = false;
    }

    partial void OnSelectedCurrencyChanged(CurrencyDto? value)
    {
        if (value != null)
        {
            CurrencyId = value.Id;
        }
    }

    /// <summary>
    /// Test invoice printer
    /// </summary>
    [RelayCommand]
    private async Task TestInvoicePrinterAsync()
    {
        if (string.IsNullOrWhiteSpace(InvoicePrinterName))
        {
            MessageBox.Show("Please select an invoice printer first.", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var isValid = await _printerService.TestPrinterAsync(InvoicePrinterName);
            if (isValid)
            {
                MessageBox.Show($"Invoice printer '{InvoicePrinterName}' is working correctly!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Invoice printer '{InvoicePrinterName}' is not available or not working.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error testing printer: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Test normal printer
    /// </summary>
    [RelayCommand]
    private async Task TestNormalPrinterAsync()
    {
        if (string.IsNullOrWhiteSpace(NormalPrinterName))
        {
            MessageBox.Show("Please select a normal printer first.", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var isValid = await _printerService.TestPrinterAsync(NormalPrinterName);
            if (isValid)
            {
                MessageBox.Show($"Normal printer '{NormalPrinterName}' is working correctly!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Normal printer '{NormalPrinterName}' is not available or not working.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error testing printer: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Test barcode printer
    /// </summary>
    [RelayCommand]
    private async Task TestBarcodePrinterAsync()
    {
        if (string.IsNullOrWhiteSpace(BarcodePrinterName))
        {
            MessageBox.Show("Please select a barcode printer first.", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var isValid = await _printerService.TestPrinterAsync(BarcodePrinterName);
            if (isValid)
            {
                MessageBox.Show($"Barcode printer '{BarcodePrinterName}' is working correctly!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Barcode printer '{BarcodePrinterName}' is not available or not working.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error testing printer: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Browse for client backup path
    /// </summary>
    [RelayCommand]
    private void BrowseClientBackupPath()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Select Client Backup Location",
            FileName = "chronopos_backup.db",
            Filter = "Database Files (*.db)|*.db|All Files (*.*)|*.*",
            DefaultExt = ".db",
            InitialDirectory = string.IsNullOrWhiteSpace(ClientBackupPath) 
                ? _backupService.GetDefaultBackupPath() 
                : Path.GetDirectoryName(ClientBackupPath)
        };

        if (dialog.ShowDialog() == true)
        {
            ClientBackupPath = dialog.FileName;
        }
    }

    /// <summary>
    /// Browse for ATMACSS backup path
    /// </summary>
    [RelayCommand]
    private void BrowseAtmacssBackupPath()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Select ATMACSS Backup Location",
            FileName = "chronopos_atmacss_backup.db",
            Filter = "Database Files (*.db)|*.db|All Files (*.*)|*.*",
            DefaultExt = ".db",
            InitialDirectory = string.IsNullOrWhiteSpace(AtmacssBackupPath) 
                ? _backupService.GetDefaultBackupPath() 
                : Path.GetDirectoryName(AtmacssBackupPath)
        };

        if (dialog.ShowDialog() == true)
        {
            AtmacssBackupPath = dialog.FileName;
        }
    }

    /// <summary>
    /// Perform manual client backup
    /// </summary>
    [RelayCommand]
    private async Task BackupClientNowAsync()
    {
        if (string.IsNullOrWhiteSpace(ClientBackupPath))
        {
            MessageBox.Show("Please select a backup path first.", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;
            var success = await _backupService.BackupDatabaseAsync(ClientBackupPath);
            
            if (success)
            {
                LastClientBackup = DateTime.Now;
                
                // Save the backup time to database
                await UpdateBackupTimeAsync();
                
                MessageBox.Show($"Backup completed successfully!\nLocation: {ClientBackupPath}", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Backup failed. Please check the path and try again.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during backup: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Perform manual ATMACSS backup
    /// </summary>
    [RelayCommand]
    private async Task BackupAtmacssNowAsync()
    {
        if (string.IsNullOrWhiteSpace(AtmacssBackupPath))
        {
            MessageBox.Show("Please select a backup path first.", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;
            var success = await _backupService.BackupDatabaseAsync(AtmacssBackupPath);
            
            if (success)
            {
                LastAtmacssBackup = DateTime.Now;
                
                // Save the backup time to database
                await UpdateBackupTimeAsync();
                
                MessageBox.Show($"Backup completed successfully!\nLocation: {AtmacssBackupPath}", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Backup failed. Please check the path and try again.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during backup: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Reload printers list
    /// </summary>
    [RelayCommand]
    private async Task ReloadPrintersAsync()
    {
        await LoadPrintersAsync();
        MessageBox.Show("Printers list refreshed successfully!", "Success",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    #region Restore Operations

    // Restore-related properties
    [ObservableProperty]
    private string? _restoreBackupPath;

    [ObservableProperty]
    private bool _hasPendingRestore;

    /// <summary>
    /// Browse for backup file to restore
    /// </summary>
    [RelayCommand]
    private void BrowseRestoreBackupPath()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select Backup File to Restore",
            Filter = "Database files (*.db)|*.db|All files (*.*)|*.*",
            DefaultExt = ".db",
            CheckFileExists = true,
            Multiselect = false
        };

        if (openFileDialog.ShowDialog() == true)
        {
            RestoreBackupPath = openFileDialog.FileName;
        }
    }

    /// <summary>
    /// Stage restore from selected backup
    /// </summary>
    [RelayCommand]
    private async Task StageRestoreAsync()
    {
        if (string.IsNullOrWhiteSpace(RestoreBackupPath))
        {
            MessageBox.Show("Please select a backup file to restore.", "Warning",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!File.Exists(RestoreBackupPath))
        {
            MessageBox.Show("Selected backup file does not exist.", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Show confirmation dialog with warning
        var result = MessageBox.Show(
            "⚠️ WARNING: This will replace your current database with the selected backup.\n\n" +
            $"Selected backup: {Path.GetFileName(RestoreBackupPath)}\n\n" +
            "A backup of your current database will be created before restore.\n" +
            "The restore will be applied when you restart the application.\n\n" +
            "Do you want to continue?",
            "Confirm Database Restore",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            IsLoading = true;

            ChronoPos.Application.Logging.AppLogger.Log($"Staging restore from: {RestoreBackupPath}");
            
            var restoreResult = await _backupService.StageRestoreAsync(RestoreBackupPath);

            if (restoreResult.Success)
            {
                HasPendingRestore = true;
                
                MessageBox.Show(
                    $"✅ Restore staged successfully!\n\n" +
                    $"{restoreResult.Message}\n\n" +
                    $"Please restart the application to apply the restore.\n\n" +
                    $"Note: A backup of your current database will be created automatically before the restore is applied.",
                    "Restore Staged",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                ChronoPos.Application.Logging.AppLogger.Log("Restore staged successfully");
            }
            else
            {
                MessageBox.Show(
                    $"Failed to stage restore:\n\n{restoreResult.Message}\n\n" +
                    $"Details: {restoreResult.ErrorDetails}",
                    "Restore Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                ChronoPos.Application.Logging.AppLogger.Log($"Restore staging failed: {restoreResult.Message}");
            }
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log($"Error staging restore: {ex.Message}");
            MessageBox.Show($"Error staging restore: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Cancel pending restore
    /// </summary>
    [RelayCommand]
    private async Task CancelPendingRestoreAsync()
    {
        var result = MessageBox.Show(
            "Are you sure you want to cancel the pending database restore?",
            "Cancel Restore",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            IsLoading = true;

            var success = await _backupService.CancelPendingRestoreAsync();

            if (success)
            {
                HasPendingRestore = false;
                RestoreBackupPath = null;
                
                MessageBox.Show("Pending restore cancelled successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                ChronoPos.Application.Logging.AppLogger.Log("Pending restore cancelled");
            }
            else
            {
                MessageBox.Show("Failed to cancel pending restore.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log($"Error cancelling restore: {ex.Message}");
            MessageBox.Show($"Error cancelling restore: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Update backup timestamps in the database
    /// </summary>
    private async Task UpdateBackupTimeAsync()
    {
        try
        {
            if (_currentSettingsId == null)
            {
                ChronoPos.Application.Logging.AppLogger.Log("Cannot update backup time: No current settings ID");
                return;
            }

            var currentUserId = _currentUserService.CurrentUserId;
            if (currentUserId == null)
            {
                ChronoPos.Application.Logging.AppLogger.Log("Cannot update backup time: No current user");
                return;
            }

            var settings = await _companySettingsService.GetByIdAsync(_currentSettingsId.Value);
            if (settings != null)
            {
                var updateDto = new UpdateCompanySettingsDto
                {
                    CompanyId = settings.CompanyId,
                    CurrencyId = settings.CurrencyId,
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
                    NumberOfUsers = settings.NumberOfUsers,
                    InvoicePrinterName = settings.InvoicePrinterName,
                    NormalPrinterName = settings.NormalPrinterName,
                    BarcodePrinterName = settings.BarcodePrinterName,
                    BarcodeScannerPort = settings.BarcodeScannerPort,
                    WeighingMachinePort = settings.WeighingMachinePort,
                    ClientBackupPath = settings.ClientBackupPath,
                    AtmacssBackupPath = settings.AtmacssBackupPath,
                    LastClientBackup = LastClientBackup,
                    LastAtmacssBackup = LastAtmacssBackup,
                    InvoicePrinters = settings.InvoicePrinters,
                    BarcodeScanners = settings.BarcodeScanners,
                    NormalPrinter = settings.NormalPrinter,
                    BarcodePrinter = settings.BarcodePrinter,
                    WeighingMachine = settings.WeighingMachine,
                    SellingType = settings.SellingType,
                    Status = settings.Status
                };

                await _companySettingsService.UpdateAsync(_currentSettingsId.Value, updateDto, currentUserId.Value);
                ChronoPos.Application.Logging.AppLogger.Log("Backup timestamps updated successfully");
            }
            else
            {
                ChronoPos.Application.Logging.AppLogger.Log($"Cannot update backup time: Settings with ID {_currentSettingsId.Value} not found");
            }
        }
        catch (Exception ex)
        {
            // Log but don't show error to user - backup succeeded, just timestamp save failed
            ChronoPos.Application.Logging.AppLogger.Log($"Error saving backup timestamp: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if there's a pending restore on load
    /// </summary>
    private async Task CheckPendingRestoreAsync()
    {
        try
        {
            HasPendingRestore = await _backupService.HasPendingRestoreAsync();
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log($"Error checking pending restore: {ex.Message}");
            HasPendingRestore = false;
        }
    }

    /// <summary>
    /// Restart the application
    /// </summary>
    [RelayCommand]
    private void RestartApplication()
    {
        var result = MessageBox.Show(
            "Are you sure you want to restart the application?",
            "Restart Application",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            ChronoPos.Application.Logging.AppLogger.Log("User initiated application restart from Company Settings");

            // Get the executable path
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;

            if (!string.IsNullOrEmpty(exePath))
            {
                // Start a new instance
                System.Diagnostics.Process.Start(exePath);

                // Shut down current instance
                SysApplication.Current.Shutdown();
            }
            else
            {
                MessageBox.Show("Unable to determine application path for restart.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log($"Error restarting application: {ex.Message}");
            MessageBox.Show($"Error restarting application: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion
}
