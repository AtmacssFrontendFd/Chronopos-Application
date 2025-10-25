using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Constants;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.ViewModels;
using ChronoPos.Desktop.Views.Dialogs;
using ChronoPos.Infrastructure.Services;
using Microsoft.Win32;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for managing tax types
/// </summary>
public partial class TaxTypesViewModel : ObservableObject
{
    #region Private Fields

    private readonly ITaxTypeService _taxTypeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IThemeService _themeService;
    private readonly IZoomService _zoomService;
    private readonly ILocalizationService _localizationService;
    private readonly IColorSchemeService _colorSchemeService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly IFontService _fontService;
    private readonly IDatabaseLocalizationService _databaseLocalizationService;

    #endregion

    #region Observable Properties

    /// <summary>
    /// Collection of tax types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TaxTypeDto> _taxTypes = new();

    /// <summary>
    /// Currently selected tax type
    /// </summary>
    [ObservableProperty]
    private TaxTypeDto? _selectedTaxType;

    /// <summary>
    /// Loading indicator
    /// </summary>
    [ObservableProperty]
    private bool _isLoading = false;

    /// <summary>
    /// Status message
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Whether to show only active tax types
    /// </summary>
    [ObservableProperty]
    private bool _showActiveOnly = true;

    /// <summary>
    /// Search text for filtering tax types
    /// </summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// Whether the side panel is visible
    /// </summary>
    [ObservableProperty]
    private bool _isSidePanelVisible = false;

    /// <summary>
    /// Current flow direction for UI
    /// </summary>
    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private bool canCreateTaxRate = false;

    [ObservableProperty]
    private bool canEditTaxRate = false;

    [ObservableProperty]
    private bool canDeleteTaxRate = false;

    [ObservableProperty]
    private bool canImportTaxRate = false;

    [ObservableProperty]
    private bool canExportTaxRate = false;

    /// <summary>
    /// Side panel view model
    /// </summary>
    [ObservableProperty]
    private TaxTypeSidePanelViewModel _sidePanelViewModel;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Filtered collection of tax types based on search and active filter
    /// </summary>
    public ObservableCollection<TaxTypeDto> FilteredTaxTypes
    {
        get
        {
            var filtered = TaxTypes.AsEnumerable();

            // Apply active filter
            if (ShowActiveOnly)
            {
                filtered = filtered.Where(t => t.IsActive);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(t => 
                    t.Name.ToLower().Contains(searchLower) ||
                    (!string.IsNullOrEmpty(t.Description) && t.Description.ToLower().Contains(searchLower)));
            }

            return new ObservableCollection<TaxTypeDto>(filtered);
        }
    }

    /// <summary>
    /// Whether there are any tax types
    /// </summary>
    public bool HasTaxTypes => TaxTypes.Count > 0;

    #endregion

    #region Commands

    /// <summary>
    /// Back navigation action (set by parent)
    /// </summary>
    public Action? GoBackAction { get; set; }

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor with all required services
    /// </summary>
    public TaxTypesViewModel(
        ITaxTypeService taxTypeService,
        ICurrentUserService currentUserService,
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        IDatabaseLocalizationService databaseLocalizationService)
    {
        _taxTypeService = taxTypeService ?? throw new ArgumentNullException(nameof(taxTypeService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));

        InitializePermissions();

        // Initialize side panel view model
        _sidePanelViewModel = new TaxTypeSidePanelViewModel(taxTypeService, layoutDirectionService);
        _sidePanelViewModel.OnSave += OnTaxTypeSaved;
        _sidePanelViewModel.OnCancel += OnSidePanelCancelRequested;
        _sidePanelViewModel.OnClose += OnSidePanelCancelRequested;

        // Initialize with current values
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        // Load tax types
        _ = Task.Run(LoadTaxTypesAsync);
    }

    #endregion

    #region Command Implementations

