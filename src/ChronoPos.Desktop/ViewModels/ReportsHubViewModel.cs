using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using ChronoPos.Infrastructure.Services;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Reports Hub screen
/// </summary>
public partial class ReportsHubViewModel : ObservableObject
{
    private readonly IDatabaseLocalizationService _localizationService;
    private readonly ILayoutDirectionService _layoutDirectionService;

    // Translation properties
    [ObservableProperty]
    private string _reportsTitle = "Reports";

    [ObservableProperty]
    private string _reportsSubtitle = "Access comprehensive business reports and analytics";

    [ObservableProperty]
    private string _salesReportTitle = "Sales Report";

    [ObservableProperty]
    private string _salesReportDescription = "View comprehensive sales analytics including daily, monthly, and yearly reports. Analyze product performance, payment methods, and customer insights.";

    [ObservableProperty]
    private string _inventoryReportTitle = "Inventory Report";

    [ObservableProperty]
    private string _inventoryReportDescription = "Monitor stock levels, track inventory movements, identify low stock items, and analyze stock valuation across all products.";

    [ObservableProperty]
    private string _cashReportTitle = "Cash Report";

    [ObservableProperty]
    private string _cashReportDescription = "Track cash flow, analyze payment methods, review shift summaries, and monitor cash drawer reconciliation.";

    [ObservableProperty]
    private string _customerReportTitle = "Customer Report";

    [ObservableProperty]
    private string _customerReportDescription = "Analyze customer purchase patterns, identify top customers, track loyalty metrics, and review customer lifetime value.";

    [ObservableProperty]
    private string _viewReportButtonLabel = "View Report →";

    [ObservableProperty]
    private string _comingSoonLabel = "Coming Soon";

    public ReportsHubViewModel(
        IDatabaseLocalizationService localizationService,
        ILayoutDirectionService layoutDirectionService)
    {
        _localizationService = localizationService;
        _layoutDirectionService = layoutDirectionService;

        // Subscribe to language changes
        _localizationService.LanguageChanged += OnLanguageChanged;

        // Load initial translations
        _ = LoadTranslationsAsync();
    }

    private async System.Threading.Tasks.Task LoadTranslationsAsync()
    {
        try
        {
            ReportsTitle = await _localizationService.GetTranslationAsync("reports_hub_title") ?? "Reports";
            ReportsSubtitle = await _localizationService.GetTranslationAsync("reports_hub_subtitle") ?? "Access comprehensive business reports and analytics";
            SalesReportTitle = await _localizationService.GetTranslationAsync("reports_hub_sales_title") ?? "Sales Report";
            SalesReportDescription = await _localizationService.GetTranslationAsync("reports_hub_sales_description") ?? "View comprehensive sales analytics including daily, monthly, and yearly reports. Analyze product performance, payment methods, and customer insights.";
            InventoryReportTitle = await _localizationService.GetTranslationAsync("reports_hub_inventory_title") ?? "Inventory Report";
            InventoryReportDescription = await _localizationService.GetTranslationAsync("reports_hub_inventory_description") ?? "Monitor stock levels, track inventory movements, identify low stock items, and analyze stock valuation across all products.";
            CashReportTitle = await _localizationService.GetTranslationAsync("reports_hub_cash_title") ?? "Cash Report";
            CashReportDescription = await _localizationService.GetTranslationAsync("reports_hub_cash_description") ?? "Track cash flow, analyze payment methods, review shift summaries, and monitor cash drawer reconciliation.";
            CustomerReportTitle = await _localizationService.GetTranslationAsync("reports_hub_customer_title") ?? "Customer Report";
            CustomerReportDescription = await _localizationService.GetTranslationAsync("reports_hub_customer_description") ?? "Analyze customer purchase patterns, identify top customers, track loyalty metrics, and review customer lifetime value.";
            ViewReportButtonLabel = await _localizationService.GetTranslationAsync("reports_hub_view_report") ?? "View Report →";
            ComingSoonLabel = await _localizationService.GetTranslationAsync("reports_hub_coming_soon") ?? "Coming Soon";
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading translations: {ex.Message}");
        }
    }

    private void OnLanguageChanged(object? sender, string languageCode)
    {
        _ = LoadTranslationsAsync();
    }
    [RelayCommand]
    private void NavigateToSalesReport()
    {
        // Navigate to Sales Report View
        var mainWindow = System.Windows.Application.Current.MainWindow as Views.MainWindow;
        if (mainWindow?.DataContext is MainWindowViewModel mainViewModel)
        {
            var salesReportViewModel = System.Windows.Application.Current.TryFindResource("SalesReportViewModel") as SalesReportViewModel;
            if (salesReportViewModel == null)
            {
                // Create new instance if not found
                salesReportViewModel = new SalesReportViewModel(
                    App.GetService<Application.Interfaces.ISalesReportService>(),
                    App.GetService<Application.Interfaces.ICategoryService>(),
                    App.GetService<Desktop.Services.IActiveCurrencyService>()
                );
            }
            
            var salesReportView = new Views.SalesReportView
            {
                DataContext = salesReportViewModel
            };
            mainViewModel.NavigateToView(salesReportView);
        }
    }

    [RelayCommand]
    private void NavigateToInventoryReport()
    {
        // Navigate to Inventory Report View
        var mainWindow = System.Windows.Application.Current.MainWindow as Views.MainWindow;
        if (mainWindow?.DataContext is MainWindowViewModel mainViewModel)
        {
            var inventoryReportViewModel = System.Windows.Application.Current.TryFindResource("InventoryReportViewModel") as InventoryReportViewModel;
            if (inventoryReportViewModel == null)
            {
                // Create new instance if not found
                inventoryReportViewModel = new InventoryReportViewModel(
                    App.GetService<Application.Interfaces.IInventoryReportService>(),
                    App.GetService<Application.Interfaces.IProductService>(),
                    App.GetService<Application.Interfaces.ICategoryService>()
                );
            }
            
            var inventoryReportView = new Views.InventoryReportView
            {
                DataContext = inventoryReportViewModel
            };
            mainViewModel.NavigateToView(inventoryReportView);
        }
    }

    [RelayCommand]
    private void NavigateToCashReport()
    {
        // TODO: Implement when Cash Report is ready
        MessageBox.Show("Cash Report - Coming Soon!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void NavigateToCustomerReport()
    {
        // Navigate to Customer Report View
        var mainWindow = System.Windows.Application.Current.MainWindow as Views.MainWindow;
        if (mainWindow?.DataContext is MainWindowViewModel mainViewModel)
        {
            var customerReportViewModel = System.Windows.Application.Current.TryFindResource("CustomerReportViewModel") as CustomerReportViewModel;
            if (customerReportViewModel == null)
            {
                // Create new instance if not found
                customerReportViewModel = new CustomerReportViewModel(
                    App.GetService<Application.Interfaces.ICustomerReportService>(),
                    App.GetService<Desktop.Services.IActiveCurrencyService>()
                );
            }
            
            var customerReportView = new Views.CustomerReportView
            {
                DataContext = customerReportViewModel
            };
            mainViewModel.NavigateToView(customerReportView);
        }
    }
}
