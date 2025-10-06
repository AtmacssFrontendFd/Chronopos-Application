using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for SellingPriceType entity
/// </summary>
public class SellingPriceTypeRepository : Repository<SellingPriceType>, ISellingPriceTypeRepository
{
    public SellingPriceTypeRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all active selling price types
    /// </summary>
    public async Task<IEnumerable<SellingPriceType>> GetActiveAsync()
    {
        return await _dbSet
            .Where(x => x.Status && x.DeletedAt == null)
            .OrderBy(x => x.TypeName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a selling price type by name
    /// </summary>
    /// <param name="typeName">The type name to search for</param>
    public async Task<SellingPriceType?> GetByNameAsync(string typeName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.TypeName == typeName && x.DeletedAt == null);
    }

    /// <summary>
    /// Checks if a selling price type name already exists
    /// </summary>
    /// <param name="typeName">The type name to check</param>
    /// <param name="excludeId">ID to exclude from the check (for updates)</param>
    public async Task<bool> ExistsAsync(string typeName, long? excludeId = null)
    {
        var query = _dbSet.Where(x => x.TypeName == typeName && x.DeletedAt == null);
        
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    /// <summary>
    /// Soft delete a selling price type
    /// </summary>
    /// <param name="id">The ID of the selling price type to delete</param>
    /// <param name="deletedBy">ID of the user performing the deletion</param>
    public async Task SoftDeleteAsync(long id, long deletedBy)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            entity.DeletedBy = deletedBy;
            entity.DeletedAt = DateTime.UtcNow;
            entity.Status = false;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Gets count of selling price types for dashboard
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        return await _dbSet
            .Where(x => x.Status && x.DeletedAt == null)
            .CountAsync();
    }

    /// <summary>
    /// Override GetAllAsync to exclude soft deleted items
    /// </summary>
    public override async Task<IEnumerable<SellingPriceType>> GetAllAsync()
    {
        return await _dbSet
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.TypeName)
            .ToListAsync();
    }

    /// <summary>
    /// Override GetByIdAsync to exclude soft deleted items
    /// </summary>
    public override async Task<SellingPriceType?> GetByIdAsync(int id)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
    }
}