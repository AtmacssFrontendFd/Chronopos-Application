using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChronoPos.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            IUnitOfWork unitOfWork,
            ILogger<CategoryService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                return categories.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                throw;
            }
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null) return null;

                var categoryDto = MapToDto(category);
                
                // Load discount mappings
                await LoadCategoryDiscountsAsync(categoryDto);
                
                return categoryDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by ID: {CategoryId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<CategoryDto>> GetByParentIdAsync(int? parentId)
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetByParentIdAsync(parentId);
                return categories.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories by parent ID: {ParentId}", parentId);
                throw;
            }
        }

        public async Task<CategoryDto> CreateAsync(CategoryDto categoryDto)
        {
            try
            {
                var category = MapToEntity(categoryDto);
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                // Save Arabic translation if provided
                await SaveCategoryTranslationAsync(category.Id, categoryDto.NameArabic, categoryDto.Description);

                // Save discount mappings if provided
                await SaveCategoryDiscountsAsync(category.Id, categoryDto.SelectedDiscountIds);

                var result = MapToDto(category);
                result.NameArabic = categoryDto.NameArabic;
                result.SelectedDiscountIds = categoryDto.SelectedDiscountIds;
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {CategoryName}", categoryDto.Name);
                throw;
            }
        }

        public async Task<CategoryDto> UpdateAsync(CategoryDto categoryDto)
        {
            try
            {
                var existingCategory = await _unitOfWork.Categories.GetByIdAsync(categoryDto.Id);
                if (existingCategory == null)
                {
                    throw new KeyNotFoundException($"Category with ID {categoryDto.Id} not found");
                }

                // Update properties
                existingCategory.Name = categoryDto.Name;
                existingCategory.Description = categoryDto.Description;
                existingCategory.ParentCategoryId = categoryDto.ParentCategoryId;
                existingCategory.IsActive = categoryDto.IsActive;
                existingCategory.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Categories.UpdateAsync(existingCategory);
                await _unitOfWork.SaveChangesAsync();

                // Update Arabic translation
                await UpdateCategoryTranslationAsync(categoryDto.Id, categoryDto.NameArabic, categoryDto.Description);

                // Update discount mappings
                await SaveCategoryDiscountsAsync(categoryDto.Id, categoryDto.SelectedDiscountIds);

                var result = MapToDto(existingCategory);
                result.NameArabic = categoryDto.NameArabic;
                result.SelectedDiscountIds = categoryDto.SelectedDiscountIds;
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", categoryDto.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    throw new KeyNotFoundException($"Category with ID {id} not found");
                }

                // Check if category has products
                var hasProducts = await HasProductsAsync(id);
                if (hasProducts)
                {
                    throw new InvalidOperationException($"Cannot delete category {id} because it has products assigned to it");
                }

                // Delete subcategories recursively
                var subcategories = await _unitOfWork.Categories.GetByParentIdAsync(id);
                foreach (var subcategory in subcategories)
                {
                    await DeleteAsync(subcategory.Id);
                }

                await _unitOfWork.Categories.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Category deleted successfully: {CategoryId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                throw;
            }
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            try
            {
                var products = await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);
                return products.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if category has products: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<bool> HasSubCategoriesAsync(int categoryId)
        {
            try
            {
                var subcategories = await _unitOfWork.Categories.GetByParentIdAsync(categoryId);
                return subcategories.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if category has subcategories: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<IEnumerable<CategoryDto>> GetHierarchyAsync()
        {
            try
            {
                var allCategories = await _unitOfWork.Categories.GetAllAsync();
                return allCategories.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category hierarchy");
                throw;
            }
        }

        private CategoryDto MapToDto(Category category)
        {
            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                ParentCategoryId = category.ParentCategoryId,
                DisplayOrder = category.DisplayOrder,
                NameArabic = string.Empty,
                ParentCategoryName = string.Empty, 
                ProductCount = 0 
            };

            // Load Arabic translation if available
            var arabicTranslation = category.CategoryTranslations?.FirstOrDefault(t => t.LanguageCode == "ar");
            if (arabicTranslation != null)
            {
                categoryDto.NameArabic = arabicTranslation.Name;
            }

            return categoryDto;
        }

        private async Task LoadCategoryDiscountsAsync(CategoryDto categoryDto)
        {
            try
            {
                var categoryDiscounts = await _unitOfWork.CategoryDiscounts.GetActiveDiscountsByCategoryIdAsync(categoryDto.Id);
                categoryDto.SelectedDiscountIds = categoryDiscounts.Select(cd => cd.DiscountsId).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category discounts for category {CategoryId}", categoryDto.Id);
                // Don't throw - just log and continue without discounts
            }
        }

        private Category MapToEntity(CategoryDto dto)
        {
            return new Category
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive,
                ParentCategoryId = dto.ParentCategoryId,
                DisplayOrder = dto.DisplayOrder
            };
        }

        #region Helper Methods

        private async Task SaveCategoryTranslationAsync(int categoryId, string nameArabic, string description)
        {
            if (string.IsNullOrWhiteSpace(nameArabic))
                return;

            try
            {
                // For now, we'll create a CategoryTranslation entity directly since we don't have a repository
                // This is a simplified implementation - in production, you'd use a proper repository pattern
                var translation = new Domain.Entities.CategoryTranslation
                {
                    CategoryId = categoryId,
                    LanguageCode = "ar",
                    Name = nameArabic,
                    Description = description ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                // Since we don't have CategoryTranslation repository, we'll add it to the Category's navigation property
                var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
                if (category != null)
                {
                    if (category.CategoryTranslations == null)
                    {
                        category.CategoryTranslations = new List<Domain.Entities.CategoryTranslation>();
                    }
                    category.CategoryTranslations.Add(translation);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving category translation for category {CategoryId}", categoryId);
                // Don't throw - just log the error
            }
        }

        private async Task UpdateCategoryTranslationAsync(int categoryId, string nameArabic, string description)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
                if (category == null) return;

                // Find existing Arabic translation
                var arabicTranslation = category.CategoryTranslations?.FirstOrDefault(t => t.LanguageCode == "ar");

                if (string.IsNullOrWhiteSpace(nameArabic))
                {
                    // Remove translation if name is empty
                    if (arabicTranslation != null && category.CategoryTranslations != null)
                    {
                        category.CategoryTranslations.Remove(arabicTranslation);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
                else
                {
                    if (arabicTranslation != null)
                    {
                        // Update existing translation
                        arabicTranslation.Name = nameArabic;
                        arabicTranslation.Description = description ?? string.Empty;
                    }
                    else
                    {
                        // Create new translation
                        var newTranslation = new Domain.Entities.CategoryTranslation
                        {
                            CategoryId = categoryId,
                            LanguageCode = "ar",
                            Name = nameArabic,
                            Description = description ?? string.Empty,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        if (category.CategoryTranslations == null)
                        {
                            category.CategoryTranslations = new List<Domain.Entities.CategoryTranslation>();
                        }
                        category.CategoryTranslations.Add(newTranslation);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category translation for category {CategoryId}", categoryId);
                // Don't throw - just log the error
            }
        }

        private async Task SaveCategoryDiscountsAsync(int categoryId, List<int> discountIds)
        {
            if (discountIds == null || !discountIds.Any())
                return;

            try
            {
                // First remove existing mappings for this category
                var existingMappings = await _unitOfWork.CategoryDiscounts.GetActiveDiscountsByCategoryIdAsync(categoryId);
                foreach (var mapping in existingMappings)
                {
                    mapping.DeletedAt = DateTime.UtcNow;
                }

                // Add new mappings
                var newMappings = discountIds.Select(discountId => new Domain.Entities.CategoryDiscount
                {
                    CategoryId = categoryId,
                    DiscountsId = discountId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                await _unitOfWork.CategoryDiscounts.AddRangeAsync(newMappings);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving category discounts for category {CategoryId}", categoryId);
                // Don't throw - just log the error
            }
        }

        #endregion
    }
}