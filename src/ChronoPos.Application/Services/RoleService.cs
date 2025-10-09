using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(
        IRoleRepository roleRepository,
        IRolePermissionRepository rolePermissionRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _rolePermissionRepository = rolePermissionRepository ?? throw new ArgumentNullException(nameof(rolePermissionRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync()
    {
        var roles = await _roleRepository.GetRolesWithPermissionCountAsync();
        
        // Filter out deleted roles and map to DTOs
        return roles
            .Where(r => r.DeletedAt == null)
            .Select(role =>
            {
                var dto = MapToDto(role);
                dto.PermissionCount = role.RolePermissions?.Count ?? 0;
                return dto;
            });
    }

    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null || role.DeletedAt != null)
            return null;

        var dto = MapToDto(role);
        dto.PermissionCount = role.RolePermissions?.Count ?? 0;
        
        return dto;
    }

    public async Task<RoleDto> CreateAsync(CreateRoleDto createDto)
    {
        // Validate role name uniqueness
        if (await RoleNameExistsAsync(createDto.RoleName))
        {
            throw new InvalidOperationException($"Role name '{createDto.RoleName}' already exists.");
        }

        var role = new Role
        {
            RoleName = createDto.RoleName.Trim(),
            Description = createDto.Description?.Trim(),
            Status = createDto.Status,
            CreatedBy = createDto.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _roleRepository.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(role);
    }

    public async Task<RoleDto> UpdateAsync(int id, UpdateRoleDto updateDto)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null || role.DeletedAt != null)
        {
            throw new InvalidOperationException($"Role with ID {id} not found.");
        }

        // Validate role name uniqueness (excluding current role)
        if (await RoleNameExistsAsync(updateDto.RoleName, id))
        {
            throw new InvalidOperationException($"Role name '{updateDto.RoleName}' already exists.");
        }

        role.RoleName = updateDto.RoleName.Trim();
        role.Description = updateDto.Description?.Trim();
        role.Status = updateDto.Status;
        role.UpdatedBy = updateDto.UpdatedBy;
        role.UpdatedAt = DateTime.UtcNow;

        await _roleRepository.UpdateAsync(role);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(role);
    }

    public async Task DeleteAsync(int id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null || role.DeletedAt != null)
        {
            throw new InvalidOperationException($"Role with ID {id} not found.");
        }

        // Soft delete
        role.DeletedAt = DateTime.UtcNow;
        role.UpdatedAt = DateTime.UtcNow;

        await _roleRepository.UpdateAsync(role);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> RoleNameExistsAsync(string roleName, int? excludeRoleId = null)
    {
        return await _roleRepository.NameExistsAsync(roleName, excludeRoleId);
    }

    public async Task AssignPermissionsToRoleAsync(int roleId, IEnumerable<int> permissionIds)
    {
        foreach (var permissionId in permissionIds)
        {
            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _rolePermissionRepository.AddAsync(rolePermission);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemovePermissionsFromRoleAsync(int roleId, IEnumerable<int> permissionIds)
    {
        var allRolePermissions = await _rolePermissionRepository.GetAllAsync();
        var rolePermissionsToRemove = allRolePermissions
            .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId) && rp.DeletedAt == null)
            .ToList();

        foreach (var rp in rolePermissionsToRemove)
        {
            rp.DeletedAt = DateTime.UtcNow;
            rp.UpdatedAt = DateTime.UtcNow;
            await _rolePermissionRepository.UpdateAsync(rp);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SyncRolePermissionsAsync(int roleId, IEnumerable<int> permissionIds)
    {
        // Get all existing role permissions
        var allRolePermissions = await _rolePermissionRepository.GetAllAsync();
        var existingRolePermissions = allRolePermissions
            .Where(rp => rp.RoleId == roleId && rp.DeletedAt == null)
            .ToList();

        var existingPermissionIds = existingRolePermissions.Select(rp => rp.PermissionId).ToList();
        var newPermissionIds = permissionIds.ToList();

        // Remove permissions that are no longer in the list
        var permissionsToRemove = existingPermissionIds.Except(newPermissionIds);
        await RemovePermissionsFromRoleAsync(roleId, permissionsToRemove);

        // Add new permissions
        var permissionsToAdd = newPermissionIds.Except(existingPermissionIds);
        await AssignPermissionsToRoleAsync(roleId, permissionsToAdd);
    }

    private RoleDto MapToDto(Role role)
    {
        return new RoleDto
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Description = role.Description,
            Status = role.Status,
            CreatedBy = role.CreatedBy,
            CreatedAt = role.CreatedAt,
            UpdatedBy = role.UpdatedBy,
            UpdatedAt = role.UpdatedAt,
            DeletedAt = role.DeletedAt,
            DeletedBy = role.DeletedBy
        };
    }
}
