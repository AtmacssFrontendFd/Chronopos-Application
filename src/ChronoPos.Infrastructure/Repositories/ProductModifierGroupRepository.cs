using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Infrastructure.Repositories
{
    public class ProductModifierGroupRepository : IProductModifierGroupRepository
    {
        private readonly ChronoPosDbContext _context;

        public ProductModifierGroupRepository(ChronoPosDbContext context)
        {
            _context = context;
        }

        public async Task<ProductModifierGroup?> GetByIdAsync(int id)
        {
            return await _context.ProductModifierGroups
                .Include(g => g.Creator)
                .Include(g => g.GroupItems!)
                    .ThenInclude(i => i.Modifier)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<IEnumerable<ProductModifierGroup>> GetAllAsync()
        {
            return await _context.ProductModifierGroups
                .Include(g => g.Creator)
                .Include(g => g.GroupItems)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModifierGroup>> GetActiveAsync()
        {
            return await _context.ProductModifierGroups
                .Include(g => g.Creator)
                .Include(g => g.GroupItems)
                .Where(g => g.Status == "Active")
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModifierGroup>> GetByStatusAsync(string status)
        {
            return await _context.ProductModifierGroups
                .Include(g => g.Creator)
                .Include(g => g.GroupItems)
                .Where(g => g.Status == status)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModifierGroup>> GetBySelectionTypeAsync(string selectionType)
        {
            return await _context.ProductModifierGroups
                .Include(g => g.Creator)
                .Include(g => g.GroupItems)
                .Where(g => g.SelectionType == selectionType)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModifierGroup>> GetRequiredGroupsAsync()
        {
            return await _context.ProductModifierGroups
                .Include(g => g.Creator)
                .Include(g => g.GroupItems)
                .Where(g => g.Required && g.Status == "Active")
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModifierGroup>> SearchAsync(string searchTerm)
        {
            var search = searchTerm.ToLower();
            return await _context.ProductModifierGroups
                .Include(g => g.Creator)
                .Include(g => g.GroupItems)
                .Where(g => g.Name.ToLower().Contains(search) ||
                           (g.Description != null && g.Description.ToLower().Contains(search)))
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<ProductModifierGroup> AddAsync(ProductModifierGroup group)
        {
            group.CreatedAt = DateTime.UtcNow;
            group.UpdatedAt = DateTime.UtcNow;
            
            _context.ProductModifierGroups.Add(group);
            await _context.SaveChangesAsync();
            
            return await GetByIdAsync(group.Id) ?? group;
        }

        public async Task<ProductModifierGroup> UpdateAsync(ProductModifierGroup group)
        {
            group.UpdatedAt = DateTime.UtcNow;
            
            _context.ProductModifierGroups.Update(group);
            await _context.SaveChangesAsync();
            
            return await GetByIdAsync(group.Id) ?? group;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var group = await _context.ProductModifierGroups.FindAsync(id);
            if (group == null)
                return false;

            _context.ProductModifierGroups.Remove(group);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ProductModifierGroups.AnyAsync(g => g.Id == id);
        }
    }
}
