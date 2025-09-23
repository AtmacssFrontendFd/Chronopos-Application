using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for ProductCombinationItem entity operations
/// </summary>
public interface IProductCombinationItemRepository : IRepository<ProductCombinationItem>
{
    /// <summary>
    /// Gets all combination items for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>Collection of combination items</returns>
    Task<IEnumerable<ProductCombinationItem>> GetByProductUnitIdAsync(int productUnitId);

    /// <summary>
    /// Gets all combination items for a specific attribute value
    /// </summary>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <returns>Collection of combination items</returns>
    Task<IEnumerable<ProductCombinationItem>> GetByAttributeValueIdAsync(int attributeValueId);

    /// <summary>
    /// Gets combination item by product unit and attribute value
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <returns>Combination item if found</returns>
    Task<ProductCombinationItem?> GetByProductUnitAndAttributeValueAsync(int productUnitId, int attributeValueId);

    /// <summary>
    /// Checks if combination already exists
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <param name="excludeId">ID to exclude from check (for updates)</param>
    /// <returns>True if combination exists</returns>
    Task<bool> CombinationExistsAsync(int productUnitId, int attributeValueId, int? excludeId = null);

    /// <summary>
    /// Gets all combination items with navigation properties
    /// </summary>
    /// <returns>Collection of combination items with ProductUnit and AttributeValue loaded</returns>
    Task<IEnumerable<ProductCombinationItem>> GetAllWithNavigationAsync();

    /// <summary>
    /// Deletes all combination items for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>Number of deleted items</returns>
    Task<int> DeleteByProductUnitIdAsync(int productUnitId);

    /// <summary>
    /// Deletes all combination items for a specific attribute value
    /// </summary>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <returns>Number of deleted items</returns>
    Task<int> DeleteByAttributeValueIdAsync(int attributeValueId);

    /// <summary>
    /// Gets combination items by multiple product unit IDs
    /// </summary>
    /// <param name="productUnitIds">Collection of product unit IDs</param>
    /// <returns>Collection of combination items</returns>
    Task<IEnumerable<ProductCombinationItem>> GetByProductUnitIdsAsync(IEnumerable<int> productUnitIds);

    /// <summary>
    /// Gets combination items by multiple attribute value IDs
    /// </summary>
    /// <param name="attributeValueIds">Collection of attribute value IDs</param>
    /// <returns>Collection of combination items</returns>
    Task<IEnumerable<ProductCombinationItem>> GetByAttributeValueIdsAsync(IEnumerable<int> attributeValueIds);
}
