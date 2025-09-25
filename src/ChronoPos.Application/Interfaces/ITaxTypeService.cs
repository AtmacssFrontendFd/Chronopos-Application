using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

public interface ITaxTypeService
{
    Task<IEnumerable<TaxTypeDto>> GetAllAsync();
    Task<IEnumerable<TaxTypeDto>> GetAllTaxTypesAsync();
    Task<TaxTypeDto?> GetByIdAsync(int id);
    Task<TaxTypeDto> CreateTaxTypeAsync(TaxTypeDto taxTypeDto);
    Task<TaxTypeDto> UpdateTaxTypeAsync(TaxTypeDto taxTypeDto);
    Task DeleteTaxTypeAsync(int id);
}
