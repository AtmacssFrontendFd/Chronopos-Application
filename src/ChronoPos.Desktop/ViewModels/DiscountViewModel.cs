using ChronoPos.Application.Constants;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Views.Dialogs;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using ChronoPos.Desktop.Services;
using InfrastructureServices = ChronoPos.Infrastructure.Services;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Win32;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for comprehensive discount management with full settings integration
/// </summary>
public partial class DiscountViewModel : ObservableObject, IDisposable
{
    #region Fields
    
    private readonly IDiscountService _discountService;
    private readonly IProductService _productService;
    private readonly ICurrentUserService _currentUserService;
    private readonly object? _categoryService; // ICategoryService when available
    private readonly ICustomerService? _customerService;
    private readonly IStoreService _storeService;
    private readonly IActiveCurrencyService _activeCurrencyService;
    private readonly Action? _navigateToAddDiscount;
    private readonly Action<DiscountDto>? _navigateToEditDiscount;
    private readonly Action? _navigateBack;
    
    // Settings services
    private readonly IThemeService _themeService;
    private readonly IZoomService _zoomService;
    private readonly ILocalizationService _localizationService;
    private readonly IColorSchemeService _colorSchemeService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly IFontService _fontService;
    private readonly InfrastructureServices.IDatabaseLocalizationService _databaseLocalizationService;
    
    #endregion

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<DiscountViewDto> discounts = new();

    [ObservableProperty]
    private ObservableCollection<DiscountViewDto> filteredDiscounts = new();

    [ObservableProperty]
    private DiscountViewDto? selectedDiscount;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedSearchType = "Discount Name";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private bool isDiscountFormVisible = false;

    [ObservableProperty]
    private bool isEditMode = false;

    [ObservableProperty]
    private DiscountDto currentDiscount = new();

    // Settings properties
    [ObservableProperty]
    private string _currentTheme = "Light";

    [ObservableProperty]
    private int _currentZoom = 100;

    [ObservableProperty]
    private string _currentLanguage = "English";

    [ObservableProperty]
    private double _currentFontSize = 14;

    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    // Translated UI Properties
    [ObservableProperty]
    private string _pageTitle = "Discount Management";

    [ObservableProperty]
    private string _backButtonText = "Back";

    [ObservableProperty]
    private string _refreshButtonText = "Refresh";

    [ObservableProperty]
    private string _addNewDiscountButtonText = "Add Discount";

    [ObservableProperty]
    private string _searchPlaceholder = "Search discounts...";

    [ObservableProperty]
    private string _searchTypeDiscountName = "Discount Name";

    [ObservableProperty]
    private string _searchTypeDiscountCode = "Discount Code";

    [ObservableProperty]
    private string _showingDiscountsFormat = "Showing {0} discounts";

    [ObservableProperty]
    private string _itemsCountText = "discounts";

    // Table Column Headers
    [ObservableProperty]
    private string _columnDiscountName = "Discount Name";

    [ObservableProperty]
    private string _columnDiscountCode = "Discount Code";

    [ObservableProperty]
    private string _columnDiscountType = "Type";

    [ObservableProperty]
    private string _columnDiscountValue = "Value";

    [ObservableProperty]
    private string _columnStartDate = "Start Date";

    [ObservableProperty]
    private string _columnEndDate = "End Date";

    [ObservableProperty]
    private string _columnStatus = "Status";

    [ObservableProperty]
    private string _columnActions = "Actions";

    // Button Text
    [ObservableProperty]
    private string _importButtonText = "Import";

    [ObservableProperty]
    private string _exportButtonText = "Export";

    [ObservableProperty]
    private string _activeOnlyButtonText = "Active Only";

    [ObservableProperty]
    private string _clearFiltersButtonText = "Clear Filters";

    [ObservableProperty]
    private string _loadingText = "Loading discounts...";

    [ObservableProperty]
    private string _noDataText = "No discounts found";

    [ObservableProperty]
    private string _noDataHintText = "Click 'Add Discount' to create your first discount";

    // Action Tooltips
    [ObservableProperty]
    private string _editDiscountTooltip = "Edit Discount";

    [ObservableProperty]
    private string _deleteDiscountTooltip = "Delete Discount";

    [ObservableProperty]
    private string _activateDiscountTooltip = "Activate Discount";

    [ObservableProperty]
    private string _deactivateDiscountTooltip = "Deactivate Discount";

    // Computed Properties
    public bool HasDiscounts => FilteredDiscounts?.Count > 0;
    
    public int TotalDiscounts => Discounts?.Count ?? 0;

    // Sidepanel Properties
    [ObservableProperty]
    private bool _isSidePanelVisible = false;

    [ObservableProperty]
    private DiscountSidePanelViewModel? _sidePanelViewModel;

