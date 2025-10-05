using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Product Group management
/// </summary>
public interface IProductGroupService
{
    /// <summary>
    /// Get all product groups
    /// </summary>
    Task<IEnumerable<ProductGroupDto>> GetAllAsync();

    /// <summary>
    /// Get product group by ID
    /// </summary>
    Task<ProductGroupDto?> GetByIdAsync(int id);

    /// <summary>
    /// Get detailed product group with all items
    /// </summary>
    Task<ProductGroupDetailDto?> GetDetailByIdAsync(int id);

    /// <summary>
    /// Get only active product groups
    /// </summary>
    Task<IEnumerable<ProductGroupDto>> GetActiveAsync();

    /// <summary>
    /// Create a new product group
    /// </summary>
    Task<ProductGroupDto> CreateAsync(CreateProductGroupDto dto);

    /// <summary>
    /// Update an existing product group
    /// </summary>
    Task<ProductGroupDto> UpdateAsync(UpdateProductGroupDto dto);

    /// <summary>
    /// Delete a product group (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Search product groups by name
    /// </summary>
    Task<IEnumerable<ProductGroupDto>> SearchAsync(string searchTerm);

    /// <summary>
    /// Get all items in a product group
    /// </summary>
    Task<IEnumerable<ProductGroupItemDto>> GetGroupItemsAsync(int groupId);

    /// <summary>
    /// Add an item to a product group
    /// </summary>
    Task<ProductGroupItemDto> AddItemToGroupAsync(CreateProductGroupItemDto dto);

    /// <summary>
    /// Remove an item from a product group
    /// </summary>
    Task<bool> RemoveItemFromGroupAsync(int itemId);

    /// <summary>
    /// Update a product group item
    /// </summary>
    Task<ProductGroupItemDto> UpdateItemAsync(ProductGroupItemDto dto);

    /// <summary>
    /// Get count of product groups
    /// </summary>
    Task<int> GetCountAsync();

    /// <summary>
    /// Check if product group name already exists
    /// </summary>
    Task<bool> ExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// Calculate total price for a product group
    /// </summary>
    Task<decimal> CalculateGroupPriceAsync(int groupId);
}
