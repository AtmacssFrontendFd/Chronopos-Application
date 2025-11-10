using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service for detecting and managing printers
/// </summary>
public interface IPrinterService
{
    /// <summary>
    /// Get all available printers on the system
    /// </summary>
    Task<List<string>> GetAvailablePrintersAsync();

    /// <summary>
    /// Test if a printer is accessible
    /// </summary>
    Task<bool> TestPrinterAsync(string printerName);

    /// <summary>
    /// Get default printer name
    /// </summary>
    Task<string?> GetDefaultPrinterAsync();

    /// <summary>
    /// Check if printer exists
    /// </summary>
    Task<bool> PrinterExistsAsync(string printerName);
}
