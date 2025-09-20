using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for CategoryDiscount entity operations
/// </summary>
public interface ICategoryDiscountRepository : IRepository<CategoryDiscount>
{
    /// <summary>
    /// Gets all discounts for a specific category
    /// </summary>
    /// <param name="categoryId">Category identifier</param>
    Task<IEnumerable<CategoryDiscount>> GetDiscountsByCategoryIdAsync(int categoryId);
    
    /// <summary>
    /// Gets all categories for a specific discount
    /// </summary>
    /// <param name="discountId">Discount identifier</param>
    Task<IEnumerable<CategoryDiscount>> GetCategoriesByDiscountIdAsync(int discountId);
    
    /// <summary>
    /// Gets active (non-deleted) discounts for a category
    /// </summary>
    /// <param name="categoryId">Category identifier</param>
    Task<IEnumerable<CategoryDiscount>> GetActiveDiscountsByCategoryIdAsync(int categoryId);
    
    /// <summary>
    /// Checks if a category-discount mapping exists
    /// </summary>
    /// <param name="categoryId">Category identifier</param>
    /// <param name="discountId">Discount identifier</param>
    Task<bool> ExistsMappingAsync(int categoryId, int discountId);
    
    /// <summary>
    /// Adds multiple category discount mappings
    /// </summary>
    /// <param name="categoryDiscounts">List of category discount mappings</param>
    Task AddRangeAsync(IEnumerable<CategoryDiscount> categoryDiscounts);
    
    /// <summary>
    /// Removes all discounts for a category (soft delete)
    /// </summary>
    /// <param name="categoryId">Category identifier</param>
    /// <param name="deletedBy">User ID who performed the deletion</param>
    Task RemoveDiscountsByCategoryIdAsync(int categoryId, int? deletedBy = null);
    
    /// <summary>
    /// Updates category discount mappings for a category
    /// </summary>
    /// <param name="categoryId">Category identifier</param>
    /// <param name="discountIds">New discount IDs to map</param>
    /// <param name="userId">User ID performing the update</param>
    Task UpdateCategoryDiscountsAsync(int categoryId, IEnumerable<int> discountIds, int? userId = null);
}