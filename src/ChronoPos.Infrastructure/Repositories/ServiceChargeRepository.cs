using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ServiceCharge entity operations
/// </summary>
public class ServiceChargeRepository : Repository<ServiceCharge>, IServiceChargeRepository
{
    public ServiceChargeRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<ServiceCharge?> GetByNameAsync(string name)
    {
        return await _context.Set<ServiceCharge>()
            .FirstOrDefaultAsync(sc => sc.Name.ToLower() == name.ToLower());
    }

    public async Task<IEnumerable<ServiceCharge>> GetActiveServiceChargesAsync()
    {
        return await _context.Set<ServiceCharge>()
            .Where(sc => sc.IsActive)
            .OrderBy(sc => sc.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceCharge>> GetAutoApplyServiceChargesAsync()
    {
        return await _context.Set<ServiceCharge>()
            .Where(sc => sc.IsActive && sc.AutoApply)
            .OrderBy(sc => sc.Name)
            .ToListAsync();
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.Set<ServiceCharge>()
            .Where(sc => sc.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(sc => sc.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public override async Task<IEnumerable<ServiceCharge>> GetAllAsync()
    {
        return await _context.Set<ServiceCharge>()
            .Include(sc => sc.TaxType)
            .OrderBy(sc => sc.Name)
            .ToListAsync();
    }

    public override async Task<ServiceCharge?> GetByIdAsync(int id)
    {
        return await _context.Set<ServiceCharge>()
            .Include(sc => sc.TaxType)
            .FirstOrDefaultAsync(sc => sc.Id == id);
    }
    
    public void Update(ServiceCharge serviceCharge)
    {
        _context.Set<ServiceCharge>().Update(serviceCharge);
    }
    
    public void Delete(ServiceCharge serviceCharge)
    {
        _context.Set<ServiceCharge>().Remove(serviceCharge);
    }
}
