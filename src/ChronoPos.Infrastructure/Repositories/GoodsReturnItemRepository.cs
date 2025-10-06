using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using ChronoPos.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for GoodsReturnItem operations
/// </summary>
public class GoodsReturnItemRepository : Repository<GoodsReturnItem>, IGoodsReturnItemRepository
{
    public GoodsReturnItemRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GoodsReturnItem>> GetByReturnIdAsync(int returnId)
    {
        return await _context.Set<GoodsReturnItem>()
            .Include(item => item.Product)
            .Include(item => item.Uom)
            .Include(item => item.Batch)
            .Where(item => item.ReturnId == returnId)
            .OrderBy(item => item.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReturnItem>> GetByProductIdAsync(int productId)
    {
        return await _context.Set<GoodsReturnItem>()
            .Include(item => item.Return)
                .ThenInclude(ret => ret.Supplier)
            .Include(item => item.Product)
            .Include(item => item.Uom)
            .Include(item => item.Batch)
            .Where(item => item.ProductId == productId)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReturnItem>> GetByBatchNoAsync(string batchNo)
    {
        return await _context.Set<GoodsReturnItem>()
            .Include(item => item.Return)
                .ThenInclude(ret => ret.Supplier)
            .Include(item => item.Product)
            .Include(item => item.Uom)
            .Include(item => item.Batch)
            .Where(item => item.BatchNo == batchNo)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReturnItem>> GetByReasonAsync(string reason)
    {
        return await _context.Set<GoodsReturnItem>()
            .Include(item => item.Return)
                .ThenInclude(ret => ret.Supplier)
            .Include(item => item.Product)
            .Include(item => item.Uom)
            .Include(item => item.Batch)
            .Where(item => item.Reason == reason)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();
    }

    public async Task DeleteByReturnIdAsync(int returnId)
    {
        var items = await _context.Set<GoodsReturnItem>()
            .Where(item => item.ReturnId == returnId)
            .ToListAsync();

        if (items.Any())
        {
            _context.Set<GoodsReturnItem>().RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<GoodsReturnItem>> GetWithProductDetailsByReturnIdAsync(int returnId)
    {
        return await _context.Set<GoodsReturnItem>()
            .Include(item => item.Product)
                .ThenInclude(p => p.Category)
            .Include(item => item.Product)
                .ThenInclude(p => p.Brand)
            .Include(item => item.Uom)
            .Include(item => item.Batch)
            .Where(item => item.ReturnId == returnId)
            .OrderBy(item => item.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a goods return item by ID with related entities
    /// </summary>
    public async Task<GoodsReturnItem?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Set<GoodsReturnItem>()
            .Include(x => x.Product)
            .Include(x => x.Batch)
            .Include(x => x.Uom)
            .Include(x => x.Return)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// Gets all goods return items with pagination and related entities
    /// </summary>
    public async Task<(IEnumerable<GoodsReturnItem> Items, int TotalCount)> GetAllWithDetailsAsync(int page, int pageSize)
    {
        var query = _context.Set<GoodsReturnItem>()
            .Include(x => x.Product)
            .Include(x => x.Batch)
            .Include(x => x.Uom)
            .Include(x => x.Return);

        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
