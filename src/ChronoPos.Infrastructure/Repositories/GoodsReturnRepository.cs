using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using ChronoPos.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for GoodsReturn operations
/// </summary>
public class GoodsReturnRepository : Repository<GoodsReturn>, IGoodsReturnRepository
{
    public GoodsReturnRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GoodsReturn>> GetByStatusAsync(string status)
    {
        return await _context.Set<GoodsReturn>()
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceGrn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
                .ThenInclude(item => item.Product)
            .Include(gr => gr.Items)
                .ThenInclude(item => item.Uom)
            .Where(gr => gr.Status == status)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReturn>> GetBySupplierAsync(int supplierId)
    {
        return await _context.Set<GoodsReturn>()
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceGrn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
                .ThenInclude(item => item.Product)
            .Include(gr => gr.Items)
                .ThenInclude(item => item.Uom)
            .Where(gr => gr.SupplierId == supplierId)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReturn>> GetByStoreAsync(int storeId)
    {
        return await _context.Set<GoodsReturn>()
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceGrn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
                .ThenInclude(item => item.Product)
            .Include(gr => gr.Items)
                .ThenInclude(item => item.Uom)
            .Where(gr => gr.StoreId == storeId)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReturn>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<GoodsReturn>()
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceGrn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
                .ThenInclude(item => item.Product)
            .Include(gr => gr.Items)
                .ThenInclude(item => item.Uom)
            .Where(gr => gr.ReturnDate >= startDate && gr.ReturnDate <= endDate)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReturn>> GetByReferenceGrnAsync(int grnId)
    {
        return await _context.Set<GoodsReturn>()
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceGrn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
                .ThenInclude(item => item.Product)
            .Include(gr => gr.Items)
                .ThenInclude(item => item.Uom)
            .Where(gr => gr.ReferenceGrnId == grnId)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public async Task<string> GetNextReturnNumberAsync()
    {
        var currentYear = DateTime.Now.Year;
        var prefix = $"GR-{currentYear}-";
        
        var lastReturn = await _context.Set<GoodsReturn>()
            .Where(gr => gr.ReturnNo.StartsWith(prefix))
            .OrderByDescending(gr => gr.ReturnNo)
            .FirstOrDefaultAsync();

        if (lastReturn == null)
        {
            return $"{prefix}0001";
        }

        var lastNumberStr = lastReturn.ReturnNo.Substring(prefix.Length);
        if (int.TryParse(lastNumberStr, out var lastNumber))
        {
            return $"{prefix}{(lastNumber + 1):D4}";
        }

        return $"{prefix}0001";
    }

    public async Task<GoodsReturn?> GetWithItemsByIdAsync(int returnId)
    {
        return await _context.Set<GoodsReturn>()
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceGrn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
                .ThenInclude(item => item.Product)
            .Include(gr => gr.Items)
                .ThenInclude(item => item.Uom)
            .Include(gr => gr.Items)
                .ThenInclude(item => item.Batch)
            .FirstOrDefaultAsync(gr => gr.Id == returnId);
    }

    public async Task<IEnumerable<GoodsReturn>> GetWithItemsByCriteriaAsync(
        int? supplierId = null, 
        int? storeId = null, 
        string? status = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        var query = _context.Set<GoodsReturn>()
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceGrn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
                .ThenInclude(item => item.Product)
            .Include(gr => gr.Items)
                .ThenInclude(item => item.Uom)
            .AsQueryable();

        if (supplierId.HasValue)
            query = query.Where(gr => gr.SupplierId == supplierId.Value);

        if (storeId.HasValue)
            query = query.Where(gr => gr.StoreId == storeId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(gr => gr.Status == status);

        if (startDate.HasValue)
            query = query.Where(gr => gr.ReturnDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(gr => gr.ReturnDate <= endDate.Value);

        return await query
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }
}