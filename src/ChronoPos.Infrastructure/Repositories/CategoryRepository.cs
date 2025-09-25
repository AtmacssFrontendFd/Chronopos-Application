using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Category entity
    /// </summary>
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ChronoPosDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Override GetByIdAsync to include navigation properties
        /// </summary>
        public override async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.CategoryTranslations)
                .Include(c => c.CategoryDiscounts)
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Override GetAllAsync to include navigation properties
        /// </summary>
        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.CategoryTranslations)
                .Include(c => c.CategoryDiscounts)
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetByParentIdAsync(int? parentId)
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == parentId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products
                .AnyAsync(p => p.CategoryId == categoryId);
        }

        public async Task<bool> HasSubCategoriesAsync(int categoryId)
        {
            return await _context.Categories
                .AnyAsync(c => c.ParentCategoryId == categoryId);
        }
    }
}