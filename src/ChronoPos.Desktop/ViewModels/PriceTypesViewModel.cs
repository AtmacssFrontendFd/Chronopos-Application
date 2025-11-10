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
using ChronoPos.Desktop.Views.Dialogs;
using ChronoPos.Infrastructure.Services;
using Microsoft.Win32;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for managing selling price types
/// </summary>
public partial class PriceTypesViewModel : ObservableObject
{
    #region Private Fields

    private readonly ISellingPriceTypeService _sellingPriceTypeService;
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
    /// Collection of selling price types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<SellingPriceTypeDto> _priceTypes = new();

    /// <summary>
    /// Currently selected price type
    /// </summary>
    [ObservableProperty]
    private SellingPriceTypeDto? _selectedPriceType;



    /// <summary>
    /// Whether the side panel is visible
    /// </summary>
    [ObservableProperty]
    private bool _isSidePanelVisible = false;

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
    /// Whether to show only active price types
    /// </summary>
    [ObservableProperty]
    private bool _showActiveOnly = true;

    /// <summary>
    /// Search text for filtering price types
    /// </summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// Current flow direction for UI
    /// </summary>
    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private bool canCreatePriceType = false;

    [ObservableProperty]
    private bool canEditPriceType = false;

    [ObservableProperty]
    private bool canDeletePriceType = false;

    [ObservableProperty]
    private bool canImportPriceType = false;

    [ObservableProperty]
    private bool canExportPriceType = false;

    /// <summary>
    /// Side panel view model
    /// </summary>
    [ObservableProperty]
    private PriceTypeSidePanelViewModel? _sidePanelViewModel;

    // Localized properties
    [ObservableProperty]
    private string _pageTitle = "Price Types";

    [ObservableProperty]
    private string _searchPlaceholder = "Search price types...";

    [ObservableProperty]
    private string _addButtonText = "Add Price Type";

    [ObservableProperty]
    private string _refreshButtonText = "Refresh";

    [ObservableProperty]
    private string _importButtonText = "Import";

    [ObservableProperty]
    private string _exportButtonText = "Export";

    [ObservableProperty]
    private string _editButtonText = "Edit";

    [ObservableProperty]
    private string _deleteButtonText = "Delete";

    [ObservableProperty]
    private string _clearFiltersText = "Clear Filters";

    [ObservableProperty]
    private string _activeOnlyText = "Active Only";

    [ObservableProperty]
    private string _showAllText = "Show All";

    [ObservableProperty]
    private string _columnActions = "Actions";

    [ObservableProperty]
    private string _columnActive = "Active";

    [ObservableProperty]
    private string _columnStatus = "Status";

    [ObservableProperty]
    private string _columnCreated = "Created";

    [ObservableProperty]
    private string _columnDescription = "Description";

    [ObservableProperty]
    private string _columnArabicName = "Arabic Name";

    [ObservableProperty]
    private string _columnTypeName = "Type Name";

    [ObservableProperty]
    private string _emptyStateTitle = "No price types found";

    [ObservableProperty]
    private string _emptyStateMessage = "Click 'Add Price Type' to create your first price type";

    [ObservableProperty]
    private string _loadingText = "Loading price types...";

    #endregion

    #region Computed Properties

