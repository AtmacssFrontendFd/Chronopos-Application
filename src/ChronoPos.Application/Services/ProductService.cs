using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using System.IO;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Product operations
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    private static readonly string LogFilePath = Path.Combine(LogDirectory, $"product_service_{DateTime.Now:yyyyMMdd}.log");
    private static readonly object LockObject = new object();
    
    static ProductService()
    {
        if (!Directory.Exists(LogDirectory))
        {
            Directory.CreateDirectory(LogDirectory);
        }
    }
    
    private static void Log(string message)
    {
        try
        {
            lock (LockObject)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] [ProductService] {message}";
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
                System.Diagnostics.Debug.WriteLine(logEntry);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductService] Logging failed: {ex.Message}");
        }
    }
    
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
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductService] SearchProductsAsync called with term: '{searchTerm}'");
            
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                System.Diagnostics.Debug.WriteLine("[ProductService] Search term is null/whitespace, returning empty");
                return Enumerable.Empty<ProductDto>();
            }
                
            System.Diagnostics.Debug.WriteLine("[ProductService] Calling _unitOfWork.Products.SearchProductsAsync...");
            var products = await _unitOfWork.Products.SearchProductsAsync(searchTerm);
            
            System.Diagnostics.Debug.WriteLine($"[ProductService] Repository returned {products?.Count() ?? 0} products");
            
            if (products != null)
            {
                var productList = products.ToList();
                foreach (var product in productList)
                {
                    System.Diagnostics.Debug.WriteLine($"[ProductService] Found product: {product.Name} (ID: {product.Id})");
                }
                
                var dtoList = productList.Select(MapToDto).ToList();
                System.Diagnostics.Debug.WriteLine($"[ProductService] Mapped to {dtoList.Count} DTOs");
                return dtoList;
            }
            
            System.Diagnostics.Debug.WriteLine("[ProductService] No products returned from repository");
            return Enumerable.Empty<ProductDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductService] ERROR in SearchProductsAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductService] Stack trace: {ex.StackTrace}");
            throw;
        }
    }
    
    public async Task<ProductDto> CreateProductAsync(ProductDto productDto)
    {
        Log("=== CreateProductAsync STARTED ===");
        try
        {
            Log($"Input ProductDto: Name='{productDto.Name}', SKU='{productDto.SKU}', CategoryId={productDto.CategoryId}, UnitOfMeasurementId={productDto.UnitOfMeasurementId}");
            Log($"Additional details: Price={productDto.Price}, StockQuantity={productDto.StockQuantity}, IsActive={productDto.IsActive}");
            
            Log("Calling MapToEntity...");
            var product = MapToEntity(productDto);
            
            Log($"Mapped Product: Id={product.Id}, Code='{product.Code}', Name='{product.Name}', CategoryId={product.CategoryId}, UnitOfMeasurementId={product.UnitOfMeasurementId}");
            Log($"Product dates: CreatedAt={product.CreatedAt}, UpdatedAt={product.UpdatedAt}");
            
            Log("Calling _unitOfWork.Products.AddAsync...");
            var createdProduct = await _unitOfWork.Products.AddAsync(product);
            Log($"Product added to context. CreatedProduct ID: {createdProduct.Id}");
            
            Log("Calling _unitOfWork.SaveChangesAsync...");
            await _unitOfWork.SaveChangesAsync();
            Log("SaveChanges completed successfully");
            
            Log("Mapping result to DTO...");
            var result = MapToDto(createdProduct);
            Log($"Result DTO: Id={result.Id}, Name='{result.Name}', UnitOfMeasurementId={result.UnitOfMeasurementId}");
            
            Log("=== CreateProductAsync COMPLETED SUCCESSFULLY ===");
            return result;
        }
        catch (Exception ex)
        {
            Log($"=== CreateProductAsync FAILED ===");
            Log($"Exception Type: {ex.GetType().Name}");
            Log($"Exception Message: {ex.Message}");
            Log($"Stack Trace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                Log($"Inner Exception Type: {ex.InnerException.GetType().Name}");
                Log($"Inner Exception Message: {ex.InnerException.Message}");
                Log($"Inner Exception Stack Trace: {ex.InnerException.StackTrace}");
            }
            
            throw;
        }
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
        existingProduct.UnitOfMeasurementId = productDto.UnitOfMeasurementId > 0 ? productDto.UnitOfMeasurementId : 1; // Default to "Pieces"
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
        Log("=== CreateCategoryAsync STARTED ===");
        try
        {
            Log($"Input CategoryDto: Name='{categoryDto.Name}', Description='{categoryDto.Description}', IsActive={categoryDto.IsActive}");
            Log($"NameArabic='{categoryDto.NameArabic}', DisplayOrder={categoryDto.DisplayOrder}");
            
            Log("Calling MapCategoryToEntity...");
            var category = MapCategoryToEntity(categoryDto);
            
            Log($"Mapped Category: Id={category.Id}, Name='{category.Name}', IsActive={category.IsActive}");
            Log($"Category dates: CreatedAt={category.CreatedAt}, UpdatedAt={category.UpdatedAt}");
            
            Log("Calling _unitOfWork.Categories.AddAsync...");
            var createdCategory = await _unitOfWork.Categories.AddAsync(category);
            Log($"Category added to context. CreatedCategory ID: {createdCategory.Id}");
            
            Log("Calling _unitOfWork.SaveChangesAsync...");
            await _unitOfWork.SaveChangesAsync();
            Log("SaveChanges completed successfully");
            
            // Create Arabic translation if provided
            if (!string.IsNullOrWhiteSpace(categoryDto.NameArabic))
            {
                Log($"Creating Arabic translation for category: '{categoryDto.NameArabic}'");
                var translation = new CategoryTranslation
                {
                    CategoryId = createdCategory.Id,
                    LanguageCode = "ar",
                    Name = categoryDto.NameArabic,
                    Description = categoryDto.Description,
                    CreatedAt = DateTime.UtcNow
                };
                
                // This would require translation repository - simplified for now
                // await _unitOfWork.CategoryTranslations.AddAsync(translation);
                // await _unitOfWork.SaveChangesAsync();
                Log("Arabic translation creation skipped (simplified implementation)");
            }
            
            Log("Mapping result to DTO...");
            var result = MapCategoryToDto(createdCategory);
            Log($"Result DTO: Id={result.Id}, Name='{result.Name}'");
            
            Log("=== CreateCategoryAsync COMPLETED SUCCESSFULLY ===");
            return result;
        }
        catch (Exception ex)
        {
            Log($"=== CreateCategoryAsync FAILED ===");
            Log($"Exception Type: {ex.GetType().Name}");
            Log($"Exception Message: {ex.Message}");
            Log($"Stack Trace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                Log($"Inner Exception Type: {ex.InnerException.GetType().Name}");
                Log($"Inner Exception Message: {ex.InnerException.Message}");
                Log($"Inner Exception Stack Trace: {ex.InnerException.StackTrace}");
            }
            
            throw;
        }
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
    
    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        return category != null ? MapCategoryToDto(category) : null;
    }
    
    public async Task<CategoryTranslationDto> CreateCategoryTranslationAsync(CategoryTranslationDto translationDto)
    {
        // Note: This would require a CategoryTranslation repository
        // For now, return the input as if it was saved
        translationDto.Id = new Random().Next(1000, 9999); // Mock ID
        translationDto.CreatedAt = DateTime.UtcNow;
        return translationDto;
    }

    // Unit of Measurement methods
    public async Task<IEnumerable<UnitOfMeasurementDto>> GetAllUnitsOfMeasurementAsync()
    {
        var uoms = await _unitOfWork.UnitsOfMeasurement.GetAllAsync();
        return uoms.Select(MapUomToDto);
    }

    public async Task<UnitOfMeasurementDto?> GetUnitOfMeasurementByIdAsync(int id)
    {
        var uom = await _unitOfWork.UnitsOfMeasurement.GetByIdAsync(id);
        return uom != null ? MapUomToDto(uom) : null;
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
            // UOM Properties
            UnitOfMeasurementId = product.UnitOfMeasurementId,
            UnitOfMeasurementName = product.UnitOfMeasurement?.Name ?? string.Empty,
            UnitOfMeasurementAbbreviation = product.UnitOfMeasurement?.Abbreviation ?? string.Empty,
            SelectedStoreId = 1, // Default store - could be enhanced to get from context
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
    
    private static Product MapToEntity(ProductDto dto)
    {
        Log($"MapToEntity called with: Name='{dto.Name}', CategoryId={dto.CategoryId}, UnitOfMeasurementId={dto.UnitOfMeasurementId}");
        
        var finalUomId = dto.UnitOfMeasurementId > 0 ? dto.UnitOfMeasurementId : 1;
        Log($"UOM mapping: Input={dto.UnitOfMeasurementId}, Final={finalUomId}");
        
        var product = new Product
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
            // UOM Property
            UnitOfMeasurementId = finalUomId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        Log($"Created Product entity: CategoryId={product.CategoryId}, UnitOfMeasurementId={product.UnitOfMeasurementId}");
        return product;
    }
    
    private static CategoryDto MapCategoryToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name ?? string.Empty,
            DisplayOrder = category.DisplayOrder,
            ProductCount = category.Products?.Count ?? 0
        };
    }
    
    private static Category MapCategoryToEntity(CategoryDto dto)
    {
        Log($"MapCategoryToEntity called with: Name='{dto.Name}', IsActive={dto.IsActive}, DisplayOrder={dto.DisplayOrder}");
        Log($"Category mapping: ParentCategoryId={dto.ParentCategoryId}, Description='{dto.Description}'");
        
        var category = new Category
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            ParentCategoryId = dto.ParentCategoryId,
            DisplayOrder = dto.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        Log($"Created Category entity: Id={category.Id}, Name='{category.Name}', IsActive={category.IsActive}");
        return category;
    }

    private static UnitOfMeasurementDto MapUomToDto(Domain.Entities.UnitOfMeasurement uom)
    {
        return new UnitOfMeasurementDto
        {
            Id = uom.Id,
            Name = uom.Name,
            Abbreviation = uom.Abbreviation,
            ConversionFactor = uom.ConversionFactor,
            BaseUomId = uom.BaseUomId,
            BaseUomName = uom.BaseUom?.Name ?? string.Empty,
            IsActive = uom.IsActive
        };
    }
}
