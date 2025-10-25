using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Infrastructure.Repositories
{
    public class ProductModifierRepository : IProductModifierRepository
    {
        private readonly ChronoPosDbContext _context;

        public ProductModifierRepository(ChronoPosDbContext context)
        {
            _context = context;
        }

        public async Task<ProductModifier?> GetByIdAsync(int id)
        {
            return await _context.ProductModifiers
                .Include(m => m.TaxType)
                .Include(m => m.Creator)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<ProductModifier>> GetAllAsync()
        {
            return await _context.ProductModifiers
                .Include(m => m.TaxType)
                .Include(m => m.Creator)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModifier>> GetActiveAsync()
        {
            return await _context.ProductModifiers
                .Include(m => m.TaxType)
                .Include(m => m.Creator)
                .Where(m => m.Status == "Active")
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModifier>> GetByStatusAsync(string status)
        {
            return await _context.ProductModifiers
                .Include(m => m.TaxType)
                .Include(m => m.Creator)
                .Where(m => m.Status == status)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<ProductModifier?> GetBySkuAsync(string sku)
        {
            return await _context.ProductModifiers
                .Include(m => m.TaxType)
                .FirstOrDefaultAsync(m => m.Sku == sku);
        }

        public async Task<ProductModifier?> GetByBarcodeAsync(string barcode)
        {
            return await _context.ProductModifiers
                .Include(m => m.TaxType)
                .FirstOrDefaultAsync(m => m.Barcode == barcode);
        }

        public async Task<IEnumerable<ProductModifier>> GetByTaxTypeIdAsync(int taxTypeId)
        {
            return await _context.ProductModifiers
                .Include(m => m.TaxType)
                .Where(m => m.TaxTypeId == taxTypeId)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductModifier>> SearchAsync(string searchTerm)
        {
            var search = searchTerm.ToLower();
            return await _context.ProductModifiers
                .Include(m => m.TaxType)
                .Include(m => m.Creator)
                .Where(m => m.Name.ToLower().Contains(search) ||
                           (m.Description != null && m.Description.ToLower().Contains(search)) ||
                           (m.Sku != null && m.Sku.ToLower().Contains(search)) ||
                           (m.Barcode != null && m.Barcode.ToLower().Contains(search)))
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<ProductModifier> AddAsync(ProductModifier modifier)
        {
            modifier.CreatedAt = DateTime.UtcNow;
            modifier.UpdatedAt = DateTime.UtcNow;
            
            _context.ProductModifiers.Add(modifier);
            await _context.SaveChangesAsync();
            
            return await GetByIdAsync(modifier.Id) ?? modifier;
        }

        public async Task<ProductModifier> UpdateAsync(ProductModifier modifier)
        {
            modifier.UpdatedAt = DateTime.UtcNow;
            
            _context.ProductModifiers.Update(modifier);
            await _context.SaveChangesAsync();
            
            return await GetByIdAsync(modifier.Id) ?? modifier;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var modifier = await _context.ProductModifiers.FindAsync(id);
            if (modifier == null)
                return false;

            _context.ProductModifiers.Remove(modifier);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ProductModifiers.AnyAsync(m => m.Id == id);
        }

        public async Task<bool> SkuExistsAsync(string sku, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return false;

            var query = _context.ProductModifiers.Where(m => m.Sku == sku);
            
            if (excludeId.HasValue)
                query = query.Where(m => m.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return false;

            var query = _context.ProductModifiers.Where(m => m.Barcode == barcode);
            
            if (excludeId.HasValue)
                query = query.Where(m => m.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}
