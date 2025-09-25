using System.Collections.Generic;
using System.Threading.Tasks;
using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces
{
    public interface IProductAttributeService
    {
        Task<List<ProductAttributeDto>> GetAllAttributesAsync();
        Task<ProductAttributeDto?> GetAttributeByIdAsync(int id);
        Task AddAttributeAsync(ProductAttributeDto dto);
        Task UpdateAttributeAsync(ProductAttributeDto dto);
        Task DeleteAttributeAsync(int id);

        Task<List<ProductAttributeValueDto>> GetValuesByAttributeIdAsync(int attributeId);
        Task<List<ProductAttributeValueDto>> GetAllAttributeValuesAsync();
        Task AddValueAsync(ProductAttributeValueDto dto);
        Task UpdateValueAsync(ProductAttributeValueDto dto);
        Task DeleteValueAsync(int id);
    }
}