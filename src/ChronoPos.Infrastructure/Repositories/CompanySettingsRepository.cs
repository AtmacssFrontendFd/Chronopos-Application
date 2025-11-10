using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

public class CompanySettingsRepository : Repository<CompanySettings>, ICompanySettingsRepository
{
    public CompanySettingsRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<CompanySettings?> GetByCompanyIdAsync(int companyId)
    {
        return await _context.CompanySettings
            .Include(cs => cs.Company)
            .Include(cs => cs.Currency)
            .Include(cs => cs.InvoiceDefaultLanguage)
            .FirstOrDefaultAsync(cs => cs.CompanyId == companyId && cs.DeletedAt == null);
    }

    public async Task<IEnumerable<CompanySettings>> GetActiveSettingsAsync()
    {
        return await _context.CompanySettings
            .Include(cs => cs.Company)
            .Include(cs => cs.Currency)
            .Include(cs => cs.InvoiceDefaultLanguage)
            .Where(cs => cs.Status == "Active" && cs.DeletedAt == null)
            .OrderBy(cs => cs.Id)
            .ToListAsync();
    }
}
