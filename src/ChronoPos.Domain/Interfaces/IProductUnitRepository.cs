using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for ProductUnit entity operations
/// </summary>
public interface IProductUnitRepository : IRepository<ProductUnit>
{
    /// <summary>
    /// Gets all product units for a specific product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>List of product units</returns>
    Task<IEnumerable<ProductUnit>> GetByProductIdAsync(int productId);
    
    /// <summary>
    /// Gets the base unit for a specific product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>The base product unit or null if not found</returns>
    Task<ProductUnit?> GetBaseUnitByProductIdAsync(int productId);
    
    /// <summary>
    /// Gets all non-base units for a specific product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>List of non-base product units</returns>
    Task<IEnumerable<ProductUnit>> GetNonBaseUnitsByProductIdAsync(int productId);
    
    /// <summary>
    /// Gets a product unit by product ID and unit ID
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <param name="unitId">The unit ID</param>
    /// <returns>The product unit or null if not found</returns>
    Task<ProductUnit?> GetByProductIdAndUnitIdAsync(int productId, long unitId);
    
    /// <summary>
    /// Deletes all product units for a specific product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>Task</returns>
    Task DeleteByProductIdAsync(int productId);
    
    /// <summary>
    /// Updates the base unit designation for a product (ensures only one base unit exists)
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <param name="newBaseUnitId">The new base unit ID</param>
    /// <returns>Task</returns>
    Task UpdateBaseUnitAsync(int productId, long newBaseUnitId);
    
    /// <summary>
    /// Gets product units with their unit details
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>List of product units with unit information</returns>
    Task<IEnumerable<ProductUnit>> GetByProductIdWithUnitsAsync(int productId);
    
    /// <summary>
    /// Gets a product unit by its SKU
    /// </summary>
    /// <param name="sku">The SKU to search for</param>
    /// <returns>The product unit or null if not found</returns>
    Task<ProductUnit?> GetBySkuAsync(string sku);
    
    /// <summary>
    /// Gets all product units with their Product and Unit navigation properties
    /// </summary>
    /// <returns>List of all product units with navigation properties</returns>
    Task<IEnumerable<ProductUnit>> GetAllWithNavigationAsync();
}