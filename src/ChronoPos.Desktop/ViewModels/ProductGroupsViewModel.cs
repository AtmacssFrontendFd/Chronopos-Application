using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Constants;
using ChronoPos.Desktop.Views.Dialogs;
using ChronoPos.Desktop.Services;
using ChronoPos.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for Product Groups management page
/// </summary>
public partial class ProductGroupsViewModel : ObservableObject
{
    private readonly IProductGroupService _productGroupService;
    private readonly IProductGroupItemService _productGroupItemService;
    private readonly IDiscountService _discountService;
    private readonly ITaxTypeService _taxTypeService;
    private readonly ISellingPriceTypeService _sellingPriceTypeService;
    private readonly IProductService _productService;
    private readonly IProductUnitService _productUnitService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly IDatabaseLocalizationService _localizationService;

    [ObservableProperty]
    private ObservableCollection<ProductGroupDto> _productGroups = new();

    [ObservableProperty]
    private ObservableCollection<ProductGroupDto> _filteredProductGroups = new();

    [ObservableProperty]
    private ProductGroupDto? _selectedProductGroup;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isSidePanelVisible = false;

    [ObservableProperty]
    private ProductGroupSidePanelViewModel? _sidePanelViewModel;

    [ObservableProperty]
    private bool _showActiveOnly = true;

    [ObservableProperty]
    private System.Windows.FlowDirection _currentFlowDirection = System.Windows.FlowDirection.LeftToRight;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool canCreateProductGroup = false;

    [ObservableProperty]
    private bool canEditProductGroup = false;

    [ObservableProperty]
    private bool canDeleteProductGroup = false;

    [ObservableProperty]
    private bool canImportProductGroup = false;

    [ObservableProperty]
    private bool canExportProductGroup = false;

    // Localized properties
    [ObservableProperty]
    private string _pageTitle = "Product Groups";

    [ObservableProperty]
    private string _searchPlaceholder = "Search product groups...";

    [ObservableProperty]
    private string _addButtonText = "Add Product Group";

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
    private string _columnProducts = "Products";

    [ObservableProperty]
    private string _columnSkuPrefix = "SKU Prefix";

    [ObservableProperty]
    private string _columnDescription = "Description";

    [ObservableProperty]
    private string _columnArabicName = "Arabic Name";

    [ObservableProperty]
    private string _columnGroupName = "Group Name";

    [ObservableProperty]
    private string _emptyStateTitle = "No product groups found";

    [ObservableProperty]
    private string _emptyStateMessage = "Click 'Add Product Group' to create your first group";

    [ObservableProperty]
    private string _activeText = "Active";

    [ObservableProperty]
    private string _inactiveText = "Inactive";

    [ObservableProperty]
    private string _loadingText = "Loading product groups...";

    [ObservableProperty]
    private string _statusTextOf = "of";

    [ObservableProperty]
    private string _statusTextProductGroups = "product groups";

    [ObservableProperty]
    private string _noActiveGroupsText = "No active product groups found";

    [ObservableProperty]
    private string _noSearchResultsText = "No product groups match your search criteria";

    [ObservableProperty]
    private string _showingText = "Showing";

    /// <summary>
    /// Text for the active filter toggle button
    /// </summary>
    public string ActiveFilterButtonText => ShowActiveOnly ? "Show All" : "Active Only";

    /// <summary>
    /// Check if there are any product groups to display
    /// </summary>
    public bool HasProductGroups => FilteredProductGroups.Count > 0;

    /// <summary>
    /// Total number of product groups (before filtering)
    /// </summary>
    public int TotalProductGroups => ProductGroups.Count;

    /// <summary>
    /// Action to navigate back (set by parent)
    /// </summary>
    public Action? GoBackAction { get; set; }

