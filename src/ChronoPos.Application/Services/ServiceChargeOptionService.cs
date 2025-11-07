using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for ServiceChargeOption operations
/// </summary>
public class ServiceChargeOptionService : IServiceChargeOptionService
{
    private readonly IServiceChargeOptionRepository _repository;
    private readonly IServiceChargeTypeRepository _typeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ServiceChargeOptionService(
        IServiceChargeOptionRepository repository,
        IServiceChargeTypeRepository typeRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _typeRepository = typeRepository ?? throw new ArgumentNullException(nameof(typeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<ServiceChargeOptionDto>> GetAllAsync()
    {
        var options = await _repository.GetAllAsync();
        return options.Select(MapToDto);
    }

    public async Task<IEnumerable<ServiceChargeOptionDto>> GetActiveOptionsAsync()
    {
        var options = await _repository.GetActiveOptionsAsync();
        return options.Select(MapToDto);
    }

    public async Task<ServiceChargeOptionDto?> GetByIdAsync(int id)
    {
        var option = await _repository.GetByIdAsync(id);
        return option != null ? MapToDto(option) : null;
    }

    public async Task<IEnumerable<ServiceChargeOptionDto>> GetByServiceChargeTypeIdAsync(int serviceChargeTypeId)
    {
        var options = await _repository.GetByServiceChargeTypeIdAsync(serviceChargeTypeId);
        return options.Select(MapToDto);
    }

    public async Task<IEnumerable<ServiceChargeOptionDto>> GetActiveByServiceChargeTypeIdAsync(int serviceChargeTypeId)
    {
        var options = await _repository.GetActiveByServiceChargeTypeIdAsync(serviceChargeTypeId);
        return options.Select(MapToDto);
    }

    public async Task<IEnumerable<ServiceChargeOptionDto>> GetByLanguageIdAsync(int languageId)
    {
        var options = await _repository.GetByLanguageIdAsync(languageId);
        return options.Select(MapToDto);
    }

    public async Task<ServiceChargeOptionDto?> GetByIdWithRelatedAsync(int id)
    {
        var option = await _repository.GetByIdWithRelatedAsync(id);
        return option != null ? MapToDto(option) : null;
    }

    public async Task<ServiceChargeOptionDto> CreateAsync(CreateServiceChargeOptionDto createDto, int? userId = null)
    {
        // Verify service charge type exists
        var type = await _typeRepository.GetByIdAsync(createDto.ServiceChargeTypeId);
        if (type == null)
        {
            throw new ArgumentException($"Service charge type with ID {createDto.ServiceChargeTypeId} not found");
        }

        // Check if option name already exists for this type
        if (await _repository.NameExistsForTypeAsync(createDto.Name, createDto.ServiceChargeTypeId))
        {
            throw new InvalidOperationException($"Option with name '{createDto.Name}' already exists for this service charge type");
        }

        var entity = new ServiceChargeOption
        {
            ServiceChargeTypeId = createDto.ServiceChargeTypeId,
            Name = createDto.Name.Trim(),
            Cost = createDto.Cost,
            Price = createDto.Price,
            LanguageId = createDto.LanguageId,
            Status = createDto.Status,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        // Reload with related data
        entity = await _repository.GetByIdAsync(entity.Id);
        return MapToDto(entity!);
    }

    public async Task<ServiceChargeOptionDto> UpdateAsync(int id, UpdateServiceChargeOptionDto updateDto, int? userId = null)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new ArgumentException($"Service charge option with ID {id} not found");
        }

        // Verify service charge type exists
        var type = await _typeRepository.GetByIdAsync(updateDto.ServiceChargeTypeId);
        if (type == null)
        {
            throw new ArgumentException($"Service charge type with ID {updateDto.ServiceChargeTypeId} not found");
        }

        // Check if option name already exists for this type (excluding current entity)
        if (await _repository.NameExistsForTypeAsync(updateDto.Name, updateDto.ServiceChargeTypeId, id))
        {
            throw new InvalidOperationException($"Option with name '{updateDto.Name}' already exists for this service charge type");
        }

        entity.ServiceChargeTypeId = updateDto.ServiceChargeTypeId;
        entity.Name = updateDto.Name.Trim();
        entity.Cost = updateDto.Cost;
        entity.Price = updateDto.Price;
        entity.LanguageId = updateDto.LanguageId;
        entity.Status = updateDto.Status;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        // Reload with related data
        entity = await _repository.GetByIdAsync(entity.Id);
        return MapToDto(entity!);
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

    public async Task<bool> NameExistsForTypeAsync(string name, int serviceChargeTypeId, int? excludeId = null)
    {
        return await _repository.NameExistsForTypeAsync(name, serviceChargeTypeId, excludeId);
    }

    public async Task<int> DeleteByServiceChargeTypeIdAsync(int serviceChargeTypeId)
    {
        var count = await _repository.DeleteByServiceChargeTypeIdAsync(serviceChargeTypeId);
        await _unitOfWork.SaveChangesAsync();
        return count;
    }

    private ServiceChargeOptionDto MapToDto(ServiceChargeOption entity)
    {
        return new ServiceChargeOptionDto
        {
            Id = entity.Id,
            ServiceChargeTypeId = entity.ServiceChargeTypeId,
            Name = entity.Name,
            Cost = entity.Cost,
            Price = entity.Price,
            LanguageId = entity.LanguageId,
            Status = entity.Status,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedBy = entity.UpdatedBy,
            UpdatedAt = entity.UpdatedAt,
            DeletedBy = entity.DeletedBy,
            DeletedAt = entity.DeletedAt,
            ServiceChargeTypeName = entity.ServiceChargeType?.Name,
            LanguageName = entity.Language?.LanguageName
        };
    }
}
