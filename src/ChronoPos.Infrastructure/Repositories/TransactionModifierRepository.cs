using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for TransactionModifier entity operations
/// </summary>
public class TransactionModifierRepository : Repository<TransactionModifier>, ITransactionModifierRepository
{
    public TransactionModifierRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransactionModifier>> GetByTransactionProductIdAsync(int transactionProductId)
    {
        return await _context.Set<TransactionModifier>()
            .Where(tm => tm.TransactionProductId == transactionProductId)
            .Include(tm => tm.ProductModifier)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransactionModifier>> GetByProductModifierIdAsync(int productModifierId)
    {
        return await _context.Set<TransactionModifier>()
            .Where(tm => tm.ProductModifierId == productModifierId)
            .Include(tm => tm.TransactionProduct)
                .ThenInclude(tp => tp.Product)
            .ToListAsync();
    }
    
    public void Update(TransactionModifier transactionModifier)
    {
        _context.Set<TransactionModifier>().Update(transactionModifier);
    }
    
    public void Delete(TransactionModifier transactionModifier)
    {
        _context.Set<TransactionModifier>().Remove(transactionModifier);
    }
}
