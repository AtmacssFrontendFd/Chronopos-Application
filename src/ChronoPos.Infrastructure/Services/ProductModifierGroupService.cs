using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Infrastructure.Services
{
    public class ProductModifierGroupService : IProductModifierGroupService
    {
        private readonly IProductModifierGroupRepository _repository;

        public ProductModifierGroupService(IProductModifierGroupRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProductModifierGroupDto?> GetByIdAsync(int id)
        {
            var group = await _repository.GetByIdAsync(id);
            return group == null ? null : MapToDto(group);
        }

        public async Task<IEnumerable<ProductModifierGroupDto>> GetAllAsync()
        {
            var groups = await _repository.GetAllAsync();
            return groups.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductModifierGroupDto>> GetActiveAsync()
        {
            var groups = await _repository.GetActiveAsync();
            return groups.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductModifierGroupDto>> GetByStatusAsync(string status)
        {
            var groups = await _repository.GetByStatusAsync(status);
            return groups.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductModifierGroupDto>> GetBySelectionTypeAsync(string selectionType)
        {
            var groups = await _repository.GetBySelectionTypeAsync(selectionType);
            return groups.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductModifierGroupDto>> GetRequiredGroupsAsync()
        {
            var groups = await _repository.GetRequiredGroupsAsync();
            return groups.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductModifierGroupDto>> SearchAsync(string searchTerm)
        {
            var groups = await _repository.SearchAsync(searchTerm);
            return groups.Select(MapToDto);
        }

        public async Task<ProductModifierGroupDto> CreateAsync(CreateProductModifierGroupDto dto)
        {
            // Validate selection type
            if (dto.SelectionType != "Single" && dto.SelectionType != "Multiple")
                throw new InvalidOperationException("Selection type must be 'Single' or 'Multiple'.");

            // Validate min/max selections
            if (dto.MinSelections < 0)
                throw new InvalidOperationException("Minimum selections cannot be negative.");

            if (dto.MaxSelections.HasValue && dto.MaxSelections.Value < dto.MinSelections)
                throw new InvalidOperationException("Maximum selections cannot be less than minimum selections.");

            var group = new ProductModifierGroup
            {
                Name = dto.Name,
                Description = dto.Description,
                SelectionType = dto.SelectionType,
                Required = dto.Required,
                MinSelections = dto.MinSelections,
                MaxSelections = dto.MaxSelections,
                Status = dto.Status,
                CreatedBy = dto.CreatedBy
            };

            var created = await _repository.AddAsync(group);
            return MapToDto(created);
        }

        public async Task<ProductModifierGroupDto> UpdateAsync(UpdateProductModifierGroupDto dto)
        {
            var existing = await _repository.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new InvalidOperationException($"Product modifier group with ID {dto.Id} not found.");

            // Validate selection type
            if (dto.SelectionType != "Single" && dto.SelectionType != "Multiple")
                throw new InvalidOperationException("Selection type must be 'Single' or 'Multiple'.");

            // Validate min/max selections
            if (dto.MinSelections < 0)
                throw new InvalidOperationException("Minimum selections cannot be negative.");

            if (dto.MaxSelections.HasValue && dto.MaxSelections.Value < dto.MinSelections)
                throw new InvalidOperationException("Maximum selections cannot be less than minimum selections.");

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.SelectionType = dto.SelectionType;
            existing.Required = dto.Required;
            existing.MinSelections = dto.MinSelections;
            existing.MaxSelections = dto.MaxSelections;
            existing.Status = dto.Status;

            var updated = await _repository.UpdateAsync(existing);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exists = await _repository.ExistsAsync(id);
            if (!exists)
                throw new InvalidOperationException($"Product modifier group with ID {id} not found.");

            return await _repository.DeleteAsync(id);
        }

        private ProductModifierGroupDto MapToDto(ProductModifierGroup group)
        {
            return new ProductModifierGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                SelectionType = group.SelectionType,
                Required = group.Required,
                MinSelections = group.MinSelections,
                MaxSelections = group.MaxSelections,
                Status = group.Status,
                CreatedBy = group.CreatedBy,
                CreatedByName = group.Creator?.FullName,
                CreatedAt = group.CreatedAt,
                UpdatedAt = group.UpdatedAt,
                ItemCount = group.GroupItems?.Count ?? 0
            };
        }
    }
}
