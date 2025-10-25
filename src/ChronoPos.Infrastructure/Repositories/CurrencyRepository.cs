using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Currency entity operations
/// </summary>
public class CurrencyRepository : Repository<Currency>, ICurrencyRepository
{
    public CurrencyRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets currency by code
    /// </summary>
    /// <param name="code">Currency code</param>
    /// <returns>Currency entity if found</returns>
    public async Task<Currency?> GetByCodeAsync(string code)
    {
        return await _context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.CurrencyCode.ToLower() == code.ToLower());
    }

    /// <summary>
    /// Gets currency by name
    /// </summary>
    /// <param name="name">Currency name</param>
    /// <returns>Currency entity if found</returns>
    public async Task<Currency?> GetByNameAsync(string name)
    {
        return await _context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.CurrencyName.ToLower() == name.ToLower());
    }

    /// <summary>
    /// Gets the default currency
    /// </summary>
    /// <returns>Default currency entity if found</returns>
    public async Task<Currency?> GetDefaultCurrencyAsync()
    {
        return await _context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.IsDefault);
    }

    /// <summary>
    /// Checks if currency code exists (case-insensitive)
    /// </summary>
    /// <param name="code">Currency code to check</param>
    /// <param name="excludeId">Currency ID to exclude from check (for updates)</param>
    /// <returns>True if code exists</returns>
    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        var query = _context.Set<Currency>()
            .Where(c => c.CurrencyCode.ToLower() == code.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Checks if currency name exists (case-insensitive)
    /// </summary>
    /// <param name="name">Currency name to check</param>
    /// <param name="excludeId">Currency ID to exclude from check (for updates)</param>
    /// <returns>True if name exists</returns>
    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.Set<Currency>()
            .Where(c => c.CurrencyName.ToLower() == name.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Gets all currencies ordered by name
    /// </summary>
    /// <returns>Collection of currencies</returns>
    public async Task<IEnumerable<Currency>> GetAllOrderedAsync()
    {
        return await _context.Set<Currency>()
            .OrderBy(c => c.CurrencyName)
            .ToListAsync();
    }

    /// <summary>
    /// Sets a currency as default (and unsets others)
    /// </summary>
    /// <param name="currencyId">Currency ID to set as default</param>
    /// <returns>Task</returns>
    public async Task SetDefaultCurrencyAsync(int currencyId)
    {
        // First, unset all currencies as default
        var allCurrencies = await _context.Set<Currency>().ToListAsync();
        foreach (var currency in allCurrencies)
        {
            currency.IsDefault = false;
        }

        // Then set the specified currency as default
        var targetCurrency = await _context.Set<Currency>().FindAsync(currencyId);
        if (targetCurrency != null)
        {
            targetCurrency.IsDefault = true;
            targetCurrency.UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Gets all currencies
    /// </summary>
    /// <returns>Collection of currencies ordered by name</returns>
    public override async Task<IEnumerable<Currency>> GetAllAsync()
    {
        return await _context.Set<Currency>()
            .OrderBy(c => c.CurrencyName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets currency by ID
    /// </summary>
    /// <param name="id">Currency ID</param>
    /// <returns>Currency entity</returns>
    public override async Task<Currency?> GetByIdAsync(int id)
    {
        return await _context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