    /// <summary>
    /// Command to go back to previous screen
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        GoBackAction?.Invoke();
    }

    /// <summary>
    /// Command to refresh tax types
    /// </summary>
    [RelayCommand]
    private async Task Refresh()
    {
        await LoadTaxTypesAsync();
    }

    /// <summary>
    /// Command to add new tax type
    /// </summary>
    [RelayCommand]
    private void AddNew()
    {
        SidePanelViewModel.ResetForNew();
        IsSidePanelVisible = true;
    }

    /// <summary>
    /// Command to edit a tax type
    /// </summary>
    [RelayCommand]
    private void Edit(TaxTypeDto taxType)
    {
        if (taxType != null)
        {
            SidePanelViewModel.LoadTaxType(taxType);
            IsSidePanelVisible = true;
        }
    }

    /// <summary>
    /// Command to delete a tax type
    /// </summary>
    [RelayCommand]
    private async Task Delete(TaxTypeDto taxType)
    {
        if (taxType != null)
        {
            var confirmDialog = new ConfirmationDialog(
                "Delete Tax Type",
                $"Are you sure you want to delete the tax type '{taxType.Name}'?\n\nThis action cannot be undone.",
                ConfirmationDialog.DialogType.Danger);
            
            var result = confirmDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = "Deleting tax type...";
                    
                    await _taxTypeService.DeleteTaxTypeAsync(taxType.Id);
                    
                    StatusMessage = "Tax type deleted successfully";
                    await LoadTaxTypesAsync();
                    
                    var successDialog = new MessageDialog(
                        "Success",
                        "Tax type deleted successfully.",
                        MessageDialog.MessageType.Success);
                    successDialog.ShowDialog();
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error deleting tax type: {ex.Message}";
                    
                    var errorDialog = new MessageDialog(
                        "Delete Error",
                        $"An error occurred while deleting the tax type:\n\n{ex.Message}",
                        MessageDialog.MessageType.Error);
                    errorDialog.ShowDialog();
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }

    /// <summary>
    /// Command to toggle active status of a tax type
    /// </summary>
    [RelayCommand]
    private async Task ToggleActive(object parameter)
    {
        if (parameter is not TaxTypeDto taxType) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Updating tax type status...";

            // Toggle the active status
            taxType.IsActive = !taxType.IsActive;
            
            // Update the tax type
            await _taxTypeService.UpdateTaxTypeAsync(taxType);
            
            StatusMessage = "Tax type status updated successfully";
            OnPropertyChanged(nameof(FilteredTaxTypes));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating tax type status: {ex.Message}";
            MessageBox.Show($"Error updating tax type status: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to toggle active filter
    /// </summary>
    [RelayCommand]
    private void ToggleActiveFilter()
    {
        ShowActiveOnly = !ShowActiveOnly;
        OnPropertyChanged(nameof(FilteredTaxTypes));
    }

    /// <summary>
    /// Command to clear search
    /// </summary>
    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    /// <summary>
    /// Command to export tax types to CSV
    /// </summary>
    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"TaxRates_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsLoading = true;
                StatusMessage = "Exporting tax rates...";

                var csv = new StringBuilder();
                csv.AppendLine("Name,Description,Value,IsPercentage,IncludedInPrice,AppliesToBuying,AppliesToSelling,CalculationOrder,IsActive");

                foreach (var taxType in TaxTypes)
                {
                    csv.AppendLine($"\"{taxType.Name}\"," +
                                 $"\"{taxType.Description ?? ""}\"," +
                                 $"{taxType.Value}," +
                                 $"{taxType.IsPercentage}," +
                                 $"{taxType.IncludedInPrice}," +
                                 $"{taxType.AppliesToBuying}," +
                                 $"{taxType.AppliesToSelling}," +
                                 $"{taxType.CalculationOrder}," +
                                 $"{taxType.IsActive}");
                }

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                StatusMessage = $"Exported {TaxTypes.Count} tax rates successfully";
                
                var successDialog = new MessageDialog(
                    "Export Successful",
                    $"Successfully exported {TaxTypes.Count} tax rates to:\n\n{saveFileDialog.FileName}",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error exporting tax rates: {ex.Message}";
            
            var errorDialog = new MessageDialog(
                "Export Error",
                $"An error occurred while exporting tax rates:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to import tax types from CSV
    /// </summary>
    [RelayCommand]
    private async Task ImportAsync()
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
                    FileName = "TaxRates_Template.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var templateCsv = new StringBuilder();
                    templateCsv.AppendLine("Name,Description,Value,IsPercentage,IncludedInPrice,AppliesToBuying,AppliesToSelling,CalculationOrder,IsActive");
                    templateCsv.AppendLine("Sample Tax,Sample description,5,true,false,true,true,1,true");
                    templateCsv.AppendLine("VAT 15%,Value Added Tax,15,true,false,false,true,2,true");

                    await File.WriteAllTextAsync(saveFileDialog.FileName, templateCsv.ToString());
                    
                    var successDialog = new MessageDialog(
                        "Template Downloaded",
                        $"Template downloaded successfully to:\n\n{saveFileDialog.FileName}\n\nPlease fill in your data and use the Import function again to upload it.",
                        MessageDialog.MessageType.Success);
                    successDialog.ShowDialog();
                }
                return;
            }
            else if (importDialog.SelectedAction == ImportDialog.ImportAction.UploadFile)
            {
                // Upload File
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = ".csv"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    StatusMessage = "Importing tax rates...";

                    var lines = await File.ReadAllLinesAsync(openFileDialog.FileName);
                    if (lines.Length <= 1)
                    {
                        var warningDialog = new MessageDialog(
                            "Import Error",
                            "The CSV file is empty or contains only headers.",
                            MessageDialog.MessageType.Warning);
                        warningDialog.ShowDialog();
                        IsLoading = false;
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
                        if (values.Length < 9)
                        {
                            errorCount++;
                            errors.AppendLine($"Line {i + 1}: Invalid format (expected 9 columns)");
                            continue;
                        }

                        var taxTypeDto = new TaxTypeDto
                        {
                            Name = values[0].Trim('"'),
                            Description = string.IsNullOrWhiteSpace(values[1].Trim('"')) ? null : values[1].Trim('"'),
                            Value = decimal.Parse(values[2]),
                            IsPercentage = bool.Parse(values[3]),
                            IncludedInPrice = bool.Parse(values[4]),
                            AppliesToBuying = bool.Parse(values[5]),
                            AppliesToSelling = bool.Parse(values[6]),
                            CalculationOrder = int.Parse(values[7]),
                            IsActive = bool.Parse(values[8])
                        };

                        await _taxTypeService.CreateTaxTypeAsync(taxTypeDto);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        var errorMessage = ex.Message;
                        
                        // Include inner exception details if available
                        if (ex.InnerException != null)
                        {
                            errorMessage += $" | Inner: {ex.InnerException.Message}";
                            
                            // Go deeper if there's another inner exception
                            if (ex.InnerException.InnerException != null)
                            {
                                errorMessage += $" | Details: {ex.InnerException.InnerException.Message}";
                            }
                        }
                        
                        errors.AppendLine($"Line {i + 1}: {errorMessage}");
                    }
                }

                    await LoadTaxTypesAsync();

                    var message = $"Import completed:\n\n✓ {successCount} tax rates imported successfully";
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
            StatusMessage = $"Error importing tax rates: {ex.Message}";
            
            var errorDialog = new MessageDialog(
                "Import Error",
                $"An error occurred while importing tax rates:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to download CSV template for importing tax rates
    /// </summary>
    [RelayCommand]
    private async Task DownloadTemplateAsync()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"TaxRates_Template.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                StatusMessage = "Generating template...";

                var csv = new StringBuilder();
                csv.AppendLine("Id,Name,Description,Value,IsPercentage,IncludedInPrice,AppliesToBuying,AppliesToSelling,CalculationOrder,IsActive");
                csv.AppendLine("1,\"VAT 5%\",\"Value Added Tax 5%\",5.00,true,false,false,true,1,true");
                csv.AppendLine("2,\"VAT 15%\",\"Value Added Tax 15%\",15.00,true,false,false,true,2,true");
                csv.AppendLine("3,\"Import Tax\",\"Import Tax Fixed\",100.00,false,false,true,false,3,true");

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                StatusMessage = "Template downloaded successfully";
                MessageBox.Show($"Template file downloaded to:\n{saveFileDialog.FileName}\n\nYou can now fill in your data and import it.", 
                    "Template Downloaded", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error downloading template: {ex.Message}";
            MessageBox.Show($"Error downloading template: {ex.Message}", "Template Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Parse CSV line handling quoted values
    /// </summary>
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

    #endregion

    #region Private Methods

    /// <summary>
    /// Load all tax types from the service
    /// </summary>
    private async Task LoadTaxTypesAsync()
    {
        try
        {
            var taxTypes = await _taxTypeService.GetAllTaxTypesAsync();
            TaxTypes = new ObservableCollection<TaxTypeDto>(taxTypes);
            FilterTaxTypes();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading tax types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Filter tax types based on search and active status
    /// </summary>
    private void FilterTaxTypes()
    {
        // Since FilteredTaxTypes is a computed property, just notify that it changed
        OnPropertyChanged(nameof(FilteredTaxTypes));
    }

    /// <summary>
    /// Handle tax type saved event from side panel
    /// </summary>
    private async void OnTaxTypeSaved()
    {
        IsSidePanelVisible = false;
        await LoadTaxTypesAsync();
    }

    /// <summary>
    /// Handle side panel cancel event
    /// </summary>
    private void OnSidePanelCancelRequested()
    {
        IsSidePanelVisible = false;
    }

    #endregion

    #region Event Handlers

    partial void OnSearchTextChanged(string value)
    {
        FilterTaxTypes();
    }

    partial void OnShowActiveOnlyChanged(bool value)
    {
        FilterTaxTypes();
    }

    private void InitializePermissions()
    {
        try
        {
            CanCreateTaxRate = _currentUserService.HasPermission(ScreenNames.TAX_RATES, TypeMatrix.CREATE);
            CanEditTaxRate = _currentUserService.HasPermission(ScreenNames.TAX_RATES, TypeMatrix.UPDATE);
            CanDeleteTaxRate = _currentUserService.HasPermission(ScreenNames.TAX_RATES, TypeMatrix.DELETE);
            CanImportTaxRate = _currentUserService.HasPermission(ScreenNames.TAX_RATES, TypeMatrix.IMPORT);
            CanExportTaxRate = _currentUserService.HasPermission(ScreenNames.TAX_RATES, TypeMatrix.EXPORT);
        }
        catch (Exception)
        {
            CanCreateTaxRate = false;
            CanEditTaxRate = false;
            CanDeleteTaxRate = false;
            CanImportTaxRate = false;
            CanExportTaxRate = false;
        }
    }

    #endregion
}