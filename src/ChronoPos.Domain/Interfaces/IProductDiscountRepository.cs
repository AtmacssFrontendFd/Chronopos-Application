using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for ProductDiscount entity operations
/// </summary>
public interface IProductDiscountRepository : IRepository<ProductDiscount>
{
    /// <summary>
    /// Gets all discounts for a specific product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    Task<IEnumerable<ProductDiscount>> GetDiscountsByProductIdAsync(int productId);
    
    /// <summary>
    /// Gets all products for a specific discount
    /// </summary>
    /// <param name="discountId">Discount identifier</param>
    Task<IEnumerable<ProductDiscount>> GetProductsByDiscountIdAsync(int discountId);
    
    /// <summary>
    /// Gets active (non-deleted) discounts for a product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    Task<IEnumerable<ProductDiscount>> GetActiveDiscountsByProductIdAsync(int productId);
    
    /// <summary>
    /// Checks if a product-discount mapping exists
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="discountId">Discount identifier</param>
    Task<bool> ExistsMappingAsync(int productId, int discountId);
    
    /// <summary>
    /// Adds multiple product discount mappings
    /// </summary>
    /// <param name="productDiscounts">List of product discount mappings</param>
    Task AddRangeAsync(IEnumerable<ProductDiscount> productDiscounts);
    
    /// <summary>
    /// Removes all discounts for a product (soft delete)
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="deletedBy">User ID who performed the deletion</param>
    Task RemoveDiscountsByProductIdAsync(int productId, int? deletedBy = null);
    
    /// <summary>
    /// Updates product discount mappings for a product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <param name="discountIds">New discount IDs to map</param>
    /// <param name="userId">User ID performing the update</param>
    Task UpdateProductDiscountsAsync(int productId, IEnumerable<int> discountIds, int? userId = null);
}