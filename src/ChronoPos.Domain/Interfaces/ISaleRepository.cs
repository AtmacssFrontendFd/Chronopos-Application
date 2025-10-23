using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for Sale entities with specific business operations
/// </summary>
public interface ISaleRepository : IRepository<Sale>
{
    /// <summary>
    /// Gets sales by customer asynchronously
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    Task<IEnumerable<Sale>> GetSalesByCustomerAsync(int customerId);
    
    /// <summary>
    /// Gets sales within a date range asynchronously
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets today's sales asynchronously
    /// </summary>
    Task<IEnumerable<Sale>> GetTodaySalesAsync();
    
    /// <summary>
    /// Gets total sales amount for a date range asynchronously
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    Task<decimal> GetTotalSalesAmountAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Generates a unique transaction number
    /// </summary>
    Task<string> GenerateTransactionNumberAsync();
}
