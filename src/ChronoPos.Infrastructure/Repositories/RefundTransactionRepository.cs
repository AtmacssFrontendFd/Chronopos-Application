using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for RefundTransaction entity operations
/// </summary>
public class RefundTransactionRepository : Repository<RefundTransaction>, IRefundTransactionRepository
{
    public RefundTransactionRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<RefundTransaction>> GetBySellingTransactionIdAsync(int sellingTransactionId)
    {
        return await _context.Set<RefundTransaction>()
            .Where(rt => rt.SellingTransactionId == sellingTransactionId)
            .Include(rt => rt.Customer)
            .Include(rt => rt.User)
            .OrderByDescending(rt => rt.RefundTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<RefundTransaction>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Set<RefundTransaction>()
            .Where(rt => rt.CustomerId == customerId)
            .Include(rt => rt.SellingTransaction)
            .Include(rt => rt.User)
            .OrderByDescending(rt => rt.RefundTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<RefundTransaction>> GetByShiftIdAsync(int shiftId)
    {
        return await _context.Set<RefundTransaction>()
            .Where(rt => rt.ShiftId == shiftId)
            .Include(rt => rt.Customer)
            .Include(rt => rt.User)
            .OrderByDescending(rt => rt.RefundTime)
            .ToListAsync();
    }

    public async Task<RefundTransaction?> GetWithDetailsAsync(int id)
    {
        return await _context.Set<RefundTransaction>()
            .Include(rt => rt.Customer)
            .Include(rt => rt.SellingTransaction)
            .Include(rt => rt.Shift)
            .Include(rt => rt.User)
            .Include(rt => rt.RefundTransactionProducts)
                .ThenInclude(rtp => rtp.TransactionProduct)
                    .ThenInclude(tp => tp.Product)
            .FirstOrDefaultAsync(rt => rt.Id == id);
    }

    public async Task<IEnumerable<RefundTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<RefundTransaction>()
            .Where(rt => rt.RefundTime >= startDate && rt.RefundTime <= endDate)
            .Include(rt => rt.Customer)
            .Include(rt => rt.User)
            .OrderByDescending(rt => rt.RefundTime)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<RefundTransaction>> GetByTransactionIdAsync(int transactionId)
    {
        return await GetBySellingTransactionIdAsync(transactionId);
    }
    
    public async Task<IEnumerable<RefundTransaction>> GetAllWithDetailsAsync()
    {
        return await _context.Set<RefundTransaction>()
            .Include(rt => rt.Customer)
            .Include(rt => rt.SellingTransaction)
            .Include(rt => rt.Shift)
            .Include(rt => rt.User)
            .Include(rt => rt.RefundTransactionProducts)
                .ThenInclude(rtp => rtp.TransactionProduct)
                    .ThenInclude(tp => tp.Product)
            .OrderByDescending(rt => rt.RefundTime)
            .ToListAsync();
    }
    
    public async Task<RefundTransaction?> GetByIdWithDetailsAsync(int id)
    {
        return await GetWithDetailsAsync(id);
    }
    
    public void Update(RefundTransaction refundTransaction)
    {
        _context.Set<RefundTransaction>().Update(refundTransaction);
    }
    
    public void Delete(RefundTransaction refundTransaction)
    {
        _context.Set<RefundTransaction>().Remove(refundTransaction);
    }
}
