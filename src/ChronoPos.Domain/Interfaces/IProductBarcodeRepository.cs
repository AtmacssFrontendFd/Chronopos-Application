using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for ProductBarcode entity operations
/// </summary>
public interface IProductBarcodeRepository : IRepository<ProductBarcode>
{
    /// <summary>
    /// Gets all barcodes for a specific product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>List of barcodes</returns>
    Task<IEnumerable<ProductBarcode>> GetByProductIdAsync(int productId);
    
    /// <summary>
    /// Gets all barcodes for a specific product unit
    /// </summary>
    /// <param name="productUnitId">The product unit ID</param>
    /// <returns>List of barcodes</returns>
    Task<IEnumerable<ProductBarcode>> GetByProductUnitIdAsync(int productUnitId);
    
    /// <summary>
    /// Gets all barcodes for a specific product group
    /// </summary>
    /// <param name="productGroupId">The product group ID</param>
    /// <returns>List of barcodes</returns>
    Task<IEnumerable<ProductBarcode>> GetByProductGroupIdAsync(int productGroupId);
    
    /// <summary>
    /// Gets a barcode by its value
    /// </summary>
    /// <param name="barcode">The barcode value</param>
    /// <returns>The barcode or null if not found</returns>
    Task<ProductBarcode?> GetByBarcodeValueAsync(string barcode);
    
    /// <summary>
    /// Checks if a barcode value already exists
    /// </summary>
    /// <param name="barcode">The barcode value</param>
    /// <param name="excludeId">Optional ID to exclude from the check</param>
    /// <returns>True if exists</returns>
    Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null);
    
    /// <summary>
    /// Deletes all barcodes for a specific product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>Task</returns>
    Task DeleteByProductIdAsync(int productId);
    
    /// <summary>
    /// Deletes all barcodes for a specific product unit
    /// </summary>
    /// <param name="productUnitId">The product unit ID</param>
    /// <returns>Task</returns>
    Task DeleteByProductUnitIdAsync(int productUnitId);
    
    /// <summary>
    /// Deletes all barcodes for a specific product group
    /// </summary>
    /// <param name="productGroupId">The product group ID</param>
    /// <returns>Task</returns>
    Task DeleteByProductGroupIdAsync(int productGroupId);
}
