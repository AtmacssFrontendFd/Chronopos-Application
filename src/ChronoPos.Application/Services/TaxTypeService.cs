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
