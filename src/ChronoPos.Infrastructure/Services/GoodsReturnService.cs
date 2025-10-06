using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Logging;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Service implementation for goods return operations
/// </summary>
public class GoodsReturnService : IGoodsReturnService
{
    private readonly ChronoPosDbContext _context;
    private readonly IGoodsReturnRepository _goodsReturnRepository;
    private readonly IGoodsReturnItemRepository _goodsReturnItemRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly IProductRepository _productRepository;

    public GoodsReturnService(
        ChronoPosDbContext context,
        IGoodsReturnRepository goodsReturnRepository,
        IGoodsReturnItemRepository goodsReturnItemRepository,
        ISupplierRepository supplierRepository,
        IStoreRepository storeRepository,
        IProductRepository productRepository)
    {
        _context = context;
        _goodsReturnRepository = goodsReturnRepository;
        _goodsReturnItemRepository = goodsReturnItemRepository;
        _supplierRepository = supplierRepository;
        _storeRepository = storeRepository;
        _productRepository = productRepository;
    }

    public async Task<GoodsReturnDto?> CreateGoodsReturnAsync(CreateGoodsReturnDto dto)
    {
        try
        {
            AppLogger.LogInfo("CreateGoodsReturnService", $"Creating goods return for Supplier {dto.SupplierId}, Store {dto.StoreId}");

            // Generate return number
            var returnNo = await _goodsReturnRepository.GetNextReturnNumberAsync();
            
            // Create goods return entity
            var goodsReturn = new GoodsReturn
            {
                ReturnNo = returnNo,
                SupplierId = dto.SupplierId,
                StoreId = dto.StoreId,
                ReferenceGrnId = dto.ReferenceGrnId,
                ReturnDate = dto.ReturnDate,
                Status = dto.Status,
                Remarks = dto.Remarks,
                CreatedBy = 1, // TODO: Get from current user context
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add to context and save to get ID
            await _goodsReturnRepository.AddAsync(goodsReturn);
            await _context.SaveChangesAsync();
            
            AppLogger.LogInfo("CreateGoodsReturnService", $"Successfully saved return header with ID: {goodsReturn.Id}");

            // Add return items and calculate total amount
            decimal totalAmount = 0;
            AppLogger.LogInfo("CreateGoodsReturnService", $"Adding {dto.Items.Count} return items");
            
            foreach (var itemDto in dto.Items)
            {
                // Calculate line total
                var lineTotal = itemDto.Quantity * itemDto.CostPrice;
                totalAmount += lineTotal;
                
                var item = new GoodsReturnItem
                {
                    ReturnId = goodsReturn.Id,
                    ProductId = itemDto.ProductId,
                    BatchId = itemDto.BatchId,
                    BatchNo = itemDto.BatchNo,
                    ExpiryDate = itemDto.ExpiryDate,
                    Quantity = itemDto.Quantity,
                    UomId = itemDto.UomId,
                    CostPrice = itemDto.CostPrice,
                    LineTotal = lineTotal,
                    Reason = itemDto.Reason,
                    CreatedAt = DateTime.UtcNow
                };

                await _goodsReturnItemRepository.AddAsync(item);
            }

            // Update total amount
            goodsReturn.TotalAmount = totalAmount;
            await _goodsReturnRepository.UpdateAsync(goodsReturn);
            await _context.SaveChangesAsync();

            AppLogger.LogInfo("CreateGoodsReturnService", $"Successfully created goods return {returnNo} with total amount {totalAmount:C}");

            // Return the created goods return as DTO
            return await GetGoodsReturnByIdAsync(goodsReturn.Id);
        }
        catch (Exception ex)
        {
            var fullErrorMessage = ex.Message;
            var innerEx = ex.InnerException;
            while (innerEx != null)
            {
                fullErrorMessage += $" | Inner: {innerEx.Message}";
                innerEx = innerEx.InnerException;
            }
            AppLogger.LogError($"Error creating goods return: {fullErrorMessage}", ex, "CreateGoodsReturnService");
            throw;
        }
    }

    public async Task<GoodsReturnDto?> UpdateGoodsReturnAsync(int returnId, CreateGoodsReturnDto dto)
    {
        try
        {
            AppLogger.LogInfo("UpdateGoodsReturnService", $"Updating goods return {returnId}");

            var existingReturn = await _goodsReturnRepository.GetWithItemsByIdAsync(returnId);
            if (existingReturn == null)
            {
                AppLogger.LogWarning("UpdateGoodsReturnService", $"Goods return {returnId} not found");
                return null;
            }

            // Check if return can be edited
            if (!await CanEditGoodsReturnAsync(returnId))
            {
                AppLogger.LogWarning("UpdateGoodsReturnService", $"Goods return {returnId} cannot be edited due to status: {existingReturn.Status}");
                throw new InvalidOperationException($"Cannot edit goods return in {existingReturn.Status} status");
            }

            // Update return properties
            existingReturn.SupplierId = dto.SupplierId;
            existingReturn.StoreId = dto.StoreId;
            existingReturn.ReferenceGrnId = dto.ReferenceGrnId;
            existingReturn.ReturnDate = dto.ReturnDate;
            existingReturn.Status = dto.Status;
            existingReturn.Remarks = dto.Remarks;
            existingReturn.UpdatedAt = DateTime.UtcNow;

            // Remove existing items
            await _goodsReturnItemRepository.DeleteByReturnIdAsync(returnId);

            // Add new items and calculate total amount
            decimal totalAmount = 0;
            foreach (var itemDto in dto.Items)
            {
                var lineTotal = itemDto.Quantity * itemDto.CostPrice;
                totalAmount += lineTotal;
                
                var item = new GoodsReturnItem
                {
                    ReturnId = returnId,
                    ProductId = itemDto.ProductId,
                    BatchId = itemDto.BatchId,
                    BatchNo = itemDto.BatchNo,
                    ExpiryDate = itemDto.ExpiryDate,
                    Quantity = itemDto.Quantity,
                    UomId = itemDto.UomId,
                    CostPrice = itemDto.CostPrice,
                    LineTotal = lineTotal,
                    Reason = itemDto.Reason,
                    CreatedAt = DateTime.UtcNow
                };

                await _goodsReturnItemRepository.AddAsync(item);
            }

            // Update total amount
            existingReturn.TotalAmount = totalAmount;
            await _goodsReturnRepository.UpdateAsync(existingReturn);
            await _context.SaveChangesAsync();

            AppLogger.LogInfo("UpdateGoodsReturnService", $"Successfully updated goods return {existingReturn.ReturnNo}");

            return await GetGoodsReturnByIdAsync(returnId);
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Error updating goods return {returnId}: {ex.Message}", ex, "UpdateGoodsReturnService");
            throw;
        }
    }

    public async Task<GoodsReturnDto?> GetGoodsReturnByIdAsync(int returnId)
    {
        try
        {
            var goodsReturn = await _goodsReturnRepository.GetWithItemsByIdAsync(returnId);
            if (goodsReturn == null) return null;

            return MapToDto(goodsReturn);
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Error getting goods return {returnId}: {ex.Message}", ex, "GetGoodsReturnByIdService");
            throw;
        }
    }

    public async Task<IEnumerable<GoodsReturnDto>> GetGoodsReturnsAsync(
        string? searchTerm = null,
        int? supplierId = null,
        int? storeId = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            AppLogger.LogInfo("GetGoodsReturnsService", $"Getting goods returns with filters - Supplier: {supplierId}, Store: {storeId}, Status: {status}");

            var goodsReturns = await _goodsReturnRepository.GetWithItemsByCriteriaAsync(
                supplierId, storeId, status, startDate, endDate);

            var result = goodsReturns.Select(MapToDto).ToList();

            // Apply search term filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                result = result.Where(gr => 
                    gr.ReturnNo.ToLower().Contains(searchTerm) ||
                    gr.SupplierName.ToLower().Contains(searchTerm) ||
                    gr.StoreName.ToLower().Contains(searchTerm) ||
                    (!string.IsNullOrEmpty(gr.ReferenceGrnNo) && gr.ReferenceGrnNo.ToLower().Contains(searchTerm))
                ).ToList();
            }

