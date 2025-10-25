using ChronoPos.Application.DTOs;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Service interface for managing and accessing the active currency across the entire application
/// </summary>
public interface IActiveCurrencyService
{
    /// <summary>
    /// Event raised when the active currency changes
    /// </summary>
    event EventHandler<CurrencyDto>? ActiveCurrencyChanged;
    
    /// <summary>
    /// Gets the current active currency
    /// </summary>
    CurrencyDto? ActiveCurrency { get; }
    
    /// <summary>
    /// Gets the currency symbol (e.g., "$", "€", "£")
    /// </summary>
    string CurrencySymbol { get; }
    
    /// <summary>
    /// Gets the currency code (e.g., "USD", "EUR", "GBP")
    /// </summary>
    string CurrencyCode { get; }
    
    /// <summary>
    /// Gets the currency name (e.g., "US Dollar", "Euro")
    /// </summary>
    string CurrencyName { get; }
    
    /// <summary>
    /// Initialize the active currency service by loading the default currency from database
    /// </summary>
    /// <returns>True if initialization was successful</returns>
    Task<bool> InitializeAsync();
    
    /// <summary>
    /// Refresh the active currency from database
    /// Call this after changing the default currency
    /// </summary>
    /// <returns>True if refresh was successful</returns>
    Task<bool> RefreshAsync();
    
    /// <summary>
    /// Format a price value with the active currency symbol
    /// </summary>
    /// <param name="amount">The amount to format</param>
    /// <param name="includeSymbol">Whether to include the currency symbol</param>
    /// <returns>Formatted price string</returns>
    string FormatPrice(decimal amount, bool includeSymbol = true);
    
    /// <summary>
    /// Format a price value with custom format and currency symbol
    /// </summary>
    /// <param name="amount">The amount to format</param>
    /// <param name="format">Custom number format (e.g., "N2", "F2")</param>
    /// <param name="includeSymbol">Whether to include the currency symbol</param>
    /// <returns>Formatted price string</returns>
    string FormatPrice(decimal amount, string format, bool includeSymbol = true);
    
    /// <summary>
    /// Convert amount from base currency (AED) to active currency
    /// </summary>
    /// <param name="amountInBaseCurrency">Amount in AED (base currency)</param>
    /// <returns>Amount in active currency</returns>
    decimal ConvertFromBaseCurrency(decimal amountInBaseCurrency);
    
    /// <summary>
    /// Convert amount from active currency to base currency (AED)
    /// </summary>
    /// <param name="amountInActiveCurrency">Amount in active currency</param>
    /// <returns>Amount in AED (base currency)</returns>
    decimal ConvertToBaseCurrency(decimal amountInActiveCurrency);
    
    /// <summary>
    /// Gets the base currency code (AED - UAE Dirham)
    /// </summary>
    string BaseCurrencyCode { get; }
    
    /// <summary>
    /// Gets the current exchange rate relative to base currency
    /// </summary>
    decimal ExchangeRate { get; }
}
