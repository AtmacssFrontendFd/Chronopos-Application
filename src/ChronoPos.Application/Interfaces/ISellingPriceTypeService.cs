using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for managing selling price types
/// </summary>
public interface ISellingPriceTypeService
{
    /// <summary>
    /// Gets all selling price types
    /// </summary>
    Task<IEnumerable<SellingPriceTypeDto>> GetAllAsync();

    /// <summary>
    /// Gets all active selling price types
    /// </summary>
    Task<IEnumerable<SellingPriceTypeDto>> GetActiveAsync();

    /// <summary>
    /// Gets a selling price type by ID
    /// </summary>
    /// <param name="id">The ID of the selling price type</param>
    Task<SellingPriceTypeDto?> GetByIdAsync(long id);

    /// <summary>
    /// Creates a new selling price type
    /// </summary>
    /// <param name="dto">The selling price type data</param>
    Task<SellingPriceTypeDto> CreateAsync(CreateSellingPriceTypeDto dto);

    /// <summary>
    /// Updates an existing selling price type
    /// </summary>
    /// <param name="dto">The updated selling price type data</param>
    Task<SellingPriceTypeDto> UpdateAsync(UpdateSellingPriceTypeDto dto);

    /// <summary>
    /// Deletes a selling price type (soft delete)
    /// </summary>
    /// <param name="id">The ID of the selling price type to delete</param>
    /// <param name="deletedBy">ID of the user performing the deletion</param>
    Task DeleteAsync(long id, long deletedBy);

    /// <summary>
    /// Checks if a selling price type name already exists
    /// </summary>
    /// <param name="typeName">The type name to check</param>
    /// <param name="excludeId">ID to exclude from the check (for updates)</param>
    Task<bool> ExistsAsync(string typeName, long? excludeId = null);

    /// <summary>
    /// Gets count of selling price types for dashboard
    /// </summary>
    Task<int> GetCountAsync();
}