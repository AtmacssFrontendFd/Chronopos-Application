using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Logging;
using ChronoPos.Domain.Enums;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Sales Report screen
/// </summary>
public partial class SalesReportViewModel : ObservableObject
{
    private readonly ISalesReportService _salesReportService;
    private readonly ICategoryService _categoryService;
    private readonly IActiveCurrencyService _activeCurrencyService;

    #region Observable Properties

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string dateRangeDisplay = string.Empty;

    // Filter Properties
    [ObservableProperty]
    private DateTime? filterStartDate = DateTime.Today.AddDays(-30); // Last 30 days instead of just today

    [ObservableProperty]
    private DateTime? filterEndDate = DateTime.Today;

    [ObservableProperty]
    private object? filterPaymentMethod;

    [ObservableProperty]
    private object? filterCategory;

    [ObservableProperty]
    private object? filterStatus;

    // Summary Data
    [ObservableProperty]
    private SalesSummaryDto summary = new();

    // Collections
    [ObservableProperty]
    private ObservableCollection<SaleTransactionDto> transactions = new();

    [ObservableProperty]
    private ObservableCollection<ProductPerformanceDto> topProducts = new();

    [ObservableProperty]
    private ObservableCollection<CategoryPerformanceDto> categoryPerformance = new();

    [ObservableProperty]
    private ObservableCollection<PaymentMethodBreakdownDto> paymentBreakdown = new();

    [ObservableProperty]
    private ObservableCollection<DailySalesDto> dailyTrend = new();

    [ObservableProperty]
    private ObservableCollection<HourlySalesDto> hourlyDistribution = new();

    // OxyPlot Chart Models
    [ObservableProperty]
    private PlotModel? salesTrendModel;

    [ObservableProperty]
    private PlotModel? topProductsModel;

    // Filter Options
    [ObservableProperty]
    private ObservableCollection<PaymentMethodOption> paymentMethods = new();

    [ObservableProperty]
    private ObservableCollection<CategoryDto> categories = new();

    [ObservableProperty]
    private ObservableCollection<StatusOption> statuses = new();

    // Pagination
    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int pageSize = 25;

    [ObservableProperty]
    private int totalRecords = 0;

    [ObservableProperty]
    private int startRecord = 0;

    [ObservableProperty]
    private int endRecord = 0;

    #endregion

    public SalesReportViewModel(
        ISalesReportService salesReportService,
        ICategoryService categoryService,
        IActiveCurrencyService activeCurrencyService)
    {
        _salesReportService = salesReportService;
        _categoryService = categoryService;
        _activeCurrencyService = activeCurrencyService;

        AppLogger.LogInfo("SalesReportViewModel initialized", filename: "sales_report");
        InitializeFilterOptions();
    }

