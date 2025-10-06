using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service for managing Product Groups
/// </summary>
public class ProductGroupService : IProductGroupService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductGroupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all product groups
    /// </summary>
    public async Task<IEnumerable<ProductGroupDto>> GetAllAsync()
    {
        var groups = await _unitOfWork.ProductGroups.GetAllAsync();
        return groups.Where(g => !g.IsDeleted)
                    .Select(Map)
                    .OrderBy(g => g.Name);
    }

    /// <summary>
    /// Get product group by ID
    /// </summary>
    public async Task<ProductGroupDto?> GetByIdAsync(int id)
    {
        var group = await _unitOfWork.ProductGroups.GetByIdAsync(id);
        if (group == null || group.IsDeleted)
            return null;

        return Map(group);
    }

    /// <summary>
    /// Get detailed product group with all items
    /// </summary>
    public async Task<ProductGroupDetailDto?> GetDetailByIdAsync(int id)
    {
        var productGroup = await _unitOfWork.ProductGroups.GetByIdAsync(id);
        
        if (productGroup == null || productGroup.IsDeleted)
            return null;

        var detail = new ProductGroupDetailDto
        {
            Id = productGroup.Id,
            Name = productGroup.Name,
            NameAr = productGroup.NameAr,
            Description = productGroup.Description,
            DescriptionAr = productGroup.DescriptionAr,
            DiscountId = productGroup.DiscountId,
            DiscountName = productGroup.Discount?.DiscountName,
            TaxTypeId = productGroup.TaxTypeId,
            TaxTypeName = productGroup.TaxType?.Name,
            PriceTypeId = productGroup.PriceTypeId,
            PriceTypeName = productGroup.PriceType?.TypeName,
            SkuPrefix = productGroup.SkuPrefix,
            Status = productGroup.Status,
            CreatedDate = productGroup.CreatedDate,
            ModifiedDate = productGroup.ModifiedDate,
            Items = new List<ProductGroupItemDto>()
        };

        // Load items if they exist
        if (productGroup.ProductGroupItems != null && productGroup.ProductGroupItems.Any())
        {
            foreach (var item in productGroup.ProductGroupItems.OrderBy(i => i.Id))
            {
                if (item.ProductId.HasValue)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId.Value);
                    if (product != null)
                    {
                        var itemDto = MapItem(item, product);
                        detail.Items.Add(itemDto);
                    }
                }
            }
        }

        // Calculate total price
        detail.TotalPrice = detail.Items.Sum(i => i.CalculatedPrice * i.Quantity);

        return detail;
    }

    /// <summary>
    /// Get only active product groups
    /// </summary>
    public async Task<IEnumerable<ProductGroupDto>> GetActiveAsync()
    {
        var allGroups = await _unitOfWork.ProductGroups.GetAllAsync();
        var groups = allGroups.Where(g => g.Status == "Active" && !g.IsDeleted);

        return groups.Select(Map).OrderBy(g => g.Name);
    }

    /// <summary>
    /// Create a new product group
    /// </summary>
    public async Task<ProductGroupDto> CreateAsync(CreateProductGroupDto dto)
    {
        // Validate unique name
        if (await ExistsAsync(dto.Name))
        {
            throw new InvalidOperationException($"Product group with name '{dto.Name}' already exists.");
        }

        var group = new ProductGroup
        {
            Name = dto.Name,
            NameAr = dto.NameAr,
            Description = dto.Description,
            DescriptionAr = dto.DescriptionAr,
            DiscountId = dto.DiscountId,
            TaxTypeId = dto.TaxTypeId,
            PriceTypeId = dto.PriceTypeId,
            SkuPrefix = dto.SkuPrefix,
            Status = dto.Status,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        await _unitOfWork.ProductGroups.AddAsync(group);
        await _unitOfWork.SaveChangesAsync();

        return Map(group);
    }

    /// <summary>
    /// Update an existing product group
    /// </summary>
    public async Task<ProductGroupDto> UpdateAsync(UpdateProductGroupDto dto)
    {
        var group = await _unitOfWork.ProductGroups.GetByIdAsync(dto.Id);
        if (group == null || group.IsDeleted)
        {
            throw new InvalidOperationException("Product group not found.");
        }

        // Validate unique name (excluding current group)
        if (await ExistsAsync(dto.Name, dto.Id))
        {
            throw new InvalidOperationException($"Product group with name '{dto.Name}' already exists.");
        }

        group.Name = dto.Name;
        group.NameAr = dto.NameAr;
        group.Description = dto.Description;
        group.DescriptionAr = dto.DescriptionAr;
        group.DiscountId = dto.DiscountId;
        group.TaxTypeId = dto.TaxTypeId;
        group.PriceTypeId = dto.PriceTypeId;
        group.SkuPrefix = dto.SkuPrefix;
        group.Status = dto.Status;
        group.ModifiedDate = DateTime.UtcNow;

        await _unitOfWork.ProductGroups.UpdateAsync(group);
        await _unitOfWork.SaveChangesAsync();

        return Map(group);
    }

    /// <summary>
    /// Delete a product group (soft delete)
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var group = await _unitOfWork.ProductGroups.GetByIdAsync(id);
        if (group == null || group.IsDeleted)
        {
            return false;
        }

        // Soft delete
        group.IsDeleted = true;
        group.DeletedDate = DateTime.UtcNow;

        await _unitOfWork.ProductGroups.UpdateAsync(group);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Search product groups by name
    /// </summary>
    public async Task<IEnumerable<ProductGroupDto>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync();
        }

        var allGroups = await _unitOfWork.ProductGroups.GetAllAsync();
        var groups = allGroups
            .Where(g => !g.IsDeleted &&
                       (g.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (g.NameAr != null && g.NameAr.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (g.Description != null && g.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))));

        return groups.Select(Map).OrderBy(g => g.Name);
    }

    /// <summary>
    /// Get all items in a product group
    /// </summary>
    public async Task<IEnumerable<ProductGroupItemDto>> GetGroupItemsAsync(int groupId)
    {
        var productGroup = await _unitOfWork.ProductGroups.GetByIdAsync(groupId);

        if (productGroup == null || productGroup.IsDeleted || productGroup.ProductGroupItems == null)
        {
            return Enumerable.Empty<ProductGroupItemDto>();
        }

        var items = new List<ProductGroupItemDto>();
        foreach (var item in productGroup.ProductGroupItems.OrderBy(i => i.Id))
        {
            if (item.ProductId.HasValue)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId.Value);
                if (product != null)
                {
                    items.Add(MapItem(item, product));
                }
            }
        }

        return items;
    }

    /// <summary>
    /// Add an item to a product group
    /// </summary>
    public async Task<ProductGroupItemDto> AddItemToGroupAsync(CreateProductGroupItemDto dto)
    {
        // Validate group exists
        var group = await _unitOfWork.ProductGroups.GetByIdAsync(dto.ProductGroupId);
        if (group == null || group.IsDeleted)
        {
            throw new InvalidOperationException("Product group not found.");
        }

        // Validate product exists if provided
        Product? product = null;
        if (dto.ProductId.HasValue)
        {
            product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId.Value);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }
        }

        var item = new ProductGroupItem
        {
            ProductGroupId = dto.ProductGroupId,
            ProductId = dto.ProductId,
            ProductUnitId = dto.ProductUnitId,
            ProductCombinationId = dto.ProductCombinationId,
            Quantity = dto.Quantity,
            PriceAdjustment = dto.PriceAdjustment,
            DiscountId = dto.DiscountId,
            TaxTypeId = dto.TaxTypeId,
            SellingPriceTypeId = dto.SellingPriceTypeId,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ProductGroupItems.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        if (product != null)
        {
            return MapItem(item, product);
        }
        
        // Return basic DTO if no product
        return new ProductGroupItemDto
        {
            Id = item.Id,
            ProductGroupId = item.ProductGroupId,
            ProductId = item.ProductId,
            ProductUnitId = item.ProductUnitId,
            ProductCombinationId = item.ProductCombinationId,
            Quantity = item.Quantity,
            PriceAdjustment = item.PriceAdjustment,
            DiscountId = item.DiscountId,
            TaxTypeId = item.TaxTypeId,
            SellingPriceTypeId = item.SellingPriceTypeId,
            Status = item.Status,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }

    /// <summary>
    /// Remove an item from a product group
    /// </summary>
    public async Task<bool> RemoveItemFromGroupAsync(int itemId)
    {
        var item = await _unitOfWork.ProductGroupItems.GetByIdAsync(itemId);
        if (item == null)
        {
            return false;
        }

        await _unitOfWork.ProductGroupItems.DeleteAsync(itemId);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Update a product group item
    /// </summary>
    public async Task<ProductGroupItemDto> UpdateItemAsync(UpdateProductGroupItemDto dto)
    {
        var item = await _unitOfWork.ProductGroupItems.GetByIdAsync(dto.Id);
        if (item == null)
        {
            throw new InvalidOperationException("Product group item not found.");
        }

        Product? product = null;
        if (dto.ProductId.HasValue)
        {
            product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId.Value);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found.");
            }
        }

        item.ProductId = dto.ProductId;
        item.ProductUnitId = dto.ProductUnitId;
        item.ProductCombinationId = dto.ProductCombinationId;
        item.Quantity = dto.Quantity;
        item.PriceAdjustment = dto.PriceAdjustment;
        item.DiscountId = dto.DiscountId;
        item.TaxTypeId = dto.TaxTypeId;
        item.SellingPriceTypeId = dto.SellingPriceTypeId;
        item.Status = dto.Status;
        item.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ProductGroupItems.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();

        if (product != null)
        {
            return MapItem(item, product);
        }
        
        // Return basic DTO if no product
        return new ProductGroupItemDto
        {
            Id = item.Id,
            ProductGroupId = item.ProductGroupId,
            ProductId = item.ProductId,
            ProductUnitId = item.ProductUnitId,
            ProductCombinationId = item.ProductCombinationId,
            Quantity = item.Quantity,
            PriceAdjustment = item.PriceAdjustment,
            DiscountId = item.DiscountId,
            TaxTypeId = item.TaxTypeId,
            SellingPriceTypeId = item.SellingPriceTypeId,
            Status = item.Status,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }

    /// <summary>
    /// Get count of product groups
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        var groups = await _unitOfWork.ProductGroups.GetAllAsync();
        return groups.Count(g => !g.IsDeleted);
    }

    /// <summary>
    /// Check if product group name already exists
    /// </summary>
    public async Task<bool> ExistsAsync(string name, int? excludeId = null)
    {
        var allGroups = await _unitOfWork.ProductGroups.GetAllAsync();
        var groups = allGroups.Where(g => !g.IsDeleted && g.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
        {
            groups = groups.Where(g => g.Id != excludeId.Value);
        }

        return groups.Any();
    }

    /// <summary>
    /// Calculate total price for a product group
    /// </summary>
    public async Task<decimal> CalculateGroupPriceAsync(int groupId)
    {
        var items = await GetGroupItemsAsync(groupId);
        return items.Sum(i => i.CalculatedPrice * i.Quantity);
    }

    /// <summary>
    /// Map ProductGroup entity to DTO
    /// </summary>
    private ProductGroupDto Map(ProductGroup group)
    {
        return new ProductGroupDto
        {
            Id = group.Id,
            Name = group.Name,
            NameAr = group.NameAr,
            Description = group.Description,
            DescriptionAr = group.DescriptionAr,
            DiscountId = group.DiscountId,
            DiscountName = group.Discount?.DiscountName,
            TaxTypeId = group.TaxTypeId,
            TaxTypeName = group.TaxType?.Name,
            PriceTypeId = group.PriceTypeId,
            PriceTypeName = group.PriceType?.TypeName,
            SkuPrefix = group.SkuPrefix,
            Status = group.Status,
            ItemCount = group.ProductGroupItems?.Count ?? 0,
            CreatedDate = group.CreatedDate,
            ModifiedDate = group.ModifiedDate,
            CreatedByName = group.CreatedByUser?.FullName
        };
    }

    /// <summary>
    /// Map ProductGroupItem entity to DTO
    /// </summary>
    private ProductGroupItemDto MapItem(ProductGroupItem item, Product product)
    {
        var basePrice = product.Price;
        var calculatedPrice = basePrice + item.PriceAdjustment;

        return new ProductGroupItemDto
        {
            Id = item.Id,
            ProductGroupId = item.ProductGroupId,
            ProductId = item.ProductId,
            ProductName = product.Name,
            ProductCode = product.Code,
            ProductUnitId = item.ProductUnitId,
            ProductCombinationId = item.ProductCombinationId,
            Quantity = item.Quantity,
            PriceAdjustment = item.PriceAdjustment,
            DiscountId = item.DiscountId,
            DiscountName = item.Discount?.DiscountName,
            TaxTypeId = item.TaxTypeId,
            SellingPriceTypeId = item.SellingPriceTypeId,
            Status = item.Status,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            ProductPrice = basePrice,
            CalculatedPrice = calculatedPrice
        };
    }
}
