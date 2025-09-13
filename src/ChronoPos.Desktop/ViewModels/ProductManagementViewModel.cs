using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Collections.ObjectModel;
using System.Windows;
using ChronoPos.Desktop.Services;
using InfrastructureServices = ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for comprehensive product management with category support and full settings integration
/// </summary>
public partial class ProductManagementViewModel : ObservableObject, IDisposable
{
    #region Fields
    
    private readonly IProductService _productService;
    private readonly Action? _navigateToAddProduct;
    private readonly Action? _navigateBack;
    
    // Settings services
    private readonly IThemeService _themeService;
    private readonly IZoomService _zoomService;
    private readonly ILocalizationService _localizationService;
    private readonly IColorSchemeService _colorSchemeService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly IFontService _fontService;
    private readonly InfrastructureServices.IDatabaseLocalizationService _databaseLocalizationService;
    
    #endregion

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<CategoryDto> categories = new();

    [ObservableProperty]
    private ObservableCollection<ProductDto> products = new();

    [ObservableProperty]
    private ObservableCollection<ProductDto> filteredProducts = new();

    [ObservableProperty]
    private CategoryDto? selectedCategory;

    [ObservableProperty]
    private ProductDto? selectedProduct;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedSearchType = "Product Name";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private bool isProductFormVisible = false;

    [ObservableProperty]
    private bool isCategoryFormVisible = false;

    [ObservableProperty]
    private bool isEditMode = false;

    [ObservableProperty]
    private ProductDto currentProduct = new();

    [ObservableProperty]
    private CategoryDto currentCategory = new();

    [ObservableProperty]
    private ObservableCollection<CategoryDto> parentCategories = new();

    // Settings properties
    [ObservableProperty]
    private string _currentTheme = "Light";

    [ObservableProperty]
    private int _currentZoom = 100;

    [ObservableProperty]
    private string _currentLanguage = "English";

    [ObservableProperty]
    private double _currentFontSize = 14;

    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    // Translated UI Properties
    [ObservableProperty]
    private string _pageTitle = "Product Management";

    [ObservableProperty]
    private string _backButtonText = "← Back";

    [ObservableProperty]
    private string _refreshButtonText = "🔄 Refresh";

    [ObservableProperty]
    private string _categoriesHeaderText = "Categories";

    [ObservableProperty]
    private string _addNewCategoryButtonText = "➕ Add Category";

    [ObservableProperty]
    private string _addNewProductButtonText = "➕ Add Product";

    [ObservableProperty]
    private string _searchPlaceholder = "Search products...";

    [ObservableProperty]
    private string _searchTypeProductName = "Product Name";

    [ObservableProperty]
    private string _searchTypeCategory = "Category";

    [ObservableProperty]
    private string _showingProductsFormat = "Showing {0} products";

    [ObservableProperty]
    private string _allCategoriesText = "All";

    [ObservableProperty]
    private string _itemsCountText = "items";

    // Table Column Headers
    [ObservableProperty]
    private string _columnProductName = "Product Name";

    [ObservableProperty]
    private string _columnItemId = "Item ID";

    [ObservableProperty]
    private string _columnStock = "Stock";

    [ObservableProperty]
    private string _columnCategory = "Category";

    [ObservableProperty]
    private string _columnPrice = "Price";

    [ObservableProperty]
    private string _columnActions = "Actions";

    // Action Tooltips
    [ObservableProperty]
    private string _editProductTooltip = "Edit Product";

    [ObservableProperty]
    private string _deleteProductTooltip = "Delete Product";

    [ObservableProperty]
    private string _duplicateProductTooltip = "Duplicate Product";

    // Category Form Labels
    [ObservableProperty]
    private string _addCategoryTitle = "Add New Category";

    [ObservableProperty]
    private string _editCategoryTitle = "Edit Category";

    [ObservableProperty]
    private string _categoryNameLabel = "Category Name *";

    [ObservableProperty]
    private string _categoryNameArabicLabel = "Category Name (Arabic)";

    [ObservableProperty]
    private string _parentCategoryLabel = "Parent Category";

    [ObservableProperty]
    private string _displayOrderLabel = "Display Order";

