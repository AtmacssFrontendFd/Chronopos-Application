using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ProductUnit entity operations
/// </summary>
public class ProductUnitRepository : Repository<ProductUnit>, IProductUnitRepository
{
    public ProductUnitRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all product units for a specific product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>List of product units</returns>
    public async Task<IEnumerable<ProductUnit>> GetByProductIdAsync(int productId)
    {
        return await _context.ProductUnits
            .Where(pu => pu.ProductId == productId)
            .Include(pu => pu.Unit)
            .OrderBy(pu => pu.IsBase ? 0 : 1) // Base unit first
            .ThenBy(pu => pu.Unit.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the base unit for a specific product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>The base product unit or null if not found</returns>
    public async Task<ProductUnit?> GetBaseUnitByProductIdAsync(int productId)
    {
        return await _context.ProductUnits
            .Include(pu => pu.Unit)
            .FirstOrDefaultAsync(pu => pu.ProductId == productId && pu.IsBase);
    }

    /// <summary>
    /// Gets all non-base units for a specific product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>List of non-base product units</returns>
    public async Task<IEnumerable<ProductUnit>> GetNonBaseUnitsByProductIdAsync(int productId)
    {
        return await _context.ProductUnits
            .Where(pu => pu.ProductId == productId && !pu.IsBase)
            .Include(pu => pu.Unit)
            .OrderBy(pu => pu.Unit.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a product unit by product ID and unit ID
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <param name="unitId">The unit ID</param>
    /// <returns>The product unit or null if not found</returns>
    public async Task<ProductUnit?> GetByProductIdAndUnitIdAsync(int productId, long unitId)
    {
        return await _context.ProductUnits
            .Include(pu => pu.Unit)
            .FirstOrDefaultAsync(pu => pu.ProductId == productId && pu.UnitId == unitId);
    }

    /// <summary>
    /// Deletes all product units for a specific product
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>Task</returns>
    public async Task DeleteByProductIdAsync(int productId)
    {
        var productUnits = await _context.ProductUnits
            .Where(pu => pu.ProductId == productId)
            .ToListAsync();

        if (productUnits.Any())
        {
            _context.ProductUnits.RemoveRange(productUnits);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Updates the base unit designation for a product (ensures only one base unit exists)
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <param name="newBaseUnitId">The new base unit ID</param>
    /// <returns>Task</returns>
    public async Task UpdateBaseUnitAsync(int productId, long newBaseUnitId)
    {
        // First, clear all existing base unit flags for this product
        var existingProductUnits = await _context.ProductUnits
            .Where(pu => pu.ProductId == productId)
            .ToListAsync();

        foreach (var unit in existingProductUnits)
        {
            unit.IsBase = false;
            unit.UpdatedAt = DateTime.UtcNow;
        }

        // Set the new base unit
        var newBaseUnit = existingProductUnits.FirstOrDefault(pu => pu.UnitId == newBaseUnitId);
        if (newBaseUnit != null)
        {
            newBaseUnit.IsBase = true;
            newBaseUnit.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets product units with their unit details
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>List of product units with unit information</returns>
    public async Task<IEnumerable<ProductUnit>> GetByProductIdWithUnitsAsync(int productId)
    {
        return await _context.ProductUnits
            .Where(pu => pu.ProductId == productId)
            .Include(pu => pu.Unit)
            .Include(pu => pu.Product)
            .OrderBy(pu => pu.IsBase ? 0 : 1) // Base unit first
            .ThenBy(pu => pu.Unit.Name)
            .ToListAsync();
    }
}