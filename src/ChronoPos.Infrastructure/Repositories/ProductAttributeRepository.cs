using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChronoPos.Infrastructure.Repositories
{
    public class ProductAttributeRepository : IProductAttributeRepository
    {
        private readonly ChronoPosDbContext _context;
        
        public ProductAttributeRepository(ChronoPosDbContext context)
        {
            _context = context;
        }

        public async Task<ProductAttribute?> GetByIdAsync(int id)
        {
            return await _context.ProductAttributes
                .Include(a => a.Values)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<ProductAttribute>> GetAllAsync()
        {
            return await _context.ProductAttributes
                .Include(a => a.Values)
                .ToListAsync();
        }

        public async Task AddAsync(ProductAttribute attribute)
        {
            _context.ProductAttributes.Add(attribute);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductAttribute attribute)
        {
            _context.ProductAttributes.Update(attribute);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ProductAttributes.FindAsync(id);
            if (entity != null)
            {
                _context.ProductAttributes.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}