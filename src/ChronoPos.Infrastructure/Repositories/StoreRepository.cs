using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Store entity operations
/// </summary>
public class StoreRepository : Repository<Store>, IStoreRepository
{
    public StoreRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets store by name
    /// </summary>
    /// <param name="name">Store name</param>
    /// <returns>Store entity if found</returns>
    public async Task<Store?> GetByNameAsync(string name)
    {
        return await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());
    }

    /// <summary>
    /// Gets all active stores
    /// </summary>
    /// <returns>Collection of active stores</returns>
    public async Task<IEnumerable<Store>> GetActiveStoresAsync()
    {
        return await _context.Set<Store>()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the default store
    /// </summary>
    /// <returns>Default store entity if found</returns>
    public async Task<Store?> GetDefaultStoreAsync()
    {
        return await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.IsDefault);
    }

    /// <summary>
    /// Checks if store name exists (case-insensitive)
    /// </summary>
    /// <param name="name">Store name to check</param>
    /// <param name="excludeId">Store ID to exclude from check (for updates)</param>
    /// <returns>True if name exists</returns>
    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.Set<Store>()
            .Where(s => s.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Sets a store as default and unsets all other stores as default
    /// </summary>
    /// <param name="storeId">Store ID to set as default</param>
    /// <returns>Updated store</returns>
    public async Task<Store> SetAsDefaultAsync(int storeId)
    {
        // First, unset all stores as default
        var allStores = await _context.Set<Store>().ToListAsync();
        foreach (var store in allStores)
        {
            store.IsDefault = false;
            store.UpdatedAt = DateTime.UtcNow;
        }

        // Then set the specified store as default
        var targetStore = allStores.FirstOrDefault(s => s.Id == storeId);
        if (targetStore == null)
        {
            throw new ArgumentException($"Store with ID {storeId} not found");
        }

        targetStore.IsDefault = true;
        targetStore.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return targetStore;
    }

    /// <summary>
    /// Gets stores with their stock levels count
    /// </summary>
    /// <returns>Collection of stores with stock levels</returns>
    public async Task<IEnumerable<Store>> GetStoresWithStockLevelsAsync()
    {
        return await _context.Set<Store>()
            .Include(s => s.StockLevels)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all stores including related data
    /// </summary>
    /// <returns>Collection of stores with navigation properties</returns>
    public override async Task<IEnumerable<Store>> GetAllAsync()
    {
        return await _context.Set<Store>()
            .Include(s => s.StockLevels)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets store by ID including related data
    /// </summary>
    /// <param name="id">Store ID</param>
    /// <returns>Store with navigation properties</returns>
    public override async Task<Store?> GetByIdAsync(int id)
    {
        return await _context.Set<Store>()
            .Include(s => s.StockLevels)
            .Include(s => s.StockTransactions)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}