using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service for managing selling price types
/// </summary>
public class SellingPriceTypeService : ISellingPriceTypeService
{
    private readonly IUnitOfWork _unitOfWork;

    public SellingPriceTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all selling price types
    /// </summary>
    public async Task<IEnumerable<SellingPriceTypeDto>> GetAllAsync()
    {
        var types = await _unitOfWork.SellingPriceTypes.GetAllAsync();
        return types.Select(MapToDto);
    }

    /// <summary>
    /// Gets all active selling price types
    /// </summary>
    public async Task<IEnumerable<SellingPriceTypeDto>> GetActiveAsync()
    {
        var types = await _unitOfWork.SellingPriceTypes.GetActiveAsync();
        return types.Select(MapToDto);
    }

    /// <summary>
    /// Gets a selling price type by ID
    /// </summary>
    /// <param name="id">The ID of the selling price type</param>
    public async Task<SellingPriceTypeDto?> GetByIdAsync(long id)
    {
        var type = await _unitOfWork.SellingPriceTypes.GetByIdAsync((int)id);
        return type != null ? MapToDto(type) : null;
    }

    /// <summary>
    /// Creates a new selling price type
    /// </summary>
    /// <param name="dto">The selling price type data</param>
    public async Task<SellingPriceTypeDto> CreateAsync(CreateSellingPriceTypeDto dto)
    {
        // Validate the type name doesn't already exist
        if (await _unitOfWork.SellingPriceTypes.ExistsAsync(dto.TypeName))
        {
            throw new InvalidOperationException($"A selling price type with the name '{dto.TypeName}' already exists.");
        }

        var entity = new SellingPriceType
        {
            TypeName = dto.TypeName,
            ArabicName = dto.ArabicName,
            Description = dto.Description,
            Status = dto.Status,
            CreatedBy = dto.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        var createdEntity = await _unitOfWork.SellingPriceTypes.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(createdEntity);
    }

    /// <summary>
    /// Updates an existing selling price type
    /// </summary>
    /// <param name="dto">The updated selling price type data</param>
    public async Task<SellingPriceTypeDto> UpdateAsync(UpdateSellingPriceTypeDto dto)
    {
        var entity = await _unitOfWork.SellingPriceTypes.GetByIdAsync((int)dto.Id);
        if (entity == null)
        {
            throw new ArgumentException($"Selling price type with ID {dto.Id} not found.", nameof(dto.Id));
        }

        // Validate the type name doesn't already exist (excluding current entity)
        if (await _unitOfWork.SellingPriceTypes.ExistsAsync(dto.TypeName, dto.Id))
        {
            throw new InvalidOperationException($"A selling price type with the name '{dto.TypeName}' already exists.");
        }

        entity.TypeName = dto.TypeName;
        entity.ArabicName = dto.ArabicName;
        entity.Description = dto.Description;
        entity.Status = dto.Status;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SellingPriceTypes.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(entity);
    }

    /// <summary>
    /// Deletes a selling price type (soft delete)
    /// </summary>
    /// <param name="id">The ID of the selling price type to delete</param>
    /// <param name="deletedBy">ID of the user performing the deletion</param>
    public async Task DeleteAsync(long id, long deletedBy)
    {
        await _unitOfWork.SellingPriceTypes.SoftDeleteAsync(id, deletedBy);
    }

    /// <summary>
    /// Checks if a selling price type name already exists
    /// </summary>
    /// <param name="typeName">The type name to check</param>
    /// <param name="excludeId">ID to exclude from the check (for updates)</param>
    public async Task<bool> ExistsAsync(string typeName, long? excludeId = null)
    {
        return await _unitOfWork.SellingPriceTypes.ExistsAsync(typeName, excludeId);
    }

    /// <summary>
    /// Gets count of selling price types for dashboard
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        return await _unitOfWork.SellingPriceTypes.GetCountAsync();
    }

    /// <summary>
    /// Maps a SellingPriceType entity to a DTO
    /// </summary>
    private static SellingPriceTypeDto MapToDto(SellingPriceType entity)
    {
        return new SellingPriceTypeDto
        {
            Id = entity.Id,
            TypeName = entity.TypeName,
            ArabicName = entity.ArabicName,
            Description = entity.Description,
            Status = entity.Status,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedBy = entity.UpdatedBy,
            UpdatedAt = entity.UpdatedAt
        };
    }
}