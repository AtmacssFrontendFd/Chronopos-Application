using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Product Group Item operations
/// </summary>
public interface IProductGroupItemService
{
    /// <summary>
    /// Get all product group items
    /// </summary>
    Task<IEnumerable<ProductGroupItemDto>> GetAllAsync();
    
    /// <summary>
    /// Get product group item by ID
    /// </summary>
    Task<ProductGroupItemDto?> GetByIdAsync(int id);
    
    /// <summary>
    /// Get all items for a specific product group
    /// </summary>
    Task<IEnumerable<ProductGroupItemDto>> GetByProductGroupIdAsync(int productGroupId);
    
    /// <summary>
    /// Create a new product group item
    /// </summary>
    Task<ProductGroupItemDto> CreateAsync(CreateProductGroupItemDto createDto);
    
    /// <summary>
    /// Update an existing product group item
    /// </summary>
    Task<ProductGroupItemDto> UpdateAsync(UpdateProductGroupItemDto updateDto);
    
    /// <summary>
    /// Delete a product group item
    /// </summary>
    Task DeleteAsync(int id);
    
    /// <summary>
    /// Get count of items in a product group
    /// </summary>
    Task<int> GetCountByProductGroupIdAsync(int productGroupId);
}
