using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for StockTransferItem operations
/// </summary>
public class StockTransferItemService : IStockTransferItemService
{
    private readonly IStockTransferItemRepository _stockTransferItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StockTransferItemService(
        IStockTransferItemRepository stockTransferItemRepository, 
        IUnitOfWork unitOfWork)
    {
        _stockTransferItemRepository = stockTransferItemRepository ?? throw new ArgumentNullException(nameof(stockTransferItemRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all stock transfer items
    /// </summary>
    /// <returns>Collection of stock transfer item DTOs</returns>
    public async Task<IEnumerable<StockTransferItemDto>> GetAllAsync()
    {
        var items = await _stockTransferItemRepository.GetAllAsync();
        return items.Select(MapToDto);
    }

    /// <summary>
    /// Gets stock transfer item by ID
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <returns>Stock transfer item DTO if found</returns>
    public async Task<StockTransferItemDto?> GetByIdAsync(int id)
    {
        var item = await _stockTransferItemRepository.GetByIdAsync(id);
        return item != null ? MapToDto(item) : null;
    }

    /// <summary>
    /// Gets stock transfer items by transfer ID
    /// </summary>
    /// <param name="transferId">Transfer ID</param>
    /// <returns>Collection of stock transfer item DTOs</returns>
    public async Task<IEnumerable<StockTransferItemDto>> GetByTransferIdAsync(int transferId)
    {
        var items = await _stockTransferItemRepository.GetByTransferIdAsync(transferId);
        return items.Select(MapToDto);
    }

    /// <summary>
    /// Gets stock transfer items by product ID
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of stock transfer item DTOs</returns>
    public async Task<IEnumerable<StockTransferItemDto>> GetByProductIdAsync(int productId)
    {
        var items = await _stockTransferItemRepository.GetByProductIdAsync(productId);
        return items.Select(MapToDto);
    }

    /// <summary>
    /// Gets stock transfer items by status
    /// </summary>
    /// <param name="status">Item status</param>
    /// <returns>Collection of stock transfer item DTOs</returns>
    public async Task<IEnumerable<StockTransferItemDto>> GetByStatusAsync(string status)
    {
        var items = await _stockTransferItemRepository.GetByStatusAsync(status);
        return items.Select(MapToDto);
    }

    /// <summary>
    /// Creates a new stock transfer item
    /// </summary>
    /// <param name="createItemDto">Stock transfer item data</param>
    /// <returns>Created stock transfer item DTO</returns>
    public async Task<StockTransferItemDto> CreateAsync(CreateStockTransferItemDto createItemDto)
    {
        var item = new StockTransferItem
        {
            TransferId = createItemDto.TransferId,
            ProductId = createItemDto.ProductId,
            UomId = createItemDto.UomId,
            BatchNo = createItemDto.BatchNo?.Trim(),
            ExpiryDate = createItemDto.ExpiryDate,
            QuantitySent = createItemDto.QuantitySent,
            QuantityReceived = 0,
            DamagedQty = 0,
            Status = "Pending",
            RemarksLine = createItemDto.RemarksLine?.Trim()
        };

        await _stockTransferItemRepository.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(item);
    }

    /// <summary>
    /// Updates an existing stock transfer item
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="updateItemDto">Updated item data</param>
    /// <returns>Updated stock transfer item DTO</returns>
    public async Task<StockTransferItemDto> UpdateAsync(int id, UpdateStockTransferItemDto updateItemDto)
    {
        var item = await _stockTransferItemRepository.GetByIdAsync(id);
        if (item == null)
        {
            throw new ArgumentException($"Stock transfer item with ID {id} not found");
        }

        // Check if the transfer is still in pending status
        if (item.Transfer?.Status != "Pending")
        {
            throw new InvalidOperationException("Cannot update items for transfers that are not in pending status");
        }

        item.ProductId = updateItemDto.ProductId;
        item.UomId = updateItemDto.UomId;
        item.BatchNo = updateItemDto.BatchNo?.Trim();
        item.ExpiryDate = updateItemDto.ExpiryDate;
        item.QuantitySent = updateItemDto.QuantitySent;
        item.QuantityReceived = updateItemDto.QuantityReceived;
        item.DamagedQty = updateItemDto.DamagedQty;
        item.Status = updateItemDto.Status;
        item.RemarksLine = updateItemDto.RemarksLine?.Trim();

        await _stockTransferItemRepository.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(item);
    }

    /// <summary>
    /// Updates item status
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="statusDto">Status update data</param>
    /// <returns>Updated stock transfer item DTO</returns>
    public async Task<StockTransferItemDto> UpdateStatusAsync(int id, StockTransferItemStatusDto statusDto)
    {
        var item = await _stockTransferItemRepository.GetByIdAsync(id);
        if (item == null)
        {
            throw new ArgumentException($"Stock transfer item with ID {id} not found");
        }

        item.Status = statusDto.Status;
        item.RemarksLine = statusDto.RemarksLine?.Trim();

        await _stockTransferItemRepository.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(item);
    }

    /// <summary>
    /// Updates item quantities (received and damaged)
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="quantityDto">Quantity update data</param>
    /// <returns>Updated stock transfer item DTO</returns>
    public async Task<StockTransferItemDto> UpdateQuantitiesAsync(int id, StockTransferItemQuantityDto quantityDto)
    {
        var success = await _stockTransferItemRepository.UpdateQuantitiesAsync(id, quantityDto.QuantityReceived, quantityDto.DamagedQty);
        if (!success)
        {
            throw new ArgumentException($"Stock transfer item with ID {id} not found");
        }

        var updatedItem = await _stockTransferItemRepository.GetByIdAsync(id);
        if (updatedItem != null && !string.IsNullOrEmpty(quantityDto.RemarksLine))
        {
            updatedItem.RemarksLine = quantityDto.RemarksLine.Trim();
            await _stockTransferItemRepository.UpdateAsync(updatedItem);
            await _unitOfWork.SaveChangesAsync();
        }

        return updatedItem != null ? MapToDto(updatedItem) : throw new InvalidOperationException("Failed to retrieve updated item");
    }

    /// <summary>
    /// Deletes a stock transfer item
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _stockTransferItemRepository.GetByIdAsync(id);
        if (item == null)
        {
            return false;
        }

        // Check if the transfer is still in pending status
        if (item.Transfer?.Status != "Pending")
        {
            throw new InvalidOperationException("Cannot delete items for transfers that are not in pending status");
        }

        await _stockTransferItemRepository.DeleteAsync(item.Id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Gets pending items for a specific transfer
    /// </summary>
    /// <param name="transferId">Transfer ID</param>
    /// <returns>Collection of pending item DTOs</returns>
    public async Task<IEnumerable<StockTransferItemDto>> GetPendingItemsAsync(int transferId)
    {
        var items = await _stockTransferItemRepository.GetPendingItemsAsync(transferId);
        return items.Select(MapToDto);
    }

    /// <summary>
    /// Gets items with full details including product and transfer info
    /// </summary>
    /// <param name="transferId">Transfer ID</param>
    /// <returns>Collection of detailed item DTOs</returns>
    public async Task<IEnumerable<StockTransferItemDetailDto>> GetItemsWithDetailsAsync(int transferId)
    {
        var items = await _stockTransferItemRepository.GetItemsWithDetailsAsync(transferId);
        return items.Select(MapToDetailDto);
    }

    /// <summary>
    /// Checks if any items exist for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if items exist</returns>
    public async Task<bool> HasItemsForProductAsync(int productId)
    {
        return await _stockTransferItemRepository.HasItemsForProductAsync(productId);
    }

    /// <summary>
    /// Bulk update item statuses
    /// </summary>
    /// <param name="updates">Collection of status updates</param>
    /// <returns>Number of items updated</returns>
    public async Task<int> BulkUpdateStatusAsync(IEnumerable<StockTransferItemBulkStatusDto> updates)
    {
        var updatedCount = 0;
        
        foreach (var update in updates)
        {
            var success = await _stockTransferItemRepository.UpdateStatusAsync(update.Id, update.Status);
            if (success)
            {
                var item = await _stockTransferItemRepository.GetByIdAsync(update.Id);
                if (item != null && !string.IsNullOrEmpty(update.RemarksLine))
                {
                    item.RemarksLine = update.RemarksLine.Trim();
                    await _stockTransferItemRepository.UpdateAsync(item);
                }
                updatedCount++;
            }
        }

        if (updatedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return updatedCount;
    }

    /// <summary>
    /// Receive multiple items in a transfer
    /// </summary>
    /// <param name="transferId">Transfer ID</param>
    /// <param name="receivedItems">Collection of received item data</param>
    /// <returns>True if all items were updated successfully</returns>
    public async Task<bool> ReceiveItemsAsync(int transferId, IEnumerable<StockTransferItemReceiveDto> receivedItems)
    {
        var allSuccess = true;

        foreach (var receivedItem in receivedItems)
        {
            var item = await _stockTransferItemRepository.GetByIdAsync(receivedItem.Id);
            if (item == null || item.TransferId != transferId)
            {
                allSuccess = false;
                continue;
            }

            item.QuantityReceived = receivedItem.QuantityReceived;
            item.DamagedQty = receivedItem.DamagedQty;
            item.Status = receivedItem.Status;
            item.RemarksLine = receivedItem.RemarksLine?.Trim();

            // Auto-update status based on quantities if not explicitly set
            if (string.IsNullOrEmpty(receivedItem.Status) || receivedItem.Status == "Received")
            {
                if (receivedItem.DamagedQty >= item.QuantitySent)
                    item.Status = "Damaged";
                else if (receivedItem.QuantityReceived + receivedItem.DamagedQty >= item.QuantitySent)
                    item.Status = "Received";
                else if (receivedItem.QuantityReceived > 0 || receivedItem.DamagedQty > 0)
                    item.Status = "Partially Received";
            }

            await _stockTransferItemRepository.UpdateAsync(item);
        }

        if (allSuccess)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return allSuccess;
    }

    /// <summary>
    /// Maps StockTransferItem entity to StockTransferItemDto
    /// </summary>
    /// <param name="item">StockTransferItem entity</param>
    /// <returns>StockTransferItem DTO</returns>
    private static StockTransferItemDto MapToDto(StockTransferItem item)
    {
        return new StockTransferItemDto
        {
            Id = item.Id,
            TransferId = item.TransferId,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? string.Empty,
            UomId = item.UomId,
            UomName = item.Uom?.Name ?? string.Empty,
            BatchNo = item.BatchNo,
            ExpiryDate = item.ExpiryDate,
            QuantitySent = item.QuantitySent,
            QuantityReceived = item.QuantityReceived,
            DamagedQty = item.DamagedQty,
            Status = item.Status,
            RemarksLine = item.RemarksLine
        };
    }

    /// <summary>
    /// Maps StockTransferItem entity to StockTransferItemDetailDto
    /// </summary>
    /// <param name="item">StockTransferItem entity</param>
    /// <returns>StockTransferItemDetail DTO</returns>
    private static StockTransferItemDetailDto MapToDetailDto(StockTransferItem item)
    {
        return new StockTransferItemDetailDto
        {
            Id = item.Id,
            TransferId = item.TransferId,
            TransferNo = item.Transfer?.TransferNo ?? string.Empty,
            TransferDate = item.Transfer?.TransferDate ?? DateTime.MinValue,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? string.Empty,
            ProductCode = string.Empty, // TODO: Add ProductCode to Product entity
            CategoryName = item.Product?.Category?.Name ?? string.Empty,
            BrandName = item.Product?.Brand?.Name ?? string.Empty,
            UomId = item.UomId,
            UomName = item.Uom?.Name ?? string.Empty,
            BatchNo = item.BatchNo,
            ExpiryDate = item.ExpiryDate,
            QuantitySent = item.QuantitySent,
            QuantityReceived = item.QuantityReceived,
            DamagedQty = item.DamagedQty,
            Status = item.Status,
            RemarksLine = item.RemarksLine,
            FromStoreName = item.Transfer?.FromStore?.Name ?? string.Empty,
            ToStoreName = item.Transfer?.ToStore?.Name ?? string.Empty,
            TransferStatus = item.Transfer?.Status ?? string.Empty
        };
    }
}