using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

public interface ICompanyService
{
    Task<IEnumerable<CompanyDto>> GetAllAsync();
    Task<IEnumerable<CompanyDto>> GetActiveAsync();
    Task<CompanyDto?> GetByIdAsync(int id);
    Task<CompanyDto?> GetByNameAsync(string name);
    Task<CompanyDto> CreateAsync(CreateCompanyDto createDto, string createdBy);
    Task<CompanyDto> UpdateAsync(int id, UpdateCompanyDto updateDto, string updatedBy);
    Task<bool> DeleteAsync(int id, string deletedBy);
    Task<bool> CompanyExistsAsync(string name, int? excludeId = null);
}
