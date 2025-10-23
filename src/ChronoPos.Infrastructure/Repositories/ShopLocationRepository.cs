using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ShopLocation operations
/// </summary>
public class ShopLocationRepository : Repository<ShopLocation>, IShopLocationRepository
{
    public ShopLocationRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ShopLocation>> GetActiveLocationsAsync()
    {
        return await _dbSet
            .Where(sl => sl.Status == "Active" && sl.DeletedAt == null)
            .OrderBy(sl => sl.LocationName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShopLocation>> GetByShopIdAsync(int shopId)
    {
        return await _dbSet
            .Where(sl => sl.ShopId == shopId && sl.DeletedAt == null)
            .OrderBy(sl => sl.LocationName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShopLocation>> GetByLocationTypeAsync(string locationType)
    {
        return await _dbSet
            .Where(sl => sl.LocationType == locationType && sl.DeletedAt == null)
            .OrderBy(sl => sl.LocationName)
            .ToListAsync();
    }

    public override async Task<IEnumerable<ShopLocation>> GetAllAsync()
    {
        return await _dbSet
            .Where(sl => sl.DeletedAt == null)
            .OrderBy(sl => sl.LocationName)
            .ToListAsync();
    }

    public override async Task<ShopLocation?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Where(sl => sl.Id == id && sl.DeletedAt == null)
            .FirstOrDefaultAsync();
    }
}
