using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Sale operations
/// </summary>
public interface ISaleService
{
    /// <summary>
    /// Gets all sales asynchronously
    /// </summary>
    Task<IEnumerable<SaleDto>> GetAllSalesAsync();
    
    /// <summary>
    /// Gets a sale by ID asynchronously
    /// </summary>
    /// <param name="id">Sale ID</param>
    Task<SaleDto?> GetSaleByIdAsync(int id);
    
    /// <summary>
    /// Gets sales by customer asynchronously
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    Task<IEnumerable<SaleDto>> GetSalesByCustomerAsync(int customerId);
    
    /// <summary>
    /// Gets today's sales asynchronously
    /// </summary>
    Task<IEnumerable<SaleDto>> GetTodaySalesAsync();
    
    /// <summary>
    /// Gets sales within a date range asynchronously
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    Task<IEnumerable<SaleDto>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Creates a new sale asynchronously
    /// </summary>
    /// <param name="createSaleDto">Sale data</param>
    Task<SaleDto> CreateSaleAsync(CreateSaleDto createSaleDto);
    
    /// <summary>
    /// Cancels a sale asynchronously
    /// </summary>
    /// <param name="id">Sale ID</param>
    Task CancelSaleAsync(int id);
    
    /// <summary>
    /// Processes a refund for a sale asynchronously
    /// </summary>
    /// <param name="id">Sale ID</param>
    Task RefundSaleAsync(int id);
    
    /// <summary>
    /// Gets total sales amount for a date range asynchronously
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    Task<decimal> GetTotalSalesAmountAsync(DateTime startDate, DateTime endDate);
}
