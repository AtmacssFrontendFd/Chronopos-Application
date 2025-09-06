using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Product entities
/// </summary>
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .ToListAsync();
    }

    public override async Task<Product?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.StockQuantity <= threshold && p.IsActive)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.ProductBarcodes)
            .Where(p => p.Name.Contains(searchTerm) || 
                       (p.SKU != null && p.SKU.Contains(searchTerm)) ||
                       p.ProductBarcodes.Any(pb => pb.Value.Contains(searchTerm)))
            .ToListAsync();
    }

    public async Task UpdateStockAsync(int productId, int newQuantity)
    {
        var product = await _dbSet.FindAsync(productId);
        if (product != null)
        {
            product.StockQuantity = newQuantity;
            product.UpdatedAt = DateTime.UtcNow;
        }
    }
}
