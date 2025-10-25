using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Transaction entity operations
/// </summary>
public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public override async Task<Transaction?> GetByIdAsync(int id)
    {
        return await _context.Set<Transaction>()
            .Include(t => t.Shift)
            .Include(t => t.Customer)
            .Include(t => t.User)
            .Include(t => t.ShopLocation)
            .Include(t => t.Table)
            .Include(t => t.Reservation)
            .Include(t => t.TransactionProducts)
                .ThenInclude(tp => tp.Product)
            .Include(t => t.TransactionProducts)
                .ThenInclude(tp => tp.TransactionModifiers)
                    .ThenInclude(tm => tm.ProductModifier)
            .Include(t => t.TransactionServiceCharges)
                .ThenInclude(tsc => tsc.ServiceCharge)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Transaction>> GetByShiftIdAsync(int shiftId)
    {
        return await _context.Set<Transaction>()
            .Where(t => t.ShiftId == shiftId)
            .Include(t => t.Customer)
            .Include(t => t.User)
            .OrderByDescending(t => t.SellingTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Set<Transaction>()
            .Where(t => t.CustomerId == customerId)
            .Include(t => t.Shift)
            .Include(t => t.User)
            .OrderByDescending(t => t.SellingTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByStatusAsync(string status)
    {
        return await _context.Set<Transaction>()
            .Where(t => t.Status == status)
            .Include(t => t.Customer)
            .Include(t => t.User)
            .OrderByDescending(t => t.SellingTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<Transaction>()
            .Where(t => t.SellingTime >= startDate && t.SellingTime <= endDate)
            .Include(t => t.Customer)
            .Include(t => t.User)
            .OrderByDescending(t => t.SellingTime)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        return await _context.Set<Transaction>()
            .Include(t => t.Customer)
            .Include(t => t.User)
            .Include(t => t.TransactionProducts)
                .ThenInclude(tp => tp.Product)
            .FirstOrDefaultAsync(t => t.InvoiceNumber == invoiceNumber);
    }

    public async Task<IEnumerable<Transaction>> GetByTableIdAsync(int tableId)
    {
        return await _context.Set<Transaction>()
            .Where(t => t.TableId == tableId)
            .Include(t => t.Customer)
            .Include(t => t.User)
            .OrderByDescending(t => t.SellingTime)
            .ToListAsync();
    }

    public async Task<Transaction?> GetWithDetailsAsync(int id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<IEnumerable<Transaction>> GetTodaysTransactionsAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        
        return await _context.Set<Transaction>()
            .Where(t => t.SellingTime >= today && t.SellingTime < tomorrow)
            .Include(t => t.Customer)
            .Include(t => t.User)
            .OrderByDescending(t => t.SellingTime)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Transaction>> GetAllWithDetailsAsync()
    {
        return await _context.Set<Transaction>()
            .Include(t => t.Shift)
            .Include(t => t.Customer)
            .Include(t => t.User)
            .Include(t => t.ShopLocation)
            .Include(t => t.Table)
            .Include(t => t.Reservation)
            .Include(t => t.TransactionProducts)
                .ThenInclude(tp => tp.Product)
            .Include(t => t.TransactionProducts)
                .ThenInclude(tp => tp.TransactionModifiers)
                    .ThenInclude(tm => tm.ProductModifier)
            .Include(t => t.TransactionServiceCharges)
                .ThenInclude(tsc => tsc.ServiceCharge)
            .OrderByDescending(t => t.SellingTime)
            .ToListAsync();
    }
    
    public async Task<Transaction?> GetByIdWithDetailsAsync(int id)
    {
        return await GetByIdAsync(id);
    }
    
    public void Update(Transaction transaction)
    {
        _context.Set<Transaction>().Update(transaction);
    }
    
    public void Delete(Transaction transaction)
    {
        _context.Set<Transaction>().Remove(transaction);
    }
}
