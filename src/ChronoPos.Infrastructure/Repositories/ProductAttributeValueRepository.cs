using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChronoPos.Infrastructure.Repositories
{
    public class ProductAttributeValueRepository : IProductAttributeValueRepository
    {
        private readonly ChronoPosDbContext _context;
        public ProductAttributeValueRepository(ChronoPosDbContext context)
        {
            _context = context;
        }

        public async Task<ProductAttributeValue?> GetByIdAsync(int id)
        {
            return await _context.ProductAttributeValues.Include(v => v.Attribute).FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<List<ProductAttributeValue>> GetByAttributeIdAsync(int attributeId)
        {
            return await _context.ProductAttributeValues.Where(v => v.AttributeId == attributeId).ToListAsync();
        }

        public async Task AddAsync(ProductAttributeValue value)
        {
            _context.ProductAttributeValues.Add(value);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductAttributeValue value)
        {
            // Check if the entity is already being tracked
            var existingEntity = _context.Entry(value);
            if (existingEntity.State == EntityState.Detached)
            {
                // Find any existing tracked entity with the same ID
                var trackedEntity = _context.ProductAttributeValues.Local.FirstOrDefault(e => e.Id == value.Id);
                if (trackedEntity != null)
                {
                    // Update the tracked entity's properties
                    _context.Entry(trackedEntity).CurrentValues.SetValues(value);
                }
                else
                {
                    // No tracked entity, safe to attach and update
                    _context.ProductAttributeValues.Update(value);
                }
            }
            else
            {
                // Entity is already tracked, just mark it as modified
                existingEntity.State = EntityState.Modified;
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ProductAttributeValues.FindAsync(id);
            if (entity != null)
            {
                _context.ProductAttributeValues.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}