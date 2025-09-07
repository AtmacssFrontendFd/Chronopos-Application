using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Product operations
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Gets all products asynchronously
    /// </summary>
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    
    /// <summary>
    /// Gets a product by ID asynchronously
    /// </summary>
    /// <param name="id">Product ID</param>
    Task<ProductDto?> GetProductByIdAsync(int id);
    
    /// <summary>
    /// Gets products by category asynchronously
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
    
    /// <summary>
    /// Searches products by name or SKU asynchronously
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
    
    /// <summary>
    /// Creates a new product asynchronously
    /// </summary>
    /// <param name="productDto">Product data</param>
    Task<ProductDto> CreateProductAsync(ProductDto productDto);
    
    /// <summary>
    /// Updates an existing product asynchronously
    /// </summary>
    /// <param name="productDto">Product data</param>
    Task<ProductDto> UpdateProductAsync(ProductDto productDto);
    
    /// <summary>
    /// Deletes a product asynchronously
    /// </summary>
    /// <param name="id">Product ID</param>
    Task DeleteProductAsync(int id);
    
    /// <summary>
    /// Gets products with low stock asynchronously
    /// </summary>
    /// <param name="threshold">Stock threshold</param>
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10);
    
    /// <summary>
    /// Gets all categories asynchronously
    /// </summary>
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    
    /// <summary>
    /// Creates a new category asynchronously
    /// </summary>
    /// <param name="categoryDto">Category data</param>
    Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto);
    
    /// <summary>
    /// Updates an existing category asynchronously
    /// </summary>
    /// <param name="categoryDto">Category data</param>
    Task<CategoryDto> UpdateCategoryAsync(CategoryDto categoryDto);
    
    /// <summary>
    /// Deletes a category asynchronously
    /// </summary>
    /// <param name="id">Category ID</param>
    Task DeleteCategoryAsync(int id);
    
    /// <summary>
    /// Gets category by ID with translations asynchronously
    /// </summary>
    /// <param name="id">Category ID</param>
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    
    /// <summary>
    /// Creates a category translation asynchronously
    /// </summary>
    /// <param name="translationDto">Category translation data</param>
    Task<CategoryTranslationDto> CreateCategoryTranslationAsync(CategoryTranslationDto translationDto);
}
