using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Sale entities
/// </summary>
public class SaleRepository : Repository<Sale>, ISaleRepository
{
    public SaleRepository(ChronoPosDbContext context) : base(context)
    {
    }
    
    public override async Task<IEnumerable<Sale>> GetAllAsync()
    {
        return await _dbSet
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }
    
    public override async Task<Sale?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
    
    public async Task<IEnumerable<Sale>> GetSalesByCustomerAsync(int customerId)
    {
        return await _dbSet
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Sale>> GetTodaySalesAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        
        return await _dbSet
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow)
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }
    
    public async Task<decimal> GetTotalSalesAmountAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate && 
                       s.Status == Domain.Enums.SaleStatus.Settled)
            .SumAsync(s => s.TotalAmount);
    }
    
    public async Task<string> GenerateTransactionNumberAsync()
    {
        var today = DateTime.Today;
        var prefix = $"TXN{today:yyyyMMdd}";
        
        var lastTransaction = await _dbSet
            .Where(s => s.TransactionNumber.StartsWith(prefix))
            .OrderByDescending(s => s.TransactionNumber)
            .FirstOrDefaultAsync();
            
        if (lastTransaction == null)
        {
            return $"{prefix}001";
        }
        
        var lastNumber = lastTransaction.TransactionNumber.Substring(prefix.Length);
        if (int.TryParse(lastNumber, out var number))
        {
            return $"{prefix}{(number + 1):D3}";
        }
        
        return $"{prefix}001";
    }
}
