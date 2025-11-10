using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs.Inventory;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Logging;
using ChronoPos.Desktop.Views;
using ChronoPos.Desktop.Views.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Inventory Report screen
/// </summary>
public partial class InventoryReportViewModel : ObservableObject
{
    private readonly IInventoryReportService _inventoryReportService;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    #region Observable Properties

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string dateRangeDisplay = string.Empty;

    // Filter Properties
    [ObservableProperty]
    private DateTime filterStartDate = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime filterEndDate = DateTime.Today;

    [ObservableProperty]
    private int? selectedProductId;

    [ObservableProperty]
    private int? selectedCategoryId;

    [ObservableProperty]
    private InventoryReportType selectedReportType = InventoryReportType.StockSummary;

    // Report Collections
    [ObservableProperty]
    private ObservableCollection<StockSummaryDto> stockSummaryData = new();

    [ObservableProperty]
    private ObservableCollection<StockLedgerReportDto> stockLedgerData = new();

    [ObservableProperty]
    private ObservableCollection<StockValuationDto> stockValuationData = new();

    [ObservableProperty]
    private ObservableCollection<LowStockDto> lowStockData = new();

    [ObservableProperty]
    private ObservableCollection<GoodsReceivedReportDto> goodsReceivedData = new();

    [ObservableProperty]
    private ObservableCollection<StockAdjustmentReportDto> stockAdjustmentData = new();

    [ObservableProperty]
    private ObservableCollection<StockTransferReportDto> stockTransferData = new();

    [ObservableProperty]
    private ObservableCollection<StockAgingDto> stockAgingData = new();

    [ObservableProperty]
    private ObservableCollection<GoodsReturnReportDto> goodsReturnData = new();

    [ObservableProperty]
    private ObservableCollection<InventorySummaryDto> inventorySummaryData = new();

    // Filter Options
    [ObservableProperty]
    private ObservableCollection<ProductFilterOption> products = new();

    [ObservableProperty]
    private ObservableCollection<CategoryFilterOption> categories = new();

    [ObservableProperty]
    private ObservableCollection<ReportTypeOption> reportTypes = new();

    // Summary Metrics
    [ObservableProperty]
    private decimal totalStockValue = 0;

    [ObservableProperty]
    private int totalProducts = 0;

    [ObservableProperty]
    private int lowStockItems = 0;

    [ObservableProperty]
    private decimal totalInwardQty = 0;

    [ObservableProperty]
    private decimal totalOutwardQty = 0;

    #endregion

    public InventoryReportViewModel(
        IInventoryReportService inventoryReportService,
        IProductService productService,
        ICategoryService categoryService)
    {
        _inventoryReportService = inventoryReportService;
        _productService = productService;
        _categoryService = categoryService;

        AppLogger.LogInfo("InventoryReportViewModel initialized", filename: "inventory_report");
        InitializeFilterOptions();
    }

