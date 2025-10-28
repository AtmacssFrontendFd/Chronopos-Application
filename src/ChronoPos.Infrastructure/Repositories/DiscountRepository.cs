using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Enums;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Discount entity operations
/// </summary>
public class DiscountRepository : Repository<Discount>, IDiscountRepository
{
    public DiscountRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets discounts that apply to a specific product
    /// </summary>
    /// <param name="productId">ID of the product</param>
    /// <returns>Collection of applicable discounts</returns>
    public async Task<IEnumerable<Discount>> GetByProductAsync(int productId)
    {
        return await _dbSet
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .Include(d => d.ProductDiscounts)
            .Where(d => d.ApplicableOn == DiscountApplicableOn.Product && 
                       d.ProductDiscounts.Any(pd => pd.ProductId == productId) &&
                       d.DeletedAt == null)
            .OrderBy(d => d.Priority)
            .ThenBy(d => d.DiscountName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets discounts that apply to a specific category
    /// </summary>
    /// <param name="categoryId">ID of the category</param>
    /// <returns>Collection of applicable discounts</returns>
    public async Task<IEnumerable<Discount>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .Include(d => d.CategoryDiscounts)
            .Where(d => d.ApplicableOn == DiscountApplicableOn.Category && 
                       d.CategoryDiscounts.Any(cd => cd.CategoryId == categoryId) &&
                       d.DeletedAt == null)
            .OrderBy(d => d.Priority)
            .ThenBy(d => d.DiscountName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets discounts that apply to customers (for backward compatibility)
    /// </summary>
    /// <param name="customerId">ID of the customer</param>
    /// <returns>Collection of applicable discounts</returns>
    public async Task<IEnumerable<Discount>> GetByCustomerAsync(int customerId)
    {
        return await _dbSet
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .Where(d => d.ApplicableOn == DiscountApplicableOn.Customer && 
                       d.DeletedAt == null)
            .OrderBy(d => d.Priority)
            .ThenBy(d => d.DiscountName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets discount by code
    /// </summary>
    /// <param name="code">Discount code</param>
    /// <returns>Discount entity if found</returns>
    public async Task<Discount?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .Include(d => d.DeletedByUser)
            .FirstOrDefaultAsync(d => d.DiscountCode == code && d.DeletedAt == null);
    }

    /// <summary>
    /// Gets all active discounts
    /// </summary>
    /// <returns>Collection of active discounts</returns>
    public async Task<IEnumerable<Discount>> GetActiveDiscountsAsync()
    {
        return await _dbSet
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .Where(d => d.IsActive && d.DeletedAt == null)
            .OrderBy(d => d.Priority)
            .ThenBy(d => d.DiscountName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all currently active discounts (within date range and active)
    /// </summary>
    /// <returns>Collection of currently active discounts</returns>
    public async Task<IEnumerable<Discount>> GetCurrentlyActiveDiscountsAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .Where(d => d.IsActive && 
                       d.DeletedAt == null && 
                       d.StartDate <= now && 
                       d.EndDate >= now)
            .OrderBy(d => d.Priority)
            .ThenBy(d => d.DiscountName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets discounts by applicable type and ID
    /// </summary>
    /// <param name="applicableOn">What the discount applies to</param>
    /// <param name="applicableId">ID of the applicable entity</param>
    /// <summary>
    /// Searches discounts with filtering and pagination
    /// </summary>
    public async Task<IEnumerable<Discount>> SearchAsync(
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
        int take = 50)
    {
        var query = BuildSearchQuery(searchTerm, discountType, applicableOn, isActive, 
            isCurrentlyActive, startDateFrom, startDateTo, endDateFrom, endDateTo, 
            storeId, createdBy);

        return await query
            .OrderBy(d => d.Priority)
            .ThenBy(d => d.DiscountName)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    /// <summary>
    /// Gets count of discounts matching search criteria
    /// </summary>
    public async Task<int> GetSearchCountAsync(
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
        int? createdBy = null)
    {
        var query = BuildSearchQuery(searchTerm, discountType, applicableOn, isActive, 
            isCurrentlyActive, startDateFrom, startDateTo, endDateFrom, endDateTo, 
            storeId, createdBy);

        return await query.CountAsync();
    }

    /// <summary>
    /// Checks if discount code exists (case-insensitive)
    /// </summary>
    /// <param name="code">Discount code to check</param>
    /// <param name="excludeId">Discount ID to exclude from check (for updates)</param>
    /// <returns>True if code exists</returns>
    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        var query = _dbSet.Where(d => d.DiscountCode.ToLower() == code.ToLower() && d.DeletedAt == null);
        
        if (excludeId.HasValue)
        {
            query = query.Where(d => d.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Soft deletes a discount
    /// </summary>
    /// <param name="id">Discount ID</param>
    /// <param name="deletedBy">ID of user performing deletion</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> SoftDeleteAsync(int id, int? deletedBy = null)
    {
        var discount = await GetByIdAsync(id);
        if (discount == null) return false;

        discount.DeletedAt = DateTime.UtcNow;
        discount.DeletedBy = deletedBy;

        await UpdateAsync(discount);
        return true;
    }

    /// <summary>
    /// Restores a soft-deleted discount
    /// </summary>
    /// <param name="id">Discount ID</param>
    /// <param name="restoredBy">ID of user performing restoration</param>
    /// <returns>True if restoration was successful</returns>
    public async Task<bool> RestoreAsync(int id, int? restoredBy = null)
    {
        var discount = await _dbSet.FirstOrDefaultAsync(d => d.Id == id);
        if (discount == null || discount.DeletedAt == null) return false;

        discount.DeletedAt = null;
        discount.DeletedBy = null;
        discount.UpdatedAt = DateTime.UtcNow;
        discount.UpdatedBy = restoredBy;

        await UpdateAsync(discount);
        return true;
    }

    /// <summary>
    /// Sets the active status of a discount
    /// </summary>
    /// <param name="id">Discount ID</param>
    /// <param name="isActive">New active status</param>
    /// <param name="updatedBy">ID of user making the change</param>
    /// <returns>True if update was successful</returns>
    public async Task<bool> SetActiveStatusAsync(int id, bool isActive, int? updatedBy = null)
    {
        var discount = await GetByIdAsync(id);
        if (discount == null) return false;

        discount.IsActive = isActive;
        discount.UpdatedAt = DateTime.UtcNow;
        discount.UpdatedBy = updatedBy;

        await UpdateAsync(discount);
        return true;
    }

    /// <summary>
    /// Gets applicable discounts for a specific order
    /// </summary>
    public async Task<IEnumerable<Discount>> GetApplicableDiscountsAsync(
        decimal orderAmount, 
        int? customerId = null, 
        IEnumerable<int>? productIds = null, 
        IEnumerable<int>? categoryIds = null)
    {
        var now = DateTime.UtcNow;
        var query = _dbSet
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .Where(d => d.IsActive && 
                       d.DeletedAt == null && 
                       d.StartDate <= now && 
                       d.EndDate >= now);

        // Filter by minimum purchase amount
        query = query.Where(d => !d.MinPurchaseAmount.HasValue || orderAmount >= d.MinPurchaseAmount.Value);

        // Filter by applicable type
        if (customerId.HasValue)
        {
            query = query.Where(d => d.ApplicableOn == DiscountApplicableOn.Shop ||
                                   (d.ApplicableOn == DiscountApplicableOn.Customer));
        }

        if (productIds != null && productIds.Any())
        {
            var productIdList = productIds.ToList();
            query = query.Where(d => d.ApplicableOn == DiscountApplicableOn.Shop ||
                                   (d.ApplicableOn == DiscountApplicableOn.Product && d.ProductDiscounts.Any(pd => productIdList.Contains(pd.ProductId))));
        }

        if (categoryIds != null && categoryIds.Any())
        {
            var categoryIdList = categoryIds.ToList();
            query = query.Where(d => d.ApplicableOn == DiscountApplicableOn.Shop ||
                                   (d.ApplicableOn == DiscountApplicableOn.Category && d.CategoryDiscounts.Any(cd => categoryIdList.Contains(cd.CategoryId))));
        }

        return await query
            .OrderBy(d => d.Priority)
            .ThenBy(d => d.DiscountName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets discounts expiring within specified days
    /// </summary>
    /// <param name="days">Number of days to check</param>
    /// <returns>Collection of expiring discounts</returns>
    public async Task<IEnumerable<Discount>> GetExpiringDiscountsAsync(int days = 7)
    {
        var now = DateTime.UtcNow;
        var expiryDate = now.AddDays(days);

        return await _dbSet
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .Where(d => d.IsActive && 
                       d.DeletedAt == null && 
                       d.EndDate >= now && 
                       d.EndDate <= expiryDate)
            .OrderBy(d => d.EndDate)
            .ThenBy(d => d.DiscountName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets discount with related entities (for usage statistics)
    /// </summary>
    /// <param name="id">Discount ID</param>
    /// <returns>Discount with related data</returns>
    public async Task<Discount?> GetWithRelatedDataAsync(int id)
    {
        return await _dbSet
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .Include(d => d.DeletedByUser)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    /// <summary>
    /// Gets discounts by store ID
    /// </summary>
    /// <param name="storeId">Store ID</param>
    /// <returns>Collection of store-specific discounts</returns>
    public async Task<IEnumerable<Discount>> GetByStoreIdAsync(int storeId)
    {
        return await _dbSet
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .Where(d => d.StoreId == storeId && d.DeletedAt == null)
            .OrderBy(d => d.Priority)
            .ThenBy(d => d.DiscountName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets discounts created by specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Collection of discounts created by the user</returns>
    public async Task<IEnumerable<Discount>> GetByCreatedByAsync(int userId)
    {
        return await _dbSet
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .Where(d => d.CreatedBy == userId && d.DeletedAt == null)
            .OrderByDescending(d => d.CreatedAt)
            .ThenBy(d => d.DiscountName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all discounts including soft-deleted ones
    /// </summary>
    /// <returns>Collection of all discounts</returns>
    public async Task<IEnumerable<Discount>> GetAllIncludingDeletedAsync()
    {
        return await _dbSet
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .Include(d => d.DeletedByUser)
            .OrderBy(d => d.Priority)
            .ThenBy(d => d.DiscountName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all discounts without soft-deleted ones
    /// </summary>
    /// <returns>Collection of active discounts</returns>
    public override async Task<IEnumerable<Discount>> GetAllAsync()
    {
        return await _dbSet
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .Where(d => d.DeletedAt == null)
            .OrderBy(d => d.Priority)
            .ThenBy(d => d.DiscountName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets discount by ID without soft-deleted ones
    /// </summary>
    /// <param name="id">Discount ID</param>
    /// <returns>Discount if found and not soft-deleted</returns>
    public override async Task<Discount?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null);
    }

    /// <summary>
    /// Builds the search query for discounts
    /// </summary>
    private IQueryable<Discount> BuildSearchQuery(
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
        int? createdBy = null)
    {
        var query = _dbSet
            .Include(d => d.CreatedByUser)
            .Include(d => d.UpdatedByUser)
            .Where(d => d.DeletedAt == null);

        // Search term filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(d => d.DiscountName.ToLower().Contains(searchLower) ||
                                   d.DiscountCode.ToLower().Contains(searchLower) ||
                                   (d.DiscountDescription != null && d.DiscountDescription.ToLower().Contains(searchLower)));
        }

        // Discount type filter
        if (discountType.HasValue)
        {
            query = query.Where(d => d.DiscountType == discountType.Value);
        }

        // Applicable on filter
        if (applicableOn.HasValue)
        {
            query = query.Where(d => d.ApplicableOn == applicableOn.Value);
        }

        // Active status filter
        if (isActive.HasValue)
        {
            query = query.Where(d => d.IsActive == isActive.Value);
        }

        // Currently active filter
        if (isCurrentlyActive.HasValue && isCurrentlyActive.Value)
        {
            var now = DateTime.UtcNow;
            query = query.Where(d => d.IsActive && 
                               d.StartDate <= now && 
                               d.EndDate >= now);
        }

        // Start date range filter
        if (startDateFrom.HasValue)
        {
            query = query.Where(d => d.StartDate >= startDateFrom.Value);
        }

        if (startDateTo.HasValue)
        {
            query = query.Where(d => d.StartDate <= startDateTo.Value);
        }

        // End date range filter
        if (endDateFrom.HasValue)
        {
            query = query.Where(d => d.EndDate >= endDateFrom.Value);
        }

        if (endDateTo.HasValue)
        {
            query = query.Where(d => d.EndDate <= endDateTo.Value);
        }

        // Store filter
        if (storeId.HasValue)
        {
            query = query.Where(d => d.StoreId == storeId.Value);
        }

        // Created by filter
        if (createdBy.HasValue)
        {
            query = query.Where(d => d.CreatedBy == createdBy.Value);
        }

        return query;
    }
}