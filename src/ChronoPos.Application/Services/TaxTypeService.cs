using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

public class TaxTypeService : ITaxTypeService
{
    private readonly IUnitOfWork _unitOfWork;

    public TaxTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TaxTypeDto>> GetAllAsync()
    {
        var types = await _unitOfWork.TaxTypes.GetAllAsync();
        return types.Select(Map);
    }

    public async Task<IEnumerable<TaxTypeDto>> GetAllTaxTypesAsync()
    {
        var types = await _unitOfWork.TaxTypes.GetAllAsync();
        return types.Select(Map);
    }

    public async Task<TaxTypeDto?> GetByIdAsync(int id)
    {
        var taxType = await _unitOfWork.TaxTypes.GetByIdAsync(id);
        return taxType != null ? Map(taxType) : null;
    }

    public async Task<TaxTypeDto> CreateTaxTypeAsync(TaxTypeDto taxTypeDto)
    {
        var taxType = new TaxType
        {
            Name = taxTypeDto.Name,
            Description = taxTypeDto.Description,
            Value = taxTypeDto.Value,
            IsPercentage = taxTypeDto.IsPercentage,
            IncludedInPrice = taxTypeDto.IncludedInPrice,
            AppliesToBuying = taxTypeDto.AppliesToBuying,
            AppliesToSelling = taxTypeDto.AppliesToSelling,
            CalculationOrder = taxTypeDto.CalculationOrder,
            IsActive = taxTypeDto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = null, // TODO: Get from current user context
            UpdatedBy = null  // TODO: Get from current user context
        };

        await _unitOfWork.TaxTypes.AddAsync(taxType);
        await _unitOfWork.SaveChangesAsync();

        return Map(taxType);
    }

    public async Task<TaxTypeDto> UpdateTaxTypeAsync(TaxTypeDto taxTypeDto)
    {
        var existingTaxType = await _unitOfWork.TaxTypes.GetByIdAsync(taxTypeDto.Id);
        if (existingTaxType == null)
        {
            throw new ArgumentException($"Tax type with ID {taxTypeDto.Id} not found.");
        }

        existingTaxType.Name = taxTypeDto.Name;
        existingTaxType.Description = taxTypeDto.Description;
        existingTaxType.Value = taxTypeDto.Value;
        existingTaxType.IsPercentage = taxTypeDto.IsPercentage;
        existingTaxType.IncludedInPrice = taxTypeDto.IncludedInPrice;
        existingTaxType.AppliesToBuying = taxTypeDto.AppliesToBuying;
        existingTaxType.AppliesToSelling = taxTypeDto.AppliesToSelling;
        existingTaxType.CalculationOrder = taxTypeDto.CalculationOrder;
        existingTaxType.IsActive = taxTypeDto.IsActive;
        existingTaxType.UpdatedAt = DateTime.UtcNow;
        existingTaxType.UpdatedBy = null; // TODO: Get from current user context

        await _unitOfWork.TaxTypes.UpdateAsync(existingTaxType);
        await _unitOfWork.SaveChangesAsync();

        return Map(existingTaxType);
    }

    public async Task DeleteTaxTypeAsync(int id)
    {
        var taxType = await _unitOfWork.TaxTypes.GetByIdAsync(id);
        if (taxType == null)
        {
            throw new ArgumentException($"Tax type with ID {id} not found.");
        }

        await _unitOfWork.TaxTypes.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    private static TaxTypeDto Map(TaxType t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        Value = t.Value,
        IsPercentage = t.IsPercentage,
        IncludedInPrice = t.IncludedInPrice,
        AppliesToBuying = t.AppliesToBuying,
        AppliesToSelling = t.AppliesToSelling,
        CalculationOrder = t.CalculationOrder,
        IsActive = t.IsActive
    };
}
