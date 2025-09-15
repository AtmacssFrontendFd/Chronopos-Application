using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

public interface ITaxTypeService
{
    Task<IEnumerable<TaxTypeDto>> GetAllAsync();
}
