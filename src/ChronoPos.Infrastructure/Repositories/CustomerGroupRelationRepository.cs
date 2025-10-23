using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for CustomerGroupRelation entity operations
/// </summary>
public class CustomerGroupRelationRepository : Repository<CustomerGroupRelation>, ICustomerGroupRelationRepository
{
    public CustomerGroupRelationRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all relations for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Collection of customer group relations</returns>
    public async Task<IEnumerable<CustomerGroupRelation>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Set<CustomerGroupRelation>()
            .Include(r => r.Customer)
            .Include(r => r.CustomerGroup)
            .Where(r => r.CustomerId == customerId && r.DeletedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all relations for a specific customer group
    /// </summary>
    /// <param name="customerGroupId">Customer group ID</param>
    /// <returns>Collection of customer group relations</returns>
    public async Task<IEnumerable<CustomerGroupRelation>> GetByCustomerGroupIdAsync(int customerGroupId)
    {
        return await _context.Set<CustomerGroupRelation>()
            .Include(r => r.Customer)
            .Include(r => r.CustomerGroup)
            .Where(r => r.CustomerGroupId == customerGroupId && r.DeletedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all active relations
    /// </summary>
    /// <returns>Collection of active customer group relations</returns>
    public async Task<IEnumerable<CustomerGroupRelation>> GetActiveRelationsAsync()
    {
        return await _context.Set<CustomerGroupRelation>()
            .Include(r => r.Customer)
            .Include(r => r.CustomerGroup)
            .Where(r => r.Status == "Active" && r.DeletedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a relation exists between customer and customer group
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="customerGroupId">Customer group ID</param>
    /// <param name="excludeId">Relation ID to exclude from check (for updates)</param>
    /// <returns>True if relation exists</returns>
    public async Task<bool> RelationExistsAsync(int customerId, int customerGroupId, int? excludeId = null)
    {
        var query = _context.Set<CustomerGroupRelation>()
            .Where(r => r.CustomerId == customerId && 
                       r.CustomerGroupId == customerGroupId && 
                       r.DeletedAt == null);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Gets relation by customer and customer group
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="customerGroupId">Customer group ID</param>
    /// <returns>CustomerGroupRelation if found</returns>
    public async Task<CustomerGroupRelation?> GetByCustomerAndGroupAsync(int customerId, int customerGroupId)
    {
        return await _context.Set<CustomerGroupRelation>()
            .Include(r => r.Customer)
            .Include(r => r.CustomerGroup)
            .FirstOrDefaultAsync(r => r.CustomerId == customerId && 
                                     r.CustomerGroupId == customerGroupId && 
                                     r.DeletedAt == null);
    }

    /// <summary>
    /// Gets all relations with customer and group details
    /// </summary>
    /// <returns>Collection of relations with navigation properties</returns>
    public async Task<IEnumerable<CustomerGroupRelation>> GetAllWithDetailsAsync()
    {
        return await _context.Set<CustomerGroupRelation>()
            .Include(r => r.Customer)
            .Include(r => r.CustomerGroup)
            .Where(r => r.DeletedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all relations including related data
    /// </summary>
    /// <returns>Collection of relations with navigation properties</returns>
    public override async Task<IEnumerable<CustomerGroupRelation>> GetAllAsync()
    {
        return await _context.Set<CustomerGroupRelation>()
            .Include(r => r.Customer)
            .Include(r => r.CustomerGroup)
            .Where(r => r.DeletedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets relation by ID including related data
    /// </summary>
    /// <param name="id">Relation ID</param>
    /// <returns>Relation with navigation properties</returns>
    public override async Task<CustomerGroupRelation?> GetByIdAsync(int id)
    {
        return await _context.Set<CustomerGroupRelation>()
            .Include(r => r.Customer)
            .Include(r => r.CustomerGroup)
            .FirstOrDefaultAsync(r => r.Id == id && r.DeletedAt == null);
    }
}