    [ObservableProperty]
    private bool canCreateDiscount = false;

    [ObservableProperty]
    private bool canEditDiscount = false;

    [ObservableProperty]
    private bool canDeleteDiscount = false;

    [ObservableProperty]
    private bool canImportDiscount = false;

    [ObservableProperty]
    private bool canExportDiscount = false;

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor with all required services
    /// </summary>
    public DiscountViewModel(
        IDiscountService discountService,
        IProductService productService,
        IStoreService storeService,
        ICurrentUserService currentUserService,
        ICustomerService? customerService,
        IActiveCurrencyService activeCurrencyService,
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        InfrastructureServices.IDatabaseLocalizationService databaseLocalizationService,
        Action? navigateToAddDiscount = null,
        Action<DiscountDto>? navigateToEditDiscount = null,
        Action? navigateBack = null)
    {
        _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _customerService = customerService;
        _activeCurrencyService = activeCurrencyService ?? throw new ArgumentNullException(nameof(activeCurrencyService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));
        
        _navigateToAddDiscount = navigateToAddDiscount;
        _navigateToEditDiscount = navigateToEditDiscount;
        _navigateBack = navigateBack;

        // Initialize permissions
        InitializePermissions();

        // Initialize current settings
        InitializeCurrentSettings();
        
        // Subscribe to property changes
        PropertyChanged += OnPropertyChanged;
        
        // Subscribe to layout direction changes
        _layoutDirectionService.DirectionChanged += OnDirectionChanged;
        
        // Subscribe to language changes
        _databaseLocalizationService.LanguageChanged += OnLanguageChanged;
        
        // Load data
        _ = Task.Run(LoadDiscountsAsync);
        
        // Load translations
        _ = Task.Run(LoadTranslationsAsync);
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task RefreshData()
    {
        IsLoading = true;
        StatusMessage = "Refreshing discount data...";
        
        try
        {
            await LoadDiscountsAsync();
            StatusMessage = "Discount data refreshed successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error refreshing data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NavigateBack()
    {
        _navigateBack?.Invoke();
    }

    [RelayCommand]
    private void AddDiscount()
    {
        ShowSidePanelForNewDiscount();
    }

    [RelayCommand]
    private async Task EditDiscount(DiscountDto? discount)
    {
        if (discount != null)
        {
            await ShowSidePanelForEditDiscount(discount);
        }
    }

    [RelayCommand]
    private async Task DeleteDiscount(DiscountDto? discount)
    {
        if (discount == null) return;

        var confirmDialog = new ConfirmationDialog(
            "Confirm Delete",
            $"Are you sure you want to delete the discount '{discount.DiscountName}'?\n\nThis action cannot be undone.",
            ConfirmationDialog.DialogType.Danger);

        if (confirmDialog.ShowDialog() == true)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Deleting discount...";
                
                await _discountService.DeleteAsync(discount.Id);
                await LoadDiscountsAsync();
                
                StatusMessage = "Discount deleted successfully";
                
                var successDialog = new MessageDialog(
                    "Success",
                    "Discount deleted successfully!",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting discount: {ex.Message}";
                
                var errorMessage = $"Failed to delete discount.\n\nError: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nInner: {ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                        errorMessage += $"\n\nDetails: {ex.InnerException.InnerException.Message}";
                }
                
                var errorDialog = new MessageDialog(
                    "Delete Error",
                    errorMessage,
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    [RelayCommand]
    private async Task ToggleDiscountStatus(DiscountDto? discount)
    {
        if (discount == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = discount.IsActive ? "Deactivating discount..." : "Activating discount...";
            
            // Get the full discount with selections before updating
            var fullDiscount = await _discountService.GetByIdAsync(discount.Id);
            if (fullDiscount == null)
            {
                StatusMessage = "Error: Discount not found";
                return;
            }
            
            // Update the status
            fullDiscount.IsActive = !fullDiscount.IsActive;
            
            await _discountService.UpdateAsync(fullDiscount.Id, new UpdateDiscountDto
            {
                DiscountName = fullDiscount.DiscountName,
                DiscountNameAr = fullDiscount.DiscountNameAr,
                DiscountDescription = fullDiscount.DiscountDescription,
                DiscountCode = fullDiscount.DiscountCode,
                DiscountType = fullDiscount.DiscountType,
                DiscountValue = fullDiscount.DiscountValue,
                MaxDiscountAmount = fullDiscount.MaxDiscountAmount,
                MinPurchaseAmount = fullDiscount.MinPurchaseAmount,
                StartDate = fullDiscount.StartDate,
                EndDate = fullDiscount.EndDate,
                ApplicableOn = fullDiscount.ApplicableOn,
                SelectedProductIds = fullDiscount.SelectedProductIds ?? new List<int>(),
                SelectedCategoryIds = fullDiscount.SelectedCategoryIds ?? new List<int>(),
                Priority = fullDiscount.Priority,
                IsStackable = fullDiscount.IsStackable,
                IsActive = fullDiscount.IsActive,
                StoreId = fullDiscount.StoreId,
                CurrencyCode = fullDiscount.CurrencyCode
            });
            
            // Update the local discount object for UI refresh
            discount.IsActive = fullDiscount.IsActive;
            
            await LoadDiscountsAsync();
            
            StatusMessage = fullDiscount.IsActive ? "Discount activated successfully" : "Discount deactivated successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating discount status: {ex.Message}";
            new MessageDialog("Error", $"Error updating discount status: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void FilterActive()
    {
        var activeDiscounts = Discounts.Where(d => d.IsActive);
        FilteredDiscounts.Clear();
        foreach (var discount in activeDiscounts)
        {
            FilteredDiscounts.Add(discount);
        }
        StatusMessage = $"Showing {FilteredDiscounts.Count} active discounts";
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        FilterDiscounts();
        StatusMessage = $"Showing all {FilteredDiscounts.Count} discounts";
    }

    [RelayCommand]
    private void Refresh()
    {
        _ = Task.Run(RefreshData);
    }

    [RelayCommand]
    private void Back()
    {
        NavigateBack();
    }

    [RelayCommand]
    private void ToggleActive(DiscountDto? discount)
    {
        _ = Task.Run(() => ToggleDiscountStatus(discount));
    }

    [RelayCommand]
    private void PrintCoupon(DiscountDto? discount)
{
    if (discount == null) return;

    try
    {
        // Create a styled FlowDocument
        var document = new FlowDocument
        {
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
            FontSize = 14,
            PagePadding = new Thickness(40),
            TextAlignment = TextAlignment.Center
        };

        // Border container
        var border = new Border
        {
            BorderBrush = System.Windows.Media.Brushes.Black,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(20),
            Margin = new Thickness(10)
        };

        // Stack content inside
        var stackPanel = new StackPanel();

        // Title
        var title = new Paragraph(new Run("DISCOUNT COUPON"))
        {
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };
        stackPanel.Children.Add(new TextBlock
        {
            Text = "DISCOUNT COUPON",
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.DarkBlue,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        });

        // Details
        stackPanel.Children.Add(new TextBlock { Text = $" {discount.DiscountName}", FontSize = 16, Margin = new Thickness(0, 5, 0, 5) });
        stackPanel.Children.Add(new TextBlock { Text = $"Code: {discount.DiscountCode}", FontSize = 16, Margin = new Thickness(0, 5, 0, 5) });
        stackPanel.Children.Add(new TextBlock { Text = $"Value: {discount.FormattedDiscountValue}", FontSize = 16, Margin = new Thickness(0, 5, 0, 5) });
        stackPanel.Children.Add(new TextBlock { Text = $"{discount.DiscountDescription}", FontSize = 14, FontStyle = FontStyles.Italic, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 10) });
        stackPanel.Children.Add(new TextBlock { Text = $"Valid: {discount.StartDate:MM/dd/yyyy} - {discount.EndDate:MM/dd/yyyy}", FontSize = 14, Margin = new Thickness(0, 5, 0, 15) });
        stackPanel.Children.Add(new TextBlock { Text = "Terms and conditions apply", FontSize = 12, Foreground = System.Windows.Media.Brushes.Gray });

        border.Child = stackPanel;

        // Convert Border into BlockUIContainer
        var container = new BlockUIContainer(border);
        document.Blocks.Add(container);

        // Print
        var printDialog = new System.Windows.Controls.PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
            printDialog.PrintDocument(paginator, $"Discount Coupon - {discount.DiscountCode}");
            StatusMessage = $"Coupon printed for {discount.DiscountName}";
        }
    }
    catch (Exception ex)
    {
        StatusMessage = $"Error printing coupon: {ex.Message}";
        new MessageDialog("Print Error", $"Error printing coupon: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
    }
}

    /// <summary>
    /// Command to export discounts to CSV
    /// </summary>
    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"Discounts_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsLoading = true;
                StatusMessage = "Exporting discounts...";

                var csv = new StringBuilder();
                // Export template without Id - 12 fields matching the form
                csv.AppendLine("DiscountName,DiscountCode,DiscountType,DiscountValue,StartDate,EndDate,IsActive,MinPurchaseAmount,MaxDiscountAmount,Priority,IsStackable,Description");

                foreach (var discount in Discounts)
                {
                    var isActiveDisplay = discount.IsActive ? "Active" : "Inactive";
                    var isStackableDisplay = discount.IsStackable ? "Yes" : "No";
                    
                    csv.AppendLine($"\"{discount.DiscountName}\"," +
                                 $"\"{discount.DiscountCode}\"," +
                                 $"\"{discount.DiscountType}\"," +
                                 $"{discount.DiscountValue}," +
                                 $"{discount.StartDate:yyyy-MM-dd}," +
                                 $"{discount.EndDate:yyyy-MM-dd}," +
                                 $"\"{isActiveDisplay}\"," +
                                 $"{discount.MinPurchaseAmount ?? 0}," +
                                 $"{discount.MaxDiscountAmount ?? 0}," +
                                 $"{discount.Priority}," +
                                 $"\"{isStackableDisplay}\"," +
                                 $"\"{discount.DiscountDescription ?? ""}\"");
                }

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                StatusMessage = $"Exported {Discounts.Count} discounts successfully";
                
                var successDialog = new MessageDialog(
                    "Export Successful",
                    $"Successfully exported {Discounts.Count} discount(s) to:\n{saveFileDialog.FileName}",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error exporting discounts: {ex.Message}";
            
            var errorMessage = $"Failed to export discounts.\n\nError: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nInner: {ex.InnerException.Message}";
                if (ex.InnerException.InnerException != null)
                    errorMessage += $"\n\nDetails: {ex.InnerException.InnerException.Message}";
            }
            
            var errorDialog = new MessageDialog(
                "Export Error",
                errorMessage,
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to import discounts from CSV
    /// </summary>
    [RelayCommand]
    private async Task ImportAsync()
    {
        var importDialog = new ImportDialog();
        importDialog.ShowDialog();
        
        if (importDialog.SelectedAction == ImportDialog.ImportAction.None)
            return;

        if (importDialog.SelectedAction == ImportDialog.ImportAction.DownloadTemplate)
        {
            // Download Template
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"Discounts_Template_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var templateCsv = new StringBuilder();
                    templateCsv.AppendLine("DiscountName,DiscountCode,DiscountType,DiscountValue,StartDate,EndDate,IsActive,MinPurchaseAmount,MaxDiscountAmount,Priority,IsStackable,Description");
                    templateCsv.AppendLine("Sample Discount,SAVE10,Percentage,10,2024-01-01,2024-12-31,Active,0,0,0,No,Sample 10% discount");
                    templateCsv.AppendLine("Holiday Sale,HOLIDAY25,Percentage,25,2024-12-01,2024-12-31,Active,100,50,1,Yes,25% off for holidays");
                    templateCsv.AppendLine("Fixed Amount Off,FIXED20,Fixed,20,2024-01-01,2024-12-31,Active,50,0,0,No,20 dollars off");

                    await File.WriteAllTextAsync(saveFileDialog.FileName, templateCsv.ToString());
                    
                    var successDialog = new MessageDialog(
                        "Template Downloaded",
                        $"Template downloaded successfully to:\n{saveFileDialog.FileName}\n\nPlease fill in your data and use the Import function again to upload it.",
                        MessageDialog.MessageType.Success);
                    successDialog.ShowDialog();
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Failed to download template.\n\nError: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nInner: {ex.InnerException.Message}";
                        if (ex.InnerException.InnerException != null)
                            errorMessage += $"\n\nDetails: {ex.InnerException.InnerException.Message}";
                    }
                    
                    var errorDialog = new MessageDialog(
                        "Download Error",
                        errorMessage,
                        MessageDialog.MessageType.Error);
                    errorDialog.ShowDialog();
                }
            }
            return;
        }

        if (importDialog.SelectedAction == ImportDialog.ImportAction.UploadFile)
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
                StatusMessage = "Importing discounts...";
                
                try
                {
                    // Reload discounts to ensure we have the latest data for duplicate checking
                    await LoadDiscountsAsync();
                    
                    var lines = await File.ReadAllLinesAsync(openFileDialog.FileName);
                    if (lines.Length < 2)
                    {
                        var warningDialog = new MessageDialog(
                            "Import Warning",
                            "The CSV file is empty or contains only headers. Please add discount data and try again.",
                            MessageDialog.MessageType.Warning);
                        warningDialog.ShowDialog();
                        return;
                    }

                    // Validation phase - check all rows before importing
                    var validationErrors = new StringBuilder();
                    var validDiscounts = new List<CreateDiscountDto>();
                    var existingCodes = Discounts.Select(d => d.DiscountCode.ToLower()).ToHashSet();
                    var newCodes = new HashSet<string>();

                    // Skip header row
                    for (int i = 1; i < lines.Length; i++)
                    {
                        try
                        {
                            var line = lines[i];
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var values = ParseCsvLine(line);
                            if (values.Length < 12)
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Invalid format (expected 12 columns: DiscountName,DiscountCode,DiscountType,DiscountValue,StartDate,EndDate,IsActive,MinPurchaseAmount,MaxDiscountAmount,Priority,IsStackable,Description)");
                                continue;
                            }

                            var name = values[0].Trim('"').Trim();
                            var code = values[1].Trim('"').Trim();
                            var typeStr = values[2].Trim('"').Trim();
                            var valueStr = values[3].Trim();
                            var startDateStr = values[4].Trim();
                            var endDateStr = values[5].Trim();
                            var isActiveStr = values[6].Trim('"').Trim();
                            var minPurchaseStr = values[7].Trim();
                            var maxDiscountStr = values[8].Trim();
                            var priorityStr = values[9].Trim();
                            var isStackableStr = values[10].Trim('"').Trim();
                            var description = values[11].Trim('"').Trim();

                            // Validate required fields
                            if (string.IsNullOrWhiteSpace(name))
                            {
                                validationErrors.AppendLine($"Line {i + 1}: DiscountName is required");
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(code))
                            {
                                validationErrors.AppendLine($"Line {i + 1}: DiscountCode is required");
                                continue;
                            }

                            // Check for duplicate codes in existing data
                            if (existingCodes.Contains(code.ToLower()))
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Discount code '{code}' already exists");
                                continue;
                            }

                            // Check for duplicate codes within the import file
                            if (newCodes.Contains(code.ToLower()))
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Duplicate discount code '{code}' in import file");
                                continue;
                            }

                            // Validate DiscountType
                            if (!Enum.TryParse<ChronoPos.Domain.Enums.DiscountType>(typeStr, true, out var discountType))
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Invalid DiscountType '{typeStr}'. Must be 'Percentage' or 'Fixed'");
                                continue;
                            }

                            // Validate DiscountValue
                            if (!decimal.TryParse(valueStr, out var discountValue) || discountValue <= 0)
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Invalid DiscountValue '{valueStr}'. Must be greater than 0");
                                continue;
                            }

                            // Validate dates
                            if (!DateTime.TryParse(startDateStr, out var startDate))
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Invalid StartDate '{startDateStr}'. Use format yyyy-MM-dd");
                                continue;
                            }

                            if (!DateTime.TryParse(endDateStr, out var endDate))
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Invalid EndDate '{endDateStr}'. Use format yyyy-MM-dd");
                                continue;
                            }

                            if (endDate < startDate)
                            {
                                validationErrors.AppendLine($"Line {i + 1}: EndDate must be after StartDate");
                                continue;
                            }

                            // Validate IsActive format
                            bool isActive;
                            if (isActiveStr.Equals("Active", StringComparison.OrdinalIgnoreCase) || 
                                isActiveStr.Equals("True", StringComparison.OrdinalIgnoreCase))
                                isActive = true;
                            else if (isActiveStr.Equals("Inactive", StringComparison.OrdinalIgnoreCase) || 
                                     isActiveStr.Equals("False", StringComparison.OrdinalIgnoreCase))
                                isActive = false;
                            else
                            {
                                validationErrors.AppendLine($"Line {i + 1}: IsActive must be 'Active', 'Inactive', 'True', or 'False', found '{isActiveStr}'");
                                continue;
                            }

                            // Validate IsStackable format
                            bool isStackable;
                            if (isStackableStr.Equals("Yes", StringComparison.OrdinalIgnoreCase) || 
                                isStackableStr.Equals("True", StringComparison.OrdinalIgnoreCase))
                                isStackable = true;
                            else if (isStackableStr.Equals("No", StringComparison.OrdinalIgnoreCase) || 
                                     isStackableStr.Equals("False", StringComparison.OrdinalIgnoreCase))
                                isStackable = false;
                            else
                            {
                                validationErrors.AppendLine($"Line {i + 1}: IsStackable must be 'Yes', 'No', 'True', or 'False', found '{isStackableStr}'");
                                continue;
                            }

                            // Parse numeric values
                            decimal? minPurchase = null;
                            if (!string.IsNullOrWhiteSpace(minPurchaseStr) && minPurchaseStr != "0")
                            {
                                if (!decimal.TryParse(minPurchaseStr, out var tempValue) || tempValue < 0)
                                {
                                    validationErrors.AppendLine($"Line {i + 1}: Invalid MinPurchaseAmount '{minPurchaseStr}'");
                                    continue;
                                }
                                minPurchase = tempValue;
                            }

                            decimal? maxDiscount = null;
                            if (!string.IsNullOrWhiteSpace(maxDiscountStr) && maxDiscountStr != "0")
                            {
                                if (!decimal.TryParse(maxDiscountStr, out var tempValue) || tempValue < 0)
                                {
                                    validationErrors.AppendLine($"Line {i + 1}: Invalid MaxDiscountAmount '{maxDiscountStr}'");
                                    continue;
                                }
                                maxDiscount = tempValue;
                            }

                            if (!int.TryParse(priorityStr, out var priority) || priority < 0)
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Invalid Priority '{priorityStr}'. Must be 0 or greater");
                                continue;
                            }

                            newCodes.Add(code.ToLower());
                            validDiscounts.Add(new CreateDiscountDto
                            {
                                DiscountName = name,
                                DiscountCode = code,
                                DiscountType = discountType,
                                DiscountValue = discountValue,
                                StartDate = startDate,
                                EndDate = endDate,
                                IsActive = isActive,
                                MinPurchaseAmount = minPurchase,
                                MaxDiscountAmount = maxDiscount,
                                Priority = priority,
                                IsStackable = isStackable,
                                DiscountDescription = string.IsNullOrWhiteSpace(description) ? null : description,
                                ApplicableOn = ChronoPos.Domain.Enums.DiscountApplicableOn.Shop,
                                CreatedBy = 1 // TODO: Get from current user
                            });
                        }
                        catch (Exception ex)
                        {
                            validationErrors.AppendLine($"Line {i + 1}: Validation error - {ex.Message}");
                        }
                    }

                    // If validation errors exist, show them and abort
                    if (validationErrors.Length > 0)
                    {
                        var errorDialog = new MessageDialog(
                            "Validation Errors",
                            $"Found {validationErrors.ToString().Split('\n').Length - 1} validation error(s). Please fix these issues and try again:\n\n{validationErrors}",
                            MessageDialog.MessageType.Error);
                        errorDialog.ShowDialog();
                        return;
                    }

                    // Import phase - all validations passed
                    int successCount = 0;
                    int errorCount = 0;
                    var importErrors = new StringBuilder();

                    foreach (var discount in validDiscounts)
                    {
                        try
                        {
                            await _discountService.CreateAsync(discount);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            var errorMsg = $"Discount '{discount.DiscountName}': {ex.Message}";
                            if (ex.InnerException != null)
                            {
                                errorMsg += $" | Inner: {ex.InnerException.Message}";
                                if (ex.InnerException.InnerException != null)
                                    errorMsg += $" | Details: {ex.InnerException.InnerException.Message}";
                            }
                            importErrors.AppendLine(errorMsg);
                        }
                    }

                    await RefreshData();

                    // Show results
                    if (errorCount == 0)
                    {
                        var successDialog = new MessageDialog(
                            "Import Successful",
                            $"Successfully imported {successCount} discount(s)!",
                            MessageDialog.MessageType.Success);
                        successDialog.ShowDialog();
                    }
                    else
                    {
                        var message = $"Import completed with some errors:\n\nSuccessfully imported: {successCount}\nFailed: {errorCount}\n\nErrors:\n{importErrors}";
                        var resultDialog = new MessageDialog(
                            "Import Completed with Errors",
                            message,
                            MessageDialog.MessageType.Warning);
                        resultDialog.ShowDialog();
                    }
                    
                    StatusMessage = $"Import completed: {successCount} successful, {errorCount} errors";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error importing discounts: {ex.Message}";
                    
                    var errorMessage = $"Failed to import discounts.\n\nError: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nInner: {ex.InnerException.Message}";
                        if (ex.InnerException.InnerException != null)
                            errorMessage += $"\n\nDetails: {ex.InnerException.InnerException.Message}";
                    }
                    
                    var errorDialog = new MessageDialog(
                        "Import Error",
                        errorMessage,
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
    /// Initialize current settings from services
    /// </summary>
    private void InitializeCurrentSettings()
    {
        CurrentTheme = _themeService.CurrentTheme.ToString();
        CurrentZoom = (int)_zoomService.CurrentZoomPercentage;
        CurrentLanguage = _localizationService.CurrentLanguage.ToString();
        CurrentFontSize = GetCurrentFontSize();
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    /// <summary>
    /// Get current font size based on zoom and font settings
    /// </summary>
    private double GetCurrentFontSize()
    {
        var baseSize = _fontService.CurrentFontSize switch
        {
            FontSize.Small => 12,
            FontSize.Medium => 14,
            FontSize.Large => 16,
            _ => 14
        };
        
        return baseSize * (_zoomService.CurrentZoomPercentage / 100.0);
    }

    /// <summary>
    /// Load all discounts from the service
    /// </summary>
    private async Task LoadDiscountsAsync()
    {
        try
        {
            IsLoading = true;
            var discountList = await _discountService.GetAllAsync();
            
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Discounts.Clear();
                foreach (var discount in discountList)
                {
                    Discounts.Add(new DiscountViewDto(discount, _activeCurrencyService));
                }
                
                FilterDiscounts();
                OnPropertyChanged(nameof(TotalDiscounts));
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading discounts: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Filter discounts based on search text and type
    /// </summary>
    private void FilterDiscounts()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredDiscounts.Clear();
            foreach (var discount in Discounts)
            {
                FilteredDiscounts.Add(discount);
            }
        }
        else
        {
            var filtered = SelectedSearchType switch
            {
                "Discount Name" => Discounts.Where(d => d.DiscountName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)),
                "Discount Code" => Discounts.Where(d => d.DiscountCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase)),
                _ => Discounts.Where(d => d.DiscountName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            };

            FilteredDiscounts.Clear();
            foreach (var discount in filtered)
            {
                FilteredDiscounts.Add(discount);
            }
        }
        
        // Notify computed properties
        OnPropertyChanged(nameof(HasDiscounts));
        OnPropertyChanged(nameof(TotalDiscounts));
    }

    /// <summary>
    /// Show sidepanel for creating a new discount
    /// </summary>
    private void ShowSidePanelForNewDiscount()
    {
        try
        {
            SidePanelViewModel?.Dispose();
            
            // Create the sidepanel ViewModel
            SidePanelViewModel = new DiscountSidePanelViewModel(
                _discountService,
                _productService,
                _categoryService,
                _customerService,
                _storeService,
                _themeService,
                _localizationService,
                _layoutDirectionService,
                _databaseLocalizationService,
                onSaved: OnSidePanelSaved,
                onCancelled: OnSidePanelCancelled
            );
            
            IsSidePanelVisible = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening discount form: {ex.Message}";
        }
    }

    /// <summary>
    /// Show sidepanel for editing an existing discount
    /// </summary>
    private async Task ShowSidePanelForEditDiscount(DiscountDto discount)
    {
        try
        {
            SidePanelViewModel?.Dispose();
            
            // Get the full discount data with product/category selections
            var fullDiscountData = await _discountService.GetByIdAsync(discount.Id);
            if (fullDiscountData == null)
            {
                new MessageDialog("Error", $"Error: Could not load discount data for ID {discount.Id}", MessageDialog.MessageType.Error).ShowDialog();
                return;
            }
            
            // Create the sidepanel ViewModel for editing
            SidePanelViewModel = new DiscountSidePanelViewModel(
                _discountService,
                _productService,
                _categoryService,
                _customerService,
                _storeService,
                _themeService,
                _localizationService,
                _layoutDirectionService,
                _databaseLocalizationService,
                fullDiscountData,
                onSaved: OnSidePanelSaved,
                onCancelled: OnSidePanelCancelled
            );
            
            IsSidePanelVisible = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening discount form: {ex.Message}";
        }
    }

    /// <summary>
    /// Handle sidepanel saved event
    /// </summary>
    private async void OnSidePanelSaved(bool success)
    {
        if (success)
        {
            await LoadDiscountsAsync();
        }
        
        IsSidePanelVisible = false;
        SidePanelViewModel?.Dispose();
        SidePanelViewModel = null;
    }

    /// <summary>
    /// Handle sidepanel cancelled event
    /// </summary>
    private void OnSidePanelCancelled()
    {
        IsSidePanelVisible = false;
        SidePanelViewModel?.Dispose();
        SidePanelViewModel = null;
    }

    /// <summary>
    /// Load translations from database
    /// </summary>
    private async Task LoadTranslationsAsync()
    {
        try
        {
            PageTitle = await _databaseLocalizationService.GetTranslationAsync("discount.page_title") ?? "Discount Management";
            BackButtonText = await _databaseLocalizationService.GetTranslationAsync("common.back") ?? "Back";
            RefreshButtonText = await _databaseLocalizationService.GetTranslationAsync("common.refresh") ?? "Refresh";
            AddNewDiscountButtonText = await _databaseLocalizationService.GetTranslationAsync("discount.add_new") ?? "Add Discount";
            SearchPlaceholder = await _databaseLocalizationService.GetTranslationAsync("discount.search_placeholder") ?? "Search discounts...";
            
            // Button text
            ImportButtonText = await _databaseLocalizationService.GetTranslationAsync("common.import") ?? "Import";
            ExportButtonText = await _databaseLocalizationService.GetTranslationAsync("common.export") ?? "Export";
            ActiveOnlyButtonText = await _databaseLocalizationService.GetTranslationAsync("discount.active_only") ?? "Active Only";
            ClearFiltersButtonText = await _databaseLocalizationService.GetTranslationAsync("discount.clear_filters") ?? "Clear Filters";
            LoadingText = await _databaseLocalizationService.GetTranslationAsync("discount.loading") ?? "Loading discounts...";
            NoDataText = await _databaseLocalizationService.GetTranslationAsync("discount.no_data") ?? "No discounts found";
            NoDataHintText = await _databaseLocalizationService.GetTranslationAsync("discount.no_data_hint") ?? "Click 'Add Discount' to create your first discount";
            
            // Column headers
            ColumnDiscountName = await _databaseLocalizationService.GetTranslationAsync("discount.column.name") ?? "Discount Name";
            ColumnDiscountCode = await _databaseLocalizationService.GetTranslationAsync("discount.column.code") ?? "Discount Code";
            ColumnDiscountType = await _databaseLocalizationService.GetTranslationAsync("discount.column.type") ?? "Type";
            ColumnDiscountValue = await _databaseLocalizationService.GetTranslationAsync("discount.column.value") ?? "Value";
            ColumnStartDate = await _databaseLocalizationService.GetTranslationAsync("discount.column.start_date") ?? "Start Date";
            ColumnEndDate = await _databaseLocalizationService.GetTranslationAsync("discount.column.end_date") ?? "End Date";
            ColumnStatus = await _databaseLocalizationService.GetTranslationAsync("discount.column.status") ?? "Status";
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

    /// <summary>
    /// Handle property changes for filtering
    /// </summary>
    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchText) || e.PropertyName == nameof(SelectedSearchType))
        {
            FilterDiscounts();
        }
    }

    private void InitializePermissions()
    {
        try
        {
            CanCreateDiscount = _currentUserService.HasPermission(ScreenNames.DISCOUNTS, TypeMatrix.CREATE);
            CanEditDiscount = _currentUserService.HasPermission(ScreenNames.DISCOUNTS, TypeMatrix.UPDATE);
            CanDeleteDiscount = _currentUserService.HasPermission(ScreenNames.DISCOUNTS, TypeMatrix.DELETE);
            CanImportDiscount = _currentUserService.HasPermission(ScreenNames.DISCOUNTS, TypeMatrix.IMPORT);
            CanExportDiscount = _currentUserService.HasPermission(ScreenNames.DISCOUNTS, TypeMatrix.EXPORT);
        }
        catch (Exception)
        {
            CanCreateDiscount = false;
            CanEditDiscount = false;
            CanDeleteDiscount = false;
            CanImportDiscount = false;
            CanExportDiscount = false;
        }
    }

    #endregion

    #region Dispose

    public void Dispose()
    {
        PropertyChanged -= OnPropertyChanged;
        _layoutDirectionService.DirectionChanged -= OnDirectionChanged;
        _databaseLocalizationService.LanguageChanged -= OnLanguageChanged;
        GC.SuppressFinalize(this);
    }

    #endregion
}

