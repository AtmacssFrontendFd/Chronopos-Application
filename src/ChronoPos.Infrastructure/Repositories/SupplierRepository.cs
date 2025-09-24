using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Supplier entities
/// </summary>
public class SupplierRepository : ISupplierRepository
{
    private readonly ChronoPosDbContext _context;
    private readonly DbSet<Supplier> _dbSet;

    public SupplierRepository(ChronoPosDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<Supplier>();
    }

    public async Task<IEnumerable<Supplier>> GetAllAsync()
    {
        return await _dbSet
            .Where(s => s.DeletedAt == null)
            .OrderBy(s => s.CompanyName)
            .ToListAsync();
    }

    public async Task<Supplier?> GetByIdAsync(long id)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.SupplierId == id && s.DeletedAt == null);
    }

    public async Task<Supplier> AddAsync(Supplier supplier)
    {
        if (supplier == null)
            throw new ArgumentNullException(nameof(supplier));

        supplier.CreatedAt = DateTime.UtcNow;
        _dbSet.Add(supplier);
        return supplier;
    }

    public async Task UpdateAsync(Supplier supplier)
    {
        if (supplier == null)
            throw new ArgumentNullException(nameof(supplier));

        supplier.UpdatedAt = DateTime.UtcNow;
        _context.Entry(supplier).State = EntityState.Modified;
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(long id)
    {
        var supplier = await _dbSet.FindAsync(id);
        if (supplier != null)
        {
            supplier.DeletedAt = DateTime.UtcNow;
            supplier.Status = "Inactive";
            _context.Entry(supplier).State = EntityState.Modified;
        }
    }

    public async Task<bool> ExistsAsync(long id)
    {
        return await _dbSet
            .AnyAsync(s => s.SupplierId == id && s.DeletedAt == null);
    }

    public async Task<IEnumerable<Supplier>> GetActiveAsync()
    {
        return await _dbSet
            .Where(s => s.DeletedAt == null && s.Status == "Active")
            .OrderBy(s => s.CompanyName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Supplier>> SearchSuppliersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        var lowerSearchTerm = searchTerm.ToLower();
        
        return await _dbSet
            .Where(s => s.DeletedAt == null &&
                       (s.CompanyName.ToLower().Contains(lowerSearchTerm) ||
                        (s.Email != null && s.Email.ToLower().Contains(lowerSearchTerm)) ||
                        (s.Mobile != null && s.Mobile.Contains(searchTerm)) ||
                        (s.CompanyPhoneNumber != null && s.CompanyPhoneNumber.Contains(searchTerm)) ||
                        (s.OwnerName != null && s.OwnerName.ToLower().Contains(lowerSearchTerm)) ||
                        (s.KeyContactName != null && s.KeyContactName.ToLower().Contains(lowerSearchTerm))))
            .OrderBy(s => s.CompanyName)
            .ToListAsync();
    }

    public async Task<Supplier?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await _dbSet
            .FirstOrDefaultAsync(s => s.Email == email && s.DeletedAt == null);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _dbSet
            .CountAsync(s => s.DeletedAt == null);
    }
}