    public ProductGroupsViewModel(
        IProductGroupService productGroupService,
        IProductGroupItemService productGroupItemService,
        IDiscountService discountService,
        ITaxTypeService taxTypeService,
        ISellingPriceTypeService sellingPriceTypeService,
        IProductService productService,
        IProductUnitService productUnitService,
        ICurrentUserService currentUserService,
        ILayoutDirectionService layoutDirectionService,
        IDatabaseLocalizationService localizationService)
    {
        _productGroupService = productGroupService;
        _productGroupItemService = productGroupItemService;
        _discountService = discountService;
        _taxTypeService = taxTypeService;
        _sellingPriceTypeService = sellingPriceTypeService;
        _productService = productService;
        _productUnitService = productUnitService;
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        
        // Subscribe to direction changes
        _layoutDirectionService.DirectionChanged += OnDirectionChanged;
        
        // Subscribe to language changes
        _localizationService.LanguageChanged += OnLanguageChanged;
        
        // Set initial direction
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft 
            : FlowDirection.LeftToRight;
        
        InitializePermissions();
        
        // Initialize side panel view model
        SidePanelViewModel = new ProductGroupSidePanelViewModel(
            _productGroupService,
            _productGroupItemService,
            _discountService,
            _taxTypeService,
            _sellingPriceTypeService,
            _productService,
            _productUnitService,
            CloseSidePanel,
            LoadProductGroupsAsync);
        
        // Load localized texts and data
        _ = Task.Run(async () =>
        {
            await LoadLocalizedTextsAsync();
            await LoadProductGroupsAsync();
        });
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterProductGroups();
    }

    partial void OnShowActiveOnlyChanged(bool value)
    {
        FilterProductGroups();
        OnPropertyChanged(nameof(ActiveFilterButtonText));
    }

