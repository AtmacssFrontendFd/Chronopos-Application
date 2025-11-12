using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Constants;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Microsoft.Win32;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views.Dialogs;
using InfrastructureServices = ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

public partial class CurrencyViewModel : ObservableObject, IDisposable
{
    private readonly ICurrencyService _currencyService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActiveCurrencyService _activeCurrencyService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly InfrastructureServices.IDatabaseLocalizationService _databaseLocalizationService;
    private readonly Action? _navigateBack;

    [ObservableProperty]
    private ObservableCollection<CurrencyDto> currencies = new();

    [ObservableProperty]
    private CurrencyDto? selectedCurrency;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string searchPlaceholder = "Search currencies by name or code...";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isSidePanelVisible = false;

    [ObservableProperty]
    private CurrencySidePanelViewModel? sidePanelViewModel;

    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private string pageTitle = "Currency Management";

    [ObservableProperty]
    private string backButtonText = "Back";

    [ObservableProperty]
    private string refreshButtonText = "Refresh";

    [ObservableProperty]
    private string addCurrencyButtonText = "Add Currency";

    [ObservableProperty]
    private string importButtonText = "Import";

    [ObservableProperty]
    private string exportButtonText = "Export";

    [ObservableProperty]
    private string loadingText = "Loading currencies...";

    [ObservableProperty]
    private string noDataText = "No currencies found";

    [ObservableProperty]
    private string noDataHintText = "Click 'Add Currency' to create your first currency";

    [ObservableProperty]
    private string itemsCountText = "currencies";

    [ObservableProperty]
    private string columnCurrencyName = "Currency Name";

    [ObservableProperty]
    private string columnCurrencyCode = "Code";

    [ObservableProperty]
    private string columnCurrencySymbol = "Symbol";

    [ObservableProperty]
    private string columnExchangeRate = "Exchange Rate";

    [ObservableProperty]
    private string columnIsDefault = "Default";

    [ObservableProperty]
    private string columnActions = "Actions";

    // Permission Properties
    [ObservableProperty]
    private bool canCreateCurrency = false;

    [ObservableProperty]
    private bool canEditCurrency = false;

    [ObservableProperty]
    private bool canDeleteCurrency = false;

    [ObservableProperty]
    private bool canImportCurrency = false;

    [ObservableProperty]
    private bool canExportCurrency = false;

    private readonly ICollectionView _filteredCurrenciesView;

    public ICollectionView FilteredCurrencies => _filteredCurrenciesView;

    public bool HasCurrencies => Currencies.Count > 0;
    public int TotalCurrencies => Currencies.Count;

    // Status filter options
    [ObservableProperty]
    private ObservableCollection<StatusFilterOption> statusFilters = new();

    [ObservableProperty]
    private StatusFilterOption? selectedStatusFilter;

    public CurrencyViewModel(
        ICurrencyService currencyService, 
        ICurrentUserService currentUserService, 
        IActiveCurrencyService activeCurrencyService,
        ILayoutDirectionService layoutDirectionService,
        InfrastructureServices.IDatabaseLocalizationService databaseLocalizationService,
        Action? navigateBack = null)
    {
        _currencyService = currencyService;
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _activeCurrencyService = activeCurrencyService ?? throw new ArgumentNullException(nameof(activeCurrencyService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));
        _navigateBack = navigateBack;
        
        // Initialize permissions
        InitializePermissions();
        
        // Initialize current settings
        InitializeCurrentSettings();
        
        // Initialize status filters
        InitializeStatusFilters();
        
        // Initialize filtered view
        _filteredCurrenciesView = CollectionViewSource.GetDefaultView(Currencies);
        _filteredCurrenciesView.Filter = FilterCurrencies;

        // Subscribe to property changes
        PropertyChanged += OnPropertyChanged;
        
        // Subscribe to layout direction changes
        _layoutDirectionService.DirectionChanged += OnDirectionChanged;
        
        // Subscribe to language changes
        _databaseLocalizationService.LanguageChanged += OnLanguageChanged;
        
        // Load currencies on startup
        _ = LoadCurrenciesAsync();
        
        // Load translations
        _ = Task.Run(LoadTranslationsAsync);
    }

