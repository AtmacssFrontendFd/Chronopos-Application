using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// Main window view model for the ChronoPos Desktop application
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string currentPageTitle = "Dashboard";

    [ObservableProperty]
    private object? currentView;

    [ObservableProperty]
    private string currentDateTime = DateTime.Now.ToString("dddd, MMMM dd, yyyy - HH:mm:ss");

    [ObservableProperty]
    private string currentUser = "Administrator";

    [ObservableProperty]
    private string statusMessage = "System Ready";

    [ObservableProperty]
    private string selectedPage = "Dashboard";

    public MainWindowViewModel(IProductService productService, IServiceProvider serviceProvider)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        
        // Start timer to update date/time
        var timer = new System.Windows.Threading.DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += (s, e) => CurrentDateTime = DateTime.Now.ToString("dddd, MMMM dd, yyyy - HH:mm:ss");
        timer.Start();

        // Show dashboard by default
        ShowDashboard();
    }

    [RelayCommand]
    private void ShowDashboard()
    {
        SelectedPage = "Dashboard";
        CurrentPageTitle = "Dashboard";
        StatusMessage = "Dashboard loaded";
        
        // Create dashboard view content
        var dashboardContent = new System.Windows.Controls.StackPanel();
        dashboardContent.Children.Add(new System.Windows.Controls.TextBlock 
        { 
            Text = "Welcome to ChronoPos Point of Sale System", 
            FontSize = 18, 
            Margin = new System.Windows.Thickness(0, 0, 0, 20) 
        });
        
        var statsPanel = new System.Windows.Controls.WrapPanel();
        
        // Quick stats cards
        var todaySalesCard = CreateStatsCard("Today's Sales", "$0.00", "#FF4CAF50");
        var productsCard = CreateStatsCard("Total Products", "0", "#FF2196F3");
        var customersCard = CreateStatsCard("Total Customers", "0", "#FFFF9800");
        
        statsPanel.Children.Add(todaySalesCard);
        statsPanel.Children.Add(productsCard);
        statsPanel.Children.Add(customersCard);
        
        dashboardContent.Children.Add(statsPanel);
        CurrentView = dashboardContent;
    }

    [RelayCommand]
    private void ShowTransactions()
    {
        SelectedPage = "Transactions";
        CurrentPageTitle = "Transactions";
        StatusMessage = "Transactions interface loaded";
        
        var transactionsContent = new System.Windows.Controls.Grid();
        transactionsContent.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(2, System.Windows.GridUnitType.Star) });
        transactionsContent.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
        
        // Product selection area
        var productArea = new System.Windows.Controls.Border
        {
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray),
            CornerRadius = new System.Windows.CornerRadius(5),
            Margin = new System.Windows.Thickness(0, 0, 10, 0),
            Child = new System.Windows.Controls.TextBlock
            {
                Text = "Product Selection Area\n(Products will be loaded here)",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            }
        };
        System.Windows.Controls.Grid.SetColumn(productArea, 0);
        
        // Cart area
        var cartArea = new System.Windows.Controls.Border
        {
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightBlue),
            CornerRadius = new System.Windows.CornerRadius(5),
            Child = new System.Windows.Controls.TextBlock
            {
                Text = "Transaction Cart\n(Cart items will be shown here)",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            }
        };
        System.Windows.Controls.Grid.SetColumn(cartArea, 1);
        
        transactionsContent.Children.Add(productArea);
        transactionsContent.Children.Add(cartArea);
        CurrentView = transactionsContent;
    }

    [RelayCommand]
    private async Task ShowManagement()
    {
        SelectedPage = "Management";
        CurrentPageTitle = "Management";
        StatusMessage = "Loading management...";
        
        try
        {
            var products = await _productService.GetAllProductsAsync();
            
            var managementContent = new System.Windows.Controls.StackPanel();
            managementContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = $"Management ({products.Count()})", 
                FontSize = 16, 
                FontWeight = System.Windows.FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 0, 0, 10) 
            });
            
            if (products.Any())
            {
                foreach (var product in products)
                {
                    var productPanel = new System.Windows.Controls.StackPanel
                    {
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        Margin = new System.Windows.Thickness(0, 5, 0, 5)
                    };
                    
                    productPanel.Children.Add(new System.Windows.Controls.TextBlock 
                    { 
                        Text = $"{product.Name} - ${product.Price:F2} (Stock: {product.StockQuantity})",
                        Width = 300
                    });
                    
                    managementContent.Children.Add(productPanel);
                }
            }
            else
            {
                managementContent.Children.Add(new System.Windows.Controls.TextBlock 
                { 
                    Text = "No products found. Add some products to get started.",
                    FontStyle = System.Windows.FontStyles.Italic,
                    Margin = new System.Windows.Thickness(0, 20, 0, 0)
                });
            }
            
            CurrentView = managementContent;
            StatusMessage = $"Loaded {products.Count()} products";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading products: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ShowReservation()
    {
        SelectedPage = "Reservation";
        CurrentPageTitle = "Reservation Management";
        StatusMessage = "Reservation interface loaded";
        
        var reservationContent = new System.Windows.Controls.TextBlock
        {
            Text = "Reservation Management\n(Customer reservation interface will be implemented here)",
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            FontSize = 16
        };
        CurrentView = reservationContent;
    }

    [RelayCommand]
    private void ShowOrderTable()
    {
        SelectedPage = "OrderTable";
        CurrentPageTitle = "Order Table";
        StatusMessage = "Order table loaded";
        
        var orderTableContent = new System.Windows.Controls.TextBlock
        {
            Text = "Order Table\n(Order history and management will be shown here)",
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            FontSize = 16
        };
        CurrentView = orderTableContent;
    }

    [RelayCommand]
    private void ShowReports()
    {
        SelectedPage = "Reports";
        CurrentPageTitle = "Reports";
        StatusMessage = "Reports interface loaded";
        
        var reportsContent = new System.Windows.Controls.TextBlock
        {
            Text = "Reports & Analytics\n(Business reports and analytics will be displayed here)",
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            FontSize = 16
        };
        CurrentView = reportsContent;
    }

    [RelayCommand]
    private void ShowSettings()
    {
        SelectedPage = "Settings";
        CurrentPageTitle = "Settings";
        StatusMessage = "Loading settings...";
        
        try
        {
            Console.WriteLine("ShowSettings: Starting to load settings");
            
            // Create and configure the settings view
            Console.WriteLine("ShowSettings: Creating SettingsView");
            var settingsView = new SettingsView();
            Console.WriteLine("ShowSettings: SettingsView created successfully");
            
            Console.WriteLine("ShowSettings: Getting SettingsViewModel from service provider");
            var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
            Console.WriteLine("ShowSettings: SettingsViewModel retrieved successfully");
            
            Console.WriteLine("ShowSettings: Setting DataContext");
            settingsView.DataContext = settingsViewModel;
            Console.WriteLine("ShowSettings: DataContext set successfully");
            
            Console.WriteLine("ShowSettings: Setting CurrentView");
            CurrentView = settingsView;
            Console.WriteLine("ShowSettings: CurrentView set successfully");
            
            StatusMessage = "Settings loaded";
            Console.WriteLine("ShowSettings: Settings loaded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ShowSettings: Error occurred - {ex.Message}");
            Console.WriteLine($"ShowSettings: Stack trace - {ex.StackTrace}");
            StatusMessage = $"Error loading settings: {ex.Message}";
            System.Windows.MessageBox.Show($"Failed to load settings: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", 
                "Settings Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Logout()
    {
        var result = System.Windows.MessageBox.Show(
            "Are you sure you want to logout?", 
            "Confirm Logout", 
            System.Windows.MessageBoxButton.YesNo, 
            System.Windows.MessageBoxImage.Question);
        
        if (result == System.Windows.MessageBoxResult.Yes)
        {
            StatusMessage = "Logging out...";
            
            // Here you can add any cleanup logic like:
            // - Clear user session
            // - Save any pending data
            // - Clear sensitive information
            
            // For now, we'll just show a confirmation and reset to dashboard
            System.Windows.MessageBox.Show(
                "You have been successfully logged out.", 
                "Logout Complete", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Information);
            
            // Reset to dashboard
            ShowDashboard();
            CurrentUser = "Guest";
            StatusMessage = "Please login to continue";
        }
    }

    private System.Windows.Controls.Border CreateStatsCard(string title, string value, string colorHex)
    {
        var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorHex);
        
        var card = new System.Windows.Controls.Border
        {
            Width = 200,
            Height = 100,
            Background = new System.Windows.Media.SolidColorBrush(color),
            CornerRadius = new System.Windows.CornerRadius(5),
            Margin = new System.Windows.Thickness(0, 0, 15, 15)
        };
        
        var stackPanel = new System.Windows.Controls.StackPanel
        {
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        
        stackPanel.Children.Add(new System.Windows.Controls.TextBlock
        {
            Text = value,
            FontSize = 24,
            FontWeight = System.Windows.FontWeights.Bold,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        });
        
        stackPanel.Children.Add(new System.Windows.Controls.TextBlock
        {
            Text = title,
            FontSize = 12,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        });
        
        card.Child = stackPanel;
        return card;
    }
}
