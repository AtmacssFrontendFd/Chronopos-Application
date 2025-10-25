using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Infrastructure.Repositories
{
    public class ProductModifierGroupItemRepository : IProductModifierGroupItemRepository
    {
        private readonly ChronoPosDbContext _context;

        public ProductModifierGroupItemRepository(ChronoPosDbContext context)
        {
            _context = context;
        }

        public async Task<ProductModifierGroupItem?> GetByIdAsync(int id)
        {
            return await _context.ProductModifierGroupItems
                .Include(i => i.Group)
                .Include(i => i.Modifier)
                    .ThenInclude(m => m!.TaxType)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<ProductModifierGroupItem>> GetAllAsync()
        {
            return await _context.ProductModifierGroupItems
                .Include(i => i.Group)
                .Include(i => i.Modifier)
                    .ThenInclude(m => m!.TaxType)
                .OrderBy(i => i.GroupId)
                .ThenBy(i => i.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModifierGroupItem>> GetByGroupIdAsync(int groupId)
        {
            return await _context.ProductModifierGroupItems
                .Include(i => i.Group)
                .Include(i => i.Modifier)
                    .ThenInclude(m => m!.TaxType)
                .Where(i => i.GroupId == groupId)
                .OrderBy(i => i.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModifierGroupItem>> GetByModifierIdAsync(int modifierId)
        {
            return await _context.ProductModifierGroupItems
                .Include(i => i.Group)
                .Include(i => i.Modifier)
                .Where(i => i.ModifierId == modifierId)
                .OrderBy(i => i.GroupId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModifierGroupItem>> GetActiveByGroupIdAsync(int groupId)
        {
            return await _context.ProductModifierGroupItems
                .Include(i => i.Group)
                .Include(i => i.Modifier)
                    .ThenInclude(m => m!.TaxType)
                .Where(i => i.GroupId == groupId && i.Status == "Active")
                .OrderBy(i => i.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModifierGroupItem>> GetDefaultSelectionsByGroupIdAsync(int groupId)
        {
            return await _context.ProductModifierGroupItems
                .Include(i => i.Group)
                .Include(i => i.Modifier)
                    .ThenInclude(m => m!.TaxType)
                .Where(i => i.GroupId == groupId && i.DefaultSelection && i.Status == "Active")
                .OrderBy(i => i.SortOrder)
                .ToListAsync();
        }

        public async Task<ProductModifierGroupItem> AddAsync(ProductModifierGroupItem item)
        {
            item.CreatedAt = DateTime.UtcNow;
            
            _context.ProductModifierGroupItems.Add(item);
            await _context.SaveChangesAsync();
            
            return await GetByIdAsync(item.Id) ?? item;
        }

        public async Task<ProductModifierGroupItem> UpdateAsync(ProductModifierGroupItem item)
        {
            _context.ProductModifierGroupItems.Update(item);
            await _context.SaveChangesAsync();
            
            return await GetByIdAsync(item.Id) ?? item;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.ProductModifierGroupItems.FindAsync(id);
            if (item == null)
                return false;

            _context.ProductModifierGroupItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByGroupIdAsync(int groupId)
        {
            var items = await _context.ProductModifierGroupItems
                .Where(i => i.GroupId == groupId)
                .ToListAsync();

            if (!items.Any())
                return false;

            _context.ProductModifierGroupItems.RemoveRange(items);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByModifierIdAsync(int modifierId)
        {
            var items = await _context.ProductModifierGroupItems
                .Where(i => i.ModifierId == modifierId)
                .ToListAsync();

            if (!items.Any())
                return false;

            _context.ProductModifierGroupItems.RemoveRange(items);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ProductModifierGroupItems.AnyAsync(i => i.Id == id);
        }

        public async Task<bool> ExistsInGroupAsync(int groupId, int modifierId)
        {
            return await _context.ProductModifierGroupItems
                .AnyAsync(i => i.GroupId == groupId && i.ModifierId == modifierId);
        }

        public async Task<int> GetMaxSortOrderAsync(int groupId)
        {
            var items = await _context.ProductModifierGroupItems
                .Where(i => i.GroupId == groupId)
                .ToListAsync();

            return items.Any() ? items.Max(i => i.SortOrder) : 0;
        }
    }
}
