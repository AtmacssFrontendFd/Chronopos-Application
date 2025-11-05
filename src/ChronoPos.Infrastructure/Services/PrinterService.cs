using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using ChronoPos.Application.Interfaces;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Service for detecting and managing printers
/// </summary>
public class PrinterService : IPrinterService
{
    /// <summary>
    /// Get all available printers on the system
    /// </summary>
    public Task<List<string>> GetAvailablePrintersAsync()
    {
        return Task.Run(() =>
        {
            var printers = new List<string>();
            
            try
            {
                // Get all installed printers (USB, Network, etc.)
                foreach (string printerName in PrinterSettings.InstalledPrinters)
                {
                    printers.Add(printerName);
                }
            }
            catch (Exception ex)
            {
                // Log error but return empty list
                Console.WriteLine($"Error getting printers: {ex.Message}");
            }

            return printers;
        });
    }

    /// <summary>
    /// Test if a printer is accessible
    /// </summary>
    public Task<bool> TestPrinterAsync(string printerName)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return false;

            try
            {
                var printerSettings = new PrinterSettings
                {
                    PrinterName = printerName
                };

                // Check if printer is valid
                return printerSettings.IsValid;
            }
            catch
            {
                return false;
            }
        });
    }

    /// <summary>
    /// Get default printer name
    /// </summary>
    public Task<string?> GetDefaultPrinterAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                var printerSettings = new PrinterSettings();
                return printerSettings.PrinterName;
            }
            catch
            {
                return null;
            }
        });
    }

    /// <summary>
    /// Check if printer exists
    /// </summary>
    public async Task<bool> PrinterExistsAsync(string printerName)
    {
        if (string.IsNullOrWhiteSpace(printerName))
            return false;

        var printers = await GetAvailablePrintersAsync();
        return printers.Any(p => p.Equals(printerName, StringComparison.OrdinalIgnoreCase));
    }
}
