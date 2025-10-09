using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Permission operations
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PermissionService(
        IPermissionRepository permissionRepository, 
        IRolePermissionRepository rolePermissionRepository,
        IUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        _rolePermissionRepository = rolePermissionRepository ?? throw new ArgumentNullException(nameof(rolePermissionRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all permissions
    /// </summary>
    /// <returns>Collection of permission DTOs</returns>
    public async Task<IEnumerable<PermissionDto>> GetAllAsync()
    {
        var permissions = await _permissionRepository.GetAllAsync();
        return permissions.Select(MapToDto);
    }

    /// <summary>
    /// Gets all active permissions
    /// </summary>
    /// <returns>Collection of active permission DTOs</returns>
    public async Task<IEnumerable<PermissionDto>> GetActivePermissionsAsync()
    {
        var permissions = await _permissionRepository.GetActivePermissionsAsync();
        return permissions.Select(MapToDto);
    }

    /// <summary>
    /// Gets permission by ID
    /// </summary>
    /// <param name="id">Permission ID</param>
    /// <returns>Permission DTO if found</returns>
    public async Task<PermissionDto?> GetByIdAsync(int id)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        return permission != null ? MapToDto(permission) : null;
    }

    /// <summary>
    /// Gets permission by code
    /// </summary>
    /// <param name="code">Permission code</param>
    /// <returns>Permission DTO if found</returns>
    public async Task<PermissionDto?> GetByCodeAsync(string code)
    {
        var permission = await _permissionRepository.GetByCodeAsync(code);
        return permission != null ? MapToDto(permission) : null;
    }

    /// <summary>
    /// Creates a new permission
    /// </summary>
    /// <param name="createPermissionDto">Permission data</param>
    /// <returns>Created permission DTO</returns>
    public async Task<PermissionDto> CreateAsync(CreatePermissionDto createPermissionDto)
    {
        // Check if code already exists
        if (await _permissionRepository.CodeExistsAsync(createPermissionDto.Code))
        {
            throw new InvalidOperationException($"Permission with code '{createPermissionDto.Code}' already exists");
        }

        var permission = new Permission
        {
            Name = createPermissionDto.Name.Trim(),
            Code = createPermissionDto.Code.Trim().ToUpperInvariant(),
            ScreenName = createPermissionDto.ScreenName?.Trim(),
            TypeMatrix = createPermissionDto.TypeMatrix?.Trim(),
            IsParent = createPermissionDto.IsParent,
            ParentPermissionId = createPermissionDto.ParentPermissionId,
            Status = createPermissionDto.Status ?? "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _permissionRepository.AddAsync(permission);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(permission);
    }

    /// <summary>
    /// Updates an existing permission
    /// </summary>
    /// <param name="id">Permission ID</param>
    /// <param name="updatePermissionDto">Updated permission data</param>
    /// <returns>Updated permission DTO</returns>
    public async Task<PermissionDto> UpdateAsync(int id, UpdatePermissionDto updatePermissionDto)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission == null)
        {
            throw new ArgumentException($"Permission with ID {id} not found");
        }

        // Check if code already exists (excluding current permission)
        if (await _permissionRepository.CodeExistsAsync(updatePermissionDto.Code, id))
        {
            throw new InvalidOperationException($"Permission with code '{updatePermissionDto.Code}' already exists");
        }

        permission.Name = updatePermissionDto.Name.Trim();
        permission.Code = updatePermissionDto.Code.Trim().ToUpperInvariant();
        permission.ScreenName = updatePermissionDto.ScreenName?.Trim();
        permission.TypeMatrix = updatePermissionDto.TypeMatrix?.Trim();
        permission.IsParent = updatePermissionDto.IsParent;
        permission.ParentPermissionId = updatePermissionDto.ParentPermissionId;
        permission.Status = updatePermissionDto.Status ?? "Active";
        permission.UpdatedAt = DateTime.UtcNow;

        await _permissionRepository.UpdateAsync(permission);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(permission);
    }

    /// <summary>
    /// Deletes a permission
    /// </summary>
    /// <param name="id">Permission ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission == null)
        {
            return false;
        }

        // Check if permission has children
        var children = await _permissionRepository.GetChildPermissionsAsync(id);
        if (children.Any())
        {
            throw new InvalidOperationException("Cannot delete permission that has child permissions");
        }

        await _permissionRepository.DeleteAsync(permission.PermissionId);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Checks if permission code exists
    /// </summary>
    /// <param name="code">Permission code</param>
    /// <param name="excludeId">Permission ID to exclude from check</param>
    /// <returns>True if code exists</returns>
    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        return await _permissionRepository.CodeExistsAsync(code, excludeId);
    }

    /// <summary>
    /// Gets parent permissions (permissions with no parent)
    /// </summary>
    /// <returns>Collection of parent permission DTOs</returns>
    public async Task<IEnumerable<PermissionDto>> GetParentPermissionsAsync()
    {
        var permissions = await _permissionRepository.GetParentPermissionsAsync();
        return permissions.Select(MapToDto);
    }

    /// <summary>
    /// Gets child permissions for a specific parent
    /// </summary>
    /// <param name="parentId">Parent permission ID</param>
    /// <returns>Collection of child permission DTOs</returns>
    public async Task<IEnumerable<PermissionDto>> GetChildPermissionsAsync(int parentId)
    {
        var permissions = await _permissionRepository.GetChildPermissionsAsync(parentId);
        return permissions.Select(MapToDto);
    }

    /// <summary>
    /// Gets permissions by screen name
    /// </summary>
    /// <param name="screenName">Screen name</param>
    /// <returns>Collection of permission DTOs</returns>
    public async Task<IEnumerable<PermissionDto>> GetByScreenNameAsync(string screenName)
    {
        var permissions = await _permissionRepository.GetByScreenNameAsync(screenName);
        return permissions.Select(MapToDto);
    }

    /// <summary>
    /// Gets permission with its children
    /// </summary>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>Permission DTO with children</returns>
    public async Task<PermissionDto?> GetPermissionWithChildrenAsync(int permissionId)
    {
        var permission = await _permissionRepository.GetPermissionWithChildrenAsync(permissionId);
        return permission != null ? MapToDto(permission) : null;
    }

    /// <summary>
    /// Gets permissions assigned to a role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>Collection of permission DTOs assigned to the role</returns>
    public async Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(int roleId)
    {
        // Get all role permissions for this role
        var allRolePermissions = await _rolePermissionRepository.GetAllAsync();
        var rolePermissions = allRolePermissions
            .Where(rp => rp.RoleId == roleId && rp.DeletedAt == null)
            .ToList();

        if (!rolePermissions.Any())
            return Enumerable.Empty<PermissionDto>();

        // Get the permission IDs
        var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();

        // Get all permissions and filter by IDs
        var allPermissions = await _permissionRepository.GetAllAsync();
        var permissions = allPermissions
            .Where(p => permissionIds.Contains(p.PermissionId) && p.DeletedAt == null)
            .ToList();

        return permissions.Select(MapToDto);
    }

    /// <summary>
    /// Maps Permission entity to PermissionDto
    /// </summary>
    /// <param name="permission">Permission entity</param>
    /// <returns>Permission DTO</returns>
    private static PermissionDto MapToDto(Permission permission)
    {
        return new PermissionDto
        {
            PermissionId = permission.PermissionId,
            Name = permission.Name,
            Code = permission.Code,
            ScreenName = permission.ScreenName,
            TypeMatrix = permission.TypeMatrix,
            IsParent = permission.IsParent,
            ParentPermissionId = permission.ParentPermissionId,
            ParentPermissionName = permission.ParentPermission?.Name,
            Status = permission.Status,
            ChildPermissionCount = permission.ChildPermissions?.Count ?? 0,
            CreatedAt = permission.CreatedAt,
            UpdatedAt = permission.UpdatedAt
        };
    }
}
