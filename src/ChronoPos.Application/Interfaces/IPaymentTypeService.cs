using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for managing payment types
/// </summary>
public interface IPaymentTypeService
{
    /// <summary>
    /// Gets all payment types
    /// </summary>
    Task<IEnumerable<PaymentTypeDto>> GetAllAsync();

    /// <summary>
    /// Gets all active payment types
    /// </summary>
    Task<IEnumerable<PaymentTypeDto>> GetActiveAsync();

    /// <summary>
    /// Gets a payment type by ID
    /// </summary>
    /// <param name="id">The ID of the payment type</param>
    Task<PaymentTypeDto?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new payment type
    /// </summary>
    /// <param name="dto">The payment type data</param>
    Task<PaymentTypeDto> CreateAsync(CreatePaymentTypeDto dto);

    /// <summary>
    /// Updates an existing payment type
    /// </summary>
    /// <param name="dto">The updated payment type data</param>
    Task<PaymentTypeDto> UpdateAsync(UpdatePaymentTypeDto dto);

    /// <summary>
    /// Deletes a payment type (soft delete)
    /// </summary>
    /// <param name="id">The ID of the payment type to delete</param>
    /// <param name="deletedBy">ID of the user performing the deletion</param>
    Task DeleteAsync(int id, int deletedBy);

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
    /// Gets count of payment types for dashboard
    /// </summary>
    Task<int> GetCountAsync();
}