using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for GoodsReceivedItem operations
/// </summary>
public interface IGoodsReceivedItemService
{
    Task<GoodsReceivedItemDto?> GetByIdAsync(int id);
    Task<IEnumerable<GoodsReceivedItemDto>> GetAllAsync();
    Task<IEnumerable<GoodsReceivedItemDto>> GetByGrnIdAsync(int grnId);
    Task<IEnumerable<GoodsReceivedItemDto>> GetByProductIdAsync(int productId);
    Task<IEnumerable<GoodsReceivedItemDto>> GetByBatchIdAsync(int batchId);
    Task<IEnumerable<GoodsReceivedItemDto>> SearchAsync(string searchTerm);
    Task<GoodsReceivedItemDto> CreateAsync(CreateGoodsReceivedItemDto createDto);
    Task<GoodsReceivedItemDto> UpdateAsync(UpdateGoodsReceivedItemDto updateDto);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteByGrnIdAsync(int grnId);
    Task<bool> ExistsAsync(int id);
    Task<decimal> GetTotalQuantityByProductIdAsync(int productId);
    Task<decimal> GetTotalAmountByGrnIdAsync(int grnId);
    Task<int> GetCountByGrnIdAsync(int grnId);
    Task<decimal> CalculateLineTotalAsync(decimal quantity, decimal costPrice);
    Task<IEnumerable<GoodsReceivedItemDto>> GetPagedAsync(int page, int pageSize, string? searchTerm = null);
}