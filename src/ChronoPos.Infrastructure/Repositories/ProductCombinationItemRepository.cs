using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ProductCombinationItem entity operations
/// </summary>
public class ProductCombinationItemRepository : Repository<ProductCombinationItem>, IProductCombinationItemRepository
{
    public ProductCombinationItemRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all combination items for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>Collection of combination items</returns>
    public async Task<IEnumerable<ProductCombinationItem>> GetByProductUnitIdAsync(int productUnitId)
    {
        return await _context.Set<ProductCombinationItem>()
            .Where(pci => pci.ProductUnitId == productUnitId)
            .Include(pci => pci.ProductUnit)
            .Include(pci => pci.AttributeValue)
                .ThenInclude(av => av.Attribute)
            .OrderBy(pci => pci.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all combination items for a specific attribute value
    /// </summary>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <returns>Collection of combination items</returns>
    public async Task<IEnumerable<ProductCombinationItem>> GetByAttributeValueIdAsync(int attributeValueId)
    {
        return await _context.Set<ProductCombinationItem>()
            .Where(pci => pci.AttributeValueId == attributeValueId)
            .Include(pci => pci.ProductUnit)
            .Include(pci => pci.AttributeValue)
                .ThenInclude(av => av.Attribute)
            .OrderBy(pci => pci.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets combination item by product unit and attribute value
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <returns>Combination item if found</returns>
    public async Task<ProductCombinationItem?> GetByProductUnitAndAttributeValueAsync(int productUnitId, int attributeValueId)
    {
        return await _context.Set<ProductCombinationItem>()
            .Include(pci => pci.ProductUnit)
            .Include(pci => pci.AttributeValue)
                .ThenInclude(av => av.Attribute)
            .FirstOrDefaultAsync(pci => pci.ProductUnitId == productUnitId && pci.AttributeValueId == attributeValueId);
    }

    /// <summary>
    /// Checks if combination already exists
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <param name="excludeId">ID to exclude from check (for updates)</param>
    /// <returns>True if combination exists</returns>
    public async Task<bool> CombinationExistsAsync(int productUnitId, int attributeValueId, int? excludeId = null)
    {
        var query = _context.Set<ProductCombinationItem>()
            .Where(pci => pci.ProductUnitId == productUnitId && pci.AttributeValueId == attributeValueId);

        if (excludeId.HasValue)
        {
            query = query.Where(pci => pci.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Gets all combination items with navigation properties
    /// </summary>
    /// <returns>Collection of combination items with ProductUnit and AttributeValue loaded</returns>
    public async Task<IEnumerable<ProductCombinationItem>> GetAllWithNavigationAsync()
    {
        return await _context.Set<ProductCombinationItem>()
            .Include(pci => pci.ProductUnit)
            .Include(pci => pci.AttributeValue)
                .ThenInclude(av => av.Attribute)
            .OrderBy(pci => pci.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Deletes all combination items for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>Number of deleted items</returns>
    public async Task<int> DeleteByProductUnitIdAsync(int productUnitId)
    {
        var combinationItems = await _context.Set<ProductCombinationItem>()
            .Where(pci => pci.ProductUnitId == productUnitId)
            .ToListAsync();

        if (combinationItems.Any())
        {
            _context.Set<ProductCombinationItem>().RemoveRange(combinationItems);
            await _context.SaveChangesAsync();
        }

        return combinationItems.Count;
    }

    /// <summary>
    /// Deletes all combination items for a specific attribute value
    /// </summary>
    /// <param name="attributeValueId">Attribute value ID</param>
    /// <returns>Number of deleted items</returns>
    public async Task<int> DeleteByAttributeValueIdAsync(int attributeValueId)
    {
        var combinationItems = await _context.Set<ProductCombinationItem>()
            .Where(pci => pci.AttributeValueId == attributeValueId)
            .ToListAsync();

        if (combinationItems.Any())
        {
            _context.Set<ProductCombinationItem>().RemoveRange(combinationItems);
            await _context.SaveChangesAsync();
        }

        return combinationItems.Count;
    }

    /// <summary>
    /// Gets combination items by multiple product unit IDs
    /// </summary>
    /// <param name="productUnitIds">Collection of product unit IDs</param>
    /// <returns>Collection of combination items</returns>
    public async Task<IEnumerable<ProductCombinationItem>> GetByProductUnitIdsAsync(IEnumerable<int> productUnitIds)
    {
        var productUnitIdList = productUnitIds.ToList();
        if (!productUnitIdList.Any())
        {
            return Enumerable.Empty<ProductCombinationItem>();
        }

        return await _context.Set<ProductCombinationItem>()
            .Where(pci => productUnitIdList.Contains(pci.ProductUnitId))
            .Include(pci => pci.ProductUnit)
            .Include(pci => pci.AttributeValue)
                .ThenInclude(av => av.Attribute)
            .OrderBy(pci => pci.ProductUnitId)
            .ThenBy(pci => pci.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets combination items by multiple attribute value IDs
    /// </summary>
    /// <param name="attributeValueIds">Collection of attribute value IDs</param>
    /// <returns>Collection of combination items</returns>
    public async Task<IEnumerable<ProductCombinationItem>> GetByAttributeValueIdsAsync(IEnumerable<int> attributeValueIds)
    {
        var attributeValueIdList = attributeValueIds.ToList();
        if (!attributeValueIdList.Any())
        {
            return Enumerable.Empty<ProductCombinationItem>();
        }

        return await _context.Set<ProductCombinationItem>()
            .Where(pci => attributeValueIdList.Contains(pci.AttributeValueId))
            .Include(pci => pci.ProductUnit)
            .Include(pci => pci.AttributeValue)
                .ThenInclude(av => av.Attribute)
            .OrderBy(pci => pci.AttributeValueId)
            .ThenBy(pci => pci.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all combination items including related data
    /// </summary>
    /// <returns>Collection of combination items with navigation properties</returns>
    public override async Task<IEnumerable<ProductCombinationItem>> GetAllAsync()
    {
        return await GetAllWithNavigationAsync();
    }

    /// <summary>
    /// Gets combination item by ID including related data
    /// </summary>
    /// <param name="id">Combination item ID</param>
    /// <returns>Combination item with navigation properties</returns>
    public override async Task<ProductCombinationItem?> GetByIdAsync(int id)
    {
        return await _context.Set<ProductCombinationItem>()
            .Include(pci => pci.ProductUnit)
            .Include(pci => pci.AttributeValue)
                .ThenInclude(av => av.Attribute)
            .FirstOrDefaultAsync(pci => pci.Id == id);
    }
}