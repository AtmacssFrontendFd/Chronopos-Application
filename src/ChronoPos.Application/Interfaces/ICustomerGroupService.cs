using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Customer Group operations
/// </summary>
public interface ICustomerGroupService
{
    /// <summary>
    /// Get all customer groups
    /// </summary>
    Task<IEnumerable<CustomerGroupDto>> GetAllAsync();
    
    /// <summary>
    /// Get customer group by ID
    /// </summary>
    Task<CustomerGroupDto?> GetByIdAsync(int id);
    
    /// <summary>
    /// Create a new customer group
    /// </summary>
    Task<CustomerGroupDto> CreateAsync(CreateCustomerGroupDto createDto);
    
    /// <summary>
    /// Update an existing customer group
    /// </summary>
    Task<CustomerGroupDto> UpdateAsync(UpdateCustomerGroupDto updateDto);
    
    /// <summary>
    /// Delete a customer group (soft delete)
    /// </summary>
    Task DeleteAsync(int id);
    
    /// <summary>
    /// Search customer groups by name
    /// </summary>
    Task<IEnumerable<CustomerGroupDto>> SearchAsync(string searchTerm);
    
    /// <summary>
    /// Get active customer groups only
    /// </summary>
    Task<IEnumerable<CustomerGroupDto>> GetActiveAsync();
    
    /// <summary>
    /// Get count of all customer groups
    /// </summary>
    Task<int> GetCountAsync();
    
    /// <summary>
    /// Check if customer group name exists
    /// </summary>
    Task<bool> ExistsAsync(string name, int? excludeId = null);
}