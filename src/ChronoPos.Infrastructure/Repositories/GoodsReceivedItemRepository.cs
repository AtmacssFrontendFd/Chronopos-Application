using Microsoft.EntityFrameworkCore;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for GoodsReceivedItem entity
/// </summary>
public class GoodsReceivedItemRepository : Repository<GoodsReceivedItem>, IGoodsReceivedItemRepository
{
    public GoodsReceivedItemRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public override async Task<GoodsReceivedItem?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(gri => gri.GoodsReceived)
            .Include(gri => gri.Product)
            .Include(gri => gri.ProductBatch)
            .Include(gri => gri.UnitOfMeasurement)
            .FirstOrDefaultAsync(gri => gri.Id == id);
    }

    public override async Task<IEnumerable<GoodsReceivedItem>> GetAllAsync()
    {
        return await _dbSet
            .Include(gri => gri.GoodsReceived)
            .Include(gri => gri.Product)
            .Include(gri => gri.ProductBatch)
            .Include(gri => gri.UnitOfMeasurement)
            .OrderByDescending(gri => gri.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReceivedItem>> GetByGrnIdAsync(int grnId)
    {
        return await _dbSet
            .Include(gri => gri.Product)
            .Include(gri => gri.ProductBatch)
            .Include(gri => gri.UnitOfMeasurement)
            .Where(gri => gri.GrnId == grnId)
            .OrderBy(gri => gri.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReceivedItem>> GetByProductIdAsync(int productId)
    {
        return await _dbSet
            .Include(gri => gri.GoodsReceived)
            .Include(gri => gri.ProductBatch)
            .Include(gri => gri.UnitOfMeasurement)
            .Where(gri => gri.ProductId == productId)
            .OrderByDescending(gri => gri.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReceivedItem>> GetByBatchIdAsync(int batchId)
    {
        return await _dbSet
            .Include(gri => gri.GoodsReceived)
            .Include(gri => gri.Product)
            .Include(gri => gri.UnitOfMeasurement)
            .Where(gri => gri.BatchId == batchId)
            .OrderByDescending(gri => gri.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReceivedItem>> SearchAsync(string searchTerm)
    {
        var term = searchTerm.ToLower();
        return await _dbSet
            .Include(gri => gri.GoodsReceived)
            .Include(gri => gri.Product)
            .Include(gri => gri.ProductBatch)
            .Include(gri => gri.UnitOfMeasurement)
            .Where(gri => 
                (gri.Product != null && gri.Product.Name.ToLower().Contains(term)) ||
                (gri.Product != null && gri.Product.Code.ToLower().Contains(term)) ||
                (gri.BatchNo != null && gri.BatchNo.ToLower().Contains(term)) ||
                (gri.GoodsReceived != null && gri.GoodsReceived.GrnNo.ToLower().Contains(term)))
            .OrderByDescending(gri => gri.CreatedAt)
            .ToListAsync();
    }

    public override async Task<GoodsReceivedItem> AddAsync(GoodsReceivedItem entity)
    {
        var result = await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    public override async Task<GoodsReceivedItem> UpdateAsync(GoodsReceivedItem item)
    {
        _context.Entry(item).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return item;
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        var item = await _dbSet.FindAsync(id);
        if (item == null)
            return false;

        _dbSet.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteByGrnIdAsync(int grnId)
    {
        var items = await _dbSet.Where(gri => gri.GrnId == grnId).ToListAsync();
        if (!items.Any())
            return false;

        _dbSet.RemoveRange(items);
        await _context.SaveChangesAsync();
        return true;
    }

    public override async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(gri => gri.Id == id);
    }

    public async Task<decimal> GetTotalQuantityByProductIdAsync(int productId)
    {
        return await _dbSet
            .Where(gri => gri.ProductId == productId)
            .SumAsync(gri => gri.Quantity);
    }

    public async Task<decimal> GetTotalAmountByGrnIdAsync(int grnId)
    {
        return await _dbSet
            .Where(gri => gri.GrnId == grnId)
            .SumAsync(gri => gri.LineTotal);
    }

    public async Task<int> GetCountByGrnIdAsync(int grnId)
    {
        return await _dbSet.CountAsync(gri => gri.GrnId == grnId);
    }

    public async Task<IEnumerable<GoodsReceivedItem>> GetPagedAsync(int skip, int take, string? searchTerm = null)
    {
        var query = _dbSet
            .Include(gri => gri.GoodsReceived)
            .Include(gri => gri.Product)
            .Include(gri => gri.ProductBatch)
            .Include(gri => gri.UnitOfMeasurement)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(gri => 
                (gri.Product != null && gri.Product.Name.ToLower().Contains(term)) ||
                (gri.Product != null && gri.Product.Code.ToLower().Contains(term)) ||
                (gri.BatchNo != null && gri.BatchNo.ToLower().Contains(term)) ||
                (gri.GoodsReceived != null && gri.GoodsReceived.GrnNo.ToLower().Contains(term)));
        }

        return await query
            .OrderByDescending(gri => gri.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
}