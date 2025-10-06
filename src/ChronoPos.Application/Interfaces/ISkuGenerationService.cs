namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service for generating unique SKUs for products and product units
/// </summary>
public interface ISkuGenerationService
{
    /// <summary>
    /// Generates a unique SKU for a product unit based on product information and unit details
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="productName">Product name</param>
    /// <param name="unitId">Unit of measurement ID</param>
    /// <param name="unitName">Unit of measurement name</param>
    /// <param name="qtyInUnit">Quantity in unit</param>
    /// <returns>Generated unique SKU</returns>
    Task<string> GenerateProductUnitSkuAsync(int productId, string productName, long unitId, string unitName, int qtyInUnit);

    /// <summary>
    /// Validates if a SKU is unique across all product units
    /// </summary>
    /// <param name="sku">SKU to validate</param>
    /// <param name="excludeId">Product unit ID to exclude from validation (for updates)</param>
    /// <returns>True if SKU is unique</returns>
    Task<bool> IsSkuUniqueAsync(string sku, int? excludeId = null);

    /// <summary>
    /// Generates a random suffix for SKU uniqueness
    /// </summary>
    /// <returns>Random 4-character suffix</returns>
    string GenerateRandomSuffix();
}