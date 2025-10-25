using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for RefundTransactionProduct entity operations
/// </summary>
public class RefundTransactionProductRepository : Repository<RefundTransactionProduct>, IRefundTransactionProductRepository
{
    public RefundTransactionProductRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<RefundTransactionProduct>> GetByRefundTransactionIdAsync(int refundTransactionId)
    {
        return await _context.Set<RefundTransactionProduct>()
            .Where(rtp => rtp.RefundTransactionId == refundTransactionId)
            .Include(rtp => rtp.TransactionProduct)
                .ThenInclude(tp => tp.Product)
            .ToListAsync();
    }

    public async Task<IEnumerable<RefundTransactionProduct>> GetByTransactionProductIdAsync(int transactionProductId)
    {
        return await _context.Set<RefundTransactionProduct>()
            .Where(rtp => rtp.TransactionProductId == transactionProductId)
            .Include(rtp => rtp.RefundTransaction)
            .ToListAsync();
    }
    
    public void Update(RefundTransactionProduct refundTransactionProduct)
    {
        _context.Set<RefundTransactionProduct>().Update(refundTransactionProduct);
    }
    
    public void Delete(RefundTransactionProduct refundTransactionProduct)
    {
        _context.Set<RefundTransactionProduct>().Remove(refundTransactionProduct);
    }
}
