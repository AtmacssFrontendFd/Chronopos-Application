using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for Currency entity operations
/// </summary>
public interface ICurrencyRepository : IRepository<Currency>
{
    /// <summary>
    /// Gets currency by code
    /// </summary>
    /// <param name="code">Currency code</param>
    /// <returns>Currency entity if found</returns>
    Task<Currency?> GetByCodeAsync(string code);

    /// <summary>
    /// Gets currency by name
    /// </summary>
    /// <param name="name">Currency name</param>
    /// <returns>Currency entity if found</returns>
    Task<Currency?> GetByNameAsync(string name);

    /// <summary>
    /// Gets the default currency
    /// </summary>
    /// <returns>Default currency entity if found</returns>
    Task<Currency?> GetDefaultCurrencyAsync();

    /// <summary>
    /// Checks if currency code exists (case-insensitive)
    /// </summary>
    /// <param name="code">Currency code to check</param>
    /// <param name="excludeId">Currency ID to exclude from check (for updates)</param>
    /// <returns>True if code exists</returns>
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);

    /// <summary>
    /// Checks if currency name exists (case-insensitive)
    /// </summary>
    /// <param name="name">Currency name to check</param>
    /// <param name="excludeId">Currency ID to exclude from check (for updates)</param>
    /// <returns>True if name exists</returns>
    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// Gets all currencies ordered by name
    /// </summary>
    /// <returns>Collection of currencies</returns>
    Task<IEnumerable<Currency>> GetAllOrderedAsync();

    /// <summary>
    /// Sets a currency as default (and unsets others)
    /// </summary>
    /// <param name="currencyId">Currency ID to set as default</param>
    /// <returns>Task</returns>
    Task SetDefaultCurrencyAsync(int currencyId);
}
