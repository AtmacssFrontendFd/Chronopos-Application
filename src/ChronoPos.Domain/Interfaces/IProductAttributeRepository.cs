using ChronoPos.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChronoPos.Domain.Interfaces
{
    public interface IProductAttributeRepository
    {
        Task<ProductAttribute?> GetByIdAsync(int id);
        Task<List<ProductAttribute>> GetAllAsync();
        Task AddAsync(ProductAttribute attribute);
        Task UpdateAsync(ProductAttribute attribute);
        Task DeleteAsync(int id);
    }
}