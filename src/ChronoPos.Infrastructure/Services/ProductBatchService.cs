using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Service implementation for ProductBatch operations
/// </summary>
public class ProductBatchService : IProductBatchService
{
    private readonly IProductBatchRepository _productBatchRepository;

    public ProductBatchService(IProductBatchRepository productBatchRepository)
    {
        _productBatchRepository = productBatchRepository;
    }

    /// <summary>
    /// Get all product batches with filtering and pagination
    /// </summary>
    public async Task<PagedResult<ProductBatchDto>> GetProductBatchesAsync(
        int page = 1,
        int pageSize = 20,
        int? productId = null,
        string? batchNo = null,
        string? status = null,
        DateTime? expiryFrom = null,
        DateTime? expiryTo = null)
    {
        var allBatches = await _productBatchRepository.GetAllAsync(productId, status, expiryFrom, expiryTo);
        
        // Apply batch number filter if provided
        if (!string.IsNullOrWhiteSpace(batchNo))
        {
            allBatches = allBatches.Where(pb => pb.BatchNo.Contains(batchNo, StringComparison.OrdinalIgnoreCase));
        }

        var totalCount = allBatches.Count();
        var batches = allBatches
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        return new PagedResult<ProductBatchDto>
        {
            Items = batches,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Get product batch by ID
    /// </summary>
    public async Task<ProductBatchDto?> GetProductBatchByIdAsync(int id)
    {
        var batch = await _productBatchRepository.GetByIdAsync(id);
        return batch == null ? null : MapToDto(batch);
    }

    /// <summary>
    /// Get product batches by product ID
    /// </summary>
    public async Task<List<ProductBatchDto>> GetProductBatchesByProductIdAsync(int productId)
    {
        var batches = await _productBatchRepository.GetByProductIdAsync(productId);
        return batches.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get expired batches
    /// </summary>
    public async Task<List<ProductBatchDto>> GetExpiredBatchesAsync()
    {
        var batches = await _productBatchRepository.GetExpiredBatchesAsync();
        return batches.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get batches near expiry
    /// </summary>
    public async Task<List<ProductBatchDto>> GetNearExpiryBatchesAsync(int days = 30)
    {
        var batches = await _productBatchRepository.GetNearExpiryBatchesAsync(days);
        return batches.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get batch summary for a product
    /// </summary>
    public async Task<ProductBatchSummaryDto> GetProductBatchSummaryAsync(int productId)
    {
        var batches = await _productBatchRepository.GetByProductIdAsync(productId);
        var batchDtos = batches.Select(MapToDto).ToList();

        var productName = batches.FirstOrDefault()?.Product?.Name ?? "";

        return new ProductBatchSummaryDto
        {
            ProductId = productId,
            ProductName = productName,
            TotalBatches = batchDtos.Count,
            TotalQuantity = batchDtos.Sum(b => b.Quantity),
            ExpiredBatches = batchDtos.Count(b => b.IsExpired),
            NearExpiryBatches = batchDtos.Count(b => b.IsNearExpiry),
            TotalValue = batchDtos.Sum(b => b.TotalValue)
        };
    }

    /// <summary>
    /// Create new product batch
    /// </summary>
    public async Task<ProductBatchDto> CreateProductBatchAsync(CreateProductBatchDto createDto)
    {
        // Check if batch number already exists for this product
        var exists = await _productBatchRepository.BatchExistsAsync(createDto.BatchNo, createDto.ProductId);
        if (exists)
        {
            throw new InvalidOperationException($"Batch number '{createDto.BatchNo}' already exists for this product.");
        }

        var batch = new ProductBatch
        {
            ProductId = createDto.ProductId,
            BatchNo = createDto.BatchNo,
            ManufactureDate = createDto.ManufactureDate,
            ExpiryDate = createDto.ExpiryDate,
            Quantity = createDto.Quantity,
            UomId = createDto.UomId,
            CostPrice = createDto.CostPrice,
            LandedCost = createDto.LandedCost,
            Status = createDto.Status
        };

        var createdBatch = await _productBatchRepository.AddAsync(batch);
        return MapToDto(createdBatch);
    }

    /// <summary>
    /// Update existing product batch
    /// </summary>
    public async Task<ProductBatchDto> UpdateProductBatchAsync(int id, CreateProductBatchDto updateDto)
    {
        var existingBatch = await _productBatchRepository.GetByIdAsync(id);
        if (existingBatch == null)
        {
            throw new InvalidOperationException($"Product batch with ID {id} not found.");
        }

        // Check if batch number already exists for this product (excluding current batch)
        var exists = await _productBatchRepository.BatchExistsAsync(updateDto.BatchNo, updateDto.ProductId, id);
        if (exists)
        {
            throw new InvalidOperationException($"Batch number '{updateDto.BatchNo}' already exists for this product.");
        }

        existingBatch.ProductId = updateDto.ProductId;
        existingBatch.BatchNo = updateDto.BatchNo;
        existingBatch.ManufactureDate = updateDto.ManufactureDate;
        existingBatch.ExpiryDate = updateDto.ExpiryDate;
        existingBatch.Quantity = updateDto.Quantity;
        existingBatch.UomId = updateDto.UomId;
        existingBatch.CostPrice = updateDto.CostPrice;
        existingBatch.LandedCost = updateDto.LandedCost;
        existingBatch.Status = updateDto.Status;

        var updatedBatch = await _productBatchRepository.UpdateAsync(existingBatch);
        return MapToDto(updatedBatch);
    }

    /// <summary>
    /// Delete product batch
    /// </summary>
    public async Task<bool> DeleteProductBatchAsync(int id)
    {
        return await _productBatchRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Check if batch number exists for a product
    /// </summary>
    public async Task<bool> BatchExistsAsync(string batchNo, int productId, int? excludeId = null)
    {
        return await _productBatchRepository.BatchExistsAsync(batchNo, productId, excludeId);
    }

    /// <summary>
    /// Update batch quantity (for inventory transactions)
    /// </summary>
    public async Task<bool> UpdateBatchQuantityAsync(int batchId, decimal newQuantity)
    {
        return await _productBatchRepository.UpdateQuantityAsync(batchId, newQuantity);
    }

    /// <summary>
    /// Get available quantity for a product across all active batches
    /// </summary>
    public async Task<decimal> GetAvailableQuantityAsync(int productId)
    {
        return await _productBatchRepository.GetTotalQuantityAsync(productId);
    }

    /// <summary>
    /// Map ProductBatch entity to DTO
    /// </summary>
    private static ProductBatchDto MapToDto(ProductBatch batch)
    {
        return new ProductBatchDto
        {
            Id = batch.Id,
            ProductId = batch.ProductId,
            ProductName = batch.Product?.Name ?? "",
            BatchNo = batch.BatchNo,
            ManufactureDate = batch.ManufactureDate,
            ExpiryDate = batch.ExpiryDate,
            Quantity = batch.Quantity,
            UomId = batch.UomId,
            UomName = batch.Uom?.Name ?? "",
            CostPrice = batch.CostPrice,
            LandedCost = batch.LandedCost,
            Status = batch.Status,
            CreatedAt = batch.CreatedAt
        };
    }
}