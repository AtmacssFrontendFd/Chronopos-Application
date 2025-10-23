using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service for managing payment types
/// </summary>
public class PaymentTypeService : IPaymentTypeService
{
    private readonly IUnitOfWork _unitOfWork;

    public PaymentTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all payment types
    /// </summary>
    public async Task<IEnumerable<PaymentTypeDto>> GetAllAsync()
    {
        var types = await _unitOfWork.PaymentTypes.GetAllAsync();
        return types.Select(MapToDto);
    }

    /// <summary>
    /// Gets all active payment types
    /// </summary>
    public async Task<IEnumerable<PaymentTypeDto>> GetActiveAsync()
    {
        var types = await _unitOfWork.PaymentTypes.GetActiveAsync();
        return types.Select(MapToDto);
    }

    /// <summary>
    /// Gets a payment type by ID
    /// </summary>
    /// <param name="id">The ID of the payment type</param>
    public async Task<PaymentTypeDto?> GetByIdAsync(int id)
    {
        var type = await _unitOfWork.PaymentTypes.GetByIdAsync(id);
        return type != null ? MapToDto(type) : null;
    }

    /// <summary>
    /// Creates a new payment type
    /// </summary>
    /// <param name="dto">The payment type data</param>
    public async Task<PaymentTypeDto> CreateAsync(CreatePaymentTypeDto dto)
    {
        var entity = new PaymentType
        {
            BusinessId = dto.BusinessId,
            Name = dto.Name,
            PaymentCode = dto.PaymentCode,
            NameAr = dto.NameAr,
            Status = dto.Status,
            ChangeAllowed = dto.ChangeAllowed,
            CustomerRequired = dto.CustomerRequired,
            MarkTransactionAsPaid = dto.MarkTransactionAsPaid,
            ShortcutKey = dto.ShortcutKey,
            IsRefundable = dto.IsRefundable,
            IsSplitAllowed = dto.IsSplitAllowed,
            CreatedBy = dto.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.PaymentTypes.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(entity);
    }

    /// <summary>
    /// Updates an existing payment type
    /// </summary>
    /// <param name="dto">The updated payment type data</param>
    public async Task<PaymentTypeDto> UpdateAsync(UpdatePaymentTypeDto dto)
    {
        var entity = await _unitOfWork.PaymentTypes.GetByIdAsync(dto.Id);
        if (entity == null)
        {
            throw new ArgumentException($"Payment type with ID {dto.Id} not found", nameof(dto.Id));
        }

        entity.BusinessId = dto.BusinessId;
        entity.Name = dto.Name;
        entity.PaymentCode = dto.PaymentCode;
        entity.NameAr = dto.NameAr;
        entity.Status = dto.Status;
        entity.ChangeAllowed = dto.ChangeAllowed;
        entity.CustomerRequired = dto.CustomerRequired;
        entity.MarkTransactionAsPaid = dto.MarkTransactionAsPaid;
        entity.ShortcutKey = dto.ShortcutKey;
        entity.IsRefundable = dto.IsRefundable;
        entity.IsSplitAllowed = dto.IsSplitAllowed;
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.PaymentTypes.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(entity);
    }

    /// <summary>
    /// Deletes a payment type (soft delete)
    /// </summary>
    /// <param name="id">The ID of the payment type to delete</param>
    /// <param name="deletedBy">ID of the user performing the deletion</param>
    public async Task DeleteAsync(int id, int deletedBy)
    {
        var entity = await _unitOfWork.PaymentTypes.GetByIdAsync(id);
        if (entity == null)
        {
            throw new ArgumentException($"Payment type with ID {id} not found", nameof(id));
        }

        entity.DeletedBy = deletedBy;
        entity.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.PaymentTypes.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Checks if a payment type name already exists
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <param name="excludeId">ID to exclude from the check (for updates)</param>
    public async Task<bool> ExistsAsync(string name, int? excludeId = null)
    {
        var types = await _unitOfWork.PaymentTypes.GetAllAsync();
        return types.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && 
                             t.Id != excludeId && 
                             t.DeletedAt == null);
    }

    /// <summary>
    /// Checks if a payment code already exists
    /// </summary>
    /// <param name="paymentCode">The payment code to check</param>
    /// <param name="excludeId">ID to exclude from the check (for updates)</param>
    public async Task<bool> PaymentCodeExistsAsync(string paymentCode, int? excludeId = null)
    {
        var types = await _unitOfWork.PaymentTypes.GetAllAsync();
        return types.Any(t => t.PaymentCode.Equals(paymentCode, StringComparison.OrdinalIgnoreCase) && 
                             t.Id != excludeId && 
                             t.DeletedAt == null);
    }

    /// <summary>
    /// Gets count of payment types for dashboard
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        var types = await _unitOfWork.PaymentTypes.GetActiveAsync();
        return types.Count();
    }

    /// <summary>
    /// Maps PaymentType entity to DTO
    /// </summary>
    private static PaymentTypeDto MapToDto(PaymentType entity)
    {
        return new PaymentTypeDto
        {
            Id = entity.Id,
            BusinessId = entity.BusinessId,
            Name = entity.Name,
            PaymentCode = entity.PaymentCode,
            NameAr = entity.NameAr,
            Status = entity.Status,
            ChangeAllowed = entity.ChangeAllowed,
            CustomerRequired = entity.CustomerRequired,
            MarkTransactionAsPaid = entity.MarkTransactionAsPaid,
            ShortcutKey = entity.ShortcutKey,
            IsRefundable = entity.IsRefundable,
            IsSplitAllowed = entity.IsSplitAllowed,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedBy = entity.UpdatedBy,
            UpdatedAt = entity.UpdatedAt
        };
    }
}