using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service for managing stock operations
/// </summary>
public class StockService : IStockService
{
    private readonly IChronoPosDbContext _context;

    public StockService(IChronoPosDbContext context)
    {
        _context = context;
    }

    #region Stock Level Operations

    public async Task<StockSaveResult> CreateStockLevelAsync(int productId, int storeId, decimal currentStock, decimal averageCost)
    {
        var result = new StockSaveResult();
        
        try
        {
            var existingLevel = await _context.StockLevels
                .FirstOrDefaultAsync(sl => sl.ProductId == productId && sl.StoreId == storeId);
                
            if (existingLevel != null)
            {
                result.Errors.Add("Stock level already exists for this product and store");
                return result;
            }
            
            var stockLevel = new StockLevel
            {
                ProductId = productId,
                StoreId = storeId,
                CurrentStock = currentStock,
                ReservedStock = 0,
                AverageCost = averageCost,
                LastCost = averageCost,
                LastUpdated = DateTime.UtcNow
            };
            
            _context.StockLevels.Add(stockLevel);
            await _context.SaveChangesAsync();
            
            result.Success = true;
            result.CreatedStockLevelId = stockLevel.Id;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error creating stock level: {ex.Message}");
        }
        
        return result;
    }

    public async Task<StockSaveResult> CreateStockMovementAsync(int productId, int storeId, StockDirection direction, decimal quantity, decimal unitCost, string referenceType, string? referenceNumber = null, string? notes = null, string createdBy = "System")
    {
        var result = new StockSaveResult();
        
        try
        {
            var stockLevel = await _context.StockLevels
                .FirstOrDefaultAsync(sl => sl.ProductId == productId && sl.StoreId == storeId);
                
            if (stockLevel == null)
            {
                result.Errors.Add("Stock level not found for this product and store");
                return result;
            }
            
            var previousStock = stockLevel.CurrentStock;
            var movement = new StockTransaction
            {
                ProductId = productId,
                StoreId = storeId,
                MovementType = direction,
                Quantity = quantity,
                UnitCost = unitCost,
                ReferenceType = referenceType,
                ReferenceNumber = referenceNumber,
                Notes = notes,
                PreviousStock = previousStock,
                NewStock = direction == StockDirection.IN ? previousStock + quantity : previousStock - quantity,
                CreatedBy = createdBy,
                Created = DateTime.UtcNow
            };
            
            _context.StockTransactions.Add(movement);
            await _context.SaveChangesAsync();
            
            result.Success = true;
            result.CreatedMovementId = movement.Id;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error creating stock movement: {ex.Message}");
        }
        
        return result;
    }

    public async Task<StockLevelDto?> GetStockLevelAsync(int productId, int storeId)
    {
        var stockLevel = await _context.StockLevels
            .Include(sl => sl.Store)
            .FirstOrDefaultAsync(sl => sl.ProductId == productId && sl.StoreId == storeId);
            
        if (stockLevel == null) return null;
        
        return new StockLevelDto
        {
            Id = stockLevel.Id,
            ProductId = stockLevel.ProductId,
            StoreId = stockLevel.StoreId,
            CurrentStock = stockLevel.CurrentStock,
            ReservedStock = stockLevel.ReservedStock,
            AverageCost = stockLevel.AverageCost,
            LastCost = stockLevel.LastCost,
            LastUpdated = stockLevel.LastUpdated,
            StoreName = stockLevel.Store?.Name ?? string.Empty
        };
    }

    public async Task<IEnumerable<StockLevelDto>> GetAllStockLevelsAsync(int productId)
    {
        var stockLevels = await _context.StockLevels
            .Include(sl => sl.Store)
            .Where(sl => sl.ProductId == productId)
            .ToListAsync();
            
        return stockLevels.Select(sl => new StockLevelDto
        {
            Id = sl.Id,
            ProductId = sl.ProductId,
            StoreId = sl.StoreId,
            CurrentStock = sl.CurrentStock,
            ReservedStock = sl.ReservedStock,
            AverageCost = sl.AverageCost,
            LastCost = sl.LastCost,
            LastUpdated = sl.LastUpdated,
            StoreName = sl.Store?.Name ?? string.Empty
        });
    }

    #endregion

    #region Stock Movement Operations

