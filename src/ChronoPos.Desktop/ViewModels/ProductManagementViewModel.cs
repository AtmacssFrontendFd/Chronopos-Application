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
public partial class ProductManagementViewModel : ObservableObject
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

    [ObservableProperty]
    private string _backButtonText = "Back";

    [ObservableProperty]
    private string _refreshButtonText = "Refresh Data";

    [ObservableProperty]
    private string _categoriesHeaderText = "Categories";

    [ObservableProperty]
    private string _addNewCategoryButtonText = "Add New Category";

    [ObservableProperty]
    private string _addNewProductButtonText = "Add New Product";

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
        
        // Initialize current settings
        UpdateCurrentSettings();
        
        _ = InitializeAsync();
    }

    #endregion

    #region Initialization

    private async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading data...";

            await LoadCategoriesAsync();
            await LoadProductsAsync();

            StatusMessage = $"Loaded {Categories.Count} categories and {Products.Count} products";
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
        
        // Add "All Categories" option
        Categories.Add(new CategoryDto { Id = 0, Name = "All", Description = "All Categories" });
        
        // Add "No Parent" option for parent categories
        ParentCategories.Add(new CategoryDto { Id = 0, Name = "No Parent Category", Description = "Top Level Category" });
        
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
    private void FilterProducts()
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
            
            if (SelectedSearchType == "Product Name")
            {
                filtered = filtered.Where(p => 
                    p.Name.ToLower().Contains(searchLower) ||
                    (p.SKU?.ToLower().Contains(searchLower) ?? false) ||
                    (p.Barcode?.ToLower().Contains(searchLower) ?? false));
            }
            else if (SelectedSearchType == "Category")
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

        StatusMessage = $"Showing {FilteredProducts.Count} of {Products.Count} products";
        
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
            StatusMessage = "Saving category...";

            if (!ValidateCategoryForm())
            {
                StatusMessage = "Please fix category validation errors";
                return;
            }

            if (IsEditMode)
            {
                await _productService.UpdateCategoryAsync(CurrentCategory);
                StatusMessage = "Category updated successfully";
            }
            else
            {
                await _productService.CreateCategoryAsync(CurrentCategory);
                StatusMessage = "Category created successfully";
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

    private bool ValidateCategoryForm()
    {
        if (string.IsNullOrWhiteSpace(CurrentCategory.Name))
        {
            MessageBox.Show("Category name is required", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        
        if (CurrentCategory.Name.Length > 100)
        {
            MessageBox.Show("Category name cannot exceed 100 characters", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(CurrentCategory.Description) && CurrentCategory.Description.Length > 500)
        {
            MessageBox.Show("Category description cannot exceed 500 characters", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(CurrentCategory.NameArabic) && CurrentCategory.NameArabic.Length > 100)
        {
            MessageBox.Show("Category name (Arabic) cannot exceed 100 characters", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (CurrentCategory.DisplayOrder < 0)
        {
            MessageBox.Show("Display order cannot be negative", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        StatusMessage = "Refreshing data...";
        
        try
        {
            await LoadCategoriesAsync();
            await LoadProductsAsync();
            FilterProducts();
            StatusMessage = "Data refreshed successfully";
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

    private async Task LoadTranslationsAsync()
    {
        try
        {
            BackButtonText = await _databaseLocalizationService.GetTranslationAsync("back_button") ?? "Back";
            RefreshButtonText = await _databaseLocalizationService.GetTranslationAsync("refresh_button") ?? "Refresh Data";
            CategoriesHeaderText = await _databaseLocalizationService.GetTranslationAsync("categories_header") ?? "Categories";
            AddNewCategoryButtonText = await _databaseLocalizationService.GetTranslationAsync("add_new_category_button") ?? "Add New Category";
            AddNewProductButtonText = await _databaseLocalizationService.GetTranslationAsync("add_new_product_button") ?? "Add New Product";
            StatusMessage = await _databaseLocalizationService.GetTranslationAsync("status_ready") ?? "Ready";
        }
        catch (Exception)
        {
            // Fallback to default values if translation fails
            BackButtonText = "Back";
            RefreshButtonText = "Refresh Data";
            StatusMessage = "Ready";
        }
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
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
