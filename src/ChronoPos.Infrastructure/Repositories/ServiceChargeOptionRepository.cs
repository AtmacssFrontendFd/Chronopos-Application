using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ServiceChargeOption entity operations
/// </summary>
public class ServiceChargeOptionRepository : Repository<ServiceChargeOption>, IServiceChargeOptionRepository
{
    public ServiceChargeOptionRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets service charge options by service charge type ID
    /// </summary>
    /// <param name="serviceChargeTypeId">Service charge type ID</param>
    /// <returns>Collection of service charge options</returns>
    public async Task<IEnumerable<ServiceChargeOption>> GetByServiceChargeTypeIdAsync(int serviceChargeTypeId)
    {
        return await _context.Set<ServiceChargeOption>()
            .Where(o => o.ServiceChargeTypeId == serviceChargeTypeId && o.DeletedAt == null)
            .OrderBy(o => o.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets active service charge options by service charge type ID
    /// </summary>
    /// <param name="serviceChargeTypeId">Service charge type ID</param>
    /// <returns>Collection of active service charge options</returns>
    public async Task<IEnumerable<ServiceChargeOption>> GetActiveByServiceChargeTypeIdAsync(int serviceChargeTypeId)
    {
        return await _context.Set<ServiceChargeOption>()
            .Where(o => o.ServiceChargeTypeId == serviceChargeTypeId && o.Status && o.DeletedAt == null)
            .OrderBy(o => o.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets service charge option by name and type ID
    /// </summary>
    /// <param name="name">Option name</param>
    /// <param name="serviceChargeTypeId">Service charge type ID</param>
    /// <returns>ServiceChargeOption entity if found</returns>
    public async Task<ServiceChargeOption?> GetByNameAndTypeAsync(string name, int serviceChargeTypeId)
    {
        return await _context.Set<ServiceChargeOption>()
            .FirstOrDefaultAsync(o => o.Name.ToLower() == name.ToLower() 
                                      && o.ServiceChargeTypeId == serviceChargeTypeId 
                                      && o.DeletedAt == null);
    }

    /// <summary>
    /// Gets service charge options by language ID
    /// </summary>
    /// <param name="languageId">Language ID</param>
    /// <returns>Collection of service charge options</returns>
    public async Task<IEnumerable<ServiceChargeOption>> GetByLanguageIdAsync(int languageId)
    {
        return await _context.Set<ServiceChargeOption>()
            .Where(o => o.LanguageId == languageId && o.DeletedAt == null)
            .OrderBy(o => o.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all active service charge options
    /// </summary>
    /// <returns>Collection of active service charge options</returns>
    public async Task<IEnumerable<ServiceChargeOption>> GetActiveOptionsAsync()
    {
        return await _context.Set<ServiceChargeOption>()
            .Where(o => o.Status && o.DeletedAt == null)
            .OrderBy(o => o.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets service charge option with related entities loaded
    /// </summary>
    /// <param name="id">Service charge option ID</param>
    /// <returns>ServiceChargeOption with related entities loaded</returns>
    public async Task<ServiceChargeOption?> GetByIdWithRelatedAsync(int id)
    {
        return await _context.Set<ServiceChargeOption>()
            .Include(o => o.ServiceChargeType)
            .Include(o => o.Language)
            .Include(o => o.CreatedByUser)
            .Include(o => o.UpdatedByUser)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    /// <summary>
    /// Checks if option name exists for a specific service charge type (case-insensitive)
    /// </summary>
    /// <param name="name">Option name to check</param>
    /// <param name="serviceChargeTypeId">Service charge type ID</param>
    /// <param name="excludeId">Option ID to exclude from check (for updates)</param>
    /// <returns>True if name exists</returns>
    public async Task<bool> NameExistsForTypeAsync(string name, int serviceChargeTypeId, int? excludeId = null)
    {
        var query = _context.Set<ServiceChargeOption>()
            .Where(o => o.Name.ToLower() == name.ToLower() 
                        && o.ServiceChargeTypeId == serviceChargeTypeId 
                        && o.DeletedAt == null);

        if (excludeId.HasValue)
        {
            query = query.Where(o => o.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Deletes all options for a specific service charge type
    /// </summary>
    /// <param name="serviceChargeTypeId">Service charge type ID</param>
    /// <returns>Number of deleted options</returns>
    public async Task<int> DeleteByServiceChargeTypeIdAsync(int serviceChargeTypeId)
    {
        var options = await _context.Set<ServiceChargeOption>()
            .Where(o => o.ServiceChargeTypeId == serviceChargeTypeId)
            .ToListAsync();

        _context.Set<ServiceChargeOption>().RemoveRange(options);
        return options.Count;
    }

    /// <summary>
    /// Gets all service charge options including related data
    /// </summary>
    /// <returns>Collection of service charge options with navigation properties</returns>
    public override async Task<IEnumerable<ServiceChargeOption>> GetAllAsync()
    {
        return await _context.Set<ServiceChargeOption>()
            .Include(o => o.ServiceChargeType)
            .Include(o => o.Language)
            .Where(o => o.DeletedAt == null)
            .OrderBy(o => o.ServiceChargeTypeId)
            .ThenBy(o => o.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets service charge option by ID including related data
    /// </summary>
    /// <param name="id">Service charge option ID</param>
    /// <returns>ServiceChargeOption with navigation properties</returns>
    public override async Task<ServiceChargeOption?> GetByIdAsync(int id)
    {
        return await _context.Set<ServiceChargeOption>()
            .Include(o => o.ServiceChargeType)
            .Include(o => o.Language)
            .Include(o => o.CreatedByUser)
            .Include(o => o.UpdatedByUser)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}
