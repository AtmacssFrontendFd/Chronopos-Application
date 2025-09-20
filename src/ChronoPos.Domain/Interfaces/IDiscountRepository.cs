using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Enums;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for Discount entity operations
/// </summary>
public interface IDiscountRepository : IRepository<Discount>
{
    /// <summary>
    /// Gets discount by code
    /// </summary>
    /// <param name="code">Discount code</param>
    /// <returns>Discount entity if found</returns>
    Task<Discount?> GetByCodeAsync(string code);

    /// <summary>
    /// Gets all active discounts
    /// </summary>
    /// <returns>Collection of active discounts</returns>
    Task<IEnumerable<Discount>> GetActiveDiscountsAsync();

    /// <summary>
    /// Gets all currently active discounts (within date range and active)
    /// </summary>
    /// <returns>Collection of currently active discounts</returns>
    Task<IEnumerable<Discount>> GetCurrentlyActiveDiscountsAsync();

    /// <summary>
    /// Gets discounts that apply to a specific product
    /// </summary>
    /// <param name="productId">ID of the product</param>
    /// <returns>Collection of applicable discounts</returns>
    Task<IEnumerable<Discount>> GetByProductAsync(int productId);

    /// <summary>
    /// Gets discounts that apply to a specific category
    /// </summary>
    /// <param name="categoryId">ID of the category</param>
    /// <returns>Collection of applicable discounts</returns>
    Task<IEnumerable<Discount>> GetByCategoryAsync(int categoryId);

    /// <summary>
    /// Gets discounts that apply to customers
    /// </summary>
    /// <param name="customerId">ID of the customer</param>
    /// <returns>Collection of applicable discounts</returns>
    Task<IEnumerable<Discount>> GetByCustomerAsync(int customerId);

    /// <summary>
    /// Searches discounts with filtering and pagination
    /// </summary>
    /// <param name="searchTerm">Search term for name or code</param>
    /// <param name="discountType">Filter by discount type</param>
    /// <param name="applicableOn">Filter by applicable on</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="isCurrentlyActive">Filter by currently active status</param>
    /// <param name="startDateFrom">Filter by start date from</param>
    /// <param name="startDateTo">Filter by start date to</param>
    /// <param name="endDateFrom">Filter by end date from</param>
    /// <param name="endDateTo">Filter by end date to</param>
    /// <param name="storeId">Filter by store ID</param>
    /// <param name="createdBy">Filter by creator</param>
    /// <param name="skip">Number of records to skip</param>
    /// <param name="take">Number of records to take</param>
    /// <returns>Collection of matching discounts</returns>
    Task<IEnumerable<Discount>> SearchAsync(
        string? searchTerm = null,
        DiscountType? discountType = null,
        DiscountApplicableOn? applicableOn = null,
        bool? isActive = null,
        bool? isCurrentlyActive = null,
        DateTime? startDateFrom = null,
        DateTime? startDateTo = null,
        DateTime? endDateFrom = null,
        DateTime? endDateTo = null,
        int? storeId = null,
        int? createdBy = null,
        int skip = 0,
        int take = 50);

    /// <summary>
    /// Gets count of discounts matching search criteria
    /// </summary>
    /// <param name="searchTerm">Search term for name or code</param>
    /// <param name="discountType">Filter by discount type</param>
    /// <param name="applicableOn">Filter by applicable on</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="isCurrentlyActive">Filter by currently active status</param>
    /// <param name="startDateFrom">Filter by start date from</param>
    /// <param name="startDateTo">Filter by start date to</param>
    /// <param name="endDateFrom">Filter by end date from</param>
    /// <param name="endDateTo">Filter by end date to</param>
    /// <param name="storeId">Filter by store ID</param>
    /// <param name="createdBy">Filter by creator</param>
    /// <returns>Count of matching discounts</returns>
    Task<int> GetSearchCountAsync(
        string? searchTerm = null,
        DiscountType? discountType = null,
        DiscountApplicableOn? applicableOn = null,
        bool? isActive = null,
        bool? isCurrentlyActive = null,
        DateTime? startDateFrom = null,
        DateTime? startDateTo = null,
        DateTime? endDateFrom = null,
        DateTime? endDateTo = null,
        int? storeId = null,
        int? createdBy = null);

    /// <summary>
    /// Checks if discount code exists (case-insensitive)
    /// </summary>
    /// <param name="code">Discount code to check</param>
    /// <param name="excludeId">Discount ID to exclude from check (for updates)</param>
    /// <returns>True if code exists</returns>
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);

    /// <summary>
    /// Soft deletes a discount
    /// </summary>
    /// <param name="id">Discount ID</param>
    /// <param name="deletedBy">ID of user performing deletion</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> SoftDeleteAsync(int id, int? deletedBy = null);

    /// <summary>
    /// Restores a soft-deleted discount
    /// </summary>
    /// <param name="id">Discount ID</param>
    /// <param name="restoredBy">ID of user performing restoration</param>
    /// <returns>True if restoration was successful</returns>
    Task<bool> RestoreAsync(int id, int? restoredBy = null);

    /// <summary>
    /// Sets the active status of a discount
    /// </summary>
    /// <param name="id">Discount ID</param>
    /// <param name="isActive">New active status</param>
    /// <param name="updatedBy">ID of user making the change</param>
    /// <returns>True if update was successful</returns>
    Task<bool> SetActiveStatusAsync(int id, bool isActive, int? updatedBy = null);

    /// <summary>
    /// Gets applicable discounts for a specific order
    /// </summary>
    /// <param name="orderAmount">Order total amount</param>
    /// <param name="customerId">Customer ID</param>
    /// <param name="productIds">List of product IDs in the order</param>
    /// <param name="categoryIds">List of category IDs for products in the order</param>
    /// <returns>Collection of applicable discounts</returns>
    Task<IEnumerable<Discount>> GetApplicableDiscountsAsync(
        decimal orderAmount, 
        int? customerId = null, 
        IEnumerable<int>? productIds = null, 
        IEnumerable<int>? categoryIds = null);

    /// <summary>
    /// Gets discounts expiring within specified days
    /// </summary>
    /// <param name="days">Number of days to check</param>
    /// <returns>Collection of expiring discounts</returns>
    Task<IEnumerable<Discount>> GetExpiringDiscountsAsync(int days = 7);

    /// <summary>
    /// Gets discount with related entities (for usage statistics)
    /// </summary>
    /// <param name="id">Discount ID</param>
    /// <returns>Discount with related data</returns>
    Task<Discount?> GetWithRelatedDataAsync(int id);

    /// <summary>
    /// Gets discounts by store ID
    /// </summary>
    /// <param name="storeId">Store ID</param>
    /// <returns>Collection of store-specific discounts</returns>
    Task<IEnumerable<Discount>> GetByStoreIdAsync(int storeId);

    /// <summary>
    /// Gets discounts created by specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Collection of discounts created by the user</returns>
    Task<IEnumerable<Discount>> GetByCreatedByAsync(int userId);

    /// <summary>
    /// Gets all discounts including soft-deleted ones
    /// </summary>
    /// <returns>Collection of all discounts</returns>
    Task<IEnumerable<Discount>> GetAllIncludingDeletedAsync();
}