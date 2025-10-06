using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Product Group Item operations
/// </summary>
public class ProductGroupItemService : IProductGroupItemService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductGroupItemService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProductGroupItemDto>> GetAllAsync()
    {
        var items = await _unitOfWork.ProductGroupItems.GetAllAsync();
        return items.Select(MapToDto);
    }

    public async Task<ProductGroupItemDto?> GetByIdAsync(int id)
    {
        var item = await _unitOfWork.ProductGroupItems.GetByIdAsync(id);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<IEnumerable<ProductGroupItemDto>> GetByProductGroupIdAsync(int productGroupId)
    {
        var items = await _unitOfWork.ProductGroupItems.GetAllAsync();
        return items
            .Where(i => i.ProductGroupId == productGroupId)
            .Select(MapToDto);
    }

    public async Task<ProductGroupItemDto> CreateAsync(CreateProductGroupItemDto createDto)
    {
        var item = new ProductGroupItem
        {
            ProductGroupId = createDto.ProductGroupId,
            ProductId = createDto.ProductId,
            ProductUnitId = createDto.ProductUnitId,
            ProductCombinationId = createDto.ProductCombinationId,
            Quantity = createDto.Quantity,
            PriceAdjustment = createDto.PriceAdjustment,
            DiscountId = createDto.DiscountId,
            TaxTypeId = createDto.TaxTypeId,
            SellingPriceTypeId = createDto.SellingPriceTypeId,
            Status = createDto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ProductGroupItems.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(item);
    }

    public async Task<ProductGroupItemDto> UpdateAsync(UpdateProductGroupItemDto updateDto)
    {
        var item = await _unitOfWork.ProductGroupItems.GetByIdAsync(updateDto.Id);
        if (item == null)
        {
            throw new InvalidOperationException($"Product group item with ID {updateDto.Id} not found.");
        }

        item.ProductGroupId = updateDto.ProductGroupId;
        item.ProductId = updateDto.ProductId;
        item.ProductUnitId = updateDto.ProductUnitId;
        item.ProductCombinationId = updateDto.ProductCombinationId;
        item.Quantity = updateDto.Quantity;
        item.PriceAdjustment = updateDto.PriceAdjustment;
        item.DiscountId = updateDto.DiscountId;
        item.TaxTypeId = updateDto.TaxTypeId;
        item.SellingPriceTypeId = updateDto.SellingPriceTypeId;
        item.Status = updateDto.Status;
        item.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ProductGroupItems.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(item);
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _unitOfWork.ProductGroupItems.GetByIdAsync(id);
        if (item == null)
        {
            throw new InvalidOperationException($"Product group item with ID {id} not found.");
        }

        await _unitOfWork.ProductGroupItems.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> GetCountByProductGroupIdAsync(int productGroupId)
    {
        var items = await _unitOfWork.ProductGroupItems.GetAllAsync();
        return items.Count(i => i.ProductGroupId == productGroupId);
    }

    private static ProductGroupItemDto MapToDto(ProductGroupItem item)
    {
        return new ProductGroupItemDto
        {
            Id = item.Id,
            ProductGroupId = item.ProductGroupId,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name,
            ProductCode = item.Product?.Code,
            ProductUnitId = item.ProductUnitId,
            ProductCombinationId = item.ProductCombinationId,
            Quantity = item.Quantity,
            PriceAdjustment = item.PriceAdjustment,
            DiscountId = item.DiscountId,
            DiscountName = item.Discount?.DiscountName,
            TaxTypeId = item.TaxTypeId,
            TaxTypeName = item.TaxType?.Name,
            SellingPriceTypeId = item.SellingPriceTypeId,
            SellingPriceTypeName = item.SellingPriceType?.TypeName,
            Status = item.Status,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            ProductPrice = item.Product?.Price ?? 0,
            CalculatedPrice = (item.Product?.Price ?? 0) + item.PriceAdjustment
        };
    }
}
