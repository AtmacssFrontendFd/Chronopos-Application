using ChronoPos.Application.DTOs;
using ChronoPos.Application.DTOs.Inventory;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Logging;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Enums;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for stock transfer operations
    /// </summary>
    public class StockTransferService : IStockTransferService
    {
        private readonly ChronoPosDbContext _context;
        private readonly IStockTransferRepository _stockTransferRepository;
        private readonly IShopLocationRepository _shopLocationRepository;
        private readonly IStockLedgerService? _stockLedgerService;

        public StockTransferService(
            ChronoPosDbContext context,
            IStockTransferRepository stockTransferRepository,
            IShopLocationRepository shopLocationRepository,
            IStockLedgerService? stockLedgerService = null)
        {
            _context = context;
            _stockTransferRepository = stockTransferRepository;
            _shopLocationRepository = shopLocationRepository;
            _stockLedgerService = stockLedgerService;
        }

        /// <summary>
        /// Get paginated list of stock transfers with filtering
        /// </summary>
        public async Task<PagedResult<StockTransferDto>> GetStockTransfersAsync(
            int page = 1, 
            int pageSize = 10, 
            string? searchTerm = null,
            int? fromStoreId = null,
            int? toStoreId = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.StockTransfers
                .Include(st => st.FromStore)
                .Include(st => st.ToStore)
                .Include(st => st.Creator)
                .Include(st => st.Items)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(st => st.TransferNo.Contains(searchTerm) || 
                                        (st.Remarks != null && st.Remarks.Contains(searchTerm)));
            }

            if (fromStoreId.HasValue)
            {
                query = query.Where(st => st.FromStoreId == fromStoreId.Value);
            }

            if (toStoreId.HasValue)
            {
                query = query.Where(st => st.ToStoreId == toStoreId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(st => st.Status == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(st => st.TransferDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(st => st.TransferDate <= toDate.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and order by date descending
            var transfers = await query
                .OrderByDescending(st => st.TransferDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(st => new StockTransferDto
                {
                    TransferId = st.TransferId,
                    TransferNo = st.TransferNo,
                    TransferDate = st.TransferDate,
                    FromStoreId = st.FromStoreId,
                    FromStoreName = st.FromStore != null ? st.FromStore.Name : "",
                    ToStoreId = st.ToStoreId,
                    ToStoreName = st.ToStore != null ? st.ToStore.Name : "",
                    Status = st.Status,
                    Remarks = st.Remarks,
                    CreatedByName = st.Creator != null ? st.Creator.FullName : "",
                    CreatedAt = st.CreatedAt,
                    TotalItems = st.Items.Count(),
                    Items = new List<StockTransferItemDto>()
                })
                .ToListAsync();

            return new PagedResult<StockTransferDto>
            {
                Items = transfers,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Get single stock transfer by ID with all details
        /// </summary>
        public async Task<StockTransferDto?> GetStockTransferByIdAsync(int transferId)
        {
            var transfer = await _context.StockTransfers
                .Include(st => st.FromStore)
                .Include(st => st.ToStore)
                .Include(st => st.Creator)
                .Include(st => st.Items)
                    .ThenInclude(sti => sti.Product)
                .Include(st => st.Items)
                    .ThenInclude(sti => sti.Uom)
                .FirstOrDefaultAsync(st => st.TransferId == transferId);

            if (transfer == null)
                return null;

            return new StockTransferDto
            {
                TransferId = transfer.TransferId,
                TransferNo = transfer.TransferNo,
                TransferDate = transfer.TransferDate,
                FromStoreId = transfer.FromStoreId,
                FromStoreName = transfer.FromStore?.Name ?? "",
                ToStoreId = transfer.ToStoreId,
                ToStoreName = transfer.ToStore?.Name ?? "",
                Status = transfer.Status,
                Remarks = transfer.Remarks,
                CreatedByName = transfer.Creator?.FullName ?? "",
                CreatedAt = transfer.CreatedAt,
                TotalItems = transfer.Items.Count,
                Items = transfer.Items.Select(item => new StockTransferItemDto
                {
                    Id = item.Id,
                    TransferId = item.TransferId,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? "",
                    UomId = item.UomId,
                    UomName = item.Uom?.Name ?? "",
                    BatchNo = item.BatchNo,
                    ExpiryDate = item.ExpiryDate,
                    QuantitySent = item.QuantitySent,
                    QuantityReceived = item.QuantityReceived,
                    DamagedQty = item.DamagedQty,
                    Status = item.Status,
                    RemarksLine = item.RemarksLine
                }).ToList()
            };
        }

        /// <summary>
        /// Create new stock transfer
        /// </summary>
        public async Task<StockTransferDto> CreateStockTransferAsync(CreateStockTransferDto dto)
        {
            try
            {
                AppLogger.LogInfo("CreateStockTransferService", $"Starting transfer creation - FromStore: {dto.FromStoreId}, ToStore: {dto.ToStoreId}");
                
                var transferNo = await GenerateTransferNumberAsync();
                AppLogger.LogInfo("CreateStockTransferService", $"Generated transfer number: {transferNo}");

                var transfer = new StockTransfer
                {
                    TransferNo = transferNo,
                    TransferDate = dto.TransferDate,
                    FromStoreId = dto.FromStoreId,
                    ToStoreId = dto.ToStoreId,
                    Status = dto.Status,
                    Remarks = dto.Remarks,
                    CreatedBy = 1, // TODO: Get from current user context
                    CreatedAt = DateTime.UtcNow
                };

                AppLogger.LogInfo("CreateStockTransferService", $"Adding transfer to context - TransferNo: {transfer.TransferNo}");
                _context.StockTransfers.Add(transfer);
                
                AppLogger.LogInfo("CreateStockTransferService", "Calling SaveChangesAsync for transfer header");
                await _context.SaveChangesAsync();
                AppLogger.LogInfo("CreateStockTransferService", $"Successfully saved transfer header with ID: {transfer.TransferId}");

                // Add transfer items and conditionally reduce stock quantities
                AppLogger.LogInfo("CreateStockTransferService", $"Adding {dto.Items.Count} transfer items");
                foreach (var itemDto in dto.Items)
                {
                    // Only reduce stock for completed/confirmed transfers (not for Draft or Pending)
                    if (dto.Status == "Completed" || dto.Status == "Confirmed")
                    {
                        AppLogger.LogInfo("CreateStockTransferService", $"Reducing stock for status: {dto.Status}");
                        // 1. Reduce product stock quantities BEFORE saving transfer item
                        await ReduceProductStockAsync(itemDto.ProductId, itemDto.QuantitySent);
                        
                        // 2. Reduce product batch quantity BEFORE saving transfer item (if batch is specified)
                        if (!string.IsNullOrEmpty(itemDto.BatchNo))
                        {
                            await ReduceProductBatchStockAsync(itemDto.ProductId, itemDto.BatchNo, itemDto.QuantitySent);
                        }
                    }
                    else
                    {
                        AppLogger.LogInfo("CreateStockTransferService", $"Skipping stock reduction for status: {dto.Status}");
                    }

                    var item = new StockTransferItem
                    {
                        TransferId = transfer.TransferId,
                        ProductId = itemDto.ProductId,
                        UomId = itemDto.UomId,
                        BatchNo = itemDto.BatchNo,
                        ExpiryDate = itemDto.ExpiryDate,
                        QuantitySent = itemDto.QuantitySent,
                        QuantityReceived = 0,
                        DamagedQty = 0,
                        Status = "Pending",
                        RemarksLine = itemDto.RemarksLine
                    };

                    _context.StockTransferItems.Add(item);
                    AppLogger.LogInfo("CreateStockTransferService", $"Added item - ProductId: {item.ProductId}, Quantity: {item.QuantitySent}, Batch: {item.BatchNo}");
                }

                AppLogger.LogInfo("CreateStockTransferService", "Calling SaveChangesAsync for transfer items");
                await _context.SaveChangesAsync();
                AppLogger.LogInfo("CreateStockTransferService", "Successfully saved all transfer items");

                // Return the created transfer
                AppLogger.LogInfo("CreateStockTransferService", $"Retrieving created transfer with ID: {transfer.TransferId}");
                return await GetStockTransferByIdAsync(transfer.TransferId) 
                    ?? throw new InvalidOperationException("Failed to retrieve created transfer");
            }
            catch (Exception ex)
            {
                AppLogger.LogError("CreateStockTransferService", ex, $"Error creating stock transfer | Exception: {ex.Message} | InnerException: {ex.InnerException?.Message ?? "None"}");
                throw;
            }
        }

        /// <summary>
        /// Update existing stock transfer
        /// </summary>
        public async Task<StockTransferDto> UpdateStockTransferAsync(int transferId, CreateStockTransferDto dto)
        {
            var transfer = await _context.StockTransfers
                .Include(st => st.Items)
                .FirstOrDefaultAsync(st => st.TransferId == transferId);

            if (transfer == null)
                throw new ArgumentException("Transfer not found", nameof(transferId));

            if (transfer.Status != "Pending")
                throw new InvalidOperationException("Cannot update transfer that is not in pending status");

            // Update transfer properties
            transfer.TransferDate = dto.TransferDate;
            transfer.FromStoreId = dto.FromStoreId;
            transfer.ToStoreId = dto.ToStoreId;
            transfer.Remarks = dto.Remarks;
            transfer.UpdatedBy = 1; // TODO: Get from current user context
            transfer.UpdatedAt = DateTime.UtcNow;

            // Remove existing items
            _context.StockTransferItems.RemoveRange(transfer.Items);

            // Add new items
            foreach (var itemDto in dto.Items)
            {
                var item = new StockTransferItem
                {
                    TransferId = transfer.TransferId,
                    ProductId = itemDto.ProductId,
                    UomId = itemDto.UomId,
                    BatchNo = itemDto.BatchNo,
                    ExpiryDate = itemDto.ExpiryDate,
                    QuantitySent = itemDto.QuantitySent,
                    QuantityReceived = 0,
                    DamagedQty = 0,
                    Status = "Pending",
                    RemarksLine = itemDto.RemarksLine
                };

                _context.StockTransferItems.Add(item);
            }

            await _context.SaveChangesAsync();

            return await GetStockTransferByIdAsync(transferId) 
                ?? throw new InvalidOperationException("Failed to retrieve updated transfer");
        }

        /// <summary>
        /// Delete stock transfer
        /// </summary>
        public async Task<bool> DeleteStockTransferAsync(int transferId)
        {
            var transfer = await _context.StockTransfers
                .Include(st => st.Items)
                .FirstOrDefaultAsync(st => st.TransferId == transferId);

            if (transfer == null)
                return false;

            if (transfer.Status != "Pending")
                return false; // Cannot delete non-pending transfers

            // Remove items first (cascading delete should handle this)
            _context.StockTransferItems.RemoveRange(transfer.Items);
            _context.StockTransfers.Remove(transfer);

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Complete stock transfer
        /// </summary>
        public async Task<bool> CompleteStockTransferAsync(int transferId)
        {
            var transfer = await _context.StockTransfers
                .FirstOrDefaultAsync(st => st.TransferId == transferId);

            if (transfer == null || transfer.Status != "Pending")
                return false;

            transfer.Status = "Completed";
            transfer.UpdatedBy = 1; // TODO: Get from current user context
            transfer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Cancel stock transfer
        /// </summary>
        public async Task<bool> CancelStockTransferAsync(int transferId)
        {
            var transfer = await _context.StockTransfers
                .FirstOrDefaultAsync(st => st.TransferId == transferId);

            if (transfer == null || transfer.Status == "Completed")
                return false;

            transfer.Status = "Cancelled";
            transfer.UpdatedBy = 1; // TODO: Get from current user context
            transfer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Receive items for stock transfer
        /// </summary>
        public async Task<bool> ReceiveTransferItemsAsync(int transferId, List<StockTransferItemDto> receivedItems)
        {
            var transfer = await _context.StockTransfers
                .Include(st => st.Items)
                .FirstOrDefaultAsync(st => st.TransferId == transferId);

            if (transfer == null || transfer.Status != "Pending")
                return false;

            foreach (var receivedItem in receivedItems)
            {
                var item = transfer.Items.FirstOrDefault(i => i.Id == receivedItem.Id);
                if (item != null)
                {
                    item.QuantityReceived = receivedItem.QuantityReceived;
                    item.DamagedQty = receivedItem.DamagedQty;
                    item.Status = receivedItem.Status;
                    item.RemarksLine = receivedItem.RemarksLine;
                }
            }

            // Check if all items are received
            var allReceived = transfer.Items.All(i => i.Status == "Received" || i.Status == "Damaged");
            if (allReceived)
            {
                transfer.Status = "Completed";
            }

            transfer.UpdatedBy = 1; // TODO: Get from current user context
            transfer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            // Create stock ledger entries AFTER saving (to avoid nested transaction conflict)
            if (_stockLedgerService != null)
            {
                foreach (var receivedItem in receivedItems)
                {
                    var item = transfer.Items.FirstOrDefault(i => i.Id == receivedItem.Id);
                    if (item != null && receivedItem.QuantityReceived > 0)
                    {
                        try
                        {
                            // Transfer OUT from source location
                            var transferOutDto = new CreateStockLedgerDto
                            {
                                ProductId = item.ProductId,
                                UnitId = (int)item.UomId,
                                MovementType = StockMovementType.TransferOut,
                                Qty = receivedItem.QuantityReceived,
                                Location = transfer.FromStore?.Name ?? "Source Store",
                                ReferenceType = StockReferenceType.Transfer,
                                ReferenceId = transferId,
                                Note = $"Transfer Out - {transfer.TransferNo}"
                            };
                            await _stockLedgerService.CreateAsync(transferOutDto);
                            
                            // Transfer IN to destination location
                            var transferInDto = new CreateStockLedgerDto
                            {
                                ProductId = item.ProductId,
                                UnitId = (int)item.UomId,
                                MovementType = StockMovementType.TransferIn,
                                Qty = receivedItem.QuantityReceived,
                                Location = transfer.ToStore?.Name ?? "Destination Store",
                                ReferenceType = StockReferenceType.Transfer,
                                ReferenceId = transferId,
                                Note = $"Transfer In - {transfer.TransferNo}"
                            };
                            await _stockLedgerService.CreateAsync(transferInDto);
                            
                            AppLogger.LogInfo("Stock ledger entries created for transfer", 
                                $"ProductId: {item.ProductId}, Qty: {receivedItem.QuantityReceived}, TransferNo: {transfer.TransferNo}", "stock_transfer");
                        }
                        catch (Exception ex)
                        {
                            AppLogger.LogError("Failed to create stock ledger entries for transfer", ex,
                                $"TransferId: {transferId}, ProductId: {item.ProductId}", "stock_transfer");
                            // Don't throw - ledger is supplementary
                        }
                    }
                }
            }
            
            return true;
        }

        /// <summary>
        /// Get all shop locations
        /// </summary>
        public async Task<List<ShopLocationDto>> GetShopLocationsAsync()
        {
            return await _context.ShopLocations
                .Where(sl => sl.Status == "Active" && sl.DeletedAt == null)
                .Select(sl => new ShopLocationDto
                {
                    Id = sl.Id,
                    LocationName = sl.LocationName,
                    LocationType = sl.LocationType,
                    City = sl.City,
                    Status = sl.Status
                })
                .OrderBy(sl => sl.LocationName)
                .ToListAsync();
        }

        /// <summary>
        /// Get products available for transfer from a specific location
        /// </summary>
        public async Task<PagedResult<ProductStockInfoDto>> GetProductsForTransferAsync(
            int fromStoreId,
            int page = 1,
            int pageSize = 20,
            string? searchTerm = null, 
            int? categoryId = null)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || 
                                       (p.Description != null && p.Description.Contains(searchTerm)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductStockInfoDto
                {
                    ProductId = p.Id,
                    ProductCode = "", // TODO: Add product code field
                    ProductName = p.Name,
                    Description = p.Description ?? "",
                    CategoryName = p.Category != null ? p.Category.Name : "",
                    SalePrice = p.Price,
                    CurrentStock = 0, // TODO: Calculate actual stock for the location
                    UomId = 1, // TODO: Get from product's default UOM
                    UomName = "Unit", // TODO: Get from UOM
                    IsActive = true
                })
                .ToListAsync();

            return new PagedResult<ProductStockInfoDto>
            {
                Items = products,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Generate transfer number
        /// </summary>
        public async Task<string> GenerateTransferNumberAsync()
        {
            return await _stockTransferRepository.GetNextTransferNumberAsync();
        }

        /// <summary>
        /// Reduce product stock quantities (both InitialStock and StockQuantity)
        /// </summary>
        private async Task ReduceProductStockAsync(int productId, decimal quantityToReduce)
        {
            AppLogger.LogInfo("ReduceProductStock", 
                $"Starting stock reduction for Product ID: {productId}, Quantity to reduce: {quantityToReduce}", 
                "stock_transfer");

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                var error = $"Product with ID {productId} not found";
                AppLogger.LogError("ReduceProductStock", new InvalidOperationException(error), error, "stock_transfer");
                throw new InvalidOperationException(error);
            }

            var originalStockQuantity = product.StockQuantity;
            var originalInitialStock = product.InitialStock;

            AppLogger.LogInfo("ReduceProductStock", 
                $"Product ID: {productId}, Name: '{product.Name}', Current Stock Qty: {originalStockQuantity}, Current Initial Stock: {originalInitialStock}", 
                "stock_transfer");

            // Check if we have enough stock
            if (product.StockQuantity < quantityToReduce)
            {
                var error = $"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}, Required: {quantityToReduce}";
                AppLogger.LogError("ReduceProductStock", new InvalidOperationException(error), error, "stock_transfer");
                throw new InvalidOperationException(error);
            }

            // Reduce both StockQuantity and InitialStock
            product.StockQuantity -= (int)quantityToReduce;
            product.InitialStock -= quantityToReduce;

            AppLogger.LogInfo("ReduceProductStock", 
                $"Product ID: {productId}, New Stock Qty: {product.StockQuantity} (was {originalStockQuantity}), New Initial Stock: {product.InitialStock} (was {originalInitialStock}), Reduced: {quantityToReduce}", 
                "stock_transfer");

            _context.Products.Update(product);

            AppLogger.LogInfo("ReduceProductStock", 
                $"Successfully reduced stock for Product ID: {productId}, Name: '{product.Name}', Final Stock Qty: {product.StockQuantity}, Final Initial Stock: {product.InitialStock}", 
                "stock_transfer");
        }

        /// <summary>
        /// Reduce product batch stock quantity
        /// </summary>
        private async Task ReduceProductBatchStockAsync(int productId, string batchNo, decimal quantityToReduce)
        {
            AppLogger.LogInfo("ReduceProductBatchStock", 
                $"Starting batch stock reduction for Product ID: {productId}, Batch: {batchNo}, Quantity to reduce: {quantityToReduce}", 
                "stock_transfer");

            var batch = await _context.ProductBatches
                .FirstOrDefaultAsync(pb => pb.ProductId == productId && pb.BatchNo == batchNo);

            if (batch == null)
            {
                var error = $"Product batch not found. Product ID: {productId}, Batch: {batchNo}";
                AppLogger.LogError("ReduceProductBatchStock", new InvalidOperationException(error), error, "stock_transfer");
                throw new InvalidOperationException(error);
            }

            var originalBatchQuantity = batch.Quantity;

            AppLogger.LogInfo("ReduceProductBatchStock", 
                $"Product ID: {productId}, Batch: {batchNo}, Current Batch Qty: {originalBatchQuantity}", 
                "stock_transfer");

            // Check if we have enough stock in this batch
            if (batch.Quantity < quantityToReduce)
            {
                var error = $"Insufficient stock in batch '{batchNo}' for product ID {productId}. Available: {batch.Quantity}, Required: {quantityToReduce}";
                AppLogger.LogError("ReduceProductBatchStock", new InvalidOperationException(error), error, "stock_transfer");
                throw new InvalidOperationException(error);
            }

            // Reduce batch quantity
            batch.Quantity -= quantityToReduce;

            AppLogger.LogInfo("ReduceProductBatchStock", 
                $"Product ID: {productId}, Batch: {batchNo}, New Batch Qty: {batch.Quantity} (was {originalBatchQuantity}), Reduced: {quantityToReduce}", 
                "stock_transfer");

            _context.ProductBatches.Update(batch);

            AppLogger.LogInfo("ReduceProductBatchStock", 
                $"Successfully reduced batch stock for Product ID: {productId}, Batch: {batchNo}, Final Batch Qty: {batch.Quantity}", 
                "stock_transfer");
        }
    }
}
