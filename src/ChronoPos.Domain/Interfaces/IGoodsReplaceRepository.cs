using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for GoodsReplace operations
/// </summary>
public interface IGoodsReplaceRepository : IRepository<GoodsReplace>
{
    /// <summary>
    /// Gets goods replaces by status asynchronously
    /// </summary>
    /// <param name="status">Replace status</param>
    Task<IEnumerable<GoodsReplace>> GetByStatusAsync(string status);
    
    /// <summary>
    /// Gets goods replaces by supplier ID asynchronously
    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    Task<IEnumerable<GoodsReplace>> GetBySupplierAsync(int supplierId);
    
    /// <summary>
    /// Gets goods replaces by store ID asynchronously
    /// </summary>
    /// <param name="storeId">Store ID</param>
    Task<IEnumerable<GoodsReplace>> GetByStoreAsync(int storeId);
    
    /// <summary>
    /// Gets goods replaces by reference return ID asynchronously
    /// </summary>
    /// <param name="returnId">Return ID</param>
    Task<IEnumerable<GoodsReplace>> GetByReferenceReturnAsync(int returnId);
    
    /// <summary>
    /// Gets goods replaces by date range asynchronously
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    Task<IEnumerable<GoodsReplace>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets the next replace number
    /// </summary>
    Task<string> GetNextReplaceNumberAsync();
    
    /// <summary>
    /// Gets goods replace with items by ID asynchronously
    /// </summary>
    /// <param name="replaceId">Replace ID</param>
    Task<GoodsReplace?> GetWithItemsByIdAsync(int replaceId);
}
