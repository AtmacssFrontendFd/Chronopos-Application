using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ProductImage entity operations
/// </summary>
public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
{
    public ProductImageRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all images for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of product images ordered by sort order</returns>
    public async Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId)
    {
        Console.WriteLine($"ProductImageRepository.GetByProductIdAsync called for product ID: {productId}");
        
        var query = _context.Set<ProductImage>()
            .Where(pi => pi.ProductId == productId)
            .OrderBy(pi => pi.SortOrder)
            .ThenBy(pi => pi.Id);
            
        Console.WriteLine($"SQL Query for product images: {query}");
        
        var result = await query.ToListAsync();
        
        Console.WriteLine($"ProductImageRepository found {result.Count} images for product ID: {productId}");
        foreach (var img in result)
        {
            Console.WriteLine($"  Found Image ID {img.Id}: ProductId={img.ProductId}, IsPrimary={img.IsPrimary}, SortOrder={img.SortOrder}");
        }
        
        return result;
    }

    /// <summary>
    /// Gets the primary image for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Primary product image if exists</returns>
    public async Task<ProductImage?> GetPrimaryImageAsync(int productId)
    {
        return await _context.Set<ProductImage>()
            .Where(pi => pi.ProductId == productId && pi.IsPrimary)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Sets a specific image as primary and clears primary flag from others
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageId">Image ID to set as primary</param>
    /// <returns>True if operation was successful</returns>
    public async Task<bool> SetPrimaryImageAsync(int productId, int imageId)
    {
        try
        {
            // Clear all primary flags for this product
            var productImages = await _context.Set<ProductImage>()
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();

            foreach (var image in productImages)
            {
                image.IsPrimary = image.Id == imageId;
                image.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Deletes all images for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if operation was successful</returns>
    public async Task<bool> DeleteByProductIdAsync(int productId)
    {
        try
        {
            var productImages = await _context.Set<ProductImage>()
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();

            _context.Set<ProductImage>().RemoveRange(productImages);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the next sort order for a product's images
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Next available sort order</returns>
    public async Task<int> GetNextSortOrderAsync(int productId)
    {
        var maxSortOrder = await _context.Set<ProductImage>()
            .Where(pi => pi.ProductId == productId)
            .MaxAsync(pi => (int?)pi.SortOrder);

        return (maxSortOrder ?? 0) + 1;
    }

    /// <summary>
    /// Reorders product images
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageOrders">Dictionary of image ID to new sort order</param>
    /// <returns>True if operation was successful</returns>
    public async Task<bool> ReorderImagesAsync(int productId, Dictionary<int, int> imageOrders)
    {
        try
        {
            var productImages = await _context.Set<ProductImage>()
                .Where(pi => pi.ProductId == productId && imageOrders.Keys.Contains(pi.Id))
                .ToListAsync();

            foreach (var image in productImages)
            {
                if (imageOrders.TryGetValue(image.Id, out var newSortOrder))
                {
                    image.SortOrder = newSortOrder;
                    image.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets all product images including related data
    /// </summary>
    /// <returns>Collection of product images with navigation properties</returns>
    public override async Task<IEnumerable<ProductImage>> GetAllAsync()
    {
        return await _context.Set<ProductImage>()
            .Include(pi => pi.Product)
            .OrderBy(pi => pi.ProductId)
            .ThenBy(pi => pi.SortOrder)
            .ToListAsync();
    }

    /// <summary>
    /// Gets product image by ID including related data
    /// </summary>
    /// <param name="id">Product image ID</param>
    /// <returns>Product image with navigation properties</returns>
    public override async Task<ProductImage?> GetByIdAsync(int id)
    {
        return await _context.Set<ProductImage>()
            .Include(pi => pi.Product)
            .FirstOrDefaultAsync(pi => pi.Id == id);
    }
}
