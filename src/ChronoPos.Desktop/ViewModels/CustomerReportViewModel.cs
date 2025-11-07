using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Logging;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace ChronoPos.Desktop.ViewModels;

public partial class CustomerReportViewModel : ObservableObject
{
    private readonly ICustomerReportService _customerReportService;
    private readonly IActiveCurrencyService _activeCurrencyService;

    #region Observable Properties

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private CustomerSummaryDto _summary = new();

    [ObservableProperty]
    private ObservableCollection<CustomerAnalysisDto> _customers = new();

    [ObservableProperty]
    private ObservableCollection<CustomerRankingDto> _topCustomers = new();

    [ObservableProperty]
    private ObservableCollection<CustomerSegmentDto> _customerSegments = new();

    [ObservableProperty]
    private PlotModel? _customerGrowthModel;

    [ObservableProperty]
    private PlotModel? _topCustomersModel;

    [ObservableProperty]
    private PlotModel? _segmentDistributionModel;

    // Filter Properties
    [ObservableProperty]
    private DateTime? _filterStartDate = DateTime.Today.AddMonths(-1);

    [ObservableProperty]
    private DateTime? _filterEndDate = DateTime.Today;

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private bool? _filterIsActive;

    [ObservableProperty]
    private string? _filterSegment;

    [ObservableProperty]
    private decimal? _filterMinRevenue;

    [ObservableProperty]
    private decimal? _filterMaxRevenue;

    [ObservableProperty]
    private int? _filterMinPurchases;

    // Pagination
    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 25;

    [ObservableProperty]
    private int _totalRecords;

    [ObservableProperty]
    private int _startRecord;

    [ObservableProperty]
    private int _endRecord;

    [ObservableProperty]
    private string _dateRangeDisplay = string.Empty;

    #endregion

    public CustomerReportViewModel(
        ICustomerReportService customerReportService,
        IActiveCurrencyService activeCurrencyService)
    {
        _customerReportService = customerReportService;
        _activeCurrencyService = activeCurrencyService;

        AppLogger.LogInfo("CustomerReportViewModel initialized", filename: "customer_report");
    }

    public async Task InitializeAsync()
    {
        AppLogger.LogInfo("[CustomerReport] ViewModel: Initializing Customer Report", filename: "customer_report");
        UpdateDateRangeDisplay();
        AppLogger.LogInfo("[CustomerReport] ViewModel: Loading report data...", filename: "customer_report");
        await LoadReportData();
        AppLogger.LogInfo("[CustomerReport] ViewModel: Customer Report initialization complete", filename: "customer_report");
    }

    #region Quick Filter Commands

    [RelayCommand]
    private async Task SetTodayFilter()
    {
        FilterStartDate = DateTime.Today;
        FilterEndDate = DateTime.Today;
        await ApplyFilters();
    }

    [RelayCommand]
    private async Task SetYesterdayFilter()
    {
        FilterStartDate = DateTime.Today.AddDays(-1);
        FilterEndDate = DateTime.Today.AddDays(-1);
        await ApplyFilters();
    }

    [RelayCommand]
    private async Task SetLast7DaysFilter()
    {
        FilterStartDate = DateTime.Today.AddDays(-7);
        FilterEndDate = DateTime.Today;
        await ApplyFilters();
    }

    [RelayCommand]
    private async Task SetLast30DaysFilter()
    {
        FilterStartDate = DateTime.Today.AddDays(-30);
        FilterEndDate = DateTime.Today;
        await ApplyFilters();
    }

    [RelayCommand]
    private async Task SetThisMonthFilter()
    {
        FilterStartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        FilterEndDate = DateTime.Today;
        await ApplyFilters();
    }

    [RelayCommand]
    private async Task SetLastMonthFilter()
    {
        var lastMonth = DateTime.Today.AddMonths(-1);
        FilterStartDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
        FilterEndDate = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
        await ApplyFilters();
    }

    #endregion

    #region Filter and Load Commands

    [RelayCommand]
    private async Task ApplyFilters()
    {
        CurrentPage = 1;
        await LoadReportData();
    }

    [RelayCommand]
    private async Task ResetFilters()
    {
        FilterStartDate = DateTime.Today.AddMonths(-1);
        FilterEndDate = DateTime.Today;
        SearchTerm = string.Empty;
        FilterIsActive = null;
        FilterSegment = null;
        FilterMinRevenue = null;
        FilterMaxRevenue = null;
        FilterMinPurchases = null;
        CurrentPage = 1;
        await LoadReportData();
    }

