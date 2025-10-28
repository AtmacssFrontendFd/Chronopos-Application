using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for CustomerDiscount entity operations
/// </summary>
public class CustomerDiscountRepository : Repository<CustomerDiscount>, ICustomerDiscountRepository
{
    public CustomerDiscountRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all discounts for a specific customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    public async Task<IEnumerable<CustomerDiscount>> GetDiscountsByCustomerIdAsync(int customerId)
    {
        return await _dbSet
            .Include(cd => cd.Discount)
            .Include(cd => cd.Customer)
            .Where(cd => cd.CustomerId == customerId)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets all customers for a specific discount
    /// </summary>
    /// <param name="discountId">Discount identifier</param>
    public async Task<IEnumerable<CustomerDiscount>> GetCustomersByDiscountIdAsync(int discountId)
    {
        FileLogger.Log($"CustomerDiscountRepository.GetCustomersByDiscountIdAsync: Starting for discountId = {discountId}");
        
        try
        {
            // First, let's check if there are any CustomerDiscount records at all
            var totalCount = await _dbSet.CountAsync();
            FileLogger.Log($"CustomerDiscountRepository.GetCustomersByDiscountIdAsync: Total CustomerDiscount records in database = {totalCount}");
            
            // Let's also check what discount IDs exist
            var allDiscountIds = await _dbSet.Select(cd => cd.DiscountsId).Distinct().ToListAsync();
            FileLogger.Log($"CustomerDiscountRepository.GetCustomersByDiscountIdAsync: Existing DiscountIds in table = [{string.Join(", ", allDiscountIds)}]");
            
            // Now execute the main query
            var result = await _dbSet
                .Include(cd => cd.Customer)
                .Include(cd => cd.Discount)
                .Where(cd => cd.DiscountsId == discountId)
                .ToListAsync();
                
            FileLogger.Log($"CustomerDiscountRepository.GetCustomersByDiscountIdAsync: Query returned {result.Count} mappings for discountId {discountId}");
            
            foreach (var mapping in result)
            {
                FileLogger.Log($"CustomerDiscountRepository.GetCustomersByDiscountIdAsync: Found mapping - CustomerId: {mapping.CustomerId}, DiscountsId: {mapping.DiscountsId}, IsActive: {mapping.IsActive}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            FileLogger.Log($"CustomerDiscountRepository.GetCustomersByDiscountIdAsync: ERROR - {ex.Message}");
            FileLogger.Log($"CustomerDiscountRepository.GetCustomersByDiscountIdAsync: Stack trace - {ex.StackTrace}");
            throw;
        }
    }
    
    /// <summary>
    /// Gets active (non-deleted) discounts for a customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    public async Task<IEnumerable<CustomerDiscount>> GetActiveDiscountsByCustomerIdAsync(int customerId)
    {
        return await _dbSet
            .Include(cd => cd.Discount)
            .Include(cd => cd.Customer)
            .Where(cd => cd.CustomerId == customerId && cd.DeletedAt == null)
            .ToListAsync();
    }
    
    /// <summary>
    /// Checks if a customer-discount mapping exists
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="discountId">Discount identifier</param>
    public async Task<bool> ExistsMappingAsync(int customerId, int discountId)
    {
        return await _dbSet
            .AnyAsync(cd => cd.CustomerId == customerId && 
                           cd.DiscountsId == discountId && 
                           cd.DeletedAt == null);
    }
    
    /// <summary>
    /// Adds multiple customer discount mappings
    /// </summary>
    /// <param name="customerDiscounts">List of customer discount mappings</param>
    public async Task AddRangeAsync(IEnumerable<CustomerDiscount> customerDiscounts)
    {
        await _dbSet.AddRangeAsync(customerDiscounts);
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Removes all discounts for a customer (soft delete)
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="deletedBy">User ID who performed the deletion</param>
    public async Task RemoveDiscountsByCustomerIdAsync(int customerId, int? deletedBy = null)
    {
        var customerDiscounts = await _dbSet
            .Where(cd => cd.CustomerId == customerId && cd.DeletedAt == null)
            .ToListAsync();
            
        foreach (var cd in customerDiscounts)
        {
            cd.DeletedAt = DateTime.UtcNow;
            cd.DeletedBy = deletedBy;
        }
        
        await _context.SaveChangesAsync();
    }
    
    /// <summary>
    /// Updates customer discount mappings for a customer
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="discountIds">New discount IDs to map</param>
    /// <param name="userId">User ID performing the update</param>
    public async Task UpdateCustomerDiscountsAsync(int customerId, IEnumerable<int> discountIds, int? userId = null)
    {
        // First, soft delete existing mappings
        await RemoveDiscountsByCustomerIdAsync(customerId, userId);
        
        // Then add new mappings
        var newMappings = discountIds.Select(discountId => new CustomerDiscount
        {
            CustomerId = customerId,
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
