using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for ProductBarcode operations
/// </summary>
public class ProductBarcodeService : IProductBarcodeService
{
    private readonly IProductBarcodeRepository _productBarcodeRepository;

    public ProductBarcodeService(IProductBarcodeRepository productBarcodeRepository)
    {
        _productBarcodeRepository = productBarcodeRepository;
    }

    /// <summary>
    /// Gets all barcodes
    /// </summary>
    /// <returns>List of barcode DTOs</returns>
    public async Task<IEnumerable<ProductBarcodeDto>> GetAllBarcodesAsync()
    {
        var barcodes = await _productBarcodeRepository.GetAllAsync();
        return barcodes.Select(MapToDto);
    }

    /// <summary>
    /// Gets a barcode by ID
    /// </summary>
    /// <param name="id">Barcode ID</param>
    /// <returns>Barcode DTO or null</returns>
    public async Task<ProductBarcodeDto?> GetBarcodeByIdAsync(int id)
    {
        var barcode = await _productBarcodeRepository.GetByIdAsync(id);
        return barcode == null ? null : MapToDto(barcode);
    }

    /// <summary>
    /// Gets all barcodes for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of barcode DTOs</returns>
    public async Task<IEnumerable<ProductBarcodeDto>> GetBarcodesByProductIdAsync(int productId)
    {
        var barcodes = await _productBarcodeRepository.GetByProductIdAsync(productId);
        return barcodes.Select(MapToDto);
    }

    /// <summary>
    /// Gets all barcodes for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>List of barcode DTOs</returns>
    public async Task<IEnumerable<ProductBarcodeDto>> GetBarcodesByProductUnitIdAsync(int productUnitId)
    {
        var barcodes = await _productBarcodeRepository.GetByProductUnitIdAsync(productUnitId);
        return barcodes.Select(MapToDto);
    }

    /// <summary>
    /// Gets all barcodes for a specific product group
    /// </summary>
    /// <param name="productGroupId">Product group ID</param>
    /// <returns>List of barcode DTOs</returns>
    public async Task<IEnumerable<ProductBarcodeDto>> GetBarcodesByProductGroupIdAsync(int productGroupId)
    {
        var barcodes = await _productBarcodeRepository.GetByProductGroupIdAsync(productGroupId);
        return barcodes.Select(MapToDto);
    }

    /// <summary>
    /// Gets a barcode by its value
    /// </summary>
    /// <param name="barcode">Barcode value</param>
    /// <returns>Barcode DTO or null</returns>
    public async Task<ProductBarcodeDto?> GetBarcodeByValueAsync(string barcode)
    {
        var productBarcode = await _productBarcodeRepository.GetByBarcodeValueAsync(barcode);
        return productBarcode == null ? null : MapToDto(productBarcode);
    }

    /// <summary>
    /// Creates a new barcode
    /// </summary>
    /// <param name="createDto">Create barcode DTO</param>
    /// <returns>Created barcode DTO</returns>
    public async Task<ProductBarcodeDto> CreateBarcodeAsync(CreateProductBarcodeDto createDto)
    {
        // Validate barcode doesn't already exist
        if (!string.IsNullOrWhiteSpace(createDto.Barcode))
        {
            var exists = await _productBarcodeRepository.BarcodeExistsAsync(createDto.Barcode);
            if (exists)
            {
                throw new InvalidOperationException($"Barcode '{createDto.Barcode}' already exists.");
            }
        }

        var barcode = new ProductBarcode
        {
            ProductId = createDto.ProductId,
            ProductUnitId = createDto.ProductUnitId,
            ProductGroupId = createDto.ProductGroupId,
            Barcode = createDto.Barcode,
            BarcodeType = createDto.BarcodeType ?? "ean",
            CreatedAt = DateTime.UtcNow
        };

        var createdBarcode = await _productBarcodeRepository.AddAsync(barcode);
        return MapToDto(createdBarcode);
    }

    /// <summary>
    /// Updates an existing barcode
    /// </summary>
    /// <param name="id">Barcode ID</param>
    /// <param name="updateDto">Update barcode DTO</param>
    /// <returns>Updated barcode DTO or null</returns>
    public async Task<ProductBarcodeDto?> UpdateBarcodeAsync(int id, UpdateProductBarcodeDto updateDto)
    {
        var existingBarcode = await _productBarcodeRepository.GetByIdAsync(id);
        if (existingBarcode == null)
        {
            return null;
        }

        // Validate barcode doesn't already exist (excluding current record)
        if (!string.IsNullOrWhiteSpace(updateDto.Barcode))
        {
            var exists = await _productBarcodeRepository.BarcodeExistsAsync(updateDto.Barcode, id);
            if (exists)
            {
                throw new InvalidOperationException($"Barcode '{updateDto.Barcode}' already exists.");
            }
        }

        // Update properties
        existingBarcode.ProductId = updateDto.ProductId;
        existingBarcode.ProductUnitId = updateDto.ProductUnitId;
        existingBarcode.ProductGroupId = updateDto.ProductGroupId;
        
        if (!string.IsNullOrWhiteSpace(updateDto.Barcode))
            existingBarcode.Barcode = updateDto.Barcode;
        
        if (!string.IsNullOrWhiteSpace(updateDto.BarcodeType))
            existingBarcode.BarcodeType = updateDto.BarcodeType;

        await _productBarcodeRepository.UpdateAsync(existingBarcode);
        return MapToDto(existingBarcode);
    }

    /// <summary>
    /// Deletes a barcode
    /// </summary>
    /// <param name="id">Barcode ID</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteBarcodeAsync(int id)
    {
        try
        {
            await _productBarcodeRepository.DeleteAsync(id);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Deletes all barcodes for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteBarcodesByProductIdAsync(int productId)
    {
        try
        {
            await _productBarcodeRepository.DeleteByProductIdAsync(productId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Deletes all barcodes for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteBarcodesByProductUnitIdAsync(int productUnitId)
    {
        try
        {
            await _productBarcodeRepository.DeleteByProductUnitIdAsync(productUnitId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Deletes all barcodes for a specific product group
    /// </summary>
    /// <param name="productGroupId">Product group ID</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteBarcodesByProductGroupIdAsync(int productGroupId)
    {
        try
        {
            await _productBarcodeRepository.DeleteByProductGroupIdAsync(productGroupId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a barcode value already exists
    /// </summary>
    /// <param name="barcode">Barcode value</param>
    /// <param name="excludeId">Optional ID to exclude from check</param>
    /// <returns>True if exists</returns>
    public async Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null)
    {
        return await _productBarcodeRepository.BarcodeExistsAsync(barcode, excludeId);
    }

    /// <summary>
    /// Maps ProductBarcode entity to DTO
    /// </summary>
    private static ProductBarcodeDto MapToDto(ProductBarcode entity)
    {
        var dto = new ProductBarcodeDto
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            ProductUnitId = entity.ProductUnitId,
            ProductGroupId = entity.ProductGroupId,
            Barcode = entity.Barcode,
            BarcodeType = entity.BarcodeType,
            CreatedAt = entity.CreatedAt,
            IsNew = false
        };

        // Set ProductUnit related properties if available
        if (entity.ProductUnit != null)
        {
            dto.ProductUnitSku = entity.ProductUnit.Sku;
            // You can add more ProductUnit properties here if needed
        }

        return dto;
    }
}
