using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Views.Dialogs;
using ChronoPos.Desktop.Services;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using ChronoPos.Application.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for Add Sales screen
/// </summary>
public partial class AddSalesViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ICustomerService _customerService;
    private readonly ITransactionService _transactionService;
    private readonly IRestaurantTableService _tableService;
    private readonly IReservationService _reservationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IShiftService _shiftService;
    private readonly IDiscountService _discountService;
    private readonly ITaxTypeService _taxTypeService;
    private readonly IRefundService _refundService;
    private readonly IPaymentTypeService _paymentTypeService;
    private readonly ITransactionServiceChargeRepository _transactionServiceChargeRepository;
    private readonly ITransactionModifierRepository _transactionModifierRepository;
    private readonly IProductBarcodeRepository _productBarcodeRepository;
    private readonly IActiveCurrencyService _activeCurrencyService;
    private readonly Action? _navigateToTransactionList;
    private readonly Action<int>? _navigateToRefundTransaction;
    private readonly Action<int>? _navigateToExchangeTransaction;

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
    private ObservableCollection<ProductGroupViewModel> productGroups = new();

    [ObservableProperty]
    private bool isShowingProducts = true;

    [ObservableProperty]
    private ObservableCollection<CustomerDto> customers = new();

    [ObservableProperty]
    private ObservableCollection<RestaurantTableDto> tables = new();

    [ObservableProperty]
    private ObservableCollection<RestaurantTableDto> filteredTables = new();

    [ObservableProperty]
    private ObservableCollection<string> locations = new();

    [ObservableProperty]
    private string? selectedLocation;

    [ObservableProperty]
    private ObservableCollection<ReservationDto> reservations = new();

    [ObservableProperty]
    private ReservationDto? selectedReservation;

    [ObservableProperty]
    private bool isTableSelectionMode = true; // true = Table mode, false = Reservation mode

    [ObservableProperty]
    private ObservableCollection<CartItemModel> cartItems = new();

    [ObservableProperty]
    private CategoryDisplayModel? selectedCategory;

    [ObservableProperty]
    private decimal discountAmount = 0m;

    [ObservableProperty]
    private decimal serviceCharge = 0m;

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
    private bool showPayLaterButton = true; // Show for new transactions

    [ObservableProperty]
    private bool showSettleButton = false; // Show for draft/billed/hold/pending/partial

    [ObservableProperty]
    private bool showRefundButton = false; // Show only for settled

    [ObservableProperty]
    private bool showExchangeButton = false; // Show only for settled

    [ObservableProperty]
    private bool showCancelButton = true; // Show for new/draft transactions

    // Barcode Scanner Properties
    [ObservableProperty]
    private string barcodeInput = string.Empty;

    [ObservableProperty]
    private bool isScannerReady = true;

    [ObservableProperty]
    private string scannerStatusMessage = "Ready to scan";

    [ObservableProperty]
    private Brush scannerStatusColor = Brushes.Green;

    /// <summary>
    /// Gets the active currency symbol for display in UI
    /// </summary>
    public string CurrencySymbol => _activeCurrencyService?.CurrencySymbol ?? "$";

    #endregion

    public AddSalesViewModel(
        IServiceProvider serviceProvider,
        IProductService productService,
        ICategoryService categoryService,
        ICustomerService customerService,
        ITransactionService transactionService,
        IRestaurantTableService tableService,
        IReservationService reservationService,
        ICurrentUserService currentUserService,
        IShiftService shiftService,
        IDiscountService discountService,
        ITaxTypeService taxTypeService,
        IRefundService refundService,
        IPaymentTypeService paymentTypeService,
        ITransactionServiceChargeRepository transactionServiceChargeRepository,
        ITransactionModifierRepository transactionModifierRepository,
        IProductBarcodeRepository productBarcodeRepository,
        IActiveCurrencyService activeCurrencyService,
        Action? navigateToTransactionList = null,
        Action<int>? navigateToRefundTransaction = null,
        Action<int>? navigateToExchangeTransaction = null)
    {
        _serviceProvider = serviceProvider;
        _productService = productService;
        _categoryService = categoryService;
        _customerService = customerService;
        _transactionService = transactionService;
        _tableService = tableService;
        _reservationService = reservationService;
        _currentUserService = currentUserService;
        _shiftService = shiftService;
        _discountService = discountService;
        _taxTypeService = taxTypeService;
        _refundService = refundService;
        _paymentTypeService = paymentTypeService;
        _transactionServiceChargeRepository = transactionServiceChargeRepository;
        _transactionModifierRepository = transactionModifierRepository;
        _productBarcodeRepository = productBarcodeRepository;
        _activeCurrencyService = activeCurrencyService;
        _navigateToTransactionList = navigateToTransactionList;
        _navigateToRefundTransaction = navigateToRefundTransaction;
        _navigateToExchangeTransaction = navigateToExchangeTransaction;

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

            // Load locations
            await LoadLocationsAsync();

            // Load reservations
            await LoadReservationsAsync();

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
            new MessageDialog("Error", $"Error initializing Add Sales screen: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
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
            
            // Initially show all tables in filtered list
            FilteredTables.Clear();
            foreach (var table in Tables)
            {
                FilteredTables.Add(table);
            }
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error loading tables: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    private async Task LoadLocationsAsync()
    {
        try
        {
            var locationsData = await _tableService.GetDistinctLocationsAsync();
            
            Locations.Clear();
            Locations.Add("All Locations"); // Add default option
            foreach (var location in locationsData.Where(l => !string.IsNullOrEmpty(l)))
            {
                Locations.Add(location);
            }
            
            // Set default selection
            if (Locations.Any())
            {
                SelectedLocation = Locations.First();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading locations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadReservationsAsync()
    {
        try
        {
            var reservationsData = await _reservationService.GetAllAsync();
            
            // Only show confirmed reservations (case-insensitive)
            var activeReservations = reservationsData
                .Where(r => r.Status?.Equals("confirmed", StringComparison.OrdinalIgnoreCase) == true)
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime);
            
            Reservations.Clear();
            foreach (var reservation in activeReservations)
            {
                Reservations.Add(reservation);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading reservations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            new MessageDialog("Error", $"Error loading categories: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
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
            new MessageDialog("Error", $"Error loading products: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
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
            new MessageDialog("Error", $"Error loading customers: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
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
            new MessageDialog("Error", $"Error loading discounts: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
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
            new MessageDialog("Error", $"Error loading tax types: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
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
            new MessageDialog("Error", $"Error loading products for category: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
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
            new MessageDialog("Error", $"Error loading all products: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    /// <summary>
    /// Show products view
    /// </summary>
    [RelayCommand]
    private void ShowProducts()
    {
        IsShowingProducts = true;
        AppLogger.Log("Switched to Products view", filename: "product_selection");
    }

    /// <summary>
    /// Show product groups view
    /// </summary>
    [RelayCommand]
    private async Task ShowProductGroups()
    {
        try
        {
            IsShowingProducts = false;
            AppLogger.Log("Switching to Product Groups view", filename: "product_group_selection");
            
            // Load product groups if not already loaded
            if (ProductGroups == null || !ProductGroups.Any())
            {
                var productGroupService = _serviceProvider!.GetRequiredService<IProductGroupService>();
                var productGroupItemService = _serviceProvider!.GetRequiredService<IProductGroupItemService>();
                var productUnitService = _serviceProvider!.GetRequiredService<IProductUnitService>();
                
                var allGroups = await productGroupService.GetAllAsync();
                
                if (allGroups != null && allGroups.Any())
                {
                    ProductGroups.Clear();
                    foreach (var group in allGroups.Where(g => g.Status == "Active"))
                    {
                        // Calculate total price for this group
                        decimal totalPrice = 0m;
                        var groupItems = await productGroupItemService.GetByProductGroupIdAsync(group.Id);
                        
                        if (groupItems != null && groupItems.Any())
                        {
                            foreach (var item in groupItems)
                            {
                                if (!item.ProductId.HasValue) continue;
                                
                                var product = await _productService.GetProductByIdAsync(item.ProductId.Value);
                                if (product == null) continue;

                                // Get price from unit if specified, otherwise from product
                                if (item.ProductUnitId.HasValue && item.ProductUnitId.Value > 0)
                                {
                                    var unit = await productUnitService.GetByIdAsync(item.ProductUnitId.Value);
                                    totalPrice += unit?.PriceOfUnit ?? product.Price;
                                }
                                else
                                {
                                    totalPrice += product.Price;
                                }
                            }
                        }
                        
                        ProductGroups.Add(new ProductGroupViewModel(group, totalPrice));
                    }
                    AppLogger.Log($"Loaded {ProductGroups.Count} product groups", filename: "product_group_selection");
                }
                else
                {
                    AppLogger.Log("No product groups found", filename: "product_group_selection");
                }
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Error loading product groups", ex, filename: "product_group_selection");
            new MessageDialog("Error", $"Error loading product groups: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
            IsShowingProducts = true; // Revert to products view on error
        }
    }

    /// <summary>
    /// Increase quantity of a product group
    /// </summary>
    [RelayCommand]
    private async Task IncreaseProductGroupQuantity(ProductGroupViewModel groupViewModel)
    {
        try
        {
            if (groupViewModel == null) return;
            
            AppLogger.Log($"Increasing quantity for product group: {groupViewModel.Name}", filename: "product_group_selection");
            
            // Get services
            var productGroupItemService = _serviceProvider!.GetRequiredService<IProductGroupItemService>();
            var productUnitService = _serviceProvider!.GetRequiredService<IProductUnitService>();
            
            // Get all items in this group
            var groupItems = await productGroupItemService.GetByProductGroupIdAsync(groupViewModel.Id);
            
            if (groupItems == null || !groupItems.Any())
            {
                AppLogger.Log($"No items found for group: {groupViewModel.Name}", filename: "product_group_selection");
                return;
            }
            
            // Load details for each item and add to cart
            foreach (var item in groupItems)
            {
                if (!item.ProductId.HasValue) continue;
                
                var product = await _productService.GetProductByIdAsync(item.ProductId.Value);
                if (product == null) continue;

                ProductUnitDto? unit = null;
                if (item.ProductUnitId.HasValue && item.ProductUnitId.Value > 0)
                {
                    unit = await productUnitService.GetByIdAsync(item.ProductUnitId.Value);
                }

                // Get the price
                var price = unit?.PriceOfUnit ?? product.Price;

                // Create selection result for comparison
                var selectionResult = new ProductSelectionResult
                {
                    Product = product,
                    ProductUnit = unit,
                    SelectedModifiers = new List<ProductModifierGroupItemDto>(),
                    ProductGroup = groupViewModel.Group,
                    FinalPrice = price
                };

                // Add to cart or increase existing
                var existingItem = CartItems.FirstOrDefault(ci => 
                    ci.ProductId == product.Id && 
                    AreSameSelections(ci, selectionResult));

                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    var productName = product.Name;
                    if (unit != null)
                    {
                        productName += $" ({unit.UnitName})";
                    }
                    productName += $" [Group: {groupViewModel.Name}]";

                    var newCartItem = new CartItemModel
                    {
                        ProductId = product.Id,
                        ProductName = productName,
                        Icon = "📦",
                        UnitPrice = price,
                        Quantity = 1,
                        TotalPrice = price,
                        Tag = selectionResult,
                        // Store selected modifiers (empty for product groups, but keeps consistency)
                        SelectedModifiers = new List<ProductModifierGroupItemDto>()
                    };

                    newCartItem.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(CartItemModel.TotalPrice))
                        {
                            RecalculateTotals();
                        }
                    };

                    CartItems.Add(newCartItem);
                }
            }
            
            // Increase group quantity
            groupViewModel.Quantity++;
            
            RecalculateTotals();
            AppLogger.Log($"Added items from group: {groupViewModel.Name}, New quantity: {groupViewModel.Quantity}", filename: "product_group_selection");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Error increasing product group quantity", ex, filename: "product_group_selection");
        }
    }

    /// <summary>
    /// Decrease quantity of a product group
    /// </summary>
    [RelayCommand]
    private async Task DecreaseProductGroupQuantity(ProductGroupViewModel groupViewModel)
    {
        try
        {
            if (groupViewModel == null || groupViewModel.Quantity <= 0) return;
            
            AppLogger.Log($"Decreasing quantity for product group: {groupViewModel.Name}", filename: "product_group_selection");
            
            // Get services
            var productGroupItemService = _serviceProvider!.GetRequiredService<IProductGroupItemService>();
            var productUnitService = _serviceProvider!.GetRequiredService<IProductUnitService>();
            
            // Get all items in this group
            var groupItems = await productGroupItemService.GetByProductGroupIdAsync(groupViewModel.Id);
            
            if (groupItems == null || !groupItems.Any())
            {
                AppLogger.Log($"No items found for group: {groupViewModel.Name}", filename: "product_group_selection");
                return;
            }
            
            // Remove one quantity of each item from cart
            foreach (var item in groupItems)
            {
                if (!item.ProductId.HasValue) continue;
                
                var product = await _productService.GetProductByIdAsync(item.ProductId.Value);
                if (product == null) continue;

                ProductUnitDto? unit = null;
                if (item.ProductUnitId.HasValue && item.ProductUnitId.Value > 0)
                {
                    unit = await productUnitService.GetByIdAsync(item.ProductUnitId.Value);
                }

                // Get the price
                var price = unit?.PriceOfUnit ?? product.Price;

                // Create selection result for comparison
                var selectionResult = new ProductSelectionResult
                {
                    Product = product,
                    ProductUnit = unit,
                    SelectedModifiers = new List<ProductModifierGroupItemDto>(),
                    ProductGroup = groupViewModel.Group,
                    FinalPrice = price
                };

                // Find and decrease cart item
                var existingItem = CartItems.FirstOrDefault(ci => 
                    ci.ProductId == product.Id && 
                    AreSameSelections(ci, selectionResult));

                if (existingItem != null)
                {
                    existingItem.Quantity--;
                    if (existingItem.Quantity <= 0)
                    {
                        CartItems.Remove(existingItem);
                    }
                }
            }
            
            // Decrease group quantity
            groupViewModel.Quantity--;
            
            RecalculateTotals();
            AppLogger.Log($"Decreased items from group: {groupViewModel.Name}, New quantity: {groupViewModel.Quantity}", filename: "product_group_selection");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Error decreasing product group quantity", ex, filename: "product_group_selection");
        }
    }

    [RelayCommand]
    private async Task IncreaseQuantity(ProductDisplayModel product)
    {
        AppLogger.LogSeparator("PRODUCT CLICK - INCREASE QUANTITY", "product_selection");
        
        if (product == null)
        {
            AppLogger.LogWarning("Product is null in IncreaseQuantity", filename: "product_selection");
            return;
        }

        AppLogger.Log($"Product clicked: ID={product.Id}, Name={product.Name}", filename: "product_selection");

        // Fetch the full product details including stock information
        var productDetails = await _productService.GetProductByIdAsync(product.Id);
        if (productDetails == null)
        {
            AppLogger.LogError($"Failed to fetch product details for ProductId={product.Id}", filename: "product_selection");
            return;
        }
        
        AppLogger.Log($"Product details loaded: {productDetails.Name}", filename: "product_selection");

        var requestedQuantity = product.Quantity + 1;

        AppLogger.Log($"Requested quantity: {requestedQuantity}, IsStockTracked: {productDetails.IsStockTracked}", filename: "product_selection");

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
                    AppLogger.LogWarning($"Insufficient stock for {productDetails.Name}. Available: {productDetails.InitialStock}, Requested: {requestedQuantity}", filename: "product_selection");
                    
                    new MessageDialog(
                        "Stock Unavailable",
                        $"Insufficient stock for '{productDetails.Name}'.\n\n" +
                        $"Available: {productDetails.InitialStock}\n" +
                        $"Requested: {requestedQuantity}\n\n" +
                        $"Cannot add more items.",
                        MessageDialog.MessageType.Warning).ShowDialog();
                    return;
                }
                else
                {
                    AppLogger.Log($"Negative stock warning for {productDetails.Name}", filename: "product_selection");
                    
                    // Allow negative stock but show warning
                    var dialog = new ConfirmationDialog(
                        "Stock Warning",
                        $"Warning: Insufficient stock for '{productDetails.Name}'.\n\n" +
                        $"Available: {productDetails.InitialStock}\n" +
                        $"Requested: {requestedQuantity}\n" +
                        $"Shortage: {requestedQuantity - productDetails.InitialStock}\n\n" +
                        $"This product allows negative stock.\n" +
                        $"Do you want to continue?",
                        ConfirmationDialog.DialogType.Warning);
                    
                    var result = dialog.ShowDialog();
                    
                    if (result != true)
                    {
                        AppLogger.Log("User cancelled negative stock warning", filename: "product_selection");
                        return;
                    }
                    
                    AppLogger.Log("User accepted negative stock warning", filename: "product_selection");
                }
            }
        }

        AppLogger.Log("=== Checking product options ===", filename: "product_selection");
        AppLogger.Log($"ServiceProvider is null: {_serviceProvider == null}", filename: "product_selection");
        
        // Check if product has any options (units, modifiers, combinations, groups)
        bool hasOptions = false;
        try
        {
            var productUnitService = _serviceProvider!.GetRequiredService<IProductUnitService>();
            var modifierLinkService = _serviceProvider!.GetRequiredService<IProductModifierLinkService>();
            var combinationItemService = _serviceProvider!.GetRequiredService<IProductCombinationItemService>();
            
            var productUnits = await productUnitService.GetByProductIdAsync(productDetails.Id);
            var modifierLinks = await modifierLinkService.GetByProductIdAsync(productDetails.Id);
            
            // Check for product combinations by checking if any product unit has combination items
            bool hasCombinations = false;
            if (productUnits != null && productUnits.Any())
            {
                var productUnitIds = productUnits.Select(u => u.Id).ToList();
                var combinationItems = await combinationItemService.GetCombinationItemsByProductUnitIdsAsync(productUnitIds);
                hasCombinations = combinationItems != null && combinationItems.Any();
            }
            
            hasOptions = (productUnits != null && productUnits.Any()) || 
                        (modifierLinks != null && modifierLinks.Any()) ||
                        hasCombinations;
            
            AppLogger.Log($"Product has options: {hasOptions} (Units: {productUnits?.Count() ?? 0}, Modifiers: {modifierLinks?.Count() ?? 0}, Combinations: {hasCombinations})", filename: "product_selection");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Error checking product options", ex, filename: "product_selection");
        }
        
        ProductSelectionResult selection;
        
        if (!hasOptions)
        {
            // No options - add directly to cart with base price
            AppLogger.Log("No options available, adding directly to cart", filename: "product_selection");
            selection = new ProductSelectionResult
            {
                Product = productDetails,
                ProductUnit = null,
                SelectedModifiers = new List<ProductModifierGroupItemDto>(),
                ProductGroup = null,
                FinalPrice = productDetails.Price
            };
        }
        else
        {
            // Has options - show selection dialog
            try
            {
                // Show Product Selection Dialog to choose units, modifiers, combinations, groups
                AppLogger.Log("Creating ProductSelectionDialog instance...", filename: "product_selection");
                var selectionDialog = new ProductSelectionDialog(_serviceProvider!, productDetails);
                
                AppLogger.Log("Showing dialog...", filename: "product_selection");
                var dialogResult = selectionDialog.ShowDialog();
                
                AppLogger.Log($"Dialog closed. Result: {dialogResult}, SelectionResult is null: {selectionDialog.SelectionResult == null}", filename: "product_selection");

                if (dialogResult != true || selectionDialog.SelectionResult == null)
                {
                    // User cancelled the selection
                    AppLogger.Log("User cancelled product selection dialog", filename: "product_selection");
                    return;
                }

                selection = selectionDialog.SelectionResult;
                AppLogger.Log($"Selection made - FinalPrice: {selection.FinalPrice}, HasUnit: {selection.ProductUnit != null}, ModifiersCount: {selection.SelectedModifiers.Count}", filename: "product_selection");
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Error showing ProductSelectionDialog", ex, filename: "product_selection");
                new MessageDialog("Error", $"Error showing product options: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
                return;
            }
        }

        product.Quantity++;

        // Add or update in cart with selected options
        var existingCartItem = CartItems.FirstOrDefault(c => 
            c.ProductId == product.Id && 
            AreSameSelections(c, selection));
        
        if (existingCartItem != null)
        {
            existingCartItem.Quantity = product.Quantity;
            existingCartItem.TotalPrice = selection.FinalPrice * existingCartItem.Quantity;
        }
        else
        {
            var newCartItem = new CartItemModel
            {
                ProductId = product.Id,
                ProductUnitId = selection.ProductUnit?.Id, // Store product unit ID if selected
                ProductName = BuildProductNameWithOptions(product.Name, selection),
                Icon = "📦",
                UnitPrice = selection.FinalPrice,
                Quantity = 1, // Start with 1 for new selections
                TotalPrice = selection.FinalPrice,
                // Store selection details for comparison
                Tag = selection,
                // Store selected modifiers for transaction tracking
                SelectedModifiers = selection.SelectedModifiers ?? new List<ProductModifierGroupItemDto>()
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

    /// <summary>
    /// Check if two selections are the same (for cart item comparison)
    /// </summary>
    private bool AreSameSelections(CartItemModel cartItem, ProductSelectionResult selection)
    {
        if (cartItem.Tag is not ProductSelectionResult existingSelection)
            return false;

        // Compare product unit
        if (existingSelection.ProductUnit?.Id != selection.ProductUnit?.Id)
            return false;

        // Compare product group
        if (existingSelection.ProductGroup?.Id != selection.ProductGroup?.Id)
            return false;

        // Compare modifiers
        var existingModifierIds = existingSelection.SelectedModifiers.Select(m => m.Id).OrderBy(id => id).ToList();
        var newModifierIds = selection.SelectedModifiers.Select(m => m.Id).OrderBy(id => id).ToList();
        
        if (!existingModifierIds.SequenceEqual(newModifierIds))
            return false;

        return true;
    }

    /// <summary>
    /// Build a descriptive product name with selected options
    /// </summary>
    /// <summary>
    /// Builds a product name with modifiers from transaction product data
    /// </summary>
    private string BuildProductNameFromTransaction(string baseName, List<TransactionModifierDto>? modifiers)
    {
        if (modifiers == null || !modifiers.Any())
        {
            return baseName;
        }

        var modifierNames = modifiers
            .Select(m => m.ModifierName)
            .Where(name => !string.IsNullOrEmpty(name));

        if (modifierNames.Any())
        {
            return baseName + " +" + string.Join(", ", modifierNames);
        }

        return baseName;
    }

    private string BuildProductNameWithOptions(string baseName, ProductSelectionResult selection)
    {
        var parts = new List<string> { baseName };

        if (selection.ProductUnit != null)
        {
            parts.Add($"({selection.ProductUnit.UnitName})");
        }

        if (selection.SelectedModifiers.Any())
        {
            var modifierNames = string.Join(", ", selection.SelectedModifiers.Select(m => m.ModifierName));
            parts.Add($"+ {modifierNames}");
        }

        if (selection.ProductGroup != null)
        {
            parts.Add($"[{selection.ProductGroup.Name}]");
        }

        return string.Join(" ", parts);
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
        Total = Subtotal + TaxAmount + ServiceCharge - DiscountAmount;
    }

    [RelayCommand]
    private void EditTable()
    {
        // TODO: Implement table selection dialog
        new MessageDialog("Info", "Table selection feature coming soon!", MessageDialog.MessageType.Info).ShowDialog();
    }

    [RelayCommand]
    private async Task SaveTransaction()
    {
        try
        {
            AppLogger.Log("SaveTransaction: Starting transaction save process");
            
            if (!CartItems.Any())
            {
                new MessageDialog("Validation", "Please add items to the cart before saving.", MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            // Get current user
            var currentUserId = _currentUserService.CurrentUserId;
            AppLogger.Log($"SaveTransaction: Current user ID = {currentUserId}");
            
            if (!currentUserId.HasValue || currentUserId.Value <= 0)
            {
                new MessageDialog("Error", "Unable to identify current user. Please log in again.", MessageDialog.MessageType.Error).ShowDialog();
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
                new MessageDialog("Not Supported", "Updating existing transactions with product changes is not yet supported.\nPlease create a new transaction instead.", MessageDialog.MessageType.Info).ShowDialog();
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
                    ReservationId = SelectedReservation?.Id,
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
                        ProductUnitId = item.ProductUnitId, // Include product unit ID
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
                AppLogger.Log($"  ReservationId={transactionDto.ReservationId?.ToString() ?? "NULL"}");
                AppLogger.Log($"  ServiceCharge={ServiceCharge}");
                AppLogger.Log($"  Products count={transactionDto.Products.Count}");
                foreach (var product in transactionDto.Products)
                {
                    AppLogger.Log($"    Product: ID={product.ProductId}, Qty={product.Quantity}, Price={product.SellingPrice}");
                }

                var savedTransaction = await _transactionService.CreateAsync(transactionDto, currentUserId.Value);
                
                // Save transaction modifiers
                await SaveTransactionModifiers(savedTransaction.Id, savedTransaction.TransactionProducts);
                
                // Add service charge if present
                AppLogger.Log($"SaveTransaction: Checking service charge - ServiceCharge={ServiceCharge}");
                if (ServiceCharge > 0)
                {
                    AppLogger.Log($"SaveTransaction: Calling AddServiceChargeToTransaction for transaction #{savedTransaction.Id} with amount ${ServiceCharge}");
                    await AddServiceChargeToTransaction(savedTransaction.Id, ServiceCharge);
                    AppLogger.Log($"SaveTransaction: AddServiceChargeToTransaction call completed");
                }
                else
                {
                    AppLogger.Log($"SaveTransaction: Skipping service charge (amount is 0 or negative)");
                }
                
                // Update CurrentTransactionId and status after saving
                CurrentTransactionId = savedTransaction.Id;
                CurrentTransactionStatus = savedTransaction.Status;
                UpdateButtonVisibility();

                AppLogger.Log($"SaveTransaction: Transaction #{savedTransaction.Id} created successfully");
                MessageBox.Show($"Transaction #{savedTransaction.Id} saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Navigate to transaction list if callback provided
                _navigateToTransactionList?.Invoke();
                new MessageDialog("Success", $"Transaction #{savedTransaction.Id} saved successfully!", MessageDialog.MessageType.Success).ShowDialog();
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
            new MessageDialog("Error", errorMessage, MessageDialog.MessageType.Error).ShowDialog();
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
                    new MessageDialog("Validation", "Customer name is required.", MessageDialog.MessageType.Warning).ShowDialog();
                    return;
                }

                if (string.IsNullOrEmpty(mobile))
                {
                    new MessageDialog("Validation", "Mobile number is required.", MessageDialog.MessageType.Warning).ShowDialog();
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

                    new MessageDialog("Success", "Customer added successfully!", MessageDialog.MessageType.Success).ShowDialog();

                    // Reload customers
                    await LoadCustomersAsync();

                    // Select the newly added customer
                    SelectedCustomer = Customers.FirstOrDefault(c => c.MobileNo == mobile);

                    dialog.DialogResult = true;
                    dialog.Close();
                }
                catch (Exception ex)
                {
                    new MessageDialog("Error", $"Error adding customer: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
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
            new MessageDialog("Error", $"Error: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
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
                    new MessageDialog("Invalid Amount", "Please enter a valid discount amount greater than 0.", MessageDialog.MessageType.Warning).ShowDialog();
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
            new MessageDialog("Error", $"Error: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
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
            : $"{discount.DiscountName} - {_activeCurrencyService.FormatPrice(discount.DiscountValue)}";

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
                    new MessageDialog("Invalid Amount", "Please enter a valid tax percentage greater than 0.", MessageDialog.MessageType.Warning).ShowDialog();
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
            new MessageDialog("Error", $"Error: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
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
    private void ShowServiceChargePopup()
    {
        try
        {
            // Create dialog window
            var dialog = new Window
            {
                Title = "Add Service Charge",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            // Service Charge Amount
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Service Charge ($)", 
                FontSize = 12, 
                Margin = new Thickness(0, 0, 0, 5) 
            });
            var chargeTextBox = new TextBox 
            { 
                Text = ServiceCharge.ToString("F2"),
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
                    ServiceCharge = charge;
                    RecalculateTotals();
                    dialog.DialogResult = true;
                    dialog.Close();
                }
                else
                {
                    MessageBox.Show("Please enter a valid service charge amount.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            new MessageDialog("Error", $"Error: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
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
        
        // Reset service charge
        ServiceCharge = 0m;
        
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

    partial void OnSelectedLocationChanged(string? value)
    {
        FilterTablesByLocation();
    }

    partial void OnSelectedReservationChanged(ReservationDto? value)
    {
        if (value != null)
        {
            // Auto-select the table associated with the reservation
            var reservedTable = Tables.FirstOrDefault(t => t.Id == value.TableId);
            if (reservedTable != null)
            {
                SelectedTable = reservedTable;
            }

            // Also populate customer info from reservation
            if (!string.IsNullOrEmpty(value.CustomerName))
            {
                CustomerName = value.CustomerName;
            }
            if (!string.IsNullOrEmpty(value.CustomerMobile))
            {
                CustomerPhone = value.CustomerMobile;
            }
        }
    }

    private void FilterTablesByLocation()
    {
        FilteredTables.Clear();

        if (SelectedLocation == "All Locations" || string.IsNullOrEmpty(SelectedLocation))
        {
            // Show all tables
            foreach (var table in Tables)
            {
                FilteredTables.Add(table);
            }
        }
        else
        {
            // Filter tables by selected location
            var filteredList = Tables.Where(t => t.Location == SelectedLocation);
            foreach (var table in filteredList)
            {
                FilteredTables.Add(table);
            }
        }

        // Auto-select first table if available
        if (FilteredTables.Any() && SelectedTable == null)
        {
            SelectedTable = FilteredTables.First();
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
                new MessageDialog("Error", "Transaction not found!", MessageDialog.MessageType.Error).ShowDialog();
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

            // Load reservation
            if (transaction.ReservationId.HasValue)
            {
                await LoadReservationsAsync(); // Ensure reservations are loaded
                SelectedReservation = Reservations.FirstOrDefault(r => r.Id == transaction.ReservationId.Value);
                
                // If reservation is selected, switch to reservation mode
                if (SelectedReservation != null)
                {
                    IsTableSelectionMode = false;
                }
            }

            // Load table and location
            if (transaction.TableId.HasValue)
            {
                SelectedTable = Tables.FirstOrDefault(t => t.Id == transaction.TableId.Value);
                
                // If table is selected, also select its location
                if (SelectedTable != null && !string.IsNullOrEmpty(SelectedTable.Location))
                {
                    SelectedLocation = SelectedTable.Location;
                }
                
                // If no reservation but has table, ensure we're in table mode
                if (SelectedReservation == null)
                {
                    IsTableSelectionMode = true;
                }
            }

            // Load transaction products into cart
            foreach (var transactionProduct in transaction.TransactionProducts)
            {
                var product = await _productService.GetProductByIdAsync(transactionProduct.ProductId);
                if (product != null)
                {
                    // Build product name with modifiers
                    string productNameWithModifiers = BuildProductNameFromTransaction(product.Name, transactionProduct.Modifiers);
                    
                    var cartItem = new CartItemModel
                    {
                        ProductId = product.Id,
                        ProductUnitId = transactionProduct.ProductUnitId, // Load product unit ID
                        ProductName = productNameWithModifiers,
                        Icon = "🍽️",
                        UnitPrice = transactionProduct.SellingPrice,
                        Quantity = (int)transactionProduct.Quantity,
                        TotalPrice = transactionProduct.LineTotal,
                        // Store the modifiers for future reference
                        SelectedModifiers = transactionProduct.Modifiers?.Select(m => new ProductModifierGroupItemDto
                        {
                            Id = m.Id,
                            ModifierId = m.ProductModifierId,
                            ModifierName = m.ModifierName,
                            PriceAdjustment = m.ExtraPrice
                        }).ToList() ?? new List<ProductModifierGroupItemDto>()
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

            // Load service charges
            await LoadServiceChargesForTransaction(transactionId);

            // Load tax - find and select matching tax type from available tax types
            TaxAmount = transaction.TotalVat;
            if (transaction.Vat > 0)
            {
                // Load all tax types to find the matching one
                var allTaxTypes = await _taxTypeService.GetAllAsync();
                var matchingTax = allTaxTypes.FirstOrDefault(t => t.IsPercentage && Math.Abs(t.Value - transaction.Vat) < 0.01m);
                
                if (matchingTax != null && !SelectedTaxTypes.Any(st => st.Id == matchingTax.Id))
                {
                    SelectedTaxTypes.Clear();
                    SelectedTaxTypes.Add(matchingTax);
                }
                
                TaxPercentage = transaction.Vat;
            }
            else
            {
                TaxPercentage = 0m;
                SelectedTaxTypes.Clear();
            }

            // Recalculate totals
            RecalculateTotals();

            // Update button visibility based on loaded transaction status
            UpdateButtonVisibility();

            new MessageDialog("Transaction Loaded", $"Transaction #{transactionId} loaded for editing.\n\nYou can now modify the transaction and save the changes.", MessageDialog.MessageType.Info).ShowDialog();
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading transaction {transactionId} for edit", ex);
            new MessageDialog("Error", $"Error loading transaction: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
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
                new MessageDialog("Error", "Transaction not found!", MessageDialog.MessageType.Error).ShowDialog();
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

            // Load reservation
            if (transaction.ReservationId.HasValue)
            {
                await LoadReservationsAsync(); // Ensure reservations are loaded
                SelectedReservation = Reservations.FirstOrDefault(r => r.Id == transaction.ReservationId.Value);
                
                // If reservation is selected, switch to reservation mode
                if (SelectedReservation != null)
                {
                    IsTableSelectionMode = false;
                }
            }

            // Load table and location
            if (transaction.TableId.HasValue)
            {
                SelectedTable = Tables.FirstOrDefault(t => t.Id == transaction.TableId.Value);
                
                // If table is selected, also select its location
                if (SelectedTable != null && !string.IsNullOrEmpty(SelectedTable.Location))
                {
                    SelectedLocation = SelectedTable.Location;
                }
                
                // If no reservation but has table, ensure we're in table mode
                if (SelectedReservation == null)
                {
                    IsTableSelectionMode = true;
                }
            }

            // Load transaction products into cart
            foreach (var transactionProduct in transaction.TransactionProducts)
            {
                var product = await _productService.GetProductByIdAsync(transactionProduct.ProductId);
                if (product != null)
                {
                    // Build product name with modifiers
                    string productNameWithModifiers = BuildProductNameFromTransaction(product.Name, transactionProduct.Modifiers);
                    
                    var cartItem = new CartItemModel
                    {
                        ProductId = product.Id,
                        ProductUnitId = transactionProduct.ProductUnitId, // Load product unit ID
                        ProductName = productNameWithModifiers,
                        Icon = "🍽️",
                        UnitPrice = transactionProduct.SellingPrice,
                        Quantity = (int)transactionProduct.Quantity,
                        TotalPrice = transactionProduct.LineTotal,
                        SelectedModifiers = transactionProduct.Modifiers?.Select(m => new ProductModifierGroupItemDto
                        {
                            Id = m.Id,
                            ModifierId = m.ProductModifierId,
                            ModifierName = m.ModifierName,
                            PriceAdjustment = m.ExtraPrice
                        }).ToList() ?? new List<ProductModifierGroupItemDto>()
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

            // Load service charges
            await LoadServiceChargesForTransaction(transactionId);

            // Load tax - find and select matching tax type from available tax types
            TaxAmount = transaction.TotalVat;
            if (transaction.Vat > 0)
            {
                // Load all tax types to find the matching one
                var allTaxTypes = await _taxTypeService.GetAllAsync();
                var matchingTax = allTaxTypes.FirstOrDefault(t => t.IsPercentage && Math.Abs(t.Value - transaction.Vat) < 0.01m);
                
                if (matchingTax != null && !SelectedTaxTypes.Any(st => st.Id == matchingTax.Id))
                {
                    SelectedTaxTypes.Clear();
                    SelectedTaxTypes.Add(matchingTax);
                }
                
                TaxPercentage = transaction.Vat;
            }
            else
            {
                TaxPercentage = 0m;
                SelectedTaxTypes.Clear();
            }

            // Recalculate totals
            RecalculateTotals();

            // Update button visibility based on loaded transaction status
            UpdateButtonVisibility();

            new MessageDialog("Ready for Payment", $"Transaction #{transactionId} loaded for payment.\n\nTotal Amount: {_activeCurrencyService.FormatPrice(transaction.TotalAmount)}\n\nPlease proceed to the payment section to complete the transaction.", MessageDialog.MessageType.Info).ShowDialog();

            // TODO: Auto-focus payment section when implemented
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading transaction {transactionId} for payment", ex);
            new MessageDialog("Error", $"Error loading transaction: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
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
            ShowPayLaterButton = true;
            ShowSettleButton = true;
            ShowRefundButton = false;
            ShowExchangeButton = false;
            ShowCancelButton = true;
        }
        // Settled transaction
        else if (CurrentTransactionStatus?.ToLower() == "settled")
        {
            ShowSaveButton = false;
            ShowSaveAndPrintButton = false;
            ShowPayLaterButton = false;
            ShowSettleButton = false;
            ShowRefundButton = true;
            ShowExchangeButton = true;
            ShowCancelButton = false;
        }
        // Draft, Billed, Hold, Pending Payment, Partial Payment
        else if (CurrentTransactionStatus?.ToLower() is "draft" or "billed" or "hold" or "pending_payment" or "partial_payment")
        {
            ShowSaveButton = false; // Already saved, hide Save
            ShowSaveAndPrintButton = true;
            ShowPayLaterButton = false; // Already saved
            ShowSettleButton = true;
            ShowRefundButton = false;
            ShowExchangeButton = false;
            ShowCancelButton = true; // Can cancel draft/billed transactions
        }
        // Cancelled - no buttons
        else if (CurrentTransactionStatus?.ToLower() == "cancelled")
        {
            ShowSaveButton = false;
            ShowSaveAndPrintButton = false;
            ShowPayLaterButton = false;
            ShowSettleButton = false;
            ShowRefundButton = false;
            ShowExchangeButton = false;
            ShowCancelButton = false;
        }
        // Refunded or Exchanged - no buttons (completed transactions)
        else if (CurrentTransactionStatus?.ToLower() is "refunded" or "exchanged")
        {
            ShowSaveButton = false;
            ShowSaveAndPrintButton = false;
            ShowPayLaterButton = false;
            ShowSettleButton = false;
            ShowRefundButton = false;
            ShowExchangeButton = false;
            ShowCancelButton = false;
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
                new MessageDialog("Validation", "Please add items to the cart before saving.", MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            var currentUserId = _currentUserService.CurrentUserId;
            if (!currentUserId.HasValue || currentUserId.Value <= 0)
            {
                new MessageDialog("Error", "Unable to identify current user. Please log in again.", MessageDialog.MessageType.Error).ShowDialog();
                return;
            }

            var shiftId = 1;
            var totalAmount = CartItems.Sum(x => x.TotalPrice);
            var totalVat = totalAmount * (TaxPercentage / 100);
            
            // Get customer balance
            decimal customerBalanceAmount = SelectedCustomer?.CustomerBalanceAmount ?? 0m;
            
            // Calculate bill total considering customer balance
            decimal billTotal = totalAmount + customerBalanceAmount;

            if (CurrentTransactionId > 0)
            {
                // Change status of existing transaction to "billed"
                var updatedTransaction = await _transactionService.ChangeStatusAsync(CurrentTransactionId, "billed", currentUserId.Value);
                CurrentTransactionStatus = "billed";
                
                // Note: Customer balance is NOT updated during Save and Print
                // It will be updated only when:
                // 1. Pay Later is used (adds full amount to balance)
                // 2. Settle with partial payment is done (adds remaining amount to balance)
                // 3. Settle with full payment is done (clears balance to 0)
                
                UpdateButtonVisibility();
                
                // Print the bill
                PrintBill(updatedTransaction);
                
                new MessageDialog("Success", $"Transaction #{CurrentTransactionId} saved and marked as Billed!\n\nBill has been printed.", MessageDialog.MessageType.Success).ShowDialog();
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
                    AmountCreditRemaining = billTotal, // Bill total becomes credit remaining
                    Vat = TaxPercentage,
                    Status = "billed",
                    Products = CartItems.Select(item => new CreateTransactionProductDto
                    {
                        ProductId = item.ProductId,
                        ProductUnitId = item.ProductUnitId, // Include product unit ID
                        Quantity = item.Quantity,
                        SellingPrice = item.UnitPrice,
                        BuyerCost = 0m,
                        Vat = TaxPercentage
                    }).ToList()
                };

                var savedTransaction = await _transactionService.CreateAsync(transactionDto, currentUserId.Value);
                
                // Save transaction modifiers
                await SaveTransactionModifiers(savedTransaction.Id, savedTransaction.TransactionProducts);
                
                // Add service charge if present
                AppLogger.Log($"SaveAndPrint: Checking service charge - ServiceCharge={ServiceCharge}");
                if (ServiceCharge > 0)
                {
                    AppLogger.Log($"SaveAndPrint: Calling AddServiceChargeToTransaction for transaction #{savedTransaction.Id} with amount ${ServiceCharge}");
                    await AddServiceChargeToTransaction(savedTransaction.Id, ServiceCharge);
                    AppLogger.Log($"SaveAndPrint: AddServiceChargeToTransaction call completed");
                }
                else
                {
                    AppLogger.Log($"SaveAndPrint: Skipping service charge (amount is 0 or negative)");
                }
                
                CurrentTransactionId = savedTransaction.Id;
                CurrentTransactionStatus = "billed";
                
                // Note: Customer balance is NOT updated during Save and Print
                // It will be updated only when:
                // 1. Pay Later is used (adds full amount to balance)
                // 2. Settle with partial payment is done (adds remaining amount to balance)
                // 3. Settle with full payment is done (clears balance to 0)
                
                UpdateButtonVisibility();

                // Print the bill
                PrintBill(savedTransaction);

                new MessageDialog("Success", $"Transaction #{savedTransaction.Id} saved and marked as Billed!\n\nBill has been printed.", MessageDialog.MessageType.Success).ShowDialog();
            }
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error saving transaction: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    /// <summary>
    /// Pay Later - Save transaction with pending payment status
    /// </summary>
    [RelayCommand]
    private async Task PayLater()
    {
        try
        {
            // Validation: Customer must be selected for Pay Later
            if (SelectedCustomer == null)
            {
                MessageBox.Show("Please select a customer before using Pay Later.\n\nPay Later requires a customer to be selected for credit tracking.", 
                    "Customer Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            
            // Get customer balance
            decimal customerBalanceAmount = SelectedCustomer?.CustomerBalanceAmount ?? 0m;
            
            // Calculate bill total considering customer balance
            // If customer has pending dues (positive balance), add to bill
            // If customer has credit (negative balance), deduct from bill
            decimal billTotal = totalAmount + customerBalanceAmount;

            if (CurrentTransactionId > 0)
            {
                // Update existing transaction with payment info
                var updateDto = new UpdateTransactionDto
                {
                    CustomerId = SelectedCustomer.Id,
                    TableId = SelectedTable?.Id,
                    ReservationId = SelectedReservation?.Id,
                    TotalAmount = totalAmount,
                    TotalVat = totalVat,
                    TotalDiscount = DiscountAmount,
                    AmountPaidCash = 0m,
                    AmountCreditRemaining = billTotal, // Bill total becomes credit remaining
                    CreditDays = 0,
                    Vat = TaxPercentage,
                    Status = "pending_payment"
                };
                
                await _transactionService.UpdateAsync(CurrentTransactionId, updateDto, currentUserId.Value);
                
                // Change status to pending_payment
                var updatedTransaction = await _transactionService.ChangeStatusAsync(CurrentTransactionId, "pending_payment", currentUserId.Value);
                CurrentTransactionStatus = "pending_payment";
                
                // Update customer balance to bill total (unpaid amount)
                if (SelectedCustomer != null)
                {
                    try
                    {
                        var customerDto = await _customerService.GetByIdAsync(SelectedCustomer.Id);
                        if (customerDto != null)
                        {
                            customerDto.CustomerBalanceAmount = billTotal;
                            await _customerService.UpdateCustomerAsync(customerDto);
                            SelectedCustomer.CustomerBalanceAmount = billTotal;
                            AppLogger.Log($"PayLater: Updated customer balance to ${billTotal:N2} for customer {SelectedCustomer.Id}");
                        }
                    }
                    catch (Exception custEx)
                    {
                        AppLogger.LogError($"PayLater: Failed to update customer balance", custEx);
                    }
                }
                
                UpdateButtonVisibility();
                
                MessageBox.Show($"Transaction #{CurrentTransactionId} saved as Pending Payment!\n\nCustomer: {SelectedCustomer.CustomerFullName}\nBill Total: {_activeCurrencyService.FormatPrice(billTotal)}", 
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Create new transaction with "pending_payment" status
                var transactionDto = new CreateTransactionDto
                {
                    ShiftId = shiftId,
                    UserId = currentUserId.Value,
                    CustomerId = SelectedCustomer.Id,
                    TableId = SelectedTable?.Id,
                    ReservationId = SelectedReservation?.Id,
                    SellingTime = DateTime.Now,
                    TotalAmount = totalAmount,
                    TotalVat = totalVat,
                    TotalDiscount = DiscountAmount,
                    AmountPaidCash = 0m,
                    AmountCreditRemaining = billTotal, // Bill total becomes credit remaining
                    CreditDays = 0, // Can be set via popup later
                    Vat = TaxPercentage,
                    Status = "pending_payment",
                    Products = CartItems.Select(item => new CreateTransactionProductDto
                    {
                        ProductId = item.ProductId,
                        ProductUnitId = item.ProductUnitId, // Include product unit ID
                        Quantity = item.Quantity,
                        SellingPrice = item.UnitPrice,
                        BuyerCost = 0m,
                        Vat = TaxPercentage
                    }).ToList()
                };

                var savedTransaction = await _transactionService.CreateAsync(transactionDto, currentUserId.Value);
                
                // Save transaction modifiers
                await SaveTransactionModifiers(savedTransaction.Id, savedTransaction.TransactionProducts);
                
                // Add service charge if present
                if (ServiceCharge > 0)
                {
                    await AddServiceChargeToTransaction(savedTransaction.Id, ServiceCharge);
                }
                
                CurrentTransactionId = savedTransaction.Id;
                CurrentTransactionStatus = "pending_payment";
                
                // Update customer balance to bill total (unpaid amount)
                if (SelectedCustomer != null)
                {
                    try
                    {
                        var customerDto = await _customerService.GetByIdAsync(SelectedCustomer.Id);
                        if (customerDto != null)
                        {
                            customerDto.CustomerBalanceAmount = billTotal;
                            await _customerService.UpdateCustomerAsync(customerDto);
                            SelectedCustomer.CustomerBalanceAmount = billTotal;
                            AppLogger.Log($"PayLater: Updated customer balance to ${billTotal:N2} for customer {SelectedCustomer.Id}");
                        }
                    }
                    catch (Exception custEx)
                    {
                        AppLogger.LogError($"PayLater: Failed to update customer balance", custEx);
                    }
                }
                
                UpdateButtonVisibility();

                MessageBox.Show($"Transaction #{savedTransaction.Id} saved as Pending Payment!\n\nCustomer: {SelectedCustomer.CustomerFullName}\nBill Total: {_activeCurrencyService.FormatPrice(billTotal)}", 
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving Pay Later transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Cancel Transaction - Save transaction with cancelled status
    /// </summary>
    [RelayCommand]
    private async Task CancelTransaction()
    {
        try
        {
            if (!CartItems.Any())
            {
                MessageBox.Show("Please add items to the cart before cancelling.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Confirm cancellation
            var result = MessageBox.Show(
                "Are you sure you want to cancel this transaction?\n\nThe transaction will be saved with Cancelled status.",
                "Confirm Cancellation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

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
                // Change status of existing transaction to "cancelled"
                var updatedTransaction = await _transactionService.ChangeStatusAsync(CurrentTransactionId, "cancelled", currentUserId.Value);
                CurrentTransactionStatus = "cancelled";
                UpdateButtonVisibility();
                
                MessageBox.Show($"Transaction #{CurrentTransactionId} has been cancelled.", 
                    "Transaction Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Create new transaction with "cancelled" status
                var transactionDto = new CreateTransactionDto
                {
                    ShiftId = shiftId,
                    UserId = currentUserId.Value,
                    CustomerId = SelectedCustomer?.Id,
                    TableId = SelectedTable?.Id,
                    ReservationId = SelectedReservation?.Id,
                    SellingTime = DateTime.Now,
                    TotalAmount = totalAmount,
                    TotalVat = totalVat,
                    TotalDiscount = DiscountAmount,
                    AmountPaidCash = 0m,
                    AmountCreditRemaining = 0m,
                    Vat = TaxPercentage,
                    Status = "cancelled",
                    Products = CartItems.Select(item => new CreateTransactionProductDto
                    {
                        ProductId = item.ProductId,
                        ProductUnitId = item.ProductUnitId, // Include product unit ID
                        Quantity = item.Quantity,
                        SellingPrice = item.UnitPrice,
                        BuyerCost = 0m,
                        Vat = TaxPercentage
                    }).ToList()
                };

                var savedTransaction = await _transactionService.CreateAsync(transactionDto, currentUserId.Value);
                
                // Save transaction modifiers
                await SaveTransactionModifiers(savedTransaction.Id, savedTransaction.TransactionProducts);
                
                CurrentTransactionId = savedTransaction.Id;
                CurrentTransactionStatus = "cancelled";
                UpdateButtonVisibility();

                MessageBox.Show($"Transaction #{savedTransaction.Id} has been cancelled.", 
                    "Transaction Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error cancelling transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                new MessageDialog("Validation", "Please add items to the cart before settling.", MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            var currentUserId = _currentUserService.CurrentUserId;
            if (!currentUserId.HasValue || currentUserId.Value <= 0)
            {
                new MessageDialog("Error", "Unable to identify current user. Please log in again.", MessageDialog.MessageType.Error).ShowDialog();
                return;
            }

            var shiftId = 1;
            var totalAmount = CartItems.Sum(x => x.TotalPrice);
            var totalVat = totalAmount * (TaxPercentage / 100);
            
            // Get customer balance
            decimal customerBalanceAmount = SelectedCustomer?.CustomerBalanceAmount ?? 0m;
            
            // Calculate bill total considering customer balance
            // If customer has pending dues (positive balance), add to bill
            // If customer has credit (negative balance), deduct from bill
            decimal billTotal = totalAmount + customerBalanceAmount;
            
            // For existing transactions with partial/pending payment, get the remaining amount and status
            decimal alreadyPaid = 0m;
            string existingStatus = string.Empty;
            decimal existingAmountCreditRemaining = 0m;

            if (CurrentTransactionId > 0)
            {
                var existingTransaction = await _transactionService.GetByIdAsync(CurrentTransactionId);
                if (existingTransaction != null)
                {
                    alreadyPaid = existingTransaction.AmountPaidCash;
                    existingStatus = existingTransaction.Status ?? string.Empty;
                    existingAmountCreditRemaining = existingTransaction.AmountCreditRemaining;
                }
            }

            // Calculate remaining amount and prefill based on transaction status
            decimal remainingAmount;
            decimal amountToPrefill;
            if (existingStatus == "partial_payment")
            {
                // For partial payment transactions, bill total is the customer's pending amount
                remainingAmount = existingAmountCreditRemaining; // show transaction remaining
                amountToPrefill = customerBalanceAmount; // prefill amount paid with customer pending amount
            }
            else
            {
                // Default behavior: remaining is billTotal - alreadyPaid
                remainingAmount = billTotal - alreadyPaid;
                amountToPrefill = remainingAmount;
            }

            // Load payment types from database
            var paymentTypes = (await _paymentTypeService.GetActiveAsync()).ToList();
            if (!paymentTypes.Any())
            {
                MessageBox.Show("No payment types available. Please add payment types first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Show payment popup
            var paymentPopup = new Window
            {
                Title = "Payment",
                Width = 480,
                Height = 550, // Increased for customer balance info and better spacing
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Total Amount Label - Show customer balance and bill total
            string balanceInfo = "";
            if (customerBalanceAmount > 0)
            {
                balanceInfo = $"\nCustomer Pending: ${customerBalanceAmount:N2} (Added to bill)";
            }
            else if (customerBalanceAmount < 0)
            {
                balanceInfo = $"\nStore Credit Available: ${Math.Abs(customerBalanceAmount):N2} (Deducted from bill)";
            }
            
                        var totalLabel = new TextBlock
                        {
                                FontSize = 16,
                                FontWeight = FontWeights.Bold,
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937")),
                                TextWrapping = TextWrapping.Wrap
                        };

                        if (existingStatus == "partial_payment")
                        {
                                // Show customer pending as bill total and transaction remaining
                                var alreadyPaidFromSale = totalAmount - existingAmountCreditRemaining;
                                totalLabel.Text = $"Sale Amount: {_activeCurrencyService.CurrencySymbol}{totalAmount:N2}{balanceInfo}\n" +
                                                                    $"Customer Pending: {_activeCurrencyService.CurrencySymbol}{customerBalanceAmount:N2}\n" +
                                                                    $"Remaining Amount of Transaction: {_activeCurrencyService.CurrencySymbol}{existingAmountCreditRemaining:N2}\n" +
                                                                    $"Already Paid: {_activeCurrencyService.CurrencySymbol}{alreadyPaidFromSale:N2}";
                        }
                        else
                        {
                                totalLabel.Text = alreadyPaid > 0 
                                        ? $"Sale Amount: {_activeCurrencyService.CurrencySymbol}{totalAmount:N2}{balanceInfo}\n" +
                                            $"Bill Total: {_activeCurrencyService.CurrencySymbol}{billTotal:N2}\n" +
                                            $"Remaining: {_activeCurrencyService.CurrencySymbol}{remainingAmount:N2}\n(Already Paid: {_activeCurrencyService.CurrencySymbol}{alreadyPaid:N2})"
                                        : $"Sale Amount: {_activeCurrencyService.CurrencySymbol}{totalAmount:N2}{balanceInfo}\n" +
                                            $"Bill Total: {_activeCurrencyService.CurrencySymbol}{billTotal:N2}";
                        }
            
            Grid.SetRow(totalLabel, 0);
            grid.Children.Add(totalLabel);

            // Payment Method Label and ComboBox
            var paymentMethodLabel = new TextBlock
            {
                Text = "Payment Method:",
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"))
            };

            // Payment Method ComboBox - Load from database
            var paymentMethodComboBox = new ComboBox
            {
                Margin = new Thickness(0, 5, 0, 0),
                FontSize = 14,
                Padding = new Thickness(10),
                DisplayMemberPath = "Name",
                SelectedValuePath = "Id"
            };
            
            foreach (var paymentType in paymentTypes)
            {
                paymentMethodComboBox.Items.Add(paymentType);
            }
            paymentMethodComboBox.SelectedIndex = 0;
            
            var paymentMethodPanel = new StackPanel();
            paymentMethodPanel.Children.Add(paymentMethodLabel);
            paymentMethodPanel.Children.Add(paymentMethodComboBox);
            Grid.SetRow(paymentMethodPanel, 2);
            grid.Children.Add(paymentMethodPanel);

            // Amount Paid Label and TextBox
            var amountLabel = new TextBlock
            {
                Text = "Amount Paid:",
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"))
            };

            var amountTextBox = new TextBox
            {
                Text = amountToPrefill.ToString("N2"), // Use calculated prefill amount (handles partial payments)
                FontSize = 14,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 5, 0, 0)
            };

            var amountPanel = new StackPanel();
            amountPanel.Children.Add(amountLabel);
            amountPanel.Children.Add(amountTextBox);
            Grid.SetRow(amountPanel, 4);
            grid.Children.Add(amountPanel);

            // Credit Days Label and TextBox (for partial payment)
            var creditDaysLabel = new TextBlock
            {
                Text = "Credit Days (for partial payment):",
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"))
            };

            var creditDaysTextBox = new TextBox
            {
                Text = "0",
                FontSize = 14,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 5, 0, 0)
            };

            var creditDaysPanel = new StackPanel();
            creditDaysPanel.Children.Add(creditDaysLabel);
            creditDaysPanel.Children.Add(creditDaysTextBox);
            Grid.SetRow(creditDaysPanel, 6);
            grid.Children.Add(creditDaysPanel);

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
                        new MessageDialog("Validation Error", "Please enter a valid amount.", MessageDialog.MessageType.Warning).ShowDialog();
                        return;
                    }

                    if (!int.TryParse(creditDaysTextBox.Text, out var creditDays) || creditDays < 0)
                    {
                        MessageBox.Show("Please enter a valid number of credit days (0 or more).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Calculate the actual remaining amount to pay based on transaction status
                    var actualRemainingAmount = existingStatus == "partial_payment" ? customerBalanceAmount : (billTotal - alreadyPaid);

                    // Validate paid amount against actual remaining amount
                    if (paidAmount < 0 || paidAmount > actualRemainingAmount)
                    {
                        MessageBox.Show($"Amount paid must be between {_activeCurrencyService.CurrencySymbol}0 and {_activeCurrencyService.FormatPrice(actualRemainingAmount)}.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    paymentPopup.DialogResult = true;
                    paymentPopup.Close();

                    // Calculate new totals considering already paid amount and bill total
                    var totalPaidNow = alreadyPaid + paidAmount;
                    var creditRemaining = billTotal - totalPaidNow;
                    string transactionStatus;

                    // Determine transaction status based on total payment against bill total
                    if (totalPaidNow >= billTotal)
                    {
                        transactionStatus = "settled"; // Full payment
                        creditRemaining = 0; // Ensure no negative credit
                    }
                    else if (totalPaidNow > 0)
                    {
                        // Partial payment - check if customer allows credit
                        if (SelectedCustomer != null && !SelectedCustomer.CreditAllowed)
                        {
                            MessageBox.Show("This customer is not allowed to have credit.\n\nPlease collect full payment or select a customer with credit privileges.", 
                                "Credit Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        transactionStatus = "partial_payment"; // Partial payment
                    }
                    else
                    {
                        // No payment - check if customer allows credit
                        if (SelectedCustomer != null && !SelectedCustomer.CreditAllowed)
                        {
                            MessageBox.Show("This customer is not allowed to have credit.\n\nPlease collect payment or select a customer with credit privileges.", 
                                "Credit Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        transactionStatus = "pending_payment"; // No payment made
                    }

                    var selectedPaymentType = (PaymentTypeDto)paymentMethodComboBox.SelectedItem;
                    
                    // Calculate customer balance change
                    // If sale fully settled: CustomerBalanceAmount = 0 (all dues cleared)
                    // If partial/pending: CustomerBalanceAmount = creditRemaining (unpaid amount becomes due)
                    decimal newCustomerBalance = 0m;
                    
                    if (transactionStatus == "settled")
                    {
                        // Full payment - clear customer balance
                        newCustomerBalance = 0m;
                    }
                    else
                    {
                        // Partial or pending payment - update customer balance with unpaid amount
                        // The unpaid portion (creditRemaining) becomes the new balance
                        newCustomerBalance = creditRemaining;
                    }

                    // Process settlement
                    if (CurrentTransactionId > 0)
                    {
                        // Update existing transaction with payment info, then settle
                        var updateDto = new UpdateTransactionDto
                        {
                            CustomerId = SelectedCustomer?.Id,
                            TableId = SelectedTable?.Id,
                            ReservationId = SelectedReservation?.Id,
                            TotalAmount = totalAmount,
                            TotalVat = totalVat,
                            TotalDiscount = DiscountAmount,
                            AmountPaidCash = totalPaidNow, // Total amount paid so far
                            AmountCreditRemaining = creditRemaining,
                            CreditDays = creditDays,
                            Vat = TaxPercentage,
                            Status = CurrentTransactionStatus ?? "draft" // Keep current status
                        };

                        await _transactionService.UpdateAsync(CurrentTransactionId, updateDto, currentUserId.Value);
                        
                        // Now change status to settled/partial_payment/pending_payment
                        var settledTransaction = await _transactionService.ChangeStatusAsync(CurrentTransactionId, transactionStatus, currentUserId.Value);
                        
                        // Update reservation status to completed if transaction is settled and has a reservation
                        if (transactionStatus == "settled" && SelectedReservation?.Id != null)
                        {
                            try
                            {
                                await _reservationService.CompleteReservationAsync(SelectedReservation.Id);
                                AppLogger.Log($"PayLater: Reservation #{SelectedReservation.Id} marked as completed");
                            }
                            catch (Exception resEx)
                            {
                                AppLogger.LogError($"PayLater: Failed to update reservation {SelectedReservation.Id} status to completed", resEx);
                            }
                        }
                        
                        CurrentTransactionStatus = transactionStatus;
                        
                        // Update customer balance if customer is selected
                        if (SelectedCustomer != null)
                        {
                            try
                            {
                                var customerDto = await _customerService.GetByIdAsync(SelectedCustomer.Id);
                                if (customerDto != null)
                                {
                                    customerDto.CustomerBalanceAmount = newCustomerBalance;
                                    await _customerService.UpdateCustomerAsync(customerDto);
                                    SelectedCustomer.CustomerBalanceAmount = newCustomerBalance;
                                    AppLogger.Log($"Settle: Updated customer balance to ${newCustomerBalance:N2} for customer {SelectedCustomer.Id}");
                                }
                            }
                            catch (Exception custEx)
                            {
                                AppLogger.LogError($"Settle: Failed to update customer balance", custEx);
                            }
                        }
                        
                        UpdateButtonVisibility();

                        var statusMessage = transactionStatus switch
                        {
                            "settled" => "Transaction settled successfully (Full Payment)!",
                            "partial_payment" => $"Transaction saved with Partial Payment!\nCredit Remaining: {_activeCurrencyService.FormatPrice(creditRemaining)}",
                            "pending_payment" => $"Transaction saved as Pending Payment!\nTotal Credit: {_activeCurrencyService.FormatPrice(creditRemaining)}",
                            _ => "Transaction updated!"
                        };
                        
                        MessageBox.Show($"{statusMessage}\n\nPayment Method: {selectedPaymentType.Name}\nAmount Paid Now: {_activeCurrencyService.FormatPrice(paidAmount)}\nTotal Paid: {_activeCurrencyService.FormatPrice(totalPaidNow)}" +
                            (creditDays > 0 ? $"\nCredit Days: {creditDays}" : ""), 
                            "Payment Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        // Clear cart after settlement if fully paid
                        if (transactionStatus == "settled")
                        {
                            ClearCart();
                        }
                    }
                    else
                    {
                        // Create new transaction with appropriate status
                        var transactionDto = new CreateTransactionDto
                        {
                            ShiftId = shiftId,
                            UserId = currentUserId.Value,
                            CustomerId = SelectedCustomer?.Id,
                            TableId = SelectedTable?.Id,
                            ReservationId = SelectedReservation?.Id,
                            SellingTime = DateTime.Now,
                            TotalAmount = totalAmount,
                            TotalVat = totalVat,
                            TotalDiscount = DiscountAmount,
                            AmountPaidCash = paidAmount,
                            AmountCreditRemaining = creditRemaining,
                            CreditDays = creditDays,
                            Vat = TaxPercentage,
                            Status = transactionStatus, // Use calculated status instead of hardcoded "settled"
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
                        
                        // Save transaction modifiers
                        await SaveTransactionModifiers(savedTransaction.Id, savedTransaction.TransactionProducts);
                        
                        CurrentTransactionId = savedTransaction.Id;
                        CurrentTransactionStatus = transactionStatus;
                        
                        // Update customer balance if customer is selected
                        if (SelectedCustomer != null)
                        {
                            try
                            {
                                var customerDto = await _customerService.GetByIdAsync(SelectedCustomer.Id);
                                if (customerDto != null)
                                {
                                    customerDto.CustomerBalanceAmount = newCustomerBalance;
                                    await _customerService.UpdateCustomerAsync(customerDto);
                                    SelectedCustomer.CustomerBalanceAmount = newCustomerBalance;
                                    AppLogger.Log($"Settle: Updated customer balance to ${newCustomerBalance:N2} for customer {SelectedCustomer.Id}");
                                }
                            }
                            catch (Exception custEx)
                            {
                                AppLogger.LogError($"Settle: Failed to update customer balance", custEx);
                            }
                        }
                        
                        // Update reservation status to completed if transaction is settled and has a reservation
                        if (transactionStatus == "settled" && SelectedReservation?.Id != null)
                        {
                            try
                            {
                                await _reservationService.CompleteReservationAsync(SelectedReservation.Id);
                                AppLogger.Log($"PayLater: Reservation #{SelectedReservation.Id} marked as completed for new transaction");
                            }
                            catch (Exception resEx)
                            {
                                AppLogger.LogError($"PayLater: Failed to update reservation {SelectedReservation.Id} status to completed", resEx);
                            }
                        }
                        
                        UpdateButtonVisibility();
                        
                        var statusMessage = transactionStatus switch
                        {
                            "settled" => "Transaction settled successfully (Full Payment)!",
                            "partial_payment" => $"Transaction saved with Partial Payment!\nCredit Remaining: {_activeCurrencyService.FormatPrice(creditRemaining)}",
                            "pending_payment" => $"Transaction saved as Pending Payment!\nTotal Credit: {_activeCurrencyService.FormatPrice(creditRemaining)}",
                            _ => "Transaction saved!"
                        };
                        
                        MessageBox.Show($"{statusMessage}\nTransaction ID: #{savedTransaction.Id}\n\nPayment Method: {selectedPaymentType.Name}\nAmount Paid: {_activeCurrencyService.FormatPrice(paidAmount)}" +
                            (creditDays > 0 ? $"\nCredit Days: {creditDays}" : ""), 
                            "Payment Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                        new MessageDialog("Payment Complete", $"Transaction #{savedTransaction.Id} settled successfully!\n\nPayment Method: {paymentMethodComboBox.SelectedItem}\nAmount Paid: {_activeCurrencyService.FormatPrice(paidAmount)}", MessageDialog.MessageType.Success).ShowDialog();
                        
                        // Clear cart after settlement if fully paid
                        if (transactionStatus == "settled")
                        {
                            ClearCart();
                        }
                    }
                }
                catch (Exception ex)
                {
                    new MessageDialog("Error", $"Error settling transaction: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
                }
            };

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(settleButton);
            Grid.SetRow(buttonPanel, 8); // Row 8 because we added credit days section
            grid.Children.Add(buttonPanel);

            paymentPopup.Content = grid;
            paymentPopup.ShowDialog();
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error showing payment popup: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    /// <summary>
    /// Process refund for current settled transaction
    /// </summary>
    [RelayCommand]
    private async Task Refund()
    {
        try
        {
            // Validate that we have a transaction ID
            if (CurrentTransactionId == 0)
            {
                MessageBox.Show("No transaction to refund.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Load the current transaction
            var transaction = await _transactionService.GetByIdAsync(CurrentTransactionId);
            if (transaction == null)
            {
                MessageBox.Show("Transaction not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if transaction is settled
            if (transaction.Status?.ToLower() != "settled")
            {
                MessageBox.Show($"Only settled transactions can be refunded.\n\nCurrent status: {transaction.Status}", 
                    "Invalid Status", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Show refund dialog
            var refundDialog = new Views.Dialogs.RefundDialog(
                transaction,
                _refundService,
                _currentUserService,
                _activeCurrencyService);

            var result = refundDialog.ShowDialog();

            if (result == true && refundDialog.IsConfirmed)
            {
                // Clear the current transaction and navigate back to transaction list
                ClearCart();
                _navigateToTransactionList?.Invoke();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error processing refund: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Navigate to exchange screen for current settled transaction
    /// </summary>
    [RelayCommand]
    private async Task Exchange()
    {
        try
        {
            // Validate that we have a transaction ID
            if (CurrentTransactionId == 0)
            {
                MessageBox.Show("No transaction to exchange.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Load the current transaction
            var transaction = await _transactionService.GetByIdAsync(CurrentTransactionId);
            if (transaction == null)
            {
                MessageBox.Show("Transaction not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if transaction is settled
            if (transaction.Status?.ToLower() != "settled")
            {
                MessageBox.Show($"Only settled transactions can be exchanged.\n\nCurrent status: {transaction.Status}", 
                    "Invalid Status", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Navigate to Exchange screen
            _navigateToExchangeTransaction?.Invoke(CurrentTransactionId);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error processing exchange: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #region Barcode Scanner Methods

    /// <summary>
    /// Searches for a product by barcode and adds it to cart
    /// </summary>
    [RelayCommand]
    private async Task SearchByBarcode()
    {
        if (string.IsNullOrWhiteSpace(BarcodeInput))
            return;

        var barcode = BarcodeInput.Trim();
        
        try
        {
            IsScannerReady = false;
            ScannerStatusMessage = "Scanning...";
            ScannerStatusColor = Brushes.Orange;
            
            AppLogger.Log($"[BARCODE SCAN] Searching for barcode: {barcode}");
            
            // Search in ProductBarcodes table
            var productBarcode = await _productBarcodeRepository.GetByBarcodeValueAsync(barcode);
            
            if (productBarcode != null && productBarcode.Product != null)
            {
                AppLogger.Log($"[BARCODE SCAN] Found product: {productBarcode.Product.Name} (ID: {productBarcode.ProductId})");
                
                // Get full product details
                var product = await _productService.GetProductByIdAsync(productBarcode.ProductId);
                
                if (product != null)
                {
                    // Check if product already in cart
                    var existingItem = CartItems.FirstOrDefault(item => item.ProductId == product.Id);
                    
                    if (existingItem != null)
                    {
                        // Increase quantity
                        existingItem.Quantity++;
                        existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
                        AppLogger.Log($"[BARCODE SCAN] Increased quantity for '{product.Name}' to {existingItem.Quantity}");
                    }
                    else
                    {
                        // Add new item to cart
                        var cartItem = new CartItemModel
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            Icon = "🛒",
                            UnitPrice = product.Price,
                            Quantity = 1,
                            TotalPrice = product.Price,
                            ProductUnitId = null
                        };
                        
                        CartItems.Add(cartItem);
                        AppLogger.Log($"[BARCODE SCAN] Added '{product.Name}' to cart - Price: ${product.Price}");
                    }
                    
                    // Recalculate totals
                    RecalculateTotals();
                    
                    // Success feedback
                    ScannerStatusMessage = $"✓ {product.Name} added!";
                    ScannerStatusColor = Brushes.LimeGreen;
                    System.Media.SystemSounds.Beep.Play();
                    
                    AppLogger.Log($"[BARCODE SCAN] SUCCESS - Product added to cart");
                }
                else
                {
                    // Product not found in system
                    ScannerStatusMessage = $"✗ Product not found in system";
                    ScannerStatusColor = Brushes.Red;
                    System.Media.SystemSounds.Hand.Play();
                    AppLogger.LogError($"[BARCODE SCAN] Product ID {productBarcode.ProductId} not found in product service");
                }
            }
            else
            {
                // Barcode not found
                ScannerStatusMessage = $"✗ Barcode not registered: {barcode}";
                ScannerStatusColor = Brushes.Red;
                System.Media.SystemSounds.Hand.Play();
                AppLogger.LogError($"[BARCODE SCAN] Barcode '{barcode}' not found in database");
            }
        }
        catch (Exception ex)
        {
            ScannerStatusMessage = $"✗ Error: {ex.Message}";
            ScannerStatusColor = Brushes.Red;
            System.Media.SystemSounds.Hand.Play();
            AppLogger.LogError($"[BARCODE SCAN] Error processing barcode '{barcode}'", ex);
        }
        finally
        {
            // Clear input for next scan
            BarcodeInput = string.Empty;
            
            // Reset status after 2 seconds
            await Task.Delay(2000);
            ScannerStatusMessage = "Ready to scan";
            ScannerStatusColor = Brushes.Green;
            IsScannerReady = true;
        }
    }

    #endregion

    /// <summary>
    /// Helper method to add service charge to a transaction
    /// </summary>
    /// <summary>
    /// Save transaction modifiers for products that have modifiers selected
    /// </summary>
    private async Task SaveTransactionModifiers(int transactionId, List<TransactionProductDto> transactionProducts)
    {
        try
        {
            var currentUserId = _currentUserService.CurrentUserId;
            
            // Get all cart items with their modifiers
            foreach (var cartItem in CartItems)
            {
                // Find the corresponding transaction product
                var transactionProduct = transactionProducts.FirstOrDefault(tp => tp.ProductId == cartItem.ProductId);
                if (transactionProduct == null || cartItem.SelectedModifiers == null || !cartItem.SelectedModifiers.Any())
                {
                    continue;
                }
                
                // Save each selected modifier
                foreach (var modifier in cartItem.SelectedModifiers)
                {
                    var transactionModifier = new TransactionModifier
                    {
                        TransactionProductId = transactionProduct.Id,
                        ProductModifierId = modifier.ModifierId,
                        ExtraPrice = modifier.PriceAdjustment,
                        CreatedBy = currentUserId,
                        CreatedAt = DateTime.Now
                    };
                    
                    await _transactionModifierRepository.AddAsync(transactionModifier);
                }
            }
            
            // Save all modifiers to database
            await _transactionModifierRepository.SaveChangesAsync();
            AppLogger.Log($"SaveTransactionModifiers: Saved modifiers for transaction #{transactionId}");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error saving transaction modifiers for transaction #{transactionId}", ex);
            // Don't throw - continue with transaction even if modifiers fail
        }
    }

    /// <summary>
    /// Helper method to add service charge to an existing transaction
    /// </summary>
    private async Task AddServiceChargeToTransaction(int transactionId, decimal chargeAmount)
    {
        try
        {
            AppLogger.Log($"AddServiceChargeToTransaction: START - Transaction #{transactionId}, Amount=${chargeAmount}");
            
            // Create service charge with nullable ServiceChargeId (manual/custom charge)
            var serviceCharge = new TransactionServiceCharge
            {
                TransactionId = transactionId,
                ServiceChargeId = null, // NULL for manual/custom service charges (not linked to predefined service charge)
                TotalAmount = chargeAmount,
                TotalVat = 0, // Can be calculated based on tax if needed
                Status = "Active",
                CreatedBy = _currentUserService.CurrentUserId,
                CreatedAt = DateTime.Now
            };

            AppLogger.Log($"AddServiceChargeToTransaction: Service charge object created - TransactionId={serviceCharge.TransactionId}, Amount={serviceCharge.TotalAmount}, Status={serviceCharge.Status}, ServiceChargeId={serviceCharge.ServiceChargeId}");
            
            await _transactionServiceChargeRepository.AddAsync(serviceCharge);
            AppLogger.Log($"AddServiceChargeToTransaction: AddAsync completed");
            
            await _transactionServiceChargeRepository.SaveChangesAsync(); // CRITICAL: Save to database
            AppLogger.Log($"AddServiceChargeToTransaction: SaveChangesAsync completed - Service charge of ${chargeAmount} added and saved to transaction #{transactionId}");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"AddServiceChargeToTransaction: ERROR adding service charge to transaction #{transactionId}", ex);
            AppLogger.LogError($"AddServiceChargeToTransaction: Exception details - {ex.GetType().Name}: {ex.Message}", ex);
            AppLogger.LogError($"AddServiceChargeToTransaction: Stack trace: {ex.StackTrace}", ex);
            if (ex.InnerException != null)
            {
                AppLogger.LogError($"AddServiceChargeToTransaction: Inner exception - {ex.InnerException.GetType().Name}: {ex.InnerException.Message}", ex.InnerException);
            }
            // Rethrow to see the actual error
            throw;
        }
    }

    /// <summary>
    /// Helper method to load service charges for a transaction
    /// </summary>
    private async Task LoadServiceChargesForTransaction(int transactionId)
    {
        try
        {
            AppLogger.Log($"LoadServiceChargesForTransaction: Loading service charges for transaction #{transactionId}");
            var serviceCharges = await _transactionServiceChargeRepository.GetByTransactionIdAsync(transactionId);
            AppLogger.Log($"LoadServiceChargesForTransaction: Retrieved {serviceCharges.Count()} service charge records");
            
            var totalServiceCharge = serviceCharges.Sum(sc => sc.TotalAmount);
            AppLogger.Log($"LoadServiceChargesForTransaction: Calculated total service charge = ${totalServiceCharge}");
            AppLogger.Log($"LoadServiceChargesForTransaction: Current ServiceCharge value before assignment = ${ServiceCharge}");
            
            ServiceCharge = totalServiceCharge;
            
            AppLogger.Log($"LoadServiceChargesForTransaction: ServiceCharge property set to ${ServiceCharge}");
            AppLogger.Log($"LoadServiceChargesForTransaction: Loaded service charges totaling ${totalServiceCharge} for transaction #{transactionId}");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading service charges for transaction #{transactionId}", ex);
            ServiceCharge = 0m;
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
                var qtyPriceText = $"  {item.Quantity:0.##} x {_activeCurrencyService.FormatPrice(item.UnitPrice)}";
                var totalText = _activeCurrencyService.FormatPrice(item.TotalPrice);
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
            var subtotalAmount = _activeCurrencyService.FormatPrice(Subtotal);
            var subtotalSpacing = new string(' ', Math.Max(0, 35 - subtotalLine.Length - subtotalAmount.Length));
            totalsPara.Inlines.Add(new System.Windows.Documents.Run($"{subtotalLine}{subtotalSpacing}{subtotalAmount}\n"));
            
            if (DiscountAmount > 0)
            {
                var discountLine = $"Discount:";
                var discountAmount = $"-{_activeCurrencyService.FormatPrice(DiscountAmount)}";
                var discountSpacing = new string(' ', Math.Max(0, 35 - discountLine.Length - discountAmount.Length));
                totalsPara.Inlines.Add(new System.Windows.Documents.Run($"{discountLine}{discountSpacing}{discountAmount}\n"));
            }
            
            if (TaxPercentage > 0)
            {
                var taxLine = $"Tax ({TaxPercentage}%):";
                var taxAmount = _activeCurrencyService.FormatPrice(TaxAmount);
                var taxSpacing = new string(' ', Math.Max(0, 35 - taxLine.Length - taxAmount.Length));
                totalsPara.Inlines.Add(new System.Windows.Documents.Run($"{taxLine}{taxSpacing}{taxAmount}\n"));
            }
            
            // Show customer balance if applicable
            if (SelectedCustomer != null)
            {
                decimal customerBalance = SelectedCustomer.CustomerBalanceAmount;
                if (customerBalance != 0)
                {
                    string balanceLabel = customerBalance > 0 ? "Previous Pending:" : "Store Credit:";
                    var balanceLine = $"{balanceLabel}";
                    var balanceAmount = customerBalance > 0 ? $"${customerBalance:N2}" : $"-${Math.Abs(customerBalance):N2}";
                    var balanceSpacing = new string(' ', Math.Max(0, 35 - balanceLine.Length - balanceAmount.Length));
                    totalsPara.Inlines.Add(new System.Windows.Documents.Run($"{balanceLine}{balanceSpacing}{balanceAmount}\n"));
                }
            }
            
            // Total line separator
            totalsPara.Inlines.Add(new System.Windows.Documents.Run("───────────────────────────────────\n") { FontWeight = FontWeights.Bold });
            
            // Calculate bill total (sale + customer balance)
            decimal customerBalanceAmount = SelectedCustomer?.CustomerBalanceAmount ?? 0m;
            decimal billTotal = Total + customerBalanceAmount;
            
            // Grand Total
            var totalLine = $"TOTAL:";
            var totalAmount = $"${billTotal:N2}";
            var totalSpacing = new string(' ', Math.Max(0, 35 - totalLine.Length - totalAmount.Length));
            totalsPara.Inlines.Add(new System.Windows.Documents.Run($"{totalLine}{totalSpacing}{totalAmount}\n") { FontWeight = FontWeights.Bold, FontSize = 13 });
            
            // Show payment status for billed transactions
            if (transaction.Status == "billed" && billTotal > 0)
            {
                totalsPara.Inlines.Add(new System.Windows.Documents.Run("\n"));
                totalsPara.Inlines.Add(new System.Windows.Documents.Run("STATUS: PENDING PAYMENT\n") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                totalsPara.Inlines.Add(new System.Windows.Documents.Run($"Amount Due: ${billTotal:N2}\n") { FontWeight = FontWeights.Bold });
            }
            
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
            new MessageDialog("Print Error", $"Error printing bill: {ex.Message}\n\nThe transaction was saved successfully.", MessageDialog.MessageType.Warning).ShowDialog();
        }
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
    private int? productUnitId; // Store selected product unit ID

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

    // Store selected modifiers for this cart item
    public List<ProductModifierGroupItemDto> SelectedModifiers { get; set; } = new();
    
    // Store product selection details (units, modifiers, groups, etc.)
    public object? Tag { get; set; }

    partial void OnQuantityChanged(int value)
    {
        // Auto-recalculate total price when quantity changes
        TotalPrice = UnitPrice * value;
    }
}
