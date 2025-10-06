using ChronoPos.Application.DTOs;
using ChronoPos.Domain.Enums;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Discount operations
/// </summary>
public interface IDiscountService
{
    /// <summary>
    /// Gets all discounts
    /// </summary>
    /// <returns>Collection of discount DTOs</returns>
    Task<IEnumerable<DiscountDto>> GetAllAsync();

    /// <summary>
    /// Gets all active discounts
    /// </summary>
    /// <returns>Collection of active discount DTOs</returns>
    Task<IEnumerable<DiscountDto>> GetActiveDiscountsAsync();

    /// <summary>
    /// Gets all currently active discounts (within date range and active)
    /// </summary>
    /// <returns>Collection of currently active discount DTOs</returns>
    Task<IEnumerable<DiscountDto>> GetCurrentlyActiveDiscountsAsync();

    /// <summary>
    /// Gets discount by ID
    /// </summary>
    /// <param name="id">Discount ID</param>
    /// <returns>Discount DTO if found</returns>
    Task<DiscountDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets discount by code
    /// </summary>
    /// <param name="code">Discount code</param>
    /// <returns>Discount DTO if found</returns>
    Task<DiscountDto?> GetByCodeAsync(string code);

    /// <summary>
    /// Gets discounts by applicable type (now uses many-to-many relationships)
    /// </summary>
    /// <param name="applicableOn">What the discount applies to</param>
    /// <param name="productIds">List of product IDs to filter by</param>
    /// <param name="categoryIds">List of category IDs to filter by</param>
    /// <returns>Collection of applicable discounts</returns>
    Task<IEnumerable<DiscountDto>> GetByApplicableAsync(DiscountApplicableOn applicableOn, IEnumerable<int>? productIds = null, IEnumerable<int>? categoryIds = null);

    /// <summary>
    /// Searches discounts with filtering
    /// </summary>
    /// <param name="searchDto">Search criteria</param>
    /// <returns>Collection of matching discounts</returns>
    Task<IEnumerable<DiscountDto>> SearchAsync(DiscountSearchDto searchDto);

    /// <summary>
    /// Gets discounts count for search criteria
    /// </summary>
    /// <param name="searchDto">Search criteria</param>
    /// <returns>Total count of matching discounts</returns>
    Task<int> GetSearchCountAsync(DiscountSearchDto searchDto);

    /// <summary>
    /// Creates a new discount
    /// </summary>
    /// <param name="discountDto">Discount data</param>
    /// <returns>Created discount DTO</returns>
    Task<DiscountDto> CreateAsync(CreateDiscountDto discountDto);

    /// <summary>
    /// Updates an existing discount
    /// </summary>
    /// <param name="id">Discount ID</param>
    /// <param name="discountDto">Updated discount data</param>
    /// <returns>Updated discount DTO</returns>
    Task<DiscountDto> UpdateAsync(int id, UpdateDiscountDto discountDto);

    /// <summary>
    /// Deletes a discount (soft delete)
    /// </summary>
    /// <param name="id">Discount ID</param>
    /// <param name="deletedBy">ID of user performing deletion</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int id, int? deletedBy = null);

    /// <summary>
    /// Permanently deletes a discount (hard delete)
    /// </summary>
    /// <param name="id">Discount ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> PermanentDeleteAsync(int id);

    /// <summary>
    /// Restores a soft-deleted discount
    /// </summary>
    /// <param name="id">Discount ID</param>
    /// <param name="restoredBy">ID of user performing restoration</param>
    /// <returns>True if restoration was successful</returns>
    Task<bool> RestoreAsync(int id, int? restoredBy = null);

    /// <summary>
    /// Activates/deactivates a discount
    /// </summary>
    /// <param name="id">Discount ID</param>
    /// <param name="isActive">New active status</param>
    /// <param name="updatedBy">ID of user making the change</param>
    /// <returns>True if update was successful</returns>
    Task<bool> SetActiveStatusAsync(int id, bool isActive, int? updatedBy = null);

    /// <summary>
    /// Checks if discount code exists
    /// </summary>
    /// <param name="code">Discount code</param>
    /// <param name="excludeId">Discount ID to exclude from check</param>
    /// <returns>True if code exists</returns>
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);

    /// <summary>
    /// Validates discount data for creation/update
    /// </summary>
    /// <param name="discountDto">Discount data to validate</param>
    /// <param name="excludeId">Discount ID to exclude from validation</param>
    /// <returns>Validation result</returns>
    Task<DiscountValidationDto> ValidateDiscountAsync(CreateDiscountDto discountDto, int? excludeId = null);

    /// <summary>
    /// Validates discount data for update
    /// </summary>
    /// <param name="discountDto">Discount data to validate</param>
    /// <param name="excludeId">Discount ID to exclude from validation</param>
    /// <returns>Validation result</returns>
    Task<DiscountValidationDto> ValidateDiscountAsync(UpdateDiscountDto discountDto, int? excludeId = null);

    /// <summary>
    /// Gets applicable discounts for a specific order
    /// </summary>
    /// <param name="orderAmount">Order total amount</param>
    /// <param name="customerId">Customer ID</param>
    /// <param name="productIds">List of product IDs in the order</param>
    /// <param name="categoryIds">List of category IDs for products in the order</param>
    /// <returns>Collection of applicable discounts</returns>
    Task<IEnumerable<DiscountDto>> GetApplicableDiscountsAsync(
        decimal orderAmount, 
        int? customerId = null, 
        IEnumerable<int>? productIds = null, 
        IEnumerable<int>? categoryIds = null);

    /// <summary>
    /// Calculates discount amount for given order details
    /// </summary>
    /// <param name="discountId">Discount ID</param>
    /// <param name="orderAmount">Order total amount</param>
    /// <param name="customerId">Customer ID</param>
    /// <param name="productIds">List of product IDs in the order</param>
    /// <param name="categoryIds">List of category IDs for products in the order</param>
    /// <returns>Calculated discount amount</returns>
    Task<decimal> CalculateDiscountAmountAsync(
        int discountId, 
        decimal orderAmount, 
        int? customerId = null, 
        IEnumerable<int>? productIds = null, 
        IEnumerable<int>? categoryIds = null);

    /// <summary>
    /// Gets discounts expiring within specified days
    /// </summary>
    /// <param name="days">Number of days to check</param>
    /// <returns>Collection of expiring discounts</returns>
    Task<IEnumerable<DiscountDto>> GetExpiringDiscountsAsync(int days = 7);

    /// <summary>
    /// Gets discount usage statistics
    /// </summary>
    /// <param name="discountId">Discount ID</param>
    /// <param name="startDate">Start date for statistics</param>
    /// <param name="endDate">End date for statistics</param>
    /// <returns>Usage statistics</returns>
    Task<DiscountUsageStatsDto> GetUsageStatsAsync(int discountId, DateTime? startDate = null, DateTime? endDate = null);
}

/// <summary>
/// DTO for discount usage statistics
/// </summary>
public class DiscountUsageStatsDto
{
    public int DiscountId { get; set; }
    public string DiscountName { get; set; } = string.Empty;
    public string DiscountCode { get; set; } = string.Empty;
    public int TotalUsages { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal AverageDiscountAmount { get; set; }
    public DateTime? FirstUsed { get; set; }
    public DateTime? LastUsed { get; set; }
    public List<DailyUsageDto> DailyUsages { get; set; } = new();
}

/// <summary>
/// DTO for daily discount usage
/// </summary>
public class DailyUsageDto
{
    public DateTime Date { get; set; }
    public int UsageCount { get; set; }
    public decimal TotalAmount { get; set; }
}