using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Brand entity operations
/// </summary>
public class BrandRepository : Repository<Brand>, IBrandRepository
{
    public BrandRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets brand by name
    /// </summary>
    /// <param name="name">Brand name</param>
    /// <returns>Brand entity if found</returns>
    public async Task<Brand?> GetByNameAsync(string name)
    {
        return await _context.Set<Brand>()
            .FirstOrDefaultAsync(b => b.Name.ToLower() == name.ToLower());
    }

    /// <summary>
    /// Gets all active brands
    /// </summary>
    /// <returns>Collection of active brands</returns>
    public async Task<IEnumerable<Brand>> GetActiveBrandsAsync()
    {
        return await _context.Set<Brand>()
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if brand name exists (case-insensitive)
    /// </summary>
    /// <param name="name">Brand name to check</param>
    /// <param name="excludeId">Brand ID to exclude from check (for updates)</param>
    /// <returns>True if name exists</returns>
    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.Set<Brand>()
            .Where(b => b.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(b => b.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Gets brands with their product count
    /// </summary>
    /// <returns>Collection of brands with product counts</returns>
    public async Task<IEnumerable<Brand>> GetBrandsWithProductCountAsync()
    {
        return await _context.Set<Brand>()
            .Include(b => b.Products)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all brands including related data
    /// </summary>
    /// <returns>Collection of brands with navigation properties</returns>
    public override async Task<IEnumerable<Brand>> GetAllAsync()
    {
        return await _context.Set<Brand>()
            .Include(b => b.Products)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets brand by ID including related data
    /// </summary>
    /// <param name="id">Brand ID</param>
    /// <returns>Brand with navigation properties</returns>
    public override async Task<Brand?> GetByIdAsync(int id)
    {
        return await _context.Set<Brand>()
            .Include(b => b.Products)
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}
