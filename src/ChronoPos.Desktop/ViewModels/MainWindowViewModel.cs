using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using ChronoPos.Application.Interfaces;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// Main window view model for the ChronoPos Desktop application
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IProductService _productService;

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

    public MainWindowViewModel(IProductService productService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        
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
    private void ShowPointOfSale()
    {
        CurrentPageTitle = "Point of Sale";
        StatusMessage = "Point of Sale interface loaded";
        
        var posContent = new System.Windows.Controls.Grid();
        posContent.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(2, System.Windows.GridUnitType.Star) });
        posContent.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
        
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
                Text = "Shopping Cart\n(Cart items will be shown here)",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            }
        };
        System.Windows.Controls.Grid.SetColumn(cartArea, 1);
        
        posContent.Children.Add(productArea);
        posContent.Children.Add(cartArea);
        CurrentView = posContent;
    }

    [RelayCommand]
    private async Task ShowProducts()
    {
        CurrentPageTitle = "Products Management";
        StatusMessage = "Loading products...";
        
        try
        {
            var products = await _productService.GetAllProductsAsync();
            
            var productsContent = new System.Windows.Controls.StackPanel();
            productsContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = $"Products ({products.Count()})", 
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
                    
                    productsContent.Children.Add(productPanel);
                }
            }
            else
            {
                productsContent.Children.Add(new System.Windows.Controls.TextBlock 
                { 
                    Text = "No products found. Add some products to get started.",
                    FontStyle = System.Windows.FontStyles.Italic,
                    Margin = new System.Windows.Thickness(0, 20, 0, 0)
                });
            }
            
            CurrentView = productsContent;
            StatusMessage = $"Loaded {products.Count()} products";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading products: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ShowCustomers()
    {
        CurrentPageTitle = "Customers Management";
        StatusMessage = "Customers interface loaded";
        
        var customersContent = new System.Windows.Controls.TextBlock
        {
            Text = "Customers Management\n(Customer management interface will be implemented here)",
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            FontSize = 16
        };
        CurrentView = customersContent;
    }

    [RelayCommand]
    private void ShowSales()
    {
        CurrentPageTitle = "Sales History";
        StatusMessage = "Sales history loaded";
        
        var salesContent = new System.Windows.Controls.TextBlock
        {
            Text = "Sales History\n(Sales history and reports will be shown here)",
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            FontSize = 16
        };
        CurrentView = salesContent;
    }

    [RelayCommand]
    private void ShowReports()
    {
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
        CurrentPageTitle = "Settings";
        StatusMessage = "Settings loaded";
        
        var settingsContent = new System.Windows.Controls.TextBlock
        {
            Text = "Application Settings\n(System configuration options will be available here)",
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            FontSize = 16
        };
        CurrentView = settingsContent;
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
