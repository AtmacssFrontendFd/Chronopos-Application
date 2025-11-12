using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Currency operations
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CurrencyService(ICurrencyRepository currencyRepository, IUnitOfWork unitOfWork)
    {
        _currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all currencies
    /// </summary>
    /// <returns>Collection of currency DTOs</returns>
    public async Task<IEnumerable<CurrencyDto>> GetAllAsync()
    {
        var currencies = await _currencyRepository.GetAllAsync();
        return currencies.Select(MapToDto);
    }

    /// <summary>
    /// Gets all currencies ordered by name
    /// </summary>
    /// <returns>Collection of currency DTOs ordered by name</returns>
    public async Task<IEnumerable<CurrencyDto>> GetAllOrderedAsync()
    {
        var currencies = await _currencyRepository.GetAllOrderedAsync();
        return currencies.Select(MapToDto);
    }

    /// <summary>
    /// Gets currency by ID
    /// </summary>
    /// <param name="id">Currency ID</param>
    /// <returns>Currency DTO if found</returns>
    public async Task<CurrencyDto?> GetByIdAsync(int id)
    {
        var currency = await _currencyRepository.GetByIdAsync(id);
        return currency != null ? MapToDto(currency) : null;
    }

    /// <summary>
    /// Gets currency by code
    /// </summary>
    /// <param name="code">Currency code</param>
    /// <returns>Currency DTO if found</returns>
    public async Task<CurrencyDto?> GetByCodeAsync(string code)
    {
        var currency = await _currencyRepository.GetByCodeAsync(code);
        return currency != null ? MapToDto(currency) : null;
    }

    /// <summary>
    /// Gets currency by name
    /// </summary>
    /// <param name="name">Currency name</param>
    /// <returns>Currency DTO if found</returns>
    public async Task<CurrencyDto?> GetByNameAsync(string name)
    {
        var currency = await _currencyRepository.GetByNameAsync(name);
        return currency != null ? MapToDto(currency) : null;
    }

    /// <summary>
    /// Gets the default currency
    /// </summary>
    /// <returns>Default currency DTO if found</returns>
    public async Task<CurrencyDto?> GetDefaultCurrencyAsync()
    {
        var currency = await _currencyRepository.GetDefaultCurrencyAsync();
        return currency != null ? MapToDto(currency) : null;
    }

    /// <summary>
    /// Creates a new currency
    /// </summary>
    /// <param name="createCurrencyDto">Currency data</param>
    /// <returns>Created currency DTO</returns>
    public async Task<CurrencyDto> CreateAsync(CreateCurrencyDto createCurrencyDto)
    {
        // Check if currency code already exists
        if (await _currencyRepository.CodeExistsAsync(createCurrencyDto.CurrencyCode))
        {
            throw new InvalidOperationException($"Currency with code '{createCurrencyDto.CurrencyCode}' already exists");
        }

        // Check if currency name already exists
        if (await _currencyRepository.NameExistsAsync(createCurrencyDto.CurrencyName))
        {
            throw new InvalidOperationException($"Currency with name '{createCurrencyDto.CurrencyName}' already exists");
        }

        // If this currency is set as default, unset all other defaults
        if (createCurrencyDto.IsDefault)
        {
            var allCurrencies = await _currencyRepository.GetAllAsync();
            foreach (var existingCurrency in allCurrencies)
            {
                if (existingCurrency.IsDefault)
                {
                    existingCurrency.IsDefault = false;
                    existingCurrency.UpdatedAt = DateTime.UtcNow;
                    await _currencyRepository.UpdateAsync(existingCurrency);
                }
            }
        }

        var currency = new Currency
        {
            CurrencyName = createCurrencyDto.CurrencyName.Trim(),
            CurrencyCode = createCurrencyDto.CurrencyCode.Trim().ToUpper(),
            Symbol = createCurrencyDto.Symbol.Trim(),
            ImagePath = createCurrencyDto.ImagePath,
            ExchangeRate = createCurrencyDto.ExchangeRate,
            IsDefault = createCurrencyDto.IsDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _currencyRepository.AddAsync(currency);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(currency);
    }

    /// <summary>
    /// Updates an existing currency
    /// </summary>
    /// <param name="id">Currency ID</param>
    /// <param name="updateCurrencyDto">Updated currency data</param>
    /// <returns>Updated currency DTO</returns>
    public async Task<CurrencyDto> UpdateAsync(int id, UpdateCurrencyDto updateCurrencyDto)
    {
        var currency = await _currencyRepository.GetByIdAsync(id);
        if (currency == null)
        {
            throw new ArgumentException($"Currency with ID {id} not found");
        }

        // Check if currency code already exists (excluding current currency)
        if (await _currencyRepository.CodeExistsAsync(updateCurrencyDto.CurrencyCode, id))
        {
            throw new InvalidOperationException($"Currency with code '{updateCurrencyDto.CurrencyCode}' already exists");
        }

        // Check if currency name already exists (excluding current currency)
        if (await _currencyRepository.NameExistsAsync(updateCurrencyDto.CurrencyName, id))
        {
            throw new InvalidOperationException($"Currency with name '{updateCurrencyDto.CurrencyName}' already exists");
        }

        // If this currency is set as default, unset all other defaults
        if (updateCurrencyDto.IsDefault && !currency.IsDefault)
        {
            await _currencyRepository.SetDefaultCurrencyAsync(id);
        }

        currency.CurrencyName = updateCurrencyDto.CurrencyName.Trim();
        currency.CurrencyCode = updateCurrencyDto.CurrencyCode.Trim().ToUpper();
        currency.Symbol = updateCurrencyDto.Symbol.Trim();
        currency.ImagePath = updateCurrencyDto.ImagePath;
        currency.ExchangeRate = updateCurrencyDto.ExchangeRate;
        currency.IsDefault = updateCurrencyDto.IsDefault;
        currency.UpdatedAt = DateTime.UtcNow;

        await _currencyRepository.UpdateAsync(currency);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(currency);
    }

    /// <summary>
    /// Deletes a currency
    /// </summary>
    /// <param name="id">Currency ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var currency = await _currencyRepository.GetByIdAsync(id);
        if (currency == null)
        {
            return false;
        }

        // Prevent deletion of default currency
        if (currency.IsDefault)
        {
            throw new InvalidOperationException("Cannot delete the default currency. Please set another currency as default first.");
        }

        await _currencyRepository.DeleteAsync(currency.Id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Checks if currency code exists
    /// </summary>
    /// <param name="code">Currency code</param>
    /// <param name="excludeId">Currency ID to exclude from check</param>
    /// <returns>True if code exists</returns>
    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        return await _currencyRepository.CodeExistsAsync(code, excludeId);
    }

    /// <summary>
    /// Checks if currency name exists
    /// </summary>
    /// <param name="name">Currency name</param>
    /// <param name="excludeId">Currency ID to exclude from check</param>
    /// <returns>True if name exists</returns>
    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        return await _currencyRepository.NameExistsAsync(name, excludeId);
    }

    /// <summary>
    /// Sets a currency as default
    /// </summary>
    /// <param name="currencyId">Currency ID to set as default</param>
    /// <returns>Updated currency DTO</returns>
    public async Task<CurrencyDto> SetDefaultCurrencyAsync(int currencyId)
    {
        var currency = await _currencyRepository.GetByIdAsync(currencyId);
        if (currency == null)
        {
            throw new ArgumentException($"Currency with ID {currencyId} not found");
        }

        await _currencyRepository.SetDefaultCurrencyAsync(currencyId);
        await _unitOfWork.SaveChangesAsync();

        // Reload the currency to get updated state
        currency = await _currencyRepository.GetByIdAsync(currencyId);
        return MapToDto(currency!);
    }

    /// <summary>
    /// Converts amount from one currency to another
    /// </summary>
    /// <param name="amount">Amount to convert</param>
    /// <param name="fromCurrencyCode">Source currency code</param>
    /// <param name="toCurrencyCode">Target currency code</param>
    /// <returns>Converted amount</returns>
    public async Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrencyCode, string toCurrencyCode)
    {
        var fromCurrency = await _currencyRepository.GetByCodeAsync(fromCurrencyCode);
        if (fromCurrency == null)
        {
            throw new ArgumentException($"Currency with code '{fromCurrencyCode}' not found");
        }

        var toCurrency = await _currencyRepository.GetByCodeAsync(toCurrencyCode);
        if (toCurrency == null)
        {
            throw new ArgumentException($"Currency with code '{toCurrencyCode}' not found");
        }

        // Convert: amount * (toCurrency.ExchangeRate / fromCurrency.ExchangeRate)
        // This assumes exchange rates are relative to a base currency
        decimal convertedAmount = amount * (toCurrency.ExchangeRate / fromCurrency.ExchangeRate);
        
        return Math.Round(convertedAmount, 4);
    }

    /// <summary>
    /// Maps Currency entity to CurrencyDto
    /// </summary>
    /// <param name="currency">Currency entity</param>
    /// <returns>Currency DTO</returns>
    private static CurrencyDto MapToDto(Currency currency)
    {
        return new CurrencyDto
        {
            Id = currency.Id,
            CurrencyName = currency.CurrencyName,
            CurrencyCode = currency.CurrencyCode,
            Symbol = currency.Symbol,
            ImagePath = currency.ImagePath,
            ExchangeRate = currency.ExchangeRate,
            IsDefault = currency.IsDefault,
            CreatedAt = currency.CreatedAt,
            UpdatedAt = currency.UpdatedAt
        };
    }
}
