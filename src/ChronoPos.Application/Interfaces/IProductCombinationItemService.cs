using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for ProductCombinationItem operations
/// </summary>
public interface IProductCombinationItemService
{
    /// <summary>
    /// Gets all combination items
    /// </summary>
    /// <returns>List of combination item DTOs</returns>
    Task<IEnumerable<ProductCombinationItemDto>> GetAllCombinationItemsAsync();

    /// <summary>
    /// Gets a combination item by ID
    /// </summary>
    /// <param name="id">Combination item ID</param>
    /// <returns>Combination item DTO or null</returns>
    Task<ProductCombinationItemDto?> GetCombinationItemByIdAsync(int id);

    /// <summary>
    /// Gets all combination items for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>List of combination item DTOs</returns>
    Task<IEnumerable<ProductCombinationItemDto>> GetCombinationItemsByProductUnitIdAsync(int productUnitId);

    /// <summary>
    /// Gets all combination items for a specific attribute value
    /// </summary>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <returns>List of combination item DTOs</returns>
    Task<IEnumerable<ProductCombinationItemDto>> GetCombinationItemsByAttributeValueIdAsync(int attributeValueId);

    /// <summary>
    /// Gets combination item by product unit and attribute value
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <returns>Combination item DTO or null</returns>
    Task<ProductCombinationItemDto?> GetCombinationItemByProductUnitAndAttributeValueAsync(int productUnitId, int attributeValueId);

    /// <summary>
    /// Creates a new combination item
    /// </summary>
    /// <param name="createDto">Create combination item DTO</param>
    /// <returns>Created combination item DTO</returns>
    Task<ProductCombinationItemDto> CreateCombinationItemAsync(CreateProductCombinationItemDto createDto);

    /// <summary>
    /// Creates multiple combination items
    /// </summary>
    /// <param name="createDtos">Collection of create combination item DTOs</param>
    /// <returns>Collection of created combination item DTOs</returns>
    Task<IEnumerable<ProductCombinationItemDto>> CreateMultipleCombinationItemsAsync(IEnumerable<CreateProductCombinationItemDto> createDtos);

    /// <summary>
    /// Updates an existing combination item
    /// </summary>
    /// <param name="id">Combination item ID</param>
    /// <param name="updateDto">Update combination item DTO</param>
    /// <returns>Updated combination item DTO or null</returns>
    Task<ProductCombinationItemDto?> UpdateCombinationItemAsync(int id, UpdateProductCombinationItemDto updateDto);

    /// <summary>
    /// Deletes a combination item
    /// </summary>
    /// <param name="id">Combination item ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteCombinationItemAsync(int id);

    /// <summary>
    /// Deletes all combination items for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>Number of deleted items</returns>
    Task<int> DeleteCombinationItemsByProductUnitIdAsync(int productUnitId);

    /// <summary>
    /// Deletes all combination items for a specific attribute value
    /// </summary>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <returns>Number of deleted items</returns>
    Task<int> DeleteCombinationItemsByAttributeValueIdAsync(int attributeValueId);

    /// <summary>
    /// Checks if a combination already exists
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <param name="excludeId">Optional ID to exclude from check</param>
    /// <returns>True if combination exists</returns>
    Task<bool> CombinationExistsAsync(int productUnitId, int attributeValueId, int? excludeId = null);

    /// <summary>
    /// Gets combination items by multiple product unit IDs
    /// </summary>
    /// <param name="productUnitIds">Collection of product unit IDs</param>
    /// <returns>Collection of combination item DTOs</returns>
    Task<IEnumerable<ProductCombinationItemDto>> GetCombinationItemsByProductUnitIdsAsync(IEnumerable<int> productUnitIds);

    /// <summary>
    /// Gets combination items by multiple attribute value IDs
    /// </summary>
    /// <param name="attributeValueIds">Collection of attribute value IDs</param>
    /// <returns>Collection of combination item DTOs</returns>
    Task<IEnumerable<ProductCombinationItemDto>> GetCombinationItemsByAttributeValueIdsAsync(IEnumerable<int> attributeValueIds);
}