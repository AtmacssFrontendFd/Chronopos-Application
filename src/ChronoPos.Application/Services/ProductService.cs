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
    private readonly IBrandRepository _brandRepository;
    private readonly IProductImageRepository _productImageRepository;
    private readonly IProductUnitService _productUnitService;
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
    
    public ProductService(IUnitOfWork unitOfWork, IBrandRepository brandRepository, IProductImageRepository productImageRepository, IProductUnitService productUnitService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _brandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
        _productImageRepository = productImageRepository ?? throw new ArgumentNullException(nameof(productImageRepository));
        _productUnitService = productUnitService ?? throw new ArgumentNullException(nameof(productUnitService));
    }
    
    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        Log("ProductService.GetAllProductsAsync: Starting...");
        
        try
        {
            Log("ProductService.GetAllProductsAsync: Calling _unitOfWork.Products.GetAllAsync()");
            var products = await _unitOfWork.Products.GetAllAsync();
            
            Log($"ProductService.GetAllProductsAsync: Repository returned {products?.Count() ?? 0} products");
            
            if (products != null)
            {
                foreach (var product in products.Take(3)) // Log first 3 products for debugging
                {
                    Log($"ProductService.GetAllProductsAsync: Product found - ID: {product.Id}, Name: '{product.Name}', IsActive: {product.IsActive}");
                }
                if (products.Count() > 3)
                {
                    Log($"ProductService.GetAllProductsAsync: ... and {products.Count() - 3} more products");
                }
                
                var result = products.Select(MapToDtoWithDiscounts);
                var resultList = result.ToList(); // Materialize to count
                
                Log($"ProductService.GetAllProductsAsync: Returning {resultList.Count} DTOs");
                return resultList;
            }
            else
            {
                Log("ProductService.GetAllProductsAsync: Repository returned null products");
                return new List<ProductDto>();
            }
        }
        catch (Exception ex)
        {
            Log($"ProductService.GetAllProductsAsync: ERROR - {ex.Message}");
            Log($"ProductService.GetAllProductsAsync: Stack trace - {ex.StackTrace}");
            throw;
        }
    }
    
    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        return product != null ? MapToDtoWithDiscounts(product) : null;
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
            
            Log("Generating unique PLU...");
            var uniquePLU = await GenerateUniquePLUAsync();
            
            Log("Calling MapToEntity...");
            var product = MapToEntityAsync(productDto, uniquePLU);
            
            Log($"Mapped Product: Id={product.Id}, Code='{product.Code}', Name='{product.Name}', PLU={product.PLU}, CategoryId={product.CategoryId}, UnitOfMeasurementId={product.UnitOfMeasurementId}");
            Log($"Product dates: CreatedAt={product.CreatedAt}, UpdatedAt={product.UpdatedAt}");
            
            // Compute and set cached tax-inclusive price before saving
            try
            {
                product.TaxInclusivePriceValue = await ComputeTaxInclusivePriceAsync(productDto);
            }
            catch (Exception ex)
            {
                Log($"Tax-inclusive price compute on create failed: {ex.Message}");
            }

            Log("Calling _unitOfWork.Products.AddAsync...");
            var createdProduct = await _unitOfWork.Products.AddAsync(product);
            Log($"Product added to context. CreatedProduct ID: {createdProduct.Id}");
            
            Log("Calling _unitOfWork.SaveChangesAsync...");
            await _unitOfWork.SaveChangesAsync();
            Log("SaveChanges completed successfully");

            // Handle ProductTaxes mapping after product has an Id
            if (productDto.SelectedTaxTypeIds != null && productDto.SelectedTaxTypeIds.Any())
            {
                Log($"Adding {productDto.SelectedTaxTypeIds.Distinct().Count()} ProductTaxes mappings for product ID {createdProduct.Id}");
                createdProduct.ProductTaxes.Clear();
                foreach (var taxTypeId in productDto.SelectedTaxTypeIds.Distinct())
                {
                    createdProduct.ProductTaxes.Add(new ProductTax
                    {
                        ProductId = createdProduct.Id,
                        TaxTypeId = taxTypeId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _unitOfWork.Products.UpdateAsync(createdProduct);
                await _unitOfWork.SaveChangesAsync();
                Log("ProductTaxes mappings saved successfully");
            }
            
            // Handle ProductDiscounts mapping after product has an Id
            if (productDto.SelectedDiscountIds != null && productDto.SelectedDiscountIds.Any())
            {
                Log($"Adding {productDto.SelectedDiscountIds.Distinct().Count()} ProductDiscounts mappings for product ID {createdProduct.Id}");
                
                var productDiscounts = productDto.SelectedDiscountIds.Distinct().Select(discountId => new ProductDiscount
                {
                    ProductId = createdProduct.Id,
                    DiscountsId = discountId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                await _unitOfWork.ProductDiscounts.AddRangeAsync(productDiscounts);
                Log("ProductDiscounts mappings saved successfully");
            }
            
            // Ensure computed tax-inclusive value persisted if ProductTaxes were just added
            try
            {
                createdProduct.TaxInclusivePriceValue = await ComputeTaxInclusivePriceAsync(productDto);
                await _unitOfWork.Products.UpdateAsync(createdProduct);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log($"Recompute tax-inclusive price after tax mapping failed: {ex.Message}");
            }

            // Handle ProductUnits creation after product has an Id
            if (productDto.ProductUnits != null && productDto.ProductUnits.Any())
            {
                Log($"Creating {productDto.ProductUnits.Count} ProductUnits for product ID {createdProduct.Id}");
                try
                {
                    var createProductUnitDtos = productDto.ProductUnits.Select(pu => new CreateProductUnitDto
                    {
                        ProductId = createdProduct.Id,
                        UnitId = pu.UnitId,
                        QtyInUnit = pu.QtyInUnit,
                        CostOfUnit = pu.CostOfUnit,
                        PriceOfUnit = pu.PriceOfUnit,
                        DiscountAllowed = pu.DiscountAllowed,
                        IsBase = pu.IsBase
                    }).ToList();

                    await _productUnitService.CreateMultipleAsync(createdProduct.Id, createProductUnitDtos);
                    Log("ProductUnits created successfully");
                }
                catch (Exception ex)
                {
                    Log($"ProductUnits creation failed: {ex.Message}");
                    throw; // Re-throw to maintain transaction integrity
                }
            }

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

    // Aggregates selling taxes and returns price including tax based on dto.Price
    private async Task<decimal> ComputeTaxInclusivePriceAsync(ProductDto dto)
    {
        try
        {
            var basePrice = dto.Price;
            if (basePrice < 0) basePrice = 0;

            if (dto.SelectedTaxTypeIds == null || dto.SelectedTaxTypeIds.Count == 0)
            {
                return dto.IsTaxInclusivePrice ? basePrice : basePrice; // no taxes
            }

            var taxTypes = await _unitOfWork.TaxTypes.GetAllAsync();
            var selected = taxTypes
                .Where(t => dto.SelectedTaxTypeIds.Contains(t.Id) && t.IsActive && t.AppliesToSelling)
                .OrderBy(t => t.CalculationOrder)
                .ToList();

            decimal running = basePrice;
            foreach (var tax in selected)
            {
                if (tax.IsPercentage)
                {
                    running += Math.Round(basePrice * (tax.Value / 100m), 2);
                }
                else
                {
                    running += tax.Value;
                }
            }

            return Math.Round(running, 2);
        }
        catch (Exception ex)
        {
            Log($"ComputeTaxInclusivePriceAsync error: {ex.Message}");
            return dto.Price;
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
    // Brand
    existingProduct.BrandId = productDto.BrandId > 0 ? productDto.BrandId : null;
        existingProduct.UnitOfMeasurementId = productDto.UnitOfMeasurementId > 0 ? productDto.UnitOfMeasurementId : 1; // Default to "Pieces"
    // Purchase/Selling Units
    existingProduct.PurchaseUnitId = productDto.PurchaseUnitId;
    existingProduct.SellingUnitId = productDto.SellingUnitId;

    // Tax & attributes
    existingProduct.IsTaxInclusivePrice = productDto.IsTaxInclusivePrice;
    existingProduct.IsDiscountAllowed = productDto.IsDiscountAllowed;
    existingProduct.MaxDiscount = productDto.MaxDiscount;
    existingProduct.IsPriceChangeAllowed = productDto.IsPriceChangeAllowed;
    existingProduct.IsService = productDto.IsService;
    existingProduct.AgeRestriction = productDto.AgeRestriction;

    // Stock control
    existingProduct.IsStockTracked = productDto.IsStockTracked;
    existingProduct.AllowNegativeStock = productDto.AllowNegativeStock;
    existingProduct.IsUsingSerialNumbers = productDto.IsUsingSerialNumbers;
    existingProduct.InitialStock = productDto.IsUsingSerialNumbers ? 0 : productDto.InitialStock;
    existingProduct.MinimumStock = productDto.MinimumStock;
    existingProduct.MaximumStock = productDto.MaximumStock;
    existingProduct.ReorderLevel = productDto.ReorderLevel;
    existingProduct.ReorderQuantity = productDto.ReorderQuantity;
    existingProduct.AverageCost = productDto.AverageCost;
    existingProduct.LastCost = productDto.LastCost;

    // Grouping
    existingProduct.ProductGroupId = productDto.ProductGroupId;
    existingProduct.Group = productDto.Group;
        
        // Handle barcode updates
        System.Diagnostics.Debug.WriteLine($"UpdateProductAsync: Clearing existing {existingProduct.ProductBarcodes.Count} barcodes for product {existingProduct.Id}");
        existingProduct.ProductBarcodes.Clear();
        
        // Add barcodes from ProductBarcodes collection
        if (productDto.ProductBarcodes != null && productDto.ProductBarcodes.Any())
        {
            System.Diagnostics.Debug.WriteLine($"UpdateProductAsync: Adding {productDto.ProductBarcodes.Count} new barcodes from ProductBarcodes collection");
            foreach (var barcodeDto in productDto.ProductBarcodes)
            {
                if (!string.IsNullOrWhiteSpace(barcodeDto.Barcode))
                {
                    existingProduct.ProductBarcodes.Add(new ProductBarcode
                    {
                        Barcode = barcodeDto.Barcode.Trim(),
                        BarcodeType = barcodeDto.BarcodeType,
                        ProductId = existingProduct.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                    System.Diagnostics.Debug.WriteLine($"UpdateProductAsync: Added barcode: {barcodeDto.Barcode} (Type: {barcodeDto.BarcodeType})");
                }
            }
        }
        // Fallback to single barcode field if ProductBarcodes collection is empty
        else if (!string.IsNullOrWhiteSpace(productDto.Barcode))
        {
            System.Diagnostics.Debug.WriteLine($"UpdateProductAsync: Using fallback single barcode: {productDto.Barcode}");
            existingProduct.ProductBarcodes.Add(new ProductBarcode
            {
                Barcode = productDto.Barcode.Trim(),
                ProductId = existingProduct.Id,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("UpdateProductAsync: No barcodes to add");
        }
        
        // Update ProductTaxes based on SelectedTaxTypeIds
        if (productDto.SelectedTaxTypeIds != null)
        {
            existingProduct.ProductTaxes.Clear();
            foreach (var taxTypeId in productDto.SelectedTaxTypeIds.Distinct())
            {
                existingProduct.ProductTaxes.Add(new ProductTax
                {
                    ProductId = existingProduct.Id,
                    TaxTypeId = taxTypeId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        
        // Update ProductDiscounts based on SelectedDiscountIds
        if (productDto.SelectedDiscountIds != null)
        {
            await _unitOfWork.ProductDiscounts.UpdateProductDiscountsAsync(
                existingProduct.Id, 
                productDto.SelectedDiscountIds.Distinct()
            );
        }

        // Recompute and cache TaxInclusivePriceValue on update
        try
        {
            existingProduct.TaxInclusivePriceValue = await ComputeTaxInclusivePriceAsync(productDto);
        }
        catch (Exception ex)
        {
            Log($"Tax-inclusive price compute on update failed: {ex.Message}");
        }

        // Handle ProductUnits update
        if (productDto.ProductUnits != null)
        {
            Log($"Updating ProductUnits for product ID {existingProduct.Id}");
            try
            {
                var updateProductUnitDtos = productDto.ProductUnits.Select(pu => new CreateProductUnitDto
                {
                    ProductId = existingProduct.Id,
                    UnitId = pu.UnitId,
                    QtyInUnit = pu.QtyInUnit,
                    CostOfUnit = pu.CostOfUnit,
                    PriceOfUnit = pu.PriceOfUnit,
                    DiscountAllowed = pu.DiscountAllowed,
                    IsBase = pu.IsBase
                }).ToList();

                await _productUnitService.UpdateAllForProductAsync(existingProduct.Id, updateProductUnitDtos);
                Log("ProductUnits updated successfully");
            }
            catch (Exception ex)
            {
                Log($"ProductUnits update failed: {ex.Message}");
                throw; // Re-throw to maintain transaction integrity
            }
        }

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
        Log("ProductService.GetAllCategoriesAsync: Starting...");
        
        try
        {
            Log("ProductService.GetAllCategoriesAsync: Calling _unitOfWork.Categories.GetAllAsync()");
            var categories = await _unitOfWork.Categories.GetAllAsync();
            
            Log($"ProductService.GetAllCategoriesAsync: Repository returned {categories?.Count() ?? 0} categories");
            
            if (categories != null)
            {
                foreach (var category in categories.Take(3)) // Log first 3 categories for debugging
                {
                    Log($"ProductService.GetAllCategoriesAsync: Category found - ID: {category.Id}, Name: '{category.Name}', IsActive: {category.IsActive}");
                }
                if (categories.Count() > 3)
                {
                    Log($"ProductService.GetAllCategoriesAsync: ... and {categories.Count() - 3} more categories");
                }
                
                var result = categories.Select(MapCategoryToDto);
                var resultList = result.ToList(); // Materialize to count
                
                Log($"ProductService.GetAllCategoriesAsync: Returning {resultList.Count} DTOs");
                return resultList;
            }
            else
            {
                Log("ProductService.GetAllCategoriesAsync: Repository returned null categories");
                return new List<CategoryDto>();
            }
        }
        catch (Exception ex)
        {
            Log($"ProductService.GetAllCategoriesAsync: ERROR - {ex.Message}");
            Log($"ProductService.GetAllCategoriesAsync: Stack trace - {ex.StackTrace}");
            throw;
        }
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
            
            // Handle CategoryDiscounts mapping after category has an Id
            if (categoryDto.SelectedDiscountIds != null && categoryDto.SelectedDiscountIds.Any())
            {
                Log($"Adding {categoryDto.SelectedDiscountIds.Distinct().Count()} CategoryDiscounts mappings for category ID {createdCategory.Id}");
                
                var categoryDiscounts = categoryDto.SelectedDiscountIds.Distinct().Select(discountId => new CategoryDiscount
                {
                    CategoryId = createdCategory.Id,
                    DiscountsId = discountId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                await _unitOfWork.CategoryDiscounts.AddRangeAsync(categoryDiscounts);
                Log("CategoryDiscounts mappings saved successfully");
            }
            
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
        Log("=== UpdateCategoryAsync STARTED ===");
        Log($"UpdateCategoryAsync - CategoryId: {categoryDto.Id}");
        Log($"UpdateCategoryAsync - Name: '{categoryDto.Name}'");
        Log($"UpdateCategoryAsync - NameArabic: '{categoryDto.NameArabic}'");
        Log($"UpdateCategoryAsync - Description: '{categoryDto.Description}'");
        Log($"UpdateCategoryAsync - IsActive: {categoryDto.IsActive}");
        Log($"UpdateCategoryAsync - SelectedDiscountIds: [{string.Join(", ", categoryDto.SelectedDiscountIds ?? new List<int>())}]");

        try
        {
            var existingCategory = await _unitOfWork.Categories.GetByIdAsync(categoryDto.Id);
            if (existingCategory == null)
            {
                Log($"UpdateCategoryAsync - Category with ID {categoryDto.Id} not found");
                throw new ArgumentException($"Category with ID {categoryDto.Id} not found.");
            }

            Log($"UpdateCategoryAsync - Found existing category: '{existingCategory.Name}'");
                
            existingCategory.Name = categoryDto.Name;
            existingCategory.Description = categoryDto.Description;
            existingCategory.IsActive = categoryDto.IsActive;
            existingCategory.UpdatedAt = DateTime.UtcNow;
            
            Log("UpdateCategoryAsync - Updating basic category properties");
            await _unitOfWork.Categories.UpdateAsync(existingCategory);
            await _unitOfWork.SaveChangesAsync();
            Log("UpdateCategoryAsync - Basic category update saved");

            // Handle Arabic translation
            if (!string.IsNullOrWhiteSpace(categoryDto.NameArabic))
            {
                Log($"UpdateCategoryAsync - Processing Arabic translation: '{categoryDto.NameArabic}'");
                await UpdateCategoryTranslationAsync(categoryDto.Id, categoryDto.NameArabic, categoryDto.Description);
            }
            else
            {
                Log("UpdateCategoryAsync - No Arabic translation provided");
            }

            // Handle discount mappings
            if (categoryDto.SelectedDiscountIds != null && categoryDto.SelectedDiscountIds.Any())
            {
                Log($"UpdateCategoryAsync - Processing discount mappings: [{string.Join(", ", categoryDto.SelectedDiscountIds)}]");
                await UpdateCategoryDiscountsAsync(categoryDto.Id, categoryDto.SelectedDiscountIds);
            }
            else
            {
                Log("UpdateCategoryAsync - No discount mappings provided, clearing existing discounts");
                await UpdateCategoryDiscountsAsync(categoryDto.Id, new List<int>());
            }

            Log("UpdateCategoryAsync - Mapping result to DTO");
            var result = MapCategoryToDto(existingCategory);
            Log($"UpdateCategoryAsync - Final result NameArabic: '{result.NameArabic}'");
            Log($"UpdateCategoryAsync - Final result SelectedDiscountIds: [{string.Join(", ", result.SelectedDiscountIds ?? new List<int>())}]");
            
            Log("=== UpdateCategoryAsync COMPLETED SUCCESSFULLY ===");
            return result;
        }
        catch (Exception ex)
        {
            Log($"=== UpdateCategoryAsync FAILED ===");
            Log($"ERROR: {ex.Message}");
            Log($"STACK TRACE: {ex.StackTrace}");
            throw;
        }
    }

    private async Task UpdateCategoryTranslationAsync(int categoryId, string nameArabic, string description)
    {
        Log($"UpdateCategoryTranslationAsync - CategoryId: {categoryId}, NameArabic: '{nameArabic}'");
        
        if (string.IsNullOrWhiteSpace(nameArabic))
        {
            Log("UpdateCategoryTranslationAsync - No Arabic name provided, skipping");
            return;
        }

        try
        {
            // Get category with translations
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category == null)
            {
                Log($"UpdateCategoryTranslationAsync - Category {categoryId} not found");
                return;
            }

            Log($"UpdateCategoryTranslationAsync - Category found, existing translations count: {category.CategoryTranslations?.Count ?? 0}");

            // Initialize translations collection if null
            if (category.CategoryTranslations == null)
            {
                category.CategoryTranslations = new List<Domain.Entities.CategoryTranslation>();
                Log("UpdateCategoryTranslationAsync - Initialized empty translations collection");
            }

            // Check if Arabic translation already exists
            var existingArabicTranslation = category.CategoryTranslations.FirstOrDefault(t => t.LanguageCode == "ar");
            
            if (existingArabicTranslation != null)
            {
                Log($"UpdateCategoryTranslationAsync - Updating existing Arabic translation from '{existingArabicTranslation.Name}' to '{nameArabic}'");
                existingArabicTranslation.Name = nameArabic;
                existingArabicTranslation.Description = description ?? string.Empty;
            }
            else
            {
                Log("UpdateCategoryTranslationAsync - Creating new Arabic translation");
                var newTranslation = new Domain.Entities.CategoryTranslation
                {
                    CategoryId = categoryId,
                    LanguageCode = "ar",
                    Name = nameArabic,
                    Description = description ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };
                category.CategoryTranslations.Add(newTranslation);
            }

            Log("UpdateCategoryTranslationAsync - Saving translation changes");
            await _unitOfWork.SaveChangesAsync();
            Log("UpdateCategoryTranslationAsync - Translation saved successfully");
        }
        catch (Exception ex)
        {
            Log($"UpdateCategoryTranslationAsync ERROR: {ex.Message}");
            Log($"UpdateCategoryTranslationAsync STACK TRACE: {ex.StackTrace}");
            // Don't throw - just log the error so the main operation can continue
        }
    }

    private async Task UpdateCategoryDiscountsAsync(int categoryId, List<int> discountIds)
    {
        Log($"UpdateCategoryDiscountsAsync - CategoryId: {categoryId}, DiscountIds: [{string.Join(", ", discountIds ?? new List<int>())}]");
        
        try
        {
            // First remove existing mappings for this category
            Log("UpdateCategoryDiscountsAsync - Getting existing discount mappings");
            var existingMappings = await _unitOfWork.CategoryDiscounts.GetActiveDiscountsByCategoryIdAsync(categoryId);
            Log($"UpdateCategoryDiscountsAsync - Found {existingMappings.Count()} existing mappings");

            foreach (var mapping in existingMappings)
            {
                mapping.DeletedAt = DateTime.UtcNow;
                Log($"UpdateCategoryDiscountsAsync - Marked mapping ID {mapping.Id} (DiscountId: {mapping.DiscountsId}) as deleted");
            }

            // Add new mappings if provided
            if (discountIds != null && discountIds.Any())
            {
                Log($"UpdateCategoryDiscountsAsync - Creating {discountIds.Count} new mappings");
                var newMappings = discountIds.Select(discountId => new Domain.Entities.CategoryDiscount
                {
                    CategoryId = categoryId,
                    DiscountsId = discountId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                await _unitOfWork.CategoryDiscounts.AddRangeAsync(newMappings);
                Log($"UpdateCategoryDiscountsAsync - Added {newMappings.Count} new mappings");
            }
            else
            {
                Log("UpdateCategoryDiscountsAsync - No new discount mappings to create");
            }

            Log("UpdateCategoryDiscountsAsync - Saving discount mapping changes");
            await _unitOfWork.SaveChangesAsync();
            Log("UpdateCategoryDiscountsAsync - Discount mappings saved successfully");
        }
        catch (Exception ex)
        {
            Log($"UpdateCategoryDiscountsAsync ERROR: {ex.Message}");
            Log($"UpdateCategoryDiscountsAsync STACK TRACE: {ex.StackTrace}");
            // Don't throw - just log the error so the main operation can continue
        }
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
    
    public Task<CategoryTranslationDto> CreateCategoryTranslationAsync(CategoryTranslationDto translationDto)
    {
        // Note: This would require a CategoryTranslation repository
        // For now, return the input as if it was saved
        translationDto.Id = new Random().Next(1000, 9999); // Mock ID
        translationDto.CreatedAt = DateTime.UtcNow;
        return Task.FromResult(translationDto);
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
            Barcode = product.ProductBarcodes?.FirstOrDefault()?.Barcode, // Map first barcode
            ProductBarcodes = product.ProductBarcodes?.Select(pb => new ProductBarcodeDto
            {
                Id = pb.Id,
                ProductId = pb.ProductId,
                Barcode = pb.Barcode,
                BarcodeType = pb.BarcodeType,
                CreatedAt = pb.CreatedAt,
                IsNew = false
            }).ToList() ?? new List<ProductBarcodeDto>(),
            Price = product.Price,
            StockQuantity = (int)product.InitialStock,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            BrandId = product.BrandId, // Map brand ID
            BrandName = product.Brand?.Name ?? string.Empty, // Map brand name
            IsActive = product.IsActive,
            CostPrice = product.Cost,
            Markup = product.Markup,
            ImagePath = product.ImagePath,
            Color = product.Color ?? "#FFC107",
            // Tax & attributes
            IsTaxInclusivePrice = product.IsTaxInclusivePrice,
            IsDiscountAllowed = product.IsDiscountAllowed,
            MaxDiscount = product.MaxDiscount,
            IsPriceChangeAllowed = product.IsPriceChangeAllowed,
            IsService = product.IsService,
            AgeRestriction = product.AgeRestriction,
            SelectedTaxTypeIds = product.ProductTaxes?.Select(pt => pt.TaxTypeId).Distinct().ToList() ?? new List<int>(),
            SelectedDiscountIds = product.ProductDiscounts?.Where(pd => pd.DeletedAt == null).Select(pd => pd.DiscountsId).Distinct().ToList() ?? new List<int>(),
            TaxInclusivePriceValue = product.TaxInclusivePriceValue,
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
            
            // Purchase and Selling Units
            PurchaseUnitId = product.PurchaseUnitId,
            PurchaseUnitName = product.PurchaseUnit?.Name ?? string.Empty,
            SellingUnitId = product.SellingUnitId,
            SellingUnitName = product.SellingUnit?.Name ?? string.Empty,
            
            // Product Grouping
            ProductGroupId = product.ProductGroupId,
            Group = product.Group,
            
            // Business Rules (Additional)
            CanReturn = product.CanReturn,
            IsGrouped = product.IsGrouped,
            SelectedStoreId = 1, // Default store - could be enhanced to get from context
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    /// <summary>
    /// Maps Product entity to ProductDto with discount details for display
    /// </summary>
    private static ProductDto MapToDtoWithDiscounts(Product product)
    {
        var dto = MapToDto(product);

        // Map active discounts for display
        if (product.ProductDiscounts?.Any() == true)
        {
            var now = DateTime.UtcNow;
            dto.ActiveDiscounts = product.ProductDiscounts
                .Where(pd => pd.DeletedAt == null && pd.Discount != null)
                .Select(pd => pd.Discount)
                .Where(d => d.IsActive && d.DeletedAt == null)
                .Where(d => now >= d.StartDate && now <= d.EndDate) // Only currently active
                .Select(d => new DiscountDisplayDto
                {
                    Id = d.Id,
                    DiscountName = d.DiscountName,
                    DiscountCode = d.DiscountCode,
                    FormattedDiscountValue = d.FormattedDiscountValue,
                    IsActive = d.IsActive,
                    IsCurrentlyActive = d.IsCurrentlyActive,
                    IsStackable = d.IsStackable,
                    StatusDisplay = d.StatusDisplay,
                    EndDate = d.EndDate
                })
                .OrderByDescending(d => d.IsCurrentlyActive)
                .ThenBy(d => d.EndDate)
                .ToList();

            // Create display string for table
            dto.ActiveDiscountsDisplay = dto.ActiveDiscounts.Any()
                ? string.Join(", ", dto.ActiveDiscounts.Take(2).Select(d => d.ShortDisplay))
                : "No active discounts";

            // Add "+" indicator if there are more than 2 discounts
            if (dto.ActiveDiscounts.Count > 2)
            {
                dto.ActiveDiscountsDisplay += $" +{dto.ActiveDiscounts.Count - 2} more";
            }
        }
        else
        {
            dto.ActiveDiscountsDisplay = "No discounts";
        }

        return dto;
    }
    
    private async Task<int> GenerateUniquePLUAsync()
    {
        Log("Generating unique PLU...");
        
        // Start from 1001 (as seen in seed data)
        var lastPLU = 1001;
        
        try
        {
            // Get the highest existing PLU
            var products = await _unitOfWork.Products.GetAllAsync();
            if (products.Any())
            {
                var maxPLU = products.Max(p => p.PLU);
                lastPLU = Math.Max(maxPLU, 1000) + 1;
            }
        }
        catch (Exception ex)
        {
            Log($"Error getting max PLU, using default: {ex.Message}");
            // If there's an error, generate a random PLU to avoid conflicts
            lastPLU = new Random().Next(2000, 9999);
        }
        
        Log($"Generated PLU: {lastPLU}");
        return lastPLU;
    }

    private static Product MapToEntityAsync(ProductDto dto, int plu)
    {
        Log($"MapToEntityAsync called with: Name='{dto.Name}', PLU={plu}, CategoryId={dto.CategoryId}, UnitOfMeasurementId={dto.UnitOfMeasurementId}, BrandId={dto.BrandId}");
        
        var finalUomId = dto.UnitOfMeasurementId > 0 ? dto.UnitOfMeasurementId : 1;
        Log($"UOM mapping: Input={dto.UnitOfMeasurementId}, Final={finalUomId}");
        
        var product = new Product
        {
            Id = dto.Id,
            Code = dto.SKU ?? string.Empty,
            PLU = plu, // Set the unique PLU
            Name = dto.Name,
            Description = dto.Description,
            SKU = dto.SKU,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            BrandId = dto.BrandId > 0 ? dto.BrandId : null, // Set to null if BrandId is 0 or negative
            IsActive = dto.IsActive,
            Cost = dto.CostPrice,
            Markup = dto.Markup,
            ImagePath = dto.ImagePath,
            Color = dto.Color ?? "#FFC107",
            // Tax & attributes
            IsTaxInclusivePrice = dto.IsTaxInclusivePrice,
            IsDiscountAllowed = dto.IsDiscountAllowed,
            MaxDiscount = dto.MaxDiscount,
            IsPriceChangeAllowed = dto.IsPriceChangeAllowed,
            IsService = dto.IsService,
            AgeRestriction = dto.AgeRestriction,
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
            
            // Purchase and Selling Units
            PurchaseUnitId = dto.PurchaseUnitId,
            SellingUnitId = dto.SellingUnitId,
            
            // Product Grouping
            ProductGroupId = dto.ProductGroupId,
            Group = dto.Group,
            
            // Business Rules (Additional)
            CanReturn = dto.CanReturn,
            IsGrouped = dto.IsGrouped,
            
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        // Handle barcode mapping
        System.Diagnostics.Debug.WriteLine("MapToEntityAsync: Processing barcodes...");
        
        // Add barcodes from ProductBarcodes collection first
        if (dto.ProductBarcodes != null && dto.ProductBarcodes.Any())
        {
            System.Diagnostics.Debug.WriteLine($"MapToEntityAsync: Adding {dto.ProductBarcodes.Count} barcodes from ProductBarcodes collection");
            foreach (var barcodeDto in dto.ProductBarcodes)
            {
                if (!string.IsNullOrWhiteSpace(barcodeDto.Barcode))
                {
                    product.ProductBarcodes.Add(new ProductBarcode
                    {
                        Barcode = barcodeDto.Barcode.Trim(),
                        BarcodeType = barcodeDto.BarcodeType,
                        CreatedAt = DateTime.UtcNow
                    });
                    System.Diagnostics.Debug.WriteLine($"MapToEntityAsync: Added barcode: {barcodeDto.Barcode} (Type: {barcodeDto.BarcodeType})");
                }
            }
        }
        // Fallback to single barcode field if ProductBarcodes collection is empty
        else if (!string.IsNullOrWhiteSpace(dto.Barcode))
        {
            System.Diagnostics.Debug.WriteLine($"MapToEntityAsync: Using fallback single barcode: {dto.Barcode}");
            product.ProductBarcodes.Add(new ProductBarcode
            {
                Barcode = dto.Barcode.Trim(),
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("MapToEntityAsync: No barcodes to add");
        }
        
        Log($"Created Product entity: PLU={product.PLU}, CategoryId={product.CategoryId}, UnitOfMeasurementId={product.UnitOfMeasurementId}");
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
            ProductCount = category.Products?.Count ?? 0,
            // Load Arabic translation if available
            NameArabic = category.CategoryTranslations?.FirstOrDefault(t => t.LanguageCode == "ar")?.Name ?? string.Empty,
            SelectedDiscountIds = category.CategoryDiscounts?.Where(cd => cd.DeletedAt == null)
                                                             .Select(cd => cd.DiscountsId)
                                                             .ToList() ?? new List<int>()
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
