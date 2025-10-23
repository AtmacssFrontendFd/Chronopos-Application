using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopFileLogger = ChronoPos.Desktop.Services.FileLogger;
using ChronoPos.Application.Logging;
using ChronoPos.Desktop.Models;
using ChronoPos.Desktop.Services;
using InfrastructureServices = ChronoPos.Infrastructure.Services;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Enums;
using System.Linq;
using ChronoPos.Application.Constants;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Stock Management page
/// </summary>
public partial class StockManagementViewModel : ObservableObject
{
    #region Fields

    private readonly IThemeService _themeService;
    private readonly IZoomService _zoomService;
    private readonly ILocalizationService _localizationService;
    private readonly IColorSchemeService _colorSchemeService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly IFontService _fontService;
    private readonly InfrastructureServices.IDatabaseLocalizationService _databaseLocalizationService;
    private readonly IProductBatchService? _productBatchService;
    private readonly IGoodsReceivedService? _goodsReceivedService;
    private readonly ISupplierService? _supplierService;
    private readonly ICurrentUserService _currentUserService;
    private readonly Action? _navigateToAddGrn;
    private readonly Action<long>? NavigateToEditGrn;
    private readonly Action? _navigateToAddStockTransfer;
    private readonly Action<int>? _navigateToEditStockTransfer;
    private readonly Action? _navigateToAddGoodsReturn;
    private readonly Action<int>? _navigateToEditGoodsReturn;
    private readonly Action? _navigateToAddGoodsReplace;
    private readonly Action<int>? _navigateToEditGoodsReplace;

    #endregion

    #region Observable Properties

    /// <summary>
    /// Collection of stock management modules
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<StockModuleInfo> _modules = new();

    /// <summary>
    /// Current theme name
    /// </summary>
    [ObservableProperty]
    private string _currentTheme = "Light";

    /// <summary>
    /// Current zoom level
    /// </summary>
    [ObservableProperty]
    private int _currentZoom = 100;

    /// <summary>
    /// Current language
    /// </summary>
    [ObservableProperty]
    private string _currentLanguage = "English";

    /// <summary>
    /// Current color scheme
    /// </summary>
    [ObservableProperty]
    private string _currentColorScheme = "Blue";

    /// <summary>
    /// Current flow direction for RTL/LTR support
    /// </summary>
    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    /// <summary>
    /// Current font family
    /// </summary>
    [ObservableProperty]
    private string _currentFontFamily = "Segoe UI";

    /// <summary>
    /// Current font size
    /// </summary>
    [ObservableProperty]
    private double _currentFontSize = 14;

    // Module Selection Properties
    [ObservableProperty]
    private bool _isStockAdjustmentSelected = true;

    [ObservableProperty]
    private bool _isStockTransferSelected = false;

    [ObservableProperty]
    private bool _isGoodsReceivedSelected = false;

    [ObservableProperty]
    private bool _isGoodsReturnSelected = false;

    [ObservableProperty]
    private bool _isGoodsReplaceSelected = false;

    [ObservableProperty]
    private bool _noModuleSelected = false;

    // Stock Transfer Properties
    [ObservableProperty]
    private ObservableCollection<StockTransferDto> _stockTransfers = new();

    [ObservableProperty]
    private StockTransferDto? _selectedTransfer;

    // Goods Return Properties
    [ObservableProperty]
    private ObservableCollection<GoodsReturnDto> _goodsReturns = new();

    [ObservableProperty]
    private GoodsReturnDto? _selectedGoodsReturn;

    // Goods Replace Properties
    [ObservableProperty]
    private ObservableCollection<GoodsReplaceDto> _goodsReplaces = new();

    [ObservableProperty]
    private GoodsReplaceDto? _selectedGoodsReplace;



    // Stock Adjustment Properties
    [ObservableProperty]
    private ObservableCollection<StockAdjustmentDto> _stockAdjustments = new();

    [ObservableProperty]
    private StockAdjustmentDto? _selectedAdjustment;

    // Goods Received Properties
    [ObservableProperty]
    private ObservableCollection<GoodsReceivedDto> _goodsReceivedNotes = new();

    [ObservableProperty]
    private GoodsReceivedDto? _selectedGrn;

    [ObservableProperty]
    private string _grnSearchTerm = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _grnStatusFilters = new() { "All", "Draft", "Posted", "Cancelled" };

    [ObservableProperty]
    private string _selectedGrnStatusFilter = "All";

    // Stock Transfer Status Filter
    [ObservableProperty]
    private ObservableCollection<string> _statusFilters = new() { "All", "Draft", "Pending", "In-Transit", "Completed", "Cancelled" };

    [ObservableProperty]
    private string _selectedStatus = "All";

    [ObservableProperty]
    private ObservableCollection<SupplierDto> _grnSupplierFilters = new();

    [ObservableProperty]
    private long? _selectedGrnSupplierId;

    [ObservableProperty]
    private bool _hasNoGrns = false;

    // Flattened collection for item-level display with financial calculations
    [ObservableProperty]
    private ObservableCollection<StockAdjustmentItemDto> _stockAdjustmentItems = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private DateTime? _startDate;

    [ObservableProperty]
    private DateTime? _endDate;

    [ObservableProperty]
    private bool _isTransferFormPanelOpen = false;

    [ObservableProperty]
    private bool _isAdjustProductPanelOpen = false;

    [ObservableProperty]
    private TransferProductModel _transferProduct = new();

    [ObservableProperty]
    private AdjustProductModel _adjustProduct = new();

    // Product Search Properties
    [ObservableProperty]
    private ObservableCollection<StockAdjustmentSearchItemDto> _searchResults = new();

    [ObservableProperty]
    private ObservableCollection<StockAdjustmentReasonDto> _adjustmentReasons = new();

    // Services for data access
    private readonly IProductService? _productService;
    private readonly IStockAdjustmentService? _stockAdjustmentService;
    private readonly IStockTransferService? _stockTransferService;
    private readonly IGoodsReturnService? _goodsReturnService;
    private readonly IGoodsReplaceService? _goodsReplaceService;

    // Debouncing timer for search
    private readonly DispatcherTimer _searchDebounceTimer;
    private string _pendingSearchTerm = string.Empty;
    private ProductDto? _lastSelectedProduct = null;

    // Common Properties for both modules
    [ObservableProperty]
    private string _backButtonText = "Back";

    [ObservableProperty]
    private string _refreshButtonText = "Refresh";

    // Permission Properties for Inventory Module
    [ObservableProperty]
    private bool _canCreateInventory = false;

    [ObservableProperty]
    private bool _canEditInventory = false;

    [ObservableProperty]
    private bool _canDeleteInventory = false;

    // Permission Properties for Stock Adjustment Module
    [ObservableProperty]
    private bool _canCreateStockAdjustment = false;

    [ObservableProperty]
    private bool _canEditStockAdjustment = false;

    [ObservableProperty]
    private bool _canDeleteStockAdjustment = false;

    // Permission Properties for Stock Transfer Module
    [ObservableProperty]
    private bool _canCreateStockTransfer = false;

    [ObservableProperty]
    private bool _canEditStockTransfer = false;

    [ObservableProperty]
    private bool _canDeleteStockTransfer = false;

    // Permission Properties for GRN Module
    [ObservableProperty]
    private bool _canCreateGrn = false;

    [ObservableProperty]
    private bool _canEditGrn = false;

    [ObservableProperty]
    private bool _canDeleteGrn = false;

    // Permission Properties for Goods Return Module
    [ObservableProperty]
    private bool _canCreateGoodsReturn = false;

    [ObservableProperty]
    private bool _canEditGoodsReturn = false;

    [ObservableProperty]
    private bool _canDeleteGoodsReturn = false;

    // Permission Properties for Goods Replace Module
    [ObservableProperty]
    private bool _canCreateGoodsReplace = false;

    [ObservableProperty]
    private bool _canEditGoodsReplace = false;

    [ObservableProperty]
    private bool _canDeleteGoodsReplace = false;

    #endregion

    #region Logging

    /// <summary>
    /// Log a message to both console and file
    /// </summary>
    private static void LogMessage(string message)
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
        System.Diagnostics.Debug.WriteLine(logEntry);
        Console.WriteLine(logEntry);
        
