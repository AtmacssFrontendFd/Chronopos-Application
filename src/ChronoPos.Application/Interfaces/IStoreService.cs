using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Store operations
/// </summary>
public interface IStoreService
{
    /// <summary>
    /// Gets all stores
    /// </summary>
    /// <returns>Collection of store DTOs</returns>
    Task<IEnumerable<StoreDto>> GetAllAsync();

    /// <summary>
    /// Gets all active stores
    /// </summary>
    /// <returns>Collection of active store DTOs</returns>
    Task<IEnumerable<StoreDto>> GetActiveStoresAsync();

    /// <summary>
    /// Gets store by ID
    /// </summary>
    /// <param name="id">Store ID</param>
    /// <returns>Store DTO if found</returns>
    Task<StoreDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets store by name
    /// </summary>
    /// <param name="name">Store name</param>
    /// <returns>Store DTO if found</returns>
    Task<StoreDto?> GetByNameAsync(string name);

    /// <summary>
    /// Gets the default store
    /// </summary>
    /// <returns>Default store DTO if found</returns>
    Task<StoreDto?> GetDefaultStoreAsync();

    /// <summary>
    /// Creates a new store
    /// </summary>
    /// <param name="storeDto">Store data</param>
    /// <returns>Created store DTO</returns>
    Task<StoreDto> CreateAsync(CreateStoreDto storeDto);

    /// <summary>
    /// Updates an existing store
    /// </summary>
    /// <param name="id">Store ID</param>
    /// <param name="storeDto">Updated store data</param>
    /// <returns>Updated store DTO</returns>
    Task<StoreDto> UpdateAsync(int id, UpdateStoreDto storeDto);

    /// <summary>
    /// Deletes a store
    /// </summary>
    /// <param name="id">Store ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if store name exists
    /// </summary>
    /// <param name="name">Store name</param>
    /// <param name="excludeId">Store ID to exclude from check</param>
    /// <returns>True if name exists</returns>
    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// Sets a store as default
    /// </summary>
    /// <param name="id">Store ID to set as default</param>
    /// <returns>Updated store DTO</returns>
    Task<StoreDto> SetAsDefaultAsync(int id);

    /// <summary>
    /// Gets stores with their stock levels count
    /// </summary>
    /// <returns>Collection of stores with stock levels</returns>
    Task<IEnumerable<StoreDto>> GetStoresWithStockLevelsAsync();
}