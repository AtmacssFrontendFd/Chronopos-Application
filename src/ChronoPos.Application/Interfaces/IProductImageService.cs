using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for ProductImage operations
/// </summary>
public interface IProductImageService
{
    /// <summary>
    /// Gets all images for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of product image DTOs</returns>
    Task<IEnumerable<ProductImageDto>> GetByProductIdAsync(int productId);

    /// <summary>
    /// Gets the primary image for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Primary product image DTO if exists</returns>
    Task<ProductImageDto?> GetPrimaryImageAsync(int productId);

    /// <summary>
    /// Gets product image by ID
    /// </summary>
    /// <param name="id">Product image ID</param>
    /// <returns>Product image DTO if found</returns>
    Task<ProductImageDto?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new product image
    /// </summary>
    /// <param name="createImageDto">Product image data</param>
    /// <returns>Created product image DTO</returns>
    Task<ProductImageDto> CreateAsync(CreateProductImageDto createImageDto);

    /// <summary>
    /// Creates multiple product images
    /// </summary>
    /// <param name="createImageDtos">Collection of product image data</param>
    /// <returns>Collection of created product image DTOs</returns>
    Task<IEnumerable<ProductImageDto>> CreateMultipleAsync(IEnumerable<CreateProductImageDto> createImageDtos);

    /// <summary>
    /// Updates an existing product image
    /// </summary>
    /// <param name="id">Product image ID</param>
    /// <param name="updateImageDto">Updated product image data</param>
    /// <returns>Updated product image DTO</returns>
    Task<ProductImageDto> UpdateAsync(int id, UpdateProductImageDto updateImageDto);

    /// <summary>
    /// Deletes a product image
    /// </summary>
    /// <param name="id">Product image ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Deletes all images for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteByProductIdAsync(int productId);

    /// <summary>
    /// Sets a specific image as primary
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageId">Image ID to set as primary</param>
    /// <returns>True if operation was successful</returns>
    Task<bool> SetPrimaryImageAsync(int productId, int imageId);

    /// <summary>
    /// Reorders product images
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageOrders">Dictionary of image ID to new sort order</param>
    /// <returns>True if operation was successful</returns>
    Task<bool> ReorderImagesAsync(int productId, Dictionary<int, int> imageOrders);
}

/// <summary>
/// DTO for creating a new product image
/// </summary>
public class CreateProductImageDto
{
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}

/// <summary>
/// DTO for updating an existing product image
/// </summary>
public class UpdateProductImageDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}
