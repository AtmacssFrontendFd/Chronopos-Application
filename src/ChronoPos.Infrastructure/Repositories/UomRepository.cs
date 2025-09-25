using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Unit of Measurement operations
/// </summary>
public class UomRepository : Repository<UnitOfMeasurement>, IUomRepository
{
    public UomRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all active UOMs asynchronously
    /// </summary>
    public async Task<IEnumerable<UnitOfMeasurement>> GetActiveUomsAsync()
    {
        return await _dbSet
            .Where(u => u.IsActive && u.DeletedAt == null)
            .Include(u => u.BaseUom)
            .Include(u => u.Creator)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets UOMs by type (Base or Derived) asynchronously
    /// </summary>
    /// <param name="type">UOM type (Base or Derived)</param>
    public async Task<IEnumerable<UnitOfMeasurement>> GetUomsByTypeAsync(string type)
    {
        return await _dbSet
            .Where(u => u.Type == type && u.DeletedAt == null)
            .Include(u => u.BaseUom)
            .Include(u => u.Creator)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets UOMs by category title asynchronously
    /// </summary>
    /// <param name="categoryTitle">Category title</param>
    public async Task<IEnumerable<UnitOfMeasurement>> GetUomsByCategoryAsync(string categoryTitle)
    {
        return await _dbSet
            .Where(u => u.CategoryTitle == categoryTitle && u.DeletedAt == null)
            .Include(u => u.BaseUom)
            .Include(u => u.Creator)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all derived UOMs for a base UOM asynchronously
    /// </summary>
    /// <param name="baseUomId">Base UOM ID</param>
    public async Task<IEnumerable<UnitOfMeasurement>> GetDerivedUomsAsync(long baseUomId)
    {
        return await _dbSet
            .Where(u => u.BaseUomId == baseUomId && u.DeletedAt == null)
            .Include(u => u.Creator)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a UOM by its abbreviation asynchronously
    /// </summary>
    /// <param name="abbreviation">UOM abbreviation</param>
    public async Task<UnitOfMeasurement?> GetByAbbreviationAsync(string abbreviation)
    {
        return await _dbSet
            .Where(u => u.Abbreviation == abbreviation && u.DeletedAt == null)
            .Include(u => u.BaseUom)
            .Include(u => u.Creator)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Checks if a UOM name exists asynchronously
    /// </summary>
    /// <param name="name">UOM name</param>
    /// <param name="excludeId">ID to exclude from the check (for updates)</param>
    public async Task<bool> ExistsByNameAsync(string name, long? excludeId = null)
    {
        var query = _dbSet.Where(u => u.Name == name && u.DeletedAt == null);
        
        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    /// <summary>
    /// Checks if a UOM abbreviation exists asynchronously
    /// </summary>
    /// <param name="abbreviation">UOM abbreviation</param>
    /// <param name="excludeId">ID to exclude from the check (for updates)</param>
    public async Task<bool> ExistsByAbbreviationAsync(string abbreviation, long? excludeId = null)
    {
        var query = _dbSet.Where(u => u.Abbreviation == abbreviation && u.DeletedAt == null);
        
        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    /// <summary>
    /// Soft deletes a UOM asynchronously
    /// </summary>
    /// <param name="id">UOM ID</param>
    /// <param name="deletedBy">User ID performing the deletion</param>
    public async Task SoftDeleteAsync(long id, int deletedBy)
    {
        var uom = await _dbSet.FindAsync(id);
        if (uom != null)
        {
            uom.DeletedAt = DateTime.UtcNow;
            uom.DeletedBy = deletedBy;
            uom.IsActive = false;
            uom.Status = "Deleted";
            
            await UpdateAsync(uom);
        }
    }

    /// <summary>
    /// Restores a soft-deleted UOM asynchronously
    /// </summary>
    /// <param name="id">UOM ID</param>
    /// <param name="restoredBy">User ID performing the restoration</param>
    public async Task RestoreAsync(long id, int restoredBy)
    {
        var uom = await _dbSet.FindAsync(id);
        if (uom != null && uom.DeletedAt.HasValue)
        {
            uom.DeletedAt = null;
            uom.DeletedBy = null;
            uom.IsActive = true;
            uom.Status = "Active";
            uom.UpdatedBy = restoredBy;
            uom.UpdatedAt = DateTime.UtcNow;
            
            await UpdateAsync(uom);
        }
    }

    /// <summary>
    /// Gets UOMs with pagination support asynchronously
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="searchTerm">Optional search term</param>
    /// <param name="includeInactive">Include inactive UOMs</param>
    public async Task<(IEnumerable<UnitOfMeasurement> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        string? searchTerm = null, 
        bool includeInactive = false)
    {
        var query = _dbSet.Where(u => u.DeletedAt == null);

        // Filter by active status if needed
        if (!includeInactive)
        {
            query = query.Where(u => u.IsActive);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u => 
                u.Name.Contains(searchTerm) ||
                u.Abbreviation != null && u.Abbreviation.Contains(searchTerm) ||
                u.CategoryTitle != null && u.CategoryTitle.Contains(searchTerm) ||
                u.Type.Contains(searchTerm));
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Get paged results
        var items = await query
            .Include(u => u.BaseUom)
            .Include(u => u.Creator)
            .OrderBy(u => u.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Override GetAllAsync to include related entities
    /// </summary>
    public override async Task<IEnumerable<UnitOfMeasurement>> GetAllAsync()
    {
        return await _dbSet
            .Where(u => u.DeletedAt == null)
            .Include(u => u.BaseUom)
            .Include(u => u.Creator)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Override GetByIdAsync to include related entities
    /// </summary>
    public override async Task<UnitOfMeasurement?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Where(u => u.Id == id && u.DeletedAt == null)
            .Include(u => u.BaseUom)
            .Include(u => u.Creator)
            .Include(u => u.Updater)
            .Include(u => u.DerivedUnits.Where(d => d.DeletedAt == null))
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Override AddAsync to set creation timestamp
    /// </summary>
    public override async Task<UnitOfMeasurement> AddAsync(UnitOfMeasurement entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.Status = "Active";
        entity.IsActive = true;
        
        return await base.AddAsync(entity);
    }

    /// <summary>
    /// Override UpdateAsync to set update timestamp
    /// </summary>
    public override async Task UpdateAsync(UnitOfMeasurement entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await base.UpdateAsync(entity);
    }

    /// <summary>
    /// Gets a UOM by its identifier asynchronously (long version)
    /// </summary>
    /// <param name="id">UOM identifier</param>
    public async Task<UnitOfMeasurement?> GetByIdAsync(long id)
    {
        return await _dbSet
            .Include(u => u.BaseUom)
            .Include(u => u.Creator)
            .Include(u => u.Updater)
            .Include(u => u.Deleter)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
    
    /// <summary>
    /// Checks if a UOM exists asynchronously (long version)
    /// </summary>
    /// <param name="id">UOM identifier</param>
    public async Task<bool> ExistsAsync(long id)
    {
        return await _dbSet.AnyAsync(u => u.Id == id);
    }
    
    /// <summary>
    /// Deletes a UOM asynchronously (long version)
    /// </summary>
    /// <param name="id">UOM identifier</param>
    public async Task DeleteAsync(long id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }
}