    public async Task<bool> AdjustStockAsync(int productId, int quantity, StockDirection transactionType, string? reference = null, string? notes = null)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsStockTracked) return false;

            var previousStock = product.StockQuantity;
            var newStock = previousStock + quantity;

            // Check for negative stock if not allowed
            if (newStock < 0 && !product.AllowNegativeStock)
            {
                throw new InvalidOperationException($"Insufficient stock. Available: {previousStock}, Requested: {Math.Abs(quantity)}");
            }

            // Update product stock - both StockQuantity and InitialStock
            product.StockQuantity = newStock;
            product.InitialStock = newStock; // Update InitialStock to match current stock
            product.UpdatedAt = DateTime.UtcNow;

            // Create transaction record
            var transaction = new StockTransaction
            {
                ProductId = productId,
                MovementType = transactionType,
                Quantity = quantity,
                PreviousStock = previousStock,
                NewStock = newStock,
                ReferenceNumber = reference,
                Notes = notes,
                Created = DateTime.UtcNow
            };

            _context.StockTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Check for alerts
            await CheckAndCreateAlertsAsync(productId);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ReceiveStockAsync(int productId, int quantity, decimal? unitCost = null, string? supplierName = null, string? batchNumber = null, DateTime? expiryDate = null)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsStockTracked) return false;

            var previousStock = product.StockQuantity;
            var newStock = previousStock + quantity;

            // Update product stock and cost if provided - both StockQuantity and InitialStock
            product.StockQuantity = newStock;
            product.InitialStock = newStock; // Update InitialStock to match current stock
            if (unitCost.HasValue && unitCost > 0)
            {
                product.LastPurchasePrice = unitCost.Value;
                product.Cost = unitCost.Value; // Update current cost
            }
            product.UpdatedAt = DateTime.UtcNow;

            // Create transaction record
            var transaction = new StockTransaction
            {
                ProductId = productId,
                MovementType = StockDirection.IN,
                Quantity = quantity,
                PreviousStock = previousStock,
                NewStock = newStock,
                UnitCost = unitCost ?? 0,
                SupplierName = supplierName,
                BatchNumber = batchNumber,
                ExpiryDate = expiryDate,
                Created = DateTime.UtcNow,
                Notes = $"Stock received from {supplierName ?? "supplier"}"
            };

            _context.StockTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Check for alerts (might resolve low stock alerts)
            await CheckAndCreateAlertsAsync(productId);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IssueStockAsync(int productId, int quantity, string? reference = null)
    {
        return await AdjustStockAsync(productId, -quantity, StockDirection.OUT, reference, $"Stock issued - {reference}");
    }

    public async Task<bool> TransferStockAsync(int fromProductId, int toProductId, int quantity, string? reference = null)
    {
        try
        {
            var issueSuccess = await AdjustStockAsync(fromProductId, -quantity, StockDirection.OUT, reference, $"Transfer out to product {toProductId}");
            if (!issueSuccess)
            {
                return false;
            }

            var receiveSuccess = await AdjustStockAsync(toProductId, quantity, StockDirection.IN, reference, $"Transfer in from product {fromProductId}");
            if (!receiveSuccess)
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Stock Queries

    public async Task<int> GetCurrentStockAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        return product?.StockQuantity ?? 0;
    }

    public async Task<IEnumerable<StockTransaction>> GetStockHistoryAsync(int productId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.StockTransactions
            .Where(st => st.ProductId == productId);

        if (fromDate.HasValue)
            query = query.Where(st => st.Created >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(st => st.Created <= toDate.Value);

        return await query
            .OrderByDescending(st => st.Created)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
    {
        return await _context.Products
            .Where(p => p.IsStockTracked && p.StockQuantity <= p.ReorderLevel && p.ReorderLevel > 0 && p.IsActive)
            .Include(p => p.Category)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetOutOfStockProductsAsync()
    {
        return await _context.Products
            .Where(p => p.IsStockTracked && p.StockQuantity <= 0 && p.IsActive)
            .Include(p => p.Category)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetOverstockProductsAsync()
    {
        return await _context.Products
            .Where(p => p.IsStockTracked && p.MaximumStock > 0 && p.StockQuantity >= p.MaximumStock && p.IsActive)
            .Include(p => p.Category)
            .ToListAsync();
    }

    #endregion

    #region Stock Alerts

    public async Task<IEnumerable<StockAlert>> GetActiveAlertsAsync()
    {
        return await _context.StockAlerts
            .Where(sa => sa.IsActive && !sa.IsRead)
            .Include(sa => sa.Product)
            .OrderByDescending(sa => sa.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> CreateStockAlertAsync(int productId, StockAlertType alertType, string message)
    {
        try
        {
            // Check if similar alert already exists
            var existingAlert = await _context.StockAlerts
                .FirstOrDefaultAsync(sa => sa.ProductId == productId && sa.AlertType == alertType && sa.IsActive && !sa.IsRead);

            if (existingAlert != null) return true; // Alert already exists

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            var alert = new StockAlert
            {
                ProductId = productId,
                AlertType = alertType,
                Message = message,
                CurrentStock = product.StockQuantity,
                TriggerLevel = (int)(alertType == StockAlertType.LowStock ? product.ReorderLevel : product.MaximumStock),
                CreatedDate = DateTime.UtcNow
            };

            _context.StockAlerts.Add(alert);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> MarkAlertAsReadAsync(int alertId, string readBy)
    {
        try
        {
            var alert = await _context.StockAlerts.FindAsync(alertId);
            if (alert == null) return false;

            alert.IsRead = true;
            alert.ReadDate = DateTime.UtcNow;
            alert.ReadBy = readBy;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task CheckAndCreateAlertsAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null || !product.IsStockTracked) return;

        // Check for out of stock
        if (product.StockQuantity <= 0)
        {
            await CreateStockAlertAsync(productId, StockAlertType.OutOfStock, 
                $"Product '{product.Name}' is out of stock");
        }
        // Check for low stock
        else if (product.StockQuantity <= product.ReorderLevel && product.ReorderLevel > 0)
        {
            await CreateStockAlertAsync(productId, StockAlertType.LowStock, 
                $"Product '{product.Name}' is running low. Current stock: {product.StockQuantity}, Reorder level: {product.ReorderLevel}");
        }

        // Check for overstock
        if (product.MaximumStock > 0 && product.StockQuantity >= product.MaximumStock)
        {
            await CreateStockAlertAsync(productId, StockAlertType.Overstock, 
                $"Product '{product.Name}' is overstocked. Current stock: {product.StockQuantity}, Maximum: {product.MaximumStock}");
        }

        // Check for negative stock when not allowed
        if (product.StockQuantity < 0 && !product.AllowNegativeStock)
        {
            await CreateStockAlertAsync(productId, StockAlertType.NegativeStock, 
                $"Product '{product.Name}' has negative stock: {product.StockQuantity}");
        }
    }

    #endregion

    #region Stock Valuation

    public async Task<decimal> GetStockValueAsync(int? productId = null)
    {
        var query = _context.Products.Where(p => p.IsStockTracked && p.IsActive);
        
        if (productId.HasValue)
            query = query.Where(p => p.Id == productId.Value);

        return await query.SumAsync(p => (decimal)p.StockQuantity * p.Price);
    }

    public async Task<decimal> GetStockValueAtCostAsync(int? productId = null)
    {
        var query = _context.Products.Where(p => p.IsStockTracked && p.IsActive);
        
        if (productId.HasValue)
            query = query.Where(p => p.Id == productId.Value);

        return await query.SumAsync(p => (decimal)p.StockQuantity * p.Cost);
    }

    #endregion

    #region Stock Reports

    public async Task<IEnumerable<StockTransaction>> GetStockMovementReportAsync(DateTime fromDate, DateTime toDate, int? productId = null)
    {
        var query = _context.StockTransactions
            .Include(st => st.Product)
            .Where(st => st.Created >= fromDate && st.Created <= toDate);

        if (productId.HasValue)
            query = query.Where(st => st.ProductId == productId.Value);

        return await query
            .OrderByDescending(st => st.Created)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetStockValuationReportAsync()
    {
        return await _context.Products
            .Where(p => p.IsStockTracked && p.IsActive)
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    #endregion

    #region Reorder Management

    public async Task<IEnumerable<Product>> GetProductsToReorderAsync()
    {
        return await _context.Products
            .Where(p => p.IsStockTracked && p.StockQuantity <= p.ReorderLevel && p.ReorderLevel > 0 && p.IsActive)
            .Include(p => p.Category)
            .ToListAsync();
    }

    public async Task<bool> CreateReorderSuggestionAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null || !product.IsStockTracked) return false;

        var reorderMessage = $"Reorder suggestion for '{product.Name}': Current stock: {product.StockQuantity}, " +
                           $"Reorder level: {product.ReorderLevel}, Suggested quantity: {product.ReorderQuantity}";

        return await CreateStockAlertAsync(productId, StockAlertType.LowStock, reorderMessage);
    }

    #endregion
}
