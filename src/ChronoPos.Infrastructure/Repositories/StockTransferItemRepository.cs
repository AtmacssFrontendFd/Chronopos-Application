using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for StockTransferItem entity operations
/// </summary>
public class StockTransferItemRepository : Repository<StockTransferItem>, IStockTransferItemRepository
{
    public StockTransferItemRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets stock transfer items by transfer ID
    /// </summary>
    /// <param name="transferId">Stock transfer ID</param>
    /// <returns>Collection of stock transfer items</returns>
    public async Task<IEnumerable<StockTransferItem>> GetByTransferIdAsync(int transferId)
    {
        return await _dbSet
            .Include(sti => sti.Product)
            .Include(sti => sti.Uom)
            .Include(sti => sti.Transfer)
            .Where(sti => sti.TransferId == transferId)
            .OrderBy(sti => sti.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Gets stock transfer items by product ID
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of stock transfer items</returns>
    public async Task<IEnumerable<StockTransferItem>> GetByProductIdAsync(int productId)
    {
        return await _dbSet
            .Include(sti => sti.Product)
            .Include(sti => sti.Uom)
            .Include(sti => sti.Transfer)
                .ThenInclude(st => st.FromStore)
            .Include(sti => sti.Transfer)
                .ThenInclude(st => st.ToStore)
            .Where(sti => sti.ProductId == productId)
            .OrderByDescending(sti => sti.Transfer.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets stock transfer items by status
    /// </summary>
    /// <param name="status">Item status</param>
    /// <returns>Collection of stock transfer items</returns>
    public async Task<IEnumerable<StockTransferItem>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Include(sti => sti.Product)
            .Include(sti => sti.Uom)
            .Include(sti => sti.Transfer)
                .ThenInclude(st => st.FromStore)
            .Include(sti => sti.Transfer)
                .ThenInclude(st => st.ToStore)
            .Where(sti => sti.Status == status)
            .OrderByDescending(sti => sti.Transfer.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Updates the status of a stock transfer item
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <param name="status">New status</param>
    /// <returns>True if update was successful</returns>
    public async Task<bool> UpdateStatusAsync(int itemId, string status)
    {
        var item = await _dbSet.FindAsync(itemId);
        if (item == null)
            return false;

        item.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Updates quantities for a stock transfer item
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <param name="quantityReceived">Quantity received</param>
    /// <param name="damagedQty">Damaged quantity</param>
    /// <returns>True if update was successful</returns>
    public async Task<bool> UpdateQuantitiesAsync(int itemId, decimal quantityReceived, decimal damagedQty)
    {
        var item = await _dbSet.FindAsync(itemId);
        if (item == null)
            return false;

        item.QuantityReceived = quantityReceived;
        item.DamagedQty = damagedQty;
        
        // Auto-update status based on quantities
        if (quantityReceived > 0 || damagedQty > 0)
        {
            if (damagedQty >= item.QuantitySent)
                item.Status = "Damaged";
            else if (quantityReceived + damagedQty >= item.QuantitySent)
                item.Status = "Received";
            else
                item.Status = "Partially Received";
        }

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Gets pending items for a specific transfer
    /// </summary>
    /// <param name="transferId">Transfer ID</param>
    /// <returns>Collection of pending items</returns>
    public async Task<IEnumerable<StockTransferItem>> GetPendingItemsAsync(int transferId)
    {
        return await _dbSet
            .Include(sti => sti.Product)
            .Include(sti => sti.Uom)
            .Where(sti => sti.TransferId == transferId && sti.Status == "Pending")
            .OrderBy(sti => sti.Product.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets items with product and UOM details
    /// </summary>
    /// <param name="transferId">Transfer ID</param>
    /// <returns>Collection of items with related data</returns>
    public async Task<IEnumerable<StockTransferItem>> GetItemsWithDetailsAsync(int transferId)
    {
        return await _dbSet
            .Include(sti => sti.Product)
                .ThenInclude(p => p.Category)
            .Include(sti => sti.Product)
                .ThenInclude(p => p.Brand)
            .Include(sti => sti.Uom)
            .Include(sti => sti.Transfer)
            .Where(sti => sti.TransferId == transferId)
            .OrderBy(sti => sti.Product.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if any items exist for a product across all transfers
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if items exist</returns>
    public async Task<bool> HasItemsForProductAsync(int productId)
    {
        return await _dbSet
            .AnyAsync(sti => sti.ProductId == productId);
    }

    /// <summary>
    /// Gets all stock transfer items including related data
    /// </summary>
    /// <returns>Collection of stock transfer items with navigation properties</returns>
    public override async Task<IEnumerable<StockTransferItem>> GetAllAsync()
    {
        return await _dbSet
            .Include(sti => sti.Product)
            .Include(sti => sti.Uom)
            .Include(sti => sti.Transfer)
            .OrderByDescending(sti => sti.Transfer.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets stock transfer item by ID including related data
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <returns>Stock transfer item with navigation properties</returns>
    public override async Task<StockTransferItem?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(sti => sti.Product)
                .ThenInclude(p => p.Category)
            .Include(sti => sti.Product)
                .ThenInclude(p => p.Brand)
            .Include(sti => sti.Uom)
            .Include(sti => sti.Transfer)
                .ThenInclude(st => st.FromStore)
            .Include(sti => sti.Transfer)
                .ThenInclude(st => st.ToStore)
            .FirstOrDefaultAsync(sti => sti.Id == id);
    }
}