/// <summary>
/// Display wrapper for DiscountDto with dynamic currency formatting
/// </summary>
public class DiscountViewDto : DiscountDto
{
    private readonly IActiveCurrencyService? _currencyService;

    public DiscountViewDto(DiscountDto discount, IActiveCurrencyService currencyService)
    {
        _currencyService = currencyService;
        
        // Copy all properties from DiscountDto
        Id = discount.Id;
        DiscountName = discount.DiscountName;
        DiscountNameAr = discount.DiscountNameAr;
        DiscountDescription = discount.DiscountDescription;
        DiscountCode = discount.DiscountCode;
        DiscountType = discount.DiscountType;
        DiscountValue = discount.DiscountValue;
        MaxDiscountAmount = discount.MaxDiscountAmount;
        MinPurchaseAmount = discount.MinPurchaseAmount;
        StartDate = discount.StartDate;
        EndDate = discount.EndDate;
        ApplicableOn = discount.ApplicableOn;
        SelectedProductIds = discount.SelectedProductIds;
        SelectedCategoryIds = discount.SelectedCategoryIds;
        SelectedCustomerIds = discount.SelectedCustomerIds;
        SelectedProductNames = discount.SelectedProductNames;
        SelectedCategoryNames = discount.SelectedCategoryNames;
        SelectedCustomerNames = discount.SelectedCustomerNames;
        Priority = discount.Priority;
        IsStackable = discount.IsStackable;
        IsActive = discount.IsActive;
        CreatedBy = discount.CreatedBy;
        CreatedByName = discount.CreatedByName;
        CreatedAt = discount.CreatedAt;
        UpdatedBy = discount.UpdatedBy;
        UpdatedByName = discount.UpdatedByName;
        UpdatedAt = discount.UpdatedAt;
        DeletedAt = discount.DeletedAt;
        DeletedBy = discount.DeletedBy;
        DeletedByName = discount.DeletedByName;
        StoreId = discount.StoreId;
        StoreName = discount.StoreName;
        CurrencyCode = discount.CurrencyCode;
    }

    public new string FormattedDiscountValue =>
        DiscountType == Domain.Enums.DiscountType.Percentage 
            ? $"{DiscountValue}%" 
            : _currencyService != null 
                ? $"{_currencyService.FormatPrice(DiscountValue)}"
                : $"{CurrencyCode} {DiscountValue:F2}";
}