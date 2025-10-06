using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for GoodsReceived operations
/// </summary>
public interface IGoodsReceivedService
{
    Task<GoodsReceivedDto?> GetByIdAsync(int id);
    Task<GoodsReceivedDto?> GetByGrnNoAsync(string grnNo);
    Task<IEnumerable<GoodsReceivedDto>> GetAllAsync();
    Task<IEnumerable<GoodsReceivedDto>> GetBySupplierIdAsync(int supplierId);
    Task<IEnumerable<GoodsReceivedDto>> GetByStoreIdAsync(int storeId);
    Task<IEnumerable<GoodsReceivedDto>> GetByStatusAsync(string status);
    Task<IEnumerable<GoodsReceivedDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<GoodsReceivedDto>> SearchAsync(string searchTerm);
    Task<GoodsReceivedDto> CreateAsync(CreateGoodsReceivedDto createDto);
    Task<GoodsReceivedDto> UpdateAsync(UpdateGoodsReceivedDto updateDto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> GrnNoExistsAsync(string grnNo, int? excludeId = null);
    Task<string> GenerateGrnNoAsync();
    Task<bool> PostGrnAsync(int id);
    Task<bool> CancelGrnAsync(int id);
    Task<decimal> CalculateTotalAmountAsync(int grnId);
    Task<int> GetCountAsync();
    Task<IEnumerable<GoodsReceivedDto>> GetPagedAsync(int page, int pageSize, string? searchTerm = null);
}