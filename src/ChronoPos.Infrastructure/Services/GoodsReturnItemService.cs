using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using ChronoPos.Application.Logging;
using Microsoft.Extensions.Logging;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Service for managing goods return items
/// </summary>
public class GoodsReturnItemService : IGoodsReturnItemService
{
    private readonly IGoodsReturnItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GoodsReturnItemService> _logger;

    public GoodsReturnItemService(
        IGoodsReturnItemRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<GoodsReturnItemService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new goods return item
    /// </summary>
    public async Task<GoodsReturnItemDto> CreateAsync(CreateGoodsReturnItemDto createDto)
    {
        AppLogger.LogInfo($"Creating goods return item for return ID: {createDto.ReturnId}, Product ID: {createDto.ProductId}");
        
        try
        {
            var entity = new GoodsReturnItem
            {
                ReturnId = createDto.ReturnId,
                ProductId = createDto.ProductId,
                BatchId = createDto.BatchId,
                BatchNo = createDto.BatchNo,
                ExpiryDate = createDto.ExpiryDate,
                Quantity = createDto.Quantity,
                UomId = createDto.UomId,
                CostPrice = createDto.CostPrice,
                Reason = createDto.Reason,
                CreatedAt = DateTime.UtcNow
            };

            // Calculate line total
            entity.LineTotal = entity.Quantity * entity.CostPrice;

            await _repository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            AppLogger.LogInfo($"Successfully created goods return item with ID: {entity.Id}");
            
            return await MapToDto(entity);
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error creating goods return item: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing goods return item
    /// </summary>
    public async Task<GoodsReturnItemDto> UpdateAsync(int id, UpdateGoodsReturnItemDto updateDto)
    {
        AppLogger.LogInfo($"Updating goods return item with ID: {id}");
        
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                throw new InvalidOperationException($"Goods return item with ID {id} not found");
            }

            // Update properties if provided
            if (updateDto.BatchId.HasValue) entity.BatchId = updateDto.BatchId;
            if (!string.IsNullOrEmpty(updateDto.BatchNo)) entity.BatchNo = updateDto.BatchNo;
            if (updateDto.ExpiryDate.HasValue) entity.ExpiryDate = updateDto.ExpiryDate;
            if (updateDto.Quantity.HasValue) 
            {
                entity.Quantity = updateDto.Quantity.Value;
                entity.LineTotal = entity.Quantity * entity.CostPrice; // Recalculate line total
            }
            if (updateDto.UomId.HasValue) entity.UomId = updateDto.UomId.Value;
            if (updateDto.CostPrice.HasValue) 
            {
                entity.CostPrice = updateDto.CostPrice.Value;
                entity.LineTotal = entity.Quantity * entity.CostPrice; // Recalculate line total
            }
            if (updateDto.Reason != null) entity.Reason = updateDto.Reason;

            await _repository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            AppLogger.LogInfo($"Successfully updated goods return item with ID: {id}");
            
            return await MapToDto(entity);
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error updating goods return item with ID {id}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets a goods return item by ID
    /// </summary>
    public async Task<GoodsReturnItemDto?> GetByIdAsync(int id)
    {
        AppLogger.LogInfo($"Getting goods return item with ID: {id}");
        
        try
        {
            var entity = await _repository.GetByIdWithDetailsAsync(id);
            return entity != null ? await MapToDto(entity) : null;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error getting goods return item with ID {id}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets all goods return items for a specific return
    /// </summary>
    public async Task<IEnumerable<GoodsReturnItemDto>> GetByReturnIdAsync(int returnId)
    {
        AppLogger.LogInfo($"Getting goods return items for return ID: {returnId}");
        
        try
        {
            var entities = await _repository.GetByReturnIdAsync(returnId);
            var dtos = new List<GoodsReturnItemDto>();
            
            foreach (var entity in entities)
            {
                dtos.Add(await MapToDto(entity));
            }
            
            return dtos;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error getting goods return items for return ID {returnId}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Deletes a goods return item
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        AppLogger.LogInfo($"Deleting goods return item with ID: {id}");
        
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                AppLogger.LogWarning($"Goods return item with ID {id} not found for deletion");
                return false;
            }

            await _repository.DeleteAsync(entity.Id);
            await _unitOfWork.SaveChangesAsync();

            AppLogger.LogInfo($"Successfully deleted goods return item with ID: {id}");
            return true;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error deleting goods return item with ID {id}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets all goods return items with pagination
    /// </summary>
    public async Task<(IEnumerable<GoodsReturnItemDto> Items, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 50)
    {
        AppLogger.LogInfo($"Getting all goods return items - Page: {page}, PageSize: {pageSize}");
        
        try
        {
            var (entities, totalCount) = await _repository.GetAllWithDetailsAsync(page, pageSize);
            var dtos = new List<GoodsReturnItemDto>();
            
            foreach (var entity in entities)
            {
                dtos.Add(await MapToDto(entity));
            }
            
            return (dtos, totalCount);
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error getting all goods return items: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Maps a GoodsReturnItem entity to DTO
    /// </summary>
    private static async Task<GoodsReturnItemDto> MapToDto(GoodsReturnItem entity)
    {
        return new GoodsReturnItemDto
        {
            Id = entity.Id,
            ReturnId = entity.ReturnId,
            ProductId = entity.ProductId,
            BatchId = entity.BatchId,
            BatchNo = entity.BatchNo,
            ExpiryDate = entity.ExpiryDate,
            Quantity = entity.Quantity,
            UomId = entity.UomId,
            CostPrice = entity.CostPrice,
            LineTotal = entity.LineTotal,
            Reason = entity.Reason,
            CreatedAt = entity.CreatedAt,
            ProductName = entity.Product?.Name,
            ProductCode = entity.Product?.Code,
            UomName = entity.Uom?.Name
        };
    }
}