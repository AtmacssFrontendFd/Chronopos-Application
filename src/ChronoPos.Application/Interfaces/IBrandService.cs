using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Brand operations
/// </summary>
public interface IBrandService
{
    /// <summary>
    /// Gets all brands
    /// </summary>
    /// <returns>Collection of brand DTOs</returns>
    Task<IEnumerable<BrandDto>> GetAllAsync();

    /// <summary>
    /// Gets all active brands
    /// </summary>
    /// <returns>Collection of active brand DTOs</returns>
    Task<IEnumerable<BrandDto>> GetActiveBrandsAsync();

    /// <summary>
    /// Gets brand by ID
    /// </summary>
    /// <param name="id">Brand ID</param>
    /// <returns>Brand DTO if found</returns>
    Task<BrandDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets brand by name
    /// </summary>
    /// <param name="name">Brand name</param>
    /// <returns>Brand DTO if found</returns>
    Task<BrandDto?> GetByNameAsync(string name);

    /// <summary>
    /// Creates a new brand
    /// </summary>
    /// <param name="brandDto">Brand data</param>
    /// <returns>Created brand DTO</returns>
    Task<BrandDto> CreateAsync(CreateBrandDto brandDto);

    /// <summary>
    /// Updates an existing brand
    /// </summary>
    /// <param name="id">Brand ID</param>
    /// <param name="brandDto">Updated brand data</param>
    /// <returns>Updated brand DTO</returns>
    Task<BrandDto> UpdateAsync(int id, UpdateBrandDto brandDto);

    /// <summary>
    /// Deletes a brand
    /// </summary>
    /// <param name="id">Brand ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if brand name exists
    /// </summary>
    /// <param name="name">Brand name</param>
    /// <param name="excludeId">Brand ID to exclude from check</param>
    /// <returns>True if name exists</returns>
    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// Gets brands with their product count
    /// </summary>
    /// <returns>Collection of brands with product counts</returns>
    Task<IEnumerable<BrandDto>> GetBrandsWithProductCountAsync();
}