            AppLogger.LogInfo("GetGoodsReturnsService", $"Found {result.Count} goods returns");
            return result;
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Error getting goods returns: {ex.Message}", ex, "GetGoodsReturnsService");
            throw;
        }
    }

    public async Task<bool> DeleteGoodsReturnAsync(int returnId)
    {
        try
        {
            AppLogger.LogInfo("DeleteGoodsReturnService", $"Deleting goods return {returnId}");

            var goodsReturn = await _goodsReturnRepository.GetByIdAsync(returnId);
            if (goodsReturn == null)
            {
                AppLogger.LogWarning("DeleteGoodsReturnService", $"Goods return {returnId} not found");
                return false;
            }

            // Check if return can be deleted
            if (!await CanDeleteGoodsReturnAsync(returnId))
            {
                AppLogger.LogWarning("DeleteGoodsReturnService", $"Goods return {returnId} cannot be deleted due to status: {goodsReturn.Status}");
                throw new InvalidOperationException($"Cannot delete goods return in {goodsReturn.Status} status");
            }

            // Delete items first
            await _goodsReturnItemRepository.DeleteByReturnIdAsync(returnId);
            
            // Delete return
            await _goodsReturnRepository.DeleteAsync(goodsReturn.Id);
            await _context.SaveChangesAsync();

            AppLogger.LogInfo("DeleteGoodsReturnService", $"Successfully deleted goods return {goodsReturn.ReturnNo}");
            return true;
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Error deleting goods return {returnId}: {ex.Message}", ex, "DeleteGoodsReturnService");
            throw;
        }
    }

    public async Task<bool> PostGoodsReturnAsync(int returnId)
    {
        try
        {
            AppLogger.LogInfo("PostGoodsReturnService", $"Posting goods return {returnId}");

            var goodsReturn = await _goodsReturnRepository.GetWithItemsByIdAsync(returnId);
            if (goodsReturn == null)
            {
                AppLogger.LogWarning("PostGoodsReturnService", $"Goods return {returnId} not found");
                return false;
            }

            if (goodsReturn.Status != "Pending")
            {
                AppLogger.LogWarning("PostGoodsReturnService", $"Goods return {returnId} cannot be posted. Current status: {goodsReturn.Status}");
                throw new InvalidOperationException($"Cannot post goods return in {goodsReturn.Status} status");
            }

            // Update status to Posted
            goodsReturn.Status = "Posted";
            goodsReturn.UpdatedAt = DateTime.UtcNow;

            // Process stock adjustments for returned items
            AppLogger.LogInfo("PostGoodsReturnService", $"Processing stock adjustments for {goodsReturn.Items.Count} items");
            
            foreach (var item in goodsReturn.Items)
            {
                await ReduceStockForReturnAsync(item.ProductId, item.BatchNo, item.Quantity);
            }

            await _goodsReturnRepository.UpdateAsync(goodsReturn);
            await _context.SaveChangesAsync();

            AppLogger.LogInfo("PostGoodsReturnService", $"Successfully posted goods return {goodsReturn.ReturnNo}");
            return true;
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Error posting goods return {returnId}: {ex.Message}", ex, "PostGoodsReturnService");
            throw;
        }
    }

    public async Task<bool> CancelGoodsReturnAsync(int returnId)
    {
        try
        {
            AppLogger.LogInfo("CancelGoodsReturnService", $"Cancelling goods return {returnId}");

            var goodsReturn = await _goodsReturnRepository.GetByIdAsync(returnId);
            if (goodsReturn == null)
            {
                AppLogger.LogWarning("CancelGoodsReturnService", $"Goods return {returnId} not found");
                return false;
            }

            if (goodsReturn.Status == "Posted")
            {
                AppLogger.LogWarning("CancelGoodsReturnService", $"Goods return {returnId} cannot be cancelled. Current status: {goodsReturn.Status}");
                throw new InvalidOperationException("Cannot cancel a posted goods return");
            }

            goodsReturn.Status = "Cancelled";
            goodsReturn.UpdatedAt = DateTime.UtcNow;

            await _goodsReturnRepository.UpdateAsync(goodsReturn);
            await _context.SaveChangesAsync();

            AppLogger.LogInfo("CancelGoodsReturnService", $"Successfully cancelled goods return {goodsReturn.ReturnNo}");
            return true;
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Error cancelling goods return {returnId}: {ex.Message}", ex, "CancelGoodsReturnService");
            throw;
        }
    }

    public async Task<IEnumerable<GoodsReturnItemDto>> GetGoodsReturnItemsAsync(int returnId)
    {
        try
        {
            var items = await _goodsReturnItemRepository.GetWithProductDetailsByReturnIdAsync(returnId);
            return items.Select(MapItemToDto);
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Error getting goods return items for return {returnId}: {ex.Message}", ex, "GetGoodsReturnItemsService");
            throw;
        }
    }

    public async Task<bool> CanEditGoodsReturnAsync(int returnId)
    {
        var goodsReturn = await _goodsReturnRepository.GetByIdAsync(returnId);
        return goodsReturn?.Status is "Draft" or "Pending";
    }

    public async Task<bool> CanDeleteGoodsReturnAsync(int returnId)
    {
        var goodsReturn = await _goodsReturnRepository.GetByIdAsync(returnId);
        return goodsReturn?.Status is "Draft" or "Pending";
    }

    public async Task<string> GetNextReturnNumberAsync()
    {
        return await _goodsReturnRepository.GetNextReturnNumberAsync();
    }

    private static GoodsReturnDto MapToDto(GoodsReturn goodsReturn)
    {
        return new GoodsReturnDto
        {
            Id = goodsReturn.Id,
            ReturnNo = goodsReturn.ReturnNo,
            ReturnDate = goodsReturn.ReturnDate,
            SupplierId = goodsReturn.SupplierId,
            SupplierName = goodsReturn.Supplier?.CompanyName ?? "Unknown",
            StoreId = goodsReturn.StoreId,
            StoreName = goodsReturn.Store?.Name ?? "Unknown",
            ReferenceGrnId = goodsReturn.ReferenceGrnId,
            ReferenceGrnNo = goodsReturn.ReferenceGrn?.GrnNo,
            TotalAmount = goodsReturn.TotalAmount,
            Status = goodsReturn.Status,
            Remarks = goodsReturn.Remarks,
            IsTotallyReplaced = goodsReturn.IsTotallyReplaced,
            CreatedByName = goodsReturn.Creator?.FullName ?? "System",
            CreatedAt = goodsReturn.CreatedAt,
            TotalItems = goodsReturn.Items?.Count ?? 0,
            Items = goodsReturn.Items?.Select(MapItemToDto).ToList() ?? new List<GoodsReturnItemDto>()
        };
    }

    private static GoodsReturnItemDto MapItemToDto(GoodsReturnItem item)
    {
        return new GoodsReturnItemDto
        {
            Id = item.Id,
            ReturnId = item.ReturnId,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? "Unknown",
            ProductCode = item.Product?.Code ?? "Unknown",
            BatchId = item.BatchId,
            BatchNo = item.BatchNo,
            ExpiryDate = item.ExpiryDate,
            Quantity = item.Quantity,
            UomId = item.UomId,
            UomName = item.Uom?.Name ?? "Unknown",
            CostPrice = item.CostPrice,
            LineTotal = item.LineTotal,
            Reason = item.Reason,
            AlreadyReplacedQuantity = item.AlreadyReplacedQuantity,
            IsTotallyReplaced = item.IsTotallyReplaced,
            CreatedAt = item.CreatedAt
        };
    }

    /// <summary>
    /// Reduce stock quantities when goods are returned (similar to StockTransferService)
    /// Updates both product stock and batch quantities
    /// </summary>
    private async Task ReduceStockForReturnAsync(int productId, string? batchNo, decimal returnQuantity)
    {
        AppLogger.LogInfo("ReduceStockForReturn", 
            $"Starting stock reduction for returned goods - Product ID: {productId}, Batch: {batchNo}, Return Qty: {returnQuantity}", 
            "goods_return");

        // First, reduce the product stock quantity
        await ReduceProductStockAsync(productId, returnQuantity);

        // If batch is specified, also reduce batch quantity
        if (!string.IsNullOrEmpty(batchNo))
        {
            await ReduceProductBatchStockAsync(productId, batchNo, returnQuantity);
        }

        AppLogger.LogInfo("ReduceStockForReturn", 
            $"Completed stock reduction for Product ID: {productId}, Batch: {batchNo}, Return Qty: {returnQuantity}", 
            "goods_return");
    }

    /// <summary>
    /// Reduce product stock quantities (both InitialStock and StockQuantity)
    /// </summary>
    private async Task ReduceProductStockAsync(int productId, decimal quantityToReduce)
    {
        AppLogger.LogInfo("ReduceProductStock", 
            $"Starting stock reduction for Product ID: {productId}, Quantity to reduce: {quantityToReduce}", 
            "goods_return");

        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            var error = $"Product with ID {productId} not found";
            AppLogger.LogError("ReduceProductStock", new InvalidOperationException(error), error, "goods_return");
            throw new InvalidOperationException(error);
        }

        var originalStockQuantity = product.StockQuantity;
        var originalInitialStock = product.InitialStock;

        AppLogger.LogInfo("ReduceProductStock", 
            $"Product ID: {productId}, Name: '{product.Name}', Current Stock Qty: {originalStockQuantity}, Current Initial Stock: {originalInitialStock}", 
            "goods_return");

        // Check if we have enough stock to return (this should normally be positive since we're returning goods)
        if (product.StockQuantity < quantityToReduce)
        {
            var error = $"Insufficient stock for product '{product.Name}' to process return. Available: {product.StockQuantity}, Return Qty: {quantityToReduce}";
            AppLogger.LogError("ReduceProductStock", new InvalidOperationException(error), error, "goods_return");
            throw new InvalidOperationException(error);
        }

        // Reduce both StockQuantity and InitialStock (returned goods reduce available stock)
        product.StockQuantity -= (int)quantityToReduce;
        product.InitialStock -= quantityToReduce;

        AppLogger.LogInfo("ReduceProductStock", 
            $"Product ID: {productId}, New Stock Qty: {product.StockQuantity} (was {originalStockQuantity}), New Initial Stock: {product.InitialStock} (was {originalInitialStock}), Reduced: {quantityToReduce}", 
            "goods_return");

        _context.Products.Update(product);

        AppLogger.LogInfo("ReduceProductStock", 
            $"Successfully reduced stock for Product ID: {productId}, Name: '{product.Name}', Final Stock Qty: {product.StockQuantity}, Final Initial Stock: {product.InitialStock}", 
            "goods_return");
    }

    /// <summary>
    /// Reduce product batch stock quantity
    /// </summary>
    private async Task ReduceProductBatchStockAsync(int productId, string batchNo, decimal quantityToReduce)
    {
        AppLogger.LogInfo("ReduceProductBatchStock", 
            $"Starting batch stock reduction for Product ID: {productId}, Batch: {batchNo}, Quantity to reduce: {quantityToReduce}", 
            "goods_return");

        var batch = await _context.ProductBatches
            .FirstOrDefaultAsync(pb => pb.ProductId == productId && pb.BatchNo == batchNo);

        if (batch == null)
        {
            var error = $"Product batch not found. Product ID: {productId}, Batch: {batchNo}";
            AppLogger.LogError("ReduceProductBatchStock", new InvalidOperationException(error), error, "goods_return");
            throw new InvalidOperationException(error);
        }

        var originalBatchQuantity = batch.Quantity;

        AppLogger.LogInfo("ReduceProductBatchStock", 
            $"Product ID: {productId}, Batch: {batchNo}, Current Batch Qty: {originalBatchQuantity}", 
            "goods_return");

        // Check if we have enough stock in this batch
        if (batch.Quantity < quantityToReduce)
        {
            var error = $"Insufficient stock in batch '{batchNo}' for product ID {productId} to process return. Available: {batch.Quantity}, Return Qty: {quantityToReduce}";
            AppLogger.LogError("ReduceProductBatchStock", new InvalidOperationException(error), error, "goods_return");
            throw new InvalidOperationException(error);
        }

        // Reduce batch quantity
        batch.Quantity -= quantityToReduce;

        AppLogger.LogInfo("ReduceProductBatchStock", 
            $"Product ID: {productId}, Batch: {batchNo}, New Batch Qty: {batch.Quantity} (was {originalBatchQuantity}), Reduced: {quantityToReduce}", 
            "goods_return");

        _context.ProductBatches.Update(batch);

        AppLogger.LogInfo("ReduceProductBatchStock", 
            $"Successfully reduced batch stock for Product ID: {productId}, Batch: {batchNo}, Final Qty: {batch.Quantity}", 
            "goods_return");
    }
}
