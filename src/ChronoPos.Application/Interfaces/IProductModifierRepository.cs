using System.Collections.Generic;
using System.Threading.Tasks;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Application.Interfaces
{
    public interface IProductModifierRepository
    {
        Task<ProductModifier?> GetByIdAsync(int id);
        Task<IEnumerable<ProductModifier>> GetAllAsync();
        Task<IEnumerable<ProductModifier>> GetActiveAsync();
        Task<IEnumerable<ProductModifier>> GetByStatusAsync(string status);
        Task<ProductModifier?> GetBySkuAsync(string sku);
        Task<ProductModifier?> GetByBarcodeAsync(string barcode);
        Task<IEnumerable<ProductModifier>> GetByTaxTypeIdAsync(int taxTypeId);
        Task<IEnumerable<ProductModifier>> SearchAsync(string searchTerm);
        Task<ProductModifier> AddAsync(ProductModifier modifier);
        Task<ProductModifier> UpdateAsync(ProductModifier modifier);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> SkuExistsAsync(string sku, int? excludeId = null);
        Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null);
    }
}
