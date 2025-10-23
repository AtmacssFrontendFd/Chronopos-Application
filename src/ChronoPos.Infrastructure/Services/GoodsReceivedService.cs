using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using ChronoPos.Application.Logging;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Service implementation for GoodsReceived operations with full stock management
/// </summary>
public class GoodsReceivedService : IGoodsReceivedService
{
    private readonly IGoodsReceivedRepository _repository;
    private readonly IGoodsReceivedItemRepository _itemRepository;
    private readonly IProductBatchService? _productBatchService;
    private readonly IProductService? _productService;
    private readonly IProductRepository? _productRepository;

    public GoodsReceivedService(
        IGoodsReceivedRepository repository,
        IGoodsReceivedItemRepository itemRepository,
        IProductBatchService? productBatchService = null,
        IProductService? productService = null,
        IProductRepository? productRepository = null)
    {
        _repository = repository;
        _itemRepository = itemRepository;
        _productBatchService = productBatchService;
        _productService = productService;
        _productRepository = productRepository;

        // Log dependency injection status
        AppLogger.LogInfo($"GoodsReceivedService created with dependencies", 
            $"ProductBatchService: {(productBatchService != null ? "✓" : "✗")}, " +
            $"ProductService: {(productService != null ? "✓" : "✗")}, " +
            $"ProductRepository: {(productRepository != null ? "✓" : "✗")}", "grn_service_init");
    }

