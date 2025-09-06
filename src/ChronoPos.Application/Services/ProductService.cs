using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Product operations
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }
    
    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        return products.Select(MapToDto);
    }
    
    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        return product != null ? MapToDto(product) : null;
    }
    
    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
    {
        var products = await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);
        return products.Select(MapToDto);
    }
    
    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<ProductDto>();
            
        var products = await _unitOfWork.Products.SearchProductsAsync(searchTerm);
        return products.Select(MapToDto);
    }
    
    public async Task<ProductDto> CreateProductAsync(ProductDto productDto)
    {
        var product = MapToEntity(productDto);
        var createdProduct = await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(createdProduct);
    }
    
    public async Task<ProductDto> UpdateProductAsync(ProductDto productDto)
    {
        var existingProduct = await _unitOfWork.Products.GetByIdAsync(productDto.Id);
        if (existingProduct == null)
            throw new ArgumentException($"Product with ID {productDto.Id} not found.");
            
        existingProduct.Name = productDto.Name;
        existingProduct.Description = productDto.Description;
        existingProduct.SKU = productDto.SKU;
        existingProduct.Price = productDto.Price;
        existingProduct.StockQuantity = productDto.StockQuantity;
        existingProduct.CategoryId = productDto.CategoryId;
        existingProduct.IsActive = productDto.IsActive;
        existingProduct.Cost = productDto.CostPrice;
        existingProduct.Markup = productDto.Markup;
        existingProduct.ImagePath = productDto.ImagePath;
        existingProduct.Color = productDto.Color;
        existingProduct.UpdatedAt = DateTime.UtcNow;
        
        await _unitOfWork.Products.UpdateAsync(existingProduct);
        await _unitOfWork.SaveChangesAsync();
        
        return MapToDto(existingProduct);
    }
    
    public async Task DeleteProductAsync(int id)
    {
        await _unitOfWork.Products.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }
    
    public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10)
    {
        var products = await _unitOfWork.Products.GetLowStockProductsAsync(threshold);
        return products.Select(MapToDto);
    }
    
    // Category management methods
    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return categories.Select(MapCategoryToDto);
    }
    
    public async Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto)
    {
        var category = MapCategoryToEntity(categoryDto);
        var createdCategory = await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();
        
        return MapCategoryToDto(createdCategory);
    }
    
    public async Task<CategoryDto> UpdateCategoryAsync(CategoryDto categoryDto)
    {
        var existingCategory = await _unitOfWork.Categories.GetByIdAsync(categoryDto.Id);
        if (existingCategory == null)
            throw new ArgumentException($"Category with ID {categoryDto.Id} not found.");
            
        existingCategory.Name = categoryDto.Name;
        existingCategory.Description = categoryDto.Description;
        existingCategory.IsActive = categoryDto.IsActive;
        existingCategory.UpdatedAt = DateTime.UtcNow;
        
        await _unitOfWork.Categories.UpdateAsync(existingCategory);
        await _unitOfWork.SaveChangesAsync();
        
        return MapCategoryToDto(existingCategory);
    }
    
    public async Task DeleteCategoryAsync(int id)
    {
        await _unitOfWork.Categories.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }
    
    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description ?? string.Empty,
            SKU = product.SKU,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            IsActive = product.IsActive,
            CostPrice = product.Cost,
            Markup = product.Markup,
            ImagePath = product.ImagePath,
            Color = product.Color ?? "#FFC107",
            // Stock Control Properties
            IsStockTracked = product.IsStockTracked,
            AllowNegativeStock = product.AllowNegativeStock,
            IsUsingSerialNumbers = product.IsUsingSerialNumbers,
            InitialStock = product.InitialStock,
            MinimumStock = product.MinimumStock,
            MaximumStock = product.MaximumStock,
            ReorderLevel = product.ReorderLevel,
            ReorderQuantity = product.ReorderQuantity,
            AverageCost = product.AverageCost,
            LastCost = product.LastCost,
            SelectedStoreId = 1, // Default store - could be enhanced to get from context
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
    
    private static Product MapToEntity(ProductDto dto)
    {
        return new Product
        {
            Id = dto.Id,
            Code = dto.SKU ?? string.Empty,
            Name = dto.Name,
            Description = dto.Description,
            SKU = dto.SKU,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            IsActive = dto.IsActive,
            Cost = dto.CostPrice,
            Markup = dto.Markup,
            ImagePath = dto.ImagePath,
            Color = dto.Color ?? "#FFC107",
            // Stock Control Properties
            IsStockTracked = dto.IsStockTracked,
            AllowNegativeStock = dto.AllowNegativeStock,
            IsUsingSerialNumbers = dto.IsUsingSerialNumbers,
            InitialStock = dto.InitialStock,
            MinimumStock = dto.MinimumStock,
            MaximumStock = dto.MaximumStock,
            ReorderLevel = dto.ReorderLevel,
            ReorderQuantity = dto.ReorderQuantity,
            AverageCost = dto.AverageCost,
            LastCost = dto.LastCost,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    private static CategoryDto MapCategoryToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            ProductCount = category.Products?.Count ?? 0
        };
    }
    
    private static Category MapCategoryToEntity(CategoryDto dto)
    {
        return new Category
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive
        };
    }
}
