using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for ServiceCharge operations
/// </summary>
public interface IServiceChargeService
{
    /// <summary>
    /// Gets all service charges
    /// </summary>
    Task<IEnumerable<ServiceChargeDto>> GetAllAsync();
    
    /// <summary>
    /// Gets all active service charges
    /// </summary>
    Task<IEnumerable<ServiceChargeDto>> GetActiveServiceChargesAsync();
    
    /// <summary>
    /// Gets auto-apply service charges
    /// </summary>
    Task<IEnumerable<ServiceChargeDto>> GetAutoApplyServiceChargesAsync();
    
    /// <summary>
    /// Gets service charge by ID
    /// </summary>
    Task<ServiceChargeDto?> GetByIdAsync(int id);
    
    /// <summary>
    /// Gets service charge by name
    /// </summary>
    Task<ServiceChargeDto?> GetByNameAsync(string name);
    
    /// <summary>
    /// Creates a new service charge
    /// </summary>
    Task<ServiceChargeDto> CreateAsync(CreateServiceChargeDto createServiceChargeDto, int currentUserId);
    
    /// <summary>
    /// Updates an existing service charge
    /// </summary>
    Task<ServiceChargeDto> UpdateAsync(int id, UpdateServiceChargeDto updateServiceChargeDto, int currentUserId);
    
    /// <summary>
    /// Deletes a service charge
    /// </summary>
    Task<bool> DeleteAsync(int id);
    
    /// <summary>
    /// Checks if service charge name exists
    /// </summary>
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    
    /// <summary>
    /// Calculates service charge amount for given subtotal
    /// </summary>
    decimal CalculateServiceChargeAmount(ServiceChargeDto serviceCharge, decimal subtotal);
}