    public IAsyncRelayCommand LoadCurrenciesCommand => new AsyncRelayCommand(LoadCurrenciesAsync);
    public IRelayCommand AddCurrencyCommand => new RelayCommand(ShowAddCurrencyPanel);
    public IRelayCommand<CurrencyDto?> EditCurrencyCommand => new RelayCommand<CurrencyDto?>(ShowEditCurrencyPanel);
    public IAsyncRelayCommand<CurrencyDto?> DeleteCurrencyCommand => new AsyncRelayCommand<CurrencyDto?>(DeleteCurrencyAsync);
    public IAsyncRelayCommand RefreshCommand => new AsyncRelayCommand(LoadCurrenciesAsync);
    public IAsyncRelayCommand ImportCommand => new AsyncRelayCommand(ImportCurrenciesAsync);
    public IAsyncRelayCommand ExportCommand => new AsyncRelayCommand(ExportCurrenciesAsync);
    public IAsyncRelayCommand<CurrencyDto?> ActivateCurrencyCommand => new AsyncRelayCommand<CurrencyDto?>(ActivateCurrencyAsync);
    public IRelayCommand BackCommand => new RelayCommand(GoBack);
    public IRelayCommand CloseSidePanelCommand => new RelayCommand(CloseSidePanel);

    private void InitializeStatusFilters()
    {
        StatusFilters.Add(new StatusFilterOption { Display = "All Currencies", Value = "All" });
        StatusFilters.Add(new StatusFilterOption { Display = "Default Only", Value = "Default" });
        StatusFilters.Add(new StatusFilterOption { Display = "Non-Default", Value = "NonDefault" });
        SelectedStatusFilter = StatusFilters.First();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchText) || e.PropertyName == nameof(SelectedStatusFilter))
        {
            _filteredCurrenciesView.Refresh();
        }
        else if (e.PropertyName == nameof(Currencies))
        {
            OnPropertyChanged(nameof(HasCurrencies));
            OnPropertyChanged(nameof(TotalCurrencies));
            _filteredCurrenciesView.Refresh();
        }
    }

    private bool FilterCurrencies(object obj)
    {
        if (obj is not CurrencyDto currency) return false;
        
        // Apply status filter
        if (SelectedStatusFilter != null && SelectedStatusFilter.Value != "All")
        {
            if (SelectedStatusFilter.Value == "Default" && !currency.IsDefault)
                return false;
            if (SelectedStatusFilter.Value == "NonDefault" && currency.IsDefault)
                return false;
        }
        
        // Apply search filter
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        
        return currency.CurrencyName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
               currency.CurrencyCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
               currency.Symbol.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }

    private async Task LoadCurrenciesAsync()
    {
        IsLoading = true;
        try
        {
            var allCurrencies = await _currencyService.GetAllAsync();
            
            // Clear and repopulate the existing collection to maintain the filtered view
            Currencies.Clear();
            foreach (var currency in allCurrencies)
            {
                Currencies.Add(currency);
            }
            
            StatusMessage = $"Loaded {Currencies.Count} currencies.";
            
            // Refresh the filtered view
            _filteredCurrenciesView.Refresh();
            OnPropertyChanged(nameof(HasCurrencies));
            OnPropertyChanged(nameof(TotalCurrencies));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading currencies: {ex.Message}";
            var errorDialog = new MessageDialog(
                "Error",
                $"Error loading currencies: {ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowAddCurrencyPanel()
    {
        if (!CanCreateCurrency)
        {
            var warningDialog = new MessageDialog(
                "Permission Denied",
                "You do not have permission to create currencies.",
                MessageDialog.MessageType.Warning);
            warningDialog.ShowDialog();
            return;
        }

        SidePanelViewModel = new CurrencySidePanelViewModel(
            _currencyService,
            OnCurrencySaved,
            CloseSidePanel);
        IsSidePanelVisible = true;
        StatusMessage = "Add new currency...";
    }

    private void ShowEditCurrencyPanel(CurrencyDto? currency)
    {
        if (currency == null) return;

        if (!CanEditCurrency)
        {
            var warningDialog = new MessageDialog(
                "Permission Denied",
                "You do not have permission to edit currencies.",
                MessageDialog.MessageType.Warning);
            warningDialog.ShowDialog();
            return;
        }
        
        SidePanelViewModel = new CurrencySidePanelViewModel(
            _currencyService,
            currency,
            OnCurrencySaved,
            CloseSidePanel);
        IsSidePanelVisible = true;
        StatusMessage = $"Edit currency '{currency.CurrencyName}'...";
    }

    private void OnCurrencySaved(bool success)
    {
        if (success)
        {
            CloseSidePanel();
            // Reload currencies to reflect changes
            _ = LoadCurrenciesAsync();
        }
    }

    private void CloseSidePanel()
    {
        IsSidePanelVisible = false;
        SidePanelViewModel = null;
        StatusMessage = "Currency editing cancelled.";
    }

    private async Task DeleteCurrencyAsync(CurrencyDto? currency)
    {
        if (currency == null) return;

        if (!CanDeleteCurrency)
        {
            var warningDialog = new MessageDialog(
                "Permission Denied",
                "You do not have permission to delete currencies.",
                MessageDialog.MessageType.Warning);
            warningDialog.ShowDialog();
            return;
        }

        if (currency.IsDefault)
        {
            var warningDialog = new MessageDialog(
                "Cannot Delete Default Currency",
                "Cannot delete the default currency. Please set another currency as default first.",
                MessageDialog.MessageType.Warning);
            warningDialog.ShowDialog();
            return;
        }
        
        var confirmDialog = new ConfirmationDialog(
            "Confirm Delete",
            $"Are you sure you want to delete the currency '{currency.CurrencyName}' ({currency.CurrencyCode})?",
            ConfirmationDialog.DialogType.Danger);
            
        if (confirmDialog.ShowDialog() != true) return;
        
        try
        {
            var success = await _currencyService.DeleteAsync(currency.Id);
            if (success)
            {
                Currencies.Remove(currency);
                StatusMessage = $"Currency '{currency.CurrencyName}' deleted.";
                _filteredCurrenciesView.Refresh();
                OnPropertyChanged(nameof(HasCurrencies));
                OnPropertyChanged(nameof(TotalCurrencies));
            }
            else
            {
                StatusMessage = $"Failed to delete currency '{currency.CurrencyName}'.";
                var errorDialog = new MessageDialog(
                    "Error",
                    $"Failed to delete currency '{currency.CurrencyName}'.",
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting currency: {ex.Message}";
            var errorDialog = new MessageDialog(
                "Error",
                $"Error deleting currency: {ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
    }

    private async Task ActivateCurrencyAsync(CurrencyDto? currency)
    {
        if (currency == null) return;

        if (currency.IsDefault)
        {
            var infoDialog = new MessageDialog(
                "Already Active",
                "This currency is already the active currency.",
                MessageDialog.MessageType.Info);
            infoDialog.ShowDialog();
            return;
        }

        var confirmDialog = new ConfirmationDialog(
            "Confirm Currency Activation",
            $"Are you sure you want to set '{currency.CurrencyName} ({currency.CurrencyCode})' as the active currency for the entire system?\n\n" +
            "This will change the currency symbol displayed throughout the application.\n" +
            "Note: Prices will remain the same - only the symbol will change.",
            ConfirmationDialog.DialogType.Warning);

        if (confirmDialog.ShowDialog() != true) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Activating currency...";

            await _currencyService.SetDefaultCurrencyAsync(currency.Id);

            // Refresh the Active Currency Service to update system-wide currency
            await _activeCurrencyService.RefreshAsync();

            StatusMessage = $"Currency '{currency.CurrencyName}' activated successfully";
            var successDialog = new MessageDialog(
                "Currency Activated",
                $"Currency '{currency.CurrencyName} ({currency.Symbol})' is now the active currency for the system.\n\n" +
                "All prices will now display with the '{currency.Symbol}' symbol.\n" +
                "Price values remain unchanged - only the currency symbol is updated.",
                MessageDialog.MessageType.Success);
            successDialog.ShowDialog();

            // Reload currencies to reflect the change
            await LoadCurrenciesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error activating currency: {ex.Message}";
            var errorDialog = new MessageDialog(
                "Error",
                $"Error activating currency: {ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ImportCurrenciesAsync()
    {
        try
        {
            // Show custom import dialog
            var importDialog = new ImportDialog();
            var dialogResult = importDialog.ShowDialog();
            
            if (dialogResult != true)
                return;

            if (importDialog.SelectedAction == ImportDialog.ImportAction.DownloadTemplate)
            {
                // Download Template
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = "Currencies_Template.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var templateCsv = new StringBuilder();
                    templateCsv.AppendLine("Id,Currency Name,Currency Code,Symbol,Image Path,Exchange Rate,Is Default");
                    templateCsv.AppendLine("0,US Dollar,USD,$,,1.0000,true");
                    templateCsv.AppendLine("0,Euro,EUR,€,,0.9200,false");
                    templateCsv.AppendLine("0,British Pound,GBP,£,C:\\Images\\gbp.png,0.7900,false");

                    await File.WriteAllTextAsync(saveFileDialog.FileName, templateCsv.ToString());
                    var successDialog = new MessageDialog(
                        "Template Downloaded",
                        $"Template downloaded successfully to:\n{saveFileDialog.FileName}\n\nPlease fill in your data and use the Import function again to upload it.\n\nNote: Image Path column is optional. Leave blank if no image.",
                        MessageDialog.MessageType.Success);
                    successDialog.ShowDialog();
                }
                return;
            }

            // Upload File
            if (importDialog.SelectedAction == ImportDialog.ImportAction.UploadFile)
            {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                IsLoading = true;
                StatusMessage = "Importing currencies...";

                var lines = await File.ReadAllLinesAsync(openFileDialog.FileName);
                if (lines.Length <= 1)
                {
                    var warningDialog = new MessageDialog(
                        "Import Error",
                        "The CSV file is empty or contains only headers.",
                        MessageDialog.MessageType.Warning);
                    warningDialog.ShowDialog();
                    return;
                }

                int successCount = 0;
                int errorCount = 0;
                var errors = new StringBuilder();

                // Skip header row
                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var line = lines[i];
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var values = ParseCsvLine(line);
                        if (values.Length < 6)
                        {
                            errorCount++;
                            errors.AppendLine($"Line {i + 1}: Invalid format (expected at least 6 columns)");
                            continue;
                        }

                        // Handle optional image path (column 5)
                        string? imagePath = null;
                        if (values.Length >= 7 && !string.IsNullOrWhiteSpace(values[4].Trim('"')))
                        {
                            var sourceImagePath = values[4].Trim('"');
                            if (File.Exists(sourceImagePath))
                            {
                                // Copy image to app data folder
                                var appDataFolder = Path.Combine(
                                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                    "ChronoPos",
                                    "CurrencyImages");
                                
                                if (!Directory.Exists(appDataFolder))
                                {
                                    Directory.CreateDirectory(appDataFolder);
                                }
                                
                                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(sourceImagePath)}";
                                imagePath = Path.Combine(appDataFolder, fileName);
                                File.Copy(sourceImagePath, imagePath, true);
                            }
                        }

                        var createDto = new CreateCurrencyDto
                        {
                            CurrencyName = values[1].Trim('"'),
                            CurrencyCode = values[2].Trim('"').ToUpper(),
                            Symbol = values[3].Trim('"'),
                            ImagePath = imagePath,
                            ExchangeRate = decimal.Parse(values.Length >= 7 ? values[5].Trim('"') : values[4].Trim('"')),
                            IsDefault = bool.Parse(values.Length >= 7 ? values[6].Trim('"') : values[5].Trim('"'))
                        };

                        await _currencyService.CreateAsync(createDto);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.AppendLine($"Line {i + 1}: {ex.Message}");
                    }
                }

                await LoadCurrenciesAsync();

                var message = $"Import completed:\n✓ {successCount} currencies imported successfully";
                if (errorCount > 0)
                {
                    message += $"\n✗ {errorCount} errors occurred\n\nErrors:\n{errors}";
                }

                var resultDialog = new MessageDialog(
                    "Import Complete",
                    message,
                    errorCount > 0 ? MessageDialog.MessageType.Warning : MessageDialog.MessageType.Success);
                resultDialog.ShowDialog();
                
                StatusMessage = $"Import completed: {successCount} successful, {errorCount} errors";
            }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error importing currencies: {ex.Message}";
            var errorDialog = new MessageDialog(
                "Import Error",
                $"Error importing currencies: {ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                currentValue.Append(c);
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.ToString());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(c);
            }
        }

        values.Add(currentValue.ToString());
        return values.ToArray();
    }

    private async Task ExportCurrenciesAsync()
    {
        if (!CanExportCurrency)
        {
            var warningDialog = new MessageDialog(
                "Permission Denied",
                "You do not have permission to export currencies.",
                MessageDialog.MessageType.Warning);
            warningDialog.ShowDialog();
            return;
        }

        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"currencies_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            var csv = new StringBuilder();
            csv.AppendLine("Currency Name,Currency Code,Symbol,Image Path,Exchange Rate,Is Default");
            
            foreach (var currency in Currencies)
            {
                var imagePath = currency.ImagePath ?? string.Empty;
                csv.AppendLine($"\"{currency.CurrencyName}\",\"{currency.CurrencyCode}\",\"{currency.Symbol}\",\"{imagePath}\",{currency.ExchangeRate:F4},{currency.IsDefault}");
            }

            await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
            
            StatusMessage = $"Exported {Currencies.Count} currencies to {saveFileDialog.FileName}";
            var successDialog = new MessageDialog(
                "Export Complete",
                $"Successfully exported {Currencies.Count} currencies.",
                MessageDialog.MessageType.Success);
            successDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error exporting currencies: {ex.Message}";
            var errorDialog = new MessageDialog(
                "Export Error",
                $"Error exporting currencies: {ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
    }

    private void InitializePermissions()
    {
        try
        {
            // Use CURRENCY screen name if it exists in ScreenNames constants
            // Otherwise, default to true for development
            CanCreateCurrency = true; // TODO: Update when CURRENCY screen is added to ScreenNames
            CanEditCurrency = true;
            CanDeleteCurrency = true;
            CanImportCurrency = true;
            CanExportCurrency = true;
            
            // Uncomment when CURRENCY is added to ScreenNames:
            // CanCreateCurrency = _currentUserService.HasPermission(ScreenNames.CURRENCY, TypeMatrix.CREATE);
            // CanEditCurrency = _currentUserService.HasPermission(ScreenNames.CURRENCY, TypeMatrix.UPDATE);
            // CanDeleteCurrency = _currentUserService.HasPermission(ScreenNames.CURRENCY, TypeMatrix.DELETE);
            // CanImportCurrency = _currentUserService.HasPermission(ScreenNames.CURRENCY, TypeMatrix.IMPORT);
            // CanExportCurrency = _currentUserService.HasPermission(ScreenNames.CURRENCY, TypeMatrix.EXPORT);
        }
        catch (Exception)
        {
            // Fail-secure: all permissions default to false
            CanCreateCurrency = false;
            CanEditCurrency = false;
            CanDeleteCurrency = false;
            CanExportCurrency = false;
        }
    }

    private void GoBack()
    {
        if (_navigateBack != null)
        {
            _navigateBack.Invoke();
        }
        else
        {
            StatusMessage = "Navigation back not configured.";
        }
    }

    private void InitializeCurrentSettings()
    {
        // Set initial flow direction based on current language
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft 
            : FlowDirection.LeftToRight;
    }

    /// <summary>
    /// Load translations from database
    /// </summary>
    private async Task LoadTranslationsAsync()
    {
        try
        {
            PageTitle = await _databaseLocalizationService.GetTranslationAsync("currency.page_title") ?? "Currency Management";
            BackButtonText = await _databaseLocalizationService.GetTranslationAsync("common.back") ?? "Back";
            RefreshButtonText = await _databaseLocalizationService.GetTranslationAsync("common.refresh") ?? "Refresh";
            AddCurrencyButtonText = await _databaseLocalizationService.GetTranslationAsync("currency.add_currency") ?? "Add Currency";
            SearchPlaceholder = await _databaseLocalizationService.GetTranslationAsync("currency.search_placeholder") ?? "Search currencies by name or code...";
            
            // Button text
            ImportButtonText = await _databaseLocalizationService.GetTranslationAsync("common.import") ?? "Import";
            ExportButtonText = await _databaseLocalizationService.GetTranslationAsync("common.export") ?? "Export";
            LoadingText = await _databaseLocalizationService.GetTranslationAsync("currency.loading") ?? "Loading currencies...";
            NoDataText = await _databaseLocalizationService.GetTranslationAsync("currency.no_data") ?? "No currencies found";
            NoDataHintText = await _databaseLocalizationService.GetTranslationAsync("currency.no_data_hint") ?? "Click 'Add Currency' to create your first currency";
            ItemsCountText = await _databaseLocalizationService.GetTranslationAsync("currency.items_count") ?? "currencies";
            
            // Column headers
            ColumnCurrencyName = await _databaseLocalizationService.GetTranslationAsync("currency.column.name") ?? "Currency Name";
            ColumnCurrencyCode = await _databaseLocalizationService.GetTranslationAsync("currency.column.code") ?? "Code";
            ColumnCurrencySymbol = await _databaseLocalizationService.GetTranslationAsync("currency.column.symbol") ?? "Symbol";
            ColumnExchangeRate = await _databaseLocalizationService.GetTranslationAsync("currency.column.exchange_rate") ?? "Exchange Rate";
            ColumnIsDefault = await _databaseLocalizationService.GetTranslationAsync("currency.column.is_default") ?? "Default";
            ColumnActions = await _databaseLocalizationService.GetTranslationAsync("common.actions") ?? "Actions";
        }
        catch (Exception ex)
        {
            // Log error but don't throw - use default English text
            System.Diagnostics.Debug.WriteLine($"Error loading translations: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle layout direction changes
    /// </summary>
    private void OnDirectionChanged(LayoutDirection newDirection)
    {
        CurrentFlowDirection = newDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft 
            : FlowDirection.LeftToRight;
    }

    /// <summary>
    /// Handle language changes
    /// </summary>
    private void OnLanguageChanged(object? sender, string languageCode)
    {
        _ = Task.Run(LoadTranslationsAsync);
    }

    public void Dispose()
    {
        PropertyChanged -= OnPropertyChanged;
        _layoutDirectionService.DirectionChanged -= OnDirectionChanged;
        _databaseLocalizationService.LanguageChanged -= OnLanguageChanged;
        GC.SuppressFinalize(this);
    }
}

// Helper class for status filter dropdown
public class StatusFilterOption
{
    public string Display { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
