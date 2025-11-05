using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    public CompanyRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Company>> GetActiveCompaniesAsync()
    {
        return await _context.Companies
            .Where(c => c.Status && c.DeletedAt == null)
            .OrderBy(c => c.CompanyName)
            .ToListAsync();
    }

    public async Task<Company?> GetByNameAsync(string name)
    {
        return await _context.Companies
            .FirstOrDefaultAsync(c => c.CompanyName == name && c.DeletedAt == null);
    }

    public async Task<bool> CompanyExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.Companies
            .Where(c => c.CompanyName == name && c.DeletedAt == null);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
