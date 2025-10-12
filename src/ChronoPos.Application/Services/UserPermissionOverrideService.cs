using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

public class UserPermissionOverrideService : IUserPermissionOverrideService
{
    private readonly IUserPermissionOverrideRepository _repository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserPermissionOverrideService(
        IUserPermissionOverrideRepository repository,
        IPermissionRepository permissionRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<UserPermissionOverrideDto>> GetAllAsync()
    {
        var overrides = await _repository.GetAllAsync();
        var permissions = await _permissionRepository.GetAllAsync();
        
        return overrides
            .Where(o => o.DeletedAt == null)
            .Select(o => MapToDto(o, permissions.FirstOrDefault(p => p.PermissionId == o.PermissionId)));
    }

    public async Task<UserPermissionOverrideDto?> GetByIdAsync(int id)
    {
        var overrideEntity = await _repository.GetByIdAsync(id);
        if (overrideEntity == null || overrideEntity.DeletedAt != null)
            return null;

        var permission = await _permissionRepository.GetByIdAsync(overrideEntity.PermissionId);
        return MapToDto(overrideEntity, permission);
    }

    public async Task<IEnumerable<UserPermissionOverrideDto>> GetByUserIdAsync(int userId)
    {
        var overrides = await _repository.GetByUserIdAsync(userId);
        var permissions = await _permissionRepository.GetAllAsync();
        
        return overrides
            .Where(o => o.DeletedAt == null)
            .Select(o => MapToDto(o, permissions.FirstOrDefault(p => p.PermissionId == o.PermissionId)));
    }

    public async Task<IEnumerable<UserPermissionOverrideDto>> GetActiveByUserIdAsync(int userId)
    {
        var overrides = await _repository.GetActiveByUserIdAsync(userId);
        var permissions = await _permissionRepository.GetAllAsync();
        
        return overrides
            .Where(o => o.DeletedAt == null && o.IsAllowed &&
                       (!o.ValidFrom.HasValue || o.ValidFrom.Value <= DateTime.UtcNow) &&
                       (!o.ValidTo.HasValue || o.ValidTo.Value >= DateTime.UtcNow))
            .Select(o => MapToDto(o, permissions.FirstOrDefault(p => p.PermissionId == o.PermissionId)));
    }

    public async Task<UserPermissionOverrideDto> CreateAsync(CreateUserPermissionOverrideDto createDto)
    {
        var overrideEntity = new UserPermissionOverride
        {
            UserId = createDto.UserId,
            PermissionId = createDto.PermissionId,
            IsAllowed = createDto.IsAllowed,
            Reason = createDto.Reason,
            ValidFrom = createDto.ValidFrom,
            ValidTo = createDto.ValidTo,
            CreatedBy = createDto.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(overrideEntity);
        await _unitOfWork.SaveChangesAsync();

        var permission = await _permissionRepository.GetByIdAsync(createDto.PermissionId);
        return MapToDto(overrideEntity, permission);
    }

    public async Task<IEnumerable<UserPermissionOverrideDto>> CreateBulkAsync(IEnumerable<CreateUserPermissionOverrideDto> createDtos)
    {
        var results = new List<UserPermissionOverrideDto>();
        var permissions = await _permissionRepository.GetAllAsync();

        foreach (var createDto in createDtos)
        {
            var overrideEntity = new UserPermissionOverride
            {
                UserId = createDto.UserId,
                PermissionId = createDto.PermissionId,
                IsAllowed = createDto.IsAllowed,
                Reason = createDto.Reason,
                ValidFrom = createDto.ValidFrom,
                ValidTo = createDto.ValidTo,
                CreatedBy = createDto.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(overrideEntity);
            
            var permission = permissions.FirstOrDefault(p => p.PermissionId == createDto.PermissionId);
            results.Add(MapToDto(overrideEntity, permission));
        }

        await _unitOfWork.SaveChangesAsync();
        return results;
    }

    public async Task<UserPermissionOverrideDto> UpdateAsync(int id, UpdateUserPermissionOverrideDto updateDto)
    {
        var overrideEntity = await _repository.GetByIdAsync(id);
        if (overrideEntity == null || overrideEntity.DeletedAt != null)
        {
            throw new InvalidOperationException($"Permission override with ID {id} not found.");
        }

        overrideEntity.IsAllowed = updateDto.IsAllowed;
        overrideEntity.Reason = updateDto.Reason;
        overrideEntity.ValidFrom = updateDto.ValidFrom;
        overrideEntity.ValidTo = updateDto.ValidTo;
        overrideEntity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(overrideEntity);
        await _unitOfWork.SaveChangesAsync();

        var permission = await _permissionRepository.GetByIdAsync(overrideEntity.PermissionId);
        return MapToDto(overrideEntity, permission);
    }

    public async Task DeleteAsync(int id)
    {
        var overrideEntity = await _repository.GetByIdAsync(id);
        if (overrideEntity == null || overrideEntity.DeletedAt != null)
        {
            throw new InvalidOperationException($"Permission override with ID {id} not found.");
        }

        overrideEntity.DeletedAt = DateTime.UtcNow;
        overrideEntity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(overrideEntity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteByUserIdAsync(int userId)
    {
        await _repository.DeleteByUserIdAsync(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateExpiredOverridesAsync()
    {
        var allOverrides = await _repository.GetAllAsync();
        var expiredOverrides = allOverrides
            .Where(o => o.DeletedAt == null && 
                       o.IsAllowed && 
                       o.ValidTo.HasValue && 
                       o.ValidTo.Value < DateTime.UtcNow)
            .ToList();

        foreach (var expiredOverride in expiredOverrides)
        {
            expiredOverride.IsAllowed = false;
            expiredOverride.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(expiredOverride);
        }

        if (expiredOverrides.Any())
        {
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private UserPermissionOverrideDto MapToDto(UserPermissionOverride entity, Permission? permission)
    {
        return new UserPermissionOverrideDto
        {
            Id = entity.UserPermissionOverrideId,
            UserId = entity.UserId,
            PermissionId = entity.PermissionId,
            PermissionName = permission?.Name,
            IsAllowed = entity.IsAllowed,
            Reason = entity.Reason,
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt
        };
    }
}
