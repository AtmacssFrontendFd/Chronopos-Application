using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

public interface ICompanyRepository : IRepository<Company>
{
    Task<IEnumerable<Company>> GetActiveCompaniesAsync();
    Task<Company?> GetByNameAsync(string name);
    Task<bool> CompanyExistsAsync(string name, int? excludeId = null);
}
