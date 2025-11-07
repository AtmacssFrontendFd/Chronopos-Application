using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using ChronoPos.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for StockLedger entity operations
/// </summary>
public class StockLedgerRepository : Repository<StockLedger>, IStockLedgerRepository
{
    public StockLedgerRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all stock ledger entries for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of stock ledger entries</returns>
    public async Task<IEnumerable<StockLedger>> GetByProductIdAsync(int productId)
    {
        return await _context.Set<StockLedger>()
            .Include(sl => sl.Product)
            .Include(sl => sl.Unit)
            .Where(sl => sl.ProductId == productId)
            .OrderBy(sl => sl.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets stock ledger entries for a product within a date range
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of stock ledger entries</returns>
    public async Task<IEnumerable<StockLedger>> GetByProductIdAndDateRangeAsync(int productId, DateTime startDate, DateTime endDate)
    {
        return await _context.Set<StockLedger>()
            .Include(sl => sl.Product)
            .Include(sl => sl.Unit)
            .Where(sl => sl.ProductId == productId && 
                        sl.CreatedAt >= startDate && 
                        sl.CreatedAt <= endDate)
            .OrderBy(sl => sl.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the current balance for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Current balance</returns>
    public async Task<decimal> GetCurrentBalanceAsync(int productId)
    {
        var latestEntry = await _context.Set<StockLedger>()
            .Where(sl => sl.ProductId == productId)
            .OrderByDescending(sl => sl.CreatedAt)
            .FirstOrDefaultAsync();

        return latestEntry?.Balance ?? 0;
    }

    /// <summary>
    /// Gets stock ledger entries by movement type
    /// </summary>
    /// <param name="movementType">Movement type</param>
    /// <returns>Collection of stock ledger entries</returns>
    public async Task<IEnumerable<StockLedger>> GetByMovementTypeAsync(StockMovementType movementType)
    {
        return await _context.Set<StockLedger>()
            .Include(sl => sl.Product)
            .Include(sl => sl.Unit)
            .Where(sl => sl.MovementType == movementType)
            .OrderByDescending(sl => sl.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets stock ledger entries by reference
    /// </summary>
    /// <param name="referenceType">Reference type</param>
    /// <param name="referenceId">Reference ID</param>
    /// <returns>Collection of stock ledger entries</returns>
    public async Task<IEnumerable<StockLedger>> GetByReferenceAsync(StockReferenceType? referenceType, int? referenceId)
    {
        var query = _context.Set<StockLedger>()
            .Include(sl => sl.Product)
            .Include(sl => sl.Unit)
            .AsQueryable();

        if (referenceType.HasValue)
        {
            query = query.Where(sl => sl.ReferenceType == referenceType.Value);
        }

        if (referenceId.HasValue)
        {
            query = query.Where(sl => sl.ReferenceId == referenceId);
        }

        return await query
            .OrderByDescending(sl => sl.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the latest stock entry for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Latest stock ledger entry</returns>
    public async Task<StockLedger?> GetLatestByProductIdAsync(int productId)
    {
        return await _context.Set<StockLedger>()
            .Include(sl => sl.Product)
            .Include(sl => sl.Unit)
            .Where(sl => sl.ProductId == productId)
            .OrderByDescending(sl => sl.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all stock ledger entries including related data
    /// </summary>
    /// <returns>Collection of stock ledger entries with navigation properties</returns>
    public override async Task<IEnumerable<StockLedger>> GetAllAsync()
    {
        return await _context.Set<StockLedger>()
            .Include(sl => sl.Product)
            .Include(sl => sl.Unit)
            .OrderByDescending(sl => sl.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets stock ledger entry by ID including related data
    /// </summary>
    /// <param name="id">Stock ledger ID</param>
    /// <returns>Stock ledger entry with navigation properties</returns>
    public override async Task<StockLedger?> GetByIdAsync(int id)
    {
        return await _context.Set<StockLedger>()
            .Include(sl => sl.Product)
            .Include(sl => sl.Unit)
            .FirstOrDefaultAsync(sl => sl.Id == id);
    }
}