    private void FilterProductGroups()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredProductGroups = new ObservableCollection<ProductGroupDto>(
                ShowActiveOnly 
                    ? ProductGroups.Where(pg => pg.Status == "Active")
                    : ProductGroups
            );
        }
        else
        {
            var searchLower = SearchText.ToLower();
            FilteredProductGroups = new ObservableCollection<ProductGroupDto>(
                (ShowActiveOnly 
                    ? ProductGroups.Where(pg => pg.Status == "Active")
                    : ProductGroups)
                .Where(pg => 
                    (pg.Name?.ToLower().Contains(searchLower) ?? false) ||
                    (pg.NameAr?.ToLower().Contains(searchLower) ?? false) ||
                    (pg.SkuPrefix?.ToLower().Contains(searchLower) ?? false) ||
                    (pg.Description?.ToLower().Contains(searchLower) ?? false))
            );
        }

        OnPropertyChanged(nameof(HasProductGroups));
        OnPropertyChanged(nameof(TotalProductGroups));
        UpdateStatusMessage();
    }

    private void UpdateStatusMessage()
    {
        if (IsLoading)
        {
            StatusMessage = LoadingText;
        }
        else if (!HasProductGroups && !string.IsNullOrWhiteSpace(SearchText))
        {
            StatusMessage = NoSearchResultsText;
        }
        else if (!HasProductGroups && ShowActiveOnly)
        {
            StatusMessage = NoActiveGroupsText;
        }
        else if (!HasProductGroups)
        {
            StatusMessage = EmptyStateTitle;
        }
        else
        {
            StatusMessage = $"{ShowingText} {FilteredProductGroups.Count} {StatusTextOf} {TotalProductGroups} {StatusTextProductGroups}";
        }
    }

    /// <summary>
    /// Load all product groups from database
    /// </summary>
    [RelayCommand]
    private async Task LoadProductGroupsAsync()
    {
        try
        {
            IsLoading = true;
            UpdateStatusMessage();
            
            var groups = await _productGroupService.GetAllAsync();
            ProductGroups = new ObservableCollection<ProductGroupDto>(groups);
            FilterProductGroups();
        }
        catch (Exception ex)
        {
            var errorDialog = new MessageDialog(
                "Loading Error",
                $"An error occurred while loading product groups:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
            
            StatusMessage = "Error loading product groups";
        }
        finally
        {
            IsLoading = false;
            UpdateStatusMessage();
        }
    }

    /// <summary>
    /// Toggle the active filter
    /// </summary>
    [RelayCommand]
    private void ToggleActiveFilter()
    {
        ShowActiveOnly = !ShowActiveOnly;
    }

    /// <summary>
    /// Clear all filters and search text
    /// </summary>
    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        ShowActiveOnly = false;
    }

    /// <summary>
    /// Toggle active status of a product group
    /// </summary>
    [RelayCommand]
    private async Task ToggleActiveAsync(ProductGroupDto productGroup)
    {
        if (productGroup == null)
            return;

        try
        {
            // Load full details
            var details = await _productGroupService.GetDetailByIdAsync(productGroup.Id);
            if (details != null)
            {
                // Create update DTO
                var updateDto = new UpdateProductGroupDto
                {
                    Id = details.Id,
                    Name = details.Name,
                    NameAr = details.NameAr,
                    Description = details.Description,
                    DescriptionAr = details.DescriptionAr,
                    DiscountId = details.DiscountId,
                    TaxTypeId = details.TaxTypeId,
                    PriceTypeId = details.PriceTypeId,
                    SkuPrefix = details.SkuPrefix,
                    Status = details.IsActive ? "Inactive" : "Active"  // Toggle using IsActive property
                };
                
                var result = await _productGroupService.UpdateAsync(updateDto);
                
                if (result != null)
                {
                    await LoadProductGroupsAsync();
                }
            }
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error toggling product group status: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    /// <summary>
    /// View product group details (placeholder for future implementation)
    /// </summary>
    [RelayCommand]
    private void ViewProductGroupDetails(ProductGroupDto productGroup)
    {
        if (productGroup == null)
            return;

        new MessageDialog("Product Group Details", $"Product Group Details:\n\nName: {productGroup.Name}\nSKU Prefix: {productGroup.SkuPrefix}\nProducts: {productGroup.ItemCount}", MessageDialog.MessageType.Info).ShowDialog();
    }

    /// <summary>
    /// Show side panel for adding a new product group
    /// </summary>
    [RelayCommand]
    private void AddProductGroup()
    {
        if (SidePanelViewModel != null)
        {
            SidePanelViewModel.PrepareForAdd();
            IsSidePanelVisible = true;
        }
    }

    /// <summary>
    /// Show side panel for editing selected product group
    /// </summary>
    [RelayCommand]
    private async Task EditProductGroupAsync()
    {
        if (SelectedProductGroup == null || SidePanelViewModel == null)
            return;

        try
        {
            // Load full details
            var details = await _productGroupService.GetDetailByIdAsync(SelectedProductGroup.Id);
            if (details != null)
            {
                SidePanelViewModel.PrepareForEdit(details);
                IsSidePanelVisible = true;
            }
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error loading product group details: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    /// <summary>
    /// Delete selected product group
    /// </summary>
    [RelayCommand]
    private async Task DeleteProductGroupAsync()
    {
        if (SelectedProductGroup == null)
        {
            var warningDialog = new MessageDialog(
                "No Selection",
                "Please select a product group to delete.",
                MessageDialog.MessageType.Warning);
            warningDialog.ShowDialog();
            return;
        }

        var confirmDialog = new ConfirmationDialog(
            "Delete Product Group",
            $"Are you sure you want to delete product group '{SelectedProductGroup.Name}'?\n\nThis action cannot be undone.",
            ConfirmationDialog.DialogType.Danger);
        
        var result = confirmDialog.ShowDialog();
        if (result == true)
        {
            try
            {
                var success = await _productGroupService.DeleteAsync(SelectedProductGroup.Id);
                if (success)
                {
                    await LoadProductGroupsAsync();
                    
                    var successDialog = new MessageDialog(
                        "Success",
                        "Product group deleted successfully.",
                        MessageDialog.MessageType.Success);
                    successDialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                var errorDialog = new MessageDialog(
                    "Delete Error",
                    $"An error occurred while deleting the product group:\n\n{ex.Message}",
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
            }
        }
    }

    /// <summary>
    /// Close the side panel
    /// </summary>
    private void CloseSidePanel()
    {
        IsSidePanelVisible = false;
    }

    /// <summary>
    /// Go back to previous view
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        GoBackAction?.Invoke();
    }

    /// <summary>
    /// Command to export product groups to CSV
    /// </summary>
    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"ProductGroups_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsLoading = true;
                StatusMessage = "Exporting product groups...";

                var csv = new StringBuilder();
                csv.AppendLine("Name,NameAr,Description,DescriptionAr,DiscountName,TaxTypeName,PriceTypeName,SkuPrefix,Status");

                foreach (var group in ProductGroups)
                {
                    csv.AppendLine($"\"{group.Name}\"," +
                                 $"\"{group.NameAr ?? ""}\"," +
                                 $"\"{group.Description ?? ""}\"," +
                                 $"\"{group.DescriptionAr ?? ""}\"," +
                                 $"\"{group.DiscountName ?? ""}\"," +
                                 $"\"{group.TaxTypeName ?? ""}\"," +
                                 $"\"{group.PriceTypeName ?? ""}\"," +
                                 $"\"{group.SkuPrefix ?? ""}\"," +
                                 $"\"{group.Status}\"");
                }

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                StatusMessage = $"Exported {ProductGroups.Count} product groups successfully";
                
                var successDialog = new MessageDialog(
                    "Export Successful",
                    $"Successfully exported {ProductGroups.Count} product groups to:\n\n{saveFileDialog.FileName}",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error exporting product groups: {ex.Message}";
            
            var errorDialog = new MessageDialog(
                "Export Error",
                $"An error occurred while exporting product groups:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to import product groups from CSV
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
                    FileName = "ProductGroups_Template.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var templateCsv = new StringBuilder();
                    templateCsv.AppendLine("Name,NameAr,Description,DescriptionAr,DiscountName,TaxTypeName,PriceTypeName,SkuPrefix,Status");
                    templateCsv.AppendLine("Sample Group,مجموعة نموذجية,Sample description,وصف العينة,,,Standard Price,SG,Active");
                    templateCsv.AppendLine("Electronics,الكترونيات,Electronic items group,مجموعة الأجهزة الإلكترونية,,,Retail Price,ELEC,Active");

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
                    StatusMessage = "Importing product groups...";

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

                        var createDto = new CreateProductGroupDto
                        {
                            Name = values[0].Trim('"'),
                            NameAr = string.IsNullOrWhiteSpace(values[1].Trim('"')) ? null : values[1].Trim('"'),
                            Description = string.IsNullOrWhiteSpace(values[2].Trim('"')) ? null : values[2].Trim('"'),
                            DescriptionAr = string.IsNullOrWhiteSpace(values[3].Trim('"')) ? null : values[3].Trim('"'),
                            // Note: DiscountId, TaxTypeId, PriceTypeId would need to be resolved from names
                            SkuPrefix = string.IsNullOrWhiteSpace(values[7].Trim('"')) ? null : values[7].Trim('"'),
                            Status = values[8].Trim('"')
                        };

                        await _productGroupService.CreateAsync(createDto);
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

                    await LoadProductGroupsAsync();

                    var message = $"Import completed:\n\n✓ {successCount} product groups imported successfully";
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
            StatusMessage = $"Error importing product groups: {ex.Message}";
            
            var errorDialog = new MessageDialog(
                "Import Error",
                $"An error occurred while importing product groups:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
        finally
        {
            IsLoading = false;
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

    private void InitializePermissions()
    {
        try
        {
            CanCreateProductGroup = _currentUserService.HasPermission(ScreenNames.PRODUCT_GROUPING, TypeMatrix.CREATE);
            CanEditProductGroup = _currentUserService.HasPermission(ScreenNames.PRODUCT_GROUPING, TypeMatrix.UPDATE);
            CanDeleteProductGroup = _currentUserService.HasPermission(ScreenNames.PRODUCT_GROUPING, TypeMatrix.DELETE);
            CanImportProductGroup = _currentUserService.HasPermission(ScreenNames.PRODUCT_GROUPING, TypeMatrix.IMPORT);
            CanExportProductGroup = _currentUserService.HasPermission(ScreenNames.PRODUCT_GROUPING, TypeMatrix.EXPORT);
        }
        catch (Exception)
        {
            CanCreateProductGroup = false;
            CanEditProductGroup = false;
            CanDeleteProductGroup = false;
            CanImportProductGroup = false;
            CanExportProductGroup = false;
        }
    }

    private void OnDirectionChanged(LayoutDirection newDirection)
    {
        CurrentFlowDirection = newDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft 
            : FlowDirection.LeftToRight;
    }

    private async Task LoadLocalizedTextsAsync()
    {
        try
        {
            PageTitle = await _localizationService.GetTranslationAsync("productgroup.page_title") ?? "Product Groups";
            SearchPlaceholder = await _localizationService.GetTranslationAsync("productgroup.search_placeholder") ?? "Search product groups...";
            AddButtonText = await _localizationService.GetTranslationAsync("productgroup.add_product_group") ?? "Add Product Group";
            RefreshButtonText = await _localizationService.GetTranslationAsync("common.refresh") ?? "Refresh";
            ImportButtonText = await _localizationService.GetTranslationAsync("common.import") ?? "Import";
            ExportButtonText = await _localizationService.GetTranslationAsync("common.export") ?? "Export";
            EditButtonText = await _localizationService.GetTranslationAsync("common.edit") ?? "Edit";
            DeleteButtonText = await _localizationService.GetTranslationAsync("common.delete") ?? "Delete";
            ClearFiltersText = await _localizationService.GetTranslationAsync("common.clear_filters") ?? "Clear Filters";
            ActiveOnlyText = await _localizationService.GetTranslationAsync("productgroup.active_only") ?? "Active Only";
            ShowAllText = await _localizationService.GetTranslationAsync("productgroup.show_all") ?? "Show All";
            ColumnActions = await _localizationService.GetTranslationAsync("productgroup.column.actions") ?? "Actions";
            ColumnActive = await _localizationService.GetTranslationAsync("productgroup.column.active") ?? "Active";
            ColumnStatus = await _localizationService.GetTranslationAsync("productgroup.column.status") ?? "Status";
            ColumnProducts = await _localizationService.GetTranslationAsync("productgroup.column.products") ?? "Products";
            ColumnSkuPrefix = await _localizationService.GetTranslationAsync("productgroup.column.sku_prefix") ?? "SKU Prefix";
            ColumnDescription = await _localizationService.GetTranslationAsync("productgroup.column.description") ?? "Description";
            ColumnArabicName = await _localizationService.GetTranslationAsync("productgroup.column.arabic_name") ?? "Arabic Name";
            ColumnGroupName = await _localizationService.GetTranslationAsync("productgroup.column.group_name") ?? "Group Name";
            EmptyStateTitle = await _localizationService.GetTranslationAsync("productgroup.empty_state_title") ?? "No product groups found";
            EmptyStateMessage = await _localizationService.GetTranslationAsync("productgroup.empty_state_message") ?? "Click 'Add Product Group' to create your first group";
            ActiveText = await _localizationService.GetTranslationAsync("common.active") ?? "Active";
            InactiveText = await _localizationService.GetTranslationAsync("common.inactive") ?? "Inactive";
            LoadingText = await _localizationService.GetTranslationAsync("common.loading") ?? "Loading product groups...";
            StatusTextOf = await _localizationService.GetTranslationAsync("common.of") ?? "of";
            StatusTextProductGroups = await _localizationService.GetTranslationAsync("productgroup.product_groups") ?? "product groups";
            NoActiveGroupsText = await _localizationService.GetTranslationAsync("productgroup.no_active_groups") ?? "No active product groups found";
            NoSearchResultsText = await _localizationService.GetTranslationAsync("productgroup.no_search_results") ?? "No product groups match your search criteria";
            ShowingText = await _localizationService.GetTranslationAsync("common.showing") ?? "Showing";
            
            // Update status message with new translations
            UpdateStatusMessage();
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
}
