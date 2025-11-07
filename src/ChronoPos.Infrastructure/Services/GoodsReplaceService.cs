using ChronoPos.Application.DTOs;
using ChronoPos.Application.DTOs.Inventory;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Logging;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Enums;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Service implementation for goods replace operations
/// </summary>
public class GoodsReplaceService : IGoodsReplaceService
{
    private readonly ChronoPosDbContext _context;
    private readonly IGoodsReplaceRepository _goodsReplaceRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IShopLocationRepository _shopLocationRepository;
    private readonly IStockLedgerService? _stockLedgerService;

    public GoodsReplaceService(
        ChronoPosDbContext context,
        IGoodsReplaceRepository goodsReplaceRepository,
        ISupplierRepository supplierRepository,
        IShopLocationRepository shopLocationRepository,
        IStockLedgerService? stockLedgerService = null)
    {
        _context = context;
        _goodsReplaceRepository = goodsReplaceRepository;
        _supplierRepository = supplierRepository;
        _shopLocationRepository = shopLocationRepository;
        _stockLedgerService = stockLedgerService;
    }

    /// <summary>
    /// Get paginated list of goods replaces with filtering
    /// </summary>
    public async Task<PagedResult<GoodsReplaceDto>> GetGoodsReplacesAsync(
        int page = 1, 
        int pageSize = 10, 
        string? searchTerm = null,
        int? supplierId = null,
        int? storeId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.GoodsReplaces
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceReturn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(gr => gr.ReplaceNo.Contains(searchTerm) || 
                                    (gr.Remarks != null && gr.Remarks.Contains(searchTerm)));
        }

        if (supplierId.HasValue)
        {
            query = query.Where(gr => gr.SupplierId == supplierId.Value);
        }

