using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using ChronoPos.Domain.Enums;
using System.IO;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Discount operations
/// </summary>
public class DiscountService : IDiscountService
{
    private readonly IDiscountRepository _discountRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    // Logging infrastructure
    private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    private static readonly string LogFilePath = Path.Combine(LogDirectory, $"discount_service_{DateTime.Now:yyyyMMdd}.log");
    private static readonly object LockObject = new object();
    
    static DiscountService()
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
                File.AppendAllText(LogFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Silent fail to prevent logging from breaking the application
        }
    }

    public DiscountService(IDiscountRepository discountRepository, IUnitOfWork unitOfWork)
    {
        _discountRepository = discountRepository ?? throw new ArgumentNullException(nameof(discountRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all discounts
    /// </summary>
    public async Task<IEnumerable<DiscountDto>> GetAllAsync()
    {
        var discounts = await _discountRepository.GetAllAsync();
        return discounts.Select(MapToDto);
    }

    /// <summary>
    /// Gets all active discounts
    /// </summary>
    public async Task<IEnumerable<DiscountDto>> GetActiveDiscountsAsync()
    {
        var discounts = await _discountRepository.GetActiveDiscountsAsync();
        return discounts.Select(MapToDto);
    }

    /// <summary>
    /// Gets all currently active discounts (within date range and active)
    /// </summary>
    public async Task<IEnumerable<DiscountDto>> GetCurrentlyActiveDiscountsAsync()
    {
        var discounts = await _discountRepository.GetCurrentlyActiveDiscountsAsync();
        return discounts.Select(MapToDto);
    }

    /// <summary>
    /// Gets discount by ID with product/category selections
    /// </summary>
    public async Task<DiscountDto?> GetByIdAsync(int id)
    {
        Log($"DiscountService.GetByIdAsync: Loading discount ID {id}");
        var discount = await _discountRepository.GetByIdAsync(id);
        var result = discount != null ? await MapToDtoWithSelections(discount) : null;
        Log($"DiscountService.GetByIdAsync: Returning {(result != null ? "DTO with selections" : "null")}");
        return result;
    }

    /// <summary>
    /// Gets discount by code
    /// </summary>
    public async Task<DiscountDto?> GetByCodeAsync(string code)
    {
        var discount = await _discountRepository.GetByCodeAsync(code);
        return discount != null ? MapToDto(discount) : null;
    }

    /// <summary>
    /// Gets discounts by applicable type (now uses many-to-many relationships)
    /// </summary>
    public async Task<IEnumerable<DiscountDto>> GetByApplicableAsync(DiscountApplicableOn applicableOn, IEnumerable<int>? productIds = null, IEnumerable<int>? categoryIds = null)
    {
        // This method now needs to be implemented using the many-to-many relationships
        // For now, return empty list as this method needs repository level changes
        return await Task.FromResult(new List<DiscountDto>());
    }

    /// <summary>
    /// Searches discounts with filtering
    /// </summary>
    public async Task<IEnumerable<DiscountDto>> SearchAsync(DiscountSearchDto searchDto)
    {
        var discounts = await _discountRepository.SearchAsync(
            searchDto.SearchTerm,
            searchDto.DiscountType,
            searchDto.ApplicableOn,
            searchDto.IsActive,
            searchDto.IsCurrentlyActive,
            searchDto.StartDateFrom,
            searchDto.StartDateTo,
            searchDto.EndDateFrom,
            searchDto.EndDateTo,
            searchDto.StoreId,
            searchDto.CreatedBy,
            searchDto.Skip,
            searchDto.Take);

        return discounts.Select(MapToDto);
    }

    /// <summary>
    /// Gets discounts count for search criteria
    /// </summary>
    public async Task<int> GetSearchCountAsync(DiscountSearchDto searchDto)
    {
        return await _discountRepository.GetSearchCountAsync(
            searchDto.SearchTerm,
            searchDto.DiscountType,
            searchDto.ApplicableOn,
            searchDto.IsActive,
            searchDto.IsCurrentlyActive,
            searchDto.StartDateFrom,
            searchDto.StartDateTo,
            searchDto.EndDateFrom,
            searchDto.EndDateTo,
            searchDto.StoreId,
            searchDto.CreatedBy);
    }

    /// <summary>
    /// Creates a new discount with product/category mappings
    /// </summary>
    public async Task<DiscountDto> CreateAsync(CreateDiscountDto discountDto)
    {
        // Validate discount data
        var validation = await ValidateDiscountAsync(discountDto);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException($"Validation failed: {string.Join(", ", validation.Errors)}");
        }

        var discount = MapToEntity(discountDto);
        
        // Create the discount first
        var createdDiscount = await _discountRepository.AddAsync(discount);
        await _unitOfWork.SaveChangesAsync();
        
        // Now create the product and category mappings
        await CreateProductDiscountMappings(createdDiscount.Id, discountDto.SelectedProductIds);
        await CreateCategoryDiscountMappings(createdDiscount.Id, discountDto.SelectedCategoryIds);
        
        await _unitOfWork.SaveChangesAsync();
        
        // Reload the discount with all mappings to return complete data
        var result = await GetByIdAsync(createdDiscount.Id);
        return result!;
    }

    /// <summary>
    /// Updates an existing discount with product/category mappings
    /// </summary>
    public async Task<DiscountDto> UpdateAsync(int id, UpdateDiscountDto discountDto)
    {
        var existingDiscount = await _discountRepository.GetByIdAsync(id);
        if (existingDiscount == null)
        {
            throw new InvalidOperationException($"Discount with ID {id} not found");
        }

        // Validate discount data
        var validation = await ValidateDiscountAsync(discountDto, id);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException($"Validation failed: {string.Join(", ", validation.Errors)}");
        }

        UpdateEntityFromDto(existingDiscount, discountDto);
        existingDiscount.UpdatedAt = DateTime.UtcNow;
        
        await _discountRepository.UpdateAsync(existingDiscount);
        
        // Update the product and category mappings
        await UpdateProductDiscountMappings(id, discountDto.SelectedProductIds);
        await UpdateCategoryDiscountMappings(id, discountDto.SelectedCategoryIds);
        
        await _unitOfWork.SaveChangesAsync();
        
        // Reload the discount with all mappings to return complete data
        var result = await GetByIdAsync(id);
        return result!;
    }

    /// <summary>
    /// Deletes a discount (soft delete)
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int? deletedBy = null)
    {
        var result = await _discountRepository.SoftDeleteAsync(id, deletedBy);
        if (result)
        {
            await _unitOfWork.SaveChangesAsync();
        }
        return result;
    }

    /// <summary>
    /// Permanently deletes a discount (hard delete)
    /// </summary>
    public async Task<bool> PermanentDeleteAsync(int id)
    {
        try
        {
            await _discountRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Restores a soft-deleted discount
    /// </summary>
    public async Task<bool> RestoreAsync(int id, int? restoredBy = null)
    {
        var result = await _discountRepository.RestoreAsync(id, restoredBy);
        if (result)
        {
            await _unitOfWork.SaveChangesAsync();
        }
        return result;
    }

    /// <summary>
    /// Activates/deactivates a discount
    /// </summary>
    public async Task<bool> SetActiveStatusAsync(int id, bool isActive, int? updatedBy = null)
    {
        var result = await _discountRepository.SetActiveStatusAsync(id, isActive, updatedBy);
        if (result)
        {
            await _unitOfWork.SaveChangesAsync();
        }
        return result;
    }

    /// <summary>
    /// Checks if discount code exists
    /// </summary>
    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        return await _discountRepository.CodeExistsAsync(code, excludeId);
    }

    /// <summary>
    /// Validates discount data for creation
    /// </summary>
    public async Task<DiscountValidationDto> ValidateDiscountAsync(CreateDiscountDto discountDto, int? excludeId = null)
    {
        var validation = new DiscountValidationDto { IsValid = true };

        // Basic validation
        if (string.IsNullOrWhiteSpace(discountDto.DiscountName))
        {
            validation.AddError("Discount name is required");
        }

        if (string.IsNullOrWhiteSpace(discountDto.DiscountCode))
        {
            validation.AddError("Discount code is required");
        }

        if (discountDto.DiscountValue <= 0)
        {
            validation.AddError("Discount value must be greater than 0");
        }

        if (discountDto.DiscountType == DiscountType.Percentage && discountDto.DiscountValue > 100)
        {
            validation.AddError("Percentage discount cannot be more than 100%");
        }

        if (discountDto.StartDate >= discountDto.EndDate)
        {
            validation.AddError("End date must be after start date");
        }

        if (discountDto.EndDate < DateTime.Today)
        {
            validation.AddWarning("Discount end date is in the past");
        }

        if (discountDto.MaxDiscountAmount.HasValue && discountDto.MaxDiscountAmount <= 0)
        {
            validation.AddError("Max discount amount must be greater than 0");
        }

        if (discountDto.MinPurchaseAmount.HasValue && discountDto.MinPurchaseAmount < 0)
        {
            validation.AddError("Min purchase amount cannot be negative");
        }

        // Check if code already exists
        if (!string.IsNullOrWhiteSpace(discountDto.DiscountCode))
        {
            var codeExists = await CodeExistsAsync(discountDto.DiscountCode, excludeId);
            if (codeExists)
            {
                validation.AddError("Discount code already exists");
            }
        }

        // Product/Category selection validation
        if (discountDto.ApplicableOn == DiscountApplicableOn.Product && 
            (discountDto.SelectedProductIds == null || !discountDto.SelectedProductIds.Any()))
        {
            validation.AddError("At least one product must be selected when discount applies to products");
        }

        if (discountDto.ApplicableOn == DiscountApplicableOn.Category && 
            (discountDto.SelectedCategoryIds == null || !discountDto.SelectedCategoryIds.Any()))
        {
            validation.AddError("At least one category must be selected when discount applies to categories");
        }

        return validation;
    }

    /// <summary>
    /// Validates discount data for update
    /// </summary>
    public async Task<DiscountValidationDto> ValidateDiscountAsync(UpdateDiscountDto discountDto, int? excludeId = null)
    {
        var createDto = new CreateDiscountDto
        {
            DiscountName = discountDto.DiscountName,
            DiscountNameAr = discountDto.DiscountNameAr,
            DiscountDescription = discountDto.DiscountDescription,
            DiscountCode = discountDto.DiscountCode,
            DiscountType = discountDto.DiscountType,
            DiscountValue = discountDto.DiscountValue,
            MaxDiscountAmount = discountDto.MaxDiscountAmount,
            MinPurchaseAmount = discountDto.MinPurchaseAmount,
            StartDate = discountDto.StartDate,
            EndDate = discountDto.EndDate,
            ApplicableOn = discountDto.ApplicableOn,
            SelectedProductIds = discountDto.SelectedProductIds,
            SelectedCategoryIds = discountDto.SelectedCategoryIds,
            Priority = discountDto.Priority,
            IsStackable = discountDto.IsStackable,
            IsActive = discountDto.IsActive,
            StoreId = discountDto.StoreId,
            CurrencyCode = discountDto.CurrencyCode
        };

        return await ValidateDiscountAsync(createDto, excludeId);
    }

    /// <summary>
    /// Gets applicable discounts for a specific order
    /// </summary>
    public async Task<IEnumerable<DiscountDto>> GetApplicableDiscountsAsync(
        decimal orderAmount, 
        int? customerId = null, 
        IEnumerable<int>? productIds = null, 
        IEnumerable<int>? categoryIds = null)
    {
        var discounts = await _discountRepository.GetApplicableDiscountsAsync(orderAmount, customerId, productIds, categoryIds);
        return discounts.Select(MapToDto);
    }

    /// <summary>
    /// Calculates discount amount for given order details
    /// </summary>
    public async Task<decimal> CalculateDiscountAmountAsync(
        int discountId, 
        decimal orderAmount, 
        int? customerId = null, 
        IEnumerable<int>? productIds = null, 
        IEnumerable<int>? categoryIds = null)
    {
        var discount = await _discountRepository.GetByIdAsync(discountId);
        if (discount == null || !discount.IsCurrentlyActive)
        {
            return 0;
        }

        // Check minimum purchase amount
        if (discount.MinPurchaseAmount.HasValue && orderAmount < discount.MinPurchaseAmount.Value)
        {
            return 0;
        }

        // Calculate discount amount
        decimal discountAmount = 0;
        if (discount.DiscountType == DiscountType.Percentage)
        {
            discountAmount = orderAmount * (discount.DiscountValue / 100);
        }
        else
        {
            discountAmount = discount.DiscountValue;
        }

        // Apply maximum discount amount cap
        if (discount.MaxDiscountAmount.HasValue && discountAmount > discount.MaxDiscountAmount.Value)
        {
            discountAmount = discount.MaxDiscountAmount.Value;
        }

        return discountAmount;
    }

    /// <summary>
    /// Gets discounts expiring within specified days
    /// </summary>
    public async Task<IEnumerable<DiscountDto>> GetExpiringDiscountsAsync(int days = 7)
    {
        var discounts = await _discountRepository.GetExpiringDiscountsAsync(days);
        return discounts.Select(MapToDto);
    }

    /// <summary>
    /// Gets discount usage statistics (mock implementation)
    /// </summary>
    public async Task<DiscountUsageStatsDto> GetUsageStatsAsync(int discountId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var discount = await _discountRepository.GetByIdAsync(discountId);
        if (discount == null)
        {
            throw new InvalidOperationException($"Discount with ID {discountId} not found");
        }

        // This is a mock implementation - in a real scenario, you would query sales/usage data
        return new DiscountUsageStatsDto
        {
            DiscountId = discount.Id,
            DiscountName = discount.DiscountName,
            DiscountCode = discount.DiscountCode,
            TotalUsages = 0, // Would come from sales data
            TotalDiscountAmount = 0, // Would come from sales data
            AverageDiscountAmount = 0, // Would come from sales data
            FirstUsed = null, // Would come from sales data
            LastUsed = null, // Would come from sales data
            DailyUsages = new List<DailyUsageDto>() // Would come from sales data
        };
    }

    #region Private Mapping Methods

    /// <summary>
    /// Creates product discount mappings for the specified discount
    /// </summary>
    private async Task CreateProductDiscountMappings(int discountId, List<int> productIds)
    {
        if (productIds?.Any() == true)
        {
            foreach (var productId in productIds)
            {
                var productDiscount = new ProductDiscount
                {
                    DiscountsId = discountId,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.ProductDiscounts.AddAsync(productDiscount);
            }
        }
    }

    /// <summary>
    /// Creates category discount mappings for the specified discount
    /// </summary>
    private async Task CreateCategoryDiscountMappings(int discountId, List<int> categoryIds)
    {
        if (categoryIds?.Any() == true)
        {
            foreach (var categoryId in categoryIds)
            {
                var categoryDiscount = new CategoryDiscount
                {
                    DiscountsId = discountId,
                    CategoryId = categoryId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.CategoryDiscounts.AddAsync(categoryDiscount);
            }
        }
    }

    /// <summary>
    /// Updates product discount mappings for the specified discount
    /// </summary>
    private async Task UpdateProductDiscountMappings(int discountId, List<int> productIds)
    {
        // Remove existing mappings
        var existingMappings = await _unitOfWork.ProductDiscounts.GetProductsByDiscountIdAsync(discountId);
        foreach (var mapping in existingMappings)
        {
            await _unitOfWork.ProductDiscounts.DeleteAsync(mapping.Id);
        }

        // Create new mappings
        await CreateProductDiscountMappings(discountId, productIds);
    }

    /// <summary>
    /// Updates category discount mappings for the specified discount
    /// </summary>
    private async Task UpdateCategoryDiscountMappings(int discountId, List<int> categoryIds)
    {
        // Remove existing mappings
        var existingMappings = await _unitOfWork.CategoryDiscounts.GetCategoriesByDiscountIdAsync(discountId);
        foreach (var mapping in existingMappings)
        {
            await _unitOfWork.CategoryDiscounts.DeleteAsync(mapping.Id);
        }

        // Create new mappings
        await CreateCategoryDiscountMappings(discountId, categoryIds);
    }

    /// <summary>
    /// Maps Discount entity to DiscountDto with selected product/category IDs
    /// </summary>
    private async Task<DiscountDto> MapToDtoWithSelections(Discount discount)
    {
        Log($"DiscountService.MapToDtoWithSelections: Starting for discount ID {discount.Id}");
        
        var dto = MapToDto(discount);
        Log($"DiscountService.MapToDtoWithSelections: Basic DTO mapped - ApplicableOn = {dto.ApplicableOn}");
        
        // Populate selected product IDs
        Log("DiscountService.MapToDtoWithSelections: Loading ProductDiscount mappings...");
        var productMappings = await _unitOfWork.ProductDiscounts.GetProductsByDiscountIdAsync(discount.Id);
        dto.SelectedProductIds = productMappings.Select(pm => pm.ProductId).ToList();
        Log($"DiscountService.MapToDtoWithSelections: Found {dto.SelectedProductIds.Count} ProductDiscount mappings");
        
        if (dto.SelectedProductIds.Any())
        {
            Log($"DiscountService.MapToDtoWithSelections: SelectedProductIds = [{string.Join(", ", dto.SelectedProductIds)}]");
        }
        
        // Populate selected category IDs
        Log("DiscountService.MapToDtoWithSelections: Loading CategoryDiscount mappings...");
        var categoryMappings = await _unitOfWork.CategoryDiscounts.GetCategoriesByDiscountIdAsync(discount.Id);
        dto.SelectedCategoryIds = categoryMappings.Select(cm => cm.CategoryId).ToList();
        Log($"DiscountService.MapToDtoWithSelections: Found {dto.SelectedCategoryIds.Count} CategoryDiscount mappings");
        
        if (dto.SelectedCategoryIds.Any())
        {
            Log($"DiscountService.MapToDtoWithSelections: SelectedCategoryIds = [{string.Join(", ", dto.SelectedCategoryIds)}]");
        }
        
        Log($"DiscountService.MapToDtoWithSelections: Completed - returning DTO with {dto.SelectedProductIds.Count} products and {dto.SelectedCategoryIds.Count} categories");
        return dto;
    }

    /// <summary>
    /// Maps Discount entity to DiscountDto
    /// </summary>
    private static DiscountDto MapToDto(Discount discount)
    {
        return new DiscountDto
        {
            Id = discount.Id,
            DiscountName = discount.DiscountName,
            DiscountNameAr = discount.DiscountNameAr,
            DiscountDescription = discount.DiscountDescription,
            DiscountCode = discount.DiscountCode,
            DiscountType = discount.DiscountType,
            DiscountValue = discount.DiscountValue,
            MaxDiscountAmount = discount.MaxDiscountAmount,
            MinPurchaseAmount = discount.MinPurchaseAmount,
            StartDate = discount.StartDate,
            EndDate = discount.EndDate,
            ApplicableOn = discount.ApplicableOn,
            Priority = discount.Priority,
            IsStackable = discount.IsStackable,
            IsActive = discount.IsActive,
            CreatedBy = discount.CreatedBy,
            CreatedByName = discount.CreatedByUser?.FullName, // Using FullName property from User
            CreatedAt = discount.CreatedAt,
            UpdatedBy = discount.UpdatedBy,
            UpdatedByName = discount.UpdatedByUser?.FullName,
            UpdatedAt = discount.UpdatedAt,
            DeletedAt = discount.DeletedAt,
            DeletedBy = discount.DeletedBy,
            DeletedByName = discount.DeletedByUser?.FullName,
            StoreId = discount.StoreId,
            StoreName = discount.Store?.Name, // Assuming Store has Name property
            CurrencyCode = discount.CurrencyCode
        };
    }

    /// <summary>
    /// Maps CreateDiscountDto to Discount entity
    /// </summary>
    private static Discount MapToEntity(CreateDiscountDto dto)
    {
        return new Discount
        {
            DiscountName = dto.DiscountName,
            DiscountNameAr = dto.DiscountNameAr,
            DiscountDescription = dto.DiscountDescription,
            DiscountCode = dto.DiscountCode,
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            MinPurchaseAmount = dto.MinPurchaseAmount,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            ApplicableOn = dto.ApplicableOn,
            Priority = dto.Priority,
            IsStackable = dto.IsStackable,
            IsActive = dto.IsActive,
            StoreId = dto.StoreId,
            CurrencyCode = dto.CurrencyCode,
            CreatedBy = dto.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates Discount entity from UpdateDiscountDto
    /// </summary>
    private static void UpdateEntityFromDto(Discount entity, UpdateDiscountDto dto)
    {
        entity.DiscountName = dto.DiscountName;
        entity.DiscountNameAr = dto.DiscountNameAr;
        entity.DiscountDescription = dto.DiscountDescription;
        entity.DiscountCode = dto.DiscountCode;
        entity.DiscountType = dto.DiscountType;
        entity.DiscountValue = dto.DiscountValue;
        entity.MaxDiscountAmount = dto.MaxDiscountAmount;
        entity.MinPurchaseAmount = dto.MinPurchaseAmount;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.ApplicableOn = dto.ApplicableOn;
        entity.Priority = dto.Priority;
        entity.IsStackable = dto.IsStackable;
        entity.IsActive = dto.IsActive;
        entity.StoreId = dto.StoreId;
        entity.CurrencyCode = dto.CurrencyCode;
        entity.UpdatedBy = dto.UpdatedBy;
    }

    #endregion
}