    [RelayCommand]
    private async Task LoadReportData()
    {
        try
        {
            IsLoading = true;
            AppLogger.LogInfo("[CustomerReport] ViewModel: Loading customer report data", filename: "customer_report");

            UpdateDateRangeDisplay();
            var filter = CreateFilter();

            AppLogger.LogInfo($"[CustomerReport] ViewModel: Filter created - StartDate={filter.StartDate:yyyy-MM-dd}, EndDate={filter.EndDate:yyyy-MM-dd}, Page={filter.PageNumber}, PageSize={filter.PageSize}", filename: "customer_report");

            AppLogger.LogInfo("[CustomerReport] ViewModel: Calling GenerateCustomerReportAsync...", filename: "customer_report");
            var report = await _customerReportService.GenerateCustomerReportAsync(filter);

            AppLogger.LogInfo("[CustomerReport] ViewModel: Customer report data received from service", filename: "customer_report");

            // Update Summary
            Summary = report.Summary;
            AppLogger.LogInfo($"Summary Data - Total Customers: {Summary.TotalCustomers}, Active: {Summary.ActiveCustomers}, Revenue: {Summary.TotalRevenue}", filename: "customer_report");

            // Update Customers
            Customers.Clear();
            foreach (var customer in report.Customers)
            {
                Customers.Add(customer);
            }

            // Update Top Customers
            TopCustomers.Clear();
            foreach (var customer in report.TopCustomersByRevenue.Take(10))
            {
                TopCustomers.Add(customer);
            }

            // Update Segments
            CustomerSegments.Clear();
            foreach (var segment in report.CustomerSegments)
            {
                CustomerSegments.Add(segment);
            }

            // Update pagination
            TotalRecords = report.TotalRecords;
            UpdatePaginationInfo();

            // Update charts
            UpdateCharts(report);

            AppLogger.LogInfo($"Report loaded successfully: {TotalRecords} total records, {Customers.Count} on current page", filename: "customer_report");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading customer report: {ex.Message}", ex, filename: "customer_report");
            new MessageDialog("Error", $"Failed to load customer report: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Pagination Commands

    [RelayCommand]
    private async Task NextPage()
    {
        if (EndRecord < TotalRecords)
        {
            CurrentPage++;
            await LoadReportData();
        }
    }

    [RelayCommand]
    private async Task PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadReportData();
        }
    }

    #endregion

    #region Export Commands

    [RelayCommand]
    private async Task ExportToExcel()
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"CustomerReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                IsLoading = true;
                var filter = CreateFilter();
                filter.PageSize = int.MaxValue;

                var data = await _customerReportService.ExportToExcelAsync(filter);
                await File.WriteAllBytesAsync(saveDialog.FileName, data);

                new MessageDialog("Success", "Customer report exported successfully!", MessageDialog.MessageType.Success).ShowDialog();
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

    [RelayCommand]
    private async Task ExportToCsv()
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"CustomerReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                IsLoading = true;
                var filter = CreateFilter();
                filter.PageSize = int.MaxValue;

                var data = await _customerReportService.ExportToCsvAsync(filter);
                await File.WriteAllBytesAsync(saveDialog.FileName, data);

                new MessageDialog("Success", "Customer report exported successfully!", MessageDialog.MessageType.Success).ShowDialog();
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

    [RelayCommand]
    private async Task ExportToPdf()
    {
        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"CustomerReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                IsLoading = true;
                var filter = CreateFilter();
                filter.PageSize = int.MaxValue;

                var data = await _customerReportService.ExportToPdfAsync(filter);
                await File.WriteAllBytesAsync(saveDialog.FileName, data);

                new MessageDialog("Success", "Customer report exported successfully!", MessageDialog.MessageType.Success).ShowDialog();
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

    [RelayCommand]
    private void ShowExportMenu()
    {
        // This will be bound to a context menu or popup in the view
        AppLogger.LogInfo("Export menu requested", filename: "customer_report");
    }

    #endregion

    #region Navigation

    [RelayCommand]
    private void BackToReportsHub()
    {
        AppLogger.LogInfo("Navigating back to Reports Hub", filename: "customer_report");
        
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

    #endregion

    #region Helper Methods

    private CustomerReportFilterDto CreateFilter()
    {
        return new CustomerReportFilterDto
        {
            StartDate = FilterStartDate,
            EndDate = FilterEndDate,
            SearchTerm = SearchTerm,
            IsActive = FilterIsActive,
            CustomerSegment = FilterSegment,
            MinimumRevenue = FilterMinRevenue,
            MaximumRevenue = FilterMaxRevenue,
            MinimumPurchases = FilterMinPurchases,
            PageNumber = CurrentPage,
            PageSize = PageSize,
            CurrencySymbol = _activeCurrencyService.CurrencySymbol
        };
    }

    private void UpdatePaginationInfo()
    {
        StartRecord = (CurrentPage - 1) * PageSize + 1;
        EndRecord = Math.Min(CurrentPage * PageSize, TotalRecords);

        if (TotalRecords == 0)
        {
            StartRecord = 0;
            EndRecord = 0;
        }
    }

    private void UpdateDateRangeDisplay()
    {
        if (FilterStartDate.HasValue && FilterEndDate.HasValue)
        {
            DateRangeDisplay = $"{FilterStartDate.Value:MMM dd, yyyy} - {FilterEndDate.Value:MMM dd, yyyy}";
        }
        else
        {
            DateRangeDisplay = "All Time";
        }
    }

    private void UpdateCharts(CustomerReportDto report)
    {
        try
        {
            var currencySymbol = _activeCurrencyService.CurrencySymbol;

            // Customer Growth Chart
            var growthModel = new PlotModel
            {
                Title = "Customer Growth Trend",
                Background = OxyColors.White
            };

            growthModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd MMM",
                Title = "Date",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(240, 240, 240)
            });

            growthModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Total Customers",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(240, 240, 240)
            });

            var customerSeries = new LineSeries
            {
                Title = "Total Customers",
                Color = OxyColor.FromRgb(74, 144, 226),
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerFill = OxyColor.FromRgb(74, 144, 226),
                StrokeThickness = 2
            };

            foreach (var point in report.CustomerGrowthTrend)
            {
                customerSeries.Points.Add(new DataPoint(
                    DateTimeAxis.ToDouble(point.Date),
                    point.TotalCustomers
                ));
            }

            growthModel.Series.Add(customerSeries);
            CustomerGrowthModel = growthModel;

            // Top Customers Bar Chart
            var topCustomersModel = new PlotModel
            {
                Title = "Top 10 Customers by Revenue",
                Background = OxyColors.White
            };

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = "CustomerAxis",
                ItemsSource = report.TopCustomersByRevenue.Select(c => c.CustomerName).ToList()
            };

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = $"Revenue ({currencySymbol})",
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromRgb(240, 240, 240),
                LabelFormatter = value => $"{currencySymbol}{value:N0}"
            };

            topCustomersModel.Axes.Add(categoryAxis);
            topCustomersModel.Axes.Add(valueAxis);

            var barSeries = new BarSeries
            {
                Title = "Revenue",
                FillColor = OxyColor.FromRgb(80, 200, 120),
                StrokeThickness = 0
            };

            foreach (var customer in report.TopCustomersByRevenue)
            {
                barSeries.Items.Add(new BarItem { Value = (double)customer.TotalRevenue });
            }

            topCustomersModel.Series.Add(barSeries);
            TopCustomersModel = topCustomersModel;

            // Segment Distribution Pie Chart
            var segmentModel = new PlotModel
            {
                Title = "Customer Segments",
                Background = OxyColors.White
            };

            var pieSeries = new PieSeries
            {
                StrokeThickness = 2,
                InsideLabelPosition = 0.5,
                AngleSpan = 360,
                StartAngle = 0
            };

            var colors = new[]
            {
                OxyColor.FromRgb(74, 144, 226),   // Blue
                OxyColor.FromRgb(80, 200, 120),   // Green
                OxyColor.FromRgb(255, 184, 77),   // Orange
                OxyColor.FromRgb(239, 71, 111),   // Red
                OxyColor.FromRgb(156, 136, 255)   // Purple
            };

            for (int i = 0; i < report.CustomerSegments.Count; i++)
            {
                var segment = report.CustomerSegments[i];
                pieSeries.Slices.Add(new PieSlice(segment.SegmentName, segment.CustomerCount)
                {
                    Fill = colors[i % colors.Length],
                    IsExploded = false
                });
            }

            segmentModel.Series.Add(pieSeries);
            SegmentDistributionModel = segmentModel;

            AppLogger.LogInfo("Charts updated successfully", filename: "customer_report");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error updating charts: {ex.Message}", ex, filename: "customer_report");
        }
    }

    #endregion
}
