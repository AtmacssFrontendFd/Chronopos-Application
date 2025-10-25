using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ExchangeTransactionProduct entity operations
/// </summary>
public class ExchangeTransactionProductRepository : Repository<ExchangeTransactionProduct>, IExchangeTransactionProductRepository
{
    public ExchangeTransactionProductRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ExchangeTransactionProduct>> GetByExchangeTransactionIdAsync(int exchangeTransactionId)
    {
        return await _context.Set<ExchangeTransactionProduct>()
            .Where(etp => etp.ExchangeTransactionId == exchangeTransactionId)
            .Include(etp => etp.OriginalTransactionProduct)
                .ThenInclude(tp => tp.Product)
            .Include(etp => etp.NewProduct)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExchangeTransactionProduct>> GetByOriginalTransactionProductIdAsync(int originalTransactionProductId)
    {
        return await _context.Set<ExchangeTransactionProduct>()
            .Where(etp => etp.OriginalTransactionProductId == originalTransactionProductId)
            .Include(etp => etp.ExchangeTransaction)
            .Include(etp => etp.NewProduct)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExchangeTransactionProduct>> GetByNewProductIdAsync(int newProductId)
    {
        return await _context.Set<ExchangeTransactionProduct>()
            .Where(etp => etp.NewProductId == newProductId)
            .Include(etp => etp.ExchangeTransaction)
            .Include(etp => etp.OriginalTransactionProduct)
                .ThenInclude(tp => tp.Product)
            .ToListAsync();
    }
    
    public void Update(ExchangeTransactionProduct exchangeTransactionProduct)
    {
        _context.Set<ExchangeTransactionProduct>().Update(exchangeTransactionProduct);
    }
    
    public void Delete(ExchangeTransactionProduct exchangeTransactionProduct)
    {
        _context.Set<ExchangeTransactionProduct>().Remove(exchangeTransactionProduct);
    }
}
