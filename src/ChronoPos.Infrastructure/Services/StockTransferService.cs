using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
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

        public StockTransferService(
            ChronoPosDbContext context,
            IStockTransferRepository stockTransferRepository,
            IShopLocationRepository shopLocationRepository)
        {
            _context = context;
            _stockTransferRepository = stockTransferRepository;
            _shopLocationRepository = shopLocationRepository;
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
                    FromStoreName = st.FromStore != null ? st.FromStore.LocationName : "",
                    ToStoreId = st.ToStoreId,
                    ToStoreName = st.ToStore != null ? st.ToStore.LocationName : "",
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
                FromStoreName = transfer.FromStore?.LocationName ?? "",
                ToStoreId = transfer.ToStoreId,
                ToStoreName = transfer.ToStore?.LocationName ?? "",
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
            var transferNo = await GenerateTransferNumberAsync();

            var transfer = new StockTransfer
            {
                TransferNo = transferNo,
                TransferDate = dto.TransferDate,
                FromStoreId = dto.FromStoreId,
                ToStoreId = dto.ToStoreId,
                Status = "Pending",
                Remarks = dto.Remarks,
                CreatedBy = 1, // TODO: Get from current user context
                CreatedAt = DateTime.UtcNow
            };

            _context.StockTransfers.Add(transfer);
            await _context.SaveChangesAsync();

            // Add transfer items
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

            // Return the created transfer
            return await GetStockTransferByIdAsync(transfer.TransferId) 
                ?? throw new InvalidOperationException("Failed to retrieve created transfer");
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
    }
}