    public async Task<GoodsReceivedDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<GoodsReceivedDto?> GetByGrnNoAsync(string grnNo)
    {
        var entity = await _repository.GetByGrnNoAsync(grnNo);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<GoodsReceivedDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<GoodsReceivedDto>> GetBySupplierIdAsync(int supplierId)
    {
        var entities = await _repository.GetBySupplierIdAsync(supplierId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<GoodsReceivedDto>> GetByStoreIdAsync(int storeId)
    {
        var entities = await _repository.GetByStoreIdAsync(storeId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<GoodsReceivedDto>> GetByStatusAsync(string status)
    {
        var entities = await _repository.GetByStatusAsync(status);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<GoodsReceivedDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var entities = await _repository.GetByDateRangeAsync(startDate, endDate);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<GoodsReceivedDto>> SearchAsync(string searchTerm)
    {
        var entities = await _repository.SearchAsync(searchTerm);
        return entities.Select(MapToDto);
    }

    public async Task<GoodsReceivedDto> CreateAsync(CreateGoodsReceivedDto createDto)
    {
        AppLogger.LogInfo($"Starting GRN creation", 
            $"GRN No: {createDto.GrnNo}, Items count: {createDto.Items?.Count ?? 0}", "grn_creation");

        // Validate GRN number uniqueness
        if (await _repository.GrnNoExistsAsync(createDto.GrnNo))
        {
            AppLogger.LogError($"GRN number already exists", 
                null, $"GRN No: {createDto.GrnNo}", "grn_creation");
            throw new ArgumentException($"GRN number '{createDto.GrnNo}' already exists.");
        }

        AppLogger.LogDebug($"Mapping DTO to entity", 
            $"GRN No: {createDto.GrnNo}, Items to map: {createDto.Items?.Count ?? 0}", "grn_creation");

        var entity = MapFromCreateDto(createDto);
        entity.CreatedAt = DateTime.UtcNow;
        
        AppLogger.LogInfo($"Entity mapped successfully", 
            $"GRN No: {entity.GrnNo}, Items mapped: {entity.Items?.Count ?? 0}", "grn_creation");

        AppLogger.LogDebug($"Saving GRN to database", 
            $"GRN No: {entity.GrnNo}", "grn_creation");
        
        var createdEntity = await _repository.AddAsync(entity);
        
        AppLogger.LogInfo($"GRN created successfully in database", 
            $"GRN ID: {createdEntity.Id}, GRN No: {createdEntity.GrnNo}, Items saved: {createdEntity.Items?.Count ?? 0}", "grn_creation");
        
        return MapToDto(createdEntity);
    }

    public async Task<GoodsReceivedDto> UpdateAsync(UpdateGoodsReceivedDto updateDto)
    {
        AppLogger.LogInfo($"Starting GRN update", 
            $"GRN ID: {updateDto.Id}, GRN No: {updateDto.GrnNo}, Items count: {updateDto.Items?.Count ?? 0}", "grn_update");

        // Check if exists
        if (!await _repository.ExistsAsync(updateDto.Id))
        {
            throw new ArgumentException($"Goods received with ID {updateDto.Id} not found.");
        }

        // Validate GRN number uniqueness (excluding current record)
        if (await _repository.GrnNoExistsAsync(updateDto.GrnNo, updateDto.Id))
        {
            throw new ArgumentException($"GRN number '{updateDto.GrnNo}' already exists.");
        }

        try
        {
            // Update the main GRN entity
            var entity = MapFromUpdateDto(updateDto);
            var updatedEntity = await _repository.UpdateAsync(entity);

            AppLogger.LogInfo($"GRN entity updated, now updating items", 
                $"GRN ID: {updateDto.Id}, New items count: {updateDto.Items?.Count ?? 0}", "grn_update");

            // Update items: Delete existing items and add new ones
            // This is simpler and safer than trying to match existing items
            await _itemRepository.DeleteByGrnIdAsync(updateDto.Id);

            if (updateDto.Items?.Any() == true)
            {
                foreach (var itemDto in updateDto.Items)
                {
                    var itemEntity = new GoodsReceivedItem
                    {
                        GrnId = updateDto.Id,
                        ProductId = itemDto.ProductId,
                        BatchId = itemDto.BatchId,
                        BatchNo = itemDto.BatchNo,
                        ManufactureDate = itemDto.ManufactureDate,
                        ExpiryDate = itemDto.ExpiryDate,
                        Quantity = itemDto.Quantity,
                        UomId = itemDto.UomId,
                        CostPrice = itemDto.CostPrice,
                        LandedCost = itemDto.LandedCost,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _itemRepository.AddAsync(itemEntity);
                }

                AppLogger.LogInfo($"GRN items updated successfully", 
                    $"GRN ID: {updateDto.Id}, Items added: {updateDto.Items.Count}", "grn_update");
            }

            // Reload the updated entity with items to return complete DTO
            var reloadedEntity = await _repository.GetByIdAsync(updateDto.Id);
            if (reloadedEntity == null)
            {
                throw new InvalidOperationException($"Failed to reload updated GRN with ID {updateDto.Id}");
            }
            
            return MapToDto(reloadedEntity);
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to update GRN", ex, 
                $"GRN ID: {updateDto.Id}, GRN No: {updateDto.GrnNo}", "grn_update");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
        {
            return false;
        }

        // Delete related items first
        await _itemRepository.DeleteByGrnIdAsync(id);
        
        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _repository.ExistsAsync(id);
    }

    public async Task<bool> GrnNoExistsAsync(string grnNo, int? excludeId = null)
    {
        return await _repository.GrnNoExistsAsync(grnNo, excludeId);
    }

    public async Task<string> GenerateGrnNoAsync()
    {
        return await _repository.GenerateGrnNoAsync();
    }

    public async Task<bool> PostGrnAsync(int id)
    {
        AppLogger.LogSeparator($"POST GRN ASYNC - ID: {id}", "grn_posting");
        AppLogger.LogInfo($"Starting GRN posting process", $"GRN ID: {id}", "grn_posting");
        
        // Check if required services are available
        AppLogger.LogInfo($"Service availability check", 
            $"ProductBatchService: {(_productBatchService != null ? "Available" : "NULL")}, ProductService: {(_productService != null ? "Available" : "NULL")}", "grn_posting");
        
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            AppLogger.LogError($"GRN not found for posting", null, $"GRN ID: {id}", "grn_posting");
            return false;
        }
        
        AppLogger.LogInfo($"GRN found", 
            $"GRN No: {entity.GrnNo}, Status: {entity.Status}, Items Count: {entity.Items?.Count ?? 0}", "grn_posting");
        
        if (entity.Status != "Pending" && entity.Status != "Draft")
        {
            AppLogger.LogWarning($"GRN cannot be posted - invalid status", 
                $"GRN No: {entity.GrnNo}, Current Status: {entity.Status}, Expected: Pending or Draft", "grn_posting");
            return false;
        }

        string originalStatus = entity.Status;
        var items = entity.Items ?? new List<GoodsReceivedItem>();
        
        if (items.Count == 0)
        {
            AppLogger.LogWarning($"GRN has no items to process", 
                $"GRN No: {entity.GrnNo}", "grn_posting");
            return false;
        }
        
        // Start database transaction for atomicity
        // Note: We'll implement a logical transaction since we don't have direct DbContext access
        var createdBatches = new List<int>(); // Track created batch IDs for rollback
        var updatedProducts = new List<(int ProductId, decimal OldStock, decimal OldInitial)>(); // Track stock changes for rollback
        
        try
        {
            AppLogger.LogInfo($"Starting transactional GRN posting", 
                $"GRN No: {entity.GrnNo}, Original Status: {originalStatus}, Items to process: {items.Count}", "grn_posting");
            
            // PHASE 1: Create ProductBatches for ALL items first (before any GRN status change)
            AppLogger.LogInfo($"PHASE 1: Creating ProductBatches for all items", 
                $"GRN No: {entity.GrnNo}, Items count: {items.Count}", "grn_posting");
            
            foreach (var item in items)
            {
                try
                {
                    AppLogger.LogInfo($"Creating ProductBatch for item", 
                        $"Item ID: {item.Id}, Product ID: {item.ProductId}, Quantity: {item.Quantity}, BatchNo: '{item.BatchNo}'", "grn_posting");
                    
                    var batchId = await CreateProductBatchForItem(item);
                    if (batchId.HasValue)
                    {
                        createdBatches.Add(batchId.Value);
                        AppLogger.LogInfo($"ProductBatch created successfully", 
                            $"Batch ID: {batchId.Value}, Item ID: {item.Id}", "grn_posting");
                    }
                }
                catch (Exception batchEx)
                {
                    AppLogger.LogError($"Failed to create ProductBatch for item", batchEx, 
                        $"Item ID: {item.Id}, Product ID: {item.ProductId}", "grn_posting");
                    throw; // This will trigger rollback
                }
            }
            
            AppLogger.LogInfo($"PHASE 1 completed: All ProductBatches created", 
                $"GRN No: {entity.GrnNo}, Created batches: {createdBatches.Count}", "grn_posting");
            
            // PHASE 2: Update Product stock quantities for ALL items
            AppLogger.LogInfo($"PHASE 2: Updating Product stock quantities for all items", 
                $"GRN No: {entity.GrnNo}", "grn_posting");
            
            foreach (var item in items)
            {
                try
                {
                    AppLogger.LogInfo($"Updating stock for product", 
                        $"Product ID: {item.ProductId}, Quantity to add: {item.Quantity}", "grn_posting");
                    
                    var stockInfo = await UpdateProductStockQuantitiesWithTracking(item.ProductId, item.Quantity);
                    updatedProducts.Add(stockInfo);
                    
                    AppLogger.LogInfo($"Product stock updated successfully", 
                        $"Product ID: {item.ProductId}, New stock: {stockInfo.OldStock + item.Quantity}", "grn_posting");
                }
                catch (Exception stockEx)
                {
                    AppLogger.LogError($"Failed to update stock for product", stockEx, 
                        $"Product ID: {item.ProductId}, Quantity: {item.Quantity}", "grn_posting");
                    throw; // This will trigger rollback
                }
            }
            
            AppLogger.LogInfo($"PHASE 2 completed: All Product stocks updated", 
                $"GRN No: {entity.GrnNo}, Updated products: {updatedProducts.Count}", "grn_posting");
            
            // PHASE 3: ONLY NOW update GRN status to Posted (after everything else succeeds)
            AppLogger.LogInfo($"PHASE 3: Updating GRN status to Posted (final step)", 
                $"GRN No: {entity.GrnNo}", "grn_posting");
            
            entity.Status = "Posted";
            await _repository.UpdateAsync(entity);
            
            AppLogger.LogInfo($"GRN posting completed successfully", 
                $"GRN No: {entity.GrnNo}, Created batches: {createdBatches.Count}, Updated products: {updatedProducts.Count}", "grn_posting");
            
            return true;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"GRN posting failed - initiating rollback", ex, 
                $"GRN No: {entity.GrnNo}, Created batches: {createdBatches.Count}, Updated products: {updatedProducts.Count}", "grn_posting");
            
            // ROLLBACK: Clean up in reverse order
            await RollbackGrnPosting(entity, createdBatches, updatedProducts, originalStatus);
            
            throw;
        }
        finally
        {
            AppLogger.LogSeparator("POST GRN ASYNC COMPLETE", "grn_posting");
        }
    }
    
    /// <summary>
    /// Process individual GRN item to create product batch and update stock quantities
    /// </summary>
    private async Task ProcessGrnItemForStockUpdate(GoodsReceivedItem item)
    {
        AppLogger.LogSeparator($"PROCESSING GRN ITEM - ID: {item.Id}", "grn_item_processing");
        AppLogger.LogInfo($"Starting GRN item processing", 
            $"Item ID: {item.Id}, Product ID: {item.ProductId}, Quantity: {item.Quantity}, Cost: ₹{item.CostPrice}, BatchNo: '{item.BatchNo}'", "grn_item_processing");
        
        try
        {
            // 1. Create or update Product Batch
            AppLogger.LogInfo($"Step 1: Creating/updating product batch", 
                $"Item ID: {item.Id}, Product ID: {item.ProductId}", "grn_item_processing");
            
            await CreateOrUpdateProductBatch(item);
            
            AppLogger.LogInfo($"Step 1 completed: Product batch creation/update", 
                $"Item ID: {item.Id}, Product ID: {item.ProductId}", "grn_item_processing");
            
            // 2. Update Product Stock Quantities
            AppLogger.LogInfo($"Step 2: Updating product stock quantities", 
                $"Item ID: {item.Id}, Product ID: {item.ProductId}, Quantity to add: {item.Quantity}", "grn_item_processing");
            
            await UpdateProductStockQuantities(item.ProductId, item.Quantity);
            
            AppLogger.LogInfo($"Step 2 completed: Product stock quantities updated", 
                $"Item ID: {item.Id}, Product ID: {item.ProductId}", "grn_item_processing");
            
            AppLogger.LogInfo($"GRN item processing completed successfully", 
                $"Item ID: {item.Id}, Product ID: {item.ProductId}", "grn_item_processing");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Failed to process GRN item", ex, 
                $"Item ID: {item.Id}, Product ID: {item.ProductId}, Quantity: {item.Quantity}", "grn_item_processing");
            throw;
        }
        finally
        {
            AppLogger.LogSeparator("GRN ITEM PROCESSING COMPLETE", "grn_item_processing");
        }
    }
    
    /// <summary>
    /// Create product batch for GRN item and return the created batch ID
    /// </summary>
    private async Task<int?> CreateProductBatchForItem(GoodsReceivedItem item)
    {
        AppLogger.LogInfo($"Creating ProductBatch for GRN item", 
            $"Item ID: {item.Id}, Product ID: {item.ProductId}, Quantity: {item.Quantity}", "product_batch_creation");
        
        if (_productBatchService == null)
        {
            AppLogger.LogError($"ProductBatchService not available", 
                null, $"Item ID: {item.Id}, Product ID: {item.ProductId}", "product_batch_creation");
            throw new InvalidOperationException($"ProductBatchService not available for batch creation");
        }

        try
        {
            // Generate batch number if not provided
            string batchNo = item.BatchNo ?? $"BATCH-{DateTime.Now:yyyyMMdd}-{item.Id}";
            
            AppLogger.LogDebug($"Creating ProductBatch DTO", 
                $"Product ID: {item.ProductId}, BatchNo: '{batchNo}', Quantity: {item.Quantity}", "product_batch_creation");
            
            // Create new product batch
            var createBatchDto = new CreateProductBatchDto
            {
                ProductId = item.ProductId,
                BatchNo = batchNo,
                ManufactureDate = item.ManufactureDate ?? DateTime.Today,
                ExpiryDate = item.ExpiryDate ?? DateTime.Today.AddYears(1),
                Quantity = item.Quantity,
                UomId = (int)item.UomId,
                CostPrice = item.CostPrice,
                LandedCost = item.LandedCost ?? item.CostPrice,
                Status = "Active"
            };
            
            var createdBatch = await _productBatchService.CreateProductBatchAsync(createBatchDto);
            
            AppLogger.LogInfo($"ProductBatch created successfully", 
                $"Batch ID: {createdBatch.Id}, BatchNo: '{createdBatch.BatchNo}', Product ID: {createdBatch.ProductId}", "product_batch_creation");
            
            return createdBatch.Id;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Critical error creating ProductBatch", ex, 
                $"Item ID: {item.Id}, Product ID: {item.ProductId}", "product_batch_creation");
            throw new InvalidOperationException($"Failed to create ProductBatch for item {item.Id}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Create or update product batch from GRN item
    /// </summary>
    private async Task CreateOrUpdateProductBatch(GoodsReceivedItem item)
    {
        AppLogger.LogSeparator($"PRODUCT BATCH CREATION - Item ID: {item.Id}", "product_batch");
        AppLogger.LogInfo($"Starting product batch creation/update", 
            $"Item ID: {item.Id}, Product ID: {item.ProductId}, Existing BatchId: {item.BatchId}", "product_batch");
        
        if (_productBatchService == null)
        {
            AppLogger.LogWarning($"ProductBatchService not available - skipping batch creation", 
                $"Item ID: {item.Id}, Product ID: {item.ProductId}", "product_batch");
            return;
        }

        try
        {
            // Check if we should update existing batch or create new one
            if (item.BatchId.HasValue)
            {
                AppLogger.LogInfo($"Updating existing product batch", 
                    $"BatchId: {item.BatchId.Value}, Quantity to add: {item.Quantity}", "product_batch");
                
                // Update existing batch quantity
                await _productBatchService.UpdateBatchQuantityAsync(item.BatchId.Value, item.Quantity);
                
                AppLogger.LogInfo($"Existing product batch updated successfully", 
                    $"BatchId: {item.BatchId.Value}, Updated Quantity: {item.Quantity}", "product_batch");
            }
            else
            {
                // Generate batch number if not provided
                string batchNo = item.BatchNo ?? $"BATCH-{DateTime.Now:yyyyMMdd}-{item.Id}";
                
                AppLogger.LogInfo($"Creating new product batch", 
                    $"Product ID: {item.ProductId}, BatchNo: '{batchNo}', Quantity: {item.Quantity}, Cost: ₹{item.CostPrice}", "product_batch");
                
                // Create new product batch
                var createBatchDto = new CreateProductBatchDto
                {
                    ProductId = item.ProductId,
                    BatchNo = batchNo,
                    ManufactureDate = item.ManufactureDate ?? DateTime.Today,
                    ExpiryDate = item.ExpiryDate ?? DateTime.Today.AddYears(1),
                    Quantity = item.Quantity,
                    UomId = (int)item.UomId, // Cast to int if needed
                    CostPrice = item.CostPrice,
                    LandedCost = item.LandedCost ?? item.CostPrice,
                    Status = "Active"
                };
                
                AppLogger.LogDebug($"Product batch DTO created", 
                    $"ProductId: {createBatchDto.ProductId}, BatchNo: '{createBatchDto.BatchNo}', MfgDate: {createBatchDto.ManufactureDate:yyyy-MM-dd}, ExpDate: {createBatchDto.ExpiryDate:yyyy-MM-dd}, Qty: {createBatchDto.Quantity}, UomId: {createBatchDto.UomId}, Cost: ₹{createBatchDto.CostPrice}, LandedCost: ₹{createBatchDto.LandedCost}", "product_batch");
                
                var createdBatch = await _productBatchService.CreateProductBatchAsync(createBatchDto);
                
                AppLogger.LogInfo($"New product batch created successfully", 
                    $"BatchId: {createdBatch.Id}, BatchNo: '{createdBatch.BatchNo}', Product ID: {createdBatch.ProductId}, Quantity: {createdBatch.Quantity}", "product_batch");
                
                // Update GRN item with the created batch ID
                item.BatchId = createdBatch.Id;
                
                AppLogger.LogInfo($"GRN item updated with batch ID", 
                    $"Item ID: {item.Id}, AssignedBatchId: {createdBatch.Id}", "product_batch");
                
                // TODO: Update the item in repository to persist the BatchId
                // await _itemRepository.UpdateAsync(item);
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Critical error creating/updating product batch", ex, 
                $"Item ID: {item.Id}, Product ID: {item.ProductId}, BatchNo: '{item.BatchNo}', Quantity: {item.Quantity}", "product_batch");
            
            // Re-throw to fail the GRN posting if batch creation fails
            // This ensures data consistency
            throw new InvalidOperationException($"Failed to create product batch for item {item.Id}: {ex.Message}", ex);
        }
        finally
        {
            AppLogger.LogSeparator("PRODUCT BATCH CREATION COMPLETE", "product_batch");
        }
    }
    
    /// <summary>
    /// Update product stock quantities with tracking for rollback
    /// </summary>
    private async Task<(int ProductId, decimal OldStock, decimal OldInitial)> UpdateProductStockQuantitiesWithTracking(int productId, decimal quantityReceived)
    {
        AppLogger.LogInfo($"Updating product stock with rollback tracking", 
            $"Product ID: {productId}, Quantity to add: {quantityReceived}", "product_stock_tracking");
        
        if (_productService == null)
        {
            AppLogger.LogError($"ProductService not available", 
                null, $"Product ID: {productId}", "product_stock_tracking");
            throw new InvalidOperationException($"ProductService not available for stock update");
        }

        try
        {
            // Get current product
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                AppLogger.LogError($"Product not found for stock update", 
                    null, $"Product ID: {productId}", "product_stock_tracking");
                throw new InvalidOperationException($"Product with ID {productId} not found");
            }
            
            // Store original values for rollback
            var originalStockQuantity = product.StockQuantity;
            var originalInitialStock = product.InitialStock;
            
            AppLogger.LogInfo($"Original stock values", 
                $"Product ID: {productId}, Stock Qty: {originalStockQuantity}, Initial Stock: {originalInitialStock}", "product_stock_tracking");
            
            // Update stock quantities
            product.StockQuantity += (int)quantityReceived;
            product.InitialStock += quantityReceived;
            
            // Save updated product directly via repository (bypasses ProductUnit validation)
            AppLogger.LogInfo($"Repository bypass attempt", 
                $"ProductRepository available: {(_productRepository != null ? "✓" : "✗")}", "product_stock_tracking");
                
            if (_productRepository != null)
            {
                // Get the product entity directly
                var productEntity = await _productRepository.GetByIdAsync(productId);
                if (productEntity != null)
                {
                    // Update stock quantities directly on entity
                    productEntity.StockQuantity = product.StockQuantity;
                    productEntity.InitialStock = product.InitialStock;
                    productEntity.UpdatedAt = DateTime.Now;
                    
                    // Save directly through repository (bypasses service validation)
                    await _productRepository.UpdateAsync(productEntity);
                    
                    AppLogger.LogInfo($"Product stock saved via repository bypass", 
                        $"Product ID: {productId}, Bypassed ProductService validation", "product_stock_tracking");
                }
                else
                {
                    throw new InvalidOperationException($"Product entity not found for ID {productId}");
                }
            }
            else
            {
                AppLogger.LogWarning($"ProductRepository not available, falling back to ProductService", 
                    $"Product ID: {productId}", "product_stock_tracking");
                await _productService.UpdateProductAsync(product);
            }
            
            AppLogger.LogInfo($"Product stock updated successfully", 
                $"Product ID: {productId}, New Stock Qty: {product.StockQuantity}, New Initial Stock: {product.InitialStock}", "product_stock_tracking");
            
            return (productId, originalStockQuantity, originalInitialStock);
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error updating product stock with tracking", ex, 
                $"Product ID: {productId}, Quantity: {quantityReceived}", "product_stock_tracking");
            throw;
        }
    }

    /// <summary>
    /// Update product stock quantities (StockQuantity and InitialStock)
    /// </summary>
    private async Task UpdateProductStockQuantities(int productId, decimal quantityReceived)
    {
        AppLogger.LogInfo($"Starting stock quantity update for product", 
            $"Product ID: {productId}, Quantity to add: {quantityReceived}", "product_stock");
        
        if (_productService == null)
        {
            AppLogger.LogError($"ProductService not available - cannot update stock", 
                null, $"Product ID: {productId}, Quantity to add: {quantityReceived}", "product_stock");
            throw new InvalidOperationException($"ProductService not available for stock update of product {productId}");
        }

        try
        {
            AppLogger.LogDebug($"Fetching current product details", 
                $"Product ID: {productId}", "product_stock");
            
            // 1. Get current product
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                AppLogger.LogError($"Product not found for stock update", 
                    null, $"Product ID: {productId}, Quantity to add: {quantityReceived}", "product_stock");
                throw new InvalidOperationException($"Product with ID {productId} not found");
            }
            
            // Store original values for logging
            var originalStockQuantity = product.StockQuantity;
            var originalInitialStock = product.InitialStock;
            
            AppLogger.LogInfo($"Current product stock details", 
                $"Product ID: {productId}, Name: '{product.Name}', Current Stock Qty: {originalStockQuantity}, Current Initial Stock: {originalInitialStock}", "product_stock");
            
            // 2. Update stock quantities - ADD the received quantity to existing stock
            product.StockQuantity += (int)quantityReceived; // Add to current stock
            product.InitialStock += quantityReceived;       // Add to initial stock as well
            
            AppLogger.LogInfo($"Calculated new stock quantities", 
                $"Product ID: {productId}, New Stock Qty: {product.StockQuantity} (was {originalStockQuantity}), New Initial Stock: {product.InitialStock} (was {originalInitialStock}), Added: {quantityReceived}", "product_stock");
            
            // 3. Save updated product
            AppLogger.LogDebug($"Saving updated product to database", 
                $"Product ID: {productId}", "product_stock");
            
            await _productService.UpdateProductAsync(product);
            
            AppLogger.LogInfo($"Product stock quantities updated successfully", 
                $"Product ID: {productId}, Name: '{product.Name}', Final Stock Qty: {product.StockQuantity}, Final Initial Stock: {product.InitialStock}, Quantity Added: {quantityReceived}", "product_stock");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"CRITICAL ERROR updating stock quantities for product", ex, 
                $"Product ID: {productId}, Quantity to add: {quantityReceived}", "product_stock");
            
            // This is a critical operation - fail the GRN posting if stock update fails
            throw new InvalidOperationException($"Failed to update stock quantities for product {productId}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Rollback GRN posting by cleaning up created batches and restoring product stocks
    /// </summary>
    private async Task RollbackGrnPosting(
        GoodsReceived entity, 
        List<int> createdBatchIds, 
        List<(int ProductId, decimal OldStock, decimal OldInitial)> updatedProducts, 
        string originalStatus)
    {
        AppLogger.LogSeparator($"ROLLBACK GRN POSTING - {entity.GrnNo}", "grn_rollback");
        AppLogger.LogInfo($"Starting GRN posting rollback", 
            $"GRN No: {entity.GrnNo}, Batches to delete: {createdBatchIds.Count}, Products to restore: {updatedProducts.Count}", "grn_rollback");

        int rollbackErrors = 0;

        // 1. Delete created ProductBatches
        if (createdBatchIds.Any() && _productBatchService != null)
        {
            AppLogger.LogInfo($"Rolling back created ProductBatches", 
                $"Batch IDs to delete: [{string.Join(", ", createdBatchIds)}]", "grn_rollback");
            
            foreach (var batchId in createdBatchIds)
            {
                try
                {
                    var deleted = await _productBatchService.DeleteProductBatchAsync(batchId);
                    if (deleted)
                    {
                        AppLogger.LogInfo($"ProductBatch deleted during rollback", 
                            $"Batch ID: {batchId}", "grn_rollback");
                    }
                    else
                    {
                        AppLogger.LogWarning($"Failed to delete ProductBatch during rollback", 
                            $"Batch ID: {batchId}", "grn_rollback");
                        rollbackErrors++;
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"Error deleting ProductBatch during rollback", ex, 
                        $"Batch ID: {batchId}", "grn_rollback");
                    rollbackErrors++;
                }
            }
        }

        // 2. Restore original Product stock quantities
        if (updatedProducts.Any() && _productService != null)
        {
            AppLogger.LogInfo($"Rolling back Product stock quantities", 
                $"Products to restore: {updatedProducts.Count}", "grn_rollback");
            
            foreach (var (productId, oldStock, oldInitial) in updatedProducts)
            {
                try
                {
                    var product = await _productService.GetProductByIdAsync(productId);
                    if (product != null)
                    {
                        AppLogger.LogInfo($"Restoring product stock quantities", 
                            $"Product ID: {productId}, Current Stock: {product.StockQuantity}, Restoring to: {oldStock}, Current Initial: {product.InitialStock}, Restoring to: {oldInitial}", "grn_rollback");
                        
                        product.StockQuantity = (int)oldStock;
                        product.InitialStock = oldInitial;
                        
                        // Use repository access to bypass ProductService validation
                        if (_productRepository != null)
                        {
                            var productEntity = await _productRepository.GetByIdAsync(productId);
                            if (productEntity != null)
                            {
                                productEntity.StockQuantity = (int)oldStock;
                                productEntity.InitialStock = oldInitial;
                                productEntity.UpdatedAt = DateTime.Now;
                                
                                await _productRepository.UpdateAsync(productEntity);
                                
                                AppLogger.LogInfo($"Product stock restored via repository", 
                                    $"Product ID: {productId}, Bypassed ProductService validation", "grn_rollback");
                            }
                        }
                        else
                        {
                            await _productService.UpdateProductAsync(product);
                        }
                        
                        AppLogger.LogInfo($"Product stock restored successfully", 
                            $"Product ID: {productId}, Restored Stock: {product.StockQuantity}, Restored Initial: {product.InitialStock}", "grn_rollback");
                    }
                    else
                    {
                        AppLogger.LogWarning($"Product not found during stock restoration", 
                            $"Product ID: {productId}", "grn_rollback");
                        rollbackErrors++;
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"Error restoring product stock during rollback", ex, 
                        $"Product ID: {productId}, Old Stock: {oldStock}, Old Initial: {oldInitial}", "grn_rollback");
                    rollbackErrors++;
                }
            }
        }

        // 3. Restore GRN status
        try
        {
            entity.Status = originalStatus;
            await _repository.UpdateAsync(entity);
            
            AppLogger.LogInfo($"GRN status restored successfully", 
                $"GRN No: {entity.GrnNo}, Status: {originalStatus}", "grn_rollback");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error restoring GRN status during rollback", ex, 
                $"GRN No: {entity.GrnNo}, Original Status: {originalStatus}", "grn_rollback");
            rollbackErrors++;
        }

        if (rollbackErrors > 0)
        {
            AppLogger.LogWarning($"Rollback completed with errors", 
                $"GRN No: {entity.GrnNo}, Rollback errors: {rollbackErrors}", "grn_rollback");
        }
        else
        {
            AppLogger.LogInfo($"Rollback completed successfully", 
                $"GRN No: {entity.GrnNo}", "grn_rollback");
        }
        
        AppLogger.LogSeparator("ROLLBACK GRN POSTING COMPLETE", "grn_rollback");
    }

    public async Task<bool> CancelGrnAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null || entity.Status == "Cancelled")
        {
            return false;
        }

        entity.Status = "Cancelled";
        await _repository.UpdateAsync(entity);
        return true;
    }

    public async Task<decimal> CalculateTotalAmountAsync(int grnId)
    {
        return await _itemRepository.GetTotalAmountByGrnIdAsync(grnId);
    }

    public async Task<int> GetCountAsync()
    {
        return await _repository.GetCountAsync();
    }

    public async Task<IEnumerable<GoodsReceivedDto>> GetPagedAsync(int page, int pageSize, string? searchTerm = null)
    {
        var skip = (page - 1) * pageSize;
        var entities = await _repository.GetPagedAsync(skip, pageSize, searchTerm);
        return entities.Select(MapToDto);
    }

    /// <summary>
    /// Maps GoodsReceived entity to DTO
    /// </summary>
    private static GoodsReceivedDto MapToDto(GoodsReceived entity)
    {
        return new GoodsReceivedDto
        {
            Id = entity.Id,
            GrnNo = entity.GrnNo,
            SupplierId = entity.SupplierId,
            SupplierName = entity.Supplier?.CompanyName ?? "",
            StoreId = entity.StoreId,
            StoreName = entity.Store?.Name ?? "",
            InvoiceNo = entity.InvoiceNo,
            InvoiceDate = entity.InvoiceDate,
            ReceivedDate = entity.ReceivedDate,
            TotalAmount = entity.TotalAmount,
            Remarks = entity.Remarks,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            Items = entity.Items?.Select(MapItemToDto).ToList() ?? new List<GoodsReceivedItemDto>()
        };
    }

    /// <summary>
    /// Maps GoodsReceivedItem entity to DTO
    /// </summary>
    private static GoodsReceivedItemDto MapItemToDto(GoodsReceivedItem entity)
    {
        return new GoodsReceivedItemDto
        {
            Id = entity.Id,
            GrnId = entity.GrnId,
            ProductId = entity.ProductId,
            ProductName = entity.Product?.Name ?? "",
            ProductCode = entity.Product?.Code ?? "",
            BatchId = entity.BatchId,
            BatchNo = entity.BatchNo,
            ManufactureDate = entity.ManufactureDate,
            ExpiryDate = entity.ExpiryDate,
            Quantity = entity.Quantity,
            UomId = entity.UomId,
            UomName = entity.UnitOfMeasurement?.Name ?? "",
            CostPrice = entity.CostPrice,
            LandedCost = entity.LandedCost,
            LineTotal = entity.LineTotal,
            CreatedAt = entity.CreatedAt
        };
    }

    /// <summary>
    /// Maps CreateGoodsReceivedDto to entity
    /// </summary>
    private static GoodsReceived MapFromCreateDto(CreateGoodsReceivedDto dto)
    {
        var entity = new GoodsReceived
        {
            GrnNo = dto.GrnNo,
            SupplierId = dto.SupplierId,
            StoreId = dto.StoreId,
            InvoiceNo = dto.InvoiceNo,
            InvoiceDate = dto.InvoiceDate,
            ReceivedDate = dto.ReceivedDate,
            TotalAmount = dto.TotalAmount,
            Remarks = dto.Remarks,
            Status = dto.Status
        };

        // Map GRN items if provided
        if (dto.Items != null && dto.Items.Any())
        {
            entity.Items = dto.Items.Select(itemDto => new GoodsReceivedItem
            {
                ProductId = itemDto.ProductId,
                BatchNo = itemDto.BatchNo,
                ManufactureDate = itemDto.ManufactureDate,
                ExpiryDate = itemDto.ExpiryDate,
                Quantity = itemDto.Quantity,
                UomId = itemDto.UomId,
                CostPrice = itemDto.CostPrice,
                LandedCost = itemDto.LandedCost,
                CreatedAt = DateTime.UtcNow
            }).ToList();
        }

        return entity;
    }

    /// <summary>
    /// Maps UpdateGoodsReceivedDto to entity
    /// </summary>
    private static GoodsReceived MapFromUpdateDto(UpdateGoodsReceivedDto dto)
    {
        return new GoodsReceived
        {
            Id = dto.Id,
            GrnNo = dto.GrnNo,
            SupplierId = dto.SupplierId,
            StoreId = dto.StoreId,
            InvoiceNo = dto.InvoiceNo,
            InvoiceDate = dto.InvoiceDate,
            ReceivedDate = dto.ReceivedDate,
            TotalAmount = dto.TotalAmount,
            Remarks = dto.Remarks,
            Status = dto.Status
        };
    }
}