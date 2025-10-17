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

namespace ChronoPos.Desktop.ViewModels;

public partial class CurrencyViewModel : ObservableObject
{
    private readonly ICurrencyService _currencyService;
    private readonly ICurrentUserService _currentUserService;
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

    public CurrencyViewModel(ICurrencyService currencyService, ICurrentUserService currentUserService, Action? navigateBack = null)
    {
        _currencyService = currencyService;
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _navigateBack = navigateBack;
        
        // Initialize permissions
        InitializePermissions();
        
        // Initialize status filters
        InitializeStatusFilters();
        
        // Initialize filtered view
        _filteredCurrenciesView = CollectionViewSource.GetDefaultView(Currencies);
        _filteredCurrenciesView.Filter = FilterCurrencies;

        // Subscribe to property changes
        PropertyChanged += OnPropertyChanged;
        
        // Load currencies on startup
        _ = LoadCurrenciesAsync();
    }

    public IAsyncRelayCommand LoadCurrenciesCommand => new AsyncRelayCommand(LoadCurrenciesAsync);
    public IRelayCommand AddCurrencyCommand => new RelayCommand(ShowAddCurrencyPanel);
    public IRelayCommand<CurrencyDto?> EditCurrencyCommand => new RelayCommand<CurrencyDto?>(ShowEditCurrencyPanel);
    public IAsyncRelayCommand<CurrencyDto?> DeleteCurrencyCommand => new AsyncRelayCommand<CurrencyDto?>(DeleteCurrencyAsync);
    public IAsyncRelayCommand RefreshCommand => new AsyncRelayCommand(LoadCurrenciesAsync);
    public IAsyncRelayCommand ImportCommand => new AsyncRelayCommand(ImportCurrenciesAsync);
    public IAsyncRelayCommand ExportCommand => new AsyncRelayCommand(ExportCurrenciesAsync);
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
            MessageBox.Show($"Error loading currencies: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBox.Show("You do not have permission to create currencies.", "Permission Denied", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
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
            MessageBox.Show("You do not have permission to edit currencies.", "Permission Denied", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
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
            MessageBox.Show("You do not have permission to delete currencies.", "Permission Denied", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (currency.IsDefault)
        {
            MessageBox.Show("Cannot delete the default currency. Please set another currency as default first.", 
                "Cannot Delete Default Currency", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        var result = MessageBox.Show(
            $"Are you sure you want to delete the currency '{currency.CurrencyName}' ({currency.CurrencyCode})?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
            
        if (result != MessageBoxResult.Yes) return;
        
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
                MessageBox.Show($"Failed to delete currency '{currency.CurrencyName}'.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting currency: {ex.Message}";
            MessageBox.Show($"Error deleting currency: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ImportCurrenciesAsync()
    {
        try
        {
            // Show dialog with Download Template and Upload File options
            var result = MessageBox.Show(
                "Would you like to download a template first?\n\n" +
                "• Click 'Yes' to download the CSV template\n" +
                "• Click 'No' to upload your file directly\n" +
                "• Click 'Cancel' to exit",
                "Import Currencies",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
                return;

            if (result == MessageBoxResult.Yes)
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
                    MessageBox.Show($"Template downloaded successfully to:\n{saveFileDialog.FileName}\n\nPlease fill in your data and use the Import function again to upload it.\n\nNote: Image Path column is optional. Leave blank if no image.", 
                        "Template Downloaded", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                return;
            }

            // Upload File
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
                    MessageBox.Show("The CSV file is empty or contains only headers.", "Import Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
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

                MessageBox.Show(message, "Import Complete", 
                    MessageBoxButton.OK, errorCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
                
                StatusMessage = $"Import completed: {successCount} successful, {errorCount} errors";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error importing currencies: {ex.Message}";
            MessageBox.Show($"Error importing currencies: {ex.Message}", "Import Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBox.Show("You do not have permission to export currencies.", "Permission Denied", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
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
            MessageBox.Show($"Successfully exported {Currencies.Count} currencies.", "Export Complete", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error exporting currencies: {ex.Message}";
            MessageBox.Show($"Error exporting currencies: {ex.Message}", "Export Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
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
}

// Helper class for status filter dropdown
public class StatusFilterOption
{
    public string Display { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
