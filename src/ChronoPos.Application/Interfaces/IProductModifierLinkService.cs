using ChronoPos.Application.DTOs;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Repository interface for ProductModifierLink
/// </summary>
public interface IProductModifierLinkRepository
{
    Task<ProductModifierLink?> GetByIdAsync(int id);
    Task<IEnumerable<ProductModifierLink>> GetAllAsync();
    Task<IEnumerable<ProductModifierLink>> GetByProductIdAsync(int productId);
    Task<IEnumerable<ProductModifierLink>> GetByModifierGroupIdAsync(int modifierGroupId);
    Task<ProductModifierLink?> GetByProductAndGroupAsync(int productId, int modifierGroupId);
    Task<ProductModifierLink> AddAsync(ProductModifierLink link);
    Task<ProductModifierLink> UpdateAsync(ProductModifierLink link);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteByProductIdAsync(int productId);
    Task<bool> DeleteByModifierGroupIdAsync(int modifierGroupId);
    Task<bool> ExistsAsync(int productId, int modifierGroupId);
}

/// <summary>
/// Service interface for ProductModifierLink
/// </summary>
public interface IProductModifierLinkService
{
    Task<ProductModifierLinkDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProductModifierLinkDto>> GetAllAsync();
    Task<IEnumerable<ProductModifierLinkDto>> GetByProductIdAsync(int productId);
    Task<IEnumerable<ProductModifierLinkDto>> GetByModifierGroupIdAsync(int modifierGroupId);
    Task<ProductModifierLinkDto> CreateAsync(CreateProductModifierLinkDto dto);
    Task<ProductModifierLinkDto> UpdateAsync(UpdateProductModifierLinkDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteByProductIdAsync(int productId);
    Task<bool> DeleteByModifierGroupIdAsync(int modifierGroupId);
    Task<bool> LinkExistsAsync(int productId, int modifierGroupId);
}
