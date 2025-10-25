using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for ServiceCharge entity operations
/// </summary>
public interface IServiceChargeRepository : IRepository<ServiceCharge>
{
    /// <summary>
    /// Gets service charge by name
    /// </summary>
    Task<ServiceCharge?> GetByNameAsync(string name);
    
    /// <summary>
    /// Gets all active service charges
    /// </summary>
    Task<IEnumerable<ServiceCharge>> GetActiveServiceChargesAsync();
    
    /// <summary>
    /// Gets auto-apply service charges
    /// </summary>
    Task<IEnumerable<ServiceCharge>> GetAutoApplyServiceChargesAsync();
    
    /// <summary>
    /// Checks if service charge name exists
    /// </summary>
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    
    /// <summary>
    /// Updates a service charge
    /// </summary>
    void Update(ServiceCharge serviceCharge);
    
    /// <summary>
    /// Deletes a service charge
    /// </summary>
    void Delete(ServiceCharge serviceCharge);
}
