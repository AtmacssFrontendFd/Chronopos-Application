using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for TransactionProduct entity operations
/// </summary>
public class TransactionProductRepository : Repository<TransactionProduct>, ITransactionProductRepository
{
    public TransactionProductRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransactionProduct>> GetByTransactionIdAsync(int transactionId)
    {
        return await _context.Set<TransactionProduct>()
            .Where(tp => tp.TransactionId == transactionId)
            .Include(tp => tp.Product)
            .Include(tp => tp.ProductUnit)
            .Include(tp => tp.TransactionModifiers)
                .ThenInclude(tm => tm.ProductModifier)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransactionProduct>> GetByProductIdAsync(int productId)
    {
        return await _context.Set<TransactionProduct>()
            .Where(tp => tp.ProductId == productId)
            .Include(tp => tp.Transaction)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransactionProduct>> GetByStatusAsync(string status)
    {
        return await _context.Set<TransactionProduct>()
            .Where(tp => tp.Status == status)
            .Include(tp => tp.Product)
            .Include(tp => tp.Transaction)
            .ToListAsync();
    }

    public async Task<TransactionProduct?> GetWithModifiersAsync(int id)
    {
        return await _context.Set<TransactionProduct>()
            .Include(tp => tp.Product)
            .Include(tp => tp.ProductUnit)
            .Include(tp => tp.TransactionModifiers)
                .ThenInclude(tm => tm.ProductModifier)
            .FirstOrDefaultAsync(tp => tp.Id == id);
    }
    
    public async Task<IEnumerable<TransactionProduct>> GetAllWithDetailsAsync()
    {
        return await _context.Set<TransactionProduct>()
            .Include(tp => tp.Product)
            .Include(tp => tp.ProductUnit)
            .Include(tp => tp.Transaction)
            .Include(tp => tp.TransactionModifiers)
                .ThenInclude(tm => tm.ProductModifier)
            .ToListAsync();
    }
    
    public void Update(TransactionProduct transactionProduct)
    {
        _context.Set<TransactionProduct>().Update(transactionProduct);
    }
    
    public void Delete(TransactionProduct transactionProduct)
    {
        _context.Set<TransactionProduct>().Remove(transactionProduct);
    }
}
