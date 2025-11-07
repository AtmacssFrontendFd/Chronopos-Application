using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ServiceChargeType entity operations
/// </summary>
public class ServiceChargeTypeRepository : Repository<ServiceChargeType>, IServiceChargeTypeRepository
{
    public ServiceChargeTypeRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets service charge type by code
    /// </summary>
    /// <param name="code">Service charge type code</param>
    /// <returns>ServiceChargeType entity if found</returns>
    public async Task<ServiceChargeType?> GetByCodeAsync(string code)
    {
        return await _context.Set<ServiceChargeType>()
            .FirstOrDefaultAsync(t => t.Code.ToLower() == code.ToLower());
    }

    /// <summary>
    /// Gets service charge type by name
    /// </summary>
    /// <param name="name">Service charge type name</param>
    /// <returns>ServiceChargeType entity if found</returns>
    public async Task<ServiceChargeType?> GetByNameAsync(string name)
    {
        return await _context.Set<ServiceChargeType>()
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
    }

    /// <summary>
    /// Gets all active service charge types
    /// </summary>
    /// <returns>Collection of active service charge types</returns>
    public async Task<IEnumerable<ServiceChargeType>> GetActiveTypesAsync()
    {
        return await _context.Set<ServiceChargeType>()
            .Where(t => t.Status && t.DeletedAt == null)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the default service charge type
    /// </summary>
    /// <returns>Default service charge type if found</returns>
    public async Task<ServiceChargeType?> GetDefaultTypeAsync()
    {
        return await _context.Set<ServiceChargeType>()
            .FirstOrDefaultAsync(t => t.IsDefault && t.Status && t.DeletedAt == null);
    }

    /// <summary>
    /// Gets service charge types with their options
    /// </summary>
    /// <returns>Collection of service charge types with options loaded</returns>
    public async Task<IEnumerable<ServiceChargeType>> GetTypesWithOptionsAsync()
    {
        return await _context.Set<ServiceChargeType>()
            .Include(t => t.ServiceChargeOptions.Where(o => o.DeletedAt == null))
            .Where(t => t.DeletedAt == null)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets service charge type with its options by ID
    /// </summary>
    /// <param name="id">Service charge type ID</param>
    /// <returns>ServiceChargeType with options loaded</returns>
    public async Task<ServiceChargeType?> GetByIdWithOptionsAsync(int id)
    {
        return await _context.Set<ServiceChargeType>()
            .Include(t => t.ServiceChargeOptions.Where(o => o.DeletedAt == null))
            .Include(t => t.CreatedByUser)
            .Include(t => t.UpdatedByUser)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <summary>
    /// Checks if service charge type code exists (case-insensitive)
    /// </summary>
    /// <param name="code">Code to check</param>
    /// <param name="excludeId">Service charge type ID to exclude from check (for updates)</param>
    /// <returns>True if code exists</returns>
    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        var query = _context.Set<ServiceChargeType>()
            .Where(t => t.Code.ToLower() == code.ToLower() && t.DeletedAt == null);

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Checks if service charge type name exists (case-insensitive)
    /// </summary>
    /// <param name="name">Name to check</param>
    /// <param name="excludeId">Service charge type ID to exclude from check (for updates)</param>
    /// <returns>True if name exists</returns>
    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.Set<ServiceChargeType>()
            .Where(t => t.Name.ToLower() == name.ToLower() && t.DeletedAt == null);

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Gets service charge types with their options count
    /// </summary>
    /// <returns>Collection of service charge types with options counts</returns>
    public async Task<IEnumerable<ServiceChargeType>> GetTypesWithOptionsCountAsync()
    {
        return await _context.Set<ServiceChargeType>()
            .Include(t => t.ServiceChargeOptions.Where(o => o.DeletedAt == null))
            .Where(t => t.DeletedAt == null)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all service charge types including related data
    /// </summary>
    /// <returns>Collection of service charge types with navigation properties</returns>
    public override async Task<IEnumerable<ServiceChargeType>> GetAllAsync()
    {
        return await _context.Set<ServiceChargeType>()
            .Include(t => t.ServiceChargeOptions.Where(o => o.DeletedAt == null))
            .Where(t => t.DeletedAt == null)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets service charge type by ID including related data
    /// </summary>
    /// <param name="id">Service charge type ID</param>
    /// <returns>ServiceChargeType with navigation properties</returns>
    public override async Task<ServiceChargeType?> GetByIdAsync(int id)
    {
        return await _context.Set<ServiceChargeType>()
            .Include(t => t.ServiceChargeOptions.Where(o => o.DeletedAt == null))
            .Include(t => t.CreatedByUser)
            .Include(t => t.UpdatedByUser)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}
