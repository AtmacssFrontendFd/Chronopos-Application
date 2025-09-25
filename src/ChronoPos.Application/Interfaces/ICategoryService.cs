using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces
{
    /// <summary>
    /// Interface for category management operations
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Gets all categories asynchronously
        /// </summary>
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        
        /// <summary>
        /// Gets a category by ID asynchronously
        /// </summary>
        /// <param name="id">Category ID</param>
        Task<CategoryDto?> GetByIdAsync(int id);
        
        /// <summary>
        /// Gets categories by parent ID asynchronously
        /// </summary>
        /// <param name="parentId">Parent category ID, null for root categories</param>
        Task<IEnumerable<CategoryDto>> GetByParentIdAsync(int? parentId);
        
        /// <summary>
        /// Creates a new category asynchronously
        /// </summary>
        /// <param name="categoryDto">Category data</param>
        Task<CategoryDto> CreateAsync(CategoryDto categoryDto);
        
        /// <summary>
        /// Updates an existing category asynchronously
        /// </summary>
        /// <param name="categoryDto">Category data</param>
        Task<CategoryDto> UpdateAsync(CategoryDto categoryDto);
        
        /// <summary>
        /// Deletes a category asynchronously
        /// </summary>
        /// <param name="id">Category ID</param>
        Task DeleteAsync(int id);
        
        /// <summary>
        /// Checks if a category has products assigned
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        Task<bool> HasProductsAsync(int categoryId);
        
        /// <summary>
        /// Checks if a category has subcategories
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        Task<bool> HasSubCategoriesAsync(int categoryId);
        
        /// <summary>
        /// Gets the hierarchy of categories
        /// </summary>
        Task<IEnumerable<CategoryDto>> GetHierarchyAsync();
    }
}