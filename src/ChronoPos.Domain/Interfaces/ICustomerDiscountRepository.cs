using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for CustomerDiscount entity operations
/// </summary>
public interface ICustomerDiscountRepository : IRepository<CustomerDiscount>
{
    /// <summary>
    /// Gets all discounts for a specific customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    Task<IEnumerable<CustomerDiscount>> GetDiscountsByCustomerIdAsync(int customerId);
    
    /// <summary>
    /// Gets all customers for a specific discount
    /// </summary>
    /// <param name="discountId">Discount identifier</param>
    Task<IEnumerable<CustomerDiscount>> GetCustomersByDiscountIdAsync(int discountId);
    
    /// <summary>
    /// Gets active (non-deleted) discounts for a customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    Task<IEnumerable<CustomerDiscount>> GetActiveDiscountsByCustomerIdAsync(int customerId);
    
    /// <summary>
    /// Checks if a customer-discount mapping exists
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="discountId">Discount identifier</param>
    Task<bool> ExistsMappingAsync(int customerId, int discountId);
    
    /// <summary>
    /// Adds multiple customer discount mappings
    /// </summary>
    /// <param name="customerDiscounts">List of customer discount mappings</param>
    Task AddRangeAsync(IEnumerable<CustomerDiscount> customerDiscounts);
    
    /// <summary>
    /// Removes all discounts for a customer (soft delete)
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="deletedBy">User ID who performed the deletion</param>
    Task RemoveDiscountsByCustomerIdAsync(int customerId, int? deletedBy = null);
    
    /// <summary>
    /// Updates customer discount mappings for a customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="discountIds">New discount IDs to map</param>
    /// <param name="userId">User ID performing the update</param>
    Task UpdateCustomerDiscountsAsync(int customerId, IEnumerable<int> discountIds, int? userId = null);
}
