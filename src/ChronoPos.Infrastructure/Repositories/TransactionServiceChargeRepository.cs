using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for TransactionServiceCharge entity operations
/// </summary>
public class TransactionServiceChargeRepository : Repository<TransactionServiceCharge>, ITransactionServiceChargeRepository
{
    public TransactionServiceChargeRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransactionServiceCharge>> GetByTransactionIdAsync(int transactionId)
    {
        return await _context.Set<TransactionServiceCharge>()
            .Where(tsc => tsc.TransactionId == transactionId)
            .Include(tsc => tsc.ServiceChargeOption)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransactionServiceCharge>> GetByServiceChargeIdAsync(int serviceChargeId)
    {
        return await _context.Set<TransactionServiceCharge>()
            .Where(tsc => tsc.ServiceChargeOptionId == serviceChargeId)
            .Include(tsc => tsc.Transaction)
            .ToListAsync();
    }
    
    public void Update(TransactionServiceCharge transactionServiceCharge)
    {
        _context.Set<TransactionServiceCharge>().Update(transactionServiceCharge);
    }
    
    public void Delete(TransactionServiceCharge transactionServiceCharge)
    {
        _context.Set<TransactionServiceCharge>().Remove(transactionServiceCharge);
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