    [ObservableProperty]
    private string _descriptionLabel = "Description";

    [ObservableProperty]
    private string _activeCategoryLabel = "Active Category";

    [ObservableProperty]
    private string _saveCategoryButton = "Save Category";

    [ObservableProperty]
    private string _cancelButton = "Cancel";

    [ObservableProperty]
    private string _closePanelTooltip = "Close panel";

    [ObservableProperty]
    private string _displayOrderHelp = "Lower numbers appear first in the list";

    [ObservableProperty]
    private string _noParentCategoryText = "No Parent Category";

    // Info Panel
    [ObservableProperty]
    private string _categoryInfoTitle = "ℹ️ Category Information";

    [ObservableProperty]
    private string _categoryInfoNameRequired = "• Category name is required and will be used in product listings";

    [ObservableProperty]
    private string _categoryInfoArabicOptional = "• Arabic name is optional for multilingual support";

    [ObservableProperty]
    private string _categoryInfoParentHierarchy = "• Parent category creates a hierarchical structure";

    [ObservableProperty]
    private string _categoryInfoDisplayOrder = "• Display order controls the sorting in category lists";

    // Dynamic Category Form Title
    public string CurrentCategoryFormTitle => IsEditMode ? EditCategoryTitle : AddCategoryTitle;

    #endregion

    #region Constructor

