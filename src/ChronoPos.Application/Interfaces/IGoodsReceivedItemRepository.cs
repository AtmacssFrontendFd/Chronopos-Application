using ChronoPos.Domain.Entities;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Repository interface for GoodsReceivedItem entity operations
/// </summary>
public interface IGoodsReceivedItemRepository
{
    Task<GoodsReceivedItem?> GetByIdAsync(int id);
    Task<IEnumerable<GoodsReceivedItem>> GetAllAsync();
    Task<IEnumerable<GoodsReceivedItem>> GetByGrnIdAsync(int grnId);
    Task<IEnumerable<GoodsReceivedItem>> GetByProductIdAsync(int productId);
    Task<IEnumerable<GoodsReceivedItem>> GetByBatchIdAsync(int batchId);
    Task<IEnumerable<GoodsReceivedItem>> SearchAsync(string searchTerm);
    Task<GoodsReceivedItem> AddAsync(GoodsReceivedItem item);
    Task<GoodsReceivedItem> UpdateAsync(GoodsReceivedItem item);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteByGrnIdAsync(int grnId);
    Task<bool> ExistsAsync(int id);
    Task<decimal> GetTotalQuantityByProductIdAsync(int productId);
    Task<decimal> GetTotalAmountByGrnIdAsync(int grnId);
    Task<int> GetCountByGrnIdAsync(int grnId);
    Task<IEnumerable<GoodsReceivedItem>> GetPagedAsync(int skip, int take, string? searchTerm = null);
}