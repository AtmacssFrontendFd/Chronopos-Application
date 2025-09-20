using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ProductDiscount entity operations
/// </summary>
public class ProductDiscountRepository : Repository<ProductDiscount>, IProductDiscountRepository
{
    public ProductDiscountRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all discounts for a specific product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    public async Task<IEnumerable<ProductDiscount>> GetDiscountsByProductIdAsync(int productId)
    {
        return await _dbSet
            .Include(pd => pd.Discount)
            .Include(pd => pd.Product)
            .Where(pd => pd.ProductId == productId)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets all products for a specific discount
    /// </summary>
    /// <param name="discountId">Discount identifier</param>
    public async Task<IEnumerable<ProductDiscount>> GetProductsByDiscountIdAsync(int discountId)
    {
        FileLogger.Log($"ProductDiscountRepository.GetProductsByDiscountIdAsync: Starting for discountId = {discountId}");
        
        try
        {
            // First, let's check if there are any ProductDiscount records at all
            var totalCount = await _dbSet.CountAsync();
            FileLogger.Log($"ProductDiscountRepository.GetProductsByDiscountIdAsync: Total ProductDiscount records in database = {totalCount}");
            
            // Let's also check what discount IDs exist
            var allDiscountIds = await _dbSet.Select(pd => pd.DiscountsId).Distinct().ToListAsync();
            FileLogger.Log($"ProductDiscountRepository.GetProductsByDiscountIdAsync: Existing DiscountIds in table = [{string.Join(", ", allDiscountIds)}]");
            
            // Now execute the main query
            var result = await _dbSet
                .Include(pd => pd.Product)
                .Include(pd => pd.Discount)
                .Where(pd => pd.DiscountsId == discountId)
                .ToListAsync();
                
            FileLogger.Log($"ProductDiscountRepository.GetProductsByDiscountIdAsync: Query returned {result.Count} mappings for discountId {discountId}");
            
            foreach (var mapping in result)
            {
                FileLogger.Log($"ProductDiscountRepository.GetProductsByDiscountIdAsync: Found mapping - ProductId: {mapping.ProductId}, DiscountsId: {mapping.DiscountsId}, IsActive: {mapping.IsActive}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            FileLogger.Log($"ProductDiscountRepository.GetProductsByDiscountIdAsync: ERROR - {ex.Message}");
            FileLogger.Log($"ProductDiscountRepository.GetProductsByDiscountIdAsync: Stack trace - {ex.StackTrace}");
            throw;
        }
    }
    
    /// <summary>
    /// Gets active (non-deleted) discounts for a product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    public async Task<IEnumerable<ProductDiscount>> GetActiveDiscountsByProductIdAsync(int productId)
    {
        return await _dbSet
            .Include(pd => pd.Discount)
            .Include(pd => pd.Product)
            .Where(pd => pd.ProductId == productId && pd.DeletedAt == null)
            .ToListAsync();
    }
    
    /// <summary>
    /// Checks if a product-discount mapping exists
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="discountId">Discount identifier</param>
    public async Task<bool> ExistsMappingAsync(int productId, int discountId)
    {
        return await _dbSet
            .AnyAsync(pd => pd.ProductId == productId && 
                           pd.DiscountsId == discountId && 
                           pd.DeletedAt == null);
    }
    
    /// <summary>
    /// Adds multiple product discount mappings
    /// </summary>
    /// <param name="productDiscounts">List of product discount mappings</param>
    public async Task AddRangeAsync(IEnumerable<ProductDiscount> productDiscounts)
    {
        await _dbSet.AddRangeAsync(productDiscounts);
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Removes all discounts for a product (soft delete)
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="deletedBy">User ID who performed the deletion</param>
    public async Task RemoveDiscountsByProductIdAsync(int productId, int? deletedBy = null)
    {
        var productDiscounts = await _dbSet
            .Where(pd => pd.ProductId == productId && pd.DeletedAt == null)
            .ToListAsync();
            
        foreach (var pd in productDiscounts)
        {
            pd.DeletedAt = DateTime.UtcNow;
            pd.DeletedBy = deletedBy;
        }
        
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Updates product discount mappings for a product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="discountIds">New discount IDs to map</param>
    /// <param name="userId">User ID performing the update</param>
    public async Task UpdateProductDiscountsAsync(int productId, IEnumerable<int> discountIds, int? userId = null)
    {
        // First, soft delete existing mappings
        await RemoveDiscountsByProductIdAsync(productId, userId);
        
        // Then add new mappings
        var newMappings = discountIds.Select(discountId => new ProductDiscount
        {
            ProductId = productId,
            DiscountsId = discountId,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        
        if (newMappings.Any())
        {
            await AddRangeAsync(newMappings);
        }
    }
}