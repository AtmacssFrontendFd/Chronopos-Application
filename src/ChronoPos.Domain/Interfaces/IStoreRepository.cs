using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for Store entity operations
/// </summary>
public interface IStoreRepository : IRepository<Store>
{
    /// <summary>
    /// Gets store by name
    /// </summary>
    /// <param name="name">Store name</param>
    /// <returns>Store entity if found</returns>
    Task<Store?> GetByNameAsync(string name);

    /// <summary>
    /// Gets all active stores
    /// </summary>
    /// <returns>Collection of active stores</returns>
    Task<IEnumerable<Store>> GetActiveStoresAsync();

    /// <summary>
    /// Gets the default store
    /// </summary>
    /// <returns>Default store entity if found</returns>
    Task<Store?> GetDefaultStoreAsync();

    /// <summary>
    /// Checks if store name exists (case-insensitive)
    /// </summary>
    /// <param name="name">Store name to check</param>
    /// <param name="excludeId">Store ID to exclude from check (for updates)</param>
    /// <returns>True if name exists</returns>
    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// Sets a store as default and unsets all other stores as default
    /// </summary>
    /// <param name="storeId">Store ID to set as default</param>
    /// <returns>Updated store</returns>
    Task<Store> SetAsDefaultAsync(int storeId);

    /// <summary>
    /// Gets stores with their stock levels count
    /// </summary>
    /// <returns>Collection of stores with stock levels</returns>
    Task<IEnumerable<Store>> GetStoresWithStockLevelsAsync();
}