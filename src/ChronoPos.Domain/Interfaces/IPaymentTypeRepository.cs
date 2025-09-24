using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for PaymentType entity
/// </summary>
public interface IPaymentTypeRepository : IRepository<PaymentType>
{
    /// <summary>
    /// Gets all active payment types
    /// </summary>
    Task<IEnumerable<PaymentType>> GetActiveAsync();

    /// <summary>
    /// Gets a payment type by name
    /// </summary>
    /// <param name="name">The name to search for</param>
    Task<PaymentType?> GetByNameAsync(string name);

    /// <summary>
    /// Gets a payment type by payment code
    /// </summary>
    /// <param name="paymentCode">The payment code to search for</param>
    Task<PaymentType?> GetByPaymentCodeAsync(string paymentCode);

    /// <summary>
    /// Checks if a payment type name already exists
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <param name="excludeId">ID to exclude from the check (for updates)</param>
    Task<bool> ExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// Checks if a payment code already exists
    /// </summary>
    /// <param name="paymentCode">The payment code to check</param>
    /// <param name="excludeId">ID to exclude from the check (for updates)</param>
    Task<bool> PaymentCodeExistsAsync(string paymentCode, int? excludeId = null);

    /// <summary>
    /// Soft delete a payment type
    /// </summary>
    /// <param name="id">ID of the payment type to delete</param>
    /// <param name="deletedBy">ID of the user performing the deletion</param>
    Task SoftDeleteAsync(int id, int deletedBy);
}