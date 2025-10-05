using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for GoodsReplaceItem operations
/// </summary>
public class GoodsReplaceItemRepository : Repository<GoodsReplaceItem>, IGoodsReplaceItemRepository
{
    public GoodsReplaceItemRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GoodsReplaceItem>> GetByReplaceIdAsync(int replaceId)
    {
        return await _dbSet
            .Include(gri => gri.Product)
            .Include(gri => gri.Uom)
            .Include(gri => gri.Replace)
            .Where(gri => gri.ReplaceId == replaceId)
            .OrderBy(gri => gri.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReplaceItem>> GetByProductIdAsync(int productId)
    {
        return await _dbSet
            .Include(gri => gri.Product)
            .Include(gri => gri.Uom)
            .Include(gri => gri.Replace)
            .Where(gri => gri.ProductId == productId)
            .OrderByDescending(gri => gri.CreatedAt)
            .ToListAsync();
    }

    public async Task<GoodsReplaceItem?> GetWithDetailsAsync(int itemId)
    {
        return await _dbSet
            .Include(gri => gri.Product)
                .ThenInclude(p => p.Category)
            .Include(gri => gri.Uom)
            .Include(gri => gri.Replace)
                .ThenInclude(gr => gr.Supplier)
            .Include(gri => gri.Replace)
                .ThenInclude(gr => gr.Store)
            .FirstOrDefaultAsync(gri => gri.Id == itemId);
    }

    public override async Task<IEnumerable<GoodsReplaceItem>> GetAllAsync()
    {
        return await _dbSet
            .Include(gri => gri.Product)
            .Include(gri => gri.Uom)
            .Include(gri => gri.Replace)
            .OrderByDescending(gri => gri.CreatedAt)
            .ToListAsync();
    }

    public override async Task<GoodsReplaceItem?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(gri => gri.Product)
            .Include(gri => gri.Uom)
            .Include(gri => gri.Replace)
            .FirstOrDefaultAsync(gri => gri.Id == id);
    }
}
