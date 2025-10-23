using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for GoodsReplace operations
/// </summary>
public class GoodsReplaceRepository : Repository<GoodsReplace>, IGoodsReplaceRepository
{
    public GoodsReplaceRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GoodsReplace>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceReturn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
                .ThenInclude(gri => gri.Product)
            .Where(gr => gr.Status == status)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReplace>> GetBySupplierAsync(int supplierId)
    {
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceReturn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
                .ThenInclude(gri => gri.Product)
            .Where(gr => gr.SupplierId == supplierId)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReplace>> GetByStoreAsync(int storeId)
    {
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceReturn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
                .ThenInclude(gri => gri.Product)
            .Where(gr => gr.StoreId == storeId)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReplace>> GetByReferenceReturnAsync(int returnId)
    {
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceReturn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
                .ThenInclude(gri => gri.Product)
            .Where(gr => gr.ReferenceReturnId == returnId)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReplace>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceReturn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
                .ThenInclude(gri => gri.Product)
            .Where(gr => gr.ReplaceDate >= startDate && gr.ReplaceDate <= endDate)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public async Task<string> GetNextReplaceNumberAsync()
    {
        var lastReplace = await _dbSet
            .OrderByDescending(gr => gr.Id)
            .FirstOrDefaultAsync();

        if (lastReplace == null)
        {
            return "GR00001";
        }

        // Extract number from last replace number (e.g., "GR00005" -> 5)
        var lastNumber = int.Parse(lastReplace.ReplaceNo.Substring(2));
        var nextNumber = lastNumber + 1;
        
        return $"GR{nextNumber:D5}";
    }

    public async Task<GoodsReplace?> GetWithItemsByIdAsync(int replaceId)
    {
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceReturn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
                .ThenInclude(gri => gri.Product)
            .Include(gr => gr.Items)
                .ThenInclude(gri => gri.Uom)
            .FirstOrDefaultAsync(gr => gr.Id == replaceId);
    }

    public override async Task<IEnumerable<GoodsReplace>> GetAllAsync()
    {
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceReturn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public override async Task<GoodsReplace?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceReturn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
                .ThenInclude(gri => gri.Product)
            .FirstOrDefaultAsync(gr => gr.Id == id);
    }
}
