using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using ValidationResult = ChronoPos.Application.Interfaces.ValidationResult;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for ProductUnit operations
/// </summary>
public class ProductUnitService : IProductUnitService
{
    private readonly IProductUnitRepository _productUnitRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggingService _logger;
    private readonly ISkuGenerationService _skuGenerationService;

    public ProductUnitService(
        IProductUnitRepository productUnitRepository,
        IUnitOfWork unitOfWork,
        ILoggingService logger,
        ISkuGenerationService skuGenerationService)
    {
        _productUnitRepository = productUnitRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _skuGenerationService = skuGenerationService;
    }

    /// <summary>
    /// Creates a new product unit
    /// </summary>
    /// <param name="createDto">Product unit creation data</param>
    /// <returns>Created product unit</returns>
    public async Task<ProductUnitDto> CreateAsync(CreateProductUnitDto createDto)
    {
        try
        {
            // Validate business rules
            await ValidateCreateProductUnit(createDto);

            // Generate SKU if not provided
            string sku = createDto.Sku;
            if (string.IsNullOrWhiteSpace(sku))
            {
                // We need to get product and unit information for SKU generation
                // For now, we'll generate a temporary SKU and update it after creation
                sku = $"TEMP-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
            }

            var productUnit = new ProductUnit
            {
                ProductId = createDto.ProductId,
                UnitId = createDto.UnitId,
                QtyInUnit = createDto.QtyInUnit,
                CostOfUnit = createDto.CostOfUnit,
                PriceOfUnit = createDto.PriceOfUnit,
                SellingPriceId = createDto.SellingPriceId,
                PriceType = createDto.PriceType,
                DiscountAllowed = createDto.DiscountAllowed,
                IsBase = createDto.IsBase,
                Sku = sku,
                CreatedAt = DateTime.UtcNow
            };

            // Ensure only one base unit per product
            if (createDto.IsBase)
            {
                await _productUnitRepository.UpdateBaseUnitAsync(createDto.ProductId, createDto.UnitId);
            }

            var createdProductUnit = await _productUnitRepository.AddAsync(productUnit);
            await _unitOfWork.SaveChangesAsync();

            // Generate proper SKU if it was temporary
            if (string.IsNullOrWhiteSpace(createDto.Sku))
            {
                // Get the created product unit with navigation properties
                var productUnitWithDetails = await _productUnitRepository.GetByIdAsync(createdProductUnit.Id);
                if (productUnitWithDetails?.Product != null && productUnitWithDetails?.Unit != null)
                {
                    var generatedSku = await _skuGenerationService.GenerateProductUnitSkuAsync(
                        productUnitWithDetails.ProductId,
                        productUnitWithDetails.Product.Name,
                        productUnitWithDetails.UnitId,
                        productUnitWithDetails.Unit.Name,
                        productUnitWithDetails.QtyInUnit);

                    // Update the SKU
                    productUnitWithDetails.Sku = generatedSku;
                    productUnitWithDetails.UpdatedAt = DateTime.UtcNow;
                    await _productUnitRepository.UpdateAsync(productUnitWithDetails);
                    await _unitOfWork.SaveChangesAsync();

                    createdProductUnit = productUnitWithDetails;
                }
            }

            _logger.Log($"Created product unit for Product ID {createDto.ProductId}, Unit ID {createDto.UnitId} with SKU {createdProductUnit.Sku}");

            return MapToDto(createdProductUnit);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating product unit: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing product unit
    /// </summary>
    /// <param name="updateDto">Product unit update data</param>
    /// <returns>Updated product unit</returns>
    public async Task<ProductUnitDto> UpdateAsync(UpdateProductUnitDto updateDto)
    {
        try
        {
            var existingProductUnit = await _productUnitRepository.GetByIdAsync(updateDto.Id);
            if (existingProductUnit == null)
            {
                throw new ArgumentException($"Product unit with ID {updateDto.Id} not found");
            }

            // Update properties
            existingProductUnit.QtyInUnit = updateDto.QtyInUnit;
            existingProductUnit.CostOfUnit = updateDto.CostOfUnit;
            existingProductUnit.PriceOfUnit = updateDto.PriceOfUnit;
            existingProductUnit.SellingPriceId = updateDto.SellingPriceId;
            existingProductUnit.PriceType = updateDto.PriceType;
            existingProductUnit.DiscountAllowed = updateDto.DiscountAllowed;
            existingProductUnit.IsBase = updateDto.IsBase;
            
            // Update SKU if provided
            if (!string.IsNullOrWhiteSpace(updateDto.Sku))
            {
                existingProductUnit.Sku = updateDto.Sku;
            }
            
            existingProductUnit.UpdatedAt = DateTime.UtcNow;

            // Ensure only one base unit per product
            if (updateDto.IsBase)
            {
                await _productUnitRepository.UpdateBaseUnitAsync(existingProductUnit.ProductId, existingProductUnit.UnitId);
            }

            await _productUnitRepository.UpdateAsync(existingProductUnit);
            await _unitOfWork.SaveChangesAsync();

            _logger.Log($"Updated product unit ID {updateDto.Id}");

            return MapToDto(existingProductUnit);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating product unit ID {updateDto.Id}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Deletes a product unit by ID
    /// </summary>
    /// <param name="id">Product unit ID</param>
    /// <returns>Task</returns>
    public async Task DeleteAsync(int id)
    {
        try
        {
            var productUnit = await _productUnitRepository.GetByIdAsync(id);
            if (productUnit == null)
            {
                throw new ArgumentException($"Product unit with ID {id} not found");
            }

            await _productUnitRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _logger.Log($"Deleted product unit ID {id}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting product unit ID {id}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets a product unit by ID
    /// </summary>
    /// <param name="id">Product unit ID</param>
    /// <returns>Product unit or null if not found</returns>
    public async Task<ProductUnitDto?> GetByIdAsync(int id)
    {
        try
        {
            var productUnit = await _productUnitRepository.GetByIdAsync(id);
            return productUnit != null ? MapToDto(productUnit) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting product unit ID {id}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets all product units for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of product units</returns>
    public async Task<IEnumerable<ProductUnitDto>> GetByProductIdAsync(int productId)
    {
        try
        {
            var productUnits = await _productUnitRepository.GetByProductIdWithUnitsAsync(productId);
            return productUnits.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting product units for Product ID {productId}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets the base unit for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Base product unit or null if not found</returns>
    public async Task<ProductUnitDto?> GetBaseUnitByProductIdAsync(int productId)
    {
        try
        {
            var baseUnit = await _productUnitRepository.GetBaseUnitByProductIdAsync(productId);
            return baseUnit != null ? MapToDto(baseUnit) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting base unit for Product ID {productId}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Creates multiple product units for a product (typically used when creating a new product)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="productUnits">List of product units to create</param>
    /// <returns>List of created product units</returns>
    public async Task<IEnumerable<ProductUnitDto>> CreateMultipleAsync(int productId, IEnumerable<CreateProductUnitDto> productUnits)
    {
        try
        {
            var productUnitsList = productUnits.ToList();

            // Validate all product units
            var validationResult = await ValidateProductUnitsAsync(productUnitsList);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            var createdUnits = new List<ProductUnitDto>();

            foreach (var createDto in productUnitsList)
            {
                createDto.ProductId = productId; // Ensure correct product ID
                var createdUnit = await CreateAsync(createDto);
                createdUnits.Add(createdUnit);
            }

            _logger.Log($"Created {createdUnits.Count} product units for Product ID {productId}");

            return createdUnits;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating multiple product units for Product ID {productId}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Updates all product units for a product (replaces existing ones)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="productUnits">New list of product units</param>
    /// <returns>List of updated product units</returns>
    public async Task<IEnumerable<ProductUnitDto>> UpdateAllForProductAsync(int productId, IEnumerable<CreateProductUnitDto> productUnits)
    {
        try
        {
            // Delete existing product units
            await DeleteByProductIdAsync(productId);

            // Create new product units
            return await CreateMultipleAsync(productId, productUnits);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating all product units for Product ID {productId}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Deletes all product units for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Task</returns>
    public async Task DeleteByProductIdAsync(int productId)
    {
        try
        {
            await _productUnitRepository.DeleteByProductIdAsync(productId);
            _logger.Log($"Deleted all product units for Product ID {productId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting product units for Product ID {productId}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Updates the base unit designation for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="newBaseUnitId">New base unit ID</param>
    /// <returns>Task</returns>
    public async Task UpdateBaseUnitAsync(int productId, long newBaseUnitId)
    {
        try
        {
            await _productUnitRepository.UpdateBaseUnitAsync(productId, newBaseUnitId);
            _logger.Log($"Updated base unit for Product ID {productId} to Unit ID {newBaseUnitId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating base unit for Product ID {productId}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Validates a list of product units for business rules
    /// </summary>
    /// <param name="productUnits">Product units to validate</param>
    /// <returns>Validation result with any errors</returns>
    public Task<Interfaces.ValidationResult> ValidateProductUnitsAsync(IEnumerable<CreateProductUnitDto> productUnits)
    {
        var result = new Interfaces.ValidationResult();
        var productUnitsList = productUnits.ToList();

        // Check if any units are provided
        if (!productUnitsList.Any())
        {
            result.AddError("At least one product unit is required");
            return Task.FromResult(result);
        }

        // Check for duplicate units
        var duplicateUnits = productUnitsList
            .GroupBy(pu => pu.UnitId)
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicateUnits.Any())
        {
            result.AddError($"Duplicate units found: {string.Join(", ", duplicateUnits.Select(g => g.Key))}");
        }

        // Check for exactly one base unit
        var baseUnits = productUnitsList.Where(pu => pu.IsBase).ToList();
        if (baseUnits.Count == 0)
        {
            result.AddError("Exactly one base unit is required");
        }
        else if (baseUnits.Count > 1)
        {
            result.AddError("Only one base unit is allowed per product");
        }

        // Validate pricing
        foreach (var unit in productUnitsList)
        {
            if (unit.PriceOfUnit < unit.CostOfUnit)
            {
                result.AddWarning($"Unit {unit.UnitId}: Price ({unit.PriceOfUnit:C}) is less than cost ({unit.CostOfUnit:C})");
            }

            if (unit.QtyInUnit <= 0)
            {
                result.AddError($"Unit {unit.UnitId}: Quantity in unit must be greater than 0");
            }

            if (unit.CostOfUnit < 0)
            {
                result.AddError($"Unit {unit.UnitId}: Cost cannot be negative");
            }

            if (unit.PriceOfUnit < 0)
            {
                result.AddError($"Unit {unit.UnitId}: Price cannot be negative");
            }
        }

        return Task.FromResult(result);
    }

    /// <summary>
    /// Gets product unit summaries for a product (lightweight version for display)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of product unit summaries</returns>
    public async Task<IEnumerable<ProductUnitSummaryDto>> GetSummariesByProductIdAsync(int productId)
    {
        try
        {
            var productUnits = await _productUnitRepository.GetByProductIdWithUnitsAsync(productId);
            return productUnits.Select(pu => new ProductUnitSummaryDto
            {
                Id = pu.Id,
                UnitId = pu.UnitId,
                UnitName = pu.Unit?.Name ?? string.Empty,
                UnitAbbreviation = pu.Unit?.Abbreviation ?? string.Empty,
                QtyInUnit = pu.QtyInUnit,
                PriceOfUnit = pu.PriceOfUnit,
                CostOfUnit = pu.CostOfUnit,
                IsBase = pu.IsBase,
                DiscountAllowed = pu.DiscountAllowed,
                PriceType = pu.PriceType,
                Sku = pu.Sku
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting product unit summaries for Product ID {productId}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets all product units in the system
    /// </summary>
    /// <returns>List of all product units</returns>
    public async Task<IEnumerable<ProductUnitDto>> GetAllAsync()
    {
        try
        {
            var productUnits = await _productUnitRepository.GetAllWithNavigationAsync();
            return productUnits.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting all product units: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets all product units with navigation properties (for UI display)
    /// </summary>
    /// <returns>List of all product units with complete information</returns>
    public async Task<IEnumerable<ProductUnit>> GetAllWithNavigationAsync()
    {
        try
        {
            return await _productUnitRepository.GetAllWithNavigationAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting all product units with navigation: {ex.Message}", ex);
            throw;
        }
    }

    #region Private Methods

    private async Task ValidateCreateProductUnit(CreateProductUnitDto createDto)
    {
        // Check if product unit already exists for this product and unit combination
        var existingProductUnit = await _productUnitRepository.GetByProductIdAndUnitIdAsync(createDto.ProductId, createDto.UnitId);
        if (existingProductUnit != null)
        {
            throw new ArgumentException($"Product unit already exists for Product ID {createDto.ProductId} and Unit ID {createDto.UnitId}");
        }
    }

    private ProductUnitDto MapToDto(ProductUnit productUnit)
    {
        return new ProductUnitDto
        {
            Id = productUnit.Id,
            ProductId = productUnit.ProductId,
            UnitId = productUnit.UnitId,
            QtyInUnit = productUnit.QtyInUnit,
            CostOfUnit = productUnit.CostOfUnit,
            PriceOfUnit = productUnit.PriceOfUnit,
            SellingPriceId = productUnit.SellingPriceId,
            PriceType = productUnit.PriceType,
            DiscountAllowed = productUnit.DiscountAllowed,
            IsBase = productUnit.IsBase,
            Sku = productUnit.Sku,
            CreatedAt = productUnit.CreatedAt,
            UpdatedAt = productUnit.UpdatedAt,
            UnitName = productUnit.Unit?.Name,
            UnitAbbreviation = productUnit.Unit?.Abbreviation,
            ProductName = productUnit.Product?.Name
        };
    }

    #endregion
}
