using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for GoodsReturn operations
/// </summary>
public interface IGoodsReturnRepository : IRepository<GoodsReturn>
{
    /// <summary>
    /// Gets goods returns by status asynchronously
    /// </summary>
    /// <param name="status">Return status</param>
    Task<IEnumerable<GoodsReturn>> GetByStatusAsync(string status);
    
    /// <summary>
    /// Gets goods returns by supplier ID asynchronously
    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    Task<IEnumerable<GoodsReturn>> GetBySupplierAsync(int supplierId);
    
    /// <summary>
    /// Gets goods returns by store ID asynchronously
    /// </summary>
    /// <param name="storeId">Store ID</param>
    Task<IEnumerable<GoodsReturn>> GetByStoreAsync(int storeId);
    
    /// <summary>
    /// Gets goods returns by date range asynchronously
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    Task<IEnumerable<GoodsReturn>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets goods returns by reference GRN ID asynchronously
    /// </summary>
    /// <param name="grnId">Reference GRN ID</param>
    Task<IEnumerable<GoodsReturn>> GetByReferenceGrnAsync(int grnId);
    
    /// <summary>
    /// Gets the next return number
    /// </summary>
    Task<string> GetNextReturnNumberAsync();
    
    /// <summary>
    /// Gets return with items by ID asynchronously
    /// </summary>
    /// <param name="returnId">Return ID</param>
    Task<GoodsReturn?> GetWithItemsByIdAsync(int returnId);
    
    /// <summary>
    /// Gets returns with items by multiple criteria
    /// </summary>
    /// <param name="supplierId">Optional supplier filter</param>
    /// <param name="storeId">Optional store filter</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    Task<IEnumerable<GoodsReturn>> GetWithItemsByCriteriaAsync(
        int? supplierId = null, 
        int? storeId = null, 
        string? status = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null);
}