using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for ServiceCharge operations
/// </summary>
public class ServiceChargeService : IServiceChargeService
{
    private readonly IServiceChargeRepository _serviceChargeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ServiceChargeService(
        IServiceChargeRepository serviceChargeRepository,
        IUnitOfWork unitOfWork)
    {
        _serviceChargeRepository = serviceChargeRepository ?? throw new ArgumentNullException(nameof(serviceChargeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<ServiceChargeDto>> GetAllAsync()
    {
        var serviceCharges = await _serviceChargeRepository.GetAllAsync();
        return serviceCharges.Select(MapToDto);
    }

    public async Task<IEnumerable<ServiceChargeDto>> GetActiveServiceChargesAsync()
    {
        var serviceCharges = await _serviceChargeRepository.GetActiveServiceChargesAsync();
        return serviceCharges.Select(MapToDto);
    }

    public async Task<IEnumerable<ServiceChargeDto>> GetAutoApplyServiceChargesAsync()
    {
        var serviceCharges = await _serviceChargeRepository.GetAutoApplyServiceChargesAsync();
        return serviceCharges.Select(MapToDto);
    }

    public async Task<ServiceChargeDto?> GetByIdAsync(int id)
    {
        var serviceCharge = await _serviceChargeRepository.GetByIdAsync(id);
        return serviceCharge != null ? MapToDto(serviceCharge) : null;
    }

    public async Task<ServiceChargeDto?> GetByNameAsync(string name)
    {
        var serviceCharge = await _serviceChargeRepository.GetByNameAsync(name);
        return serviceCharge != null ? MapToDto(serviceCharge) : null;
    }

    public async Task<ServiceChargeDto> CreateAsync(CreateServiceChargeDto createServiceChargeDto, int currentUserId)
    {
        // Check if name already exists
        if (await _serviceChargeRepository.NameExistsAsync(createServiceChargeDto.Name))
        {
            throw new InvalidOperationException($"Service charge with name '{createServiceChargeDto.Name}' already exists");
        }

        var serviceCharge = new ServiceCharge
        {
            Name = createServiceChargeDto.Name.Trim(),
            NameArabic = createServiceChargeDto.NameArabic?.Trim(),
            Description = createServiceChargeDto.Description?.Trim(),
            IsPercentage = createServiceChargeDto.IsPercentage,
            Value = createServiceChargeDto.Value,
            TaxTypeId = createServiceChargeDto.TaxTypeId,
            IsActive = createServiceChargeDto.IsActive,
            AutoApply = createServiceChargeDto.AutoApply,
            CreatedBy = currentUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _serviceChargeRepository.AddAsync(serviceCharge);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(serviceCharge);
    }

    public async Task<ServiceChargeDto> UpdateAsync(int id, UpdateServiceChargeDto updateServiceChargeDto, int currentUserId)
    {
        var serviceCharge = await _serviceChargeRepository.GetByIdAsync(id);
        if (serviceCharge == null)
        {
            throw new ArgumentException($"Service charge with ID {id} not found");
        }

        // Check if name already exists (excluding current service charge)
        if (await _serviceChargeRepository.NameExistsAsync(updateServiceChargeDto.Name, id))
        {
            throw new InvalidOperationException($"Service charge with name '{updateServiceChargeDto.Name}' already exists");
        }

        serviceCharge.Name = updateServiceChargeDto.Name.Trim();
        serviceCharge.NameArabic = updateServiceChargeDto.NameArabic?.Trim();
        serviceCharge.Description = updateServiceChargeDto.Description?.Trim();
        serviceCharge.IsPercentage = updateServiceChargeDto.IsPercentage;
        serviceCharge.Value = updateServiceChargeDto.Value;
        serviceCharge.TaxTypeId = updateServiceChargeDto.TaxTypeId;
        serviceCharge.IsActive = updateServiceChargeDto.IsActive;
        serviceCharge.AutoApply = updateServiceChargeDto.AutoApply;
        serviceCharge.UpdatedBy = currentUserId;
        serviceCharge.UpdatedAt = DateTime.UtcNow;

        await _serviceChargeRepository.UpdateAsync(serviceCharge);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(serviceCharge);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var serviceCharge = await _serviceChargeRepository.GetByIdAsync(id);
        if (serviceCharge == null)
        {
            return false;
        }

        await _serviceChargeRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        return await _serviceChargeRepository.NameExistsAsync(name, excludeId);
    }

    public decimal CalculateServiceChargeAmount(ServiceChargeDto serviceCharge, decimal subtotal)
    {
        if (serviceCharge.IsPercentage)
        {
            return subtotal * (serviceCharge.Value / 100);
        }
        return serviceCharge.Value;
    }

    private static ServiceChargeDto MapToDto(ServiceCharge serviceCharge)
    {
        return new ServiceChargeDto
        {
            Id = serviceCharge.Id,
            Name = serviceCharge.Name,
            NameArabic = serviceCharge.NameArabic,
            Description = serviceCharge.Description,
            IsPercentage = serviceCharge.IsPercentage,
            Value = serviceCharge.Value,
            TaxTypeId = serviceCharge.TaxTypeId,
            TaxTypeName = serviceCharge.TaxType?.Name,
            IsActive = serviceCharge.IsActive,
            AutoApply = serviceCharge.AutoApply,
            CreatedAt = serviceCharge.CreatedAt,
            UpdatedAt = serviceCharge.UpdatedAt
        };
    }
}