    public ProductManagementViewModel(
        IProductService productService,
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        InfrastructureServices.IDatabaseLocalizationService databaseLocalizationService,
        Action? navigateToAddProduct = null, 
        Action? navigateBack = null)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));
        _navigateToAddProduct = navigateToAddProduct;
        _navigateBack = navigateBack;
        
        // Subscribe to settings changes
        _themeService.ThemeChanged += OnThemeChanged;
        _zoomService.ZoomChanged += OnZoomChanged;
        _localizationService.LanguageChanged += OnLanguageChanged;
        _layoutDirectionService.DirectionChanged += OnLayoutDirectionChanged;
        _fontService.FontChanged += OnFontChanged;
        _databaseLocalizationService.LanguageChanged += OnDatabaseLanguageChanged;
        
        // Initialize current settings
        UpdateCurrentSettings();

        // Initialize with default values first
    InitializeDefaultValues();
        
        _ = InitializeAsync();
    }

    #endregion

    #region Initialization

    private async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = await GetTranslationAsync("status_loading", "Loading...");

            // Ensure translation keywords exist
            await ProductManagementTranslations.EnsureTranslationKeywordsAsync(_databaseLocalizationService);
            
            // Load translations first
            await LoadTranslationsAsync();

            await LoadCategoriesAsync();
            await LoadProductsAsync();

            DebugBindings();

            var statusFormat = await GetTranslationAsync("status_loaded_categories_products", "Loaded {0} categories and {1} products");
            StatusMessage = string.Format(statusFormat, Categories.Count - 1, Products.Count); // -1 for "All" category
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
            MessageBox.Show($"Failed to load data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Data Loading

    private async Task LoadCategoriesAsync()
    {
        var categoryList = await _productService.GetAllCategoriesAsync();
        Categories.Clear();
        ParentCategories.Clear();
        
        // Add "All Categories" option with translation
        var allCategoriesText = await GetTranslationAsync("all_categories", "All");
        Categories.Add(new CategoryDto { Id = 0, Name = allCategoriesText, Description = "All Categories" });
        
        // Add "No Parent" option for parent categories with translation
        var noParentText = await GetTranslationAsync("no_parent_category", "No Parent Category");
        ParentCategories.Add(new CategoryDto { Id = 0, Name = noParentText, Description = "Top Level Category" });
        
        foreach (var category in categoryList)
        {
            Categories.Add(category);
            ParentCategories.Add(category);
        }

        // Select "All Categories" by default
        SelectedCategory = Categories.FirstOrDefault();
    }

    private async Task LoadProductsAsync()
    {
        var productList = await _productService.GetAllProductsAsync();
        Products.Clear();
        
        foreach (var product in productList)
        {
            Products.Add(product);
        }

        FilterProducts();
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async void FilterProducts()
    {
        var filtered = Products.AsEnumerable();

        // Filter by category
        if (SelectedCategory != null && SelectedCategory.Id != 0)
        {
            filtered = filtered.Where(p => p.CategoryId == SelectedCategory.Id);
        }

        // Filter by search text based on selected search type
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            
            if (SelectedSearchType == SearchTypeProductName)
            {
                filtered = filtered.Where(p => 
                    p.Name.ToLower().Contains(searchLower) ||
                    (p.SKU?.ToLower().Contains(searchLower) ?? false) ||
                    (p.Barcode?.ToLower().Contains(searchLower) ?? false));
            }
            else if (SelectedSearchType == SearchTypeCategory)
            {
                filtered = filtered.Where(p => 
                    p.CategoryName.ToLower().Contains(searchLower));
            }
        }

        FilteredProducts.Clear();
        foreach (var product in filtered)
        {
            FilteredProducts.Add(product);
        }

        // Update status message with translation
        var showingFormat = await GetTranslationAsync("showing_products_count", "Showing {0} products");
        StatusMessage = string.Format(showingFormat, FilteredProducts.Count);
        
        // Update category counts after filtering
        UpdateCategoryProductCounts();
    }

    [RelayCommand]
    private void AddNewProduct()
    {
        if (_navigateToAddProduct != null)
        {
            _navigateToAddProduct();
        }
        else
        {
            // Fallback to existing form behavior
            CurrentProduct = new ProductDto
            {
                IsActive = true,
                CategoryId = SelectedCategory?.Id ?? 1,
                Color = "#FFC107"
            };
            IsEditMode = false;
            IsProductFormVisible = true;
        }
    }

    [RelayCommand]
    private void EditProduct(ProductDto? product)
    {
        if (product == null) return;

        CurrentProduct = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            StockQuantity = product.StockQuantity,
            SKU = product.SKU,
            Barcode = product.Barcode,
            IsActive = product.IsActive,
            CostPrice = product.CostPrice,
            Markup = product.Markup,
            ImagePath = product.ImagePath,
            Color = product.Color
        };
        IsEditMode = true;
        IsProductFormVisible = true;
    }

    [RelayCommand]
    private async Task SaveProduct()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Saving product...";

            if (IsEditMode)
            {
                await _productService.UpdateProductAsync(CurrentProduct);
                StatusMessage = "Product updated successfully";
            }
            else
            {
                await _productService.CreateProductAsync(CurrentProduct);
                StatusMessage = "Product created successfully";
            }

            IsProductFormVisible = false;
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving product: {ex.Message}";
            MessageBox.Show($"Failed to save product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteProduct(ProductDto? product)
    {
        if (product == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete '{product.Name}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Deleting product...";

                await _productService.DeleteProductAsync(product.Id);
                StatusMessage = "Product deleted successfully";
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting product: {ex.Message}";
                MessageBox.Show($"Failed to delete product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    [RelayCommand]
    private void AddNewCategory()
    {
        CurrentCategory = new CategoryDto { IsActive = true, DisplayOrder = 0 };
        IsEditMode = false;
        IsCategoryFormVisible = true;
    }

    [RelayCommand]
    private void EditCategory(CategoryDto? category)
    {
        if (category == null || category.Id == 0) return;

        CurrentCategory = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            ParentCategoryId = category.ParentCategoryId,
            DisplayOrder = category.DisplayOrder,
            NameArabic = category.NameArabic
        };
        IsEditMode = true;
        IsCategoryFormVisible = true;
    }

    [RelayCommand]
    private async Task SaveCategory()
    {
        try
        {
            IsLoading = true;
            StatusMessage = await GetTranslationAsync("status_saving", "Saving...");

            if (!await ValidateCategoryFormAsync())
            {
                StatusMessage = await GetTranslationAsync("status_ready", "Please fix category validation errors");
                return;
            }

            if (IsEditMode)
            {
                await _productService.UpdateCategoryAsync(CurrentCategory);
                StatusMessage = await GetTranslationAsync("status_category_updated", "Category updated successfully");
            }
            else
            {
                await _productService.CreateCategoryAsync(CurrentCategory);
                StatusMessage = await GetTranslationAsync("status_category_created", "Category created successfully");
            }

            IsCategoryFormVisible = false;
            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving category: {ex.Message}";
            MessageBox.Show($"Failed to save category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

private void DebugBindings()
{
    FileLogger.Log($"ColumnProductName: {ColumnProductName}");
    FileLogger.Log($"ColumnItemId: {ColumnItemId}");
    FileLogger.Log($"ColumnStock: {ColumnStock}");
    FileLogger.Log($"ColumnCategory: {ColumnCategory}");
    FileLogger.Log($"ColumnPrice: {ColumnPrice}");
    FileLogger.Log($"ColumnActions: {ColumnActions}");
    
    FileLogger.Log($"SearchTypeProductName: {SearchTypeProductName}");
    FileLogger.Log($"SearchTypeCategory: {SearchTypeCategory}");
}
    private async Task<bool> ValidateCategoryFormAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentCategory.Name))
        {
            var message = await GetTranslationAsync("validation_category_name_required", "Category name is required");
            var title = await GetTranslationAsync("validation_error_title", "Validation Error");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        
        if (CurrentCategory.Name.Length > 100)
        {
            var message = await GetTranslationAsync("validation_category_name_length", "Category name cannot exceed 100 characters");
            var title = await GetTranslationAsync("validation_error_title", "Validation Error");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(CurrentCategory.Description) && CurrentCategory.Description.Length > 500)
        {
            var message = await GetTranslationAsync("validation_description_length", "Category description cannot exceed 500 characters");
            var title = await GetTranslationAsync("validation_error_title", "Validation Error");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(CurrentCategory.NameArabic) && CurrentCategory.NameArabic.Length > 100)
        {
            var message = await GetTranslationAsync("validation_arabic_name_length", "Category name (Arabic) cannot exceed 100 characters");
            var title = await GetTranslationAsync("validation_error_title", "Validation Error");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (CurrentCategory.DisplayOrder < 0)
        {
            var message = await GetTranslationAsync("validation_display_order_negative", "Display order cannot be negative");
            var title = await GetTranslationAsync("validation_error_title", "Validation Error");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    [RelayCommand]
    private void CancelProductForm()
    {
        IsProductFormVisible = false;
    }

    [RelayCommand]
    private void CancelCategoryForm()
    {
        IsCategoryFormVisible = false;
    }

    [RelayCommand]
    private void DuplicateProduct(ProductDto? product)
    {
        if (product == null) return;

        CurrentProduct = new ProductDto
        {
            Name = $"{product.Name} (Copy)",
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            StockQuantity = 0,
            SKU = null, // Clear SKU for duplicate
            Barcode = null, // Clear barcode for duplicate
            IsActive = product.IsActive,
            CostPrice = product.CostPrice,
            Markup = product.Markup,
            ImagePath = product.ImagePath,
            Color = product.Color
        };
        IsEditMode = false;
        IsProductFormVisible = true;
    }

    [RelayCommand]
    private void NavigateBack()
    {
        _navigateBack?.Invoke();
    }

    [RelayCommand]
    private async Task RefreshData()
    {
        IsLoading = true;
        StatusMessage = await GetTranslationAsync("status_refreshing", "Refreshing data...");
        
        try
        {
            await LoadCategoriesAsync();
            await LoadProductsAsync();
            FilterProducts();
            StatusMessage = await GetTranslationAsync("status_data_refreshed", "Data refreshed successfully");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error refreshing data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ScrollCategoriesLeft()
    {
        CategoryScrollRequested?.Invoke("Left");
    }

    [RelayCommand]
    private void ScrollCategoriesRight()
    {
        CategoryScrollRequested?.Invoke("Right");
    }

    #endregion

    #region Events

    public event Action<string>? CategoryScrollRequested;

    #endregion

    #region Helper Methods
    
    private void UpdateCategoryProductCounts()
    {
        foreach (var category in Categories)
        {
            category.ProductCount = Products.Count(p => p.CategoryId == category.Id);
        }
    }

    #endregion

    #region Partial Methods for Property Changes

    partial void OnIsEditModeChanged(bool value)
    {
        OnPropertyChanged(nameof(CurrentCategoryFormTitle));
    }

    partial void OnSelectedCategoryChanged(CategoryDto? value)
    {
        FilterProducts();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterProducts();
    }

    partial void OnSelectedSearchTypeChanged(string value)
    {
        FilterProducts();
    }

    #endregion

    #region Settings Event Handlers

    private void OnThemeChanged(Theme newTheme)
    {
        CurrentTheme = newTheme.ToString();
    }

    private void OnZoomChanged(ZoomLevel newZoom)
    {
        CurrentZoom = (int)newZoom;
    }

    private async void OnLanguageChanged(SupportedLanguage newLanguage)
    {
        CurrentLanguage = newLanguage.ToString();
        await LoadTranslationsAsync();
    }

    private void OnLayoutDirectionChanged(LayoutDirection newDirection)
    {
        CurrentFlowDirection = newDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    private void OnFontChanged(FontSize newFontSize)
    {
        // Convert enum to approximate double value for display
        CurrentFontSize = newFontSize switch
        {
            FontSize.VerySmall => 10.0,
            FontSize.Small => 12.0,
            FontSize.Medium => 14.0,
            FontSize.Large => 16.0,
            _ => 14.0
        };
    }

    private void UpdateCurrentSettings()
    {
        CurrentTheme = _themeService.CurrentTheme.ToString();
        CurrentZoom = (int)_zoomService.CurrentZoomLevel;
        CurrentLanguage = _localizationService.CurrentLanguage.ToString();
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        CurrentFontSize = _fontService.CurrentFontSize switch
        {
            FontSize.VerySmall => 10.0,
            FontSize.Small => 12.0,
            FontSize.Medium => 14.0,
            FontSize.Large => 16.0,
            _ => 14.0
        };
    }

    private void InitializeDefaultValues()
{
    // Set default values for all UI properties
    ColumnProductName = "Product Name";
    ColumnItemId = "Item ID";
    ColumnStock = "Stock";
    ColumnCategory = "Category";
    ColumnPrice = "Price";
    ColumnActions = "Actions";
    
    SearchTypeProductName = "Product Name";
    SearchTypeCategory = "Category";
    
    // Force UI update
    OnPropertyChanged(nameof(ColumnProductName));
    OnPropertyChanged(nameof(ColumnItemId));
    OnPropertyChanged(nameof(ColumnStock));
    OnPropertyChanged(nameof(ColumnCategory));
    OnPropertyChanged(nameof(ColumnPrice));
    OnPropertyChanged(nameof(ColumnActions));
}

    private async Task LoadTranslationsAsync()
    {
        try
        {
            // Page and Navigation
            PageTitle = await GetTranslationAsync("product_management_title", "Product Management");
            BackButtonText = await GetTranslationAsync("back_button", "← Back");
            RefreshButtonText = await GetTranslationAsync("refresh_button", "🔄 Refresh");
            
            // Categories Section
            CategoriesHeaderText = await GetTranslationAsync("categories_header", "Categories");
            AddNewCategoryButtonText = await GetTranslationAsync("add_new_category_button", "➕ Add Category");
            AddNewProductButtonText = await GetTranslationAsync("add_new_product_button", "➕ Add Product");
            AllCategoriesText = await GetTranslationAsync("all_categories", "All");
            ItemsCountText = await GetTranslationAsync("items_count", "items");
            
            // Search Section
            SearchPlaceholder = await GetTranslationAsync("search_placeholder", "Search products...");
            SearchTypeProductName = await GetTranslationAsync("search_type_product_name", "Product Name");
            SearchTypeCategory = await GetTranslationAsync("search_type_category", "Category");
            ShowingProductsFormat = await GetTranslationAsync("showing_products_count", "Showing {0} products");
            
            // Table Headers
            ColumnProductName = await GetTranslationAsync("column_product_name", "Product Name");
            ColumnItemId = await GetTranslationAsync("column_item_id", "Item ID");
            ColumnStock = await GetTranslationAsync("column_stock", "Stock");
            ColumnCategory = await GetTranslationAsync("column_category", "Category");
            ColumnPrice = await GetTranslationAsync("column_price", "Price");
            ColumnActions = await GetTranslationAsync("column_actions", "Actions");
            
            // Action Tooltips
            EditProductTooltip = await GetTranslationAsync("action_edit", "Edit Product");
            DeleteProductTooltip = await GetTranslationAsync("action_delete", "Delete Product");
            DuplicateProductTooltip = await GetTranslationAsync("action_duplicate", "Duplicate Product");
            
            // Category Form
            AddCategoryTitle = await GetTranslationAsync("add_new_category_title", "Add New Category");
            EditCategoryTitle = await GetTranslationAsync("edit_category_title", "Edit Category");
            CategoryNameLabel = await GetTranslationAsync("category_name_label", "Category Name *");
            CategoryNameArabicLabel = await GetTranslationAsync("category_name_arabic_label", "Category Name (Arabic)");
            ParentCategoryLabel = await GetTranslationAsync("parent_category_label", "Parent Category");
            DisplayOrderLabel = await GetTranslationAsync("display_order_label", "Display Order");
            DescriptionLabel = await GetTranslationAsync("description_label", "Description");
            ActiveCategoryLabel = await GetTranslationAsync("active_category_label", "Active Category");
            SaveCategoryButton = await GetTranslationAsync("save_category_button", "Save Category");
            CancelButton = await GetTranslationAsync("cancel_button", "Cancel");
            ClosePanelTooltip = await GetTranslationAsync("close_panel_tooltip", "Close panel");
            DisplayOrderHelp = await GetTranslationAsync("display_order_help", "Lower numbers appear first in the list");
            NoParentCategoryText = await GetTranslationAsync("no_parent_category", "No Parent Category");
            
            // Info Panel
            CategoryInfoTitle = await GetTranslationAsync("category_info_title", "ℹ️ Category Information");
            CategoryInfoNameRequired = await GetTranslationAsync("category_info_name_required", "• Category name is required and will be used in product listings");
            CategoryInfoArabicOptional = await GetTranslationAsync("category_info_arabic_optional", "• Arabic name is optional for multilingual support");
            CategoryInfoParentHierarchy = await GetTranslationAsync("category_info_parent_hierarchy", "• Parent category creates a hierarchical structure");
            CategoryInfoDisplayOrder = await GetTranslationAsync("category_info_display_order", "• Display order controls the sorting in category lists");
            

            // Force update of all bound properties
        OnPropertyChanged(nameof(ColumnProductName));
        OnPropertyChanged(nameof(ColumnItemId));
        OnPropertyChanged(nameof(ColumnStock));
        OnPropertyChanged(nameof(ColumnCategory));
        OnPropertyChanged(nameof(ColumnPrice));
        OnPropertyChanged(nameof(ColumnActions));
        
        // Also force update of search-related properties
        OnPropertyChanged(nameof(SearchTypeProductName));
        OnPropertyChanged(nameof(SearchTypeCategory));
            // Status Message
            if (StatusMessage == "Ready" || string.IsNullOrEmpty(StatusMessage))
            {
                StatusMessage = await GetTranslationAsync("status_ready", "Ready");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading translations: {ex.Message}");
            // Fallback values are already set as defaults
        }
    }

    private async Task<string> GetTranslationAsync(string key, string fallback)
    {
        try
        {
            return await _databaseLocalizationService.GetTranslationAsync(key) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private async void OnDatabaseLanguageChanged(object? sender, string languageCode)
    {
        await LoadTranslationsAsync();
        
        // Reload categories to update translated labels
        await LoadCategoriesAsync();
        
        // Update product count display
        FilterProducts();
    }

    #endregion

    #region Cleanup

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Unsubscribe from events
            _themeService.ThemeChanged -= OnThemeChanged;
            _zoomService.ZoomChanged -= OnZoomChanged;
            _localizationService.LanguageChanged -= OnLanguageChanged;
            _layoutDirectionService.DirectionChanged -= OnLayoutDirectionChanged;
            _fontService.FontChanged -= OnFontChanged;
            _databaseLocalizationService.LanguageChanged -= OnDatabaseLanguageChanged;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
