using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for ServiceChargeType operations
/// </summary>
public class ServiceChargeTypeService : IServiceChargeTypeService
{
    private readonly IServiceChargeTypeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ServiceChargeTypeService(
        IServiceChargeTypeRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<ServiceChargeTypeDto>> GetAllAsync()
    {
        var types = await _repository.GetAllAsync();
        return types.Select(MapToDto);
    }

    public async Task<IEnumerable<ServiceChargeTypeDto>> GetActiveTypesAsync()
    {
        var types = await _repository.GetActiveTypesAsync();
        return types.Select(MapToDto);
    }

    public async Task<ServiceChargeTypeDto?> GetByIdAsync(int id)
    {
        var type = await _repository.GetByIdAsync(id);
        return type != null ? MapToDto(type) : null;
    }

    public async Task<ServiceChargeTypeDto?> GetByCodeAsync(string code)
    {
        var type = await _repository.GetByCodeAsync(code);
        return type != null ? MapToDto(type) : null;
    }

    public async Task<ServiceChargeTypeDto?> GetByNameAsync(string name)
    {
        var type = await _repository.GetByNameAsync(name);
        return type != null ? MapToDto(type) : null;
    }

    public async Task<ServiceChargeTypeDto?> GetDefaultTypeAsync()
    {
        var type = await _repository.GetDefaultTypeAsync();
        return type != null ? MapToDto(type) : null;
    }

    public async Task<IEnumerable<ServiceChargeTypeDto>> GetTypesWithOptionsAsync()
    {
        var types = await _repository.GetTypesWithOptionsAsync();
        return types.Select(MapToDto);
    }

    public async Task<ServiceChargeTypeDto?> GetByIdWithOptionsAsync(int id)
    {
        var type = await _repository.GetByIdWithOptionsAsync(id);
        return type != null ? MapToDto(type) : null;
    }

    public async Task<ServiceChargeTypeDto> CreateAsync(CreateServiceChargeTypeDto createDto, int? userId = null)
    {
        // Check if code already exists
        if (await _repository.CodeExistsAsync(createDto.Code))
        {
            throw new InvalidOperationException($"Service charge type with code '{createDto.Code}' already exists");
        }

        // Check if name already exists
        if (await _repository.NameExistsAsync(createDto.Name))
        {
            throw new InvalidOperationException($"Service charge type with name '{createDto.Name}' already exists");
        }

        var entity = new ServiceChargeType
        {
            Code = createDto.Code.Trim(),
            Name = createDto.Name.Trim(),
            ChargeOptionScope = createDto.ChargeOptionScope?.Trim(),
            IsDefault = createDto.IsDefault,
            Status = createDto.Status,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<ServiceChargeTypeDto> UpdateAsync(int id, UpdateServiceChargeTypeDto updateDto, int? userId = null)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new ArgumentException($"Service charge type with ID {id} not found");
        }

        // Check if code already exists (excluding current entity)
        if (await _repository.CodeExistsAsync(updateDto.Code, id))
        {
            throw new InvalidOperationException($"Service charge type with code '{updateDto.Code}' already exists");
        }

        // Check if name already exists (excluding current entity)
        if (await _repository.NameExistsAsync(updateDto.Name, id))
        {
            throw new InvalidOperationException($"Service charge type with name '{updateDto.Name}' already exists");
        }

        entity.Code = updateDto.Code.Trim();
        entity.Name = updateDto.Name.Trim();
        entity.ChargeOptionScope = updateDto.ChargeOptionScope?.Trim();
        entity.IsDefault = updateDto.IsDefault;
        entity.Status = updateDto.Status;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id, int? userId = null)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            return false;
        }

        // Soft delete
        entity.DeletedBy = userId;
        entity.DeletedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        return await _repository.CodeExistsAsync(code, excludeId);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        return await _repository.NameExistsAsync(name, excludeId);
    }

    public async Task<IEnumerable<ServiceChargeTypeDto>> GetTypesWithOptionsCountAsync()
    {
        var types = await _repository.GetTypesWithOptionsCountAsync();
        return types.Select(MapToDto);
    }

    private ServiceChargeTypeDto MapToDto(ServiceChargeType entity)
    {
        return new ServiceChargeTypeDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            ChargeOptionScope = entity.ChargeOptionScope,
            IsDefault = entity.IsDefault,
            Status = entity.Status,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedBy = entity.UpdatedBy,
            UpdatedAt = entity.UpdatedAt,
            DeletedBy = entity.DeletedBy,
            DeletedAt = entity.DeletedAt,
            OptionsCount = entity.ServiceChargeOptions?.Count ?? 0
        };
    }
}
