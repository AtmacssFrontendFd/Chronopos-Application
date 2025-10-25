using Microsoft.EntityFrameworkCore;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Infrastructure.Repositories;

public class ProductModifierLinkRepository : IProductModifierLinkRepository
{
    private readonly ChronoPosDbContext _context;

    public ProductModifierLinkRepository(ChronoPosDbContext context)
    {
        _context = context;
    }

    public async Task<ProductModifierLink?> GetByIdAsync(int id)
    {
        return await _context.ProductModifierLinks
            .Include(l => l.Product)
            .Include(l => l.ModifierGroup)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<ProductModifierLink>> GetAllAsync()
    {
        return await _context.ProductModifierLinks
            .Include(l => l.Product)
            .Include(l => l.ModifierGroup)
            .OrderBy(l => l.ProductId)
            .ThenBy(l => l.ModifierGroupId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductModifierLink>> GetByProductIdAsync(int productId)
    {
        return await _context.ProductModifierLinks
            .Include(l => l.Product)
            .Include(l => l.ModifierGroup)
            .Where(l => l.ProductId == productId)
            .OrderBy(l => l.ModifierGroupId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductModifierLink>> GetByModifierGroupIdAsync(int modifierGroupId)
    {
        return await _context.ProductModifierLinks
            .Include(l => l.Product)
            .Include(l => l.ModifierGroup)
            .Where(l => l.ModifierGroupId == modifierGroupId)
            .OrderBy(l => l.ProductId)
            .ToListAsync();
    }

    public async Task<ProductModifierLink?> GetByProductAndGroupAsync(int productId, int modifierGroupId)
    {
        return await _context.ProductModifierLinks
            .Include(l => l.Product)
            .Include(l => l.ModifierGroup)
            .FirstOrDefaultAsync(l => l.ProductId == productId && l.ModifierGroupId == modifierGroupId);
    }

    public async Task<ProductModifierLink> AddAsync(ProductModifierLink link)
    {
        link.CreatedAt = DateTime.UtcNow;
        
        _context.ProductModifierLinks.Add(link);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(link.Id) ?? link;
    }

    public async Task<ProductModifierLink> UpdateAsync(ProductModifierLink link)
    {
        _context.ProductModifierLinks.Update(link);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(link.Id) ?? link;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var link = await _context.ProductModifierLinks.FindAsync(id);
        if (link == null)
            return false;

        _context.ProductModifierLinks.Remove(link);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteByProductIdAsync(int productId)
    {
        var links = await _context.ProductModifierLinks
            .Where(l => l.ProductId == productId)
            .ToListAsync();

        if (!links.Any())
            return false;

        _context.ProductModifierLinks.RemoveRange(links);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteByModifierGroupIdAsync(int modifierGroupId)
    {
        var links = await _context.ProductModifierLinks
            .Where(l => l.ModifierGroupId == modifierGroupId)
            .ToListAsync();

        if (!links.Any())
            return false;

        _context.ProductModifierLinks.RemoveRange(links);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int productId, int modifierGroupId)
    {
        return await _context.ProductModifierLinks
            .AnyAsync(l => l.ProductId == productId && l.ModifierGroupId == modifierGroupId);
    }
}
