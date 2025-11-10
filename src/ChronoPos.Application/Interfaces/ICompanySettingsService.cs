using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

public interface ICompanySettingsService
{
    Task<IEnumerable<CompanySettingsDto>> GetAllAsync();
    Task<IEnumerable<CompanySettingsDto>> GetActiveAsync();
    Task<CompanySettingsDto?> GetByIdAsync(int id);
    Task<CompanySettingsDto?> GetByCompanyIdAsync(int companyId);
    Task<CompanySettingsDto> CreateAsync(CreateCompanySettingsDto createDto, int createdBy);
    Task<CompanySettingsDto> UpdateAsync(int id, UpdateCompanySettingsDto updateDto, int updatedBy);
    Task<bool> DeleteAsync(int id, int deletedBy);
}
