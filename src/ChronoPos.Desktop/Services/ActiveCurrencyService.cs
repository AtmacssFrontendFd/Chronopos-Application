using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using System.Globalization;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Service implementation for managing and accessing the active currency across the entire application
/// This is a singleton service that caches the active currency and provides formatted price methods
/// </summary>
public class ActiveCurrencyService : IActiveCurrencyService
{
    private readonly ICurrencyService _currencyService;
    private CurrencyDto? _activeCurrency;
    private readonly object _lock = new object();

    /// <summary>
    /// Event raised when the active currency changes
    /// </summary>
    public event EventHandler<CurrencyDto>? ActiveCurrencyChanged;

    /// <summary>
    /// Gets the current active currency
    /// </summary>
    public CurrencyDto? ActiveCurrency
    {
        get
        {
            lock (_lock)
            {
                return _activeCurrency;
            }
        }
        private set
        {
            lock (_lock)
            {
                _activeCurrency = value;
            }
        }
    }

    /// <summary>
    /// Gets the currency symbol (e.g., "$", "€", "£")
    /// </summary>
    public string CurrencySymbol
    {
        get
        {
            lock (_lock)
            {
                return _activeCurrency?.Symbol ?? "$";
            }
        }
    }

    /// <summary>
    /// Gets the currency code (e.g., "USD", "EUR", "GBP")
    /// </summary>
    public string CurrencyCode
    {
        get
        {
            lock (_lock)
            {
                return _activeCurrency?.CurrencyCode ?? "USD";
            }
        }
    }

    /// <summary>
    /// Gets the currency name (e.g., "US Dollar", "Euro")
    /// </summary>
    public string CurrencyName
    {
        get
        {
            lock (_lock)
            {
                return _activeCurrency?.CurrencyName ?? "US Dollar";
            }
        }
    }

    /// <summary>
    /// Gets the base currency code (AED - UAE Dirham)
    /// </summary>
    public string BaseCurrencyCode => "AED";

    /// <summary>
    /// Gets the current exchange rate relative to base currency (AED)
    /// </summary>
    public decimal ExchangeRate
    {
        get
        {
            lock (_lock)
            {
                return _activeCurrency?.ExchangeRate ?? 1.0m;
            }
        }
    }

    public ActiveCurrencyService(ICurrencyService currencyService)
    {
        _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
    }

