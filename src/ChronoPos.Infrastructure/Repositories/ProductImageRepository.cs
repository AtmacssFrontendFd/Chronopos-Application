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
        return await _context.Set<ProductImage>()
            .Where(pi => pi.ProductId == productId)
            .Include(pi => pi.Product)
            .Include(pi => pi.ProductUnit)
            .OrderBy(pi => pi.SortOrder)
            .ThenBy(pi => pi.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all images for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>Collection of product images ordered by sort order</returns>
    public async Task<IEnumerable<ProductImage>> GetByProductUnitIdAsync(int productUnitId)
    {
        return await _context.Set<ProductImage>()
            .Where(pi => pi.ProductUnitId == productUnitId)
            .Include(pi => pi.Product)
            .Include(pi => pi.ProductUnit)
            .OrderBy(pi => pi.SortOrder)
            .ThenBy(pi => pi.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all images for a specific product group
    /// </summary>
    /// <param name="productGroupId">Product group ID</param>
    /// <returns>Collection of product images ordered by sort order</returns>
    public async Task<IEnumerable<ProductImage>> GetByProductGroupIdAsync(int productGroupId)
    {
        return await _context.Set<ProductImage>()
            .Where(pi => pi.ProductGroupId == productGroupId)
            .Include(pi => pi.Product)
            .Include(pi => pi.ProductUnit)
            .OrderBy(pi => pi.SortOrder)
            .ThenBy(pi => pi.Id)
            .ToListAsync();
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
    /// Gets the primary image for a product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>Primary product unit image if exists</returns>
    public async Task<ProductImage?> GetPrimaryImageByProductUnitAsync(int productUnitId)
    {
        return await _context.Set<ProductImage>()
            .Where(pi => pi.ProductUnitId == productUnitId && pi.IsPrimary)
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
    /// Deletes all images for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>True if operation was successful</returns>
    public async Task<bool> DeleteByProductUnitIdAsync(int productUnitId)
    {
        try
        {
            var productImages = await _context.Set<ProductImage>()
                .Where(pi => pi.ProductUnitId == productUnitId)
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
    /// Deletes all images for a specific product group
    /// </summary>
    /// <param name="productGroupId">Product group ID</param>
    /// <returns>True if operation was successful</returns>
    public async Task<bool> DeleteByProductGroupIdAsync(int productGroupId)
    {
        try
        {
            var productImages = await _context.Set<ProductImage>()
                .Where(pi => pi.ProductGroupId == productGroupId)
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
            .Include(pi => pi.ProductUnit)
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
            .Include(pi => pi.ProductUnit)
            .FirstOrDefaultAsync(pi => pi.Id == id);
    }
}
