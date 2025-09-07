using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for comprehensive product management with category support
/// </summary>
public partial class ProductManagementViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly Action? _navigateToAddProduct;
    private readonly Action? _navigateBack;

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

    #endregion

    #region Constructor

    public ProductManagementViewModel(IProductService productService, Action? navigateToAddProduct = null, Action? navigateBack = null)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _navigateToAddProduct = navigateToAddProduct;
        _navigateBack = navigateBack;
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
        
        // Add "All Categories" option
        Categories.Add(new CategoryDto { Id = 0, Name = "All", Description = "All Categories" });
        
        foreach (var category in categoryList)
        {
            Categories.Add(category);
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

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(p => 
                p.Name.ToLower().Contains(searchLower) ||
                (p.SKU?.ToLower().Contains(searchLower) ?? false) ||
                (p.Barcode?.ToLower().Contains(searchLower) ?? false) ||
                p.CategoryName.ToLower().Contains(searchLower));
        }

        FilteredProducts.Clear();
        foreach (var product in filtered)
        {
            FilteredProducts.Add(product);
        }

        StatusMessage = $"Showing {FilteredProducts.Count} of {Products.Count} products";
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
        CurrentCategory = new CategoryDto { IsActive = true };
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
            IsActive = category.IsActive
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

    #endregion
}
