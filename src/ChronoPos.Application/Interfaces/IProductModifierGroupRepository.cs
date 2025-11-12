using System.Collections.Generic;
using System.Threading.Tasks;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Application.Interfaces
{
    public interface IProductModifierGroupRepository
    {
        Task<ProductModifierGroup?> GetByIdAsync(int id);
        Task<IEnumerable<ProductModifierGroup>> GetAllAsync();
        Task<IEnumerable<ProductModifierGroup>> GetActiveAsync();
        Task<IEnumerable<ProductModifierGroup>> GetByStatusAsync(string status);
        Task<IEnumerable<ProductModifierGroup>> GetBySelectionTypeAsync(string selectionType);
        Task<IEnumerable<ProductModifierGroup>> GetRequiredGroupsAsync();
        Task<IEnumerable<ProductModifierGroup>> SearchAsync(string searchTerm);
        Task<ProductModifierGroup> AddAsync(ProductModifierGroup group);
        Task<ProductModifierGroup> UpdateAsync(ProductModifierGroup group);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
