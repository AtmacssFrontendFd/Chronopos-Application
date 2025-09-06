using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for ShopLocation operations
/// </summary>
public interface IShopLocationRepository : IRepository<ShopLocation>
{
    /// <summary>
    /// Gets all active shop locations asynchronously
    /// </summary>
    Task<IEnumerable<ShopLocation>> GetActiveLocationsAsync();
    
    /// <summary>
    /// Gets shop locations by shop ID asynchronously
    /// </summary>
    /// <param name="shopId">Shop ID</param>
    Task<IEnumerable<ShopLocation>> GetByShopIdAsync(int shopId);
    
    /// <summary>
    /// Gets shop locations by location type asynchronously
    /// </summary>
    /// <param name="locationType">Location type</param>
    Task<IEnumerable<ShopLocation>> GetByLocationTypeAsync(string locationType);
}
