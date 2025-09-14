using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for ProductImage entity operations
/// </summary>
public interface IProductImageRepository : IRepository<ProductImage>
{
    /// <summary>
    /// Gets all images for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of product images ordered by sort order</returns>
    Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId);

    /// <summary>
    /// Gets the primary image for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Primary product image if exists</returns>
    Task<ProductImage?> GetPrimaryImageAsync(int productId);

    /// <summary>
    /// Sets a specific image as primary and clears primary flag from others
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageId">Image ID to set as primary</param>
    /// <returns>True if operation was successful</returns>
    Task<bool> SetPrimaryImageAsync(int productId, int imageId);

    /// <summary>
    /// Deletes all images for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if operation was successful</returns>
    Task<bool> DeleteByProductIdAsync(int productId);

    /// <summary>
    /// Gets the next sort order for a product's images
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Next available sort order</returns>
    Task<int> GetNextSortOrderAsync(int productId);

    /// <summary>
    /// Reorders product images
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageOrders">Dictionary of image ID to new sort order</param>
    /// <returns>True if operation was successful</returns>
    Task<bool> ReorderImagesAsync(int productId, Dictionary<int, int> imageOrders);
}
