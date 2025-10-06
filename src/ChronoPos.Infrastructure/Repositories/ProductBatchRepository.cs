using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ProductBatch operations
/// </summary>
public class ProductBatchRepository : IProductBatchRepository
{
    private readonly ChronoPosDbContext _context;

    public ProductBatchRepository(ChronoPosDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all product batches with optional filtering
    /// </summary>
    public async Task<IEnumerable<ProductBatch>> GetAllAsync(
        int? productId = null,
        string? status = null,
        DateTime? expiryFrom = null,
        DateTime? expiryTo = null)
    {
        var query = _context.ProductBatches
            .Include(pb => pb.Product)
            .Include(pb => pb.Uom)
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(pb => pb.ProductId == productId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(pb => pb.Status == status);
        }

        if (expiryFrom.HasValue)
        {
            query = query.Where(pb => pb.ExpiryDate >= expiryFrom.Value);
        }

        if (expiryTo.HasValue)
        {
            query = query.Where(pb => pb.ExpiryDate <= expiryTo.Value);
        }

        return await query
            .OrderBy(pb => pb.ExpiryDate)
            .ThenBy(pb => pb.BatchNo)
            .ToListAsync();
    }

    /// <summary>
    /// Get product batch by ID
    /// </summary>
    public async Task<ProductBatch?> GetByIdAsync(int id)
    {
        return await _context.ProductBatches
            .Include(pb => pb.Product)
            .Include(pb => pb.Uom)
            .FirstOrDefaultAsync(pb => pb.Id == id);
    }

    /// <summary>
    /// Get product batches by product ID
    /// </summary>
    public async Task<IEnumerable<ProductBatch>> GetByProductIdAsync(int productId)
    {
        return await _context.ProductBatches
            .Include(pb => pb.Product)
            .Include(pb => pb.Uom)
            .Where(pb => pb.ProductId == productId)
            .OrderBy(pb => pb.ExpiryDate)
            .ThenBy(pb => pb.BatchNo)
            .ToListAsync();
    }

    /// <summary>
    /// Get product batch by batch number and product ID
    /// </summary>
    public async Task<ProductBatch?> GetByBatchNoAsync(string batchNo, int productId)
    {
        return await _context.ProductBatches
            .Include(pb => pb.Product)
            .Include(pb => pb.Uom)
            .FirstOrDefaultAsync(pb => pb.BatchNo == batchNo && pb.ProductId == productId);
    }

    /// <summary>
    /// Get expired batches
    /// </summary>
    public async Task<IEnumerable<ProductBatch>> GetExpiredBatchesAsync()
    {
        var today = DateTime.Today;
        return await _context.ProductBatches
            .Include(pb => pb.Product)
            .Include(pb => pb.Uom)
            .Where(pb => pb.ExpiryDate.HasValue && pb.ExpiryDate.Value < today && pb.Status == "Active")
            .OrderBy(pb => pb.ExpiryDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get batches near expiry (within specified days)
    /// </summary>
    public async Task<IEnumerable<ProductBatch>> GetNearExpiryBatchesAsync(int days = 30)
    {
        var today = DateTime.Today;
        var futureDate = today.AddDays(days);
        
        return await _context.ProductBatches
            .Include(pb => pb.Product)
            .Include(pb => pb.Uom)
            .Where(pb => pb.ExpiryDate.HasValue && 
                         pb.ExpiryDate.Value >= today && 
                         pb.ExpiryDate.Value <= futureDate &&
                         pb.Status == "Active")
            .OrderBy(pb => pb.ExpiryDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get batches by status
    /// </summary>
    public async Task<IEnumerable<ProductBatch>> GetByStatusAsync(string status)
    {
        return await _context.ProductBatches
            .Include(pb => pb.Product)
            .Include(pb => pb.Uom)
            .Where(pb => pb.Status == status)
            .OrderBy(pb => pb.ExpiryDate)
            .ThenBy(pb => pb.BatchNo)
            .ToListAsync();
    }

    /// <summary>
    /// Add new product batch
    /// </summary>
    public async Task<ProductBatch> AddAsync(ProductBatch productBatch)
    {
        productBatch.CreatedAt = DateTime.Now;
        _context.ProductBatches.Add(productBatch);
        await _context.SaveChangesAsync();
        
        // Load navigation properties
        await _context.Entry(productBatch)
            .Reference(pb => pb.Product)
            .LoadAsync();
        await _context.Entry(productBatch)
            .Reference(pb => pb.Uom)
            .LoadAsync();
            
        return productBatch;
    }

    /// <summary>
    /// Update existing product batch
    /// </summary>
    public async Task<ProductBatch> UpdateAsync(ProductBatch productBatch)
    {
        _context.ProductBatches.Update(productBatch);
        await _context.SaveChangesAsync();
        
        // Load navigation properties
        await _context.Entry(productBatch)
            .Reference(pb => pb.Product)
            .LoadAsync();
        await _context.Entry(productBatch)
            .Reference(pb => pb.Uom)
            .LoadAsync();
            
        return productBatch;
    }

    /// <summary>
    /// Delete product batch
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var productBatch = await _context.ProductBatches.FindAsync(id);
        if (productBatch == null)
        {
            return false;
        }

        _context.ProductBatches.Remove(productBatch);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Check if batch number exists for a product
    /// </summary>
    public async Task<bool> BatchExistsAsync(string batchNo, int productId, int? excludeId = null)
    {
        var query = _context.ProductBatches
            .Where(pb => pb.BatchNo == batchNo && pb.ProductId == productId);

        if (excludeId.HasValue)
        {
            query = query.Where(pb => pb.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Get total quantity for a product across all batches
    /// </summary>
    public async Task<decimal> GetTotalQuantityAsync(int productId)
    {
        return await _context.ProductBatches
            .Where(pb => pb.ProductId == productId && pb.Status == "Active")
            .SumAsync(pb => pb.Quantity);
    }

    /// <summary>
    /// Update batch quantity (for inventory transactions)
    /// </summary>
    public async Task<bool> UpdateQuantityAsync(int batchId, decimal newQuantity)
    {
        var batch = await _context.ProductBatches.FindAsync(batchId);
        if (batch == null)
        {
            return false;
        }

        batch.Quantity = newQuantity;
        
        // Automatically set status to Inactive if quantity is 0
        if (newQuantity <= 0)
        {
            batch.Status = "Inactive";
        }
        else if (batch.Status == "Inactive" && newQuantity > 0)
        {
            batch.Status = "Active";
        }

        await _context.SaveChangesAsync();
        return true;
    }
}