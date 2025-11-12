using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Infrastructure.Services
{
    public class ProductModifierService : IProductModifierService
    {
        private readonly IProductModifierRepository _repository;

        public ProductModifierService(IProductModifierRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProductModifierDto?> GetByIdAsync(int id)
        {
            var modifier = await _repository.GetByIdAsync(id);
            return modifier == null ? null : MapToDto(modifier);
        }

        public async Task<IEnumerable<ProductModifierDto>> GetAllAsync()
        {
            var modifiers = await _repository.GetAllAsync();
            return modifiers.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductModifierDto>> GetActiveAsync()
        {
            var modifiers = await _repository.GetActiveAsync();
            return modifiers.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductModifierDto>> GetByStatusAsync(string status)
        {
            var modifiers = await _repository.GetByStatusAsync(status);
            return modifiers.Select(MapToDto);
        }

        public async Task<ProductModifierDto?> GetBySkuAsync(string sku)
        {
            var modifier = await _repository.GetBySkuAsync(sku);
            return modifier == null ? null : MapToDto(modifier);
        }

        public async Task<ProductModifierDto?> GetByBarcodeAsync(string barcode)
        {
            var modifier = await _repository.GetByBarcodeAsync(barcode);
            return modifier == null ? null : MapToDto(modifier);
        }

        public async Task<IEnumerable<ProductModifierDto>> GetByTaxTypeIdAsync(int taxTypeId)
        {
            var modifiers = await _repository.GetByTaxTypeIdAsync(taxTypeId);
            return modifiers.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductModifierDto>> SearchAsync(string searchTerm)
        {
            var modifiers = await _repository.SearchAsync(searchTerm);
            return modifiers.Select(MapToDto);
        }

        public async Task<ProductModifierDto> CreateAsync(CreateProductModifierDto dto)
        {
            // Validate SKU uniqueness
            if (!string.IsNullOrWhiteSpace(dto.Sku))
            {
                if (await _repository.SkuExistsAsync(dto.Sku))
                    throw new InvalidOperationException($"SKU '{dto.Sku}' already exists.");
            }

            // Validate Barcode uniqueness
            if (!string.IsNullOrWhiteSpace(dto.Barcode))
            {
                if (await _repository.BarcodeExistsAsync(dto.Barcode))
                    throw new InvalidOperationException($"Barcode '{dto.Barcode}' already exists.");
            }

            var modifier = new ProductModifier
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Cost = dto.Cost,
                Sku = dto.Sku,
                Barcode = dto.Barcode,
                TaxTypeId = dto.TaxTypeId,
                Status = dto.Status,
                CreatedBy = dto.CreatedBy
            };

            var created = await _repository.AddAsync(modifier);
            return MapToDto(created);
        }

        public async Task<ProductModifierDto> UpdateAsync(UpdateProductModifierDto dto)
        {
            var existing = await _repository.GetByIdAsync(dto.Id);
            if (existing == null)
                throw new InvalidOperationException($"Product modifier with ID {dto.Id} not found.");

            // Validate SKU uniqueness
            if (!string.IsNullOrWhiteSpace(dto.Sku))
            {
                if (await _repository.SkuExistsAsync(dto.Sku, dto.Id))
                    throw new InvalidOperationException($"SKU '{dto.Sku}' already exists.");
            }

            // Validate Barcode uniqueness
            if (!string.IsNullOrWhiteSpace(dto.Barcode))
            {
                if (await _repository.BarcodeExistsAsync(dto.Barcode, dto.Id))
                    throw new InvalidOperationException($"Barcode '{dto.Barcode}' already exists.");
            }

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.Price = dto.Price;
            existing.Cost = dto.Cost;
            existing.Sku = dto.Sku;
            existing.Barcode = dto.Barcode;
            existing.TaxTypeId = dto.TaxTypeId;
            existing.Status = dto.Status;

            var updated = await _repository.UpdateAsync(existing);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exists = await _repository.ExistsAsync(id);
            if (!exists)
                throw new InvalidOperationException($"Product modifier with ID {id} not found.");

            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> ValidateSkuAsync(string sku, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return true;

            return !await _repository.SkuExistsAsync(sku, excludeId);
        }

        public async Task<bool> ValidateBarcodeAsync(string barcode, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return true;

            return !await _repository.BarcodeExistsAsync(barcode, excludeId);
        }

        private ProductModifierDto MapToDto(ProductModifier modifier)
        {
            return new ProductModifierDto
            {
                Id = modifier.Id,
                Name = modifier.Name,
                Description = modifier.Description,
                Price = modifier.Price,
                Cost = modifier.Cost,
                Sku = modifier.Sku,
                Barcode = modifier.Barcode,
                TaxTypeId = modifier.TaxTypeId,
                TaxTypeName = modifier.TaxType?.Name,
                Status = modifier.Status,
                CreatedBy = modifier.CreatedBy,
                CreatedByName = modifier.Creator?.FullName,
                CreatedAt = modifier.CreatedAt,
                UpdatedAt = modifier.UpdatedAt
            };
        }
    }
}
