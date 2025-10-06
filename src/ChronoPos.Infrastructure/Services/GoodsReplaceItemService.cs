using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Logging;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Service implementation for goods replace item operations
/// </summary>
public class GoodsReplaceItemService : IGoodsReplaceItemService
{
    private readonly ChronoPosDbContext _context;
    private readonly IGoodsReplaceItemRepository _goodsReplaceItemRepository;

    public GoodsReplaceItemService(
        ChronoPosDbContext context,
        IGoodsReplaceItemRepository goodsReplaceItemRepository)
    {
        _context = context;
        _goodsReplaceItemRepository = goodsReplaceItemRepository;
    }

    /// <summary>
    /// Get goods replace items by replace ID
    /// </summary>
    public async Task<List<GoodsReplaceItemDto>> GetItemsByReplaceIdAsync(int replaceId)
    {
        var items = await _goodsReplaceItemRepository.GetByReplaceIdAsync(replaceId);

        return items.Select(item => new GoodsReplaceItemDto
        {
            Id = item.Id,
            ReplaceId = item.ReplaceId,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? "",
            UomId = item.UomId,
            UomName = item.Uom?.Name ?? "",
            BatchNo = item.BatchNo,
            ExpiryDate = item.ExpiryDate,
            Quantity = item.Quantity,
            Rate = item.Rate,
            ReferenceReturnItemId = item.ReferenceReturnItemId,
            RemarksLine = item.RemarksLine,
            CreatedAt = item.CreatedAt
        }).ToList();
    }

    /// <summary>
    /// Get goods replace item by ID
    /// </summary>
    public async Task<GoodsReplaceItemDto?> GetItemByIdAsync(int itemId)
    {
        var item = await _goodsReplaceItemRepository.GetByIdAsync(itemId);

        if (item == null)
            return null;

        return new GoodsReplaceItemDto
        {
            Id = item.Id,
            ReplaceId = item.ReplaceId,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? "",
            UomId = item.UomId,
            UomName = item.Uom?.Name ?? "",
            BatchNo = item.BatchNo,
            ExpiryDate = item.ExpiryDate,
            Quantity = item.Quantity,
            Rate = item.Rate,
            ReferenceReturnItemId = item.ReferenceReturnItemId,
            RemarksLine = item.RemarksLine,
            CreatedAt = item.CreatedAt
        };
    }

    /// <summary>
    /// Get detailed goods replace item by ID
    /// </summary>
    public async Task<GoodsReplaceItemDetailDto?> GetItemDetailAsync(int itemId)
    {
        var item = await _goodsReplaceItemRepository.GetWithDetailsAsync(itemId);

        if (item == null)
            return null;

        return new GoodsReplaceItemDetailDto
        {
            Id = item.Id,
            ReplaceId = item.ReplaceId,
            ReplaceNo = item.Replace?.ReplaceNo ?? "",
            ReplaceDate = item.Replace?.ReplaceDate ?? DateTime.MinValue,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? "",
            ProductCode = "", // TODO: Add product code field
            CategoryName = item.Product?.Category?.Name ?? "",
            BrandName = "", // TODO: Add brand if needed
            UomId = item.UomId,
            UomName = item.Uom?.Name ?? "",
            BatchNo = item.BatchNo,
            ExpiryDate = item.ExpiryDate,
            Quantity = item.Quantity,
            Rate = item.Rate,
            ReferenceReturnItemId = item.ReferenceReturnItemId,
            RemarksLine = item.RemarksLine,
            SupplierName = item.Replace?.Supplier?.CompanyName ?? "",
            StoreName = item.Replace?.Store?.Name ?? "",
            ReplaceStatus = item.Replace?.Status ?? ""
        };
    }

    /// <summary>
    /// Update goods replace item
    /// </summary>
    public async Task<GoodsReplaceItemDto> UpdateItemAsync(int itemId, UpdateGoodsReplaceItemDto dto)
    {
        var item = await _goodsReplaceItemRepository.GetByIdAsync(itemId);

        if (item == null)
            throw new ArgumentException("Goods replace item not found", nameof(itemId));

        // Check if parent replace is in pending status
        var replace = await _context.GoodsReplaces.FindAsync(item.ReplaceId);
        if (replace == null || replace.Status != "Pending")
            throw new InvalidOperationException("Cannot update item for goods replace that is not in pending status");

        // Update item properties
        item.ProductId = dto.ProductId;
        item.UomId = dto.UomId;
        item.BatchNo = dto.BatchNo;
        item.ExpiryDate = dto.ExpiryDate;
        item.Quantity = dto.Quantity;
        item.Rate = dto.Rate;
        item.ReferenceReturnItemId = dto.ReferenceReturnItemId;
        item.RemarksLine = dto.RemarksLine;

        _context.GoodsReplaceItems.Update(item);
        await _context.SaveChangesAsync();

        // Recalculate total amount for parent replace
        await RecalculateTotalAmountAsync(item.ReplaceId);

        return await GetItemByIdAsync(itemId) 
            ?? throw new InvalidOperationException("Failed to retrieve updated item");
    }

    /// <summary>
    /// Delete goods replace item
    /// </summary>
    public async Task<bool> DeleteItemAsync(int itemId)
    {
        var item = await _goodsReplaceItemRepository.GetByIdAsync(itemId);

        if (item == null)
            return false;

        // Check if parent replace is in pending status
        var replace = await _context.GoodsReplaces.FindAsync(item.ReplaceId);
        if (replace == null || replace.Status != "Pending")
            return false;

        _context.GoodsReplaceItems.Remove(item);
        await _context.SaveChangesAsync();

        // Recalculate total amount for parent replace
        await RecalculateTotalAmountAsync(item.ReplaceId);

        return true;
    }

    /// <summary>
    /// Get summary statistics for goods replace items
    /// </summary>
    public async Task<GoodsReplaceItemSummaryDto> GetItemsSummaryAsync(int replaceId)
    {
        var items = await _goodsReplaceItemRepository.GetByReplaceIdAsync(replaceId);

        var summary = new GoodsReplaceItemSummaryDto
        {
            TotalItems = items.Count(),
            TotalQuantity = items.Sum(i => i.Quantity),
            TotalAmount = items.Sum(i => i.Amount),
            AverageRate = items.Any() ? items.Average(i => i.Rate) : 0
        };

        return summary;
    }

    /// <summary>
    /// Recalculate total amount for goods replace
    /// </summary>
    private async Task RecalculateTotalAmountAsync(int replaceId)
    {
        var replace = await _context.GoodsReplaces
            .Include(gr => gr.Items)
            .FirstOrDefaultAsync(gr => gr.Id == replaceId);

        if (replace != null)
        {
            replace.TotalAmount = replace.Items.Sum(i => i.Amount);
            replace.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
