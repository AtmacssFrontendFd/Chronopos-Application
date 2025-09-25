using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChronoPos.Application.DTOs;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using ChronoPos.Application.Interfaces;

namespace ChronoPos.Application.Services
{
    public class ProductAttributeService : IProductAttributeService
    {
        private readonly IProductAttributeRepository _attributeRepo;
        private readonly IProductAttributeValueRepository _valueRepo;

        public ProductAttributeService(IProductAttributeRepository attributeRepo, IProductAttributeValueRepository valueRepo)
        {
            _attributeRepo = attributeRepo;
            _valueRepo = valueRepo;
        }

        public async Task<List<ProductAttributeDto>> GetAllAttributesAsync()
        {
            var entities = await _attributeRepo.GetAllAsync();
            return entities.ConvertAll(MapToDto);
        }

        public async Task<ProductAttributeDto?> GetAttributeByIdAsync(int id)
        {
            var entity = await _attributeRepo.GetByIdAsync(id);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task AddAttributeAsync(ProductAttributeDto dto)
        {
            var entity = MapToEntity(dto);
            await _attributeRepo.AddAsync(entity);
        }

        public async Task UpdateAttributeAsync(ProductAttributeDto dto)
        {
            var entity = MapToEntity(dto);
            await _attributeRepo.UpdateAsync(entity);
        }

        public async Task DeleteAttributeAsync(int id)
        {
            await _attributeRepo.DeleteAsync(id);
        }

        public async Task<List<ProductAttributeValueDto>> GetValuesByAttributeIdAsync(int attributeId)
        {
            var values = await _valueRepo.GetByAttributeIdAsync(attributeId);
            return values.ConvertAll(MapToValueDto);
        }

        public async Task<List<ProductAttributeValueDto>> GetAllAttributeValuesAsync()
        {
            var attributes = await _attributeRepo.GetAllAsync();
            var allValues = new List<ProductAttributeValueDto>();
            
            foreach (var attribute in attributes)
            {
                var values = await _valueRepo.GetByAttributeIdAsync(attribute.Id);
                foreach (var value in values)
                {
                    var dto = MapToValueDto(value);
                    // Add parent attribute information
                    dto.AttributeName = attribute.Name;
                    dto.AttributeNameAr = attribute.NameAr;
                    dto.AttributeType = attribute.Type;
                    dto.IsRequired = attribute.IsRequired;
                    allValues.Add(dto);
                }
            }
            
            return allValues;
        }

        public async Task AddValueAsync(ProductAttributeValueDto dto)
        {
            var entity = MapToValueEntity(dto);
            await _valueRepo.AddAsync(entity);
        }

        public async Task UpdateValueAsync(ProductAttributeValueDto dto)
        {
            var entity = MapToValueEntity(dto);
            await _valueRepo.UpdateAsync(entity);
        }

        public async Task DeleteValueAsync(int id)
        {
            await _valueRepo.DeleteAsync(id);
        }

        // Mapping helpers
        private ProductAttributeDto MapToDto(ProductAttribute entity)
        {
            return new ProductAttributeDto
            {
                Id = entity.Id,
                Name = entity.Name,
                NameAr = entity.NameAr,
                IsRequired = entity.IsRequired,
                Type = entity.Type,
                Status = entity.Status,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Values = entity.Values?.Select(MapToValueDto).ToList(),
                ValuesCount = entity.Values?.Count ?? 0
            };
        }

        private ProductAttribute MapToEntity(ProductAttributeDto dto)
        {
            return new ProductAttribute
            {
                Id = dto.Id,
                Name = dto.Name,
                NameAr = dto.NameAr,
                IsRequired = dto.IsRequired,
                Type = dto.Type,
                Status = dto.Status,
                CreatedBy = dto.CreatedBy,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        private ProductAttributeValueDto MapToValueDto(ProductAttributeValue entity)
        {
            return new ProductAttributeValueDto
            {
                Id = entity.Id,
                AttributeId = entity.AttributeId,
                Value = entity.Value,
                ValueAr = entity.ValueAr,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt
            };
        }

        private ProductAttributeValue MapToValueEntity(ProductAttributeValueDto dto)
        {
            return new ProductAttributeValue
            {
                Id = dto.Id,
                AttributeId = dto.AttributeId,
                Value = dto.Value,
                ValueAr = dto.ValueAr,
                Status = dto.Status,
                CreatedAt = dto.CreatedAt
            };
        }
    }
}