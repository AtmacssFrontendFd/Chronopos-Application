using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

public interface ICompanySettingsRepository : IRepository<CompanySettings>
{
    Task<CompanySettings?> GetByCompanyIdAsync(int companyId);
    Task<IEnumerable<CompanySettings>> GetActiveSettingsAsync();
}
