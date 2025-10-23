using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for RestaurantTable entity operations
/// </summary>
public class RestaurantTableRepository : Repository<RestaurantTable>, IRestaurantTableRepository
{
    public RestaurantTableRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets table by table number
    /// </summary>
    /// <param name="tableNumber">Table number</param>
    /// <returns>RestaurantTable entity if found</returns>
    public async Task<RestaurantTable?> GetByTableNumberAsync(string tableNumber)
    {
        return await _context.Set<RestaurantTable>()
            .FirstOrDefaultAsync(t => t.TableNumber.ToLower() == tableNumber.ToLower());
    }

    /// <summary>
    /// Gets all available tables
    /// </summary>
    /// <returns>Collection of available tables</returns>
    public async Task<IEnumerable<RestaurantTable>> GetAvailableTablesAsync()
    {
        return await _context.Set<RestaurantTable>()
            .Where(t => t.Status == "available")
            .OrderBy(t => t.TableNumber)
            .ToListAsync();
    }

    /// <summary>
    /// Gets tables by status
    /// </summary>
    /// <param name="status">Table status</param>
    /// <returns>Collection of tables with specified status</returns>
    public async Task<IEnumerable<RestaurantTable>> GetTablesByStatusAsync(string status)
    {
        return await _context.Set<RestaurantTable>()
            .Where(t => t.Status == status)
            .OrderBy(t => t.TableNumber)
            .ToListAsync();
    }

    /// <summary>
    /// Gets tables by location
    /// </summary>
    /// <param name="location">Location name</param>
    /// <returns>Collection of tables in specified location</returns>
    public async Task<IEnumerable<RestaurantTable>> GetTablesByLocationAsync(string location)
    {
        return await _context.Set<RestaurantTable>()
            .Where(t => t.Location != null && t.Location.ToLower() == location.ToLower())
            .OrderBy(t => t.TableNumber)
            .ToListAsync();
    }

    /// <summary>
    /// Gets tables with minimum capacity
    /// </summary>
    /// <param name="capacity">Minimum capacity required</param>
    /// <returns>Collection of tables with at least the specified capacity</returns>
    public async Task<IEnumerable<RestaurantTable>> GetTablesByMinCapacityAsync(int capacity)
    {
        return await _context.Set<RestaurantTable>()
            .Where(t => t.Capacity >= capacity)
            .OrderBy(t => t.Capacity)
            .ThenBy(t => t.TableNumber)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if table number exists (case-insensitive)
    /// </summary>
    /// <param name="tableNumber">Table number to check</param>
    /// <param name="excludeId">Table ID to exclude from check (for updates)</param>
    /// <returns>True if table number exists</returns>
    public async Task<bool> TableNumberExistsAsync(string tableNumber, int? excludeId = null)
    {
        var query = _context.Set<RestaurantTable>()
            .Where(t => t.TableNumber.ToLower() == tableNumber.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Gets tables with their reservation count
    /// </summary>
    /// <returns>Collection of tables with reservation counts</returns>
    public async Task<IEnumerable<RestaurantTable>> GetTablesWithReservationCountAsync()
    {
        return await _context.Set<RestaurantTable>()
            .Include(t => t.Reservations.Where(r => !r.IsDeleted))
            .OrderBy(t => t.TableNumber)
            .ToListAsync();
    }

    /// <summary>
    /// Updates table status
    /// </summary>
    /// <param name="id">Table ID</param>
    /// <param name="status">New status</param>
    /// <returns>True if update was successful</returns>
    public async Task<bool> UpdateTableStatusAsync(int id, string status)
    {
        var table = await _context.Set<RestaurantTable>().FindAsync(id);
        if (table == null)
        {
            return false;
        }

        table.Status = status;
        return true;
    }

    /// <summary>
    /// Gets all tables including related data
    /// </summary>
    /// <returns>Collection of tables with navigation properties</returns>
    public override async Task<IEnumerable<RestaurantTable>> GetAllAsync()
    {
        return await _context.Set<RestaurantTable>()
            .Include(t => t.Reservations.Where(r => !r.IsDeleted))
            .OrderBy(t => t.TableNumber)
            .ToListAsync();
    }

    /// <summary>
    /// Gets table by ID including related data
    /// </summary>
    /// <param name="id">Table ID</param>
    /// <returns>Table with navigation properties</returns>
    public override async Task<RestaurantTable?> GetByIdAsync(int id)
    {
        return await _context.Set<RestaurantTable>()
            .Include(t => t.Reservations.Where(r => !r.IsDeleted))
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}
