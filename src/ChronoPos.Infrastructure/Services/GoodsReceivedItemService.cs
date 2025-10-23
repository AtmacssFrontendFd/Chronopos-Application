using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Service implementation for GoodsReceivedItem operations
/// </summary>
public class GoodsReceivedItemService : IGoodsReceivedItemService
{
    private readonly IGoodsReceivedItemRepository _repository;

    public GoodsReceivedItemService(IGoodsReceivedItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<GoodsReceivedItemDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<GoodsReceivedItemDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<GoodsReceivedItemDto>> GetByGrnIdAsync(int grnId)
    {
        var entities = await _repository.GetByGrnIdAsync(grnId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<GoodsReceivedItemDto>> GetByProductIdAsync(int productId)
    {
        var entities = await _repository.GetByProductIdAsync(productId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<GoodsReceivedItemDto>> GetByBatchIdAsync(int batchId)
    {
        var entities = await _repository.GetByBatchIdAsync(batchId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<GoodsReceivedItemDto>> SearchAsync(string searchTerm)
    {
        var entities = await _repository.SearchAsync(searchTerm);
        return entities.Select(MapToDto);
    }

    public async Task<GoodsReceivedItemDto> CreateAsync(CreateGoodsReceivedItemDto createDto)
    {
        var entity = MapFromCreateDto(createDto);
        entity.CreatedAt = DateTime.UtcNow;
        
        var createdEntity = await _repository.AddAsync(entity);
        return MapToDto(createdEntity);
    }

    public async Task<GoodsReceivedItemDto> UpdateAsync(UpdateGoodsReceivedItemDto updateDto)
    {
        // Check if exists
        if (!await _repository.ExistsAsync(updateDto.Id))
        {
            throw new ArgumentException($"Goods received item with ID {updateDto.Id} not found.");
        }

        var entity = MapFromUpdateDto(updateDto);
        var updatedEntity = await _repository.UpdateAsync(entity);
        return MapToDto(updatedEntity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
        {
            return false;
        }

        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> DeleteByGrnIdAsync(int grnId)
    {
        return await _repository.DeleteByGrnIdAsync(grnId);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _repository.ExistsAsync(id);
    }

    public async Task<decimal> GetTotalQuantityByProductIdAsync(int productId)
    {
        return await _repository.GetTotalQuantityByProductIdAsync(productId);
    }

    public async Task<decimal> GetTotalAmountByGrnIdAsync(int grnId)
    {
        return await _repository.GetTotalAmountByGrnIdAsync(grnId);
    }

    public async Task<int> GetCountByGrnIdAsync(int grnId)
    {
        return await _repository.GetCountByGrnIdAsync(grnId);
    }

    public Task<decimal> CalculateLineTotalAsync(decimal quantity, decimal costPrice)
    {
        return Task.FromResult(quantity * costPrice);
    }

    public async Task<IEnumerable<GoodsReceivedItemDto>> GetPagedAsync(int page, int pageSize, string? searchTerm = null)
    {
        var skip = (page - 1) * pageSize;
        var entities = await _repository.GetPagedAsync(skip, pageSize, searchTerm);
        return entities.Select(MapToDto);
    }

    /// <summary>
    /// Maps GoodsReceivedItem entity to DTO
    /// </summary>
    private static GoodsReceivedItemDto MapToDto(GoodsReceivedItem entity)
    {
        return new GoodsReceivedItemDto
        {
            Id = entity.Id,
            GrnId = entity.GrnId,
            ProductId = entity.ProductId,
            ProductName = entity.Product?.Name ?? "",
            ProductCode = entity.Product?.Code ?? "",
            BatchId = entity.BatchId,
            BatchNo = entity.BatchNo,
            ManufactureDate = entity.ManufactureDate,
            ExpiryDate = entity.ExpiryDate,
            Quantity = entity.Quantity,
            UomId = entity.UomId,
            UomName = entity.UnitOfMeasurement?.Name ?? "",
            CostPrice = entity.CostPrice,
            LandedCost = entity.LandedCost,
            LineTotal = entity.LineTotal,
            CreatedAt = entity.CreatedAt
        };
    }

    /// <summary>
    /// Maps CreateGoodsReceivedItemDto to entity
    /// </summary>
    private static GoodsReceivedItem MapFromCreateDto(CreateGoodsReceivedItemDto dto)
    {
        return new GoodsReceivedItem
        {
            GrnId = dto.GrnId,
            ProductId = dto.ProductId,
            BatchId = dto.BatchId,
            BatchNo = dto.BatchNo,
            ManufactureDate = dto.ManufactureDate,
            ExpiryDate = dto.ExpiryDate,
            Quantity = dto.Quantity,
            UomId = dto.UomId,
            CostPrice = dto.CostPrice,
            LandedCost = dto.LandedCost
        };
    }

    /// <summary>
    /// Maps UpdateGoodsReceivedItemDto to entity
    /// </summary>
    private static GoodsReceivedItem MapFromUpdateDto(UpdateGoodsReceivedItemDto dto)
    {
        return new GoodsReceivedItem
        {
            Id = dto.Id,
            GrnId = dto.GrnId,
            ProductId = dto.ProductId,
            BatchId = dto.BatchId,
            BatchNo = dto.BatchNo,
            ManufactureDate = dto.ManufactureDate,
            ExpiryDate = dto.ExpiryDate,
            Quantity = dto.Quantity,
            UomId = dto.UomId,
            CostPrice = dto.CostPrice,
            LandedCost = dto.LandedCost
        };
    }
}