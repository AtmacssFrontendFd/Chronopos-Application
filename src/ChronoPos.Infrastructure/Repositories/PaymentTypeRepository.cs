using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for PaymentType entity
/// </summary>
public class PaymentTypeRepository : Repository<PaymentType>, IPaymentTypeRepository
{
    public PaymentTypeRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all active payment types
    /// </summary>
    public async Task<IEnumerable<PaymentType>> GetActiveAsync()
    {
        return await _dbSet
            .Where(x => x.Status && x.DeletedAt == null)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a payment type by name
    /// </summary>
    /// <param name="name">The name to search for</param>
    public async Task<PaymentType?> GetByNameAsync(string name)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.Name == name && x.DeletedAt == null);
    }

    /// <summary>
    /// Gets a payment type by payment code
    /// </summary>
    /// <param name="paymentCode">The payment code to search for</param>
    public async Task<PaymentType?> GetByPaymentCodeAsync(string paymentCode)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.PaymentCode == paymentCode && x.DeletedAt == null);
    }

    /// <summary>
    /// Checks if a payment type name already exists
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <param name="excludeId">ID to exclude from the check (for updates)</param>
    public async Task<bool> ExistsAsync(string name, int? excludeId = null)
    {
        var query = _dbSet.Where(x => x.Name == name && x.DeletedAt == null);
        
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    /// <summary>
    /// Checks if a payment code already exists
    /// </summary>
    /// <param name="paymentCode">The payment code to check</param>
    /// <param name="excludeId">ID to exclude from the check (for updates)</param>
    public async Task<bool> PaymentCodeExistsAsync(string paymentCode, int? excludeId = null)
    {
        var query = _dbSet.Where(x => x.PaymentCode == paymentCode && x.DeletedAt == null);
        
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }

    /// <summary>
    /// Soft delete a payment type
    /// </summary>
    /// <param name="id">ID of the payment type to delete</param>
    /// <param name="deletedBy">ID of the user performing the deletion</param>
    public async Task SoftDeleteAsync(int id, int deletedBy)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            entity.DeletedBy = deletedBy;
            entity.DeletedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }
    }

    /// <summary>
    /// Override GetAllAsync to exclude soft deleted items
    /// </summary>
    public override async Task<IEnumerable<PaymentType>> GetAllAsync()
    {
        return await _dbSet
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Override GetByIdAsync to exclude soft deleted items
    /// </summary>
    public override async Task<PaymentType?> GetByIdAsync(int id)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
    }
}