        try
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var chronoPosPath = Path.Combine(appDataPath, "ChronoPos");
            var logFilePath = Path.Combine(chronoPosPath, "app.log");
            
            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }
        catch
        {
            // Ignore file write errors
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// Command to navigate to a specific stock module
    /// </summary>
    [RelayCommand]
    private void NavigateToModule(string moduleType)
    {
        // TODO: Implement navigation to specific stock module
        System.Diagnostics.Debug.WriteLine($"Navigating to {moduleType} module...");
    }

    /// <summary>
    /// Command to select a specific module
    /// </summary>
    [RelayCommand]
    private void SelectModule(string moduleType)
    {
        // Reset all selections
        IsStockAdjustmentSelected = false;
        IsStockTransferSelected = false;
        IsGoodsReceivedSelected = false;
        IsGoodsReturnSelected = false;
        IsGoodsReplaceSelected = false;
        NoModuleSelected = false;

        // Update module selections
        foreach (var module in Modules)
        {
            module.IsSelected = module.ModuleType == moduleType;
        }

        // Set the appropriate selection
        switch (moduleType)
        {
            case "StockAdjustment":
                IsStockAdjustmentSelected = true;
                break;
            case "StockTransfer":
                IsStockTransferSelected = true;
                _ = LoadStockTransfersAsync();
                break;
            case "GoodsReceived":
                IsGoodsReceivedSelected = true;
                _ = LoadGoodsReceivedNotesAsync(); // Fire and forget
                break;
            case "GoodsReturn":
                IsGoodsReturnSelected = true;
                _ = LoadGoodsReturnsAsync();
                break;
            case "GoodsReplace":
                IsGoodsReplaceSelected = true;
                _ = LoadGoodsReplacesAsync();
                break;
            default:
                NoModuleSelected = true;
                break;
        }
    }

    /// <summary>
    /// Command to refresh stock modules data
    /// </summary>
    [RelayCommand]
    private async Task RefreshModulesAsync()
    {
        await LoadModuleDataAsync();
    }

    /// <summary>
    /// Command to refresh stock adjustments data specifically
    /// </summary>
    [RelayCommand]
    private async Task RefreshStockAdjustments()
    {
        LogMessage("[RefreshStockAdjustments] Refreshing stock adjustments data");
        await LoadStockAdjustmentsAsync();
    }

    /// <summary>
    /// Command to go back to management page
    /// </summary>
    public ICommand? GoBackCommand { get; set; }

    // Stock Transfer Commands
    [RelayCommand]
    private void CreateNewTransfer()
    {
        System.Diagnostics.Debug.WriteLine("CreateNewTransfer command executed!");
        
        // Close adjust panel if open
        IsAdjustProductPanelOpen = false;
        
        // Navigate to AddStockTransfer screen instead of opening side panel
        _navigateToAddStockTransfer?.Invoke();
    }

    [RelayCommand]
    private void OpenTransferFormPanel()
    {
        // Close adjust panel if open
        IsAdjustProductPanelOpen = false;
        
        IsTransferFormPanelOpen = true;
        TransferProduct = new TransferProductModel();
    }

    [RelayCommand]
    private void CloseTransferFormPanel()
    {
        IsTransferFormPanelOpen = false;
    }

    [RelayCommand]
    private void SaveTransferProduct()
    {
        // TODO: Implement save transfer logic
        IsTransferFormPanelOpen = false;
    }

    [RelayCommand]
    private void EditTransfer(StockTransferDto transfer)
    {
        try
        {
            AppLogger.LogInfo($"Editing Stock Transfer: {transfer.TransferNo}", $"Transfer ID: {transfer.TransferId}, Status: {transfer.Status}", "stock_management");
            
            // Check if transfer can be edited (only Pending status should be editable)
            if (transfer.Status != "Pending")
            {
                MessageBox.Show($"Cannot edit Stock Transfer {transfer.TransferNo}. Only transfers with 'Pending' status can be edited.\nCurrent status: {transfer.Status}", 
                               "Edit Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Navigate to AddStockTransfer in edit mode
            _navigateToEditStockTransfer?.Invoke(transfer.TransferId);
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to edit Stock Transfer", ex, $"Transfer: {transfer.TransferNo}, ID: {transfer.TransferId}", "stock_management");
            MessageBox.Show($"Failed to edit stock transfer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteTransfer(StockTransferDto transfer)
    {
        try
        {
            AppLogger.LogInfo($"Delete Stock Transfer request: {transfer.TransferNo}", $"Transfer ID: {transfer.TransferId}, Status: {transfer.Status}", "stock_management");
            
            // Check if transfer can be deleted (only Pending status should be deletable)
            if (transfer.Status != "Pending")
            {
                MessageBox.Show($"Cannot delete Stock Transfer {transfer.TransferNo}. Only transfers with 'Pending' status can be deleted.\nCurrent status: {transfer.Status}", 
                               "Delete Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete Stock Transfer {transfer.TransferNo}?\n\nThis action cannot be undone and will restore the stock quantities.", 
                                       "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes && _stockTransferService != null)
            {
                await _stockTransferService.DeleteStockTransferAsync(transfer.TransferId);
                    
                AppLogger.LogInfo($"Stock Transfer deleted successfully: {transfer.TransferNo}", $"Transfer ID: {transfer.TransferId}", "stock_management");
                MessageBox.Show($"Stock Transfer {transfer.TransferNo} deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Refresh the list
                await LoadStockTransfersAsync();
            }
            else if (_stockTransferService == null)
            {
                AppLogger.LogWarning("StockTransferService not available for delete operation", "Service injection failed", "stock_management");
                MessageBox.Show("Delete service not available. Please try again later.", "Service Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to delete Stock Transfer", ex, $"Transfer: {transfer.TransferNo}, ID: {transfer.TransferId}", "stock_management");
            MessageBox.Show($"Failed to delete stock transfer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ViewTransfer(StockTransferDto transfer)
    {
        try
        {
            AppLogger.LogInfo($"Viewing Stock Transfer: {transfer.TransferNo}", $"Transfer ID: {transfer.TransferId}, Status: {transfer.Status}", "stock_management");
            
            // Show transfer details in a dialog or navigate to view page
            var message = $"Stock Transfer Details:\n\n" +
                         $"Transfer No: {transfer.TransferNo}\n" +
                         $"From Store: {transfer.FromStoreName}\n" +
                         $"To Store: {transfer.ToStoreName}\n" +
                         $"Status: {transfer.Status}\n" +
                         $"Total Items: {transfer.TotalItems}\n" +
                         $"Created By: {transfer.CreatedByName}\n" +
                         $"Created Date: {transfer.CreatedAt:yyyy-MM-dd HH:mm}\n" +
                         $"Remarks: {transfer.Remarks ?? "N/A"}";
            
            MessageBox.Show(message, $"View Stock Transfer - {transfer.TransferNo}", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to view Stock Transfer", ex, $"Transfer: {transfer.TransferNo}, ID: {transfer.TransferId}", "stock_management");
            MessageBox.Show($"Failed to view stock transfer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ClearFilters()
    {
        SearchText = string.Empty;
        StartDate = null;
        EndDate = null;
        SelectedStatus = "All";
        SelectedGrnStatusFilter = "All";
        SelectedGrnSupplierId = null;
        GrnSearchTerm = string.Empty;
        
        // Refresh data after clearing filters
        await LoadStockAdjustmentsAsync();
        await LoadStockTransfersAsync();
        await LoadGoodsReceivedNotesAsync();
        await LoadGoodsReturnsAsync();
    }

    #endregion

    #region Goods Return Commands

    [RelayCommand]
    private void CreateNewGoodsReturn()
    {
        System.Diagnostics.Debug.WriteLine("CreateNewGoodsReturn command executed!");
        
        // Close adjust panel if open
        IsAdjustProductPanelOpen = false;
        
        // Navigate to AddGoodsReturn screen
        _navigateToAddGoodsReturn?.Invoke();
    }

    [RelayCommand]
    private void EditGoodsReturn(GoodsReturnDto goodsReturn)
    {
        if (goodsReturn == null)
        {
            AppLogger.LogWarning("Edit Goods Return attempted with null object", "", "stock_management");
            MessageBox.Show("No goods return selected for editing.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            AppLogger.LogInfo($"Editing Goods Return: {goodsReturn.ReturnNo}", $"Return ID: {goodsReturn.Id}, Status: {goodsReturn.Status}", "stock_management");
            
            // Check if goods return can be edited (only Pending status should be editable)
            if (goodsReturn.Status != "Pending")
            {
                MessageBox.Show($"Cannot edit Goods Return {goodsReturn.ReturnNo}. Only returns with 'Pending' status can be edited.\nCurrent status: {goodsReturn.Status}", 
                               "Edit Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Navigate to AddGoodsReturn in edit mode
            _navigateToEditGoodsReturn?.Invoke(goodsReturn.Id);
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to edit Goods Return", ex, $"Return: {goodsReturn.ReturnNo}, ID: {goodsReturn.Id}", "stock_management");
            MessageBox.Show($"Failed to edit goods return: {ex.Message}", "Edit Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteGoodsReturn(GoodsReturnDto goodsReturn)
    {
        if (goodsReturn == null)
        {
            AppLogger.LogWarning("Delete Goods Return attempted with null object", "", "stock_management");
            MessageBox.Show("No goods return selected for deletion.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show($"Are you sure you want to delete Goods Return {goodsReturn.ReturnNo}?\n\nThis action cannot be undone.", 
                                   "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes && _goodsReturnService != null)
        {
            try
            {
                await _goodsReturnService.DeleteGoodsReturnAsync(goodsReturn.Id);
                
                AppLogger.LogInfo($"Goods Return deleted successfully: {goodsReturn.ReturnNo}", $"Return ID: {goodsReturn.Id}", "stock_management");
                MessageBox.Show($"Goods Return {goodsReturn.ReturnNo} deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Refresh the list
                await LoadGoodsReturnsAsync();
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Failed to delete Goods Return", ex, $"Return: {goodsReturn.ReturnNo}, ID: {goodsReturn.Id}", "stock_management");
                MessageBox.Show($"Failed to delete goods return: {ex.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else if (_goodsReturnService == null)
        {
            AppLogger.LogWarning("GoodsReturnService not available for delete operation", "Service injection failed", "stock_management");
            MessageBox.Show("Delete service not available. Please try again later.", "Service Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private async Task ViewGoodsReturn(GoodsReturnDto goodsReturn)
    {
        try
        {
            AppLogger.LogInfo($"Viewing Goods Return: {goodsReturn.ReturnNo}", $"Return ID: {goodsReturn.Id}, Status: {goodsReturn.Status}", "stock_management");
            
            // Show goods return details in a dialog or navigate to view page
            var message = $"Goods Return Details:\n\n" +
                         $"Return No: {goodsReturn.ReturnNo}\n" +
                         $"Supplier: {goodsReturn.SupplierName}\n" +
                         $"Store: {goodsReturn.StoreName}\n" +
                         $"Status: {goodsReturn.Status}\n" +
                         $"Total Items: {goodsReturn.TotalItems}\n" +
                         $"Total Amount: {goodsReturn.TotalAmount:C}\n" +
                         $"Created By: {goodsReturn.CreatedByName}\n" +
                         $"Return Date: {goodsReturn.ReturnDate:yyyy-MM-dd}\n" +
                         $"Remarks: {goodsReturn.Remarks ?? "N/A"}";
            
            MessageBox.Show(message, $"View Goods Return - {goodsReturn.ReturnNo}", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to view Goods Return", ex, $"Return: {goodsReturn.ReturnNo}, ID: {goodsReturn.Id}", "stock_management");
            MessageBox.Show($"Failed to view goods return: {ex.Message}", "View Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }



    #endregion

    #region Goods Replace Commands

    [RelayCommand]
    private void CreateNewGoodsReplace()
    {
        System.Diagnostics.Debug.WriteLine("CreateNewGoodsReplace command executed!");
        
        // Close adjust panel if open
        IsAdjustProductPanelOpen = false;
        
        // Navigate to AddGoodsReplace screen
        _navigateToAddGoodsReplace?.Invoke();
    }

    [RelayCommand]
    private void EditGoodsReplace(GoodsReplaceDto goodsReplace)
    {
        if (goodsReplace == null)
        {
            AppLogger.LogWarning("Edit Goods Replace attempted with null object", "", "stock_management");
            MessageBox.Show("No goods replace selected for editing.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            AppLogger.LogInfo($"Editing Goods Replace: {goodsReplace.ReplaceNo}", $"Replace ID: {goodsReplace.Id}, Status: {goodsReplace.Status}", "stock_management");
            
            // Check if goods replace can be edited (only Pending status should be editable)
            if (goodsReplace.Status != "Pending")
            {
                AppLogger.LogWarning($"Cannot edit Goods Replace with status: {goodsReplace.Status}", $"Replace: {goodsReplace.ReplaceNo}", "stock_management");
                MessageBox.Show($"Goods Replace with status '{goodsReplace.Status}' cannot be edited.\nOnly 'Pending' replacements can be edited.", 
                    "Edit Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Navigate to edit screen
            _navigateToEditGoodsReplace?.Invoke(goodsReplace.Id);
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to edit Goods Replace", ex, $"Replace: {goodsReplace.ReplaceNo}, ID: {goodsReplace.Id}", "stock_management");
            MessageBox.Show($"Failed to open goods replace for editing: {ex.Message}", "Edit Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteGoodsReplace(GoodsReplaceDto goodsReplace)
    {
        if (goodsReplace == null)
        {
            AppLogger.LogWarning("Delete Goods Replace attempted with null object", "", "stock_management");
            MessageBox.Show("No goods replace selected for deletion.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show($"Are you sure you want to delete Goods Replace {goodsReplace.ReplaceNo}?\n\nThis action cannot be undone.", 
                                   "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes && _goodsReplaceService != null)
        {
            try
            {
                await _goodsReplaceService.DeleteGoodsReplaceAsync(goodsReplace.Id);
                
                AppLogger.LogInfo($"Goods Replace deleted successfully: {goodsReplace.ReplaceNo}", $"Replace ID: {goodsReplace.Id}", "stock_management");
                MessageBox.Show($"Goods Replace {goodsReplace.ReplaceNo} deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Refresh the list
                await LoadGoodsReplacesAsync();
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Failed to delete Goods Replace", ex, $"Replace: {goodsReplace.ReplaceNo}, ID: {goodsReplace.Id}", "stock_management");
                MessageBox.Show($"Failed to delete goods replace: {ex.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else if (_goodsReplaceService == null)
        {
            AppLogger.LogWarning("GoodsReplaceService not available for delete operation", "Service injection failed", "stock_management");
            MessageBox.Show("Delete service not available. Please try again later.", "Service Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private async Task ViewGoodsReplace(GoodsReplaceDto goodsReplace)
    {
        try
        {
            AppLogger.LogInfo($"Viewing Goods Replace: {goodsReplace.ReplaceNo}", $"Replace ID: {goodsReplace.Id}, Status: {goodsReplace.Status}", "stock_management");
            
            // Show goods replace details in a dialog or navigate to view page
            var message = $"Goods Replace Details:\n\n" +
                         $"Replace No: {goodsReplace.ReplaceNo}\n" +
                         $"Supplier: {goodsReplace.SupplierName}\n" +
                         $"Store: {goodsReplace.StoreName}\n" +
                         $"Replace Date: {goodsReplace.ReplaceDate:dd/MM/yyyy}\n" +
                         $"Status: {goodsReplace.Status}\n" +
                         $"Total Amount: {goodsReplace.TotalAmount:C2}\n" +
                         $"Total Items: {goodsReplace.TotalItems}\n" +
                         $"Created By: {goodsReplace.CreatedByName}\n" +
                         $"Remarks: {goodsReplace.Remarks ?? "N/A"}";
            
            MessageBox.Show(message, "Goods Replace Details", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to view Goods Replace details", ex, $"Replace: {goodsReplace.ReplaceNo}", "stock_management");
            MessageBox.Show($"Failed to load goods replace details: {ex.Message}", "View Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    // Stock Adjustment Commands
    [RelayCommand]
    private async Task OpenAdjustProductPanel()
    {
        LogMessage("[StockManagementViewModel] OpenAdjustProductPanel called");
        
        // Close transfer panel if open
        IsTransferFormPanelOpen = false;
        
        IsAdjustProductPanelOpen = true;
        AdjustProduct = new AdjustProductModel();
        
        // Subscribe to AdjustProduct property changes for search functionality
        AdjustProduct.PropertyChanged += OnAdjustProductPropertyChanged;
        LogMessage("[StockManagementViewModel] Subscribed to AdjustProduct.PropertyChanged");
        
        // Load adjustment reasons
        await LoadAdjustmentReasonsAsync();
        LogMessage("[StockManagementViewModel] Adjustment reasons loaded");
        
        // Load initial search results to populate dropdown
        await LoadInitialSearchResultsAsync();
        LogMessage("[StockManagementViewModel] Initial search results loaded");
    }

    [RelayCommand]
    private void CloseAdjustProductPanel()
    {
        LogMessage("[StockManagementViewModel] CloseAdjustProductPanel called");
        
        // Unsubscribe from property changes
        if (AdjustProduct != null)
        {
            AdjustProduct.PropertyChanged -= OnAdjustProductPropertyChanged;
            LogMessage("[StockManagementViewModel] Unsubscribed from AdjustProduct.PropertyChanged");
        }
        
        IsAdjustProductPanelOpen = false;
    }

    [RelayCommand]
    private async Task SaveAdjustProduct()
    {
        try
        {
            DesktopFileLogger.Log("[SaveAdjustProduct] === SAVE BUTTON CLICKED ===");
            DesktopFileLogger.Log("[SaveAdjustProduct] === STARTING SAVE OPERATION ===");
            Console.WriteLine("[SaveAdjustProduct] SAVE BUTTON CLICKED - CONSOLE LOG");
            
            // Check if service is available first
            DesktopFileLogger.Log($"[SaveAdjustProduct] StockAdjustmentService null check: {_stockAdjustmentService == null}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] AdjustProduct null check: {AdjustProduct == null}");
            
            // Validation
            DesktopFileLogger.Log($"[SaveAdjustProduct] Validating form data...");
            DesktopFileLogger.Log($"[SaveAdjustProduct] SelectedSearchItem: {AdjustProduct.SelectedSearchItem?.Name ?? "NULL"}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] ProductId: {AdjustProduct.ProductId}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] CurrentStock: {AdjustProduct.CurrentStock}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] NewQuantity: {AdjustProduct.NewQuantity}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] ReasonText: '{AdjustProduct.ReasonText}'");
            DesktopFileLogger.Log($"[SaveAdjustProduct] DifferenceQuantity: {AdjustProduct.DifferenceQuantity}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] ExpiryDate: {AdjustProduct.ExpiryDate}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] SelectedBatch: {(AdjustProduct.SelectedBatch != null ? $"'{AdjustProduct.SelectedBatch.BatchNo}'" : "NULL")}");
            
            if (!AdjustProduct.HasValidSelection)
            {
                DesktopFileLogger.Log($"[SaveAdjustProduct] ERROR: No valid selection - SelectedSearchItem: {AdjustProduct.SelectedSearchItem?.Name ?? "NULL"}, ProductId: {AdjustProduct.ProductId}");
                var modeText = AdjustProduct.AdjustmentMode == StockAdjustmentMode.Product ? "product" : "product unit";
                MessageBox.Show($"Please select a {modeText}.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(AdjustProduct.ReasonText))
            {
                DesktopFileLogger.Log("[SaveAdjustProduct] ERROR: No reason provided");
                MessageBox.Show("Please provide a reason for the adjustment.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AdjustProduct.DifferenceQuantity == 0)
            {
                DesktopFileLogger.Log("[SaveAdjustProduct] ERROR: No quantity difference");
                MessageBox.Show("No quantity change detected. Please enter a different quantity.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_stockAdjustmentService == null)
            {
                DesktopFileLogger.Log("[SaveAdjustProduct] ERROR: StockAdjustmentService is null");
                MessageBox.Show("Stock adjustment service not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DesktopFileLogger.Log("[SaveAdjustProduct] Validation passed. Creating reason...");

            // First, create or get the reason
            int reasonId;
            try 
            {
                DesktopFileLogger.Log($"[SaveAdjustProduct] Calling CreateReasonIfNotExistsAsync with reason: '{AdjustProduct.ReasonText}'");
                reasonId = await _stockAdjustmentService.CreateReasonIfNotExistsAsync(AdjustProduct.ReasonText);
                DesktopFileLogger.Log($"[SaveAdjustProduct] Reason created/found with ID: {reasonId}");
            }
            catch (Exception ex)
            {
                DesktopFileLogger.Log($"[SaveAdjustProduct] ERROR creating reason: {ex.Message}");
                DesktopFileLogger.Log($"[SaveAdjustProduct] Exception details: {ex}");
                MessageBox.Show($"Error creating adjustment reason: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Create the stock adjustment DTO
            DesktopFileLogger.Log("[SaveAdjustProduct] Creating stock adjustment DTO...");
            
            // Get UOM ID from StockAdjustmentService (let it handle the lookup)
            DesktopFileLogger.Log($"[SaveAdjustProduct] Getting UOM ID for ProductId: {AdjustProduct.ProductId}");
            
            // For now, we'll let the service handle UOM validation and use 1 as default
            // TODO: Add proper UOM lookup based on product configuration
            var productUomId = 1;
            DesktopFileLogger.Log($"[SaveAdjustProduct] Using UomId: {productUomId} (will be validated by service)");
            
            var selectedItem = AdjustProduct.SelectedSearchItem;
            var itemName = selectedItem?.Name ?? "Unknown Item";
            
            var createDto = new CreateStockAdjustmentDto
            {
                AdjustmentDate = DateTime.Now,
                StoreLocationId = 1, // TODO: Get default store location or let user select
                ReasonId = reasonId,
                Remarks = $"Stock adjustment for {itemName} ({AdjustProduct.AdjustmentMode})",
                Items = new List<CreateStockAdjustmentItemDto>
                {
                    new CreateStockAdjustmentItemDto
                    {
                        ProductId = AdjustProduct.ProductId,
                        UomId = selectedItem?.UnitId ?? productUomId,
                        BatchNo = AdjustProduct.SelectedBatch?.BatchNo,
                        ExpiryDate = AdjustProduct.ExpiryDate,
                        QuantityBefore = AdjustProduct.CurrentStock,
                        QuantityAfter = AdjustProduct.NewQuantity,
                        ReasonLine = AdjustProduct.ReasonText,
                        RemarksLine = $"{(AdjustProduct.IsIncrement ? "Increased" : "Decreased")} by {AdjustProduct.ChangeAmount} ({AdjustProduct.CurrentStock} → {AdjustProduct.NewQuantity})",
                        AdjustmentMode = AdjustProduct.AdjustmentMode,
                        ProductUnitId = selectedItem?.ProductUnitId,
                        ConversionFactor = selectedItem?.ConversionFactor ?? 1,
                        IsIncrement = AdjustProduct.IsIncrement,
                        ChangeAmount = AdjustProduct.ChangeAmount
                    }
                }
            };

            DesktopFileLogger.Log($"[SaveAdjustProduct] DTO created:");
            DesktopFileLogger.Log($"  - AdjustmentDate: {createDto.AdjustmentDate}");
            DesktopFileLogger.Log($"  - StoreLocationId: {createDto.StoreLocationId}");
            DesktopFileLogger.Log($"  - ReasonId: {createDto.ReasonId}");
            DesktopFileLogger.Log($"  - Remarks: {createDto.Remarks}");
            DesktopFileLogger.Log($"  - Items Count: {createDto.Items.Count}");
            if (createDto.Items.Any())
            {
                var item = createDto.Items.First();
                DesktopFileLogger.Log($"  - Item ProductId: {item.ProductId}");
                DesktopFileLogger.Log($"  - Item BatchNo: '{item.BatchNo ?? "NULL"}'");
                DesktopFileLogger.Log($"  - Item QuantityBefore: {item.QuantityBefore}");
                DesktopFileLogger.Log($"  - Item QuantityAfter: {item.QuantityAfter}");
                DesktopFileLogger.Log($"  - Item ReasonLine: {item.ReasonLine}");
            }

            // Save the adjustment
            DesktopFileLogger.Log("[SaveAdjustProduct] Calling CreateStockAdjustmentAsync...");
            var result = await _stockAdjustmentService.CreateStockAdjustmentAsync(createDto);
            DesktopFileLogger.Log($"[SaveAdjustProduct] Service call completed. Result: {result}");

            if (result != null)
            {
                DesktopFileLogger.Log($"[SaveAdjustProduct] SUCCESS! Adjustment saved with number: {result.AdjustmentNo}");
                MessageBox.Show("Stock adjustment saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Reset form and close panel
                DesktopFileLogger.Log("[SaveAdjustProduct] Resetting form and closing panel...");
                AdjustProduct.Reset();
                IsAdjustProductPanelOpen = false;
                
                // Refresh data
                DesktopFileLogger.Log("[SaveAdjustProduct] Refreshing data...");
                await LoadModuleDataAsync();
                DesktopFileLogger.Log("[SaveAdjustProduct] Data refresh completed");
            }
            else
            {
                DesktopFileLogger.Log($"[SaveAdjustProduct] ERROR: Service returned null result");
                MessageBox.Show("Failed to save stock adjustment. No records were created.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            DesktopFileLogger.Log($"[SaveAdjustProduct] FATAL ERROR: {ex.Message}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] Exception type: {ex.GetType().Name}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                DesktopFileLogger.Log($"[SaveAdjustProduct] Inner exception: {ex.InnerException.Message}");
            }
            MessageBox.Show($"An error occurred while saving: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            DesktopFileLogger.Log("[SaveAdjustProduct] === SAVE OPERATION COMPLETED ===");
        }
    }

    /// <summary>
    /// Handles property changes in the AdjustProduct model, particularly for search functionality
    /// </summary>
    private async void OnAdjustProductPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        DesktopFileLogger.LogSeparator($"PROPERTY CHANGED: {e.PropertyName}");
        DesktopFileLogger.Log($"Property: {e.PropertyName}");
        DesktopFileLogger.Log($"Current SearchText: '{AdjustProduct.SearchText}'");
        DesktopFileLogger.Log($"Current SelectedProduct: {AdjustProduct.SelectedProduct?.Name ?? "NULL"} (ID: {AdjustProduct.SelectedProduct?.Id ?? 0})");
        DesktopFileLogger.Log($"Last Selected Product: {_lastSelectedProduct?.Name ?? "NULL"} (ID: {_lastSelectedProduct?.Id ?? 0})");
        DesktopFileLogger.Log($"IsUpdatingFromSelection flag: {AdjustProduct.IsUpdatingFromSelection}");
        
        if (e.PropertyName == nameof(AdjustProductModel.SearchText))
        {
            DesktopFileLogger.Log("=== SEARCHTEXT CHANGED ===");
            
            // Don't trigger search if we're updating from a selection  
            if (AdjustProduct.IsUpdatingFromSelection)
            {
                DesktopFileLogger.Log("SKIPPING SEARCH: Updating from selection (first check)");
                return;
            }
            
            // CRITICAL FIX: Don't search if we're updating from selection
            if (AdjustProduct.IsUpdatingFromSelection)
            {
                DesktopFileLogger.Log("SKIPPING SEARCH: IsUpdatingFromSelection is true");
                return;
            }
            
            // Only search if user is actually typing (not selecting)
            var currentText = AdjustProduct.SearchText ?? string.Empty;
            var selectedItemName = AdjustProduct.SelectedSearchItem?.Name ?? string.Empty;
            
            DesktopFileLogger.Log($"Comparing: '{currentText}' vs '{selectedItemName}'");
            
            // If the search text matches the selected item exactly, don't search
            if (!string.IsNullOrEmpty(selectedItemName) && currentText.Equals(selectedItemName, StringComparison.OrdinalIgnoreCase))
            {
                DesktopFileLogger.Log("SKIPPING SEARCH: SearchText matches selected item name");
                return;
            }
            
            _pendingSearchTerm = currentText;
            DesktopFileLogger.Log($"TRIGGERING SEARCH for: '{_pendingSearchTerm}'");
            
            // Cancel previous search timer
            _searchDebounceTimer?.Stop();
            
            // Start new search timer with 300ms delay
            _searchDebounceTimer?.Start();
            DesktopFileLogger.Log("Search timer started");
        }
        else if (e.PropertyName == nameof(AdjustProductModel.SelectedSearchItem))
        {
            DesktopFileLogger.Log("=== SELECTEDSEARCHITEM CHANGED ===");
            
            var selectedItem = AdjustProduct.SelectedSearchItem;
            if (selectedItem != null)
            {
                DesktopFileLogger.Log($"New selected item: {selectedItem.Name} (Mode: {selectedItem.Mode})");
                
                // Load current stock for the selected item
                await LoadCurrentStockForSelectedItem(selectedItem);
                
                // Load available batches for the selected product
                await LoadProductBatchesAsync(selectedItem.ProductId);
            }
        }
        else if (e.PropertyName == nameof(AdjustProductModel.SelectedProduct))
        {
            DesktopFileLogger.Log("=== SELECTEDPRODUCT CHANGED ===");
            
            var currentProduct = AdjustProduct.SelectedProduct;
            var isActualChange = currentProduct?.Id != _lastSelectedProduct?.Id;
            
            DesktopFileLogger.Log($"Is actual product change: {isActualChange}");
            
            if (isActualChange)
            {
                DesktopFileLogger.Log($"PRODUCT SELECTION CHANGED: {_lastSelectedProduct?.Name ?? "NULL"} → {currentProduct?.Name ?? "NULL"}");
                
                // Update search text with selected product name
                if (currentProduct != null)
                {
                    DesktopFileLogger.Log($"Updating SearchText to match selected product: '{currentProduct.Name}'");
                    
                    // The flag is now managed in the Model itself
                    AdjustProduct.SearchText = currentProduct.Name;
                    
                    DesktopFileLogger.Log($"SearchText updated successfully");
                }
                
                // Update last selected product
                _lastSelectedProduct = currentProduct;
                
                // Load stock for the selected product
                DesktopFileLogger.Log("Loading stock for selected product...");
                await LoadCurrentStockForSelectedProduct();
            }
            else
            {
                DesktopFileLogger.Log("IGNORING: Same product selected again");
            }
        }
        
        DesktopFileLogger.LogSeparator("END PROPERTY CHANGE");
    }

    [RelayCommand]
    private void SearchProduct()
    {
        // TODO: Implement product search logic
    }

    // New Enhanced Commands for Stock Adjustment
    [RelayCommand]
    private void IncrementQuantity()
    {
        AdjustProduct.ChangeAmount += 1;
    }

    [RelayCommand]
    private void DecrementQuantity()
    {
        if (AdjustProduct.ChangeAmount > 0)
        {
            AdjustProduct.ChangeAmount -= 1;
        }
    }

    // Toggle commands for increment/decrement mode
    [RelayCommand]
    private void ToggleIncrementMode()
    {
        AppLogger.Log("Stock Management", "Toggling to Increment mode");
        AdjustProduct.IsIncrement = true;
    }

    [RelayCommand]
    private void ToggleDecrementMode()
    {
        AppLogger.Log("Stock Management", "Toggling to Decrement mode");
        AdjustProduct.IsIncrement = false;
    }

    [RelayCommand]
    private void Keypad(string input)
    {
        switch (input.ToLower())
        {
            case "clear":
                AdjustProduct.ChangeAmountText = "0";
                break;
            case "backspace":
                var backspaceText = AdjustProduct.ChangeAmountText ?? "0";
                if (backspaceText.Length > 0)
                {
                    backspaceText = backspaceText[..^1];
                    if (string.IsNullOrEmpty(backspaceText))
                        backspaceText = "0";
                    AdjustProduct.ChangeAmountText = backspaceText;
                }
                break;
            case "enter":
                // Focus next field or save
                break;
            case "+":
                AdjustProduct.ChangeAmountText = (AdjustProduct.CurrentStock + 1).ToString();
                break;
            case "-":
                AdjustProduct.ChangeAmountText = Math.Max(0, AdjustProduct.CurrentStock - 1).ToString();
                break;
            case ".":
                // Handle decimal point - only add if not already present
                var current = AdjustProduct.ChangeAmountText ?? "0";
                if (!current.Contains("."))
                {
                    AdjustProduct.ChangeAmountText = current + ".";
                }
                break;
            default:
                if (int.TryParse(input, out var digit))
                {
                    var existingText = AdjustProduct.ChangeAmountText ?? "0";
                    if (existingText == "0")
                    {
                        AdjustProduct.ChangeAmountText = digit.ToString();
                    }
                    else
                    {
                        AdjustProduct.ChangeAmountText = existingText + digit.ToString();
                    }
                }
                break;
        }
    }

    [RelayCommand]
    private void SelectProductMode()
    {
        DesktopFileLogger.Log("[StockManagementViewModel] Product mode selected");
        var previousMode = AdjustProduct.AdjustmentMode;
        AdjustProduct.AdjustmentMode = StockAdjustmentMode.Product;
        
        // Only clear selection if mode actually changed
        if (previousMode != StockAdjustmentMode.Product)
        {
            DesktopFileLogger.Log("[StockManagementViewModel] Mode changed, clearing selection");
            SearchResults.Clear();
            AdjustProduct.SearchText = string.Empty;
            AdjustProduct.SelectedSearchItem = null;
        }
        else
        {
            DesktopFileLogger.Log("[StockManagementViewModel] Same mode selected, keeping current selection");
        }
    }

    [RelayCommand]
    private void SelectProductUnitMode()
    {
        DesktopFileLogger.Log("[StockManagementViewModel] Product Unit mode selected");
        var previousMode = AdjustProduct.AdjustmentMode;
        AdjustProduct.AdjustmentMode = StockAdjustmentMode.ProductUnit;
        
        // Only clear selection if mode actually changed
        if (previousMode != StockAdjustmentMode.ProductUnit)
        {
            DesktopFileLogger.Log("[StockManagementViewModel] Mode changed, clearing selection");
            SearchResults.Clear();
            AdjustProduct.SearchText = string.Empty;
            AdjustProduct.SelectedSearchItem = null;
        }
        else
        {
            DesktopFileLogger.Log("[StockManagementViewModel] Same mode selected, keeping current selection");
        }
    }

    [RelayCommand]
    private async Task SaveStockAdjustment()
    {
        if (!AdjustProduct.IsValid)
        {
            // Show validation error
            return;
        }

        try
        {
            // Add debugging logs for batch selection
            AppLogger.LogInfo($"STOCK ADJUSTMENT - ProductId: {AdjustProduct.ProductId}", 
                $"SelectedBatch: {(AdjustProduct.SelectedBatch != null ? $"'{AdjustProduct.SelectedBatch.BatchNo}'" : "NULL")}", 
                "stock_adjustment");

            // Create stock adjustment DTO
            var adjustmentDto = new CreateStockAdjustmentDto
            {
                AdjustmentDate = DateTime.Now,
                StoreLocationId = 1, // TODO: Get from selected store
                ReasonId = AdjustProduct.ReasonId ?? 1, // Default to 1 if null
                Notes = $"Stock adjustment for {AdjustProduct.ProductName}",
                Items = new List<CreateStockAdjustmentItemDto>
                {
                    new CreateStockAdjustmentItemDto
                    {
                        ProductId = AdjustProduct.ProductId,
                        UomId = 1, // TODO: Get from product
                        BatchNo = AdjustProduct.SelectedBatch?.BatchNo,
                        ExpiryDate = AdjustProduct.SelectedBatch?.ExpiryDate ?? AdjustProduct.ExpiryDate,
                        QuantityBefore = AdjustProduct.CurrentStock,
                        QuantityAfter = AdjustProduct.NewQuantity,
                        ConversionFactor = 1, // Default for Product mode
                        ReasonLine = "Stock adjustment via touch interface"
                    }
                }
            };

            AppLogger.LogInfo($"STOCK ADJUSTMENT DTO CREATED", 
                $"BatchNo in DTO: '{adjustmentDto.Items.First().BatchNo ?? "NULL"}'", 
                "stock_adjustment");

            // Save via service (ProductBatch updates are now handled within the service)
            if (_stockAdjustmentService != null)
            {
                var result = await _stockAdjustmentService.CreateStockAdjustmentAsync(adjustmentDto);
                AppLogger.LogInfo($"STOCK ADJUSTMENT COMPLETED", 
                    $"AdjustmentNo: {result.AdjustmentNo}, Service handles both product and batch updates", 
                    "stock_adjustment");
                
                // Refresh batches after the service has updated them
                await LoadProductBatchesAsync(AdjustProduct.ProductId);
                
                // Show success message
                MessageBox.Show($"Stock adjustment saved successfully!\nAdjustment No: {result.AdjustmentNo}", 
                               "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Reset form and close panel
                AdjustProduct.Reset();
                CloseAdjustProductPanel();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving stock adjustment: {ex.Message}", 
                           "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #region GRN Commands
    [RelayCommand]
    private void AddGrn()
    {
        try
        {
            _navigateToAddGrn?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error navigating to Add GRN: {ex.Message}", 
                           "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ViewGrn(GoodsReceivedDto grn)
    {
        try
        {
            AppLogger.LogInfo($"Viewing GRN: {grn.GrnNo}", $"GRN ID: {grn.Id}, Status: {grn.Status}", "stock_management");
            
            if (_goodsReceivedService != null)
            {
                // Load detailed GRN information
                var detailedGrn = await _goodsReceivedService.GetByIdAsync(grn.Id);
                
                if (detailedGrn != null)
                {
                    // Create a detailed view message
                    var details = $"GRN Details:\n\n" +
                                $"GRN No: {detailedGrn.GrnNo}\n" +
                                $"Received Date: {detailedGrn.ReceivedDate:dd/MM/yyyy}\n" +
                                $"Supplier: {detailedGrn.SupplierName}\n" +
                                $"Store: {detailedGrn.StoreName}\n" +
                                $"Invoice No: {detailedGrn.InvoiceNo ?? "N/A"}\n" +
                                $"Status: {detailedGrn.Status}\n" +
                                $"Total Amount: ₹{detailedGrn.TotalAmount:N2}\n" +
                                $"Created: {detailedGrn.CreatedAt:dd/MM/yyyy HH:mm}\n" +
                                $"Remarks: {detailedGrn.Remarks ?? "N/A"}";
                    
                    MessageBox.Show(details, $"View GRN - {grn.GrnNo}", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"GRN {grn.GrnNo} not found in database.", "GRN Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                // Fallback for when service is not available
                var details = $"GRN Details:\n\n" +
                            $"GRN No: {grn.GrnNo}\n" +
                            $"Received Date: {grn.ReceivedDate:dd/MM/yyyy}\n" +
                            $"Supplier: {grn.SupplierName}\n" +
                            $"Store: {grn.StoreName}\n" +
                            $"Invoice No: {grn.InvoiceNo ?? "N/A"}\n" +
                            $"Status: {grn.Status}\n" +
                            $"Total Amount: ₹{grn.TotalAmount:N2}\n" +
                            $"Created: {grn.CreatedAt:dd/MM/yyyy HH:mm}";
                
                MessageBox.Show(details, $"View GRN - {grn.GrnNo}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to view GRN details", ex, $"GRN: {grn.GrnNo}, ID: {grn.Id}", "stock_management");
            MessageBox.Show($"Error loading GRN details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void EditGrn(GoodsReceivedDto grn)
    {
        try
        {
            AppLogger.LogInfo($"Editing GRN: {grn.GrnNo}", $"GRN ID: {grn.Id}, Status: {grn.Status}", "stock_management");
            
            // Check if GRN can be edited (only Draft status should be editable)
            if (grn.Status != "Draft")
            {
                MessageBox.Show($"Cannot edit GRN {grn.GrnNo}. Only GRNs with 'Draft' status can be edited.\nCurrent status: {grn.Status}", 
                               "Edit Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Navigate to AddGrn in edit mode - we'll create this navigation method
            NavigateToEditGrn?.Invoke(grn.Id);
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to edit GRN", ex, $"GRN: {grn.GrnNo}, ID: {grn.Id}", "stock_management");
            MessageBox.Show($"Error opening GRN for editing: {ex.Message}", 
                           "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteGrn(GoodsReceivedDto grn)
    {
        try
        {
            AppLogger.LogInfo($"Delete GRN request: {grn.GrnNo}", $"GRN ID: {grn.Id}, Status: {grn.Status}", "stock_management");
            
            // Check if GRN can be deleted (only Draft status should be deletable)
            if (grn.Status != "Draft")
            {
                MessageBox.Show($"Cannot delete GRN {grn.GrnNo}. Only GRNs with 'Draft' status can be deleted.\nCurrent status: {grn.Status}", 
                               "Delete Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete GRN {grn.GrnNo}?\n\nThis action cannot be undone.", 
                                       "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes && _goodsReceivedService != null)
            {
                // Check if the service has a delete method
                var serviceType = _goodsReceivedService.GetType();
                var deleteMethod = serviceType.GetMethod("DeleteAsync") ?? serviceType.GetMethod("DeleteGoodsReceivedAsync");
                
                if (deleteMethod != null)
                {
                    // Call the delete method
                    var deleteTask = (Task)deleteMethod.Invoke(_goodsReceivedService, new object[] { grn.Id })!;
                    await deleteTask;
                    
                    AppLogger.LogInfo($"GRN deleted successfully: {grn.GrnNo}", $"GRN ID: {grn.Id}", "stock_management");
                    MessageBox.Show($"GRN {grn.GrnNo} deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Refresh the list
                    await LoadGoodsReceivedNotesAsync();
                }
                else
                {
                    AppLogger.LogWarning("Delete method not found in GoodsReceivedService", "Using fallback approach", "stock_management");
                    
                    // Fallback: Remove from collection (for testing purposes)
                    GoodsReceivedNotes.Remove(grn);
                    MessageBox.Show($"GRN {grn.GrnNo} deleted successfully! (Test mode)", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (_goodsReceivedService == null)
            {
                AppLogger.LogWarning("GoodsReceivedService not available for delete operation", "Service injection failed", "stock_management");
                MessageBox.Show("Delete service not available. Please try again later.", "Service Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to delete GRN", ex, $"GRN: {grn.GrnNo}, ID: {grn.Id}", "stock_management");
            MessageBox.Show($"Error deleting GRN: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task RefreshGrn()
    {
        await LoadGoodsReceivedNotesAsync();
    }

    #endregion

    #region Constructor

    public StockManagementViewModel(
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        InfrastructureServices.IDatabaseLocalizationService databaseLocalizationService,
        ICurrentUserService currentUserService,
        IProductService? productService = null,
        IStockAdjustmentService? stockAdjustmentService = null,
        IProductBatchService? productBatchService = null,
        IGoodsReceivedService? goodsReceivedService = null,
        ISupplierService? supplierService = null,
        IStockTransferService? stockTransferService = null,
        IGoodsReturnService? goodsReturnService = null,
        IGoodsReplaceService? goodsReplaceService = null,
        Action? navigateToAddGrn = null,
        Action<long>? navigateToEditGrn = null,
        Action? navigateToAddStockTransfer = null,
        Action<int>? navigateToEditStockTransfer = null,
        Action? navigateToAddGoodsReturn = null,
        Action<int>? navigateToEditGoodsReturn = null,
        Action? navigateToAddGoodsReplace = null,
        Action<int>? navigateToEditGoodsReplace = null)
    {
        LogMessage("[StockManagementViewModel] Constructor called");
        LogMessage($"[StockManagementViewModel] ProductService is null: {productService == null}");
        LogMessage($"[StockManagementViewModel] StockAdjustmentService is null: {stockAdjustmentService == null}");
        
        // New AppLogger usage
        AppLogger.LogInfo("StockManagementViewModel initialized", 
            $"ProductService: {(productService != null ? "Available" : "Null")}, StockAdjustmentService: {(stockAdjustmentService != null ? "Available" : "Null")}", 
            "viewmodel");
        
        _themeService = themeService;
        _zoomService = zoomService;
        _localizationService = localizationService;
        _colorSchemeService = colorSchemeService;
        _layoutDirectionService = layoutDirectionService;
        _fontService = fontService;
        _databaseLocalizationService = databaseLocalizationService;
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _productService = productService;
        _stockAdjustmentService = stockAdjustmentService;
        _productBatchService = productBatchService;
        _goodsReceivedService = goodsReceivedService;
        _supplierService = supplierService;
        _stockTransferService = stockTransferService;
        _goodsReturnService = goodsReturnService;
        _goodsReplaceService = goodsReplaceService;
        _navigateToAddGrn = navigateToAddGrn;
        NavigateToEditGrn = navigateToEditGrn;
        _navigateToAddStockTransfer = navigateToAddStockTransfer;
        _navigateToEditStockTransfer = navigateToEditStockTransfer;
        _navigateToAddGoodsReturn = navigateToAddGoodsReturn;
        _navigateToEditGoodsReturn = navigateToEditGoodsReturn;
        _navigateToAddGoodsReplace = navigateToAddGoodsReplace;
        _navigateToEditGoodsReplace = navigateToEditGoodsReplace;

        // Log service availability
        AppLogger.LogInfo("StockManagementViewModel Services", 
            $"GoodsReturnService: {(goodsReturnService != null ? "Available" : "NULL")}, GoodsReplaceService: {(goodsReplaceService != null ? "Available" : "NULL")}", 
            "viewmodel");

        // Initialize search debouncing timer
        _searchDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300) // 300ms debounce
        };
        _searchDebounceTimer.Tick += async (sender, e) =>
        {
            _searchDebounceTimer.Stop();
            
            // Check if this is for product search (in adjust panel), stock adjustments search, or GRN search
            if (IsAdjustProductPanelOpen && !string.IsNullOrEmpty(_pendingSearchTerm))
            {
                LogMessage($"[StockManagementViewModel] Timer tick - performing product search for: '{_pendingSearchTerm}'");
                await PerformSearchAsync(_pendingSearchTerm);
            }
            else if (IsGoodsReceivedSelected)
            {
                LogMessage($"[StockManagementViewModel] Timer tick - performing GRN search for: '{GrnSearchTerm}'");
                await LoadGoodsReceivedNotesAsync();
            }
            else if (IsStockAdjustmentSelected)
            {
                LogMessage($"[StockManagementViewModel] Timer tick - performing stock adjustments search for: '{SearchText}'");
                await LoadStockAdjustmentsAsync();
            }
            else if (IsStockTransferSelected)
            {
                LogMessage($"[StockManagementViewModel] Timer tick - performing stock transfers search for: '{SearchText}'");
                await LoadStockTransfersAsync();
            }
            else if (IsGoodsReturnSelected)
            {
                LogMessage($"[StockManagementViewModel] Timer tick - performing goods returns search for: '{SearchText}'");
                await LoadGoodsReturnsAsync();
            }
            else if (IsGoodsReplaceSelected)
            {
                LogMessage($"[StockManagementViewModel] Timer tick - performing goods replaces search for: '{SearchText}'");
                await LoadGoodsReplacesAsync();
            }
        };

        // Subscribe to settings changes
        _themeService.ThemeChanged += OnThemeChanged;
        _zoomService.ZoomChanged += OnZoomChanged;
        _localizationService.LanguageChanged += OnLanguageChanged;
        _colorSchemeService.PrimaryColorChanged += OnPrimaryColorChanged;
        _layoutDirectionService.DirectionChanged += OnDirectionChanged;
        _databaseLocalizationService.LanguageChanged += OnDatabaseLanguageChanged;

        // Initialize with current settings
        InitializeSettings();
        
        // Initialize permissions
        InitializePermissions();
        
        // Setup product search event handler
        AdjustProduct.PropertyChanged += OnAdjustProductPropertyChanged;
        
        // Setup search text change handler for stock adjustments table
        PropertyChanged += OnViewModelPropertyChanged;
        
        // Initialize modules immediately for instant UI display
        InitializeModulesSync();
        
        // Load async data in background
        _ = LoadAsyncDataInBackground();
    }

    /// <summary>
    /// Handle property changes in the main ViewModel (e.g., SearchText, StartDate, EndDate, GRN filters)
    /// </summary>
    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchText))
        {
            LogMessage($"[SearchText Changed] New value: '{SearchText}'");
            
            // Cancel previous search timer
            _searchDebounceTimer?.Stop();
            
            // Start new search timer with 300ms delay
            _searchDebounceTimer?.Start();
        }
        else if (e.PropertyName == nameof(StartDate) || e.PropertyName == nameof(EndDate))
        {
            LogMessage($"[Date Filter Changed] StartDate: '{StartDate}', EndDate: '{EndDate}'");
            
            // Reload data when date filters change based on current selection
            if (IsStockAdjustmentSelected)
            {
                await LoadStockAdjustmentsAsync();
            }
            else if (IsStockTransferSelected)
            {
                await LoadStockTransfersAsync();
            }
            else if (IsGoodsReturnSelected)
            {
                await LoadGoodsReturnsAsync();
            }
        }
        else if (e.PropertyName == nameof(GrnSearchTerm))
        {
            LogMessage($"[GrnSearchTerm Changed] New value: '{GrnSearchTerm}'");
            
            // Reload GRN data when search term changes (with debouncing via timer)
            _searchDebounceTimer?.Stop();
            _searchDebounceTimer?.Start();
        }
        else if (e.PropertyName == nameof(SelectedStatus))
        {
            LogMessage($"[Status Filter Changed] Status: '{SelectedStatus}' - IsStockTransferSelected: {IsStockTransferSelected}, IsGoodsReturnSelected: {IsGoodsReturnSelected}");
            
            // Reload data based on current selection
            if (IsStockTransferSelected)
            {
                LogMessage($"[Status Filter] Reloading Stock Transfers with Status: '{SelectedStatus}'");
                await LoadStockTransfersAsync();
            }
            else if (IsGoodsReturnSelected)
            {
                LogMessage($"[Status Filter] Reloading Goods Returns with Status: '{SelectedStatus}'");
                await LoadGoodsReturnsAsync();
            }
            else
            {
                LogMessage($"[Status Filter] No section selected for reload");
            }
        }
        else if (e.PropertyName == nameof(SelectedGrnStatusFilter) || e.PropertyName == nameof(SelectedGrnSupplierId))
        {
            LogMessage($"[GRN Filter Changed] Status: '{SelectedGrnStatusFilter}', SupplierId: '{SelectedGrnSupplierId}'");
            
            // Reload GRN data when filters change
            if (IsGoodsReceivedSelected)
            {
                await LoadGoodsReceivedNotesAsync();
            }
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initialize permission properties based on current user's permissions
    /// </summary>
    private void InitializePermissions()
    {
        try
        {
            // Inventory permissions
            CanCreateInventory = _currentUserService.HasPermission(ScreenNames.INVENTORY, TypeMatrix.CREATE);
            CanEditInventory = _currentUserService.HasPermission(ScreenNames.INVENTORY, TypeMatrix.UPDATE);
            CanDeleteInventory = _currentUserService.HasPermission(ScreenNames.INVENTORY, TypeMatrix.DELETE);

            // Stock Adjustment permissions
            CanCreateStockAdjustment = _currentUserService.HasPermission(ScreenNames.STOCK_ADJUSTMENT, TypeMatrix.CREATE);
            CanEditStockAdjustment = _currentUserService.HasPermission(ScreenNames.STOCK_ADJUSTMENT, TypeMatrix.UPDATE);
            CanDeleteStockAdjustment = _currentUserService.HasPermission(ScreenNames.STOCK_ADJUSTMENT, TypeMatrix.DELETE);

            // Stock Transfer permissions
            CanCreateStockTransfer = _currentUserService.HasPermission(ScreenNames.STOCK_TRANSFER, TypeMatrix.CREATE);
            CanEditStockTransfer = _currentUserService.HasPermission(ScreenNames.STOCK_TRANSFER, TypeMatrix.UPDATE);
            CanDeleteStockTransfer = _currentUserService.HasPermission(ScreenNames.STOCK_TRANSFER, TypeMatrix.DELETE);

            // GRN permissions
            CanCreateGrn = _currentUserService.HasPermission(ScreenNames.GRN, TypeMatrix.CREATE);
            CanEditGrn = _currentUserService.HasPermission(ScreenNames.GRN, TypeMatrix.UPDATE);
            CanDeleteGrn = _currentUserService.HasPermission(ScreenNames.GRN, TypeMatrix.DELETE);

            // Goods Return permissions
            CanCreateGoodsReturn = _currentUserService.HasPermission(ScreenNames.GOODS_RETURN, TypeMatrix.CREATE);
            CanEditGoodsReturn = _currentUserService.HasPermission(ScreenNames.GOODS_RETURN, TypeMatrix.UPDATE);
            CanDeleteGoodsReturn = _currentUserService.HasPermission(ScreenNames.GOODS_RETURN, TypeMatrix.DELETE);

            // Goods Replace permissions
            CanCreateGoodsReplace = _currentUserService.HasPermission(ScreenNames.GOODS_REPLACE, TypeMatrix.CREATE);
            CanEditGoodsReplace = _currentUserService.HasPermission(ScreenNames.GOODS_REPLACE, TypeMatrix.UPDATE);
            CanDeleteGoodsReplace = _currentUserService.HasPermission(ScreenNames.GOODS_REPLACE, TypeMatrix.DELETE);

            AppLogger.LogInfo("Stock Management permissions initialized successfully", 
                $"Inventory: C={CanCreateInventory},U={CanEditInventory},D={CanDeleteInventory} | " +
                $"Adjustment: C={CanCreateStockAdjustment},U={CanEditStockAdjustment},D={CanDeleteStockAdjustment} | " +
                $"Transfer: C={CanCreateStockTransfer},U={CanEditStockTransfer},D={CanDeleteStockTransfer} | " +
                $"GRN: C={CanCreateGrn},U={CanEditGrn},D={CanDeleteGrn} | " +
                $"Return: C={CanCreateGoodsReturn},U={CanEditGoodsReturn},D={CanDeleteGoodsReturn} | " +
                $"Replace: C={CanCreateGoodsReplace},U={CanEditGoodsReplace},D={CanDeleteGoodsReplace}",
                "viewmodel");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to initialize Stock Management permissions", ex, "viewmodel");
            // Fail-secure: all permissions default to false
        }
    }

    /// <summary>
    /// Initialize modules synchronously for immediate UI display
    /// </summary>
    private void InitializeModulesSync()
    {
        var modules = new List<StockModuleInfo>
        {
            new StockModuleInfo
            {
                Title = "Stock Adjustment", // Default text, will be updated by async localization
                ModuleType = "StockAdjustment",
                ItemCount = 125,
                ItemCountLabel = "Items",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush(),
                IsSelected = IsStockAdjustmentSelected
            },
            new StockModuleInfo
            {
                Title = "Stock Transfer", // Default text, will be updated by async localization
                ModuleType = "StockTransfer", 
                ItemCount = 32,
                ItemCountLabel = "Transfers",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush(),
                IsSelected = IsStockTransferSelected
            },
            new StockModuleInfo
            {
                Title = "Goods Received", // Default text, will be updated by async localization
                ModuleType = "GoodsReceived",
                ItemCount = 67,
                ItemCountLabel = "Receipts",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush(),
                IsSelected = IsGoodsReceivedSelected
            },
            new StockModuleInfo
            {
                Title = "Goods Return", // Default text, will be updated by async localization
                ModuleType = "GoodsReturn",
                ItemCount = 15,
                ItemCountLabel = "Returns",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush(),
                IsSelected = IsGoodsReturnSelected
            },
            new StockModuleInfo
            {
                Title = "Goods Replace", // Default text, will be updated by async localization
                ModuleType = "GoodsReplace",
                ItemCount = 8,
                ItemCountLabel = "Replacements",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush(),
                IsSelected = IsGoodsReplaceSelected
            }
        };

        Modules.Clear();
        foreach (var module in modules)
        {
            Modules.Add(module);
        }
    }

    /// <summary>
    /// Load async data in background without affecting UI initialization
    /// </summary>
    private async Task LoadAsyncDataInBackground()
    {
        try
        {
            // Load localized titles in background
            await UpdateModuleTitlesAsync();
            
            // Load adjustment reasons
            await LoadAdjustmentReasonsAsync();
            
            // Load stock adjustments data
            await LoadStockAdjustmentsAsync();
            
            // Load stock transfers data
            await LoadStockTransfersAsync();
            
            // Load GRN supplier filters
            await LoadGrnSupplierFiltersAsync();
        }
        catch (Exception ex)
        {
            LogMessage($"Error loading background data: {ex.Message}");
        }
    }

    /// <summary>
    /// Update module titles with localized text
    /// </summary>
    private async Task UpdateModuleTitlesAsync()
    {
        if (Modules.Count >= 4)
        {
            Modules[0].Title = await _databaseLocalizationService.GetTranslationAsync("stock.adjustment");
            Modules[1].Title = await _databaseLocalizationService.GetTranslationAsync("stock.transfer");
            Modules[2].Title = await _databaseLocalizationService.GetTranslationAsync("stock.goods_received");
            Modules[3].Title = await _databaseLocalizationService.GetTranslationAsync("stock.goods_return");
        }
    }

    /// <summary>
    /// Initialize all settings with current values
    /// </summary>
    private void InitializeSettings()
    {
        CurrentTheme = _themeService.CurrentTheme.ToString();
        CurrentZoom = (int)_zoomService.CurrentZoomLevel;
        CurrentLanguage = _localizationService.CurrentLanguage.ToString() ?? "English";
        CurrentColorScheme = _colorSchemeService.CurrentPrimaryColor?.Name ?? "Blue";
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        CurrentFontFamily = "Segoe UI"; // Default font family
        CurrentFontSize = GetFontSizeValue(_fontService.CurrentFontSize);
    }

    /// <summary>
    /// Load stock module data (now just calls the initialization methods)
    /// </summary>
    private async Task LoadModuleDataAsync()
    {
        // Refresh modules with latest data
        InitializeModulesSync();
        
        // Update with localized titles
        await UpdateModuleTitlesAsync();
        
        // Load adjustment reasons
        await LoadAdjustmentReasonsAsync();
        
        // Load stock adjustments
        await LoadStockAdjustmentsAsync();
        
        // Load stock transfers
        await LoadStockTransfersAsync();
    }

    /// <summary>
    /// Load stock adjustment reasons from database
    /// </summary>
    private async Task LoadAdjustmentReasonsAsync()
    {
        try
        {
            if (_stockAdjustmentService != null)
            {
                var reasons = await _stockAdjustmentService.GetAdjustmentReasonsAsync();
                AdjustmentReasons.Clear();
                foreach (var reason in reasons)
                {
                    AdjustmentReasons.Add(reason);
                }
            }
            else
            {
                // Add default reasons if service is not available
                AdjustmentReasons.Clear();
                AdjustmentReasons.Add(new StockAdjustmentReasonDto { Id = 1, Name = "Stock count correction" });
                AdjustmentReasons.Add(new StockAdjustmentReasonDto { Id = 2, Name = "Damaged items" });
                AdjustmentReasons.Add(new StockAdjustmentReasonDto { Id = 3, Name = "Expired products" });
                AdjustmentReasons.Add(new StockAdjustmentReasonDto { Id = 4, Name = "Theft/Loss" });
                AdjustmentReasons.Add(new StockAdjustmentReasonDto { Id = 5, Name = "New stock addition" });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading adjustment reasons: {ex.Message}");
        }
    }

    /// <summary>
    /// Perform the actual product search with database integration
    /// </summary>
    private async Task PerformSearchAsync(string searchTerm)
    {
        try
        {
            DesktopFileLogger.LogSeparator("UNIFIED SEARCH START");
            DesktopFileLogger.Log($"Search term: '{searchTerm}'");
            DesktopFileLogger.Log($"Adjustment mode: {AdjustProduct.AdjustmentMode}");
            DesktopFileLogger.Log($"StockAdjustmentService is null: {_stockAdjustmentService == null}");
            DesktopFileLogger.Log($"SearchResults count before: {SearchResults.Count}");
            
            // Don't clear results if we have a selected item and the search term matches
            if (AdjustProduct.SelectedSearchItem != null && 
                !string.IsNullOrEmpty(AdjustProduct.SelectedSearchItem.Name) &&
                searchTerm.Equals(AdjustProduct.SelectedSearchItem.Name, StringComparison.OrdinalIgnoreCase))
            {
                DesktopFileLogger.Log("SKIPPING SEARCH: Search term matches selected item, keeping current results");
                return;
            }
            
            // Store current selection to restore later if it matches new results
            var currentSelection = AdjustProduct.SelectedSearchItem;
            
            // CRITICAL FIX: Temporarily disable selection binding to prevent WPF from clearing it
            var tempSelection = AdjustProduct.SelectedSearchItem;
            
            DesktopFileLogger.Log("CLEARING SearchResults...");
            SearchResults.Clear();
            DesktopFileLogger.Log($"SearchResults cleared, count: {SearchResults.Count}");
            
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
            {
                DesktopFileLogger.Log("Search term too short, returning empty results");
                return;
            }

            if (_stockAdjustmentService != null)
            {
                DesktopFileLogger.Log($"Calling unified search for mode: {AdjustProduct.AdjustmentMode}...");
                
                // Use the new unified search method
                var searchItems = await _stockAdjustmentService.SearchForStockAdjustmentAsync(
                    searchTerm, 
                    AdjustProduct.AdjustmentMode, 
                    10); // Limit to 10 results for dropdown
                
                DesktopFileLogger.Log($"Search items returned: {searchItems?.Count ?? 0}");
                
                if (searchItems != null)
                {
                    foreach (var item in searchItems)
                    {
                        SearchResults.Add(item);
                        DesktopFileLogger.Log($"Added to SearchResults: {item.SearchDisplayText}");
                    }
                }
                
                DesktopFileLogger.Log($"Final SearchResults count: {SearchResults.Count}");
                
                // CRITICAL FIX: Restore selection immediately after adding items, before UI update
                if (currentSelection != null)
                {
                    var matchingItem = SearchResults.FirstOrDefault(r => 
                        r.Id == currentSelection.Id && 
                        r.Mode == currentSelection.Mode);
                        
                    if (matchingItem != null)
                    {
                        DesktopFileLogger.Log($"Restoring selection: {matchingItem.Name}");
                        // Use Dispatcher to ensure this happens after UI update
                        App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                            AdjustProduct.SelectedSearchItem = matchingItem;
                            DesktopFileLogger.Log($"Selection restored via Dispatcher: {matchingItem.Name}");
                        }));
                    }
                    else
                    {
                        DesktopFileLogger.Log($"Previous selection '{currentSelection.Name}' not found in new results");
                    }
                }
                
                // Force UI update
                OnPropertyChanged(nameof(SearchResults));
                DesktopFileLogger.Log("UI PropertyChanged event fired for SearchResults");
            }
            else
            {
                DesktopFileLogger.Log("ERROR: StockAdjustmentService is null!");
            }
            
            DesktopFileLogger.LogSeparator("UNIFIED SEARCH END");
        }
        catch (Exception ex)
        {
            DesktopFileLogger.Log($"UNIFIED SEARCH ERROR: {ex.Message}");
            DesktopFileLogger.Log($"Stack trace: {ex.StackTrace}");
            SearchResults.Clear();
        }
    }

    /// <summary>
    /// Load current stock level for selected search item
    /// </summary>
    private async Task LoadCurrentStockForSelectedItem(StockAdjustmentSearchItemDto selectedItem)
    {
        try
        {
            DesktopFileLogger.Log($"Loading stock for: {selectedItem.Name} (Mode: {selectedItem.Mode})");
            
            if (selectedItem.Mode == StockAdjustmentMode.Product)
            {
                // For product mode, use the current quantity directly
                AdjustProduct.CurrentStock = selectedItem.CurrentQuantity;
                DesktopFileLogger.Log($"Product mode - Current stock set to: {selectedItem.CurrentQuantity}");
            }
            else if (selectedItem.Mode == StockAdjustmentMode.ProductUnit)
            {
                // For product unit mode, use the quantity in unit
                AdjustProduct.CurrentStock = selectedItem.CurrentQuantity;
                DesktopFileLogger.Log($"ProductUnit mode - Current stock set to: {selectedItem.CurrentQuantity} (QtyInUnit)");
            }
        }
        catch (Exception ex)
        {
            DesktopFileLogger.Log($"Error loading stock for selected item: {ex.Message}");
        }
    }

    /// <summary>
    /// Load current stock level for selected product (legacy method)
    /// </summary>
    private async Task LoadCurrentStockForSelectedProduct()
    {
        try
        {
            DesktopFileLogger.LogSeparator("LOAD STOCK");
            DesktopFileLogger.Log($"Selected Product: {AdjustProduct.SelectedProduct?.Name ?? "NULL"}");
            
            if (AdjustProduct.SelectedProduct != null)
            {
                DesktopFileLogger.Log($"Loading stock for product: {AdjustProduct.SelectedProduct.Name} (ID: {AdjustProduct.SelectedProduct.Id})");
                
                // TODO: Load actual stock level from database
                // For now, simulate with product stock quantity
                var currentStock = (decimal)AdjustProduct.SelectedProduct.StockQuantity;
                AdjustProduct.CurrentStock = currentStock;
                AdjustProduct.NewQuantity = currentStock; // Initialize with current stock
                
                DesktopFileLogger.Log($"Current Stock set to: {currentStock}");
                DesktopFileLogger.Log($"New Quantity initialized to: {currentStock}");
            }
            else
            {
                DesktopFileLogger.Log("No product selected - skipping stock load");
            }
            
            DesktopFileLogger.LogSeparator("LOAD STOCK END");
        }
        catch (Exception ex)
        {
            DesktopFileLogger.Log($"ERROR loading current stock: {ex.Message}");
            DesktopFileLogger.Log($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Load product batches for the selected product
    /// </summary>
    private async Task LoadProductBatchesAsync(int productId)
    {
        try
        {
            DesktopFileLogger.Log($"Loading product batches for product ID: {productId}");
            
            if (_productBatchService != null && productId > 0)
            {
                var batches = await _productBatchService.GetProductBatchesByProductIdAsync(productId);
                AdjustProduct.AvailableBatches = batches.ToList();
                
                // Auto-select first batch if available
                if (batches.Any())
                {
                    AdjustProduct.SelectedBatch = batches.First();
                }
                
                DesktopFileLogger.Log($"Loaded {batches.Count} batches for product {productId}");
            }
            else
            {
                DesktopFileLogger.Log("No ProductBatch service available or invalid product ID");
                AdjustProduct.AvailableBatches = new List<ProductBatchDto>();
                AdjustProduct.SelectedBatch = null;
            }
        }
        catch (Exception ex)
        {
            DesktopFileLogger.Log($"ERROR loading product batches: {ex.Message}");
            AdjustProduct.AvailableBatches = new List<ProductBatchDto>();
            AdjustProduct.SelectedBatch = null;
        }
    }

    /// <summary>
    /// Load initial search results when dropdown is opened (without search text)
    /// </summary>
    public async Task LoadInitialSearchResultsAsync()
    {
        AppLogger.LogSeparator("LOADING INITIAL SEARCH RESULTS", "stock_adjustment");
        AppLogger.LogInfo($"LoadInitialSearchResultsAsync called", 
            $"SearchResults.Count: {SearchResults.Count}, StockAdjustmentService null: {_stockAdjustmentService == null}", "stock_adjustment");
        
        // Force loading regardless of current SearchResults count to test
        if (_stockAdjustmentService != null)
        {
            DesktopFileLogger.LogSeparator("LOADING INITIAL SEARCH RESULTS");
            DesktopFileLogger.Log("Loading all products/units for dropdown display");
            DesktopFileLogger.Log($"Adjustment mode: {AdjustProduct.AdjustmentMode}");
            DesktopFileLogger.Log($"StockAdjustmentService is null: {_stockAdjustmentService == null}");
            
            AppLogger.LogInfo("Starting search for initial products", 
                $"Mode: {AdjustProduct.AdjustmentMode}, Using search term: 'pr' (2 chars minimum required)", "stock_adjustment");
            
            try
            {
                // Use a 2-character search term to meet the minimum length requirement from PerformSearchAsync
                // We'll try common 2-letter combinations that are likely to match products
                var allItems = await _stockAdjustmentService.SearchForStockAdjustmentAsync(
                    "pr", // Use "pr" for "product" - should match many items
                    AdjustProduct.AdjustmentMode, 
                    50); // Load more items initially (50 instead of 10)
                
                DesktopFileLogger.Log($"Initial search items returned: {allItems?.Count ?? 0}");
                
                // New AppLogger usage
                AppLogger.LogInfo($"Search completed - items found: {allItems?.Count ?? 0}", 
                    $"Search term: 'pr', Mode: {AdjustProduct.AdjustmentMode}", "stock_adjustment");
                
                // Try with different search terms if "pr" returns nothing
                if ((allItems == null || allItems.Count == 0))
                {
                    AppLogger.LogWarning("No items found with 'pr', trying with other 2-character terms", "Testing different search approaches", "stock_adjustment");
                    
                    // Try with different 2-character combinations that might match products
                    var testTerms = new[] { "bu", "te", "it", "ro", "co", "pa", "de" };
                    foreach (var term in testTerms)
                    {
                        allItems = await _stockAdjustmentService.SearchForStockAdjustmentAsync(
                            term, AdjustProduct.AdjustmentMode, 50);
                        AppLogger.LogInfo($"Search with '{term}' returned: {allItems?.Count ?? 0} items", "stock_adjustment");
                        if (allItems != null && allItems.Count > 0)
                        {
                            AppLogger.LogInfo($"Found products using search term: '{term}'", "stock_adjustment");
                            break;
                        }
                    }
                }
                
                if (allItems != null && allItems.Count > 0)
                {
                    SearchResults.Clear();
                    AppLogger.LogInfo($"Clearing SearchResults and adding {allItems.Count} items", "stock_adjustment");
                    
                    foreach (var item in allItems)
                    {
                        SearchResults.Add(item);
                        DesktopFileLogger.Log($"Added to initial SearchResults: {item.SearchDisplayText}");
                        AppLogger.LogDebug($"Added item: {item.SearchDisplayText}", $"Name: {item.Name}, ID: {item.Id}", "stock_adjustment");
                    }
                    
                    OnPropertyChanged(nameof(SearchResults));
                    DesktopFileLogger.Log($"Initial SearchResults loaded: {SearchResults.Count} items");
                    AppLogger.LogInfo($"Initial SearchResults populated successfully", 
                        $"Final count: {SearchResults.Count}, PropertyChanged event fired", "stock_adjustment");
                }
                else
                {
                    DesktopFileLogger.Log("No items returned from initial search");
                    AppLogger.LogWarning("No items found for initial search", 
                        $"All search attempts returned empty results. Mode: {AdjustProduct.AdjustmentMode}", "stock_adjustment");
                }
            }
            catch (Exception ex)
            {
                DesktopFileLogger.Log($"Error loading initial search results: {ex.Message}");
                AppLogger.LogError("Exception in LoadInitialSearchResultsAsync", ex, 
                    $"Mode: {AdjustProduct.AdjustmentMode}", "stock_adjustment");
            }
        }
        else
        {
            AppLogger.LogWarning("LoadInitialSearchResultsAsync skipped", 
                "_stockAdjustmentService is null - service not available", "stock_adjustment");
            DesktopFileLogger.Log($"Skipping initial load - SearchResults.Count: {SearchResults.Count}, StockAdjustmentService is null: {_stockAdjustmentService == null}");
        }
    }

    /// <summary>
    /// Load stock transfers data
    /// </summary>
    private async Task LoadStockTransfersAsync()
    {
        try
        {
            AppLogger.LogInfo("LoadStockTransfersAsync", 
                $"Starting to load stock transfers. Service is null: {_stockTransferService == null}. Current collection count: {StockTransfers.Count}", 
                "stock_transfer");
                
            StockTransfers.Clear();
            
            AppLogger.LogInfo("StockTransfersCleared", 
                $"Collection cleared. Current count: {StockTransfers.Count}", 
                "stock_transfer");
            
            if (_stockTransferService != null)
            {
                // Load actual data from service with search filter, status filter, and date range
                var searchTerm = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
                var statusFilter = SelectedStatus == "All" ? null : SelectedStatus;
                var result = await _stockTransferService.GetStockTransfersAsync(
                    page: 1, 
                    pageSize: 100, 
                    searchTerm: searchTerm,
                    status: statusFilter,
                    fromDate: StartDate,
                    toDate: EndDate);
                
                AppLogger.LogInfo("StockTransfersLoaded", 
                    $"Loaded {result.Items.Count()} stock transfers, Total: {result.TotalCount} (Status Filter: '{statusFilter ?? "None"}', Search: '{searchTerm ?? "None"}')", 
                    "stock_transfer");
                
                foreach (var transfer in result.Items)
                {
                    AppLogger.LogInfo("AddingStockTransfer", 
                        $"Adding transfer: {transfer.TransferNo} - {transfer.FromStoreName} to {transfer.ToStoreName}", 
                        "stock_transfer");
                    StockTransfers.Add(transfer);
                }
                
                AppLogger.LogInfo("StockTransfersCollection", 
                    $"Final collection count: {StockTransfers.Count}", 
                    "stock_transfer");
            }
            else
            {
                // Fallback to dummy data if service is not available
                AppLogger.LogWarning("StockTransferService unavailable", "Using dummy data for stock transfers", "stock_transfer");
                
                var dummyTransfers = new List<StockTransferDto>
                {
                    new StockTransferDto
                    {
                        TransferId = 1,
                        TransferNo = "ST00001",
                        TransferDate = DateTime.Today.AddDays(-5),
                        FromStoreName = "Main Warehouse",
                        ToStoreName = "Retail Store A",
                        Status = "Completed",
                        TotalItems = 5,
                        CreatedByName = "John Doe",
                        CreatedAt = DateTime.Today.AddDays(-5),
                        Remarks = "Weekly stock replenishment"
                    },
                    new StockTransferDto
                    {
                        TransferId = 2,
                        TransferNo = "ST00002",
                        TransferDate = DateTime.Today.AddDays(-3),
                        FromStoreName = "Retail Store A",
                        ToStoreName = "Retail Store B",
                        Status = "Pending",
                        TotalItems = 3,
                        CreatedByName = "Jane Smith",
                        CreatedAt = DateTime.Today.AddDays(-3),
                        Remarks = "Product redistribution"
                    }
                };

                foreach (var transfer in dummyTransfers)
                {
                    StockTransfers.Add(transfer);
                }
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("LoadStockTransfersAsync", 
                ex, 
                $"Error loading stock transfers: {ex.Message}", 
                "stock_transfer");
        }
    }

    /// <summary>
    /// Load goods returns data
    /// </summary>
    private async Task LoadGoodsReturnsAsync()
    {
        try
        {
            var searchTerm = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
            var statusFilter = SelectedStatus == "All" ? null : SelectedStatus;
            var startDate = StartDate;
            var endDate = EndDate;
            
            AppLogger.LogInfo("LoadGoodsReturnsAsync", 
                $"Starting to load goods returns with filters - SearchTerm: '{searchTerm}', StatusFilter: '{statusFilter}', StartDate: {startDate}, EndDate: {endDate}. Service is null: {_goodsReturnService == null}. Current collection count: {GoodsReturns.Count}", 
                "goods_return");
                
            GoodsReturns.Clear();
            
            AppLogger.LogInfo("GoodsReturnsCleared", 
                $"Collection cleared. Current count: {GoodsReturns.Count}", 
                "goods_return");
            
            if (_goodsReturnService != null)
            {
                // Load actual data from service with search filter, status filter, and date range
                var result = await _goodsReturnService.GetGoodsReturnsAsync(
                    searchTerm: searchTerm,
                    status: statusFilter,
                    startDate: startDate,
                    endDate: endDate,
                    supplierId: null);
                
                var goodsReturnsList = result.ToList();
                AppLogger.LogInfo("GoodsReturnsLoaded", 
                    $"Loaded {goodsReturnsList.Count} goods returns (Status Filter: '{statusFilter ?? "None"}', Search: '{searchTerm ?? "None"}')", 
                    "goods_return");
                
                foreach (var goodsReturn in goodsReturnsList)
                {
                    AppLogger.LogInfo("AddingGoodsReturn", 
                        $"Adding return: {goodsReturn.ReturnNo} - {goodsReturn.SupplierName} (Store: {goodsReturn.StoreName})", 
                        "goods_return");
                    GoodsReturns.Add(goodsReturn);
                }
                
                AppLogger.LogInfo("GoodsReturnsCollection", 
                    $"Final collection count: {GoodsReturns.Count}", 
                    "goods_return");
            }
            else
            {
                // Add dummy data for testing when service is not available
                AppLogger.LogWarning("GoodsReturnServiceNull", 
                    "GoodsReturnService is null, adding dummy data", 
                    "goods_return");

                var dummyReturns = new List<GoodsReturnDto>
                {
                    new GoodsReturnDto
                    {
                        Id = 1,
                        ReturnNo = "GR-2025-0001",
                        SupplierName = "ABC Suppliers Ltd.",
                        StoreName = "Main Store",
                        ReturnDate = DateTime.Today.AddDays(-1),
                        Status = "Posted",
                        TotalAmount = 1250.50m,
                        TotalItems = 5,
                        CreatedByName = "John Doe",
                        CreatedAt = DateTime.Today.AddDays(-1),
                        Remarks = "Damaged products return"
                    },
                    new GoodsReturnDto
                    {
                        Id = 2,
                        ReturnNo = "GR-2025-0002",
                        SupplierName = "XYZ Trading Co.",
                        StoreName = "Branch Store",
                        ReturnDate = DateTime.Today.AddDays(-2),
                        Status = "Pending",
                        TotalAmount = 850.75m,
                        TotalItems = 3,
                        CreatedByName = "Jane Smith",
                        CreatedAt = DateTime.Today.AddDays(-2),
                        Remarks = "Wrong product delivered"
                    },
                    new GoodsReturnDto
                    {
                        Id = 3,
                        ReturnNo = "GR-2025-0003",
                        SupplierName = "PQR Enterprises",
                        StoreName = "Main Store",
                        ReturnDate = DateTime.Today.AddDays(-5),
                        Status = "Cancelled",
                        TotalAmount = 2100.00m,
                        TotalItems = 8,
                        CreatedByName = "Mike Johnson",
                        CreatedAt = DateTime.Today.AddDays(-5),
                        Remarks = "Supplier refused return"
                    }
                };

                foreach (var goodsReturn in dummyReturns)
                {
                    GoodsReturns.Add(goodsReturn);
                }
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("LoadGoodsReturnsAsync", 
                ex, 
                $"Error loading goods returns: {ex.Message}", 
                "goods_return");
        }
    }

    /// <summary>
    /// Load goods replaces data
    /// </summary>
    private async Task LoadGoodsReplacesAsync()
    {
        try
        {
            var searchTerm = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
            var statusFilter = SelectedStatus == "All" ? null : SelectedStatus;
            var startDate = StartDate;
            var endDate = EndDate;
            
            AppLogger.LogInfo("LoadGoodsReplacesAsync", 
                $"Starting to load goods replaces with filters - SearchTerm: '{searchTerm}', StatusFilter: '{statusFilter}', StartDate: {startDate}, EndDate: {endDate}. Service is null: {_goodsReplaceService == null}. Current collection count: {GoodsReplaces.Count}", 
                "goods_replace");
                
            GoodsReplaces.Clear();
            
            AppLogger.LogInfo("GoodsReplacesCleared", 
                $"Collection cleared. Current count: {GoodsReplaces.Count}", 
                "goods_replace");
            
            if (_goodsReplaceService != null)
            {
                // Load actual data from service with search filter, status filter, and date range
                var result = await _goodsReplaceService.GetGoodsReplacesAsync(
                    page: 1,
                    pageSize: 1000,
                    searchTerm: searchTerm,
                    status: statusFilter,
                    fromDate: startDate,
                    toDate: endDate);
                
                var goodsReplacesList = result.Items.ToList();
                AppLogger.LogInfo("GoodsReplacesLoaded", 
                    $"Loaded {goodsReplacesList.Count} goods replaces (Status Filter: '{statusFilter ?? "None"}', Search: '{searchTerm ?? "None"}')", 
                    "goods_replace");
                
                foreach (var goodsReplace in goodsReplacesList)
                {
                    AppLogger.LogInfo("AddingGoodsReplace", 
                        $"Adding replace: {goodsReplace.ReplaceNo} - {goodsReplace.SupplierName} (Store: {goodsReplace.StoreName})", 
                        "goods_replace");
                    GoodsReplaces.Add(goodsReplace);
                }
                
                AppLogger.LogInfo("GoodsReplacesCollection", 
                    $"Final collection count: {GoodsReplaces.Count}", 
                    "goods_replace");
            }
            else
            {
                AppLogger.LogWarning("GoodsReplaceServiceNull", 
                    "GoodsReplaceService is null, no data loaded", 
                    "goods_replace");
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("LoadGoodsReplacesAsync", 
                ex, 
                $"Error loading goods replaces: {ex.Message}", 
                "goods_replace");
        }
    }

    /// <summary>
    /// Load stock adjustments data with financial calculations
    /// </summary>
    private async Task LoadStockAdjustmentsAsync()
    {
        try
        {
            if (_stockAdjustmentService != null)
            {
                // Load actual data from service with search filter and date range
                var searchTerm = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
                var result = await _stockAdjustmentService.GetStockAdjustmentsAsync(1, 100, searchTerm, null, null, StartDate, EndDate);
                
                StockAdjustments.Clear();
                StockAdjustmentItems.Clear();
                
                foreach (var adjustment in result.Items)
                {
                    StockAdjustments.Add(adjustment);
                    
                    // Get detailed adjustment with items
                    var detailedAdjustment = await _stockAdjustmentService.GetStockAdjustmentByIdAsync(adjustment.AdjustmentId);
                    if (detailedAdjustment?.Items != null)
                    {
                        foreach (var item in detailedAdjustment.Items)
                        {
                            // Add adjustment info to each item for the flattened view
                            item.AdjustmentId = detailedAdjustment.AdjustmentId;
                            // Add to flattened collection
                            StockAdjustmentItems.Add(item);
                        }
                    }
                }
            }
            else
            {
                // Fallback with sample data for testing
                StockAdjustmentItems.Clear();
                
                var sampleItems = new List<StockAdjustmentItemDto>
                {
                    new StockAdjustmentItemDto
                    {
                        Id = 1,
                        AdjustmentId = 1,
                        ProductId = 1,
                        ProductName = "Cheese Burger",
                        ProductSku = "CHB001",
                        UomName = "PCS",
                        QuantityBefore = 50,
                        QuantityAfter = 45,
                        DifferenceQty = -5,
                        CostPrice = 5.50m,
                        TaxRate = 0.10m, // 10% tax
                        ReasonLine = "Stock Count Adjustment",
                        RemarksLine = "Monthly inventory count",
                        AdjustmentNo = "ADJ202509080001",
                        AdjustmentDate = DateTime.Today.AddDays(-1)
                    },
                    new StockAdjustmentItemDto
                    {
                        Id = 2,
                        AdjustmentId = 2,
                        ProductId = 2,
                        ProductName = "French Fries",
                        ProductSku = "FF001",
                        UomName = "PCS",
                        QuantityBefore = 30,
                        QuantityAfter = 35,
                        DifferenceQty = 5,
                        CostPrice = 3.25m,
                        TaxRate = 0.10m, // 10% tax
                        ReasonLine = "Supplier Delivery",
                        RemarksLine = "Weekly stock replenishment",
                        AdjustmentNo = "ADJ202509080002",
                        AdjustmentDate = DateTime.Today
                    }
                };
                
                foreach (var item in sampleItems)
                {
                    StockAdjustmentItems.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"[LoadStockAdjustmentsAsync] ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Load goods received notes data from database
    /// </summary>
    private async Task LoadGoodsReceivedNotesAsync()
    {
        try
        {
            AppLogger.LogInfo("Loading Goods Received Notes", 
                $"SearchTerm: '{GrnSearchTerm}', StatusFilter: '{SelectedGrnStatusFilter}', SupplierFilter: {SelectedGrnSupplierId}", 
                "stock_management");

            if (_goodsReceivedService != null)
            {
                // Apply search filter
                var searchTerm = string.IsNullOrWhiteSpace(GrnSearchTerm) ? null : GrnSearchTerm.Trim();
                
                // Apply status filter
                string? statusFilter = null;
                if (SelectedGrnStatusFilter != "All")
                {
                    statusFilter = SelectedGrnStatusFilter;
                }

                // Apply supplier filter
                long? supplierIdFilter = null;
                if (SelectedGrnSupplierId.HasValue && SelectedGrnSupplierId.Value > 0)
                {
                    supplierIdFilter = SelectedGrnSupplierId.Value;
                }

                // Load GRN data from service (basic implementation)
                var result = await _goodsReceivedService.GetPagedAsync(
                    page: 1, 
                    pageSize: 100, 
                    searchTerm: searchTerm
                );
                
                GoodsReceivedNotes.Clear();
                var grns = result.ToList();
                
                // Apply status filter in memory (since service doesn't support it yet)
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    grns = grns.Where(g => g.Status == statusFilter).ToList();
                }
                
                // Apply supplier filter in memory (since service doesn't support it yet)
                if (supplierIdFilter.HasValue)
                {
                    grns = grns.Where(g => g.SupplierId == supplierIdFilter.Value).ToList();
                }
                
                foreach (var grn in grns)
                {
                    GoodsReceivedNotes.Add(grn);
                }
                
                HasNoGrns = GoodsReceivedNotes.Count == 0;
                
                AppLogger.LogInfo($"Loaded {GoodsReceivedNotes.Count} GRN records from database", 
                    $"Applied filters - Status: {statusFilter}, Supplier: {supplierIdFilter}", "stock_management");
            }
            else
            {
                // Fallback to sample data if service is not available
                AppLogger.LogWarning("GoodsReceivedService not available, using sample data", 
                    "Service injection may have failed", "stock_management");
                
                LoadSampleGrnData();
            }
        }
        catch (Exception ex)
        {
            LogMessage($"[LoadGoodsReceivedNotesAsync] ERROR: {ex.Message}");
            AppLogger.LogError("Failed to load Goods Received Notes", ex, 
                $"SearchTerm: '{GrnSearchTerm}', StatusFilter: '{SelectedGrnStatusFilter}'", "stock_management");
            
            // Fallback to sample data on error
            LoadSampleGrnData();
            HasNoGrns = GoodsReceivedNotes.Count == 0;
        }
    }

    /// <summary>
    /// Load sample GRN data for testing/fallback
    /// </summary>
    private void LoadSampleGrnData()
    {
        GoodsReceivedNotes.Clear();
        
        var sampleGrns = new List<GoodsReceivedDto>
        {
            new GoodsReceivedDto
            {
                Id = 1,
                GrnNo = "GRN202509290001",
                ReceivedDate = DateTime.Today.AddDays(-2),
                SupplierName = "ABC Suppliers Ltd",
                StoreName = "Main Warehouse",
                InvoiceNo = "INV-2024-001",
                TotalAmount = 1250.75m,
                Status = "Posted",
                CreatedAt = DateTime.Today.AddDays(-2)
            },
            new GoodsReceivedDto
            {
                Id = 2,
                GrnNo = "GRN202509290002",
                ReceivedDate = DateTime.Today.AddDays(-1),
                SupplierName = "XYZ Trading Co",
                StoreName = "Retail Store A",
                InvoiceNo = "XYZ-INV-789",
                TotalAmount = 875.50m,
                Status = "Draft",
                CreatedAt = DateTime.Today.AddDays(-1)
            },
            new GoodsReceivedDto
            {
                Id = 3,
                GrnNo = "GRN202509290003",
                ReceivedDate = DateTime.Today,
                SupplierName = "Quality Foods Inc",
                StoreName = "Main Warehouse",
                InvoiceNo = "QF-2024-456",
                TotalAmount = 2100.25m,
                Status = "Posted",
                CreatedAt = DateTime.Today
            }
        };
        
        // Apply search filter to sample data
        if (!string.IsNullOrWhiteSpace(GrnSearchTerm))
        {
            var searchLower = GrnSearchTerm.ToLower();
            sampleGrns = sampleGrns.Where(g => 
                g.GrnNo.ToLower().Contains(searchLower) ||
                (g.InvoiceNo?.ToLower().Contains(searchLower) == true) ||
                (g.SupplierName?.ToLower().Contains(searchLower) == true)
            ).ToList();
        }
        
        // Apply status filter to sample data
        if (SelectedGrnStatusFilter != "All")
        {
            sampleGrns = sampleGrns.Where(g => g.Status == SelectedGrnStatusFilter).ToList();
        }
        
        foreach (var grn in sampleGrns)
        {
            GoodsReceivedNotes.Add(grn);
        }
        
        HasNoGrns = GoodsReceivedNotes.Count == 0;
    }

    /// <summary>
    /// Load supplier filters for GRN filtering
    /// </summary>
    private async Task LoadGrnSupplierFiltersAsync()
    {
        try
        {
            AppLogger.LogInfo("Loading GRN supplier filters", filename: "stock_management");
            
            GrnSupplierFilters.Clear();
            GrnSupplierFilters.Add(new SupplierDto { SupplierId = 0, CompanyName = "All Suppliers" });
            
            if (_supplierService != null)
            {
                AppLogger.LogInfo("Supplier service available, loading real suppliers", filename: "stock_management");
                
                var suppliers = await _supplierService.GetAllAsync();
                AppLogger.LogInfo($"Loaded {suppliers?.Count() ?? 0} suppliers from database", filename: "stock_management");
                
                if (suppliers != null && suppliers.Any())
                {
                    foreach (var supplier in suppliers.OrderBy(s => s.CompanyName))
                    {
                        GrnSupplierFilters.Add(supplier);
                        AppLogger.LogInfo($"Added supplier: {supplier.CompanyName} (ID: {supplier.SupplierId})", filename: "stock_management");
                    }
                }
                else
                {
                    AppLogger.LogWarning("No suppliers found in database", filename: "stock_management");
                }
            }
            else
            {
                AppLogger.LogWarning("Supplier service not available, using fallback suppliers", filename: "stock_management");
                // Use fallback suppliers only when service is not available
                GrnSupplierFilters.Add(new SupplierDto { SupplierId = 1, CompanyName = "ABC Suppliers Ltd" });
                GrnSupplierFilters.Add(new SupplierDto { SupplierId = 2, CompanyName = "XYZ Trading Co" });
                GrnSupplierFilters.Add(new SupplierDto { SupplierId = 3, CompanyName = "Quality Foods Inc" });
            }
            
            AppLogger.LogInfo($"GRN supplier filters loaded successfully. Total count: {GrnSupplierFilters.Count}", filename: "stock_management");
        }
        catch (Exception ex)
        {
            LogMessage($"[LoadGrnSupplierFiltersAsync] ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Get the primary color brush from color scheme service
    /// </summary>
    private Brush GetPrimaryColorBrush()
    {
        return new SolidColorBrush(_colorSchemeService.CurrentPrimaryColor?.Color ?? Colors.DodgerBlue);
    }

    /// <summary>
    /// Get the button background brush based on current theme
    /// </summary>
    private Brush GetButtonBackgroundBrush()
    {
        return _themeService.CurrentTheme == Theme.Dark 
            ? new SolidColorBrush(Color.FromRgb(45, 45, 48))
            : new SolidColorBrush(Colors.White);
    }

    #endregion

    #region Event Handlers

    private void OnThemeChanged(Theme newTheme)
    {
        CurrentTheme = newTheme.ToString();
        
        // Update button backgrounds for all modules
        foreach (var module in Modules)
        {
            module.ButtonBackground = GetButtonBackgroundBrush();
        }
    }

    private void OnZoomChanged(ZoomLevel newZoom)
    {
        CurrentZoom = (int)newZoom;
        // UI will automatically respond to zoom changes through resource bindings
    }

    private void OnLanguageChanged(SupportedLanguage newLanguage)
    {
        CurrentLanguage = newLanguage.ToString();
    }

    private void OnPrimaryColorChanged(ColorOption newColor)
    {
        CurrentColorScheme = newColor.Name;
        
        // Update all module icons to use the same new primary color
        var newPrimaryColorBrush = new SolidColorBrush(newColor.Color);
        foreach (var module in Modules)
        {
            module.IconBackground = newPrimaryColorBrush;
        }
    }

    private void OnDirectionChanged(LayoutDirection newDirection)
    {
        CurrentFlowDirection = newDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    private async void OnDatabaseLanguageChanged(object? sender, string newLanguageCode)
    {
        // Reload module data with new language
        await LoadModuleDataAsync();
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Cleanup event subscriptions
    /// </summary>
    ~StockManagementViewModel()
    {
        // Unsubscribe from events
        if (_themeService != null)
            _themeService.ThemeChanged -= OnThemeChanged;
        if (_zoomService != null)
            _zoomService.ZoomChanged -= OnZoomChanged;
        if (_localizationService != null)
            _localizationService.LanguageChanged -= OnLanguageChanged;
        if (_colorSchemeService != null)
            _colorSchemeService.PrimaryColorChanged -= OnPrimaryColorChanged;
        if (_layoutDirectionService != null)
            _layoutDirectionService.DirectionChanged -= OnDirectionChanged;
        if (_databaseLocalizationService != null)
            _databaseLocalizationService.LanguageChanged -= OnDatabaseLanguageChanged;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Helper method to convert FontSize enum to numeric value
    /// </summary>
    private static double GetFontSizeValue(FontSize fontSize)
    {
        return fontSize switch
        {
            FontSize.VerySmall => 10,
            FontSize.Small => 12,
            FontSize.Medium => 14,
            FontSize.Large => 16,
            _ => 14
        };
    }

    #endregion
}

/// <summary>
/// Information about a stock management module
/// </summary>
public class StockModuleInfo : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private string _moduleType = string.Empty;
    private int _itemCount = 0;
    private string _itemCountLabel = string.Empty;
    private Brush _iconBackground = new SolidColorBrush(Colors.DodgerBlue);
    private Brush _buttonBackground = new SolidColorBrush(Colors.White);
    private bool _isSelected = false;

    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
            }
        }
    }

    public string ModuleType
    {
        get => _moduleType;
        set
        {
            if (_moduleType != value)
            {
                _moduleType = value;
                OnPropertyChanged();
            }
        }
    }

    public int ItemCount
    {
        get => _itemCount;
        set
        {
            if (_itemCount != value)
            {
                _itemCount = value;
                OnPropertyChanged();
            }
        }
    }

    public string ItemCountLabel
    {
        get => _itemCountLabel;
        set
        {
            if (_itemCountLabel != value)
            {
                _itemCountLabel = value;
                OnPropertyChanged();
            }
        }
    }

    public Brush IconBackground
    {
        get => _iconBackground;
        set
        {
            if (_iconBackground != value)
            {
                _iconBackground = value;
                OnPropertyChanged();
            }
        }
    }

    public Brush ButtonBackground
    {
        get => _buttonBackground;
        set
        {
            if (_buttonBackground != value)
            {
                _buttonBackground = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Model for transfer product form
/// </summary>
public class TransferProductModel : INotifyPropertyChanged
{
    private int _productId = 0;
    private string _productName = string.Empty;
    private string _fromShop = string.Empty;
    private string _toShop = string.Empty;
    private string _productUnit = string.Empty;
    private decimal _quantity = 0;

    public int ProductId
    {
        get => _productId;
        set
        {
            if (_productId != value)
            {
                _productId = value;
                OnPropertyChanged();
            }
        }
    }

    public string ProductName
    {
        get => _productName;
        set
        {
            if (_productName != value)
            {
                _productName = value;
                OnPropertyChanged();
            }
        }
    }

    public string FromShop
    {
        get => _fromShop;
        set
        {
            if (_fromShop != value)
            {
                _fromShop = value;
                OnPropertyChanged();
            }
        }
    }

    public string ToShop
    {
        get => _toShop;
        set
        {
            if (_toShop != value)
            {
                _toShop = value;
                OnPropertyChanged();
            }
        }
    }

    public string ProductUnit
    {
        get => _productUnit;
        set
        {
            if (_productUnit != value)
            {
                _productUnit = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
