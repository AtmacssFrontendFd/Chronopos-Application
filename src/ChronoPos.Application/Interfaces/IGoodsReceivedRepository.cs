using ChronoPos.Domain.Entities;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Repository interface for GoodsReceived entity operations
/// </summary>
public interface IGoodsReceivedRepository
{
    Task<GoodsReceived?> GetByIdAsync(int id);
    Task<GoodsReceived?> GetByGrnNoAsync(string grnNo);
    Task<IEnumerable<GoodsReceived>> GetAllAsync();
    Task<IEnumerable<GoodsReceived>> GetBySupplierIdAsync(int supplierId);
    Task<IEnumerable<GoodsReceived>> GetByStoreIdAsync(int storeId);
    Task<IEnumerable<GoodsReceived>> GetByStatusAsync(string status);
    Task<IEnumerable<GoodsReceived>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<GoodsReceived>> SearchAsync(string searchTerm);
    Task<GoodsReceived> AddAsync(GoodsReceived goodsReceived);
    Task<GoodsReceived> UpdateAsync(GoodsReceived goodsReceived);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> GrnNoExistsAsync(string grnNo, int? excludeId = null);
    Task<string> GenerateGrnNoAsync();
    Task<int> GetCountAsync();
    Task<IEnumerable<GoodsReceived>> GetPagedAsync(int skip, int take, string? searchTerm = null);
}