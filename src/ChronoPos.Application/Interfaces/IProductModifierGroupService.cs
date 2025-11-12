using System.Collections.Generic;
using System.Threading.Tasks;
using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces
{
    public interface IProductModifierGroupService
    {
        Task<ProductModifierGroupDto?> GetByIdAsync(int id);
        Task<IEnumerable<ProductModifierGroupDto>> GetAllAsync();
        Task<IEnumerable<ProductModifierGroupDto>> GetActiveAsync();
        Task<IEnumerable<ProductModifierGroupDto>> GetByStatusAsync(string status);
        Task<IEnumerable<ProductModifierGroupDto>> GetBySelectionTypeAsync(string selectionType);
        Task<IEnumerable<ProductModifierGroupDto>> GetRequiredGroupsAsync();
        Task<IEnumerable<ProductModifierGroupDto>> SearchAsync(string searchTerm);
        Task<ProductModifierGroupDto> CreateAsync(CreateProductModifierGroupDto dto);
        Task<ProductModifierGroupDto> UpdateAsync(UpdateProductModifierGroupDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
