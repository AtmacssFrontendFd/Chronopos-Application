using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Reports Hub screen
/// </summary>
public partial class ReportsHubViewModel : ObservableObject
{
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
