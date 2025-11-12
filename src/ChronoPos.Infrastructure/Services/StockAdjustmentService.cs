using ChronoPos.Application.DTOs;
using ChronoPos.Application.DTOs.Inventory;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Logging;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Enums;
using ChronoPos.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for stock adjustment operations
    /// </summary>
    public class StockAdjustmentService : IStockAdjustmentService
    {
        private readonly ChronoPosDbContext _context;
        private readonly IStockLedgerService? _stockLedgerService;

        public StockAdjustmentService(ChronoPosDbContext context, IStockLedgerService? stockLedgerService = null)
        {
            _context = context;
            _stockLedgerService = stockLedgerService;
        }

        /// <summary>
        /// Get paginated list of stock adjustments with filtering
        /// </summary>
        public async Task<PagedResult<StockAdjustmentDto>> GetStockAdjustmentsAsync(
            int page = 1, 
            int pageSize = 10, 
            string? searchTerm = null,
            int? storeLocationId = null,
            int? reasonId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.StockAdjustments
                .Include(s => s.StoreLocation)
                .Include(s => s.Reason)
                .Include(s => s.Creator)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.AdjustmentNo.Contains(searchTerm) || 
                                       (s.Remarks != null && s.Remarks.Contains(searchTerm)) ||
                                       s.Items.Any(i => i.Product != null && i.Product.Name.Contains(searchTerm)));
            }

            if (storeLocationId.HasValue)
            {
                query = query.Where(s => s.StoreLocationId == storeLocationId.Value);
            }

            if (reasonId.HasValue)
            {
                query = query.Where(s => s.ReasonId == reasonId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(s => s.AdjustmentDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(s => s.AdjustmentDate <= toDate.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and order by date descending
            var adjustments = await query
                .OrderByDescending(s => s.AdjustmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StockAdjustmentDto
                {
                    AdjustmentId = s.AdjustmentId,
                    AdjustmentNo = s.AdjustmentNo,
                    AdjustmentDate = s.AdjustmentDate,
                    StoreLocationId = s.StoreLocationId,
                    StoreLocationName = s.StoreLocation != null ? s.StoreLocation.LocationName : "",
                    ReasonId = s.ReasonId,
                    ReasonName = s.Reason != null ? s.Reason.Name : "",
                    Status = s.Status,
                    Remarks = s.Remarks,
                    CreatedByName = s.Creator != null ? s.Creator.FullName : "",
                    CreatedAt = s.CreatedAt,
                    Items = new List<StockAdjustmentItemDto>()
                })
                .ToListAsync();

            return new PagedResult<StockAdjustmentDto>
            {
                Items = adjustments,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Get single stock adjustment by ID with all details
        /// </summary>
        public async Task<StockAdjustmentDto?> GetStockAdjustmentByIdAsync(int adjustmentId)
        {
            var adjustment = await _context.StockAdjustments
                .Include(s => s.StoreLocation)
                .Include(s => s.Reason)
                .Include(s => s.Creator)
                .Include(s => s.Updater)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Uom)
                .FirstOrDefaultAsync(s => s.AdjustmentId == adjustmentId);

            if (adjustment == null)
                return null;

            return new StockAdjustmentDto
            {
                AdjustmentId = adjustment.AdjustmentId,
                AdjustmentNo = adjustment.AdjustmentNo,
                AdjustmentDate = adjustment.AdjustmentDate,
                StoreLocationId = adjustment.StoreLocationId,
                StoreLocationName = adjustment.StoreLocation?.LocationName ?? "",
                ReasonId = adjustment.ReasonId,
                ReasonName = adjustment.Reason?.Name ?? "",
                Status = adjustment.Status,
                Remarks = adjustment.Remarks,
                CreatedByName = adjustment.Creator?.FullName ?? "",
                CreatedAt = adjustment.CreatedAt,
                Items = adjustment.Items.Select(i => new StockAdjustmentItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "",
                    ProductSku = i.Product?.SKU ?? "",
                    UomName = i.Uom?.Name ?? "",
                    QuantityBefore = i.QuantityBefore,
                    QuantityAfter = i.QuantityAfter,
                    DifferenceQty = i.DifferenceQty,
                    ConversionFactor = i.ConversionFactor,
                    ReasonLine = i.ReasonLine,
                    RemarksLine = i.RemarksLine,
                    // Financial data from Product
                    CostPrice = i.Product?.Cost ?? 0,
                    TaxRate = i.Product?.TaxRate ?? 0,
                    // Adjustment-level data for flattened display
                    AdjustmentNo = adjustment.AdjustmentNo,
                    AdjustmentDate = adjustment.AdjustmentDate
                }).ToList()
            };
        }

        /// <summary>
        /// Create new stock adjustment
        /// </summary>
        public async Task<StockAdjustmentDto> CreateStockAdjustmentAsync(CreateStockAdjustmentDto createDto)
        {
            AppLogger.LogSeparator("STARTING CreateStockAdjustmentAsync", "stock_adjustment");
            AppLogger.LogInfo($"Input DTO - AdjustmentDate: {createDto.AdjustmentDate}", null, "stock_adjustment");
            AppLogger.LogInfo($"Input DTO - StoreLocationId: {createDto.StoreLocationId}", null, "stock_adjustment");
            AppLogger.LogInfo($"Input DTO - ReasonId: {createDto.ReasonId}", null, "stock_adjustment");
            AppLogger.LogInfo($"Input DTO - Remarks: {createDto.Remarks}", null, "stock_adjustment");
            AppLogger.LogInfo($"Input DTO - Items Count: {createDto.Items?.Count ?? 0}", null, "stock_adjustment");
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            AppLogger.LogInfo("Database transaction started", null, "stock_adjustment");
            
            try
            {
                // Validate and ensure required foreign key records exist
                AppLogger.LogInfo("Validating foreign key constraints...", null, "stock_adjustment");
                await EnsureRequiredDataExistsAsync(createDto);
                
                // Generate adjustment number
                AppLogger.LogInfo("Generating adjustment number...", null, "stock_adjustment");
                var adjustmentNo = await GenerateAdjustmentNumberAsync();
                AppLogger.LogInfo($"Generated adjustment number: {adjustmentNo}", null, "stock_adjustment");

                // Create the main adjustment record
                AppLogger.LogInfo("Creating main adjustment record...", null, "stock_adjustment");
                var adjustment = new StockAdjustment
                {
                    AdjustmentNo = adjustmentNo,
                    AdjustmentDate = createDto.AdjustmentDate,
                    StoreLocationId = createDto.StoreLocationId,
                    ReasonId = createDto.ReasonId,
                    Status = "Pending",
                    Remarks = createDto.Remarks,
                    CreatedBy = 1, // TODO: Get current user ID from context/session
                    CreatedAt = DateTime.Now
                };

                AppLogger.LogInfo("Adding adjustment to context...", null, "stock_adjustment");
                _context.StockAdjustments.Add(adjustment);
                
                AppLogger.LogInfo("Saving main adjustment record...", null, "stock_adjustment");
                await _context.SaveChangesAsync();
                AppLogger.LogInfo($"Main adjustment saved with ID: {adjustment.AdjustmentId}", null, "stock_adjustment");

                // Add adjustment items
                AppLogger.LogInfo($"Processing {createDto.Items?.Count ?? 0} adjustment items...", null, "stock_adjustment");
                if (createDto.Items != null)
                {
                    foreach (var itemDto in createDto.Items)
                    {
                        AppLogger.LogInfo($"Creating item for ProductId: {itemDto.ProductId}", $"BatchNo: '{itemDto.BatchNo ?? "None"}'", "stock_adjustment");
                        AppLogger.LogInfo($"Item - QuantityBefore: {itemDto.QuantityBefore}, QuantityAfter: {itemDto.QuantityAfter}", null, "stock_adjustment");
                        
                        var item = new StockAdjustmentItem
                        {
                            AdjustmentId = adjustment.AdjustmentId,
                            ProductId = itemDto.ProductId,
                            UomId = itemDto.UomId,
                            BatchNo = itemDto.BatchNo, // Use provided batch number from DTO
                            QuantityBefore = itemDto.QuantityBefore,
                            QuantityAfter = itemDto.QuantityAfter,
                            DifferenceQty = itemDto.QuantityAfter - itemDto.QuantityBefore,
                            ConversionFactor = itemDto.ConversionFactor,
                            ReasonLine = itemDto.ReasonLine,
                            RemarksLine = itemDto.RemarksLine
                        };

                        AppLogger.LogInfo($"Adding item to context - DifferenceQty: {item.DifferenceQty}", $"BatchNo: '{item.BatchNo ?? "None"}'", "stock_adjustment");
                        _context.StockAdjustmentItems.Add(item);
                    }
                }

                AppLogger.LogInfo("Saving adjustment items...", null, "stock_adjustment");
                await _context.SaveChangesAsync();
                AppLogger.LogInfo("All items saved successfully", null, "stock_adjustment");
                
                // Update actual product stock levels for each adjustment item
                AppLogger.LogSeparator("Updating Product Stock Levels", "stock_adjustment");
                if (createDto.Items != null)
                {
                    foreach (var itemDto in createDto.Items)
                    {
                        AppLogger.LogInfo($"Processing adjustment for ProductId: {itemDto.ProductId}", $"BatchNo: '{itemDto.BatchNo ?? "None"}', Mode: {itemDto.AdjustmentMode}", "stock_adjustment");
                        AppLogger.LogInfo($"Adjustment mode: {itemDto.AdjustmentMode}", null, "stock_adjustment");
                        AppLogger.LogInfo($"New quantity: {itemDto.QuantityAfter}", null, "stock_adjustment");
                        
                        // Update the actual product stock quantity and initial stock using increment/decrement logic
                        var product = await _context.Products.FindAsync(itemDto.ProductId);
                        if (product != null)
                        {
                            var previousStock = product.StockQuantity;
                            var previousInitialStock = product.InitialStock;
                            
                            decimal changeAmountInBaseUnit;
                            
                            if (itemDto.AdjustmentMode == StockAdjustmentMode.ProductUnit)
                            {
                                // For ProductUnit mode, apply conversion factor to change amount
                                var conversionFactor = itemDto.ConversionFactor;
                                changeAmountInBaseUnit = itemDto.ChangeAmount * conversionFactor;
                                
                                AppLogger.LogInfo($"ProductUnit mode: Change {itemDto.ChangeAmount} × Conversion Factor {conversionFactor} = {changeAmountInBaseUnit}", null, "stock_adjustment");
                                
                                // Also update the ProductUnit table
                                var productUnit = await _context.ProductUnits.FindAsync(itemDto.ProductUnitId);
                                if (productUnit != null)
                                {
                                    var previousUnitQty = productUnit.QtyInUnit;
                                    if (itemDto.IsIncrement)
                                    {
                                        productUnit.QtyInUnit = (int)(previousUnitQty + itemDto.ChangeAmount);
                                    }
                                    else
                                    {
                                        productUnit.QtyInUnit = (int)(previousUnitQty - itemDto.ChangeAmount);
                                    }
                                    productUnit.UpdatedAt = DateTime.Now;
                                    
                                    AppLogger.LogInfo($"ProductUnit updated: {previousUnitQty} → {productUnit.QtyInUnit}", null, "stock_adjustment");
                                }
                            }
                            else
                            {
                                // For Product mode, use change amount directly
                                changeAmountInBaseUnit = itemDto.ChangeAmount;
                                AppLogger.LogInfo($"Product mode: Using change amount directly = {changeAmountInBaseUnit}", null, "stock_adjustment");
                            }
                            
                            // Apply increment or decrement to Product
                            if (itemDto.IsIncrement)
                            {
                                product.InitialStock = previousInitialStock + changeAmountInBaseUnit;
                                product.StockQuantity = (int)Math.Round(previousStock + changeAmountInBaseUnit);
                                AppLogger.LogInfo($"INCREMENT: Adding {changeAmountInBaseUnit} to stock", null, "stock_adjustment");
                            }
                            else
                            {
                                product.InitialStock = previousInitialStock - changeAmountInBaseUnit;
                                product.StockQuantity = (int)Math.Round(Math.Max(0, previousStock - changeAmountInBaseUnit)); // Prevent negative stock
                                AppLogger.LogInfo($"DECREMENT: Subtracting {changeAmountInBaseUnit} from stock", null, "stock_adjustment");
                            }
                            
                            product.UpdatedAt = DateTime.Now;
                            
                            AppLogger.LogInfo($"Product stock updated: {previousStock} → {product.StockQuantity}", null, "stock_adjustment");
                            AppLogger.LogInfo($"Product initial stock updated: {previousInitialStock} → {product.InitialStock}", null, "stock_adjustment");
                            
                            // Update ProductBatch quantity if BatchNo is specified
                            if (!string.IsNullOrEmpty(itemDto.BatchNo))
                            {
                                AppLogger.LogInfo($"BATCH UPDATE: Processing BatchNo: {itemDto.BatchNo} for ProductId: {itemDto.ProductId}", $"ChangeAmount: {changeAmountInBaseUnit}, IsIncrement: {itemDto.IsIncrement}", "stock_adjustment");
                                
                                var productBatch = await _context.ProductBatches
                                    .FirstOrDefaultAsync(pb => pb.ProductId == itemDto.ProductId && pb.BatchNo == itemDto.BatchNo);
                                
                                if (productBatch != null)
                                {
                                    var previousBatchQty = productBatch.Quantity;
                                    
                                    // Apply the same change amount to the batch
                                    if (itemDto.IsIncrement)
                                    {
                                        productBatch.Quantity += changeAmountInBaseUnit;
                                        AppLogger.LogInfo($"BATCH INCREMENT: Adding {changeAmountInBaseUnit} to batch quantity", null, "stock_adjustment");
                                    }
                                    else
                                    {
                                        productBatch.Quantity = Math.Max(0, productBatch.Quantity - changeAmountInBaseUnit);
                                        AppLogger.LogInfo($"BATCH DECREMENT: Subtracting {changeAmountInBaseUnit} from batch quantity (prevent negative)", null, "stock_adjustment");
                                    }
                                    
                                    AppLogger.LogInfo($"BATCH SUCCESS: ProductBatch '{itemDto.BatchNo}' quantity updated: {previousBatchQty} → {productBatch.Quantity}", null, "stock_adjustment");
                                }
                                else
                                {
                                    AppLogger.LogError($"BATCH ERROR: ProductBatch not found for ProductId: {itemDto.ProductId}, BatchNo: '{itemDto.BatchNo}'", null, null, "stock_adjustment");
                                    // Log available batches for debugging
                                    var availableBatches = await _context.ProductBatches
                                        .Where(pb => pb.ProductId == itemDto.ProductId)
                                        .Select(pb => new { pb.BatchNo, pb.Quantity })
                                        .ToListAsync();
                                    AppLogger.LogInfo($"Available batches for ProductId {itemDto.ProductId}: {string.Join(", ", availableBatches.Select(b => $"'{b.BatchNo}'({b.Quantity})"))}", null, "stock_adjustment");
                                }
                            }
                            else
                            {
                                AppLogger.LogInfo($"BATCH SKIP: No BatchNo provided for ProductId: {itemDto.ProductId}, skipping batch update", null, "stock_adjustment");
                            }
                        }
                        else
                        {
                            AppLogger.LogWarning($"Product not found for ID: {itemDto.ProductId}", null, "stock_adjustment");
                        }
                    }
                    
                    AppLogger.LogInfo("Saving product stock updates...", null, "stock_adjustment");
                    await _context.SaveChangesAsync();
                    AppLogger.LogInfo("Product stock levels updated successfully", null, "stock_adjustment");
                }
                
                AppLogger.LogInfo("Committing transaction...", null, "stock_adjustment");
                await transaction.CommitAsync();
                AppLogger.LogInfo("Transaction committed successfully", null, "stock_adjustment");

                // Create stock ledger entries AFTER transaction commits (to avoid nested transaction issues)
                if (_stockLedgerService != null && createDto.Items != null)
                {
                    AppLogger.LogInfo($"🔵 Creating stock ledger entries for {createDto.Items.Count} adjustment items", null, "stock_adjustment");
                    foreach (var item in createDto.Items)
                    {
                        try
                        {
                            AppLogger.LogInfo($"  → Processing adjustment item: ProductId={item.ProductId}, QtyChange={item.QuantityAfter - item.QuantityBefore}", 
                                null, "stock_adjustment");
                            
                            // Get product to find its ProductUnit
                            var product = await _context.Products
                                .Include(p => p.ProductUnits)
                                .FirstOrDefaultAsync(p => p.Id == item.ProductId);
                                
                            if (product == null)
                            {
                                AppLogger.LogWarning($"  ❌ Product {item.ProductId} not found for stock ledger", 
                                    null, "stock_adjustment");
                                continue;
                            }

                            // Find ProductUnit for this product
                            // Stock adjustment can be for product (no unit) or product unit (with unit)
                            int? productUnitId = null;
                            if (product.SellingUnitId.HasValue && item.UomId > 0)
                            {
                                AppLogger.LogInfo($"  🔍 Product {product.Id} has SellingUnitId={product.SellingUnitId}, UomId={item.UomId}, searching for ProductUnit...", 
                                    null, "stock_adjustment");
                                
                                // Find ProductUnit that matches the UomId from adjustment
                                var productUnit = product.ProductUnits?
                                    .FirstOrDefault(pu => pu.UnitId == item.UomId);
                                productUnitId = productUnit?.Id;
                                
                                if (productUnitId == null)
                                {
                                    AppLogger.LogWarning($"  ⚠️ ProductUnit NOT FOUND for Product {product.Id} with UomId {item.UomId}. ProductUnits count: {product.ProductUnits?.Count ?? 0}", 
                                        null, "stock_adjustment");
                                    AppLogger.LogInfo($"  ℹ️ Will save stock ledger with NULL unit_id (product-level adjustment)", 
                                        null, "stock_adjustment");
                                }
                                else
                                {
                                    AppLogger.LogInfo($"  ✅ ProductUnit FOUND: ProductUnitId={productUnitId}", 
                                        null, "stock_adjustment");
                                }
                            }
                            else
                            {
                            AppLogger.LogInfo($"  ℹ️ Product {product.Id} has NO SellingUnitId or UomId is 0 - product-level adjustment (NULL unit_id)", 
                                null, "stock_adjustment");
                            }
                            
                            // Calculate quantity change: positive for increment, negative for decrement
                            decimal qtyChange = item.IsIncrement ? item.ChangeAmount : -item.ChangeAmount;
                            
                            AppLogger.LogInfo($"  📊 Adjustment details: IsIncrement={item.IsIncrement}, ChangeAmount={item.ChangeAmount}, QtyChange={qtyChange}", 
                                null, "stock_adjustment");
                            
                            var ledgerDto = new CreateStockLedgerDto
                            {
                                ProductId = item.ProductId,
                                UnitId = productUnitId, // Can be null for product-level adjustments
                                MovementType = StockMovementType.Adjustment,
                                Qty = qtyChange, // Positive for increment, negative for decrement
                                Location = "Main Store",
                                ReferenceType = StockReferenceType.Adjustment,
                                ReferenceId = adjustment.AdjustmentId,
                                Note = $"Stock Adjustment - {adjustment.AdjustmentNo} ({(item.IsIncrement ? "Increment" : "Decrement")} {item.ChangeAmount})"
                            };
                            
                            AppLogger.LogInfo($"  📝 Creating stock ledger: ProductId={ledgerDto.ProductId}, UnitId={ledgerDto.UnitId?.ToString() ?? "NULL"}, Qty={ledgerDto.Qty}", 
                                null, "stock_adjustment");                            await _stockLedgerService.CreateAsync(ledgerDto);
                            
                            AppLogger.LogInfo($"  ✅ Stock ledger entry CREATED for ProductId {item.ProductId}", 
                                null, "stock_adjustment");
                        }
                        catch (Exception ledgerEx)
                        {
                            AppLogger.LogError($"  ❌ Failed to create stock ledger entry for ProductId: {item.ProductId}", 
                                ledgerEx, null, "stock_adjustment");
                            // Don't throw - ledger is supplementary
                        }
                    }
                    
                    AppLogger.LogInfo($"🔵 Stock ledger entries completed for adjustment {adjustment.AdjustmentNo}", 
                        null, "stock_adjustment");
                }

                // Return the created adjustment as DTO
                AppLogger.LogInfo($"Retrieving created adjustment with ID: {adjustment.AdjustmentId}", null, "stock_adjustment");
                var result = await GetStockAdjustmentByIdAsync(adjustment.AdjustmentId);
                
                if (result == null)
                {
                    AppLogger.LogError("Failed to retrieve created adjustment", null, null, "stock_adjustment");
                    throw new InvalidOperationException("Failed to retrieve created adjustment");
                }
                
                AppLogger.LogInfo($"SUCCESS: Returning adjustment DTO with number: {result.AdjustmentNo}", null, "stock_adjustment");
                AppLogger.LogSeparator("CreateStockAdjustmentAsync COMPLETED", "stock_adjustment");
                return result;
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"ERROR in CreateStockAdjustmentAsync: {ex.Message}", ex, null, "stock_adjustment");
                AppLogger.LogError($"Exception type: {ex.GetType().Name}", null, null, "stock_adjustment");
                AppLogger.LogError($"Stack trace: {ex.StackTrace}", null, null, "stock_adjustment");
                
                // Log inner exception details
                if (ex.InnerException != null)
                {
                    AppLogger.LogError($"INNER EXCEPTION: {ex.InnerException.Message}", ex.InnerException, null, "stock_adjustment");
                    AppLogger.LogError($"Inner exception type: {ex.InnerException.GetType().Name}", null, null, "stock_adjustment");
                    
                    if (ex.InnerException.InnerException != null)
                    {
                        AppLogger.LogError($"INNER INNER EXCEPTION: {ex.InnerException.InnerException.Message}", ex.InnerException.InnerException, null, "stock_adjustment");
                    }
                }
                
                AppLogger.LogWarning("Rolling back transaction...", null, "stock_adjustment");
                await transaction.RollbackAsync();
                AppLogger.LogWarning("Transaction rolled back", null, "stock_adjustment");
                throw;
            }
        }

        /// <summary>
        /// Update existing stock adjustment
        /// </summary>
        public async Task<StockAdjustmentDto> UpdateStockAdjustmentAsync(int adjustmentId, CreateStockAdjustmentDto updateDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var adjustment = await _context.StockAdjustments
                    .Include(s => s.Items)
                    .FirstOrDefaultAsync(s => s.AdjustmentId == adjustmentId);

                if (adjustment == null || adjustment.Status == "Completed")
                    throw new InvalidOperationException("Adjustment not found or already completed");

                // Update main record
                adjustment.AdjustmentDate = updateDto.AdjustmentDate;
                adjustment.StoreLocationId = updateDto.StoreLocationId;
                adjustment.ReasonId = updateDto.ReasonId;
                adjustment.Remarks = updateDto.Remarks;
                adjustment.UpdatedBy = 1; // TODO: Get current user ID from context/session
                adjustment.UpdatedAt = DateTime.Now;

                // Remove existing items
                _context.StockAdjustmentItems.RemoveRange(adjustment.Items);

                // Add updated items
                foreach (var itemDto in updateDto.Items)
                {
                    var item = new StockAdjustmentItem
                    {
                        AdjustmentId = adjustment.AdjustmentId,
                        ProductId = itemDto.ProductId,
                        UomId = itemDto.UomId,
                        BatchNo = itemDto.BatchNo, // Use provided batch number from DTO
                        QuantityBefore = itemDto.QuantityBefore,
                        QuantityAfter = itemDto.QuantityAfter,
                        DifferenceQty = itemDto.QuantityAfter - itemDto.QuantityBefore,
                        ConversionFactor = itemDto.ConversionFactor,
                        ReasonLine = itemDto.ReasonLine,
                        RemarksLine = itemDto.RemarksLine
                    };

                    _context.StockAdjustmentItems.Add(item);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                // Return the updated adjustment as DTO
                return await GetStockAdjustmentByIdAsync(adjustmentId) ?? 
                       throw new InvalidOperationException("Failed to retrieve updated adjustment");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Complete stock adjustment and update inventory
        /// </summary>
        public async Task<bool> CompleteStockAdjustmentAsync(int adjustmentId, int completedBy)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var adjustment = await _context.StockAdjustments
                    .Include(s => s.Items)
                        .ThenInclude(i => i.Product)
                    .Include(s => s.Items)
                        .ThenInclude(i => i.Uom)
                    .FirstOrDefaultAsync(s => s.AdjustmentId == adjustmentId);

                if (adjustment == null || adjustment.Status == "Completed")
                    return false;

                // Update adjustment status
                adjustment.Status = "Completed";
                adjustment.UpdatedBy = completedBy;
                adjustment.UpdatedAt = DateTime.Now;

                // Create stock movements for each item
                foreach (var item in adjustment.Items)
                {
                    if (item.DifferenceQty != 0)
                    {
                        var movement = new StockMovement
                        {
                            ProductId = item.ProductId,
                            UomId = item.UomId,
                            LocationId = adjustment.StoreLocationId,
                            MovementType = item.DifferenceQty > 0 ? "IN" : "OUT",
                            Quantity = Math.Abs(item.DifferenceQty),
                            ReferenceType = "StockAdjustment",
                            ReferenceId = adjustment.AdjustmentId,
                            BatchId = null, // No batch tracking for now
                            CreatedBy = completedBy,
                            CreatedAt = DateTime.Now
                        };

                        _context.StockMovements.Add(movement);
                        
                        // Create stock ledger entry
                        if (_stockLedgerService != null)
                        {
                            try
                            {
                                // Get product to find its ProductUnit
                                var product = await _context.Products
                                    .Include(p => p.ProductUnits)
                                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);
                                    
                                if (product == null)
                                {
                                    AppLogger.LogWarning($"Product {item.ProductId} not found for stock ledger in CompleteStockAdjustment", 
                                        null, "stock_adjustment");
                                    continue;
                                }

                                // Find ProductUnit for this product
                                int? productUnitId = null;
                                if (product.SellingUnitId.HasValue && item.UomId > 0)
                                {
                                    var productUnit = product.ProductUnits?
                                        .FirstOrDefault(pu => pu.UnitId == item.UomId);
                                    productUnitId = productUnit?.Id;
                                    
                                    if (productUnitId == null)
                                    {
                                        AppLogger.LogWarning($"ProductUnit NOT FOUND for Product {product.Id} with UomId {item.UomId} in CompleteStockAdjustment", 
                                            null, "stock_adjustment");
                                    }
                                }
                                
                                var stockLedgerDto = new CreateStockLedgerDto
                                {
                                    ProductId = item.ProductId,
                                    UnitId = productUnitId, // Can be null for product-level adjustments
                                    MovementType = StockMovementType.Adjustment,
                                    Qty = item.DifferenceQty, // Can be positive or negative
                                    Location = "Main Store",
                                    ReferenceType = StockReferenceType.Adjustment,
                                    ReferenceId = adjustment.AdjustmentId,
                                    Note = $"Stock Adjustment - {adjustment.AdjustmentNo}"
                                };
                                
                                await _stockLedgerService.CreateAsync(stockLedgerDto);
                                
                                AppLogger.LogInfo($"Stock ledger created in CompleteStockAdjustment for ProductId {item.ProductId}, UnitId={productUnitId?.ToString() ?? "NULL"}", 
                                    null, "stock_adjustment");
                            }
                            catch (Exception ex)
                            {
                                AppLogger.LogError("Failed to create stock ledger entry for adjustment in CompleteStockAdjustment", ex,
                                    $"AdjustmentId: {adjustmentId}, ProductId: {item.ProductId}", "stock_adjustment");
                                // Don't throw - ledger is supplementary
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Delete stock adjustment (only if pending)
        /// </summary>
        public async Task<bool> DeleteStockAdjustmentAsync(int adjustmentId)
        {
            var adjustment = await _context.StockAdjustments
                .FirstOrDefaultAsync(s => s.AdjustmentId == adjustmentId);

            if (adjustment == null || adjustment.Status == "Completed")
                return false;

            _context.StockAdjustments.Remove(adjustment);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Approve stock adjustment (same as complete for this implementation)
        /// </summary>
        public async Task<bool> ApproveStockAdjustmentAsync(int adjustmentId)
        {
            return await CompleteStockAdjustmentAsync(adjustmentId, 1); // TODO: Get current user ID
        }

        /// <summary>
        /// Cancel stock adjustment
        /// </summary>
        public async Task<bool> CancelStockAdjustmentAsync(int adjustmentId)
        {
            var adjustment = await _context.StockAdjustments
                .FirstOrDefaultAsync(s => s.AdjustmentId == adjustmentId);

            if (adjustment == null || adjustment.Status == "Completed")
                return false;

            adjustment.Status = "Cancelled";
            adjustment.UpdatedBy = 1; // TODO: Get current user ID
            adjustment.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Get products for adjustment with current stock
        /// </summary>
        public async Task<PagedResult<ProductStockInfoDto>> GetProductsForAdjustmentAsync(
            int page = 1,
            int pageSize = 20,
            string? searchTerm = null, 
            int? categoryId = null,
            int? storeLocationId = null)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || 
                                       p.SKU != null && p.SKU.Contains(searchTerm));
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
                    ProductCode = p.SKU ?? "",
                    ProductName = p.Name,
                    CategoryName = p.Category != null ? p.Category.Name : "",
                    CurrentStock = p.StockQuantity, // Get actual stock from Product table
                    UomId = 1, // TODO: Get default UOM for product
                    UomName = "PCS" // TODO: Get from UOM table
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
        /// Get stock adjustment reasons
        /// </summary>
        public async Task<List<StockAdjustmentReasonDto>> GetAdjustmentReasonsAsync()
        {
            return await _context.StockAdjustmentReasons
                .Where(r => r.Status == "Active")
                .OrderBy(r => r.Name)
                .Select(r => new StockAdjustmentReasonDto
                {
                    Id = r.StockAdjustmentReasonsId,
                    Name = r.Name,
                    Description = r.Description,
                    IsActive = r.Status == "Active"
                })
                .ToListAsync();
        }

        /// <summary>
        /// Create a new adjustment reason if it doesn't exist, or return existing reason ID
        /// </summary>
        public async Task<int> CreateReasonIfNotExistsAsync(string reasonName)
        {
            Console.WriteLine($"[StockAdjustmentService] CreateReasonIfNotExistsAsync called with reason: '{reasonName}'");
            
            if (string.IsNullOrWhiteSpace(reasonName))
            {
                Console.WriteLine("[StockAdjustmentService] ERROR: Reason name is null or empty");
                throw new ArgumentException("Reason name cannot be empty", nameof(reasonName));
            }

            try
            {
                Console.WriteLine("[StockAdjustmentService] Checking if reason already exists...");
                // Check if reason already exists
                var existingReason = await _context.StockAdjustmentReasons
                    .FirstOrDefaultAsync(r => r.Name.ToLower() == reasonName.ToLower());

                if (existingReason != null)
                {
                    Console.WriteLine($"[StockAdjustmentService] Found existing reason with ID: {existingReason.StockAdjustmentReasonsId}");
                    return existingReason.StockAdjustmentReasonsId;
                }

                Console.WriteLine("[StockAdjustmentService] Reason not found, creating new reason...");
                // Create new reason
                var newReason = new StockAdjustmentReason
                {
                    Name = reasonName.Trim(),
                    Description = $"Custom reason: {reasonName.Trim()}",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = null // TODO: Get current user ID
                };

                Console.WriteLine($"[StockAdjustmentService] Adding new reason to context: {newReason.Name}");
                _context.StockAdjustmentReasons.Add(newReason);
                
                Console.WriteLine("[StockAdjustmentService] Saving changes to database...");
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"[StockAdjustmentService] New reason saved with ID: {newReason.StockAdjustmentReasonsId}");
                return newReason.StockAdjustmentReasonsId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StockAdjustmentService] ERROR in CreateReasonIfNotExistsAsync: {ex.Message}");
                Console.WriteLine($"[StockAdjustmentService] Exception details: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Get store locations for dropdown
        /// </summary>
        public async Task<List<StockAdjustmentSupportDto.LocationDto>> GetStoreLocationsAsync()
        {
            return await _context.ShopLocations
                .Where(l => l.Status == "Active")
                .OrderBy(l => l.LocationName)
                .Select(l => new StockAdjustmentSupportDto.LocationDto
                {
                    Id = l.Id,
                    Name = l.LocationName,
                    Type = l.LocationType
                })
                .ToListAsync();
        }

        /// <summary>
        /// Generate unique adjustment number
        /// </summary>
        public async Task<string> GenerateAdjustmentNumberAsync()
        {
            Console.WriteLine("[StockAdjustmentService] Generating adjustment number...");
            
            var today = DateTime.Now;
            var prefix = $"ADJ{today:yyyyMM}";
            Console.WriteLine($"[StockAdjustmentService] Using prefix: {prefix}");
            
            Console.WriteLine("[StockAdjustmentService] Looking for last adjustment with this prefix...");
            var lastAdjustment = await _context.StockAdjustments
                .Where(s => s.AdjustmentNo.StartsWith(prefix))
                .OrderByDescending(s => s.AdjustmentNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastAdjustment != null)
            {
                Console.WriteLine($"[StockAdjustmentService] Found last adjustment: {lastAdjustment.AdjustmentNo}");
                var lastNumberPart = lastAdjustment.AdjustmentNo.Substring(prefix.Length);
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                    Console.WriteLine($"[StockAdjustmentService] Next number will be: {nextNumber}");
                }
                else
                {
                    Console.WriteLine($"[StockAdjustmentService] Could not parse number from: {lastNumberPart}, using 1");
                }
            }
            else
            {
                Console.WriteLine("[StockAdjustmentService] No previous adjustments found, starting with 1");
            }

            var adjustmentNumber = $"{prefix}{nextNumber:D4}";
            Console.WriteLine($"[StockAdjustmentService] Generated adjustment number: {adjustmentNumber}");
            return adjustmentNumber;
        }

        /// <summary>
        /// Ensures that required foreign key records exist in the database
        /// </summary>
        private async Task EnsureRequiredDataExistsAsync(CreateStockAdjustmentDto createDto)
        {
            FileLogger.LogSeparator("Foreign Key Validation");
            FileLogger.LogInfo("Checking foreign key constraints...");
            
            // Check and create default UOM if it doesn't exist
            var defaultUom = await _context.UnitsOfMeasurement.FirstOrDefaultAsync(u => u.Id == 1);
            if (defaultUom == null)
            {
                FileLogger.LogWarning("Default UOM (ID: 1) not found - creating...");
                defaultUom = new Domain.Entities.UnitOfMeasurement
                {
                    Id = 1,
                    Name = "Pieces",
                    Abbreviation = "PCS",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                _context.UnitsOfMeasurement.Add(defaultUom);
                await _context.SaveChangesAsync();
                FileLogger.LogInfo("Default UOM created successfully");
            }
            else
            {
                FileLogger.LogInfo("Default UOM (ID: 1) exists");
            }
            
            // Check and create default store location if it doesn't exist
            var defaultStore = await _context.ShopLocations.FirstOrDefaultAsync(s => s.Id == 1);
            if (defaultStore == null)
            {
                FileLogger.LogWarning("Default store location (ID: 1) not found - creating...");
                defaultStore = new Domain.Entities.ShopLocation
                {
                    Id = 1,
                    ShopId = 1,
                    LocationType = "Retail",
                    LocationName = "Main Store",
                    AddressLine1 = "Default Location",
                    Status = "Active",
                    CanSell = true,
                    CreatedAt = DateTime.Now
                };
                _context.ShopLocations.Add(defaultStore);
                await _context.SaveChangesAsync();
                FileLogger.LogInfo("Default store location created successfully");
            }
            else
            {
                FileLogger.LogInfo("Default store location (ID: 1) exists");
            }
            
            // Check and create default user if it doesn't exist (for CreatedBy field)
            var defaultUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == 1);
            if (defaultUser == null)
            {
                FileLogger.LogWarning("Default user (ID: 1) not found - creating...");
                defaultUser = new Domain.Entities.User
                {
                    Id = 1,
                    FullName = "System User",
                    Email = "system@chronopos.com",
                    Password = "system123", // Default password
                    Role = "System",
                    RolePermissionId = 1,
                    ShopId = 1,
                    CreatedAt = DateTime.Now
                };
                _context.Users.Add(defaultUser);
                await _context.SaveChangesAsync();
                FileLogger.LogInfo("Default user created successfully");
            }
            else
            {
                FileLogger.LogInfo("Default user (ID: 1) exists");
            }
            
            FileLogger.LogInfo("All required foreign key records validated/created");
        }

        /// <summary>
        /// Search for products and product units for stock adjustment
        /// </summary>
        public async Task<List<StockAdjustmentSearchItemDto>> SearchForStockAdjustmentAsync(
            string searchTerm,
            StockAdjustmentMode mode,
            int maxResults = 50)
        {
            var results = new List<StockAdjustmentSearchItemDto>();

            if (string.IsNullOrWhiteSpace(searchTerm))
                return results;

            var searchLower = searchTerm.ToLower();

            try
            {
                if (mode == StockAdjustmentMode.Product)
                {
                    // Search for products only
                    var products = await _context.Products
                        .Where(p => p.Name.ToLower().Contains(searchLower) || 
                                   p.Code.ToLower().Contains(searchLower))
                        .Take(maxResults)
                        .Select(p => new StockAdjustmentSearchItemDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            DisplayName = p.Name,
                            Mode = StockAdjustmentMode.Product,
                            CurrentQuantity = p.InitialStock,
                            ProductId = p.Id,
                            ImagePath = p.ImagePath
                        })
                        .ToListAsync();

                    results.AddRange(products);
                }
                else if (mode == StockAdjustmentMode.ProductUnit)
                {
                    // Search for product units with conversion factors
                    var productUnits = await _context.ProductUnits
                        .Include(pu => pu.Product)
                        .Include(pu => pu.Unit)
                        .Where(pu => pu.Product.Name.ToLower().Contains(searchLower) ||
                                    pu.Product.Code.ToLower().Contains(searchLower) ||
                                    (pu.Unit.Name != null && pu.Unit.Name.ToLower().Contains(searchLower)))
                        .Take(maxResults)
                        .Select(pu => new StockAdjustmentSearchItemDto
                        {
                            Id = pu.Id,
                            Name = pu.Product.Name,
                            DisplayName = $"{pu.Product.Name} - {pu.Unit.Name} ({pu.QtyInUnit})",
                            Mode = StockAdjustmentMode.ProductUnit,
                            CurrentQuantity = pu.QtyInUnit,
                            ProductId = pu.ProductId,
                            ProductUnitId = pu.Id,
                            UnitId = pu.UnitId,
                            ConversionFactor = pu.Unit.ConversionFactor,
                            QtyInUnit = pu.QtyInUnit,
                            ImagePath = pu.Product.ImagePath,
                            UnitName = pu.Unit.Name,
                            UnitAbbreviation = pu.Unit.Abbreviation
                        })
                        .ToListAsync();

                    results.AddRange(productUnits);
                }

                FileLogger.LogInfo($"[StockAdjustmentService] Search returned {results.Count} results for mode: {mode}, term: '{searchTerm}'");
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"[StockAdjustmentService] Error in SearchForStockAdjustmentAsync: {ex.Message}", ex);
            }

            return results;
        }
    }
}
