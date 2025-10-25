using System.Collections.Generic;
using System.Threading.Tasks;
using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces
{
    public interface IProductModifierGroupItemService
    {
        Task<ProductModifierGroupItemDto?> GetByIdAsync(int id);
        Task<IEnumerable<ProductModifierGroupItemDto>> GetAllAsync();
        Task<IEnumerable<ProductModifierGroupItemDto>> GetByGroupIdAsync(int groupId);
        Task<IEnumerable<ProductModifierGroupItemDto>> GetByModifierIdAsync(int modifierId);
        Task<IEnumerable<ProductModifierGroupItemDto>> GetActiveByGroupIdAsync(int groupId);
        Task<IEnumerable<ProductModifierGroupItemDto>> GetDefaultSelectionsByGroupIdAsync(int groupId);
        Task<ProductModifierGroupItemDto> CreateAsync(CreateProductModifierGroupItemDto dto);
        Task<ProductModifierGroupItemDto> UpdateAsync(UpdateProductModifierGroupItemDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByGroupIdAsync(int groupId);
        Task<bool> DeleteByModifierIdAsync(int modifierId);
    }
}
