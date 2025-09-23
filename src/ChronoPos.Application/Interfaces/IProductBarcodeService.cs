using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for ProductBarcode operations
/// </summary>
public interface IProductBarcodeService
{
    /// <summary>
    /// Gets all barcodes
    /// </summary>
    /// <returns>List of barcode DTOs</returns>
    Task<IEnumerable<ProductBarcodeDto>> GetAllBarcodesAsync();

    /// <summary>
    /// Gets a barcode by ID
    /// </summary>
    /// <param name="id">Barcode ID</param>
    /// <returns>Barcode DTO or null</returns>
    Task<ProductBarcodeDto?> GetBarcodeByIdAsync(int id);

    /// <summary>
    /// Gets all barcodes for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of barcode DTOs</returns>
    Task<IEnumerable<ProductBarcodeDto>> GetBarcodesByProductIdAsync(int productId);

    /// <summary>
    /// Gets all barcodes for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>List of barcode DTOs</returns>
    Task<IEnumerable<ProductBarcodeDto>> GetBarcodesByProductUnitIdAsync(int productUnitId);

    /// <summary>
    /// Gets all barcodes for a specific product group
    /// </summary>
    /// <param name="productGroupId">Product group ID</param>
    /// <returns>List of barcode DTOs</returns>
    Task<IEnumerable<ProductBarcodeDto>> GetBarcodesByProductGroupIdAsync(int productGroupId);

    /// <summary>
    /// Gets a barcode by its value
    /// </summary>
    /// <param name="barcode">Barcode value</param>
    /// <returns>Barcode DTO or null</returns>
    Task<ProductBarcodeDto?> GetBarcodeByValueAsync(string barcode);

    /// <summary>
    /// Creates a new barcode
    /// </summary>
    /// <param name="createDto">Create barcode DTO</param>
    /// <returns>Created barcode DTO</returns>
    Task<ProductBarcodeDto> CreateBarcodeAsync(CreateProductBarcodeDto createDto);

    /// <summary>
    /// Updates an existing barcode
    /// </summary>
    /// <param name="id">Barcode ID</param>
    /// <param name="updateDto">Update barcode DTO</param>
    /// <returns>Updated barcode DTO or null</returns>
    Task<ProductBarcodeDto?> UpdateBarcodeAsync(int id, UpdateProductBarcodeDto updateDto);

    /// <summary>
    /// Deletes a barcode
    /// </summary>
    /// <param name="id">Barcode ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteBarcodeAsync(int id);

    /// <summary>
    /// Deletes all barcodes for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteBarcodesByProductIdAsync(int productId);

    /// <summary>
    /// Deletes all barcodes for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteBarcodesByProductUnitIdAsync(int productUnitId);

    /// <summary>
    /// Deletes all barcodes for a specific product group
    /// </summary>
    /// <param name="productGroupId">Product group ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteBarcodesByProductGroupIdAsync(int productGroupId);

    /// <summary>
    /// Checks if a barcode value already exists
    /// </summary>
    /// <param name="barcode">Barcode value</param>
    /// <param name="excludeId">Optional ID to exclude from check</param>
    /// <returns>True if exists</returns>
    Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null);
}
