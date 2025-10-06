using Microsoft.EntityFrameworkCore;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for GoodsReceived entity
/// </summary>
public class GoodsReceivedRepository : Repository<GoodsReceived>, IGoodsReceivedRepository
{
    public GoodsReceivedRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public override async Task<GoodsReceived?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.Items)
                .ThenInclude(i => i.Product)
            .Include(gr => gr.Items)
                .ThenInclude(i => i.UnitOfMeasurement)
            .FirstOrDefaultAsync(gr => gr.Id == id);
    }

    public async Task<GoodsReceived?> GetByGrnNoAsync(string grnNo)
    {
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.Items)
                .ThenInclude(i => i.Product)
            .Include(gr => gr.Items)
                .ThenInclude(i => i.UnitOfMeasurement)
            .FirstOrDefaultAsync(gr => gr.GrnNo == grnNo);
    }

    public override async Task<IEnumerable<GoodsReceived>> GetAllAsync()
    {
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.Items)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReceived>> GetBySupplierIdAsync(int supplierId)
    {
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.Items)
            .Where(gr => gr.SupplierId == supplierId)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReceived>> GetByStoreIdAsync(int storeId)
    {
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.Items)
            .Where(gr => gr.StoreId == storeId)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReceived>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.Items)
            .Where(gr => gr.Status == status)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReceived>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.Items)
            .Where(gr => gr.ReceivedDate >= startDate && gr.ReceivedDate <= endDate)
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GoodsReceived>> SearchAsync(string searchTerm)
    {
        var term = searchTerm.ToLower();
        return await _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.Items)
            .Where(gr => 
                gr.GrnNo.ToLower().Contains(term) ||
                (gr.InvoiceNo != null && gr.InvoiceNo.ToLower().Contains(term)) ||
                (gr.Supplier != null && gr.Supplier.CompanyName.ToLower().Contains(term)) ||
                (gr.Store != null && gr.Store.Name.ToLower().Contains(term)) ||
                gr.Status.ToLower().Contains(term))
            .OrderByDescending(gr => gr.CreatedAt)
            .ToListAsync();
    }

    public override async Task<GoodsReceived> AddAsync(GoodsReceived entity)
    {
        var result = await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    public override async Task<GoodsReceived> UpdateAsync(GoodsReceived goodsReceived)
    {
        // Check if entity is already being tracked
        var existingEntry = _context.Entry(goodsReceived);
        
        if (existingEntry.State == EntityState.Detached)
        {
            // Entity is not tracked, we can safely attach it
            var trackedEntity = _dbSet.Local.FirstOrDefault(e => e.Id == goodsReceived.Id);
            if (trackedEntity != null)
            {
                // Update the tracked entity with new values
                _context.Entry(trackedEntity).CurrentValues.SetValues(goodsReceived);
            }
            else
            {
                // No existing tracked entity, attach this one
                _context.Entry(goodsReceived).State = EntityState.Modified;
            }
        }
        else
        {
            // Entity is already tracked, just mark it as modified
            existingEntry.State = EntityState.Modified;
        }
        
        await _context.SaveChangesAsync();
        return goodsReceived;
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        var goodsReceived = await _dbSet.FindAsync(id);
        if (goodsReceived == null)
            return false;

        _dbSet.Remove(goodsReceived);
        await _context.SaveChangesAsync();
        return true;
    }

    public override async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(gr => gr.Id == id);
    }

    public async Task<bool> GrnNoExistsAsync(string grnNo, int? excludeId = null)
    {
        var query = _dbSet.Where(gr => gr.GrnNo == grnNo);
        if (excludeId.HasValue)
        {
            query = query.Where(gr => gr.Id != excludeId.Value);
        }
        return await query.AnyAsync();
    }

    public async Task<string> GenerateGrnNoAsync()
    {
        var year = DateTime.Now.Year;
        var prefix = $"GRN-{year}-";
        
        var lastGrn = await _dbSet
            .Where(gr => gr.GrnNo.StartsWith(prefix))
            .OrderByDescending(gr => gr.GrnNo)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastGrn != null)
        {
            var lastNumberStr = lastGrn.GrnNo.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }

    public async Task<int> GetCountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public async Task<IEnumerable<GoodsReceived>> GetPagedAsync(int skip, int take, string? searchTerm = null)
    {
        var query = _dbSet
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.Items)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(gr => 
                gr.GrnNo.ToLower().Contains(term) ||
                (gr.InvoiceNo != null && gr.InvoiceNo.ToLower().Contains(term)) ||
                (gr.Supplier != null && gr.Supplier.CompanyName.ToLower().Contains(term)) ||
                (gr.Store != null && gr.Store.Name.ToLower().Contains(term)) ||
                gr.Status.ToLower().Contains(term));
        }

        return await query
            .OrderByDescending(gr => gr.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
}