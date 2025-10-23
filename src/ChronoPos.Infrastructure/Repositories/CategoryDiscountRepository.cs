using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for CategoryDiscount entity operations
/// </summary>
public class CategoryDiscountRepository : Repository<CategoryDiscount>, ICategoryDiscountRepository
{
    public CategoryDiscountRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all discounts for a specific category
    /// </summary>
    /// <param name="categoryId">Category identifier</param>
    public async Task<IEnumerable<CategoryDiscount>> GetDiscountsByCategoryIdAsync(int categoryId)
    {
        return await _dbSet
            .Include(cd => cd.Discount)
            .Include(cd => cd.Category)
            .Where(cd => cd.CategoryId == categoryId)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets all categories for a specific discount
    /// </summary>
    /// <param name="discountId">Discount identifier</param>
    public async Task<IEnumerable<CategoryDiscount>> GetCategoriesByDiscountIdAsync(int discountId)
    {
        FileLogger.Log($"CategoryDiscountRepository.GetCategoriesByDiscountIdAsync: Starting for discountId = {discountId}");
        
        try
        {
            // First, let's check if there are any CategoryDiscount records at all
            var totalCount = await _dbSet.CountAsync();
            FileLogger.Log($"CategoryDiscountRepository.GetCategoriesByDiscountIdAsync: Total CategoryDiscount records in database = {totalCount}");
            
            // Let's also check what discount IDs exist
            var allDiscountIds = await _dbSet.Select(cd => cd.DiscountsId).Distinct().ToListAsync();
            FileLogger.Log($"CategoryDiscountRepository.GetCategoriesByDiscountIdAsync: Existing DiscountIds in table = [{string.Join(", ", allDiscountIds)}]");
            
            // Now execute the main query
            var result = await _dbSet
                .Include(cd => cd.Category)
                .Include(cd => cd.Discount)
                .Where(cd => cd.DiscountsId == discountId)
                .ToListAsync();
                
            FileLogger.Log($"CategoryDiscountRepository.GetCategoriesByDiscountIdAsync: Query returned {result.Count} mappings for discountId {discountId}");
            
            foreach (var mapping in result)
            {
                FileLogger.Log($"CategoryDiscountRepository.GetCategoriesByDiscountIdAsync: Found mapping - CategoryId: {mapping.CategoryId}, DiscountsId: {mapping.DiscountsId}, IsActive: {mapping.IsActive}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            FileLogger.Log($"CategoryDiscountRepository.GetCategoriesByDiscountIdAsync: ERROR - {ex.Message}");
            FileLogger.Log($"CategoryDiscountRepository.GetCategoriesByDiscountIdAsync: Stack trace - {ex.StackTrace}");
            throw;
        }
    }
    
    /// <summary>
    /// Gets active (non-deleted) discounts for a category
    /// </summary>
    /// <param name="categoryId">Category identifier</param>
    public async Task<IEnumerable<CategoryDiscount>> GetActiveDiscountsByCategoryIdAsync(int categoryId)
    {
        return await _dbSet
            .Include(cd => cd.Discount)
            .Include(cd => cd.Category)
            .Where(cd => cd.CategoryId == categoryId && cd.DeletedAt == null)
            .ToListAsync();
    }
    
    /// <summary>
    /// Checks if a category-discount mapping exists
    /// </summary>
    /// <param name="categoryId">Category identifier</param>
    /// <param name="discountId">Discount identifier</param>
    public async Task<bool> ExistsMappingAsync(int categoryId, int discountId)
    {
        return await _dbSet
            .AnyAsync(cd => cd.CategoryId == categoryId && 
                           cd.DiscountsId == discountId && 
                           cd.DeletedAt == null);
    }
    
    /// <summary>
    /// Adds multiple category discount mappings
    /// </summary>
    /// <param name="categoryDiscounts">List of category discount mappings</param>
    public async Task AddRangeAsync(IEnumerable<CategoryDiscount> categoryDiscounts)
    {
        await _dbSet.AddRangeAsync(categoryDiscounts);
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Removes all discounts for a category (soft delete)
    /// </summary>
    /// <param name="categoryId">Category identifier</param>
    /// <param name="deletedBy">User ID who performed the deletion</param>
    public async Task RemoveDiscountsByCategoryIdAsync(int categoryId, int? deletedBy = null)
    {
        var categoryDiscounts = await _dbSet
            .Where(cd => cd.CategoryId == categoryId && cd.DeletedAt == null)
            .ToListAsync();
            
        foreach (var cd in categoryDiscounts)
        {
            cd.DeletedAt = DateTime.UtcNow;
            cd.DeletedBy = deletedBy;
        }
        
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Updates category discount mappings for a category
    /// </summary>
    /// <param name="categoryId">Category identifier</param>
    /// <param name="discountIds">New discount IDs to map</param>
    /// <param name="userId">User ID performing the update</param>
    public async Task UpdateCategoryDiscountsAsync(int categoryId, IEnumerable<int> discountIds, int? userId = null)
    {
        // First, soft delete existing mappings
        await RemoveDiscountsByCategoryIdAsync(categoryId, userId);
        
        // Then add new mappings
        var newMappings = discountIds.Select(discountId => new CategoryDiscount
        {
            CategoryId = categoryId,
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