    /// <summary>
    /// Initialize the active currency service by loading the default currency from database
    /// </summary>
    /// <returns>True if initialization was successful</returns>
    public async Task<bool> InitializeAsync()
    {
        try
        {
            var defaultCurrency = await _currencyService.GetDefaultCurrencyAsync();

            if (defaultCurrency != null)
            {
                ActiveCurrency = defaultCurrency;
                System.Diagnostics.Debug.WriteLine($"[ActiveCurrencyService] Initialized with currency: {defaultCurrency.CurrencyName} ({defaultCurrency.Symbol})");
                return true;
            }
            else
            {
                // No default currency found, try to get the first currency or create a default USD
                var allCurrencies = await _currencyService.GetAllAsync();
                var firstCurrency = allCurrencies.FirstOrDefault();

                if (firstCurrency != null)
                {
                    // Set the first currency as default
                    await _currencyService.SetDefaultCurrencyAsync(firstCurrency.Id);
                    ActiveCurrency = await _currencyService.GetByIdAsync(firstCurrency.Id);
                    System.Diagnostics.Debug.WriteLine($"[ActiveCurrencyService] No default currency found. Set first currency as default: {ActiveCurrency?.CurrencyName}");
                    return true;
                }
                else
                {
                    // No currencies at all - this shouldn't happen with seeded data, but handle it gracefully
                    System.Diagnostics.Debug.WriteLine("[ActiveCurrencyService] WARNING: No currencies found in database!");
                    
                    // Create a default USD currency
                    var createDto = new CreateCurrencyDto
                    {
                        CurrencyName = "US Dollar",
                        CurrencyCode = "USD",
                        Symbol = "$",
                        ExchangeRate = 1.0000m,
                        IsDefault = true
                    };

                    var createdCurrency = await _currencyService.CreateAsync(createDto);
                    ActiveCurrency = createdCurrency;
                    System.Diagnostics.Debug.WriteLine("[ActiveCurrencyService] Created default USD currency");
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ActiveCurrencyService] ERROR initializing: {ex.Message}");
            
            // Fallback to USD defaults
            ActiveCurrency = new CurrencyDto
            {
                Id = 0,
                CurrencyName = "US Dollar",
                CurrencyCode = "USD",
                Symbol = "$",
                ExchangeRate = 1.0000m,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            return false;
        }
    }

    /// <summary>
    /// Refresh the active currency from database
    /// Call this after changing the default currency
    /// </summary>
    /// <returns>True if refresh was successful</returns>
    public async Task<bool> RefreshAsync()
    {
        try
        {
            var defaultCurrency = await _currencyService.GetDefaultCurrencyAsync();

            if (defaultCurrency != null)
            {
                var oldCurrency = ActiveCurrency;
                ActiveCurrency = defaultCurrency;

                // Raise event if currency changed
                if (oldCurrency == null || oldCurrency.Id != defaultCurrency.Id)
                {
                    ActiveCurrencyChanged?.Invoke(this, defaultCurrency);
                    System.Diagnostics.Debug.WriteLine($"[ActiveCurrencyService] Currency changed to: {defaultCurrency.CurrencyName} ({defaultCurrency.Symbol})");
                }

                return true;
            }

            System.Diagnostics.Debug.WriteLine("[ActiveCurrencyService] WARNING: No default currency found during refresh");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ActiveCurrencyService] ERROR refreshing: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Format a price value with the active currency symbol
    /// </summary>
    /// <param name="amount">The amount to format</param>
    /// <param name="includeSymbol">Whether to include the currency symbol</param>
    /// <returns>Formatted price string</returns>
    public string FormatPrice(decimal amount, bool includeSymbol = true)
    {
        return FormatPrice(amount, "N2", includeSymbol);
    }

    /// <summary>
    /// Format a price value with custom format and currency symbol
    /// </summary>
    /// <param name="amount">The amount to format</param>
    /// <param name="format">Custom number format (e.g., "N2", "F2", "C")</param>
    /// <param name="includeSymbol">Whether to include the currency symbol</param>
    /// <returns>Formatted price string</returns>
    public string FormatPrice(decimal amount, string format, bool includeSymbol = true)
    {
        try
        {
            var formattedAmount = amount.ToString(format, CultureInfo.InvariantCulture);

            if (includeSymbol)
            {
                var symbol = CurrencySymbol;
                
                // Check if symbol should be placed before or after the amount
                // Common convention: $, £, € before; others after
                if (symbol == "$" || symbol == "£" || symbol == "€" || symbol == "¥" || symbol == "₹")
                {
                    return $"{symbol}{formattedAmount}";
                }
                else
                {
                    return $"{formattedAmount} {symbol}";
                }
            }

            return formattedAmount;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ActiveCurrencyService] ERROR formatting price: {ex.Message}");
            return includeSymbol ? $"${amount:N2}" : amount.ToString("N2");
        }
    }

    /// <summary>
    /// Returns the same amount without conversion - only currency symbol changes
    /// Note: Currency conversion has been disabled - only symbol display changes
    /// </summary>
    /// <param name="amountInBaseCurrency">Amount to display</param>
    /// <returns>Same amount (no conversion applied)</returns>
    public decimal ConvertFromBaseCurrency(decimal amountInBaseCurrency)
    {
        // No conversion - only symbol changes when switching currencies
        // The prices remain the same, only the display symbol updates
        return amountInBaseCurrency;
    }

    /// <summary>
    /// Returns the same amount without conversion - only currency symbol changes
    /// Note: Currency conversion has been disabled - only symbol display changes
    /// </summary>
    /// <param name="amountInActiveCurrency">Amount to store</param>
    /// <returns>Same amount (no conversion applied)</returns>
    public decimal ConvertToBaseCurrency(decimal amountInActiveCurrency)
    {
        // No conversion - only symbol changes when switching currencies
        // The prices remain the same, only the display symbol updates
        return amountInActiveCurrency;
    }
}
