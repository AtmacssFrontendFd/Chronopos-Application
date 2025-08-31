using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for Product entities with specific business operations
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Gets products by category asynchronously
    /// </summary>
    /// <param name="categoryId">Category identifier</param>
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
    
    /// <summary>
    /// Gets products with low stock asynchronously
    /// </summary>
    /// <param name="threshold">Stock threshold</param>
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
    
    /// <summary>
    /// Searches products by name or SKU asynchronously
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
    
    /// <summary>
    /// Updates product stock quantity asynchronously
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="newQuantity">New stock quantity</param>
    Task UpdateStockAsync(int productId, int newQuantity);
}
