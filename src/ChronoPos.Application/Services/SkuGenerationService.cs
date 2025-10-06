using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service for generating unique SKUs for products and product units
/// </summary>
public class SkuGenerationService : ISkuGenerationService
{
    private readonly IProductUnitRepository _productUnitRepository;
    private readonly ILogger<SkuGenerationService> _logger;
    private readonly Random _random;

    public SkuGenerationService(
        IProductUnitRepository productUnitRepository,
        ILogger<SkuGenerationService> logger)
    {
        _productUnitRepository = productUnitRepository;
        _logger = logger;
        _random = new Random();
    }

    /// <summary>
    /// Generates a unique SKU for a product unit based on product information and unit details
    /// </summary>
    public async Task<string> GenerateProductUnitSkuAsync(int productId, string productName, long unitId, string unitName, int qtyInUnit)
    {
        try
        {
            // Create base SKU pattern: PROD{ProductId}-{UnitCode}-{Qty}
            string basePattern = CreateBaseSku(productId, productName, unitName, qtyInUnit);
            
            // Check if base pattern is unique
            if (await IsSkuUniqueAsync(basePattern))
            {
                _logger.LogInformation("Generated unique SKU using base pattern: {Sku}", basePattern);
                return basePattern;
            }

            // If not unique, add random suffix
            string uniqueSku = await GenerateUniqueSkuWithSuffix(basePattern);
            _logger.LogInformation("Generated unique SKU with suffix: {Sku}", uniqueSku);
            
            return uniqueSku;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SKU for ProductId: {ProductId}, UnitId: {UnitId}", productId, unitId);
            throw;
        }
    }

    /// <summary>
    /// Validates if a SKU is unique across all product units
    /// </summary>
    public async Task<bool> IsSkuUniqueAsync(string sku, int? excludeId = null)
    {
        try
        {
            var existingProductUnit = await _productUnitRepository.GetBySkuAsync(sku);
            
            if (existingProductUnit == null)
                return true;

            // If excludeId is provided, check if it's the same product unit (for updates)
            if (excludeId.HasValue && existingProductUnit.Id == excludeId.Value)
                return true;

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking SKU uniqueness for: {Sku}", sku);
            return false;
        }
    }

    /// <summary>
    /// Generates a random suffix for SKU uniqueness
    /// </summary>
    public string GenerateRandomSuffix()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var suffix = new StringBuilder(4);
        
        for (int i = 0; i < 4; i++)
        {
            suffix.Append(chars[_random.Next(chars.Length)]);
        }
        
        return suffix.ToString();
    }

    #region Private Methods

    /// <summary>
    /// Creates the base SKU pattern
    /// </summary>
    private string CreateBaseSku(int productId, string productName, string unitName, int qtyInUnit)
    {
        // Get first 3 characters of product name (uppercase, letters only)
        string productCode = GetProductCode(productName);
        
        // Get first 2 characters of unit name (uppercase, letters only)
        string unitCode = GetUnitCode(unitName);
        
        // Create pattern: {ProductCode}{ProductId}-{UnitCode}-{Qty}
        return $"{productCode}{productId:D3}-{unitCode}-{qtyInUnit}";
    }

    /// <summary>
    /// Generates unique SKU by adding random suffix
    /// </summary>
    private async Task<string> GenerateUniqueSkuWithSuffix(string basePattern)
    {
        int attempts = 0;
        const int maxAttempts = 50;

        while (attempts < maxAttempts)
        {
            string suffix = GenerateRandomSuffix();
            string candidateSku = $"{basePattern}-{suffix}";

            if (await IsSkuUniqueAsync(candidateSku))
            {
                return candidateSku;
            }

            attempts++;
        }

        // Fallback: use timestamp suffix
        string timestampSuffix = DateTimeOffset.Now.ToString("HHmmss");
        return $"{basePattern}-{timestampSuffix}";
    }

    /// <summary>
    /// Extracts product code from product name
    /// </summary>
    private string GetProductCode(string productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
            return "PRD";

        // Extract letters only and take first 3 characters
        string letters = new string(productName.Where(char.IsLetter).ToArray()).ToUpperInvariant();
        
        if (letters.Length >= 3)
            return letters.Substring(0, 3);
        
        // Pad with 'X' if less than 3 characters
        return letters.PadRight(3, 'X');
    }

    /// <summary>
    /// Extracts unit code from unit name
    /// </summary>
    private string GetUnitCode(string unitName)
    {
        if (string.IsNullOrWhiteSpace(unitName))
            return "UN";

        // Extract letters only and take first 2 characters
        string letters = new string(unitName.Where(char.IsLetter).ToArray()).ToUpperInvariant();
        
        if (letters.Length >= 2)
            return letters.Substring(0, 2);
        
        // Pad with 'X' if less than 2 characters
        return letters.PadRight(2, 'X');
    }

    #endregion
}