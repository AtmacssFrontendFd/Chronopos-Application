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
            .Where(p => p.IsActive)
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
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.Stock <= threshold && p.IsActive)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.IsActive && 
                       (p.Name.Contains(searchTerm) || 
                        p.SKU != null && p.SKU.Contains(searchTerm) ||
                        p.Description != null && p.Description.Contains(searchTerm)))
            .ToListAsync();
    }
    
    public async Task UpdateStockAsync(int productId, int newQuantity)
    {
        var product = await _dbSet.FindAsync(productId);
        if (product != null)
        {
            product.Stock = newQuantity;
            product.UpdatedAt = DateTime.UtcNow;
        }
    }
}
