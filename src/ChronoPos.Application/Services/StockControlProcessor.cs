using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service for processing stock control operations during product creation
/// </summary>
public class StockControlProcessor
{
    private readonly ChronoPosDbContext _context;
    private readonly IStockService _stockService;

    public StockControlProcessor(ChronoPosDbContext context, IStockService stockService)
    {
        _context = context;
        _stockService = stockService;
    }

    public async Task<StockSaveResult> ProcessStockOnProductSave(Product product, ProductDto productDto)
    {
        var result = new StockSaveResult();
        
        try
        {
            // Update product stock properties
            product.IsStockTracked = productDto.IsStockTracked;
            product.AllowNegativeStock = productDto.AllowNegativeStock;
            product.IsUsingSerialNumbers = productDto.IsUsingSerialNumbers;
            product.MinimumStock = productDto.MinimumStock;
            product.MaximumStock = productDto.MaximumStock;
            product.ReorderLevel = productDto.ReorderLevel;
            product.ReorderQuantity = productDto.ReorderQuantity;
            product.AverageCost = productDto.AverageCost;
            product.LastCost = productDto.LastCost;
            product.InitialStock = productDto.InitialStock;

            // Create initial stock level if stock tracking enabled and initial stock > 0
            if (productDto.IsStockTracked && productDto.InitialStock > 0 && !productDto.IsUsingSerialNumbers)
            {
                var stockLevelId = await CreateInitialStockLevel(product, productDto);
                var movementId = await CreateInitialStockMovement(product, productDto);
                
                result.CreatedStockLevelId = stockLevelId;
                result.CreatedMovementId = movementId;
            }
            
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error processing stock: {ex.Message}");
        }
        
        return result;
    }
    
    private async Task<int> CreateInitialStockLevel(Product product, ProductDto productDto)
    {
        var stockLevel = new StockLevel
        {
            StoreId = productDto.SelectedStoreId,
            ProductId = product.Id,
            CurrentStock = productDto.InitialStock,
            ReservedStock = 0,
            AverageCost = productDto.AverageCost,
            LastCost = productDto.AverageCost,
            LastUpdated = DateTime.UtcNow
        };
        
        _context.StockLevels.Add(stockLevel);
        await _context.SaveChangesAsync();
        
        return stockLevel.Id;
    }
    
    private async Task<int> CreateInitialStockMovement(Product product, ProductDto productDto)
    {
        var movement = new StockTransaction
        {
            StoreId = productDto.SelectedStoreId,
            ProductId = product.Id,
            MovementType = StockDirection.IN,
            Quantity = productDto.InitialStock,
            UnitCost = productDto.AverageCost,
            ReferenceType = "INITIAL",
            ReferenceNumber = $"INIT-{product.Code}",
            Notes = "Initial stock entry for new product",
            PreviousStock = 0,
            NewStock = productDto.InitialStock,
            CreatedBy = "System", // TODO: Get current user
            Created = DateTime.UtcNow
        };
        
        _context.StockTransactions.Add(movement);
        await _context.SaveChangesAsync();
        
        return movement.Id;
    }
}