    public async Task InitializeAsync()
    {
        try
        {
            AppLogger.LogInfo("Initializing Inventory Report", filename: "inventory_report");
            await LoadFilterData();
            await LoadReportDataAsync();
            AppLogger.LogInfo("Inventory Report initialization complete", filename: "inventory_report");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error initializing Inventory Report: {ex.Message}", ex, filename: "inventory_report");
            MessageBox.Show($"Error loading inventory report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void InitializeFilterOptions()
    {
        // Initialize Report Types
        ReportTypes = new ObservableCollection<ReportTypeOption>
        {
            new ReportTypeOption { Name = "Stock Summary", Type = InventoryReportType.StockSummary },
            new ReportTypeOption { Name = "Stock Ledger", Type = InventoryReportType.StockLedger },
            new ReportTypeOption { Name = "Stock Valuation", Type = InventoryReportType.StockValuation },
            new ReportTypeOption { Name = "Low Stock Alert", Type = InventoryReportType.LowStock },
            new ReportTypeOption { Name = "Goods Received", Type = InventoryReportType.GoodsReceived },
            new ReportTypeOption { Name = "Stock Adjustment", Type = InventoryReportType.StockAdjustment },
            new ReportTypeOption { Name = "Stock Transfer", Type = InventoryReportType.StockTransfer },
            new ReportTypeOption { Name = "Stock Aging", Type = InventoryReportType.StockAging },
            new ReportTypeOption { Name = "Goods Return/Replace", Type = InventoryReportType.GoodsReturn },
            new ReportTypeOption { Name = "Category Summary", Type = InventoryReportType.CategorySummary }
        };

        UpdateDateRangeDisplay();
    }

    private async Task LoadFilterData()
    {
        try
        {
            // Load Products
            var allProducts = await _productService.GetAllProductsAsync();
            Products = new ObservableCollection<ProductFilterOption>(
                new[] { new ProductFilterOption { Id = null, Name = "All Products" } }
                .Concat(allProducts.Select(p => new ProductFilterOption { Id = p.Id, Name = p.Name }))
            );

            // Load Categories
            var allCategories = await _categoryService.GetAllAsync();
            Categories = new ObservableCollection<CategoryFilterOption>(
                new[] { new CategoryFilterOption { Id = null, Name = "All Categories" } }
                .Concat(allCategories.Select(c => new CategoryFilterOption { Id = c.Id, Name = c.Name }))
            );
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading filter data: {ex.Message}", ex, filename: "inventory_report");
        }
    }

    [RelayCommand]
    private async Task LoadReportDataAsync()
    {
        try
        {
            IsLoading = true;
            UpdateDateRangeDisplay();

            AppLogger.LogInfo($"Loading {SelectedReportType} report", filename: "inventory_report");

            // Clear all collections
            ClearAllCollections();

            // Load data based on selected report type
            switch (SelectedReportType)
            {
                case InventoryReportType.StockSummary:
                    await LoadStockSummary();
                    break;
                case InventoryReportType.StockLedger:
                    await LoadStockLedger();
                    break;
                case InventoryReportType.StockValuation:
                    await LoadStockValuation();
                    break;
                case InventoryReportType.LowStock:
                    await LoadLowStock();
                    break;
                case InventoryReportType.GoodsReceived:
                    await LoadGoodsReceived();
                    break;
                case InventoryReportType.StockAdjustment:
                    await LoadStockAdjustment();
                    break;
                case InventoryReportType.StockTransfer:
                    await LoadStockTransfer();
                    break;
                case InventoryReportType.StockAging:
                    await LoadStockAging();
                    break;
                case InventoryReportType.GoodsReturn:
                    await LoadGoodsReturn();
                    break;
                case InventoryReportType.CategorySummary:
                    await LoadCategorySummary();
                    break;
            }

            AppLogger.LogInfo($"{SelectedReportType} report loaded successfully", filename: "inventory_report");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading report data: {ex.Message}", ex, filename: "inventory_report");
            MessageBox.Show($"Error loading report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadStockSummary()
    {
        var endDate = FilterEndDate.Date.AddDays(1).AddTicks(-1); // End of day
        var data = await _inventoryReportService.GetStockSummaryAsync(FilterStartDate, endDate, SelectedProductId, SelectedCategoryId);
        StockSummaryData = new ObservableCollection<StockSummaryDto>(data);
        
        // Update summary metrics
        TotalProducts = StockSummaryData.Count;
        TotalStockValue = StockSummaryData.Sum(s => s.StockValue);
        TotalInwardQty = StockSummaryData.Sum(s => s.InwardQty);
        TotalOutwardQty = StockSummaryData.Sum(s => s.OutwardQty);
    }

    private async Task LoadStockLedger()
    {
        var endDate = FilterEndDate.Date.AddDays(1).AddTicks(-1); // End of day
        var data = await _inventoryReportService.GetStockLedgerAsync(SelectedProductId, FilterStartDate, endDate);
        StockLedgerData = new ObservableCollection<StockLedgerReportDto>(data);
    }

    private async Task LoadStockValuation()
    {
        var data = await _inventoryReportService.GetStockValuationAsync(SelectedCategoryId);
        StockValuationData = new ObservableCollection<StockValuationDto>(data);
        TotalStockValue = StockValuationData.Sum(v => v.TotalValue);
        TotalProducts = StockValuationData.Count;
    }

    private async Task LoadLowStock()
    {
        var data = await _inventoryReportService.GetLowStockReportAsync(SelectedCategoryId);
        LowStockData = new ObservableCollection<LowStockDto>(data);
        LowStockItems = LowStockData.Count;
    }

    private async Task LoadGoodsReceived()
    {
        var endDate = FilterEndDate.Date.AddDays(1).AddTicks(-1); // End of day
        var data = await _inventoryReportService.GetGoodsReceivedReportAsync(FilterStartDate, endDate);
        GoodsReceivedData = new ObservableCollection<GoodsReceivedReportDto>(data);
    }

    private async Task LoadStockAdjustment()
    {
        var endDate = FilterEndDate.Date.AddDays(1).AddTicks(-1); // End of day
        var data = await _inventoryReportService.GetStockAdjustmentReportAsync(FilterStartDate, endDate);
        StockAdjustmentData = new ObservableCollection<StockAdjustmentReportDto>(data);
    }

    private async Task LoadStockTransfer()
    {
        var endDate = FilterEndDate.Date.AddDays(1).AddTicks(-1); // End of day
        var data = await _inventoryReportService.GetStockTransferReportAsync(FilterStartDate, endDate);
        StockTransferData = new ObservableCollection<StockTransferReportDto>(data);
    }

    private async Task LoadStockAging()
    {
        var data = await _inventoryReportService.GetStockAgingReportAsync(SelectedCategoryId);
        StockAgingData = new ObservableCollection<StockAgingDto>(data);
    }

    private async Task LoadGoodsReturn()
    {
        var endDate = FilterEndDate.Date.AddDays(1).AddTicks(-1); // End of day
        var data = await _inventoryReportService.GetGoodsReturnReportAsync(FilterStartDate, endDate);
        GoodsReturnData = new ObservableCollection<GoodsReturnReportDto>(data);
    }

    private async Task LoadCategorySummary()
    {
        var data = await _inventoryReportService.GetInventorySummaryByCategoryAsync();
        InventorySummaryData = new ObservableCollection<InventorySummaryDto>(data);
        TotalStockValue = InventorySummaryData.Sum(s => s.TotalValue);
        LowStockItems = InventorySummaryData.Sum(s => s.LowStockItems);
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedProductId = null;
        SelectedCategoryId = null;
        FilterStartDate = DateTime.Today.AddDays(-30);
        FilterEndDate = DateTime.Today;
    }

    [RelayCommand]
    private async Task RefreshReport()
    {
        await LoadReportDataAsync();
    }

    [RelayCommand]
    private void BackToReportsHub()
    {
        var mainWindow = System.Windows.Application.Current.MainWindow as Views.MainWindow;
        if (mainWindow?.DataContext is MainWindowViewModel mainViewModel)
        {
            var reportsHubViewModel = App.GetService<ReportsHubViewModel>();
            var reportsHubView = new Views.ReportsHubView
            {
                DataContext = reportsHubViewModel
            };
            mainViewModel.NavigateToView(reportsHubView);
        }
    }

    [RelayCommand]
    private async Task ExportToExcel()
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"InventoryReport_{SelectedReportType.ToString()}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                IsLoading = true;
                var filter = CreateFilter();
                
                AppLogger.LogInfo($"Exporting {SelectedReportType.ToString()} inventory report to Excel", filename: "inventory_report");
                
                var data = await _inventoryReportService.ExportToExcelAsync(filter);
                await File.WriteAllBytesAsync(saveDialog.FileName, data);
                
                new MessageDialog("Success", "Report exported successfully!", MessageDialog.MessageType.Success).ShowDialog();
                AppLogger.LogInfo($"Successfully exported to {saveDialog.FileName}", filename: "inventory_report");
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error exporting to Excel: {ex.Message}", ex, filename: "inventory_report");
            new MessageDialog("Error", $"Error exporting to Excel: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportToCsv()
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"InventoryReport_{SelectedReportType.ToString()}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                IsLoading = true;
                var filter = CreateFilter();
                
                AppLogger.LogInfo($"Exporting {SelectedReportType.ToString()} inventory report to CSV", filename: "inventory_report");
                
                var data = await _inventoryReportService.ExportToCsvAsync(filter);
                await File.WriteAllBytesAsync(saveDialog.FileName, data);
                
                new MessageDialog("Success", "Report exported successfully!", MessageDialog.MessageType.Success).ShowDialog();
                AppLogger.LogInfo($"Successfully exported to {saveDialog.FileName}", filename: "inventory_report");
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error exporting to CSV: {ex.Message}", ex, filename: "inventory_report");
            new MessageDialog("Error", $"Error exporting to CSV: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportToPdf()
    {
        try
        {
            IsLoading = true;
            AppLogger.LogInfo("PDF export requested", filename: "inventory_report");
            
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"InventoryReport_{SelectedReportType}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var filter = CreateFilter();
                var pdfBytes = await _inventoryReportService.ExportToPdfAsync(filter);
                
                await File.WriteAllBytesAsync(saveFileDialog.FileName, pdfBytes);
                
                AppLogger.LogInfo($"PDF exported successfully: {saveFileDialog.FileName}", filename: "inventory_report");
                new MessageDialog("Success", "PDF exported successfully!", MessageDialog.MessageType.Success).ShowDialog();
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error exporting to PDF: {ex.Message}", ex, filename: "inventory_report");
            new MessageDialog("Error", $"Error exporting to PDF: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private InventoryReportFilterDto CreateFilter()
    {
        return new InventoryReportFilterDto
        {
            StartDate = FilterStartDate,
            EndDate = FilterEndDate,
            ProductId = SelectedProductId,
            CategoryId = SelectedCategoryId,
            ReportType = SelectedReportType.ToString()
        };
    }

    private void ClearAllCollections()
    {
        StockSummaryData.Clear();
        StockLedgerData.Clear();
        StockValuationData.Clear();
        LowStockData.Clear();
        GoodsReceivedData.Clear();
        StockAdjustmentData.Clear();
        StockTransferData.Clear();
        StockAgingData.Clear();
        GoodsReturnData.Clear();
        InventorySummaryData.Clear();

        TotalStockValue = 0;
        TotalProducts = 0;
        LowStockItems = 0;
        TotalInwardQty = 0;
        TotalOutwardQty = 0;
    }

    private void UpdateDateRangeDisplay()
    {
        DateRangeDisplay = $"{FilterStartDate:MMM dd, yyyy} - {FilterEndDate:MMM dd, yyyy}";
    }

    partial void OnFilterStartDateChanged(DateTime value)
    {
        UpdateDateRangeDisplay();
    }

    partial void OnFilterEndDateChanged(DateTime value)
    {
        UpdateDateRangeDisplay();
    }

    partial void OnSelectedReportTypeChanged(InventoryReportType value)
    {
        _ = LoadReportDataAsync();
    }
}

#region Filter Option Classes

public class ProductFilterOption
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CategoryFilterOption
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ReportTypeOption
{
    public string Name { get; set; } = string.Empty;
    public InventoryReportType Type { get; set; }
}

#endregion
