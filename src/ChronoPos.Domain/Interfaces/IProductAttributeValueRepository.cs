using ChronoPos.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChronoPos.Domain.Interfaces
{
    public interface IProductAttributeValueRepository
    {
        Task<ProductAttributeValue?> GetByIdAsync(int id);
        Task<List<ProductAttributeValue>> GetByAttributeIdAsync(int attributeId);
        Task AddAsync(ProductAttributeValue value);
        Task UpdateAsync(ProductAttributeValue value);
        Task DeleteAsync(int id);
    }
}