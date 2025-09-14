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
            .Include(p => p.ProductBarcodes)
            .ToListAsync();
    }

    public override async Task<Product?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.ProductBarcodes)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.ProductBarcodes)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.ProductBarcodes)
            .Where(p => p.StockQuantity <= threshold && p.IsActive)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductRepository] SearchProductsAsync called with term: '{searchTerm}'");
            System.Diagnostics.Debug.WriteLine($"[ProductRepository] DbSet null check: {_dbSet == null}");
            System.Diagnostics.Debug.WriteLine($"[ProductRepository] Context null check: {_context == null}");
            
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                System.Diagnostics.Debug.WriteLine("[ProductRepository] Search term is null/whitespace, returning empty");
                return new List<Product>();
            }

            // First let's check if we have any products at all
            var totalCount = await _dbSet.CountAsync();
            System.Diagnostics.Debug.WriteLine($"[ProductRepository] Total products in database: {totalCount}");
            
            // Let's also check first 5 products
            var firstFew = await _dbSet.Take(5).Select(p => new { p.Id, p.Name }).ToListAsync();
            System.Diagnostics.Debug.WriteLine($"[ProductRepository] First 5 products:");
            foreach (var p in firstFew)
            {
                System.Diagnostics.Debug.WriteLine($"  - ID: {p.Id}, Name: '{p.Name}'");
            }
            
            System.Diagnostics.Debug.WriteLine($"[ProductRepository] Executing search query for term: '{searchTerm}'");
            
            var query = _dbSet
                .Include(p => p.Category)
                .Include(p => p.ProductBarcodes)
                .Where(p => EF.Functions.Like(p.Name.ToLower(), $"%{searchTerm.ToLower()}%") || 
                           (p.SKU != null && EF.Functions.Like(p.SKU.ToLower(), $"%{searchTerm.ToLower()}%")) || 
                           p.ProductBarcodes.Any(pb => EF.Functions.Like(pb.Barcode.ToLower(), $"%{searchTerm.ToLower()}%")));
            
            // Log the SQL query
            System.Diagnostics.Debug.WriteLine($"[ProductRepository] Generated SQL query: {query.ToQueryString()}");
            
            var results = await query.ToListAsync();
            
            System.Diagnostics.Debug.WriteLine($"[ProductRepository] Query returned {results.Count} results");
            
            foreach (var product in results)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductRepository] Result: {product.Name} (ID: {product.Id})");
            }
            
            return results;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductRepository] ERROR in SearchProductsAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductRepository] Inner exception: {ex.InnerException?.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductRepository] Stack trace: {ex.StackTrace}");
            throw;
        }
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
