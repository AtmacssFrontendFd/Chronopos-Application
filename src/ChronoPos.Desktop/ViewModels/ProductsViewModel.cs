using CommunityToolkit.Mvvm.ComponentModel;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for products management
/// </summary>
public partial class ProductsViewModel : ObservableObject
{
    private readonly IProductService _productService;

    [ObservableProperty]
    private IEnumerable<ProductDto> products = new List<ProductDto>();

    [ObservableProperty]
    private ProductDto? selectedProduct;

    [ObservableProperty]
    private string searchText = string.Empty;

    public ProductsViewModel(IProductService productService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    public async Task LoadProductsAsync()
    {
        try
        {
            Products = await _productService.GetAllProductsAsync();
        }
        catch (Exception ex)
        {
            // Handle error
            throw new InvalidOperationException($"Failed to load products: {ex.Message}", ex);
        }
    }
}
