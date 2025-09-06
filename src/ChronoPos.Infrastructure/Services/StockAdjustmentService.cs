using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for stock adjustment operations
    /// </summary>
    public class StockAdjustmentService : IStockAdjustmentService
    {
        private readonly ChronoPosDbContext _context;

        public StockAdjustmentService(ChronoPosDbContext context)
        {
            _context = context;
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
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.AdjustmentNo.Contains(searchTerm) || 
                                       s.Remarks != null && s.Remarks.Contains(searchTerm));
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
                    ReasonLine = i.ReasonLine,
                    RemarksLine = i.RemarksLine
                }).ToList()
            };
        }

        /// <summary>
        /// Create new stock adjustment
        /// </summary>
        public async Task<StockAdjustmentDto> CreateStockAdjustmentAsync(CreateStockAdjustmentDto createDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Generate adjustment number
                var adjustmentNo = await GenerateAdjustmentNumberAsync();

                // Create the main adjustment record
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

                _context.StockAdjustments.Add(adjustment);
                await _context.SaveChangesAsync();

                // Add adjustment items
                foreach (var itemDto in createDto.Items)
                {
                    var item = new StockAdjustmentItem
                    {
                        AdjustmentId = adjustment.AdjustmentId,
                        ProductId = itemDto.ProductId,
                        UomId = itemDto.UomId,
                        BatchNo = null, // Not provided in create DTO
                        QuantityBefore = itemDto.QuantityBefore,
                        QuantityAfter = itemDto.QuantityAfter,
                        DifferenceQty = itemDto.QuantityAfter - itemDto.QuantityBefore,
                        ReasonLine = itemDto.ReasonLine,
                        RemarksLine = itemDto.RemarksLine
                    };

                    _context.StockAdjustmentItems.Add(item);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Return the created adjustment as DTO
                return await GetStockAdjustmentByIdAsync(adjustment.AdjustmentId) ?? 
                       throw new InvalidOperationException("Failed to retrieve created adjustment");
            }
            catch
            {
                await transaction.RollbackAsync();
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
                        BatchNo = null, // Not provided in update DTO
                        QuantityBefore = itemDto.QuantityBefore,
                        QuantityAfter = itemDto.QuantityAfter,
                        DifferenceQty = itemDto.QuantityAfter - itemDto.QuantityBefore,
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
                    CurrentStock = 0, // TODO: Calculate from stock movements
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
            var today = DateTime.Now;
            var prefix = $"ADJ{today:yyyyMM}";
            
            var lastAdjustment = await _context.StockAdjustments
                .Where(s => s.AdjustmentNo.StartsWith(prefix))
                .OrderByDescending(s => s.AdjustmentNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastAdjustment != null)
            {
                var lastNumberPart = lastAdjustment.AdjustmentNo.Substring(prefix.Length);
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D4}";
        }
    }
}
