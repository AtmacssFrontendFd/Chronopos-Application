using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Infrastructure.Services;

public class ProductModifierLinkService : IProductModifierLinkService
{
    private readonly IProductModifierLinkRepository _repository;

    public ProductModifierLinkService(IProductModifierLinkRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductModifierLinkDto?> GetByIdAsync(int id)
    {
        var link = await _repository.GetByIdAsync(id);
        return link == null ? null : MapToDto(link);
    }

    public async Task<IEnumerable<ProductModifierLinkDto>> GetAllAsync()
    {
        var links = await _repository.GetAllAsync();
        return links.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductModifierLinkDto>> GetByProductIdAsync(int productId)
    {
        var links = await _repository.GetByProductIdAsync(productId);
        return links.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductModifierLinkDto>> GetByModifierGroupIdAsync(int modifierGroupId)
    {
        var links = await _repository.GetByModifierGroupIdAsync(modifierGroupId);
        return links.Select(MapToDto);
    }

    public async Task<ProductModifierLinkDto> CreateAsync(CreateProductModifierLinkDto dto)
    {
        // Check if link already exists
        if (await _repository.ExistsAsync(dto.ProductId, dto.ModifierGroupId))
        {
            throw new InvalidOperationException(
                $"Link between Product {dto.ProductId} and Modifier Group {dto.ModifierGroupId} already exists.");
        }

        var link = new ProductModifierLink
        {
            ProductId = dto.ProductId,
            ModifierGroupId = dto.ModifierGroupId
        };

        var created = await _repository.AddAsync(link);
        return MapToDto(created);
    }

    public async Task<ProductModifierLinkDto> UpdateAsync(UpdateProductModifierLinkDto dto)
    {
        var existing = await _repository.GetByIdAsync(dto.Id);
        if (existing == null)
            throw new InvalidOperationException($"Product modifier link with ID {dto.Id} not found.");

        // Check if the new combination already exists (excluding current record)
        var existingLink = await _repository.GetByProductAndGroupAsync(dto.ProductId, dto.ModifierGroupId);
        if (existingLink != null && existingLink.Id != dto.Id)
        {
            throw new InvalidOperationException(
                $"Link between Product {dto.ProductId} and Modifier Group {dto.ModifierGroupId} already exists.");
        }

        existing.ProductId = dto.ProductId;
        existing.ModifierGroupId = dto.ModifierGroupId;

        var updated = await _repository.UpdateAsync(existing);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var exists = await _repository.GetByIdAsync(id);
        if (exists == null)
            throw new InvalidOperationException($"Product modifier link with ID {id} not found.");

        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> DeleteByProductIdAsync(int productId)
    {
        return await _repository.DeleteByProductIdAsync(productId);
    }

    public async Task<bool> DeleteByModifierGroupIdAsync(int modifierGroupId)
    {
        return await _repository.DeleteByModifierGroupIdAsync(modifierGroupId);
    }

    public async Task<bool> LinkExistsAsync(int productId, int modifierGroupId)
    {
        return await _repository.ExistsAsync(productId, modifierGroupId);
    }

    private ProductModifierLinkDto MapToDto(ProductModifierLink link)
    {
        return new ProductModifierLinkDto
        {
            Id = link.Id,
            ProductId = link.ProductId,
            ModifierGroupId = link.ModifierGroupId,
            CreatedAt = link.CreatedAt,
            ProductName = link.Product?.Name,
            ProductSku = link.Product?.Code,
            ModifierGroupName = link.ModifierGroup?.Name
        };
    }
}
