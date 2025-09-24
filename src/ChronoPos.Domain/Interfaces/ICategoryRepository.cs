using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Category entity
    /// </summary>
    public interface ICategoryRepository : IRepository<Category>
    {
        /// <summary>
        /// Gets categories by parent category ID
        /// </summary>
        /// <param name="parentId">Parent category ID, null for root categories</param>
        Task<IEnumerable<Category>> GetByParentIdAsync(int? parentId);
        
        /// <summary>
        /// Gets products by category ID
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(int categoryId);
        
        /// <summary>
        /// Checks if category has products
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        Task<bool> HasProductsAsync(int categoryId);
        
        /// <summary>
        /// Checks if category has subcategories
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        Task<bool> HasSubCategoriesAsync(int categoryId);
    }
}