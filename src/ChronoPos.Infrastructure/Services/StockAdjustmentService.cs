using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
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
            FileLogger.LogSeparator("STARTING CreateStockAdjustmentAsync");
            FileLogger.LogInfo($"Input DTO - AdjustmentDate: {createDto.AdjustmentDate}");
            FileLogger.LogInfo($"Input DTO - StoreLocationId: {createDto.StoreLocationId}");
            FileLogger.LogInfo($"Input DTO - ReasonId: {createDto.ReasonId}");
            FileLogger.LogInfo($"Input DTO - Remarks: {createDto.Remarks}");
            FileLogger.LogInfo($"Input DTO - Items Count: {createDto.Items?.Count ?? 0}");
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            FileLogger.LogInfo("Database transaction started");
            
            try
            {
                // Validate and ensure required foreign key records exist
                FileLogger.LogInfo("Validating foreign key constraints...");
                await EnsureRequiredDataExistsAsync(createDto);
                
                // Generate adjustment number
                FileLogger.LogInfo("Generating adjustment number...");
                var adjustmentNo = await GenerateAdjustmentNumberAsync();
                FileLogger.LogInfo($"Generated adjustment number: {adjustmentNo}");

                // Create the main adjustment record
                FileLogger.LogInfo("Creating main adjustment record...");
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

                FileLogger.LogInfo("Adding adjustment to context...");
                _context.StockAdjustments.Add(adjustment);
                
                FileLogger.LogInfo("Saving main adjustment record...");
                await _context.SaveChangesAsync();
                FileLogger.LogInfo($"Main adjustment saved with ID: {adjustment.AdjustmentId}");

                // Add adjustment items
                FileLogger.LogInfo($"Processing {createDto.Items?.Count ?? 0} adjustment items...");
                if (createDto.Items != null)
                {
                    foreach (var itemDto in createDto.Items)
                    {
                        FileLogger.LogInfo($"Creating item for ProductId: {itemDto.ProductId}");
                        FileLogger.LogInfo($"Item - QuantityBefore: {itemDto.QuantityBefore}, QuantityAfter: {itemDto.QuantityAfter}");
                        
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

                        FileLogger.LogInfo($"Adding item to context - DifferenceQty: {item.DifferenceQty}");
                        _context.StockAdjustmentItems.Add(item);
                    }
                }

                FileLogger.LogInfo("Saving adjustment items...");
                await _context.SaveChangesAsync();
                FileLogger.LogInfo("All items saved successfully");
                
                // Update actual product stock levels for each adjustment item
                FileLogger.LogSeparator("Updating Product Stock Levels");
                if (createDto.Items != null)
                {
                    foreach (var itemDto in createDto.Items)
                    {
                        FileLogger.LogInfo($"Updating stock for ProductId: {itemDto.ProductId}");
                        FileLogger.LogInfo($"New stock level: {itemDto.QuantityAfter}");
                        
                        // Update the actual product stock quantity
                        var product = await _context.Products.FindAsync(itemDto.ProductId);
                        if (product != null)
                        {
                            var previousStock = product.StockQuantity;
                            product.StockQuantity = (int)Math.Round(itemDto.QuantityAfter); // Convert decimal to int with rounding
                            product.UpdatedAt = DateTime.Now;
                            
                            FileLogger.LogInfo($"Product stock updated: {previousStock} → {product.StockQuantity}");
                        }
                        else
                        {
                            FileLogger.LogWarning($"Product not found for ID: {itemDto.ProductId}");
                        }
                    }
                    
                    FileLogger.LogInfo("Saving product stock updates...");
                    await _context.SaveChangesAsync();
                    FileLogger.LogInfo("Product stock levels updated successfully");
                }
                
                FileLogger.LogInfo("Committing transaction...");
                await transaction.CommitAsync();
                FileLogger.LogInfo("Transaction committed successfully");

                // Return the created adjustment as DTO
                FileLogger.LogInfo($"Retrieving created adjustment with ID: {adjustment.AdjustmentId}");
                var result = await GetStockAdjustmentByIdAsync(adjustment.AdjustmentId);
                
                if (result == null)
                {
                    FileLogger.LogError("Failed to retrieve created adjustment");
                    throw new InvalidOperationException("Failed to retrieve created adjustment");
                }
                
                FileLogger.LogInfo($"SUCCESS: Returning adjustment DTO with number: {result.AdjustmentNo}");
                FileLogger.LogSeparator("CreateStockAdjustmentAsync COMPLETED");
                return result;
            }
            catch (Exception ex)
            {
                FileLogger.LogError($"ERROR in CreateStockAdjustmentAsync: {ex.Message}", ex);
                FileLogger.LogError($"Exception type: {ex.GetType().Name}");
                FileLogger.LogError($"Stack trace: {ex.StackTrace}");
                
                FileLogger.LogWarning("Rolling back transaction...");
                await transaction.RollbackAsync();
                FileLogger.LogWarning("Transaction rolled back");
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
    }
}
