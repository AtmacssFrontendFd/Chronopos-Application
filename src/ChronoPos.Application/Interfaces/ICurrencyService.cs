using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Currency operations
/// </summary>
public interface ICurrencyService
{
    /// <summary>
    /// Gets all currencies
    /// </summary>
    /// <returns>Collection of currency DTOs</returns>
    Task<IEnumerable<CurrencyDto>> GetAllAsync();

    /// <summary>
    /// Gets all currencies ordered by name
    /// </summary>
    /// <returns>Collection of currency DTOs ordered by name</returns>
    Task<IEnumerable<CurrencyDto>> GetAllOrderedAsync();

    /// <summary>
    /// Gets currency by ID
    /// </summary>
    /// <param name="id">Currency ID</param>
    /// <returns>Currency DTO if found</returns>
    Task<CurrencyDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets currency by code
    /// </summary>
    /// <param name="code">Currency code</param>
    /// <returns>Currency DTO if found</returns>
    Task<CurrencyDto?> GetByCodeAsync(string code);

    /// <summary>
    /// Gets currency by name
    /// </summary>
    /// <param name="name">Currency name</param>
    /// <returns>Currency DTO if found</returns>
    Task<CurrencyDto?> GetByNameAsync(string name);

    /// <summary>
    /// Gets the default currency
    /// </summary>
    /// <returns>Default currency DTO if found</returns>
    Task<CurrencyDto?> GetDefaultCurrencyAsync();

    /// <summary>
    /// Creates a new currency
    /// </summary>
    /// <param name="currencyDto">Currency data</param>
    /// <returns>Created currency DTO</returns>
    Task<CurrencyDto> CreateAsync(CreateCurrencyDto currencyDto);

    /// <summary>
    /// Updates an existing currency
    /// </summary>
    /// <param name="id">Currency ID</param>
    /// <param name="currencyDto">Updated currency data</param>
    /// <returns>Updated currency DTO</returns>
    Task<CurrencyDto> UpdateAsync(int id, UpdateCurrencyDto currencyDto);

    /// <summary>
    /// Deletes a currency
    /// </summary>
    /// <param name="id">Currency ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if currency code exists
    /// </summary>
    /// <param name="code">Currency code</param>
    /// <param name="excludeId">Currency ID to exclude from check</param>
    /// <returns>True if code exists</returns>
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);

    /// <summary>
    /// Checks if currency name exists
    /// </summary>
    /// <param name="name">Currency name</param>
    /// <param name="excludeId">Currency ID to exclude from check</param>
    /// <returns>True if name exists</returns>
    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// Sets a currency as default
    /// </summary>
    /// <param name="currencyId">Currency ID to set as default</param>
    /// <returns>Updated currency DTO</returns>
    Task<CurrencyDto> SetDefaultCurrencyAsync(int currencyId);

    /// <summary>
    /// Converts amount from one currency to another
    /// </summary>
    /// <param name="amount">Amount to convert</param>
    /// <param name="fromCurrencyCode">Source currency code</param>
    /// <param name="toCurrencyCode">Target currency code</param>
    /// <returns>Converted amount</returns>
    Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrencyCode, string toCurrencyCode);
}
