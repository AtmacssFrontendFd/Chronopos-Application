using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ExchangeTransaction entity operations
/// </summary>
public class ExchangeTransactionRepository : Repository<ExchangeTransaction>, IExchangeTransactionRepository
{
    public ExchangeTransactionRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ExchangeTransaction>> GetBySellingTransactionIdAsync(int sellingTransactionId)
    {
        return await _context.Set<ExchangeTransaction>()
            .Where(et => et.SellingTransactionId == sellingTransactionId)
            .Include(et => et.Customer)
            .OrderByDescending(et => et.ExchangeTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExchangeTransaction>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Set<ExchangeTransaction>()
            .Where(et => et.CustomerId == customerId)
            .Include(et => et.SellingTransaction)
            .OrderByDescending(et => et.ExchangeTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExchangeTransaction>> GetByShiftIdAsync(int shiftId)
    {
        return await _context.Set<ExchangeTransaction>()
            .Where(et => et.ShiftId == shiftId)
            .Include(et => et.Customer)
            .OrderByDescending(et => et.ExchangeTime)
            .ToListAsync();
    }

    public async Task<ExchangeTransaction?> GetWithDetailsAsync(int id)
    {
        return await _context.Set<ExchangeTransaction>()
            .Include(et => et.Customer)
            .Include(et => et.SellingTransaction)
            .Include(et => et.Shift)
            .Include(et => et.ExchangeTransactionProducts)
                .ThenInclude(etp => etp.OriginalTransactionProduct)
                    .ThenInclude(tp => tp.Product)
            .Include(et => et.ExchangeTransactionProducts)
                .ThenInclude(etp => etp.NewProduct)
            .FirstOrDefaultAsync(et => et.Id == id);
    }

    public async Task<IEnumerable<ExchangeTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<ExchangeTransaction>()
            .Where(et => et.ExchangeTime >= startDate && et.ExchangeTime <= endDate)
            .Include(et => et.Customer)
            .OrderByDescending(et => et.ExchangeTime)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<ExchangeTransaction>> GetByTransactionIdAsync(int transactionId)
    {
        return await GetBySellingTransactionIdAsync(transactionId);
    }
    
    public async Task<IEnumerable<ExchangeTransaction>> GetAllWithDetailsAsync()
    {
        return await _context.Set<ExchangeTransaction>()
            .Include(et => et.Customer)
            .Include(et => et.SellingTransaction)
            .Include(et => et.Shift)
            .Include(et => et.ExchangeTransactionProducts)
                .ThenInclude(etp => etp.OriginalTransactionProduct)
                    .ThenInclude(tp => tp.Product)
            .Include(et => et.ExchangeTransactionProducts)
                .ThenInclude(etp => etp.NewProduct)
            .OrderByDescending(et => et.ExchangeTime)
            .ToListAsync();
    }
    
    public async Task<ExchangeTransaction?> GetByIdWithDetailsAsync(int id)
    {
        return await GetWithDetailsAsync(id);
    }
    
    public void Update(ExchangeTransaction exchangeTransaction)
    {
        _context.Set<ExchangeTransaction>().Update(exchangeTransaction);
    }
    
    public void Delete(ExchangeTransaction exchangeTransaction)
    {
        _context.Set<ExchangeTransaction>().Remove(exchangeTransaction);
    }
}
