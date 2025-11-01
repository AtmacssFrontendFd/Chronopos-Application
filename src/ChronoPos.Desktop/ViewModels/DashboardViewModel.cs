using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Dashboard screen
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly IDashboardService _dashboardService;
    private DispatcherTimer? _kpiRefreshTimer;
    private DispatcherTimer? _chartRefreshTimer;
    private DispatcherTimer? _lastRefreshDisplayTimer;
    
    #region Observable Properties

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasError = false;

    // KPI Properties
    [ObservableProperty]
    private decimal todaysSales = 0;

    [ObservableProperty]
    private decimal monthlySales = 0;

    [ObservableProperty]
    private decimal growthPercentage = 0;

    [ObservableProperty]
    private int activeTables = 0;

    [ObservableProperty]
    private int pendingOrders = 0;

    [ObservableProperty]
    private int lowStockItems = 0;

    [ObservableProperty]
    private int totalCustomers = 0;

    [ObservableProperty]
    private decimal averageTransactionValue = 0;

    [ObservableProperty]
    private int todaysTransactionCount = 0;

    [ObservableProperty]
    private decimal yesterdaysSales = 0;

    [ObservableProperty]
    private decimal lastMonthSales = 0;

    // Collections
    [ObservableProperty]
    private ObservableCollection<ProductSalesDto> popularProducts = new();

    [ObservableProperty]
    private ObservableCollection<RecentSaleDto> recentSales = new();

    [ObservableProperty]
    private ObservableCollection<SalesAnalyticsDto> salesAnalytics = new();

    [ObservableProperty]
    private ObservableCollection<HourlySalesDto> hourlySales = new();

    [ObservableProperty]
    private ObservableCollection<CategorySalesDto> topCategories = new();

    [ObservableProperty]
    private ObservableCollection<TopCustomerDto> topCustomers = new();

    // Customer Insights
    [ObservableProperty]
    private int newCustomersToday = 0;

    [ObservableProperty]
    private int newCustomersThisWeek = 0;

    [ObservableProperty]
    private int newCustomersThisMonth = 0;

    [ObservableProperty]
    private decimal returningCustomerPercentage = 0;

    [ObservableProperty]
    private int returningCustomerCount = 0;

    [ObservableProperty]
    private decimal customerGrowthPercentage = 0;

    [ObservableProperty]
    private decimal averageCustomerValue = 0;

    [ObservableProperty]
    private decimal averageTransactionsPerCustomer = 0;

    // Chart Settings
    [ObservableProperty]
    private string selectedChartPeriod = "Daily";

    [ObservableProperty]
    private int chartDays = 30;

    [ObservableProperty]
    private double maxSalesValue = 1000; // Dynamic max value for chart scaling

    // UI State
    [ObservableProperty]
    private DateTime lastRefreshTime = DateTime.Now;

    [ObservableProperty]
    private string lastRefreshTimeDisplay = "Just now";

    #endregion

    #region Navigation Actions

    // Navigation actions to be set by MainWindow
    public Action? NavigateToProductsAction { get; set; }
    public Action? NavigateToTransactionsAction { get; set; }
    public Action? NavigateToCustomersAction { get; set; }
    public Action? NavigateToStockManagementAction { get; set; }
    public Action? NavigateToNewSaleAction { get; set; }
    public Action? NavigateToReportsAction { get; set; }

    #endregion

    #region Constructor

    public DashboardViewModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        
        // Don't load data in constructor - it should be done when view is loaded
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize the dashboard - called when view is loaded
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            Console.WriteLine("DashboardViewModel: Starting InitializeAsync...");
            await LoadDashboardDataAsync();
            Console.WriteLine("DashboardViewModel: LoadDashboardDataAsync completed");
            StartAutoRefreshTimers();
            Console.WriteLine("DashboardViewModel: Auto-refresh timers started");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DashboardViewModel InitializeAsync Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
            HandleError($"Error initializing dashboard: {ex.Message}\n\nDetails: {ex.InnerException?.Message}");
            throw; // Re-throw to be caught by view
        }
    }

    /// <summary>
    /// Load all dashboard data
    /// </summary>
    [RelayCommand]
    private async Task LoadDashboardDataAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            Console.WriteLine("DashboardViewModel: Loading dashboard data...");

            // Load data in parallel for better performance
            Console.WriteLine("DashboardViewModel: Starting parallel data load...");
            var kpiTask = LoadKpisAsync();
            var productsTask = LoadPopularProductsAsync();
            var salesTask = LoadRecentSalesAsync();
            var analyticsTask = LoadSalesAnalyticsAsync();
            var categoriesTask = LoadTopCategoriesAsync();
            var customersTask = LoadCustomerInsightsAsync();

            await Task.WhenAll(kpiTask, productsTask, salesTask, analyticsTask, categoriesTask, customersTask);
            Console.WriteLine("DashboardViewModel: All data loaded successfully");

            LastRefreshTime = DateTime.Now;
            LastRefreshTimeDisplay = "Just now";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DashboardViewModel LoadDashboardDataAsync Error: {ex.Message}");
            Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            HandleError($"Error loading dashboard data: {ex.Message}");
            throw; // Re-throw to propagate to InitializeAsync
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Data Loading Methods

    private async Task LoadKpisAsync()
    {
        try
        {
            Console.WriteLine("DashboardViewModel: Loading KPIs...");
            var kpis = await _dashboardService.GetDashboardKpisAsync();
            Console.WriteLine("DashboardViewModel: KPIs loaded successfully");
            
            TodaysSales = kpis.TodaysSales;
            MonthlySales = kpis.MonthlySales;
            GrowthPercentage = kpis.GrowthPercentage;
            ActiveTables = kpis.ActiveTables;
            PendingOrders = kpis.PendingOrders;
            LowStockItems = kpis.LowStockItems;
            TotalCustomers = kpis.TotalCustomers;
            AverageTransactionValue = kpis.AverageTransactionValue;
            TodaysTransactionCount = kpis.TodaysTransactionCount;
            YesterdaysSales = kpis.YesterdaysSales;
            LastMonthSales = kpis.LastMonthSales;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading KPIs: {ex.Message}");
            Console.WriteLine($"KPIs Inner Exception: {ex.InnerException?.Message}");
            Console.WriteLine($"KPIs Stack Trace: {ex.StackTrace}");
            throw; // Re-throw to be caught by LoadDashboardDataAsync
        }
    }

    private async Task LoadPopularProductsAsync()
        {
            try
            {
                var products = await _dashboardService.GetPopularProductsAsync(6, 7);
                
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    PopularProducts.Clear();
                    foreach (var product in products)
                    {
                        PopularProducts.Add(product);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading popular products: {ex.Message}");
            }
        }    private async Task LoadRecentSalesAsync()
        {
            try
            {
                var sales = await _dashboardService.GetRecentSalesAsync(10);
                
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    RecentSales.Clear();
                    foreach (var sale in sales)
                    {
                        RecentSales.Add(sale);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading recent sales: {ex.Message}");
            }
        }    private async Task LoadSalesAnalyticsAsync()
    {
        try
        {
            var analytics = await _dashboardService.GetDailySalesAnalyticsAsync(ChartDays);
            
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                SalesAnalytics.Clear();
                foreach (var item in analytics)
                {
                    SalesAnalytics.Add(item);
                }
                
                // Calculate dynamic max value for better chart scaling
                // Use max sales value + 20% buffer, or minimum of 1000
                if (SalesAnalytics.Any(x => x.Sales > 0))
                {
                    var maxSales = SalesAnalytics.Max(x => x.Sales);
                    MaxSalesValue = Math.Max((double)(maxSales * 1.2m), 1000);
                }
                else
                {
                    MaxSalesValue = 1000; // Default minimum
                }
            });

            Console.WriteLine($"DashboardViewModel: Loaded SalesAnalytics - items={SalesAnalytics.Count}, MaxSalesValue={MaxSalesValue}");
            foreach (var s in SalesAnalytics.Where(x => x.Sales > 0))
            {
                Console.WriteLine($"  SalesAnalytics non-zero: {s.Date:yyyy-MM-dd} Sales={s.Sales}");
            }

            // Also load hourly sales for today
            var hourly = await _dashboardService.GetHourlySalesDistributionAsync();
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                HourlySales.Clear();
                foreach (var item in hourly)
                {
                    HourlySales.Add(item);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading sales analytics: {ex.Message}");
        }
    }

    private async Task LoadTopCategoriesAsync()
        {
            try
            {
                var categories = await _dashboardService.GetTopCategoriesAsync(5, 30);
                
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    TopCategories.Clear();
                    foreach (var category in categories)
                    {
                        TopCategories.Add(category);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading top categories: {ex.Message}");
            }
        }    private async Task LoadCustomerInsightsAsync()
    {
        try
        {
            var insights = await _dashboardService.GetCustomerInsightsAsync();
            
            NewCustomersToday = insights.NewCustomersToday;
            NewCustomersThisWeek = insights.NewCustomersThisWeek;
            NewCustomersThisMonth = insights.NewCustomersThisMonth;
            ReturningCustomerPercentage = insights.ReturningCustomerPercentage;
            ReturningCustomerCount = insights.ReturningCustomerCount;
            CustomerGrowthPercentage = insights.CustomerGrowthPercentage;
            AverageCustomerValue = insights.AverageCustomerValue;
            AverageTransactionsPerCustomer = insights.AverageTransactionsPerCustomer;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                TopCustomers.Clear();
                foreach (var customer in insights.TopCustomers)
                {
                    TopCustomers.Add(customer);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading customer insights: {ex.Message}");
        }
    }

    #endregion

    #region Auto-Refresh

    private void StartAutoRefreshTimers()
    {
        // KPI refresh timer - every 30 seconds
        _kpiRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _kpiRefreshTimer.Tick += async (s, e) => await RefreshKpisAsync();
        _kpiRefreshTimer.Start();

        // Chart refresh timer - every 5 minutes
        _chartRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(5)
        };
        _chartRefreshTimer.Tick += async (s, e) => await RefreshChartsAsync();
        _chartRefreshTimer.Start();

        // Last refresh display update timer - every 30 seconds
        _lastRefreshDisplayTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _lastRefreshDisplayTimer.Tick += (s, e) => UpdateLastRefreshTimeDisplay();
        _lastRefreshDisplayTimer.Start();
    }

    private async Task RefreshKpisAsync()
    {
        try
        {
            await LoadKpisAsync();
            await LoadRecentSalesAsync();
            
            LastRefreshTime = DateTime.Now;
            UpdateLastRefreshTimeDisplay();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing KPIs: {ex.Message}");
        }
    }

    private async Task RefreshChartsAsync()
    {
        try
        {
            await LoadSalesAnalyticsAsync();
            await LoadTopCategoriesAsync();
            await LoadPopularProductsAsync();
            
            LastRefreshTime = DateTime.Now;
            UpdateLastRefreshTimeDisplay();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing charts: {ex.Message}");
        }
    }

    private void UpdateLastRefreshTimeDisplay()
    {
        var elapsed = DateTime.Now - LastRefreshTime;
        if (elapsed.TotalSeconds < 60)
            LastRefreshTimeDisplay = "Just now";
        else if (elapsed.TotalMinutes < 60)
            LastRefreshTimeDisplay = $"{(int)elapsed.TotalMinutes} min ago";
        else
            LastRefreshTimeDisplay = $"{(int)elapsed.TotalHours} hour{((int)elapsed.TotalHours > 1 ? "s" : "")} ago";
    }

    #endregion

    #region Chart Period Commands

    [RelayCommand]
    private async Task ChangeToDailyChart()
    {
        try
        {
            SelectedChartPeriod = "Daily";
            ChartDays = 30;
            await LoadSalesAnalyticsAsync();
        }
        catch (Exception ex)
        {
            HandleError($"Error changing to daily chart: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ChangeToWeeklyChart()
    {
        try
        {
            SelectedChartPeriod = "Weekly";
            var analytics = await _dashboardService.GetWeeklySalesAnalyticsAsync(4);
            
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                SalesAnalytics.Clear();
                foreach (var item in analytics)
                {
                    SalesAnalytics.Add(item);
                }
                
                // Calculate dynamic max value
                if (SalesAnalytics.Any(x => x.Sales > 0))
                {
                    var maxSales = SalesAnalytics.Max(x => x.Sales);
                    MaxSalesValue = Math.Max((double)(maxSales * 1.2m), 1000);
                }
                else
                {
                    MaxSalesValue = 1000;
                }
            });
        }
        catch (Exception ex)
        {
            HandleError($"Error changing to weekly chart: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ChangeToMonthlyChart()
    {
        try
        {
            SelectedChartPeriod = "Monthly";
            var analytics = await _dashboardService.GetMonthlySalesAnalyticsAsync(12);
            
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                SalesAnalytics.Clear();
                foreach (var item in analytics)
                {
                    SalesAnalytics.Add(item);
                }
                
                // Calculate dynamic max value
                if (SalesAnalytics.Any(x => x.Sales > 0))
                {
                    var maxSales = SalesAnalytics.Max(x => x.Sales);
                    MaxSalesValue = Math.Max((double)(maxSales * 1.2m), 1000);
                }
                else
                {
                    MaxSalesValue = 1000;
                }
            });
        }
        catch (Exception ex)
        {
            HandleError($"Error changing to monthly chart: {ex.Message}");
        }
    }

    #endregion

    #region Navigation Commands

    [RelayCommand]
    private void ViewAllProducts()
    {
        NavigateToProductsAction?.Invoke();
    }

    [RelayCommand]
    private void ViewAllSales()
    {
        NavigateToTransactionsAction?.Invoke();
    }

    [RelayCommand]
    private void ViewAllCustomers()
    {
        NavigateToCustomersAction?.Invoke();
    }

    [RelayCommand]
    private void ViewLowStockItems()
    {
        NavigateToStockManagementAction?.Invoke();
    }

    [RelayCommand]
    private void CreateNewSale()
    {
        NavigateToNewSaleAction?.Invoke();
    }

    [RelayCommand]
    private void AddProduct()
    {
        NavigateToProductsAction?.Invoke();
    }

    [RelayCommand]
    private void ViewCustomers()
    {
        NavigateToCustomersAction?.Invoke();
    }

    [RelayCommand]
    private void GenerateReport()
    {
        NavigateToReportsAction?.Invoke();
    }

    #endregion

    #region Error Handling

    private void HandleError(string message)
    {
        HasError = true;
        ErrorMessage = message;
        Console.WriteLine($"Dashboard Error: {message}");
    }

    [RelayCommand]
    private void ClearError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Cleanup resources - called when view is unloaded
    /// </summary>
    public void Cleanup()
    {
        _kpiRefreshTimer?.Stop();
        _chartRefreshTimer?.Stop();
        _lastRefreshDisplayTimer?.Stop();
        
        Console.WriteLine("DashboardViewModel: Cleanup completed - all timers stopped");
    }

    #endregion
}
