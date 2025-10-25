using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;
using ChronoPos.Application.Logging;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for Add Sales screen
/// </summary>
public partial class AddSalesViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ICustomerService _customerService;
    private readonly ITransactionService _transactionService;
    private readonly IRestaurantTableService _tableService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IShiftService _shiftService;
    private readonly IDiscountService _discountService;
    private readonly ITaxTypeService _taxTypeService;
    private readonly IRefundService _refundService;

    #region Observable Properties

    [ObservableProperty]
    private string currentUser = "Administrator";

    [ObservableProperty]
    private string tableNumber = "01";

    [ObservableProperty]
    private string customerName = string.Empty;

    [ObservableProperty]
    private string customerPhone = string.Empty;

    [ObservableProperty]
    private CustomerDto? selectedCustomer;

    [ObservableProperty]
    private RestaurantTableDto? selectedTable;

    [ObservableProperty]
    private decimal subtotal = 0m;

    [ObservableProperty]
    private decimal taxPercentage = 0m; // No default tax, must be explicitly added

    [ObservableProperty]
    private decimal taxAmount = 0m;

    [ObservableProperty]
    private decimal total = 0m;

    [ObservableProperty]
    private ObservableCollection<CategoryDisplayModel> categories = new();

    [ObservableProperty]
    private ObservableCollection<ProductDisplayModel> filteredProducts = new();

    [ObservableProperty]
    private ObservableCollection<CustomerDto> customers = new();

    [ObservableProperty]
    private ObservableCollection<RestaurantTableDto> tables = new();

    [ObservableProperty]
    private ObservableCollection<CartItemModel> cartItems = new();

    [ObservableProperty]
    private CategoryDisplayModel? selectedCategory;

    [ObservableProperty]
    private decimal discountAmount = 0m;

    [ObservableProperty]
    private decimal containerCharge = 0m;

    [ObservableProperty]
    private ObservableCollection<DiscountDto> availableDiscounts = new();

    [ObservableProperty]
    private ObservableCollection<DiscountDto> selectedDiscounts = new();

    [ObservableProperty]
    private ObservableCollection<TaxTypeDto> availableTaxTypes = new();

    [ObservableProperty]
    private ObservableCollection<TaxTypeDto> selectedTaxTypes = new();

    [ObservableProperty]
    private int currentTransactionId = 0; // 0 means new transaction

    [ObservableProperty]
    private string currentTransactionStatus = "draft";

    // Button visibility properties
    [ObservableProperty]
    private bool showSaveButton = true; // Show only for new transactions

    [ObservableProperty]
    private bool showSaveAndPrintButton = false; // Show for draft/billed/hold/pending/partial

    [ObservableProperty]
    private bool showSettleButton = false; // Show for draft/billed/hold/pending/partial

    [ObservableProperty]
    private bool showRefundButton = false; // Show only for settled

    [ObservableProperty]
    private bool showExchangeButton = false; // Show only for settled

    [ObservableProperty]
    private bool isRefundMode = false; // Indicates if we're in refund mode

    [ObservableProperty]
    private int refundSourceTransactionId = 0; // Source transaction ID for refund

    #endregion

    public AddSalesViewModel(
        IProductService productService,
        ICategoryService categoryService,
        ICustomerService customerService,
        ITransactionService transactionService,
        IRestaurantTableService tableService,
        ICurrentUserService currentUserService,
        IShiftService shiftService,
        IDiscountService discountService,
        ITaxTypeService taxTypeService,
        IRefundService refundService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _customerService = customerService;
        _transactionService = transactionService;
        _tableService = tableService;
        _currentUserService = currentUserService;
        _shiftService = shiftService;
        _discountService = discountService;
        _taxTypeService = taxTypeService;
        _refundService = refundService;

        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Load categories with product counts
            await LoadCategoriesAsync();

            // Load all products initially
            await LoadProductsAsync();

            // Load customers for dropdown
            await LoadCustomersAsync();

            // Load restaurant tables
            await LoadTablesAsync();

            // Load discounts
            await LoadDiscountsAsync();

            // Load tax types
            await LoadTaxTypesAsync();

            // Set default customer info
            if (Customers.Any())
            {
                SelectedCustomer = Customers.FirstOrDefault();
            }
            
            // Set default table
            if (Tables.Any())
            {
                SelectedTable = Tables.FirstOrDefault();
            }

            // Initialize button visibility for new transaction
            UpdateButtonVisibility();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing Add Sales screen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadTablesAsync()
    {
        try
        {
            var tablesData = await _tableService.GetAllAsync();
            
            Tables.Clear();
            foreach (var table in tablesData)
            {
                Tables.Add(table);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading tables: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categoriesData = await _categoryService.GetAllAsync();
            
            // Generic category icon (suitable for all industries)
            var defaultIcon = "📦"; // Generic box/package icon

            Categories.Clear();
            
            foreach (var category in categoriesData)
            {
                Categories.Add(new CategoryDisplayModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Icon = defaultIcon, // Use generic icon for all categories
                    ItemCount = await GetProductCountForCategoryAsync(category.Id),
                    IsSelected = false
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task<int> GetProductCountForCategoryAsync(int categoryId)
    {
        try
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            return products.Count();
        }
        catch
        {
            return 0;
        }
    }

    private async Task LoadProductsAsync(int? categoryId = null)
    {
        try
        {
            IEnumerable<ProductDto> products;

            if (categoryId.HasValue)
            {
                products = await _productService.GetProductsByCategoryAsync(categoryId.Value);
            }
            else
            {
                products = await _productService.GetAllProductsAsync();
            }

            FilteredProducts.Clear();

            foreach (var product in products.Take(20)) // Limit to 20 products for performance
            {
                FilteredProducts.Add(new ProductDisplayModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = 0,
                    CategoryId = product.CategoryId
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            var customersData = await _customerService.GetAllAsync();
            
            Customers.Clear();
            foreach (var customer in customersData) // Load all customers
            {
                Customers.Add(customer);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadDiscountsAsync()
    {
        try
        {
            var discounts = await _discountService.GetAllAsync();
            
            AvailableDiscounts.Clear();
            foreach (var discount in discounts.Where(d => d.IsActive))
            {
                AvailableDiscounts.Add(discount);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading discounts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadTaxTypesAsync()
    {
        try
        {
            var taxTypes = await _taxTypeService.GetAllAsync();
            
            AvailableTaxTypes.Clear();
            foreach (var taxType in taxTypes.Where(t => t.IsActive))
            {
                AvailableTaxTypes.Add(taxType);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading tax types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task SelectCategory(CategoryDisplayModel category)
    {
        if (category == null) return;

        // Deselect all categories first
        foreach (var cat in Categories)
        {
            cat.IsSelected = false;
        }

        // Select the clicked category
        category.IsSelected = true;
        SelectedCategory = category;
        
        // Load products for the selected category
        try
        {
            var products = await _productService.GetProductsByCategoryAsync(category.Id);
            
            FilteredProducts.Clear();
            foreach (var product in products)
            {
                FilteredProducts.Add(new ProductDisplayModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = 0,
                    CategoryId = product.CategoryId
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading products for category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Show all products from all categories
    /// </summary>
    [RelayCommand]
    private async Task ShowAllProducts()
    {
        try
        {
            // Deselect all categories
            foreach (var cat in Categories)
            {
                cat.IsSelected = false;
            }
            SelectedCategory = null;

            // Load all products
            var allProducts = await _productService.GetAllProductsAsync();
            
            FilteredProducts.Clear();
            foreach (var product in allProducts)
            {
                FilteredProducts.Add(new ProductDisplayModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = 0,
                    CategoryId = product.CategoryId
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading all products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task IncreaseQuantity(ProductDisplayModel product)
    {
        if (product == null) return;

        // Fetch the full product details including stock information
        var productDetails = await _productService.GetProductByIdAsync(product.Id);
        if (productDetails == null) return;

        var requestedQuantity = product.Quantity + 1;

        // Check if stock tracking is enabled for this product
        if (productDetails.IsStockTracked)
        {
            // Check available stock (using InitialStock as the current available quantity)
            if (requestedQuantity > productDetails.InitialStock)
            {
                // Stock is insufficient
                if (!productDetails.AllowNegativeStock)
                {
                    // Restrict user from adding more
                    MessageBox.Show(
                        $"Insufficient stock for '{productDetails.Name}'.\n\n" +
                        $"Available: {productDetails.InitialStock}\n" +
                        $"Requested: {requestedQuantity}\n\n" +
                        $"Cannot add more items.",
                        "Stock Unavailable",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    // Allow negative stock but show warning
                    var result = MessageBox.Show(
                        $"Warning: Insufficient stock for '{productDetails.Name}'.\n\n" +
                        $"Available: {productDetails.InitialStock}\n" +
                        $"Requested: {requestedQuantity}\n" +
                        $"Shortage: {requestedQuantity - productDetails.InitialStock}\n\n" +
                        $"This product allows negative stock.\n" +
                        $"Do you want to continue?",
                        "Stock Warning",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    
                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
            }
        }

        product.Quantity++;

        // Add or update in cart
        var existingCartItem = CartItems.FirstOrDefault(c => c.ProductId == product.Id);
        if (existingCartItem != null)
        {
            existingCartItem.Quantity = product.Quantity;
            existingCartItem.TotalPrice = existingCartItem.UnitPrice * existingCartItem.Quantity;
        }
        else
        {
            var newCartItem = new CartItemModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Icon = "📦",
                UnitPrice = product.Price,
                Quantity = product.Quantity,
                TotalPrice = product.Price * product.Quantity
            };
            
            // Subscribe to property changes to auto-recalculate totals
            newCartItem.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(CartItemModel.TotalPrice))
                {
                    RecalculateTotals();
                }
            };
            
            CartItems.Add(newCartItem);
        }

        RecalculateTotals();
    }

    [RelayCommand]
    private void DecreaseQuantity(ProductDisplayModel product)
    {
        if (product == null || product.Quantity <= 0) return;

        product.Quantity--;

        // Update or remove from cart
        var existingCartItem = CartItems.FirstOrDefault(c => c.ProductId == product.Id);
        if (existingCartItem != null)
        {
            if (product.Quantity == 0)
            {
                CartItems.Remove(existingCartItem);
            }
            else
            {
                existingCartItem.Quantity = product.Quantity;
                existingCartItem.TotalPrice = existingCartItem.UnitPrice * existingCartItem.Quantity;
            }
        }

        RecalculateTotals();
    }

    [RelayCommand]
    private void RemoveFromCart(CartItemModel cartItem)
    {
        if (cartItem == null) return;

        // Remove from cart
        CartItems.Remove(cartItem);

        // Reset product quantity in the product list
        var product = FilteredProducts.FirstOrDefault(p => p.Id == cartItem.ProductId);
        if (product != null)
        {
            product.Quantity = 0;
        }

        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        Subtotal = CartItems.Sum(c => c.TotalPrice);
        TaxAmount = Subtotal * (TaxPercentage / 100);
        Total = Subtotal + TaxAmount + ContainerCharge - DiscountAmount;
    }

    [RelayCommand]
    private void EditTable()
    {
        // TODO: Implement table selection dialog
        MessageBox.Show("Table selection feature coming soon!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private async Task SaveTransaction()
    {
        try
        {
            AppLogger.Log("SaveTransaction: Starting transaction save process");
            
            if (!CartItems.Any())
            {
                MessageBox.Show("Please add items to the cart before saving.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get current user
            var currentUserId = _currentUserService.CurrentUserId;
            AppLogger.Log($"SaveTransaction: Current user ID = {currentUserId}");
            
            if (!currentUserId.HasValue || currentUserId.Value <= 0)
            {
                MessageBox.Show("Unable to identify current user. Please log in again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Use hardcoded shift ID for now (shift management will be implemented later)
            var shiftId = 1;
            AppLogger.Log($"SaveTransaction: Using hardcoded shift ID = {shiftId}");

            // Calculate totals
            var totalAmount = CartItems.Sum(x => x.TotalPrice);
            var totalVat = totalAmount * (TaxPercentage / 100);
            
            AppLogger.Log($"SaveTransaction: Total Amount = {totalAmount}, Total VAT = {totalVat}");
            AppLogger.Log($"SaveTransaction: Cart items count = {CartItems.Count}");

            // For now, always create new transaction
            // TODO: Implement proper transaction update when products change
            if (CurrentTransactionId > 0)
            {
                MessageBox.Show("Updating existing transactions with product changes is not yet supported.\nPlease create a new transaction instead.", 
                    "Not Supported", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                // Create new transaction
                var transactionDto = new CreateTransactionDto
                {
                    ShiftId = shiftId,
                    UserId = currentUserId.Value,
                    CustomerId = SelectedCustomer?.Id,
                    TableId = SelectedTable?.Id,
                    SellingTime = DateTime.Now,
                    TotalAmount = totalAmount,
                    TotalVat = totalVat,
                    TotalDiscount = DiscountAmount,
                    AmountPaidCash = 0m,
                    AmountCreditRemaining = 0m,
                    Vat = TaxPercentage,
                    Status = "draft",
                    Products = CartItems.Select(item => new CreateTransactionProductDto
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        SellingPrice = item.UnitPrice,
                        BuyerCost = 0m,
                        Vat = TaxPercentage
                    }).ToList()
                };

                AppLogger.Log($"SaveTransaction: Transaction DTO details:");
                AppLogger.Log($"  ShiftId={transactionDto.ShiftId}");
                AppLogger.Log($"  UserId={transactionDto.UserId}");
                AppLogger.Log($"  CustomerId={transactionDto.CustomerId?.ToString() ?? "NULL"}");
                AppLogger.Log($"  TableId={transactionDto.TableId?.ToString() ?? "NULL"}");
                AppLogger.Log($"  Products count={transactionDto.Products.Count}");
                foreach (var product in transactionDto.Products)
                {
                    AppLogger.Log($"    Product: ID={product.ProductId}, Qty={product.Quantity}, Price={product.SellingPrice}");
                }

                var savedTransaction = await _transactionService.CreateAsync(transactionDto, currentUserId.Value);
                
                // Update CurrentTransactionId and status after saving
                CurrentTransactionId = savedTransaction.Id;
                CurrentTransactionStatus = savedTransaction.Status;
                UpdateButtonVisibility();

                AppLogger.Log($"SaveTransaction: Transaction #{savedTransaction.Id} created successfully");
                MessageBox.Show($"Transaction #{savedTransaction.Id} saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Don't clear cart after save - user might want to continue editing
        }
        catch (Exception ex)
        {
            AppLogger.LogError("SaveTransaction: Error saving transaction", ex);
            var errorMessage = $"Error saving transaction: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nDetails: {ex.InnerException.Message}";
                AppLogger.LogError($"SaveTransaction: Inner exception - {ex.InnerException.Message}", ex.InnerException);
            }
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ShowAddCustomerPopup()
    {
        try
        {
            // Create a simple input dialog for customer details
            var dialog = new Window
            {
                Title = "Quick Add Customer",
                Width = 450,
                Height = 380, // Increased height to fully show Save button
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            // Customer Name
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Customer Name *", 
                FontSize = 12, 
                Margin = new Thickness(0, 0, 0, 5) 
            });
            var nameTextBox = new TextBox 
            { 
                Height = 35, 
                FontSize = 14, 
                Margin = new Thickness(0, 0, 0, 15) 
            };
            stackPanel.Children.Add(nameTextBox);

            // Mobile Number
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Mobile Number *", 
                FontSize = 12, 
                Margin = new Thickness(0, 0, 0, 5) 
            });
            var mobileTextBox = new TextBox 
            { 
                Height = 35, 
                FontSize = 14, 
                Margin = new Thickness(0, 0, 0, 15) 
            };
            stackPanel.Children.Add(mobileTextBox);

            // Email (optional)
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Email (Optional)", 
                FontSize = 12, 
                Margin = new Thickness(0, 0, 0, 5) 
            });
            var emailTextBox = new TextBox 
            { 
                Height = 35, 
                FontSize = 14, 
                Margin = new Thickness(0, 0, 0, 15) 
            };
            stackPanel.Children.Add(emailTextBox);

            // Buttons
            var buttonPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var saveButton = new Button 
            { 
                Content = "Save", 
                Width = 80, 
                Height = 35, 
                Margin = new Thickness(0, 0, 10, 0),
                Background = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                Foreground = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };

            var cancelButton = new Button 
            { 
                Content = "Cancel", 
                Width = 80, 
                Height = 35,
                Background = new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                Foreground = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };

            saveButton.Click += async (s, e) =>
            {
                var name = nameTextBox.Text?.Trim();
                var mobile = mobileTextBox.Text?.Trim();
                var email = emailTextBox.Text?.Trim();

                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("Customer name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(mobile))
                {
                    MessageBox.Show("Mobile number is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    // Create new customer
                    var newCustomer = new CustomerDto
                    {
                        CustomerFullName = name,
                        MobileNo = mobile,
                        OfficialEmail = email ?? string.Empty,
                        Status = "Active"
                    };

                    await _customerService.CreateCustomerAsync(newCustomer);

                    MessageBox.Show("Customer added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Reload customers
                    await LoadCustomersAsync();

                    // Select the newly added customer
                    SelectedCustomer = Customers.FirstOrDefault(c => c.MobileNo == mobile);

                    dialog.DialogResult = true;
                    dialog.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            cancelButton.Click += (s, e) =>
            {
                dialog.DialogResult = false;
                dialog.Close();
            };

            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            dialog.Content = stackPanel;
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ShowDiscountPopup()
    {
        try
        {
            // Create dialog window - increased by 20% more
            var dialog = new Window
            {
                Title = "Apply Discounts",
                Width = SystemParameters.PrimaryScreenWidth * 0.3,
                Height = SystemParameters.PrimaryScreenHeight * 0.43, // Increased to show apply button
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var mainPanel = new StackPanel { Margin = new Thickness(15) }; // Reduced margin

            // Title
            mainPanel.Children.Add(new TextBlock 
            { 
                Text = "Select Discounts", 
                FontSize = 14, // Reduced font size
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 10) // Reduced margin
            });

            // Dropdown + Add Button Row
            var dropdownPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8) // Reduced margin
            };

            var discountComboBox = new System.Windows.Controls.ComboBox
            {
                Width = 200, // Reduced width
                Height = 32, // Reduced height
                Margin = new Thickness(0, 0, 8, 0), // Reduced margin
                DisplayMemberPath = "DiscountName",
                ItemsSource = AvailableDiscounts
            };

            var addButton = new Button
            {
                Content = "Add",
                Width = 80, // Reduced width
                Height = 32, // Reduced height
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold
            };

            dropdownPanel.Children.Add(discountComboBox);
            dropdownPanel.Children.Add(addButton);
            mainPanel.Children.Add(dropdownPanel);

            // Manual Entry Section
            mainPanel.Children.Add(new TextBlock
            {
                Text = "Or enter manual discount amount:",
                FontSize = 11, // Reduced font size
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 8, 0, 4) // Reduced margin
            });

            var manualPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10) // Reduced margin
            };

            var manualTextBox = new TextBox
            {
                Width = 200, // Reduced width
                Height = 32, // Reduced height
                Margin = new Thickness(0, 0, 8, 0), // Reduced margin
                VerticalContentAlignment = VerticalAlignment.Center,
                Text = "0.00"
            };

            var addManualButton = new Button
            {
                Content = "Add Manual",
                Width = 80, // Reduced width
                Height = 32, // Reduced height
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold
            };

            manualPanel.Children.Add(manualTextBox);
            manualPanel.Children.Add(addManualButton);
            mainPanel.Children.Add(manualPanel);

            // Selected Discounts Label
            mainPanel.Children.Add(new TextBlock
            {
                Text = "Selected Discounts:",
                FontSize = 12, // Reduced font size
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 6) // Reduced margin
            });

            // Chips Container (WrapPanel for selected discounts)
            var chipsPanel = new System.Windows.Controls.WrapPanel
            {
                Orientation = Orientation.Horizontal,
                MaxHeight = 80, // Reduced height
                Margin = new Thickness(0, 0, 0, 10) // Reduced margin
            };

            var chipsScrollViewer = new ScrollViewer
            {
                Content = chipsPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 80, // Reduced height
                Margin = new Thickness(0, 0, 0, 10) // Reduced margin
            };

            mainPanel.Children.Add(chipsScrollViewer);

            // Pre-populate chips with already selected discounts
            foreach (var discount in SelectedDiscounts)
            {
                var chip = CreateDiscountChip(discount, chipsPanel);
                chipsPanel.Children.Add(chip);
            }

            // Add Button Click Handler
            addButton.Click += (s, e) =>
            {
                if (discountComboBox.SelectedItem is DiscountDto selectedDiscount)
                {
                    // Check if already added
                    if (!SelectedDiscounts.Any(d => d.Id == selectedDiscount.Id))
                    {
                        SelectedDiscounts.Add(selectedDiscount);
                        var chip = CreateDiscountChip(selectedDiscount, chipsPanel);
                        chipsPanel.Children.Add(chip);
                    }
                    discountComboBox.SelectedIndex = -1;
                }
            };

            // Add Manual Button Click Handler
            addManualButton.Click += (s, e) =>
            {
                if (decimal.TryParse(manualTextBox.Text, out var manualAmount) && manualAmount > 0)
                {
                    var manualDiscount = new DiscountDto
                    {
                        Id = -1,
                        DiscountName = "Manual Discount",
                        DiscountValue = manualAmount,
                        DiscountType = Domain.Enums.DiscountType.Fixed,
                        IsActive = true
                    };

                    // Remove previous manual discount if exists
                    var existingManual = SelectedDiscounts.FirstOrDefault(d => d.Id == -1);
                    if (existingManual != null)
                    {
                        SelectedDiscounts.Remove(existingManual);
                        var existingChip = chipsPanel.Children.OfType<Border>()
                            .FirstOrDefault(b => ((DiscountDto)b.Tag).Id == -1);
                        if (existingChip != null)
                            chipsPanel.Children.Remove(existingChip);
                    }

                    SelectedDiscounts.Add(manualDiscount);
                    var chip = CreateDiscountChip(manualDiscount, chipsPanel);
                    chipsPanel.Children.Add(chip);
                    manualTextBox.Text = "0.00";
                }
                else
                {
                    MessageBox.Show("Please enter a valid discount amount greater than 0.", "Invalid Amount", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            // Buttons
            var buttonPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0) // Reduced margin
            };

            var applyButton = new Button 
            { 
                Content = "Apply", 
                Width = 80, // Reduced width
                Height = 32, // Reduced height
                Margin = new Thickness(0, 0, 8, 0), // Reduced margin
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.Bold,
                FontSize = 12 // Reduced font size
            };
            var cancelButton = new Button 
            { 
                Content = "Cancel", 
                Width = 80, // Reduced width
                Height = 32, // Reduced height
                Background = Brushes.LightGray,
                Foreground = Brushes.Black,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold,
                FontSize = 12 // Reduced font size
            };

            applyButton.Click += (s, e) =>
            {
                // Calculate total discount amount
                DiscountAmount = 0m;
                foreach (var discount in SelectedDiscounts)
                {
                    if (discount.DiscountType == Domain.Enums.DiscountType.Percentage)
                    {
                        DiscountAmount += (Subtotal * discount.DiscountValue / 100);
                    }
                    else
                    {
                        DiscountAmount += discount.DiscountValue;
                    }
                }

                RecalculateTotals();
                dialog.DialogResult = true;
                dialog.Close();
            };

            cancelButton.Click += (s, e) =>
            {
                dialog.DialogResult = false;
                dialog.Close();
            };

            buttonPanel.Children.Add(applyButton);
            buttonPanel.Children.Add(cancelButton);
            mainPanel.Children.Add(buttonPanel);

            dialog.Content = mainPanel;
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private Border CreateDiscountChip(DiscountDto discount, System.Windows.Controls.WrapPanel parent)
    {
        var chipBorder = new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEF2FF")),
            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#818CF8")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(12, 6, 8, 6),
            Margin = new Thickness(0, 0, 6, 6),
            Tag = discount
        };

        var chipPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var displayText = discount.DiscountType == Domain.Enums.DiscountType.Percentage 
            ? $"{discount.DiscountName} - {discount.DiscountValue}%"
            : $"{discount.DiscountName} - ${discount.DiscountValue:F2}";

        chipPanel.Children.Add(new TextBlock
        {
            Text = displayText,
            FontSize = 12,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4338CA")),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0)
        });

        var removeButton = new Button
        {
            Content = "×",
            Width = 18,
            Height = 18,
            FontSize = 16,
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4338CA")),
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            VerticalAlignment = VerticalAlignment.Center
        };

        removeButton.Click += (s, e) =>
        {
            SelectedDiscounts.Remove(discount);
            parent.Children.Remove(chipBorder);
        };

        chipPanel.Children.Add(removeButton);
        chipBorder.Child = chipPanel;

        return chipBorder;
    }

    [RelayCommand]
    private void ShowTaxPopup()
    {
        try
        {
            // Create dialog window - increased by 20% more
            var dialog = new Window
            {
                Title = "Configure Tax Types",
                Width = SystemParameters.PrimaryScreenWidth * 0.3,
                Height = SystemParameters.PrimaryScreenHeight * 0.43, // Increased to show apply button
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var mainPanel = new StackPanel { Margin = new Thickness(15) }; // Reduced margin

            // Title
            mainPanel.Children.Add(new TextBlock 
            { 
                Text = "Select Tax Types", 
                FontSize = 14, // Reduced font size
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 10) // Reduced margin
            });

            // Dropdown + Add Button Row
            var dropdownPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8) // Reduced margin
            };

            var taxComboBox = new System.Windows.Controls.ComboBox
            {
                Width = 200, // Reduced width
                Height = 32, // Reduced height
                Margin = new Thickness(0, 0, 8, 0), // Reduced margin
                DisplayMemberPath = "Name",
                ItemsSource = AvailableTaxTypes.Where(t => t.AppliesToSelling).ToList()
            };

            var addButton = new Button
            {
                Content = "Add",
                Width = 80, // Reduced width
                Height = 32, // Reduced height
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold
            };

            dropdownPanel.Children.Add(taxComboBox);
            dropdownPanel.Children.Add(addButton);
            mainPanel.Children.Add(dropdownPanel);

            // Manual Entry Section
            mainPanel.Children.Add(new TextBlock
            {
                Text = "Or enter manual tax percentage:",
                FontSize = 11, // Reduced font size
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 8, 0, 4) // Reduced margin
            });

            var manualPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10) // Reduced margin
            };

            var manualTextBox = new TextBox
            {
                Width = 200, // Reduced width
                Height = 32, // Reduced height
                Margin = new Thickness(0, 0, 8, 0), // Reduced margin
                VerticalContentAlignment = VerticalAlignment.Center,
                Text = "0.00"
            };

            var addManualButton = new Button
            {
                Content = "Add Manual",
                Width = 80, // Reduced width
                Height = 32, // Reduced height
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold
            };

            manualPanel.Children.Add(manualTextBox);
            manualPanel.Children.Add(addManualButton);
            mainPanel.Children.Add(manualPanel);

            // Selected Tax Types Label
            mainPanel.Children.Add(new TextBlock
            {
                Text = "Selected Tax Types:",
                FontSize = 12, // Reduced font size
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 6) // Reduced margin
            });

            // Chips Container
            var chipsPanel = new System.Windows.Controls.WrapPanel
            {
                Orientation = Orientation.Horizontal,
                MaxHeight = 80, // Reduced height
                Margin = new Thickness(0, 0, 0, 10) // Reduced margin
            };

            var chipsScrollViewer = new ScrollViewer
            {
                Content = chipsPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 80, // Reduced height
                Margin = new Thickness(0, 0, 0, 10) // Reduced margin
            };

            mainPanel.Children.Add(chipsScrollViewer);

            // Pre-populate chips with already selected tax types
            foreach (var tax in SelectedTaxTypes)
            {
                var chip = CreateTaxChip(tax, chipsPanel);
                chipsPanel.Children.Add(chip);
            }

            // Add Button Click Handler
            addButton.Click += (s, e) =>
            {
                if (taxComboBox.SelectedItem is TaxTypeDto selectedTax)
                {
                    // Check if already added
                    if (!SelectedTaxTypes.Any(t => t.Id == selectedTax.Id))
                    {
                        SelectedTaxTypes.Add(selectedTax);
                        var chip = CreateTaxChip(selectedTax, chipsPanel);
                        chipsPanel.Children.Add(chip);
                    }
                    taxComboBox.SelectedIndex = -1;
                }
            };

            // Add Manual Button Click Handler
            addManualButton.Click += (s, e) =>
            {
                if (decimal.TryParse(manualTextBox.Text, out var manualPercent) && manualPercent > 0)
                {
                    var manualTax = new TaxTypeDto
                    {
                        Id = -1,
                        Name = "Manual Tax",
                        Value = manualPercent,
                        IsPercentage = true,
                        AppliesToSelling = true,
                        IsActive = true,
                        CalculationOrder = 999
                    };

                    // Remove previous manual tax if exists
                    var existingManual = SelectedTaxTypes.FirstOrDefault(t => t.Id == -1);
                    if (existingManual != null)
                    {
                        SelectedTaxTypes.Remove(existingManual);
                        var existingChip = chipsPanel.Children.OfType<Border>()
                            .FirstOrDefault(b => ((TaxTypeDto)b.Tag).Id == -1);
                        if (existingChip != null)
                            chipsPanel.Children.Remove(existingChip);
                    }

                    SelectedTaxTypes.Add(manualTax);
                    var chip = CreateTaxChip(manualTax, chipsPanel);
                    chipsPanel.Children.Add(chip);
                    manualTextBox.Text = "0.00";
                }
                else
                {
                    MessageBox.Show("Please enter a valid tax percentage greater than 0.", "Invalid Amount", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            // Buttons
            var buttonPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0) // Reduced margin
            };

            var applyButton = new Button 
            { 
                Content = "Apply", 
                Width = 80, // Reduced width
                Height = 32, // Reduced height
                Margin = new Thickness(0, 0, 8, 0), // Reduced margin
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.Bold,
                FontSize = 12 // Reduced font size
            };
            var cancelButton = new Button 
            { 
                Content = "Cancel", 
                Width = 80, // Reduced width
                Height = 32, // Reduced height
                Background = Brushes.LightGray,
                Foreground = Brushes.Black,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold,
                FontSize = 12 // Reduced font size
            };

            applyButton.Click += (s, e) =>
            {
                // Calculate total tax amount
                TaxAmount = 0m;
                TaxPercentage = 0m;
                foreach (var taxType in SelectedTaxTypes.OrderBy(t => t.CalculationOrder))
                {
                    if (taxType.IsPercentage)
                    {
                        TaxAmount += (Subtotal * taxType.Value / 100);
                        TaxPercentage += taxType.Value;
                    }
                    else
                    {
                        TaxAmount += taxType.Value;
                    }
                }

                RecalculateTotals();
                dialog.DialogResult = true;
                dialog.Close();
            };

            cancelButton.Click += (s, e) =>
            {
                dialog.DialogResult = false;
                dialog.Close();
            };

            buttonPanel.Children.Add(applyButton);
            buttonPanel.Children.Add(cancelButton);
            mainPanel.Children.Add(buttonPanel);

            dialog.Content = mainPanel;
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private Border CreateTaxChip(TaxTypeDto tax, System.Windows.Controls.WrapPanel parent)
    {
        var chipBorder = new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE")),
            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(12, 6, 8, 6),
            Margin = new Thickness(0, 0, 6, 6),
            Tag = tax
        };

        var chipPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        chipPanel.Children.Add(new TextBlock
        {
            Text = $"{tax.Name} - {tax.ValueDisplay}",
            FontSize = 12,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E40AF")),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0)
        });

        var removeButton = new Button
        {
            Content = "×",
            Width = 18,
            Height = 18,
            FontSize = 16,
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E40AF")),
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            VerticalAlignment = VerticalAlignment.Center
        };

        removeButton.Click += (s, e) =>
        {
            SelectedTaxTypes.Remove(tax);
            parent.Children.Remove(chipBorder);
        };

        chipPanel.Children.Add(removeButton);
        chipBorder.Child = chipPanel;

        return chipBorder;
    }

    [RelayCommand]
    private void ShowContainerChargePopup()
    {
        try
        {
            // Create dialog window
            var dialog = new Window
            {
                Title = "Add Container Charge",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            // Container Charge Amount
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Container Charge ($)", 
                FontSize = 12, 
                Margin = new Thickness(0, 0, 0, 5) 
            });
            var chargeTextBox = new TextBox 
            { 
                Text = ContainerCharge.ToString("F2"),
                Height = 35, 
                FontSize = 14, 
                Margin = new Thickness(0, 0, 0, 15) 
            };
            stackPanel.Children.Add(chargeTextBox);

            // Buttons
            var buttonPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal, 
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var applyButton = new Button 
            { 
                Content = "Apply", 
                Width = 80, 
                Height = 35,
                Margin = new Thickness(0, 0, 10, 0)
            };
            var cancelButton = new Button 
            { 
                Content = "Cancel", 
                Width = 80, 
                Height = 35
            };

            applyButton.Click += (s, e) =>
            {
                if (decimal.TryParse(chargeTextBox.Text, out var charge))
                {
                    ContainerCharge = charge;
                    RecalculateTotals();
                    dialog.DialogResult = true;
                    dialog.Close();
                }
                else
                {
                    MessageBox.Show("Please enter a valid container charge amount.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            cancelButton.Click += (s, e) =>
            {
                dialog.DialogResult = false;
                dialog.Close();
            };

            buttonPanel.Children.Add(applyButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            dialog.Content = stackPanel;
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string GenerateInvoiceNumber()
    {
        return $"INV-{DateTime.Now:yyyyMMdd}-{DateTime.Now.Ticks % 10000:D4}";
    }

    private void ClearCart()
    {
        CartItems.Clear();
        
        // Reset product quantities
        foreach (var product in FilteredProducts)
        {
            product.Quantity = 0;
        }

        // Reset transaction tracking
        CurrentTransactionId = 0;
        CurrentTransactionStatus = string.Empty;
        UpdateButtonVisibility();

        RecalculateTotals();
    }

    partial void OnSelectedCustomerChanged(CustomerDto? value)
    {
        if (value != null)
        {
            CustomerName = value.CustomerFullName ?? string.Empty;
            CustomerPhone = value.MobileNo ?? string.Empty;
        }
    }

    /// <summary>
    /// Load transaction data for editing
    /// </summary>
    public async Task LoadTransactionForEdit(int transactionId)
    {
        try
        {
            var transaction = await _transactionService.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                MessageBox.Show("Transaction not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Clear existing cart first
            ClearCart();
            
            // Set current transaction tracking AFTER clearing cart
            CurrentTransactionId = transactionId;
            CurrentTransactionStatus = transaction.Status;

            // Load customer
            if (transaction.CustomerId.HasValue)
            {
                SelectedCustomer = Customers.FirstOrDefault(c => c.Id == transaction.CustomerId.Value);
            }

            // Load table
            if (transaction.TableId.HasValue)
            {
                SelectedTable = Tables.FirstOrDefault(t => t.Id == transaction.TableId.Value);
            }

            // Load transaction products into cart
            foreach (var transactionProduct in transaction.TransactionProducts)
            {
                var product = await _productService.GetProductByIdAsync(transactionProduct.ProductId);
                if (product != null)
                {
                    var cartItem = new CartItemModel
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Icon = "🍽️",
                        UnitPrice = transactionProduct.SellingPrice,
                        Quantity = (int)transactionProduct.Quantity,
                        TotalPrice = transactionProduct.LineTotal
                    };
                    CartItems.Add(cartItem);

                    // Update product quantity in FilteredProducts
                    var productDisplay = FilteredProducts.FirstOrDefault(p => p.Id == product.Id);
                    if (productDisplay != null)
                    {
                        productDisplay.Quantity = (int)transactionProduct.Quantity;
                    }
                }
            }

            // Load discounts
            DiscountAmount = transaction.TotalDiscount;

            // Load tax - calculate percentage from amount to avoid recalculation issues
            TaxAmount = transaction.TotalVat;
            if (Subtotal > 0 && transaction.TotalVat > 0)
            {
                TaxPercentage = (transaction.TotalVat / (transaction.TotalAmount - transaction.TotalVat + transaction.TotalDiscount)) * 100;
            }
            else
            {
                TaxPercentage = 0m;
            }

            // Recalculate totals
            RecalculateTotals();

            // Update button visibility based on loaded transaction status
            UpdateButtonVisibility();

            MessageBox.Show($"Transaction #{transactionId} loaded for editing.\n\nYou can now modify the transaction and save the changes.", 
                "Transaction Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading transaction {transactionId} for edit", ex);
            MessageBox.Show($"Error loading transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Load transaction data for payment
    /// </summary>
    public async Task LoadTransactionForPayment(int transactionId)
    {
        try
        {
            var transaction = await _transactionService.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                MessageBox.Show("Transaction not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Clear existing cart first
            ClearCart();
            
            // Set current transaction tracking AFTER clearing cart
            CurrentTransactionId = transactionId;
            CurrentTransactionStatus = transaction.Status;

            // Load customer
            if (transaction.CustomerId.HasValue)
            {
                SelectedCustomer = Customers.FirstOrDefault(c => c.Id == transaction.CustomerId.Value);
            }

            // Load table
            if (transaction.TableId.HasValue)
            {
                SelectedTable = Tables.FirstOrDefault(t => t.Id == transaction.TableId.Value);
            }

            // Load transaction products into cart
            foreach (var transactionProduct in transaction.TransactionProducts)
            {
                var product = await _productService.GetProductByIdAsync(transactionProduct.ProductId);
                if (product != null)
                {
                    var cartItem = new CartItemModel
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Icon = "🍽️",
                        UnitPrice = transactionProduct.SellingPrice,
                        Quantity = (int)transactionProduct.Quantity,
                        TotalPrice = transactionProduct.LineTotal
                    };
                    CartItems.Add(cartItem);

                    // Update product quantity in FilteredProducts
                    var productDisplay = FilteredProducts.FirstOrDefault(p => p.Id == product.Id);
                    if (productDisplay != null)
                    {
                        productDisplay.Quantity = (int)transactionProduct.Quantity;
                    }
                }
            }

            // Load discounts
            DiscountAmount = transaction.TotalDiscount;

            // Load tax - calculate percentage from amount to avoid recalculation issues
            TaxAmount = transaction.TotalVat;
            if (Subtotal > 0 && transaction.TotalVat > 0)
            {
                TaxPercentage = (transaction.TotalVat / (transaction.TotalAmount - transaction.TotalVat + transaction.TotalDiscount)) * 100;
            }
            else
            {
                TaxPercentage = 0m;
            }

            // Recalculate totals
            RecalculateTotals();

            // Update button visibility based on loaded transaction status
            UpdateButtonVisibility();

            MessageBox.Show($"Transaction #{transactionId} loaded for payment.\n\nTotal Amount: ${transaction.TotalAmount:N2}\n\nPlease proceed to the payment section to complete the transaction.", 
                "Ready for Payment", MessageBoxButton.OK, MessageBoxImage.Information);

            // TODO: Auto-focus payment section when implemented
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading transaction {transactionId} for payment", ex);
            MessageBox.Show($"Error loading transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Updates button visibility based on transaction status
    /// </summary>
    private void UpdateButtonVisibility()
    {
        // New transaction (not saved yet)
        if (CurrentTransactionId == 0)
        {
            ShowSaveButton = true;
            ShowSaveAndPrintButton = true;
            ShowSettleButton = true;
            ShowRefundButton = false;
            ShowExchangeButton = false;
        }
        // Settled transaction
        else if (CurrentTransactionStatus?.ToLower() == "settled")
        {
            ShowSaveButton = false;
            ShowSaveAndPrintButton = false;
            ShowSettleButton = false;
            ShowRefundButton = true;
            ShowExchangeButton = true;
        }
        // Draft, Billed, Hold, Pending Payment, Partial Payment
        else if (CurrentTransactionStatus?.ToLower() is "draft" or "billed" or "hold" or "pending_payment" or "partial_payment")
        {
            ShowSaveButton = false; // Already saved, hide Save
            ShowSaveAndPrintButton = true;
            ShowSettleButton = true;
            ShowRefundButton = false;
            ShowExchangeButton = false;
        }
        // Cancelled - no buttons
        else if (CurrentTransactionStatus?.ToLower() == "cancelled")
        {
            ShowSaveButton = false;
            ShowSaveAndPrintButton = false;
            ShowSettleButton = false;
            ShowRefundButton = false;
            ShowExchangeButton = false;
        }
        // Refunded or Exchanged - no buttons (completed transactions)
        else if (CurrentTransactionStatus?.ToLower() is "refunded" or "exchanged")
        {
            ShowSaveButton = false;
            ShowSaveAndPrintButton = false;
            ShowSettleButton = false;
            ShowRefundButton = false;
            ShowExchangeButton = false;
        }
    }

    /// <summary>
    /// Save transaction and print bill
    /// </summary>
    [RelayCommand]
    private async Task SaveAndPrint()
    {
        try
        {
            if (!CartItems.Any())
            {
                MessageBox.Show("Please add items to the cart before saving.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var currentUserId = _currentUserService.CurrentUserId;
            if (!currentUserId.HasValue || currentUserId.Value <= 0)
            {
                MessageBox.Show("Unable to identify current user. Please log in again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var shiftId = 1;
            var totalAmount = CartItems.Sum(x => x.TotalPrice);
            var totalVat = totalAmount * (TaxPercentage / 100);

            if (CurrentTransactionId > 0)
            {
                // Change status of existing transaction to "billed"
                var updatedTransaction = await _transactionService.ChangeStatusAsync(CurrentTransactionId, "billed", currentUserId.Value);
                CurrentTransactionStatus = "billed";
                UpdateButtonVisibility();
                
                // Print the bill
                PrintBill(updatedTransaction);
                
                MessageBox.Show($"Transaction #{CurrentTransactionId} saved and marked as Billed!\n\nBill has been printed.", 
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Create new transaction with "billed" status
                var transactionDto = new CreateTransactionDto
                {
                    ShiftId = shiftId,
                    UserId = currentUserId.Value,
                    CustomerId = SelectedCustomer?.Id,
                    TableId = SelectedTable?.Id,
                    SellingTime = DateTime.Now,
                    TotalAmount = totalAmount,
                    TotalVat = totalVat,
                    TotalDiscount = DiscountAmount,
                    AmountPaidCash = 0m,
                    AmountCreditRemaining = 0m,
                    Vat = TaxPercentage,
                    Status = "billed",
                    Products = CartItems.Select(item => new CreateTransactionProductDto
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        SellingPrice = item.UnitPrice,
                        BuyerCost = 0m,
                        Vat = TaxPercentage
                    }).ToList()
                };

                var savedTransaction = await _transactionService.CreateAsync(transactionDto, currentUserId.Value);
                CurrentTransactionId = savedTransaction.Id;
                CurrentTransactionStatus = "billed";
                UpdateButtonVisibility();

                // Print the bill
                PrintBill(savedTransaction);

                MessageBox.Show($"Transaction #{savedTransaction.Id} saved and marked as Billed!\n\nBill has been printed.", 
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Settle transaction (complete payment)
    /// </summary>
    [RelayCommand]
    private async Task Settle()
    {
        try
        {
            if (!CartItems.Any())
            {
                MessageBox.Show("Please add items to the cart before settling.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var currentUserId = _currentUserService.CurrentUserId;
            if (!currentUserId.HasValue || currentUserId.Value <= 0)
            {
                MessageBox.Show("Unable to identify current user. Please log in again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var shiftId = 1;
            var totalAmount = CartItems.Sum(x => x.TotalPrice);
            var totalVat = totalAmount * (TaxPercentage / 100);

            // Show payment popup
            var paymentPopup = new Window
            {
                Title = "Payment",
                Width = 400,
                Height = 360, // Increased by 20% from 300 to show Settle button properly
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Total Amount Label
            var totalLabel = new TextBlock
            {
                Text = $"Total Amount: ${totalAmount:N2}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937"))
            };
            Grid.SetRow(totalLabel, 0);
            grid.Children.Add(totalLabel);

            // Payment Method Label and ComboBox
            var paymentMethodLabel = new TextBlock
            {
                Text = "Payment Method:",
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"))
            };

            // Payment Method ComboBox
            var paymentMethodComboBox = new ComboBox
            {
                Margin = new Thickness(0, 5, 0, 0),
                FontSize = 14,
                Padding = new Thickness(10)
            };
            paymentMethodComboBox.Items.Add("Cash");
            paymentMethodComboBox.Items.Add("Card");
            paymentMethodComboBox.Items.Add("UPI");
            paymentMethodComboBox.Items.Add("Credit");
            paymentMethodComboBox.SelectedIndex = 0;
            
            var paymentMethodPanel = new StackPanel();
            paymentMethodPanel.Children.Add(paymentMethodLabel);
            paymentMethodPanel.Children.Add(paymentMethodComboBox);
            Grid.SetRow(paymentMethodPanel, 2);
            grid.Children.Add(paymentMethodPanel);

            // Amount Label and TextBox
            var amountLabel = new TextBlock
            {
                Text = "Amount Paid:",
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"))
            };

            var amountTextBox = new TextBox
            {
                Text = totalAmount.ToString("N2"),
                FontSize = 14,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 5, 0, 0)
            };

            var amountPanel = new StackPanel();
            amountPanel.Children.Add(amountLabel);
            amountPanel.Children.Add(amountTextBox);
            Grid.SetRow(amountPanel, 4);
            grid.Children.Add(amountPanel);

            // Buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 100,
                Height = 40,
                Margin = new Thickness(0, 0, 10, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937")),
                BorderThickness = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
            cancelButton.Click += (s, e) => paymentPopup.Close();

            var settleButton = new Button
            {
                Content = "Save & Settle",
                Width = 120,
                Height = 40,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };

            settleButton.Click += async (s, e) =>
            {
                try
                {
                    if (!decimal.TryParse(amountTextBox.Text, out var paidAmount))
                    {
                        MessageBox.Show("Please enter a valid amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    paymentPopup.DialogResult = true;
                    paymentPopup.Close();

                    // Process settlement
                    if (CurrentTransactionId > 0)
                    {
                        // Update existing transaction with payment info, then settle
                        var updateDto = new UpdateTransactionDto
                        {
                            CustomerId = SelectedCustomer?.Id,
                            TableId = SelectedTable?.Id,
                            TotalAmount = totalAmount,
                            TotalVat = totalVat,
                            TotalDiscount = DiscountAmount,
                            AmountPaidCash = paidAmount,
                            AmountCreditRemaining = totalAmount - paidAmount,
                            Vat = TaxPercentage,
                            Status = CurrentTransactionStatus ?? "draft" // Keep current status
                        };

                        await _transactionService.UpdateAsync(CurrentTransactionId, updateDto, currentUserId.Value);
                        
                        // Now change status to settled
                        var settledTransaction = await _transactionService.ChangeStatusAsync(CurrentTransactionId, "settled", currentUserId.Value);
                        CurrentTransactionStatus = "settled";
                        UpdateButtonVisibility();
                        
                        MessageBox.Show($"Transaction #{CurrentTransactionId} settled successfully!\n\nPayment Method: {paymentMethodComboBox.SelectedItem}\nAmount Paid: ${paidAmount:N2}", 
                            "Payment Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        // Clear cart after settlement
                        ClearCart();
                    }
                    else
                    {
                        // Create new transaction with "settled" status
                        var transactionDto = new CreateTransactionDto
                        {
                            ShiftId = shiftId,
                            UserId = currentUserId.Value,
                            CustomerId = SelectedCustomer?.Id,
                            TableId = SelectedTable?.Id,
                            SellingTime = DateTime.Now,
                            TotalAmount = totalAmount,
                            TotalVat = totalVat,
                            TotalDiscount = DiscountAmount,
                            AmountPaidCash = paidAmount,
                            AmountCreditRemaining = totalAmount - paidAmount,
                            Vat = TaxPercentage,
                            Status = "settled",
                            Products = CartItems.Select(item => new CreateTransactionProductDto
                            {
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                SellingPrice = item.UnitPrice,
                                BuyerCost = 0m,
                                Vat = TaxPercentage
                            }).ToList()
                        };

                        var savedTransaction = await _transactionService.CreateAsync(transactionDto, currentUserId.Value);
                        
                        MessageBox.Show($"Transaction #{savedTransaction.Id} settled successfully!\n\nPayment Method: {paymentMethodComboBox.SelectedItem}\nAmount Paid: ${paidAmount:N2}", 
                            "Payment Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        // Clear cart after settlement
                        ClearCart();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error settling transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(settleButton);
            Grid.SetRow(buttonPanel, 6);
            grid.Children.Add(buttonPanel);

            paymentPopup.Content = grid;
            paymentPopup.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error showing payment popup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Print professional bill
    /// </summary>
    private void PrintBill(TransactionDto transaction)
    {
        try
        {
            var printDialog = new System.Windows.Controls.PrintDialog();
            
            // Create print document
            var document = new System.Windows.Documents.FlowDocument
            {
                PagePadding = new Thickness(50),
                FontFamily = new FontFamily("Courier New"),
                FontSize = 11
            };

            // Header
            var headerPara = new System.Windows.Documents.Paragraph
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            headerPara.Inlines.Add(new System.Windows.Documents.Run("═══════════════════════════════════\n") { FontWeight = FontWeights.Bold });
            headerPara.Inlines.Add(new System.Windows.Documents.Run("SALES RECEIPT\n") { FontSize = 16, FontWeight = FontWeights.Bold });
            headerPara.Inlines.Add(new System.Windows.Documents.Run("═══════════════════════════════════\n") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(headerPara);

            // Bill Info
            var infoPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 10, 0, 10) };
            infoPara.Inlines.Add(new System.Windows.Documents.Run($"Invoice #: {transaction.InvoiceNumber ?? transaction.Id.ToString()}\n"));
            infoPara.Inlines.Add(new System.Windows.Documents.Run($"Date: {transaction.SellingTime:dd-MM-yyyy}\n"));
            infoPara.Inlines.Add(new System.Windows.Documents.Run($"Time: {transaction.SellingTime:HH:mm:ss}\n"));
            
            if (SelectedCustomer != null)
            {
                infoPara.Inlines.Add(new System.Windows.Documents.Run($"Customer: {SelectedCustomer.DisplayName}\n"));
                if (!string.IsNullOrEmpty(SelectedCustomer.PrimaryMobile))
                {
                    infoPara.Inlines.Add(new System.Windows.Documents.Run($"Phone: {SelectedCustomer.PrimaryMobile}\n"));
                }
            }
            else
            {
                infoPara.Inlines.Add(new System.Windows.Documents.Run("Customer: Walk-in\n"));
            }
            
            if (SelectedTable != null)
            {
                infoPara.Inlines.Add(new System.Windows.Documents.Run($"Table: {SelectedTable.DisplayName}\n"));
            }
            
            document.Blocks.Add(infoPara);

            // Separator
            var separatorPara1 = new System.Windows.Documents.Paragraph
            {
                Margin = new Thickness(0, 5, 0, 5),
                TextAlignment = TextAlignment.Center
            };
            separatorPara1.Inlines.Add(new System.Windows.Documents.Run("───────────────────────────────────") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(separatorPara1);

            // Items Header
            var itemsHeaderPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 5, 0, 5) };
            itemsHeaderPara.Inlines.Add(new System.Windows.Documents.Run("ITEMS\n") { FontWeight = FontWeights.Bold });
            itemsHeaderPara.Inlines.Add(new System.Windows.Documents.Run("───────────────────────────────────\n"));
            document.Blocks.Add(itemsHeaderPara);

            // Items
            foreach (var item in CartItems)
            {
                var itemPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 2, 0, 2) };
                
                // Product name
                itemPara.Inlines.Add(new System.Windows.Documents.Run($"{item.ProductName}\n"));
                
                // Quantity and price
                var qtyPriceText = $"  {item.Quantity:0.##} x ${item.UnitPrice:N2}";
                var totalText = $"${item.TotalPrice:N2}";
                var spacing = new string(' ', Math.Max(0, 35 - qtyPriceText.Length - totalText.Length));
                itemPara.Inlines.Add(new System.Windows.Documents.Run($"{qtyPriceText}{spacing}{totalText}\n"));
                
                document.Blocks.Add(itemPara);
            }

            // Separator
            var separatorPara2 = new System.Windows.Documents.Paragraph
            {
                Margin = new Thickness(0, 5, 0, 5),
                TextAlignment = TextAlignment.Center
            };
            separatorPara2.Inlines.Add(new System.Windows.Documents.Run("───────────────────────────────────") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(separatorPara2);

            // Totals
            var totalsPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 5, 0, 10) };
            
            // Subtotal
            var subtotalLine = $"Subtotal:";
            var subtotalAmount = $"${Subtotal:N2}";
            var subtotalSpacing = new string(' ', Math.Max(0, 35 - subtotalLine.Length - subtotalAmount.Length));
            totalsPara.Inlines.Add(new System.Windows.Documents.Run($"{subtotalLine}{subtotalSpacing}{subtotalAmount}\n"));
            
            if (DiscountAmount > 0)
            {
                var discountLine = $"Discount:";
                var discountAmount = $"-${DiscountAmount:N2}";
                var discountSpacing = new string(' ', Math.Max(0, 35 - discountLine.Length - discountAmount.Length));
                totalsPara.Inlines.Add(new System.Windows.Documents.Run($"{discountLine}{discountSpacing}{discountAmount}\n"));
            }
            
            if (TaxPercentage > 0)
            {
                var taxLine = $"Tax ({TaxPercentage}%):";
                var taxAmount = $"${TaxAmount:N2}";
                var taxSpacing = new string(' ', Math.Max(0, 35 - taxLine.Length - taxAmount.Length));
                totalsPara.Inlines.Add(new System.Windows.Documents.Run($"{taxLine}{taxSpacing}{taxAmount}\n"));
            }
            
            // Total line separator
            totalsPara.Inlines.Add(new System.Windows.Documents.Run("───────────────────────────────────\n") { FontWeight = FontWeights.Bold });
            
            // Grand Total
            var totalLine = $"TOTAL:";
            var totalAmount = $"${Total:N2}";
            var totalSpacing = new string(' ', Math.Max(0, 35 - totalLine.Length - totalAmount.Length));
            totalsPara.Inlines.Add(new System.Windows.Documents.Run($"{totalLine}{totalSpacing}{totalAmount}\n") { FontWeight = FontWeights.Bold, FontSize = 13 });
            
            document.Blocks.Add(totalsPara);

            // Footer
            var footerPara = new System.Windows.Documents.Paragraph
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            footerPara.Inlines.Add(new System.Windows.Documents.Run("═══════════════════════════════════\n") { FontWeight = FontWeights.Bold });
            footerPara.Inlines.Add(new System.Windows.Documents.Run("Thank you for your business!\n"));
            footerPara.Inlines.Add(new System.Windows.Documents.Run("Please come again\n"));
            footerPara.Inlines.Add(new System.Windows.Documents.Run("═══════════════════════════════════\n") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(footerPara);

            // Print
            var paginator = ((System.Windows.Documents.IDocumentPaginatorSource)document).DocumentPaginator;
            printDialog.PrintDocument(paginator, "Sales Receipt");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error printing bill: {ex.Message}");
            MessageBox.Show($"Error printing bill: {ex.Message}\n\nThe transaction was saved successfully.", "Print Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// Load transaction for refund processing
    /// </summary>
    public async Task LoadTransactionForRefund(int transactionId)
    {
        try
        {
            if (transactionId <= 0)
            {
                MessageBox.Show("Invalid transaction ID for refund.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Enter refund mode
            IsRefundMode = true;
            RefundSourceTransactionId = transactionId;

            // Load the transaction
            var transaction = await _transactionService.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                MessageBox.Show("Transaction not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IsRefundMode = false;
                return;
            }

            // Clear current cart and load transaction items
            ClearCart();
            
            // Load transaction products
            foreach (var product in transaction.TransactionProducts)
            {
                var cartItem = new CartItemModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    UnitPrice = product.SellingPrice,
                    Quantity = (int)product.Quantity,
                    TotalPrice = product.SellingPrice * product.Quantity,
                    IsSelected = false,
                    RefundQuantity = 0,
                    MaxRefundQuantity = (int)product.Quantity
                };

                CartItems.Add(cartItem);
            }

            // Update customer and table info
            if (transaction.CustomerId.HasValue)
            {
                var customer = await _customerService.GetByIdAsync(transaction.CustomerId.Value);
                if (customer != null)
                {
                    // Check if customer exists in Customers collection, if not add it
                    var existingCustomer = Customers.FirstOrDefault(c => c.Id == customer.Id);
                    if (existingCustomer == null)
                    {
                        Customers.Add(customer);
                    }
                    
                    // Set selected customer
                    SelectedCustomer = Customers.FirstOrDefault(c => c.Id == customer.Id);
                    CustomerName = customer.DisplayName;
                    CustomerPhone = customer.PrimaryMobile;
                }
            }
            else
            {
                // Clear customer selection for walk-in
                SelectedCustomer = null;
                CustomerName = string.Empty;
                CustomerPhone = string.Empty;
            }

            if (transaction.TableId.HasValue)
            {
                SelectedTable = Tables.FirstOrDefault(t => t.Id == transaction.TableId.Value);
            }

            // Load discount and tax from transaction
            DiscountAmount = transaction.TotalDiscount;
            TaxPercentage = transaction.Vat;
            
            // Set transaction details for display
            CurrentTransactionId = transactionId;
            CurrentTransactionStatus = "refund";
            
            // Update button visibility for refund mode - show only Refund button
            ShowSaveButton = false;
            ShowSaveAndPrintButton = false;
            ShowSettleButton = false;
            ShowRefundButton = true; // Show refund button to process the refund
            ShowExchangeButton = false;

            // Recalculate totals
            RecalculateTotals();

            MessageBox.Show("Refund mode activated. Select items and specify quantities to refund.", "Refund Mode", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading transaction for refund: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            IsRefundMode = false;
        }
    }

    /// <summary>
    /// Process refund - saves refund transaction
    /// </summary>
    [RelayCommand]
    private async Task Refund()
    {
        try
        {
            if (!IsRefundMode)
            {
                // If not in refund mode, load the transaction for refund
                await LoadTransactionForRefund(CurrentTransactionId);
                return;
            }

            // Validate refund items
            var refundItems = CartItems.Where(item => item.IsSelected && item.RefundQuantity > 0).ToList();
            
            if (!refundItems.Any())
            {
                MessageBox.Show("Please select items and specify quantities to refund.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var currentUserId = _currentUserService.CurrentUserId;
            if (!currentUserId.HasValue || currentUserId.Value <= 0)
            {
                MessageBox.Show("Unable to identify current user. Please log in again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Calculate refund amount
            decimal refundAmount = refundItems.Sum(item => item.UnitPrice * item.RefundQuantity);
            decimal refundVat = refundAmount * (TaxPercentage / 100);

            // Confirm refund
            var result = MessageBox.Show(
                $"Are you sure you want to process this refund?\n\n" +
                $"Items to refund: {refundItems.Count}\n" +
                $"Refund amount: ${refundAmount:N2}\n\n" +
                $"This action cannot be undone.",
                "Confirm Refund",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            var shiftId = 1; // TODO: Get current shift ID

            // Get transaction products to map refund items
            var transaction = await _transactionService.GetByIdAsync(RefundSourceTransactionId);
            if (transaction == null)
            {
                MessageBox.Show("Source transaction not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Create refund DTO
            var refundDto = new CreateRefundTransactionDto
            {
                SellingTransactionId = RefundSourceTransactionId,
                CustomerId = SelectedCustomer?.Id,
                ShiftId = shiftId,
                UserId = currentUserId.Value,
                TotalAmount = refundAmount,
                TotalVat = refundVat,
                IsCash = true,
                RefundTime = DateTime.Now,
                Products = refundItems.Select(item =>
                {
                    // Find corresponding transaction product
                    var transactionProduct = transaction.TransactionProducts
                        .FirstOrDefault(tp => tp.ProductId == item.ProductId);
                    
                    return new CreateRefundTransactionProductDto
                    {
                        TransactionProductId = transactionProduct?.Id ?? 0,
                        TotalQuantityReturned = item.RefundQuantity,
                        TotalVat = (item.UnitPrice * item.RefundQuantity) * (TaxPercentage / 100),
                        TotalAmount = item.UnitPrice * item.RefundQuantity
                    };
                }).ToList()
            };

            // Save refund using RefundService
            var savedRefund = await _refundService.CreateAsync(refundDto);

            MessageBox.Show(
                $"Refund processed successfully!\n\n" +
                $"Refund ID: #{savedRefund.Id}\n" +
                $"Amount: ${refundAmount:N2}",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Clear cart and exit refund mode
            ClearCart();
            IsRefundMode = false;
            RefundSourceTransactionId = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error processing refund: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Exchange transaction
    /// <summary>
    /// Exchange command - navigates to exchange screen for the current transaction
    /// <summary>
    /// Exchange command - navigate to exchange screen for current transaction
    /// </summary>
    [RelayCommand]
    private async Task Exchange()
    {
        if (CurrentTransactionId <= 0)
        {
            MessageBox.Show("Please save the transaction first before initiating an exchange.", "Transaction Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CurrentTransactionStatus?.ToLower() != "settled")
        {
            MessageBox.Show("Only settled transactions can be exchanged.", "Invalid Status", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Navigate back to transactions, which will trigger the exchange screen
        // Note: This requires a callback to MainWindowViewModel
        MessageBox.Show(
            "To exchange this transaction, please use the Exchange button from the Transaction screen.\n\n" +
            "Go to: Transactions → Find the transaction → Click Exchange button",
            "Exchange Navigation",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}

/// <summary>
/// Display model for category with icon
/// </summary>
public partial class CategoryDisplayModel : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string icon = "📦";

    [ObservableProperty]
    private int itemCount;

    [ObservableProperty]
    private bool isSelected;
}

/// <summary>
/// Display model for product in the selection area
/// </summary>
public partial class ProductDisplayModel : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private decimal price;

    [ObservableProperty]
    private int quantity;

    [ObservableProperty]
    private int categoryId;
}

/// <summary>
/// Model for cart item
/// </summary>
public partial class CartItemModel : ObservableObject
{
    [ObservableProperty]
    private int productId;

    [ObservableProperty]
    private string productName = string.Empty;

    [ObservableProperty]
    private string icon = "🍽️";

    [ObservableProperty]
    private decimal unitPrice;

    [ObservableProperty]
    private int quantity;

    [ObservableProperty]
    private decimal totalPrice;

    [ObservableProperty]
    private bool isSelected = false; // For refund mode checkbox

    [ObservableProperty]
    private int refundQuantity = 0; // Quantity being refunded

    [ObservableProperty]
    private int maxRefundQuantity = 0; // Maximum quantity that can be refunded (original quantity)

    partial void OnQuantityChanged(int value)
    {
        // Auto-recalculate total price when quantity changes
        TotalPrice = UnitPrice * value;
    }

    partial void OnRefundQuantityChanged(int value)
    {
        // Ensure refund quantity doesn't exceed max
        if (value > MaxRefundQuantity)
        {
            RefundQuantity = MaxRefundQuantity;
        }
        if (value < 0)
        {
            RefundQuantity = 0;
        }
    }
}
