using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllAsync();
    Task<RoleDto?> GetByIdAsync(int id);
    Task<RoleDto> CreateAsync(CreateRoleDto createDto);
    Task<RoleDto> UpdateAsync(int id, UpdateRoleDto updateDto);
    Task DeleteAsync(int id);
    Task<bool> RoleNameExistsAsync(string roleName, int? excludeRoleId = null);
    Task AssignPermissionsToRoleAsync(int roleId, IEnumerable<int> permissionIds);
    Task RemovePermissionsFromRoleAsync(int roleId, IEnumerable<int> permissionIds);
    Task SyncRolePermissionsAsync(int roleId, IEnumerable<int> permissionIds);
}
