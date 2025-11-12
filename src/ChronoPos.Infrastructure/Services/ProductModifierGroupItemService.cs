using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Infrastructure.Services
{
    public class ProductModifierGroupItemService : IProductModifierGroupItemService
    {
        private readonly IProductModifierGroupItemRepository _repository;
        private readonly IProductModifierGroupRepository _groupRepository;
        private readonly IProductModifierRepository _modifierRepository;

        public ProductModifierGroupItemService(
            IProductModifierGroupItemRepository repository,
            IProductModifierGroupRepository groupRepository,
            IProductModifierRepository modifierRepository)
        {
            _repository = repository;
            _groupRepository = groupRepository;
            _modifierRepository = modifierRepository;
        }

        public async Task<ProductModifierGroupItemDto?> GetByIdAsync(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            return item == null ? null : MapToDto(item);
        }

        public async Task<IEnumerable<ProductModifierGroupItemDto>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductModifierGroupItemDto>> GetByGroupIdAsync(int groupId)
        {
            var items = await _repository.GetByGroupIdAsync(groupId);
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductModifierGroupItemDto>> GetByModifierIdAsync(int modifierId)
        {
            var items = await _repository.GetByModifierIdAsync(modifierId);
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductModifierGroupItemDto>> GetActiveByGroupIdAsync(int groupId)
        {
            var items = await _repository.GetActiveByGroupIdAsync(groupId);
            return items.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductModifierGroupItemDto>> GetDefaultSelectionsByGroupIdAsync(int groupId)
        {
            var items = await _repository.GetDefaultSelectionsByGroupIdAsync(groupId);
            return items.Select(MapToDto);
        }

        public async Task<ProductModifierGroupItemDto> CreateAsync(CreateProductModifierGroupItemDto dto)
        {
            // Validate group exists
            var groupExists = await _groupRepository.ExistsAsync(dto.GroupId);
            if (!groupExists)
                throw new InvalidOperationException($"Modifier group with ID {dto.GroupId} not found.");

            // Validate modifier exists
            var modifierExists = await _modifierRepository.ExistsAsync(dto.ModifierId);
            if (!modifierExists)
                throw new InvalidOperationException($"Modifier with ID {dto.ModifierId} not found.");

            // Check if modifier already exists in group
            var alreadyExists = await _repository.ExistsInGroupAsync(dto.GroupId, dto.ModifierId);
            if (alreadyExists)
                throw new InvalidOperationException($"Modifier is already added to this group.");

            // If sort order is 0, set it to max + 1
            if (dto.SortOrder == 0)
            {
                var maxSortOrder = await _repository.GetMaxSortOrderAsync(dto.GroupId);
                dto.SortOrder = maxSortOrder + 1;
            }

            var item = new ProductModifierGroupItem
            {
                GroupId = dto.GroupId,
                ModifierId = dto.ModifierId,
                PriceAdjustment = dto.PriceAdjustment,
                SortOrder = dto.SortOrder,
                DefaultSelection = dto.DefaultSelection,
                Status = dto.Status
            };

            var created = await _repository.AddAsync(item);
            return MapToDto(created);
        }

        public async Task<ProductModifierGroupItemDto> UpdateAsync(UpdateProductModifierGroupItemDto dto)
        {
            var existing = await _repository.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new InvalidOperationException($"Modifier group item with ID {dto.Id} not found.");

            // Validate group exists
            var groupExists = await _groupRepository.ExistsAsync(dto.GroupId);
            if (!groupExists)
                throw new InvalidOperationException($"Modifier group with ID {dto.GroupId} not found.");

            // Validate modifier exists
            var modifierExists = await _modifierRepository.ExistsAsync(dto.ModifierId);
            if (!modifierExists)
                throw new InvalidOperationException($"Modifier with ID {dto.ModifierId} not found.");

            // Check if changing to a modifier that already exists in group
            if (existing.GroupId != dto.GroupId || existing.ModifierId != dto.ModifierId)
            {
                var alreadyExists = await _repository.ExistsInGroupAsync(dto.GroupId, dto.ModifierId);
                if (alreadyExists)
                    throw new InvalidOperationException($"Modifier is already added to this group.");
            }

            existing.GroupId = dto.GroupId;
            existing.ModifierId = dto.ModifierId;
            existing.PriceAdjustment = dto.PriceAdjustment;
            existing.SortOrder = dto.SortOrder;
            existing.DefaultSelection = dto.DefaultSelection;
            existing.Status = dto.Status;

            var updated = await _repository.UpdateAsync(existing);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exists = await _repository.ExistsAsync(id);
            if (!exists)
                throw new InvalidOperationException($"Modifier group item with ID {id} not found.");

            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> DeleteByGroupIdAsync(int groupId)
        {
            return await _repository.DeleteByGroupIdAsync(groupId);
        }

        public async Task<bool> DeleteByModifierIdAsync(int modifierId)
        {
            return await _repository.DeleteByModifierIdAsync(modifierId);
        }

        private ProductModifierGroupItemDto MapToDto(ProductModifierGroupItem item)
        {
            var modifierPrice = item.Modifier?.Price ?? 0;
            var finalPrice = modifierPrice + item.PriceAdjustment;

            return new ProductModifierGroupItemDto
            {
                Id = item.Id,
                GroupId = item.GroupId,
                GroupName = item.Group?.Name,
                ModifierId = item.ModifierId,
                ModifierName = item.Modifier?.Name,
                ModifierPrice = modifierPrice,
                PriceAdjustment = item.PriceAdjustment,
                FinalPrice = finalPrice,
                SortOrder = item.SortOrder,
                DefaultSelection = item.DefaultSelection,
                Status = item.Status,
                CreatedAt = item.CreatedAt
            };
        }
    }
}
