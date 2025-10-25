using System.Collections.Generic;
using System.Threading.Tasks;
using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces
{
    public interface IProductModifierService
    {
        Task<ProductModifierDto?> GetByIdAsync(int id);
        Task<IEnumerable<ProductModifierDto>> GetAllAsync();
        Task<IEnumerable<ProductModifierDto>> GetActiveAsync();
        Task<IEnumerable<ProductModifierDto>> GetByStatusAsync(string status);
        Task<ProductModifierDto?> GetBySkuAsync(string sku);
        Task<ProductModifierDto?> GetByBarcodeAsync(string barcode);
        Task<IEnumerable<ProductModifierDto>> GetByTaxTypeIdAsync(int taxTypeId);
        Task<IEnumerable<ProductModifierDto>> SearchAsync(string searchTerm);
        Task<ProductModifierDto> CreateAsync(CreateProductModifierDto dto);
        Task<ProductModifierDto> UpdateAsync(UpdateProductModifierDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ValidateSkuAsync(string sku, int? excludeId = null);
        Task<bool> ValidateBarcodeAsync(string barcode, int? excludeId = null);
    }
}