        if (storeId.HasValue)
        {
            query = query.Where(gr => gr.StoreId == storeId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(gr => gr.Status == status);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(gr => gr.ReplaceDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(gr => gr.ReplaceDate <= toDate.Value);
        }

        // Get total count for pagination
        var totalCount = await query.CountAsync();

        // Apply pagination and order by date descending
        var replaces = await query
            .OrderByDescending(gr => gr.ReplaceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(gr => new GoodsReplaceDto
            {
                Id = gr.Id,
                ReplaceNo = gr.ReplaceNo,
                SupplierId = gr.SupplierId,
                SupplierName = gr.Supplier != null ? gr.Supplier.CompanyName : "",
                StoreId = gr.StoreId,
                StoreName = gr.Store != null ? gr.Store.Name : "",
                ReferenceReturnId = gr.ReferenceReturnId,
                ReferenceReturnNo = gr.ReferenceReturn != null ? gr.ReferenceReturn.ReturnNo : null,
                ReplaceDate = gr.ReplaceDate,
                TotalAmount = gr.TotalAmount,
                Status = gr.Status,
                Remarks = gr.Remarks,
                CreatedByName = gr.Creator != null ? gr.Creator.FullName : "",
                CreatedAt = gr.CreatedAt,
                UpdatedAt = gr.UpdatedAt,
                TotalItems = gr.Items.Count(),
                Items = new List<GoodsReplaceItemDto>()
            })
            .ToListAsync();

        return new PagedResult<GoodsReplaceDto>
        {
            Items = replaces,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Get single goods replace by ID with all details
    /// </summary>
    public async Task<GoodsReplaceDto?> GetGoodsReplaceByIdAsync(int replaceId)
    {
        var replace = await _context.GoodsReplaces
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Include(gr => gr.ReferenceReturn)
            .Include(gr => gr.Creator)
            .Include(gr => gr.Items)
                .ThenInclude(gri => gri.Product)
            .Include(gr => gr.Items)
                .ThenInclude(gri => gri.Uom)
            .FirstOrDefaultAsync(gr => gr.Id == replaceId);

        if (replace == null)
            return null;

        return new GoodsReplaceDto
        {
            Id = replace.Id,
            ReplaceNo = replace.ReplaceNo,
            SupplierId = replace.SupplierId,
            SupplierName = replace.Supplier?.CompanyName ?? "",
            StoreId = replace.StoreId,
            StoreName = replace.Store?.Name ?? "",
            ReferenceReturnId = replace.ReferenceReturnId,
            ReferenceReturnNo = replace.ReferenceReturn?.ReturnNo,
            ReplaceDate = replace.ReplaceDate,
            TotalAmount = replace.TotalAmount,
            Status = replace.Status,
            Remarks = replace.Remarks,
            CreatedByName = replace.Creator?.FullName ?? "",
            CreatedAt = replace.CreatedAt,
            UpdatedAt = replace.UpdatedAt,
            TotalItems = replace.Items.Count,
            Items = replace.Items.Select(item => new GoodsReplaceItemDto
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
            }).ToList()
        };
    }

    /// <summary>
    /// Create new goods replace
    /// </summary>
    public async Task<GoodsReplaceDto> CreateGoodsReplaceAsync(CreateGoodsReplaceDto dto)
    {
        try
        {
            AppLogger.LogInfo("CreateGoodsReplaceService", 
                $"Starting goods replace creation - Supplier: {dto.SupplierId}, Store: {dto.StoreId}", 
                "goods_replace");
            
            var replaceNo = await GenerateReplaceNumberAsync();
            AppLogger.LogInfo("CreateGoodsReplaceService", $"Generated replace number: {replaceNo}", "goods_replace");

            // Calculate total amount
            var totalAmount = dto.Items.Sum(item => item.Quantity * item.Rate);

            var replace = new GoodsReplace
            {
                ReplaceNo = replaceNo,
                SupplierId = dto.SupplierId,
                StoreId = dto.StoreId,
                ReferenceReturnId = dto.ReferenceReturnId,
                ReplaceDate = dto.ReplaceDate,
                TotalAmount = totalAmount,
                Status = dto.Status,
                Remarks = dto.Remarks,
                CreatedBy = 1, // TODO: Get from current user context
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            AppLogger.LogInfo("CreateGoodsReplaceService", 
                $"Adding goods replace to context - ReplaceNo: {replace.ReplaceNo}", 
                "goods_replace");
            _context.GoodsReplaces.Add(replace);
            
            AppLogger.LogInfo("CreateGoodsReplaceService", "Calling SaveChangesAsync for replace header", "goods_replace");
            await _context.SaveChangesAsync();
            AppLogger.LogInfo("CreateGoodsReplaceService", 
                $"Successfully saved replace header with ID: {replace.Id}", 
                "goods_replace");

            // Add replace items and increase stock if status is Posted
            AppLogger.LogInfo("CreateGoodsReplaceService", $"Adding {dto.Items.Count} replace items", "goods_replace");
            foreach (var itemDto in dto.Items)
            {
                // Only increase stock for posted replacements
                if (dto.Status == "Posted")
                {
                    AppLogger.LogInfo("CreateGoodsReplaceService", 
                        $"Increasing stock for status: {dto.Status}", 
                        "goods_replace");
                    
                    // 1. Increase product stock quantities
                    await IncreaseProductStockAsync(itemDto.ProductId, itemDto.Quantity);
                    
                    // 2. Add or update product batch (if batch is specified)
                    if (!string.IsNullOrEmpty(itemDto.BatchNo))
                    {
                        await AddOrUpdateProductBatchAsync(
                            itemDto.ProductId, 
                            itemDto.BatchNo, 
                            itemDto.ExpiryDate,
                            itemDto.Quantity,
                            itemDto.Rate);
                    }
                    
                    // 3. Update return item tracking (if this replacement is linked to a return)
                    if (itemDto.ReferenceReturnItemId.HasValue)
                    {
                        await UpdateReturnItemTrackingAsync(itemDto.ReferenceReturnItemId.Value, itemDto.Quantity);
                    }
                }
                else
                {
                    AppLogger.LogInfo("CreateGoodsReplaceService", 
                        $"Skipping stock increase for status: {dto.Status}", 
                        "goods_replace");
                }

                var item = new GoodsReplaceItem
                {
                    ReplaceId = replace.Id,
                    ProductId = itemDto.ProductId,
                    UomId = itemDto.UomId,
                    BatchNo = itemDto.BatchNo,
                    ExpiryDate = itemDto.ExpiryDate,
                    Quantity = itemDto.Quantity,
                    Rate = itemDto.Rate,
                    ReferenceReturnItemId = itemDto.ReferenceReturnItemId,
                    RemarksLine = itemDto.RemarksLine,
                    CreatedAt = DateTime.UtcNow
                };

                _context.GoodsReplaceItems.Add(item);
                AppLogger.LogInfo("CreateGoodsReplaceService", 
                    $"Added item - ProductId: {item.ProductId}, Quantity: {item.Quantity}, Rate: {item.Rate}, Batch: {item.BatchNo}", 
                    "goods_replace");
            }

            AppLogger.LogInfo("CreateGoodsReplaceService", "Calling SaveChangesAsync for replace items", "goods_replace");
            await _context.SaveChangesAsync();
            AppLogger.LogInfo("CreateGoodsReplaceService", "Successfully saved all replace items and tracking updates", "goods_replace");

            // Check if the entire return is now fully replaced (only if status is Posted)
            // IMPORTANT: This must be called AFTER SaveChangesAsync so the tracking updates are committed
            if (dto.Status == "Posted" && dto.ReferenceReturnId.HasValue)
            {
                await CheckAndMarkReturnAsFullyReplacedAsync(dto.ReferenceReturnId.Value);
                // Save the IsTotallyReplaced flag update
                await _context.SaveChangesAsync();
            }

            // Return the created replace
            AppLogger.LogInfo("CreateGoodsReplaceService", 
                $"Retrieving created replace with ID: {replace.Id}", 
                "goods_replace");
            return await GetGoodsReplaceByIdAsync(replace.Id) 
                ?? throw new InvalidOperationException("Failed to retrieve created goods replace");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("CreateGoodsReplaceService", ex, 
                $"Error creating goods replace | Exception: {ex.Message} | InnerException: {ex.InnerException?.Message ?? "None"}", 
                "goods_replace");
            throw;
        }
    }

    /// <summary>
    /// Update existing goods replace
    /// </summary>
    public async Task<GoodsReplaceDto> UpdateGoodsReplaceAsync(int replaceId, CreateGoodsReplaceDto dto)
    {
        var replace = await _context.GoodsReplaces
            .Include(gr => gr.Items)
            .FirstOrDefaultAsync(gr => gr.Id == replaceId);

        if (replace == null)
            throw new ArgumentException("Goods replace not found", nameof(replaceId));

        if (replace.Status != "Pending")
            throw new InvalidOperationException("Cannot update goods replace that is not in pending status");

        // Calculate total amount
        var totalAmount = dto.Items.Sum(item => item.Quantity * item.Rate);

        // Update replace properties
        replace.SupplierId = dto.SupplierId;
        replace.StoreId = dto.StoreId;
        replace.ReferenceReturnId = dto.ReferenceReturnId;
        replace.ReplaceDate = dto.ReplaceDate;
        replace.TotalAmount = totalAmount;
        // Keep status as Pending during update - use PostGoodsReplaceAsync to change to Posted
        replace.Status = "Pending";
        replace.Remarks = dto.Remarks;
        replace.UpdatedAt = DateTime.UtcNow;

        // Remove existing items
        _context.GoodsReplaceItems.RemoveRange(replace.Items);

        // Add new items (without stock updates - those happen only when posting)
        foreach (var itemDto in dto.Items)
        {
            var item = new GoodsReplaceItem
            {
                ReplaceId = replace.Id,
                ProductId = itemDto.ProductId,
                UomId = itemDto.UomId,
                BatchNo = itemDto.BatchNo,
                ExpiryDate = itemDto.ExpiryDate,
                Quantity = itemDto.Quantity,
                Rate = itemDto.Rate,
                ReferenceReturnItemId = itemDto.ReferenceReturnItemId,
                RemarksLine = itemDto.RemarksLine,
                CreatedAt = DateTime.UtcNow
            };

            _context.GoodsReplaceItems.Add(item);
        }

        await _context.SaveChangesAsync();

        return await GetGoodsReplaceByIdAsync(replaceId) 
            ?? throw new InvalidOperationException("Failed to retrieve updated goods replace");
    }

    /// <summary>
    /// Delete goods replace
    /// </summary>
    public async Task<bool> DeleteGoodsReplaceAsync(int replaceId)
    {
        var replace = await _context.GoodsReplaces
            .Include(gr => gr.Items)
            .FirstOrDefaultAsync(gr => gr.Id == replaceId);

        if (replace == null)
            return false;

        if (replace.Status != "Pending")
            return false; // Cannot delete non-pending replacements

        // Remove items first (cascading delete should handle this)
        _context.GoodsReplaceItems.RemoveRange(replace.Items);
        _context.GoodsReplaces.Remove(replace);

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Post goods replace (mark as posted/completed and increase stock)
    /// </summary>
    public async Task<bool> PostGoodsReplaceAsync(int replaceId)
    {
        var replace = await _context.GoodsReplaces
            .Include(gr => gr.Items)
            .FirstOrDefaultAsync(gr => gr.Id == replaceId);

        if (replace == null || replace.Status != "Pending")
            return false;

        // Increase stock for all items AND update return tracking
        foreach (var item in replace.Items)
        {
            // 1. Increase product stock quantities
            await IncreaseProductStockAsync(item.ProductId, item.Quantity);
            
            // 2. Add or update product batch (if batch is specified)
            if (!string.IsNullOrEmpty(item.BatchNo))
            {
                await AddOrUpdateProductBatchAsync(
                    item.ProductId, 
                    item.BatchNo, 
                    item.ExpiryDate,
                    item.Quantity,
                    item.Rate);
            }
            
            // 3. Update return item tracking (if this replacement is linked to a return)
            if (item.ReferenceReturnItemId.HasValue)
            {
                await UpdateReturnItemTrackingAsync(item.ReferenceReturnItemId.Value, item.Quantity);
            }
        }

        replace.Status = "Posted";
        replace.UpdatedAt = DateTime.UtcNow;

        // 4. Check if the entire return is now fully replaced (before saving)
        if (replace.ReferenceReturnId.HasValue)
        {
            await CheckAndMarkReturnAsFullyReplacedAsync(replace.ReferenceReturnId.Value);
        }

        // Save all stock and tracking updates in one transaction
        await _context.SaveChangesAsync();
        
        // Create stock ledger entries AFTER saving (to avoid nested transaction conflict)
        if (_stockLedgerService != null)
        {
            foreach (var item in replace.Items)
            {
                try
                {
                    var stockLedgerDto = new CreateStockLedgerDto
                    {
                        ProductId = item.ProductId,
                        UnitId = (int)item.UomId,
                        MovementType = StockMovementType.Purchase, // Goods replaced adds stock back (like purchase)
                        Qty = item.Quantity,
                        Location = "Main Store",
                        ReferenceType = StockReferenceType.GRN, // Using GRN type as replacement is similar to receiving goods
                        ReferenceId = replace.Id,
                        Note = $"Goods Replaced - {replace.ReplaceNo}"
                    };
                    
                    await _stockLedgerService.CreateAsync(stockLedgerDto);
                    AppLogger.LogInfo("Stock ledger entry created for goods replace", 
                        $"ProductId: {item.ProductId}, Qty: {item.Quantity}, ReplaceNo: {replace.ReplaceNo}", "goods_replace");
                }
                catch (Exception ex)
                {
                    AppLogger.LogError("Failed to create stock ledger entry for goods replace", ex,
                        $"ReplaceId: {replaceId}, ProductId: {item.ProductId}", "goods_replace");
                    // Don't throw - ledger is supplementary
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Cancel goods replace
    /// </summary>
    public async Task<bool> CancelGoodsReplaceAsync(int replaceId)
    {
        var replace = await _context.GoodsReplaces
            .FirstOrDefaultAsync(gr => gr.Id == replaceId);

        if (replace == null || replace.Status == "Posted")
            return false;

        replace.Status = "Cancelled";
        replace.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Get all suppliers
    /// </summary>
    public async Task<List<SupplierDto>> GetSuppliersAsync()
    {
        return await _context.Suppliers
            .Where(s => s.Status == "Active" && s.DeletedAt == null)
            .Select(s => new SupplierDto
            {
                SupplierId = s.SupplierId,
                CompanyName = s.CompanyName,
                OwnerName = s.OwnerName,
                OwnerMobile = s.OwnerMobile,
                Email = s.Email,
                Status = s.Status,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .OrderBy(s => s.CompanyName)
            .ToListAsync();
    }

    /// <summary>
    /// Get all stores
    /// </summary>
    public async Task<List<ShopLocationDto>> GetStoresAsync()
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
    /// Get products available for replacement
    /// </summary>
    public async Task<PagedResult<ProductStockInfoDto>> GetProductsForReplaceAsync(
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
                CurrentStock = p.StockQuantity,
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
    /// Generate replace number
    /// </summary>
    public async Task<string> GenerateReplaceNumberAsync()
    {
        return await _goodsReplaceRepository.GetNextReplaceNumberAsync();
    }

    /// <summary>
    /// Get goods returns by supplier (for linking)
    /// </summary>
    public async Task<List<GoodsReturnDto>> GetGoodsReturnsBySupplierAsync(int supplierId)
    {
        return await _context.GoodsReturns
            .Include(gr => gr.Supplier)
            .Include(gr => gr.Store)
            .Where(gr => gr.SupplierId == supplierId && gr.Status == "Posted")
            .Select(gr => new GoodsReturnDto
            {
                Id = gr.Id,
                ReturnNo = gr.ReturnNo,
                SupplierId = gr.SupplierId,
                SupplierName = gr.Supplier != null ? gr.Supplier.CompanyName : "",
                StoreId = gr.StoreId,
                StoreName = gr.Store != null ? gr.Store.Name : "",
                ReturnDate = gr.ReturnDate,
                TotalAmount = gr.TotalAmount,
                Status = gr.Status,
                Remarks = gr.Remarks
            })
            .OrderByDescending(gr => gr.ReturnDate)
            .ToListAsync();
    }

    /// <summary>
    /// Increase product stock quantities (both InitialStock and StockQuantity)
    /// </summary>
    private async Task IncreaseProductStockAsync(int productId, decimal quantityToAdd)
    {
        AppLogger.LogInfo("IncreaseProductStock", 
            $"Starting stock increase for Product ID: {productId}, Quantity to add: {quantityToAdd}", 
            "goods_replace");

        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            var error = $"Product with ID {productId} not found";
            AppLogger.LogError("IncreaseProductStock", new InvalidOperationException(error), error, "goods_replace");
            throw new InvalidOperationException(error);
        }

        var originalStockQuantity = product.StockQuantity;
        var originalInitialStock = product.InitialStock;

        AppLogger.LogInfo("IncreaseProductStock", 
            $"Product ID: {productId}, Name: '{product.Name}', Current Stock Qty: {originalStockQuantity}, Current Initial Stock: {originalInitialStock}", 
            "goods_replace");

        // Increase both StockQuantity and InitialStock
        product.StockQuantity += (int)quantityToAdd;
        product.InitialStock += quantityToAdd;

        AppLogger.LogInfo("IncreaseProductStock", 
            $"Product ID: {productId}, New Stock Qty: {product.StockQuantity} (was {originalStockQuantity}), New Initial Stock: {product.InitialStock} (was {originalInitialStock}), Added: {quantityToAdd}", 
            "goods_replace");

        _context.Products.Update(product);

        AppLogger.LogInfo("IncreaseProductStock", 
            $"Successfully increased stock for Product ID: {productId}, Name: '{product.Name}', Final Stock Qty: {product.StockQuantity}, Final Initial Stock: {product.InitialStock}", 
            "goods_replace");
    }

    /// <summary>
    /// Add or update product batch stock quantity
    /// </summary>
    private async Task AddOrUpdateProductBatchAsync(
        int productId, 
        string batchNo, 
        DateTime? expiryDate,
        decimal quantity,
        decimal rate)
    {
        AppLogger.LogInfo("AddOrUpdateProductBatch", 
            $"Starting batch operation for Product ID: {productId}, Batch: {batchNo}, Quantity: {quantity}", 
            "goods_replace");

        var batch = await _context.ProductBatches
            .FirstOrDefaultAsync(pb => pb.ProductId == productId && pb.BatchNo == batchNo);

        if (batch == null)
        {
            // Create new batch
            AppLogger.LogInfo("AddOrUpdateProductBatch", 
                $"Creating new batch for Product ID: {productId}, Batch: {batchNo}", 
                "goods_replace");
            
            batch = new ProductBatch
            {
                ProductId = productId,
                BatchNo = batchNo,
                ExpiryDate = expiryDate,
                Quantity = quantity,
                CostPrice = rate,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.ProductBatches.Add(batch);
            
            AppLogger.LogInfo("AddOrUpdateProductBatch", 
                $"Created new batch - Product ID: {productId}, Batch: {batchNo}, Quantity: {quantity}", 
                "goods_replace");
        }
        else
        {
            // Update existing batch
            var originalQuantity = batch.Quantity;
            
            AppLogger.LogInfo("AddOrUpdateProductBatch", 
                $"Updating existing batch - Product ID: {productId}, Batch: {batchNo}, Current Qty: {originalQuantity}", 
                "goods_replace");
            
            batch.Quantity += quantity;
            batch.ExpiryDate = expiryDate ?? batch.ExpiryDate;
            batch.CostPrice = rate; // Update cost price
            
            _context.ProductBatches.Update(batch);
            
            AppLogger.LogInfo("AddOrUpdateProductBatch", 
                $"Updated batch - Product ID: {productId}, Batch: {batchNo}, New Qty: {batch.Quantity} (was {originalQuantity}), Added: {quantity}", 
                "goods_replace");
        }

        AppLogger.LogInfo("AddOrUpdateProductBatch", 
            $"Successfully processed batch for Product ID: {productId}, Batch: {batchNo}", 
            "goods_replace");
    }

    /// <summary>
    /// Update return item tracking when a replacement is posted
    /// </summary>
    private async Task UpdateReturnItemTrackingAsync(int returnItemId, decimal replacedQuantity)
    {
        AppLogger.LogInfo("UpdateReturnItemTracking", 
            $"Updating tracking for Return Item ID: {returnItemId}, Replaced Qty: {replacedQuantity}", 
            "goods_replace");

        var returnItem = await _context.GoodsReturnItems.FindAsync(returnItemId);
        if (returnItem == null)
        {
            AppLogger.LogWarning("UpdateReturnItemTracking", 
                $"Return item with ID {returnItemId} not found", 
                "goods_replace");
            return;
        }

        var originalReplacedQty = returnItem.AlreadyReplacedQuantity;
        
        // Increase the already replaced quantity
        returnItem.AlreadyReplacedQuantity += replacedQuantity;
        
        // Check if this item is now totally replaced
        if (returnItem.AlreadyReplacedQuantity >= returnItem.Quantity)
        {
            returnItem.IsTotallyReplaced = true;
            AppLogger.LogInfo("UpdateReturnItemTracking", 
                $"Return Item ID {returnItemId} is now TOTALLY REPLACED (Qty: {returnItem.Quantity}, Replaced: {returnItem.AlreadyReplacedQuantity})", 
                "goods_replace");
        }
        else
        {
            AppLogger.LogInfo("UpdateReturnItemTracking", 
                $"Return Item ID {returnItemId} is PARTIALLY REPLACED (Qty: {returnItem.Quantity}, Replaced: {returnItem.AlreadyReplacedQuantity})", 
                "goods_replace");
        }

        _context.GoodsReturnItems.Update(returnItem);
        
        AppLogger.LogInfo("UpdateReturnItemTracking", 
            $"Updated Return Item ID {returnItemId}: AlreadyReplacedQty {originalReplacedQty} → {returnItem.AlreadyReplacedQuantity}, IsTotallyReplaced: {returnItem.IsTotallyReplaced}", 
            "goods_replace");
    }

    /// <summary>
    /// Check if all items in a return are fully replaced and mark the return accordingly
    /// </summary>
    private async Task CheckAndMarkReturnAsFullyReplacedAsync(int returnId)
    {
        AppLogger.LogInfo("CheckAndMarkReturnAsFullyReplaced", 
            $"Checking if Return ID {returnId} is fully replaced", 
            "goods_replace");

        var goodsReturn = await _context.GoodsReturns
            .Include(gr => gr.Items)
            .FirstOrDefaultAsync(gr => gr.Id == returnId);

        if (goodsReturn == null)
        {
            AppLogger.LogWarning("CheckAndMarkReturnAsFullyReplaced", 
                $"Goods return with ID {returnId} not found", 
                "goods_replace");
            return;
        }

        // Check if ALL items are totally replaced
        bool allItemsReplaced = goodsReturn.Items.Any() && goodsReturn.Items.All(item => item.IsTotallyReplaced);

        if (allItemsReplaced && !goodsReturn.IsTotallyReplaced)
        {
            goodsReturn.IsTotallyReplaced = true;
            _context.GoodsReturns.Update(goodsReturn);
            
            AppLogger.LogInfo("CheckAndMarkReturnAsFullyReplaced", 
                $"Goods Return {goodsReturn.ReturnNo} (ID: {returnId}) is now FULLY REPLACED - all {goodsReturn.Items.Count} items are replaced", 
                "goods_replace");
        }
        else if (allItemsReplaced)
        {
            AppLogger.LogInfo("CheckAndMarkReturnAsFullyReplaced", 
                $"Goods Return {goodsReturn.ReturnNo} (ID: {returnId}) was already marked as fully replaced", 
                "goods_replace");
        }
        else
        {
            int totalItems = goodsReturn.Items.Count;
            int replacedItems = goodsReturn.Items.Count(item => item.IsTotallyReplaced);
            
            AppLogger.LogInfo("CheckAndMarkReturnAsFullyReplaced", 
                $"Goods Return {goodsReturn.ReturnNo} (ID: {returnId}) is PARTIALLY REPLACED - {replacedItems}/{totalItems} items replaced", 
                "goods_replace");
        }
    }
}
