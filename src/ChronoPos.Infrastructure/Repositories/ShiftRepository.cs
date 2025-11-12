using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Shift entity operations
/// </summary>
public class ShiftRepository : Repository<Shift>, IShiftRepository
{
    public ShiftRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Shift>> GetByUserIdAsync(int userId)
    {
        return await _context.Set<Shift>()
            .Where(s => s.UserId == userId)
            .Include(s => s.User)
            .Include(s => s.ShopLocation)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Shift>> GetByShopLocationIdAsync(int shopLocationId)
    {
        return await _context.Set<Shift>()
            .Where(s => s.ShopLocationId == shopLocationId)
            .Include(s => s.User)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Shift>> GetByStatusAsync(string status)
    {
        return await _context.Set<Shift>()
            .Where(s => s.Status == status)
            .Include(s => s.User)
            .Include(s => s.ShopLocation)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<Shift?> GetCurrentOpenShiftAsync(int userId)
    {
        return await _context.Set<Shift>()
            .Where(s => s.UserId == userId && s.Status == "Open")
            .Include(s => s.User)
            .Include(s => s.ShopLocation)
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Shift>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<Shift>()
            .Where(s => s.StartTime >= startDate && s.StartTime <= endDate)
            .Include(s => s.User)
            .Include(s => s.ShopLocation)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<Shift?> GetWithTransactionsAsync(int shiftId)
    {
        return await _context.Set<Shift>()
            .Include(s => s.User)
            .Include(s => s.ShopLocation)
            .Include(s => s.Transactions)
                .ThenInclude(t => t.Customer)
            .Include(s => s.RefundTransactions)
            .Include(s => s.ExchangeTransactions)
            .FirstOrDefaultAsync(s => s.ShiftId == shiftId);
    }
    
    public async Task<Shift?> GetActiveShiftForUserAsync(int userId)
    {
        return await GetCurrentOpenShiftAsync(userId);
    }
    
    public void Update(Shift shift)
    {
        _context.Set<Shift>().Update(shift);
    }
    
    public void Delete(Shift shift)
    {
        _context.Set<Shift>().Remove(shift);
    }
}
