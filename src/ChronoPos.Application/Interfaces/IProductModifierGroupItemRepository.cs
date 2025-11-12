using System.Collections.Generic;
using System.Threading.Tasks;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Application.Interfaces
{
    public interface IProductModifierGroupItemRepository
    {
        Task<ProductModifierGroupItem?> GetByIdAsync(int id);
        Task<IEnumerable<ProductModifierGroupItem>> GetAllAsync();
        Task<IEnumerable<ProductModifierGroupItem>> GetByGroupIdAsync(int groupId);
        Task<IEnumerable<ProductModifierGroupItem>> GetByModifierIdAsync(int modifierId);
        Task<IEnumerable<ProductModifierGroupItem>> GetActiveByGroupIdAsync(int groupId);
        Task<IEnumerable<ProductModifierGroupItem>> GetDefaultSelectionsByGroupIdAsync(int groupId);
        Task<ProductModifierGroupItem> AddAsync(ProductModifierGroupItem item);
        Task<ProductModifierGroupItem> UpdateAsync(ProductModifierGroupItem item);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByGroupIdAsync(int groupId);
        Task<bool> DeleteByModifierIdAsync(int modifierId);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsInGroupAsync(int groupId, int modifierId);
        Task<int> GetMaxSortOrderAsync(int groupId);
    }
}