    /// <summary>
    /// Filtered collection of price types based on search text and active filter
    /// </summary>
    public IEnumerable<SellingPriceTypeDto> FilteredPriceTypes
    {
        get
        {
            var filtered = PriceTypes.AsEnumerable();

            // Apply active filter
            if (ShowActiveOnly)
            {
                filtered = filtered.Where(pt => pt.Status);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLowerInvariant();
                filtered = filtered.Where(pt => 
                    (pt.TypeName?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (pt.ArabicName?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (pt.Description?.ToLowerInvariant().Contains(searchLower) ?? false));
            }

            return filtered;
        }
    }

    /// <summary>
    /// Whether there are any price types
    /// </summary>
    public bool HasPriceTypes => PriceTypes.Any();

    /// <summary>
    /// Total number of price types
    /// </summary>
    public int TotalPriceTypes => PriceTypes.Count;

    #endregion

    #region Property Changed Overrides

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredPriceTypes));
    }

    partial void OnShowActiveOnlyChanged(bool value)
    {
        OnPropertyChanged(nameof(FilteredPriceTypes));
        OnPropertyChanged(nameof(HasPriceTypes));
    }

    partial void OnPriceTypesChanged(ObservableCollection<SellingPriceTypeDto> value)
    {
        OnPropertyChanged(nameof(FilteredPriceTypes));
        OnPropertyChanged(nameof(HasPriceTypes));
        OnPropertyChanged(nameof(TotalPriceTypes));
    }

    #endregion

    #region Constructor

    public PriceTypesViewModel(
        ISellingPriceTypeService sellingPriceTypeService,
        ICurrentUserService currentUserService,
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        IDatabaseLocalizationService databaseLocalizationService)
    {
        _sellingPriceTypeService = sellingPriceTypeService ?? throw new ArgumentNullException(nameof(sellingPriceTypeService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));

        InitializePermissions();
        
        // Subscribe to layout direction changes
        _layoutDirectionService.DirectionChanged += OnDirectionChanged;
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        // Subscribe to language changes
        _databaseLocalizationService.LanguageChanged += OnLanguageChanged;

        // Load localized texts and initial data
        _ = Task.Run(async () =>
        {
            await LoadLocalizedTextsAsync();
            await LoadPriceTypesAsync();
        });
    }

    #endregion

    #region Commands

    /// <summary>
    /// Command to add a new price type
    /// </summary>
    [RelayCommand]
    private void AddPriceType()
    {
        SidePanelViewModel = new PriceTypeSidePanelViewModel(
            _sellingPriceTypeService,
            OnSidePanelSaved,
            OnSidePanelCancelled);
        
        IsSidePanelVisible = true;
        StatusMessage = "Ready to add new price type";
    }

    /// <summary>
    /// Command to edit a price type
    /// </summary>
    [RelayCommand]
    private void EditPriceType(SellingPriceTypeDto? priceType)
    {
        if (priceType != null)
        {
            SidePanelViewModel = new PriceTypeSidePanelViewModel(
                _sellingPriceTypeService,
                priceType,
                OnSidePanelSaved,
                OnSidePanelCancelled);
            
            IsSidePanelVisible = true;
            StatusMessage = $"Editing '{priceType.TypeName}'";
        }
    }



    /// <summary>
    /// Command to delete a price type
    /// </summary>
    [RelayCommand]
    private async Task DeletePriceTypeAsync(SellingPriceTypeDto? priceType)
    {
        if (priceType == null)
        {
            var warningDialog = new MessageDialog(
                "No Selection",
                "Please select a price type to delete.",
                MessageDialog.MessageType.Warning);
            warningDialog.ShowDialog();
            return;
        }

        var confirmDialog = new ConfirmationDialog(
            "Delete Price Type",
            $"Are you sure you want to delete '{priceType.TypeName}'?\n\nThis action cannot be undone.",
            ConfirmationDialog.DialogType.Danger);
        
        var result = confirmDialog.ShowDialog();
        if (result == true)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Deleting price type...";

                await _sellingPriceTypeService.DeleteAsync(priceType.Id, 1); // TODO: Get user ID from context
                StatusMessage = "Price type deleted successfully";
                await LoadPriceTypesAsync();
                
                var successDialog = new MessageDialog(
                    "Success",
                    "Price type deleted successfully.",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting price type: {ex.Message}";
                
                var errorDialog = new MessageDialog(
                    "Delete Error",
                    $"An error occurred while deleting the price type:\n\n{ex.Message}",
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }





    /// <summary>
    /// Command to filter active price types
    /// </summary>
    [RelayCommand]
    private void FilterActive()
    {
        ShowActiveOnly = !ShowActiveOnly;
        StatusMessage = ShowActiveOnly ? "Showing only active price types" : "Showing all price types";
        OnPropertyChanged(nameof(FilteredPriceTypes));
    }

    /// <summary>
    /// Command to clear filters
    /// </summary>
    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        ShowActiveOnly = false;
        StatusMessage = "Filters cleared";
        OnPropertyChanged(nameof(FilteredPriceTypes));
    }

    /// <summary>
    /// Command to export price types to CSV
    /// </summary>
    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"PriceTypes_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsLoading = true;
                StatusMessage = "Exporting price types...";

                var csv = new StringBuilder();
                csv.AppendLine("TypeName,ArabicName,Description,Status");

                foreach (var priceType in PriceTypes)
                {
                    csv.AppendLine($"\"{priceType.TypeName}\"," +
                                 $"\"{priceType.ArabicName}\"," +
                                 $"\"{priceType.Description ?? ""}\"," +
                                 $"{priceType.Status}");
                }

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                StatusMessage = $"Exported {PriceTypes.Count} price types successfully";
                
                var successDialog = new MessageDialog(
                    "Export Successful",
                    $"Successfully exported {PriceTypes.Count} price types to:\n\n{saveFileDialog.FileName}",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error exporting price types: {ex.Message}";
            
            var errorDialog = new MessageDialog(
                "Export Error",
                $"An error occurred while exporting price types:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to import price types from CSV
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
                    FileName = "PriceTypes_Template.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var templateCsv = new StringBuilder();
                    templateCsv.AppendLine("TypeName,ArabicName,Description,Status");
                    templateCsv.AppendLine("Sample Price Type,نوع السعر النموذجي,Sample description,true");
                    templateCsv.AppendLine("Retail Price,سعر التجزئة,Standard retail pricing,true");

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
                    StatusMessage = "Importing price types...";

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
                        if (values.Length < 4)
                        {
                            errorCount++;
                            errors.AppendLine($"Line {i + 1}: Invalid format (expected 4 columns)");
                            continue;
                        }

                        var createDto = new CreateSellingPriceTypeDto
                        {
                            TypeName = values[0].Trim('"'),
                            ArabicName = values[1].Trim('"'),
                            Description = string.IsNullOrWhiteSpace(values[2].Trim('"')) ? null : values[2].Trim('"'),
                            Status = bool.Parse(values[3]),
                            CreatedBy = 1 // TODO: Get from current user
                        };

                        await _sellingPriceTypeService.CreateAsync(createDto);
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

                await LoadPriceTypesAsync();

                var message = $"Import completed:\n\n✓ {successCount} price types imported successfully";
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
            StatusMessage = $"Error importing price types: {ex.Message}";
            
            var errorDialog = new MessageDialog(
                "Import Error",
                $"An error occurred while importing price types:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to download CSV template for importing price types
    /// </summary>
    [RelayCommand]
    private async Task DownloadTemplateAsync()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"PriceTypes_Template.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                StatusMessage = "Generating template...";

                var csv = new StringBuilder();
                csv.AppendLine("TypeName,ArabicName,Description,Status");
                csv.AppendLine("\"Retail\",\"تجزئة\",\"Standard retail price\",true");
                csv.AppendLine("\"Wholesale\",\"جملة\",\"Bulk purchase price\",true");
                csv.AppendLine("\"VIP\",\"في آي بي\",\"Special customer price\",true");

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                StatusMessage = "Template downloaded successfully";
                
                var successDialog = new MessageDialog(
                    "Template Downloaded",
                    $"Template file downloaded to:\n\n{saveFileDialog.FileName}\n\nYou can now fill in your data and import it.",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error downloading template: {ex.Message}";
            
            var errorDialog = new MessageDialog(
                "Template Error",
                $"An error occurred while downloading template:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
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

    #region Public Properties

    /// <summary>
    /// Action to navigate back
    /// </summary>
    public Action? GoBackAction { get; set; }

    /// <summary>
    /// Command for back navigation
    /// </summary>
    [RelayCommand]
    private void Back()
    {
        GoBackAction?.Invoke();
    }

    /// <summary>
    /// Command to refresh data
    /// </summary>
    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        await LoadPriceTypesAsync();
    }

    /// <summary>
    /// Command to toggle active status
    /// </summary>
    [RelayCommand]
    private async Task ToggleActiveAsync(SellingPriceTypeDto? priceType)
    {
        if (priceType != null)
        {
            try
            {
                var updateDto = new UpdateSellingPriceTypeDto
                {
                    Id = priceType.Id,
                    TypeName = priceType.TypeName,
                    ArabicName = priceType.ArabicName,
                    Description = priceType.Description,
                    Status = !priceType.Status,
                    UpdatedBy = 1 // TODO: Get from current user context
                };

                await _sellingPriceTypeService.UpdateAsync(updateDto);
                await LoadPriceTypesAsync();
                StatusMessage = $"Price type {(updateDto.Status ? "activated" : "deactivated")} successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating price type status: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// Command to view price type details
    /// </summary>
    [RelayCommand]
    private void ViewPriceTypeDetails(SellingPriceTypeDto? priceType)
    {
        if (priceType != null)
        {
            StatusMessage = $"Viewing details for '{priceType.TypeName}'";
            // TODO: Implement details view
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Load price types from the service
    /// </summary>
    private async Task LoadPriceTypesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading price types...";

            // Always load all price types, filtering is handled by FilteredPriceTypes property
            var priceTypes = await _sellingPriceTypeService.GetAllAsync();

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                PriceTypes.Clear();
                foreach (var priceType in priceTypes)
                {
                    PriceTypes.Add(priceType);
                }
            });
            
            // Force refresh of filtered collection
            OnPropertyChanged(nameof(FilteredPriceTypes));

            StatusMessage = $"Loaded {PriceTypes.Count} price types";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading price types: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Handle layout direction changes
    /// </summary>
    private void OnDirectionChanged(LayoutDirection direction)
    {
        CurrentFlowDirection = direction == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    /// <summary>
    /// Callback when side panel saves
    /// </summary>
    private async void OnSidePanelSaved(bool success)
    {
        if (success)
        {
            IsSidePanelVisible = false;
            SidePanelViewModel = null;
            await LoadPriceTypesAsync();
            StatusMessage = "Price type saved successfully";
        }
    }

    /// <summary>
    /// Callback when side panel is cancelled
    /// </summary>
    private void OnSidePanelCancelled()
    {
        IsSidePanelVisible = false;
        SidePanelViewModel = null;
        StatusMessage = "Operation cancelled";
    }

    private void InitializePermissions()
    {
        try
        {
            CanCreatePriceType = _currentUserService.HasPermission(ScreenNames.PRICE_TYPES, TypeMatrix.CREATE);
            CanEditPriceType = _currentUserService.HasPermission(ScreenNames.PRICE_TYPES, TypeMatrix.UPDATE);
            CanDeletePriceType = _currentUserService.HasPermission(ScreenNames.PRICE_TYPES, TypeMatrix.DELETE);
            CanImportPriceType = _currentUserService.HasPermission(ScreenNames.PRICE_TYPES, TypeMatrix.IMPORT);
            CanExportPriceType = _currentUserService.HasPermission(ScreenNames.PRICE_TYPES, TypeMatrix.EXPORT);
        }
        catch (Exception)
        {
            CanCreatePriceType = false;
            CanEditPriceType = false;
            CanDeletePriceType = false;
            CanImportPriceType = false;
            CanExportPriceType = false;
        }
    }

    private async Task LoadLocalizedTextsAsync()
    {
        try
        {
            PageTitle = await _databaseLocalizationService.GetTranslationAsync("pricetype.page_title") ?? "Price Types";
            SearchPlaceholder = await _databaseLocalizationService.GetTranslationAsync("pricetype.search_placeholder") ?? "Search price types...";
            AddButtonText = await _databaseLocalizationService.GetTranslationAsync("pricetype.add_price_type") ?? "Add Price Type";
            RefreshButtonText = await _databaseLocalizationService.GetTranslationAsync("common.refresh") ?? "Refresh";
            ImportButtonText = await _databaseLocalizationService.GetTranslationAsync("common.import") ?? "Import";
            ExportButtonText = await _databaseLocalizationService.GetTranslationAsync("common.export") ?? "Export";
            EditButtonText = await _databaseLocalizationService.GetTranslationAsync("common.edit") ?? "Edit";
            DeleteButtonText = await _databaseLocalizationService.GetTranslationAsync("common.delete") ?? "Delete";
            ClearFiltersText = await _databaseLocalizationService.GetTranslationAsync("common.clear_filters") ?? "Clear Filters";
            ActiveOnlyText = await _databaseLocalizationService.GetTranslationAsync("pricetype.active_only") ?? "Active Only";
            ShowAllText = await _databaseLocalizationService.GetTranslationAsync("pricetype.show_all") ?? "Show All";
            ColumnActions = await _databaseLocalizationService.GetTranslationAsync("pricetype.column.actions") ?? "Actions";
            ColumnActive = await _databaseLocalizationService.GetTranslationAsync("pricetype.column.active") ?? "Active";
            ColumnStatus = await _databaseLocalizationService.GetTranslationAsync("pricetype.column.status") ?? "Status";
            ColumnCreated = await _databaseLocalizationService.GetTranslationAsync("pricetype.column.created") ?? "Created";
            ColumnDescription = await _databaseLocalizationService.GetTranslationAsync("pricetype.column.description") ?? "Description";
            ColumnArabicName = await _databaseLocalizationService.GetTranslationAsync("pricetype.column.arabic_name") ?? "Arabic Name";
            ColumnTypeName = await _databaseLocalizationService.GetTranslationAsync("pricetype.column.type_name") ?? "Type Name";
            EmptyStateTitle = await _databaseLocalizationService.GetTranslationAsync("pricetype.empty_state_title") ?? "No price types found";
            EmptyStateMessage = await _databaseLocalizationService.GetTranslationAsync("pricetype.empty_state_message") ?? "Click 'Add Price Type' to create your first price type";
            LoadingText = await _databaseLocalizationService.GetTranslationAsync("pricetype.loading") ?? "Loading price types...";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading localized texts: {ex.Message}");
        }
    }

    private async void OnLanguageChanged(object? sender, string languageCode)
    {
        await LoadLocalizedTextsAsync();
    }

    #endregion
}