    public async Task InitializeAsync()
    {
        try
        {
            AppLogger.LogInfo("Initializing Sales Report", filename: "sales_report");
            await LoadFilterData();
            await LoadReportDataAsync();
            AppLogger.LogInfo("Sales Report initialization complete", filename: "sales_report");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error initializing Sales Report: {ex.Message}", ex, filename: "sales_report");
            MessageBox.Show($"Error loading sales report: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void InitializeFilterOptions()
    {
        // Initialize Payment Methods
        PaymentMethods = new ObservableCollection<PaymentMethodOption>
        {
            new PaymentMethodOption { Name = "All Payment Methods", Value = null },
            new PaymentMethodOption { Name = "Cash", Value = PaymentMethod.Cash },
            new PaymentMethodOption { Name = "Credit Card", Value = PaymentMethod.CreditCard },
            new PaymentMethodOption { Name = "Debit Card", Value = PaymentMethod.DebitCard },
            new PaymentMethodOption { Name = "Digital Wallet", Value = PaymentMethod.DigitalWallet },
            new PaymentMethodOption { Name = "Bank Transfer", Value = PaymentMethod.BankTransfer },
            new PaymentMethodOption { Name = "Check", Value = PaymentMethod.Check }
        };
        FilterPaymentMethod = PaymentMethods[0];

        // Initialize Statuses
        Statuses = new ObservableCollection<StatusOption>
        {
            new StatusOption { Name = "All Statuses", Value = null },
            new StatusOption { Name = "Settled", Value = SaleStatus.Settled },
            new StatusOption { Name = "Billed", Value = SaleStatus.Billed },
            new StatusOption { Name = "Draft", Value = SaleStatus.Draft },
            new StatusOption { Name = "Hold", Value = SaleStatus.Hold },
            new StatusOption { Name = "Cancelled", Value = SaleStatus.Cancelled },
            new StatusOption { Name = "Pending Payment", Value = SaleStatus.PendingPayment },
            new StatusOption { Name = "Partial Payment", Value = SaleStatus.PartialPayment }
        };
        FilterStatus = Statuses[0];

        UpdateDateRangeDisplay();
    }

    private async Task LoadFilterData()
    {
        try
        {
            AppLogger.LogInfo("Loading filter data (categories)", filename: "sales_report");
            
            // Load categories
            var categoryList = await _categoryService.GetAllAsync();
            AppLogger.LogInfo($"Retrieved {categoryList.Count()} categories", filename: "sales_report");
            
            Categories = new ObservableCollection<CategoryDto>(
                new[] { new CategoryDto { Id = 0, Name = "All Categories" } }
                .Concat(categoryList)
            );
            FilterCategory = Categories[0];
            
            AppLogger.LogInfo("Filter data loaded successfully", filename: "sales_report");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading filter data: {ex.Message}", ex, filename: "sales_report");
            MessageBox.Show($"Error loading filter data: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task LoadReportDataAsync()
    {
        try
        {
            AppLogger.LogInfo("Loading sales report data", filename: "sales_report");
            IsLoading = true;

            var filter = CreateFilter();
            AppLogger.LogInfo($"Filter created: StartDate={filter.StartDate:yyyy-MM-dd}, EndDate={filter.EndDate:yyyy-MM-dd}, Page={filter.PageNumber}, PageSize={filter.PageSize}", filename: "sales_report");
            
            var report = await _salesReportService.GenerateSalesReportAsync(filter);
            AppLogger.LogInfo("Sales report data received from service", filename: "sales_report");

            // Update summary
            Summary = report.Summary;
            
            // Debug: Show what data we got
            AppLogger.LogInfo($"Summary Data - Total Sales: {Summary.TotalSalesAmount}, Transactions: {Summary.TotalTransactions}, Items Sold: {Summary.TotalItemsSold}", filename: "sales_report");

            // Update collections
            Transactions = new ObservableCollection<SaleTransactionDto>(report.Transactions);
            TopProducts = new ObservableCollection<ProductPerformanceDto>(report.TopProducts);
            CategoryPerformance = new ObservableCollection<CategoryPerformanceDto>(report.CategoryPerformance);
            PaymentBreakdown = new ObservableCollection<PaymentMethodBreakdownDto>(report.PaymentBreakdown);
            DailyTrend = new ObservableCollection<DailySalesDto>(report.DailyTrend);
            HourlyDistribution = new ObservableCollection<HourlySalesDto>(report.HourlyDistribution);

            // Update charts
            UpdateCharts();

            // Update pagination
            TotalRecords = report.TotalRecords;
            UpdatePaginationDisplay();

            AppLogger.LogInfo($"Report loaded successfully: {TotalRecords} total records, {Transactions.Count} on current page", filename: "sales_report");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading sales report: {ex.Message}", ex, filename: "sales_report");
            MessageBox.Show($"Error loading report: {ex.Message}\n\nPlease check the logs for more details.", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private SalesReportFilterDto CreateFilter()
    {
        var filter = new SalesReportFilterDto
        {
            StartDate = FilterStartDate?.Date, // Start of day
            EndDate = FilterEndDate?.Date.AddDays(1).AddTicks(-1), // End of day (23:59:59.9999999)
            PageNumber = CurrentPage,
            PageSize = PageSize,
            CurrencySymbol = _activeCurrencyService.CurrencySymbol
        };

        if (FilterPaymentMethod is PaymentMethodOption paymentOption && paymentOption.Value.HasValue)
            filter.PaymentMethod = paymentOption.Value.Value;

        if (FilterCategory is CategoryDto category && category.Id > 0)
            filter.CategoryId = category.Id;

        if (FilterStatus is StatusOption statusOption && statusOption.Value.HasValue)
            filter.Status = statusOption.Value.Value;

        return filter;
    }

    private void UpdateDateRangeDisplay()
    {
        if (FilterStartDate.HasValue && FilterEndDate.HasValue)
        {
            if (FilterStartDate.Value.Date == FilterEndDate.Value.Date)
            {
                DateRangeDisplay = FilterStartDate.Value.ToString("MMMM dd, yyyy");
            }
            else
            {
                DateRangeDisplay = $"{FilterStartDate.Value:MMM dd, yyyy} - {FilterEndDate.Value:MMM dd, yyyy}";
            }
        }
        else
        {
            DateRangeDisplay = "All Time";
        }
    }

    private void UpdatePaginationDisplay()
    {
        StartRecord = TotalRecords > 0 ? ((CurrentPage - 1) * PageSize) + 1 : 0;
        EndRecord = Math.Min(CurrentPage * PageSize, TotalRecords);
    }

    #region Filter Commands

    [RelayCommand]
    private async Task SetTodayFilter()
    {
        FilterStartDate = DateTime.Today;
        FilterEndDate = DateTime.Today;
        UpdateDateRangeDisplay();
        await ApplyFilters();
    }

    [RelayCommand]
    private async Task SetYesterdayFilter()
    {
        FilterStartDate = DateTime.Today.AddDays(-1);
        FilterEndDate = DateTime.Today.AddDays(-1);
        UpdateDateRangeDisplay();
        await ApplyFilters();
    }

    [RelayCommand]
    private async Task SetLast7DaysFilter()
    {
        FilterStartDate = DateTime.Today.AddDays(-6);
        FilterEndDate = DateTime.Today;
        UpdateDateRangeDisplay();
        await ApplyFilters();
    }

    [RelayCommand]
    private async Task SetLast30DaysFilter()
    {
        FilterStartDate = DateTime.Today.AddDays(-29);
        FilterEndDate = DateTime.Today;
        UpdateDateRangeDisplay();
        await ApplyFilters();
    }

    [RelayCommand]
    private async Task SetThisMonthFilter()
    {
        var today = DateTime.Today;
        FilterStartDate = new DateTime(today.Year, today.Month, 1);
        FilterEndDate = DateTime.Today;
        UpdateDateRangeDisplay();
        await ApplyFilters();
    }

    [RelayCommand]
    private async Task SetLastMonthFilter()
    {
        var today = DateTime.Today;
        var lastMonth = today.AddMonths(-1);
        FilterStartDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
        FilterEndDate = new DateTime(lastMonth.Year, lastMonth.Month, 
            DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
        UpdateDateRangeDisplay();
        await ApplyFilters();
    }

    [RelayCommand]
    private async Task ApplyFilters()
    {
        CurrentPage = 1; // Reset to first page
        UpdateDateRangeDisplay();
        await LoadReportDataAsync();
    }

    [RelayCommand]
    private async Task ResetFilters()
    {
        FilterStartDate = DateTime.Today;
        FilterEndDate = DateTime.Today;
        FilterPaymentMethod = PaymentMethods[0];
        FilterCategory = Categories[0];
        FilterStatus = Statuses[0];
        CurrentPage = 1;
        UpdateDateRangeDisplay();
        await LoadReportDataAsync();
    }

    #endregion

    #region Pagination Commands

    [RelayCommand]
    private async Task PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadReportDataAsync();
        }
    }

    [RelayCommand]
    private async Task NextPage()
    {
        var totalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);
        if (CurrentPage < totalPages)
        {
            CurrentPage++;
            await LoadReportDataAsync();
        }
    }

    #endregion

    #region Export Commands

    [RelayCommand]
    private void ShowExportMenu()
    {
        var menu = new System.Windows.Controls.ContextMenu();
        
        var excelItem = new System.Windows.Controls.MenuItem { Header = "Export to Excel" };
        excelItem.Click += async (s, e) => await ExportToExcelAsync();
        
        var csvItem = new System.Windows.Controls.MenuItem { Header = "Export to CSV" };
        csvItem.Click += async (s, e) => await ExportToCsvAsync();
        
        var pdfItem = new System.Windows.Controls.MenuItem { Header = "Export to PDF" };
        pdfItem.Click += async (s, e) => await ExportToPdfAsync();
        
        menu.Items.Add(excelItem);
        menu.Items.Add(csvItem);
        menu.Items.Add(pdfItem);
        
        menu.IsOpen = true;
    }

    private async Task ExportToExcelAsync()
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                IsLoading = true;
                var filter = CreateFilter();
                filter.PageSize = int.MaxValue; // Get all records for export
                
                var data = await _salesReportService.ExportToExcelAsync(filter);
                await File.WriteAllBytesAsync(saveDialog.FileName, data);
                
                new MessageDialog("Success", "Report exported successfully!", MessageDialog.MessageType.Success).ShowDialog();
            }
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error exporting to Excel: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExportToCsvAsync()
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                IsLoading = true;
                var filter = CreateFilter();
                filter.PageSize = int.MaxValue; // Get all records for export
                
                var data = await _salesReportService.ExportToCsvAsync(filter);
                await File.WriteAllBytesAsync(saveDialog.FileName, data);
                
                new MessageDialog("Success", "Report exported successfully!", MessageDialog.MessageType.Success).ShowDialog();
            }
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error exporting to CSV: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExportToPdfAsync()
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                IsLoading = true;
                var filter = CreateFilter();
                filter.PageSize = int.MaxValue; // Get all records for export
                
                var data = await _salesReportService.ExportToPdfAsync(filter);
                await File.WriteAllBytesAsync(saveDialog.FileName, data);
                
                new MessageDialog("Success", "Report exported successfully!", MessageDialog.MessageType.Success).ShowDialog();
            }
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error exporting to PDF: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    private void UpdateCharts()
    {
        var currencySymbol = _activeCurrencyService.CurrencySymbol;
        
        // Sales Trend Chart (Line Chart)
        if (DailyTrend.Any())
        {
            var salesModel = new PlotModel 
            { 
                Title = "Sales Trend",
                TitleFontSize = 16,
                Background = OxyColors.White
            };

            var lineSeries = new LineSeries
            {
                Title = "Daily Sales",
                Color = OxyColor.FromRgb(30, 144, 255),
                StrokeThickness = 3,
                MarkerType = MarkerType.Circle,
                MarkerSize = 6,
                MarkerFill = OxyColor.FromRgb(30, 144, 255),
                MarkerStroke = OxyColors.White,
                MarkerStrokeThickness = 2
            };

            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "MM/dd",
                Title = "Date",
                Angle = -45,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(240, 240, 240)
            };

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = $"Sales ({currencySymbol})",
                LabelFormatter = x => $"{currencySymbol}{x:N2}",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(240, 240, 240),
                MinorGridlineStyle = LineStyle.Dot,
                MinorGridlineColor = OxyColor.FromRgb(245, 245, 245)
            };

            foreach (var dailySale in DailyTrend)
            {
                lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(dailySale.Date), (double)dailySale.TotalSales));
            }

            salesModel.Series.Add(lineSeries);
            salesModel.Axes.Add(dateAxis);
            salesModel.Axes.Add(valueAxis);
            
            SalesTrendModel = salesModel;
        }

        // Top Products Chart (Bar Chart)
        if (TopProducts.Any())
        {
            var productsModel = new PlotModel 
            { 
                Title = "Top 10 Products by Revenue",
                TitleFontSize = 16,
                Background = OxyColors.White
            };

            var barSeries = new BarSeries
            {
                Title = "Revenue",
                FillColor = OxyColor.FromRgb(75, 192, 192),
                StrokeThickness = 1,
                StrokeColor = OxyColors.White
            };

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = "ProductAxis",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(240, 240, 240)
            };

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = $"Revenue ({currencySymbol})",
                LabelFormatter = x => $"{currencySymbol}{x:N0}",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(240, 240, 240),
                MinorGridlineStyle = LineStyle.Dot,
                MinorGridlineColor = OxyColor.FromRgb(245, 245, 245)
            };

            var topProductsList = TopProducts.Take(10).OrderBy(p => p.TotalRevenue).ToList();
            
            foreach (var product in topProductsList)
            {
                categoryAxis.Labels.Add(product.ProductName);
                barSeries.Items.Add(new BarItem { Value = (double)product.TotalRevenue });
            }

            productsModel.Series.Add(barSeries);
            productsModel.Axes.Add(categoryAxis);
            productsModel.Axes.Add(valueAxis);
            
            TopProductsModel = productsModel;
        }
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
}

// Helper classes for filter options
public class PaymentMethodOption
{
    public string Name { get; set; } = string.Empty;
    public PaymentMethod? Value { get; set; }
}

public class StatusOption
{
    public string Name { get; set; } = string.Empty;
    public SaleStatus? Value { get; set; }
}
