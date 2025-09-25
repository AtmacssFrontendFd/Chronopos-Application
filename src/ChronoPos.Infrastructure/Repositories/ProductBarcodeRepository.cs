using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ProductBarcode entity operations
/// </summary>
public class ProductBarcodeRepository : Repository<ProductBarcode>, IProductBarcodeRepository
{
    public ProductBarcodeRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all barcodes for a specific product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>List of barcodes</returns>
    public async Task<IEnumerable<ProductBarcode>> GetByProductIdAsync(int productId)
    {
        return await _context.ProductBarcodes
            .Where(pb => pb.ProductId == productId)
            .Include(pb => pb.Product)
            .Include(pb => pb.ProductUnit)
            .OrderBy(pb => pb.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all barcodes for a specific product unit
    /// </summary>
    /// <param name="productUnitId">The product unit ID</param>
    /// <returns>List of barcodes</returns>
    public async Task<IEnumerable<ProductBarcode>> GetByProductUnitIdAsync(int productUnitId)
    {
        return await _context.ProductBarcodes
            .Where(pb => pb.ProductUnitId == productUnitId)
            .Include(pb => pb.Product)
            .Include(pb => pb.ProductUnit)
            .OrderBy(pb => pb.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all barcodes for a specific product group
    /// </summary>
    /// <param name="productGroupId">The product group ID</param>
    /// <returns>List of barcodes</returns>
    public async Task<IEnumerable<ProductBarcode>> GetByProductGroupIdAsync(int productGroupId)
    {
        return await _context.ProductBarcodes
            .Where(pb => pb.ProductGroupId == productGroupId)
            .Include(pb => pb.Product)
            .Include(pb => pb.ProductUnit)
            .OrderBy(pb => pb.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a barcode by its value
    /// </summary>
    /// <param name="barcode">The barcode value</param>
    /// <returns>The barcode or null if not found</returns>
    public async Task<ProductBarcode?> GetByBarcodeValueAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return null;

        return await _context.ProductBarcodes
            .Include(pb => pb.Product)
            .Include(pb => pb.ProductUnit)
            .FirstOrDefaultAsync(pb => pb.Barcode == barcode);
    }

    /// <summary>
    /// Checks if a barcode value already exists
    /// </summary>
    /// <param name="barcode">The barcode value</param>
    /// <param name="excludeId">Optional ID to exclude from the check</param>
    /// <returns>True if exists</returns>
    public async Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return false;

        var query = _context.ProductBarcodes.Where(pb => pb.Barcode == barcode);
        
        if (excludeId.HasValue)
        {
            query = query.Where(pb => pb.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Deletes all barcodes for a specific product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>Task</returns>
    public async Task DeleteByProductIdAsync(int productId)
    {
        var barcodes = await _context.ProductBarcodes
            .Where(pb => pb.ProductId == productId)
            .ToListAsync();

        if (barcodes.Any())
        {
            _context.ProductBarcodes.RemoveRange(barcodes);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Deletes all barcodes for a specific product unit
    /// </summary>
    /// <param name="productUnitId">The product unit ID</param>
    /// <returns>Task</returns>
    public async Task DeleteByProductUnitIdAsync(int productUnitId)
    {
        var barcodes = await _context.ProductBarcodes
            .Where(pb => pb.ProductUnitId == productUnitId)
            .ToListAsync();

        if (barcodes.Any())
        {
            _context.ProductBarcodes.RemoveRange(barcodes);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Deletes all barcodes for a specific product group
    /// </summary>
    /// <param name="productGroupId">The product group ID</param>
    /// <returns>Task</returns>
    public async Task DeleteByProductGroupIdAsync(int productGroupId)
    {
        var barcodes = await _context.ProductBarcodes
            .Where(pb => pb.ProductGroupId == productGroupId)
            .ToListAsync();

        if (barcodes.Any())
        {
            _context.ProductBarcodes.RemoveRange(barcodes);
            await _context.SaveChangesAsync();
        }
    }
}
