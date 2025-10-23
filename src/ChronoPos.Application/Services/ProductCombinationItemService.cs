using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for ProductCombinationItem operations
/// </summary>
public class ProductCombinationItemService : IProductCombinationItemService
{
    private readonly IProductCombinationItemRepository _productCombinationItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductCombinationItemService(
        IProductCombinationItemRepository productCombinationItemRepository,
        IUnitOfWork unitOfWork)
    {
        _productCombinationItemRepository = productCombinationItemRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Gets all combination items
    /// </summary>
    /// <returns>List of combination item DTOs</returns>
    public async Task<IEnumerable<ProductCombinationItemDto>> GetAllCombinationItemsAsync()
    {
        var combinationItems = await _productCombinationItemRepository.GetAllWithNavigationAsync();
        return combinationItems.Select(MapToDto);
    }

    /// <summary>
    /// Gets a combination item by ID
    /// </summary>
    /// <param name="id">Combination item ID</param>
    /// <returns>Combination item DTO or null</returns>
    public async Task<ProductCombinationItemDto?> GetCombinationItemByIdAsync(int id)
    {
        var combinationItem = await _productCombinationItemRepository.GetByIdAsync(id);
        return combinationItem == null ? null : MapToDto(combinationItem);
    }

    /// <summary>
    /// Gets all combination items for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>List of combination item DTOs</returns>
    public async Task<IEnumerable<ProductCombinationItemDto>> GetCombinationItemsByProductUnitIdAsync(int productUnitId)
    {
        var combinationItems = await _productCombinationItemRepository.GetByProductUnitIdAsync(productUnitId);
        var dtos = combinationItems.Select(MapToDto).ToList();
        
        // Debug logging
        System.Diagnostics.Debug.WriteLine($"GetCombinationItemsByProductUnitIdAsync - ProductUnit ID: {productUnitId}");
        System.Diagnostics.Debug.WriteLine($"Found {dtos.Count} combination items");
        foreach (var dto in dtos)
        {
            System.Diagnostics.Debug.WriteLine($"  - Combination ID: {dto.Id}, AttributeValueId: {dto.AttributeValueId}, AttributeValueName: {dto.AttributeValueName}");
        }
        
        return dtos;
    }

    /// <summary>
    /// Gets all combination items for a specific attribute value
    /// </summary>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <returns>List of combination item DTOs</returns>
    public async Task<IEnumerable<ProductCombinationItemDto>> GetCombinationItemsByAttributeValueIdAsync(int attributeValueId)
    {
        var combinationItems = await _productCombinationItemRepository.GetByAttributeValueIdAsync(attributeValueId);
        return combinationItems.Select(MapToDto);
    }

    /// <summary>
    /// Gets combination item by product unit and attribute value
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <returns>Combination item DTO or null</returns>
    public async Task<ProductCombinationItemDto?> GetCombinationItemByProductUnitAndAttributeValueAsync(int productUnitId, int attributeValueId)
    {
        var combinationItem = await _productCombinationItemRepository.GetByProductUnitAndAttributeValueAsync(productUnitId, attributeValueId);
        return combinationItem == null ? null : MapToDto(combinationItem);
    }

    /// <summary>
    /// Creates a new combination item
    /// </summary>
    /// <param name="createDto">Create combination item DTO</param>
    /// <returns>Created combination item DTO</returns>
    public async Task<ProductCombinationItemDto> CreateCombinationItemAsync(CreateProductCombinationItemDto createDto)
    {
        // Validate combination doesn't already exist
        var exists = await _productCombinationItemRepository.CombinationExistsAsync(createDto.ProductUnitId, createDto.AttributeValueId);
        if (exists)
        {
            throw new InvalidOperationException($"Combination of ProductUnit {createDto.ProductUnitId} and AttributeValue {createDto.AttributeValueId} already exists.");
        }

        var combinationItem = new ProductCombinationItem
        {
            ProductUnitId = createDto.ProductUnitId,
            AttributeValueId = createDto.AttributeValueId,
            CreatedAt = DateTime.UtcNow
        };

        var createdCombinationItem = await _productCombinationItemRepository.AddAsync(combinationItem);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(createdCombinationItem);
    }

    /// <summary>
    /// Creates multiple combination items
    /// </summary>
    /// <param name="createDtos">Collection of create combination item DTOs</param>
    /// <returns>Collection of created combination item DTOs</returns>
    public async Task<IEnumerable<ProductCombinationItemDto>> CreateMultipleCombinationItemsAsync(IEnumerable<CreateProductCombinationItemDto> createDtos)
    {
        var createDtoList = createDtos.ToList();
        if (!createDtoList.Any())
        {
            return Enumerable.Empty<ProductCombinationItemDto>();
        }

        var createdItems = new List<ProductCombinationItem>();

        foreach (var createDto in createDtoList)
        {
            // Check if combination already exists
            var exists = await _productCombinationItemRepository.CombinationExistsAsync(createDto.ProductUnitId, createDto.AttributeValueId);
            if (exists)
            {
                throw new InvalidOperationException($"Combination of ProductUnit {createDto.ProductUnitId} and AttributeValue {createDto.AttributeValueId} already exists.");
            }

            var combinationItem = new ProductCombinationItem
            {
                ProductUnitId = createDto.ProductUnitId,
                AttributeValueId = createDto.AttributeValueId,
                CreatedAt = DateTime.UtcNow
            };

            var createdItem = await _productCombinationItemRepository.AddAsync(combinationItem);
            createdItems.Add(createdItem);
        }

        await _unitOfWork.SaveChangesAsync();
        return createdItems.Select(MapToDto);
    }

    /// <summary>
    /// Updates an existing combination item
    /// </summary>
    /// <param name="id">Combination item ID</param>
    /// <param name="updateDto">Update combination item DTO</param>
    /// <returns>Updated combination item DTO or null</returns>
    public async Task<ProductCombinationItemDto?> UpdateCombinationItemAsync(int id, UpdateProductCombinationItemDto updateDto)
    {
        var existingCombinationItem = await _productCombinationItemRepository.GetByIdAsync(id);
        if (existingCombinationItem == null)
        {
            return null;
        }

        // Validate combination doesn't already exist (excluding current record)
        var exists = await _productCombinationItemRepository.CombinationExistsAsync(updateDto.ProductUnitId, updateDto.AttributeValueId, id);
        if (exists)
        {
            throw new InvalidOperationException($"Combination of ProductUnit {updateDto.ProductUnitId} and AttributeValue {updateDto.AttributeValueId} already exists.");
        }

        // Update properties
        existingCombinationItem.ProductUnitId = updateDto.ProductUnitId;
        existingCombinationItem.AttributeValueId = updateDto.AttributeValueId;

        await _productCombinationItemRepository.UpdateAsync(existingCombinationItem);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(existingCombinationItem);
    }

    /// <summary>
    /// Deletes a combination item
    /// </summary>
    /// <param name="id">Combination item ID</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteCombinationItemAsync(int id)
    {
        try
        {
            await _productCombinationItemRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Deletes all combination items for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>Number of deleted items</returns>
    public async Task<int> DeleteCombinationItemsByProductUnitIdAsync(int productUnitId)
    {
        return await _productCombinationItemRepository.DeleteByProductUnitIdAsync(productUnitId);
    }

    /// <summary>
    /// Deletes all combination items for a specific attribute value
    /// </summary>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <returns>Number of deleted items</returns>
    public async Task<int> DeleteCombinationItemsByAttributeValueIdAsync(int attributeValueId)
    {
        return await _productCombinationItemRepository.DeleteByAttributeValueIdAsync(attributeValueId);
    }

    /// <summary>
    /// Checks if a combination already exists
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <param name="excludeId">Optional ID to exclude from check</param>
    /// <returns>True if combination exists</returns>
    public async Task<bool> CombinationExistsAsync(int productUnitId, int attributeValueId, int? excludeId = null)
    {
        return await _productCombinationItemRepository.CombinationExistsAsync(productUnitId, attributeValueId, excludeId);
    }

    /// <summary>
    /// Gets combination items by multiple product unit IDs
    /// </summary>
    /// <param name="productUnitIds">Collection of product unit IDs</param>
    /// <returns>Collection of combination item DTOs</returns>
    public async Task<IEnumerable<ProductCombinationItemDto>> GetCombinationItemsByProductUnitIdsAsync(IEnumerable<int> productUnitIds)
    {
        var combinationItems = await _productCombinationItemRepository.GetByProductUnitIdsAsync(productUnitIds);
        return combinationItems.Select(MapToDto);
    }

    /// <summary>
    /// Gets combination items by multiple attribute value IDs
    /// </summary>
    /// <param name="attributeValueIds">Collection of attribute value IDs</param>
    /// <returns>Collection of combination item DTOs</returns>
    public async Task<IEnumerable<ProductCombinationItemDto>> GetCombinationItemsByAttributeValueIdsAsync(IEnumerable<int> attributeValueIds)
    {
        var combinationItems = await _productCombinationItemRepository.GetByAttributeValueIdsAsync(attributeValueIds);
        return combinationItems.Select(MapToDto);
    }

    /// <summary>
    /// Maps ProductCombinationItem entity to DTO
    /// </summary>
    private static ProductCombinationItemDto MapToDto(ProductCombinationItem entity)
    {
        var dto = new ProductCombinationItemDto
        {
            Id = entity.Id,
            ProductUnitId = entity.ProductUnitId,
            AttributeValueId = entity.AttributeValueId,
            CreatedAt = entity.CreatedAt,
            IsNew = false
        };

        // Set ProductUnit related properties if available
        if (entity.ProductUnit != null)
        {
            dto.ProductUnitSku = entity.ProductUnit.Sku;
            // You can add more ProductUnit properties here if needed
        }

        // Set AttributeValue related properties if available
        if (entity.AttributeValue != null)
        {
            dto.AttributeValueName = entity.AttributeValue.Value;
            dto.AttributeValueNameAr = entity.AttributeValue.ValueAr;
            
            // Set Attribute related properties if available
            if (entity.AttributeValue.Attribute != null)
            {
                dto.AttributeName = entity.AttributeValue.Attribute.Name;
                dto.AttributeNameAr = entity.AttributeValue.Attribute.NameAr;
            }
        }

        return dto;
    }
}