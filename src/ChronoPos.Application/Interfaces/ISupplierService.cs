using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

public interface ISupplierService
{
    Task<IEnumerable<SupplierDto>> GetAllAsync();
    Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync();
    Task<SupplierDto?> GetByIdAsync(long id);
    Task<SupplierDto> CreateSupplierAsync(SupplierDto supplierDto);
    Task<SupplierDto> UpdateSupplierAsync(SupplierDto supplierDto);
    Task<bool> DeleteSupplierAsync(long id);
    Task<IEnumerable<SupplierDto>> SearchSuppliersAsync(string searchTerm);
    Task<int> GetTotalCountAsync();
    Task<IEnumerable<SupplierDto>> GetActiveAsync();
}