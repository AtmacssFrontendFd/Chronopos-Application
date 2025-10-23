using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for StockTransfer operations
/// </summary>
public class StockTransferRepository : Repository<StockTransfer>, IStockTransferRepository
{
    public StockTransferRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<StockTransfer>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Include(st => st.FromStore)
            .Include(st => st.ToStore)
            .Include(st => st.Creator)
            .Include(st => st.Items)
                .ThenInclude(sti => sti.Product)
            .Where(st => st.Status == status)
            .OrderByDescending(st => st.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockTransfer>> GetByFromStoreAsync(int fromStoreId)
    {
        return await _dbSet
            .Include(st => st.FromStore)
            .Include(st => st.ToStore)
            .Include(st => st.Creator)
            .Include(st => st.Items)
                .ThenInclude(sti => sti.Product)
            .Where(st => st.FromStoreId == fromStoreId)
            .OrderByDescending(st => st.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockTransfer>> GetByToStoreAsync(int toStoreId)
    {
        return await _dbSet
            .Include(st => st.FromStore)
            .Include(st => st.ToStore)
            .Include(st => st.Creator)
            .Include(st => st.Items)
                .ThenInclude(sti => sti.Product)
            .Where(st => st.ToStoreId == toStoreId)
            .OrderByDescending(st => st.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockTransfer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(st => st.FromStore)
            .Include(st => st.ToStore)
            .Include(st => st.Creator)
            .Include(st => st.Items)
                .ThenInclude(sti => sti.Product)
            .Where(st => st.TransferDate >= startDate && st.TransferDate <= endDate)
            .OrderByDescending(st => st.CreatedAt)
            .ToListAsync();
    }

    public async Task<string> GetNextTransferNumberAsync()
    {
        var lastTransfer = await _dbSet
            .OrderByDescending(st => st.TransferId)
            .FirstOrDefaultAsync();

        if (lastTransfer == null)
        {
            return "ST00001";
        }

        // Extract number from last transfer number (e.g., "ST00005" -> 5)
        var lastNumber = int.Parse(lastTransfer.TransferNo.Substring(2));
        var nextNumber = lastNumber + 1;
        
        return $"ST{nextNumber:D5}";
    }

    public async Task<StockTransfer?> GetWithItemsByIdAsync(int transferId)
    {
        return await _dbSet
            .Include(st => st.FromStore)
            .Include(st => st.ToStore)
            .Include(st => st.Creator)
            .Include(st => st.Items)
                .ThenInclude(sti => sti.Product)
            .Include(st => st.Items)
                .ThenInclude(sti => sti.Uom)
            .FirstOrDefaultAsync(st => st.TransferId == transferId);
    }

    public override async Task<IEnumerable<StockTransfer>> GetAllAsync()
    {
        return await _dbSet
            .Include(st => st.FromStore)
            .Include(st => st.ToStore)
            .Include(st => st.Creator)
            .Include(st => st.Items)
            .OrderByDescending(st => st.CreatedAt)
            .ToListAsync();
    }

    public override async Task<StockTransfer?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(st => st.FromStore)
            .Include(st => st.ToStore)
            .Include(st => st.Creator)
            .Include(st => st.Items)
                .ThenInclude(sti => sti.Product)
            .FirstOrDefaultAsync(st => st.TransferId == id);
    }
}
