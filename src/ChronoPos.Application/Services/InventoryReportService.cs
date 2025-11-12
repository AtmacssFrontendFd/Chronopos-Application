using ChronoPos.Application.DTOs.Inventory;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Enums;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.Text;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Inventory Report operations
/// </summary>
public class InventoryReportService : IInventoryReportService
{
    private readonly IChronoPosDbContext _context;

    public InventoryReportService(IChronoPosDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Report 1: Stock Summary Report
    /// Shows opening stock, inward, outward, and closing stock by product
    /// </summary>
    public async Task<IEnumerable<StockSummaryDto>> GetStockSummaryAsync(
        DateTime startDate, 
        DateTime endDate, 
        int? categoryId = null,
        int? productId = null)
    {
        try
        {
            // Get all products (with optional filters)
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);

            if (productId.HasValue)
                productsQuery = productsQuery.Where(p => p.Id == productId.Value);

            var products = await productsQuery.ToListAsync();

            var summaryList = new List<StockSummaryDto>();

            foreach (var product in products)
            {
                // Get stock ledger entries for this product
                var ledgerEntries = await _context.StockLedgers
                    .Where(sl => sl.ProductId == product.Id)
                    .OrderBy(sl => sl.CreatedAt)
                    .ToListAsync();

                // Calculate opening stock (balance before start date)
                var openingEntry = ledgerEntries
                    .Where(sl => sl.CreatedAt < startDate)
                    .OrderByDescending(sl => sl.CreatedAt)
                    .FirstOrDefault();
                var openingStock = openingEntry?.Balance ?? 0;

                // Get entries within period
                var periodEntries = ledgerEntries
                    .Where(sl => sl.CreatedAt >= startDate && sl.CreatedAt <= endDate)
                    .ToList();

                // Calculate inward qty (positive movements)
                var inwardQty = periodEntries
                    .Where(sl => sl.MovementType == StockMovementType.Purchase ||
                                sl.MovementType == StockMovementType.TransferIn ||
                                sl.MovementType == StockMovementType.Return ||
                                sl.MovementType == StockMovementType.Opening)
                    .Sum(sl => sl.Qty);

                // Calculate outward qty (negative movements)
                var outwardQty = periodEntries
                    .Where(sl => sl.MovementType == StockMovementType.Sale ||
                                sl.MovementType == StockMovementType.TransferOut ||
                                sl.MovementType == StockMovementType.Waste)
                    .Sum(sl => sl.Qty);

                // Calculate closing stock (balance at end date)
                var closingEntry = ledgerEntries
                    .Where(sl => sl.CreatedAt <= endDate)
                    .OrderByDescending(sl => sl.CreatedAt)
                    .FirstOrDefault();
                var closingStock = closingEntry?.Balance ?? openingStock;

                // Get unit name from base unit
                var baseUnit = await _context.ProductUnits
                    .Include(pu => pu.Unit)
                    .Where(pu => pu.ProductId == product.Id && pu.IsBase)
                    .FirstOrDefaultAsync();

                summaryList.Add(new StockSummaryDto
                {
                    ProductId = product.Id,
                    ProductCode = product.Code,
                    ProductName = product.Name,
                    Category = product.Category?.Name ?? "-",
                    Unit = baseUnit?.Unit?.Name ?? "Unit",
                    OpeningStock = openingStock,
                    InwardQty = inwardQty,
                    OutwardQty = outwardQty,
                    ClosingStock = closingStock,
                    UnitCost = product.Cost,
                    StockValue = closingStock * product.Cost
                });
            }

            return summaryList.OrderBy(s => s.ProductName);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating stock summary report: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Report 2: Stock Ledger/Movement Report
    /// Shows detailed transaction history for a product
    /// </summary>
    public async Task<IEnumerable<StockLedgerReportDto>> GetStockLedgerAsync(
        int? productId, 
        DateTime startDate, 
        DateTime endDate)
    {
        try
        {
            var query = _context.StockLedgers
                .Include(sl => sl.Product)
                .Include(sl => sl.Unit)
                    .ThenInclude(u => u.Unit)
                .Where(sl => sl.CreatedAt >= startDate && sl.CreatedAt <= endDate)
                .AsQueryable();

            if (productId.HasValue)
                query = query.Where(sl => sl.ProductId == productId.Value);

            var ledgerEntries = await query
                .OrderBy(sl => sl.ProductId)
                .ThenBy(sl => sl.CreatedAt)
                .ToListAsync();

            return ledgerEntries.Select(sl => new StockLedgerReportDto
            {
                Id = sl.Id,
                ProductId = sl.ProductId,
                ProductName = sl.Product?.Name ?? "-",
                Date = sl.CreatedAt,
                TransactionType = sl.MovementType,
                ReferenceType = sl.ReferenceType,
                RefNo = sl.ReferenceId.HasValue && sl.ReferenceType.HasValue 
                    ? $"{sl.ReferenceType.Value}-{sl.ReferenceId.Value}" 
                    : "-",
                Location = sl.Location,
                InQty = IsInwardMovement(sl.MovementType) ? sl.Qty : 0,
                OutQty = IsOutwardMovement(sl.MovementType) ? sl.Qty : 0,
                Balance = sl.Balance,
                Remarks = sl.Note
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating stock ledger report: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Report 3: Stock Valuation Report
    /// Shows current stock value by product
    /// </summary>
    public async Task<IEnumerable<StockValuationDto>> GetStockValuationAsync(
        int? categoryId = null,
        bool includeZeroStock = false)
    {
        try
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!includeZeroStock)
                query = query.Where(p => p.StockQuantity > 0);

            var products = await query.ToListAsync();

            var valuationList = products.Select(p => new StockValuationDto
            {
                ProductId = p.Id,
                ProductCode = p.Code,
                Product = p.Name,
                Category = p.Category?.Name ?? "-",
                Unit = "Unit", // Will be updated with actual unit
                Quantity = p.StockQuantity,
                UnitCost = p.Cost,
                TotalValue = p.StockQuantity * p.Cost,
                PercentageOfTotal = 0 // Will be calculated
            }).ToList();

            // Calculate percentage of total
            var totalValue = valuationList.Sum(v => v.TotalValue);
            if (totalValue > 0)
            {
                foreach (var item in valuationList)
                {
                    item.PercentageOfTotal = (item.TotalValue / totalValue) * 100;
                }
            }

            return valuationList.OrderByDescending(v => v.TotalValue);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating stock valuation report: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Report 4: Low Stock/Reorder Report
    /// Shows products that need reordering
    /// </summary>
    public async Task<IEnumerable<LowStockDto>> GetLowStockReportAsync(
        int? categoryId = null,
        bool showOnlyOutOfStock = false)
    {
        try
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.ReorderLevel > 0) // Only products with reorder level set
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (showOnlyOutOfStock)
                query = query.Where(p => p.StockQuantity <= 0);
            else
                query = query.Where(p => p.StockQuantity <= p.ReorderLevel);

            var products = await query.ToListAsync();

            var lowStockList = new List<LowStockDto>();

            foreach (var product in products)
            {
                // Get last purchase date from stock ledger
                var lastPurchase = await _context.StockLedgers
                    .Where(sl => sl.ProductId == product.Id && 
                                sl.MovementType == StockMovementType.Purchase)
                    .OrderByDescending(sl => sl.CreatedAt)
                    .FirstOrDefaultAsync();

                // Get supplier from last GRN
                string? supplierName = null;
                if (lastPurchase != null && lastPurchase.ReferenceId.HasValue)
                {
                    var grn = await _context.GoodsReceived
                        .Include(g => g.Supplier)
                        .Where(g => g.Id == lastPurchase.ReferenceId.Value)
                        .FirstOrDefaultAsync();
                    supplierName = grn?.Supplier?.CompanyName;
                }

                var shortage = product.ReorderLevel - product.StockQuantity;
                var estimatedValue = shortage > 0 ? shortage * product.Cost : 0;

                lowStockList.Add(new LowStockDto
                {
                    ProductId = product.Id,
                    ProductCode = product.Code,
                    Product = product.Name,
                    Category = product.Category?.Name ?? "-",
                    CurrentQty = product.StockQuantity,
                    ReorderLevel = product.ReorderLevel,
                    Shortage = shortage,
                    Supplier = supplierName,
                    LastPurchaseDate = lastPurchase?.CreatedAt,
                    UnitCost = product.Cost,
                    EstimatedReorderValue = estimatedValue
                });
            }

            return lowStockList.OrderBy(l => l.CurrentQty);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating low stock report: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Report 5: Goods Received Note (GRN) Report
    /// </summary>
    public async Task<IEnumerable<GoodsReceivedReportDto>> GetGoodsReceivedReportAsync(
        DateTime startDate, 
        DateTime endDate,
        int? supplierId = null,
        string? status = null)
    {
        try
        {
            var query = _context.GoodsReceivedItems
                .Include(gri => gri.GoodsReceived)
                    .ThenInclude(gr => gr.Supplier)
                .Include(gri => gri.Product)
                .Include(gri => gri.UnitOfMeasurement)
                .Where(gri => gri.GoodsReceived.ReceivedDate >= startDate && 
                             gri.GoodsReceived.ReceivedDate <= endDate)
                .AsQueryable();

            if (supplierId.HasValue)
                query = query.Where(gri => gri.GoodsReceived.SupplierId == supplierId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(gri => gri.GoodsReceived.Status == status);

            var grnItems = await query
                .OrderByDescending(gri => gri.GoodsReceived.ReceivedDate)
                .ToListAsync();

            return grnItems.Select(gri => new GoodsReceivedReportDto
            {
                GrnId = gri.GrnId,
                GrnNo = gri.GoodsReceived.GrnNo,
                Date = gri.GoodsReceived.ReceivedDate,
                Supplier = gri.GoodsReceived.Supplier?.CompanyName ?? "-",
                ProductCode = gri.Product?.Code ?? "-",
                Product = gri.Product?.Name ?? "-",
                Qty = gri.Quantity,
                Unit = gri.UnitOfMeasurement?.Name ?? "Unit",
                UnitPrice = gri.CostPrice,
                Total = gri.Quantity * gri.CostPrice,
                Status = gri.GoodsReceived.Status,
                InvoiceNo = gri.GoodsReceived.InvoiceNo
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating GRN report: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Report 6: Stock Adjustment Report
    /// </summary>
    public async Task<IEnumerable<StockAdjustmentReportDto>> GetStockAdjustmentReportAsync(
        DateTime startDate, 
        DateTime endDate,
        string? status = null)
    {
        try
        {
            // Get stock ledger entries with Adjustment movement type
            var query = _context.StockLedgers
                .Include(sl => sl.Product)
                .Include(sl => sl.Unit)
                    .ThenInclude(u => u.Unit)
                .Where(sl => sl.MovementType == StockMovementType.Adjustment &&
                            sl.CreatedAt >= startDate && 
                            sl.CreatedAt <= endDate)
                .AsQueryable();

            var adjustments = await query
                .OrderByDescending(sl => sl.CreatedAt)
                .ToListAsync();

            var adjustmentList = new List<StockAdjustmentReportDto>();

            foreach (var adj in adjustments)
            {
                // Get previous balance to calculate difference
                var previousEntry = await _context.StockLedgers
                    .Where(sl => sl.ProductId == adj.ProductId && 
                                sl.CreatedAt < adj.CreatedAt)
                    .OrderByDescending(sl => sl.CreatedAt)
                    .FirstOrDefaultAsync();

                var previousQty = previousEntry?.Balance ?? 0;
                var difference = adj.Qty; // Adjustment quantity can be positive or negative

                adjustmentList.Add(new StockAdjustmentReportDto
                {
                    AdjustmentId = adj.Id,
                    AdjustmentNo = adj.ReferenceId.HasValue ? $"ADJ-{adj.ReferenceId.Value}" : $"ADJ-{adj.Id}",
                    Date = adj.CreatedAt,
                    ProductCode = adj.Product?.Code ?? "-",
                    Product = adj.Product?.Name ?? "-",
                    Unit = adj.Unit?.Unit?.Name ?? "Unit",
                    PreviousQty = previousQty,
                    NewQty = adj.Balance,
                    Difference = difference,
                    Reason = adj.Note ?? "Manual Adjustment",
                    User = "System", // Will be updated if user tracking is available
                    Status = "Approved", // Default status
                    Note = adj.Note
                });
            }

            return adjustmentList;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating stock adjustment report: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Report 7: Stock Transfer Report
    /// </summary>
    public async Task<IEnumerable<StockTransferReportDto>> GetStockTransferReportAsync(
        DateTime startDate, 
        DateTime endDate,
        string? status = null)
    {
        try
        {
            var query = _context.StockTransferItems
                .Include(sti => sti.Transfer)
                    .ThenInclude(st => st.FromStore)
                .Include(sti => sti.Transfer)
                    .ThenInclude(st => st.ToStore)
                .Include(sti => sti.Product)
                .Include(sti => sti.Uom)
                .Where(sti => sti.Transfer.TransferDate >= startDate && 
                             sti.Transfer.TransferDate <= endDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(sti => sti.Transfer.Status == status);

            var transferItems = await query
                .OrderByDescending(sti => sti.Transfer.TransferDate)
                .ToListAsync();

            return transferItems.Select(sti => new StockTransferReportDto
            {
                TransferId = sti.TransferId,
                TransferNo = sti.Transfer.TransferNo,
                Date = sti.Transfer.TransferDate,
                FromLocation = sti.Transfer.FromStore?.Name ?? "-",
                ToLocation = sti.Transfer.ToStore?.Name ?? "-",
                ProductCode = sti.Product?.Code ?? "-",
                Product = sti.Product?.Name ?? "-",
                Qty = sti.QuantitySent,
                Unit = sti.Uom?.Name ?? "Unit",
                Status = sti.Transfer.Status,
                SentBy = null, // Will be updated if user tracking is available
                ReceivedBy = null,
                Note = sti.Transfer.Remarks
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating stock transfer report: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Report 8: Stock Aging Report
    /// </summary>
    public async Task<IEnumerable<StockAgingDto>> GetStockAgingReportAsync(
        int? categoryId = null,
        int minDays = 0)
    {
        try
        {
            var query = _context.ProductBatches
                .Include(pb => pb.Product)
                    .ThenInclude(p => p.Category)
                .Where(pb => pb.Quantity > 0 && pb.Status == "Active")
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(pb => pb.Product.CategoryId == categoryId.Value);

            var batches = await query.ToListAsync();

            var agingList = batches.Select(pb =>
            {
                var daysInStock = pb.ManufactureDate.HasValue 
                    ? (DateTime.UtcNow - pb.ManufactureDate.Value).Days 
                    : 0;
                var daysToExpiry = pb.ExpiryDate.HasValue 
                    ? (pb.ExpiryDate.Value - DateTime.UtcNow).Days 
                    : (int?)null;

                var ageCategory = daysInStock switch
                {
                    <= 30 => "0-30 days",
                    <= 60 => "31-60 days",
                    <= 90 => "61-90 days",
                    <= 180 => "91-180 days",
                    _ => "180+ days"
                };

                return new StockAgingDto
                {
                    ProductId = pb.ProductId,
                    BatchId = pb.Id,
                    ProductCode = pb.Product?.Code ?? "-",
                    Product = pb.Product?.Name ?? "-",
                    BatchNo = pb.BatchNo,
                    Quantity = pb.Quantity,
                    DaysInStock = daysInStock,
                    PurchaseDate = pb.ManufactureDate ?? pb.CreatedAt,
                    ExpiryDate = pb.ExpiryDate,
                    DaysToExpiry = daysToExpiry,
                    UnitCost = pb.Product?.Cost ?? 0,
                    TotalValue = pb.Quantity * (pb.Product?.Cost ?? 0),
                    AgeCategory = ageCategory
                };
            })
            .Where(a => a.DaysInStock >= minDays)
            .OrderBy(a => a.DaysToExpiry ?? int.MaxValue)
            .ThenByDescending(a => a.DaysInStock)
            .ToList();

            return agingList;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating stock aging report: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Report 9: Goods Return/Replacement Report
    /// </summary>
    public async Task<IEnumerable<GoodsReturnReportDto>> GetGoodsReturnReportAsync(
        DateTime startDate, 
        DateTime endDate,
        int? supplierId = null,
        string? type = null)
    {
        try
        {
            // Combine Goods Return and Goods Replace
            var returns = new List<GoodsReturnReportDto>();

            // Get Goods Returns
            var goodsReturnQuery = _context.GoodsReturnItems
                .Include(gri => gri.Return)
                    .ThenInclude(gr => gr.Supplier)
                .Include(gri => gri.Product)
                .Where(gri => gri.Return.ReturnDate >= startDate && 
                             gri.Return.ReturnDate <= endDate)
                .AsQueryable();

            if (supplierId.HasValue)
                goodsReturnQuery = goodsReturnQuery.Where(gri => gri.Return.SupplierId == supplierId.Value);

            var returnItems = await goodsReturnQuery.ToListAsync();

            returns.AddRange(returnItems.Select(gri => new GoodsReturnReportDto
            {
                ReturnId = gri.ReturnId,
                ReturnNo = gri.Return.ReturnNo,
                Date = gri.Return.ReturnDate,
                Type = "Return",
                Supplier = gri.Return.Supplier?.CompanyName ?? "-",
                ProductCode = gri.Product?.Code ?? "-",
                Product = gri.Product?.Name ?? "-",
                Qty = gri.Quantity,
                Unit = "Unit",
                Reason = gri.Reason ?? "-",
                Amount = gri.LineTotal,
                ReplacementStatus = "Not Applicable",
                Status = gri.Return.Status,
                Note = gri.Return.Remarks
            }));

            // Get Goods Replacements
            var goodsReplaceQuery = _context.GoodsReplaceItems
                .Include(gri => gri.Replace)
                    .ThenInclude(gr => gr.Supplier)
                .Include(gri => gri.Product)
                .Where(gri => gri.Replace.ReplaceDate >= startDate && 
                             gri.Replace.ReplaceDate <= endDate)
                .AsQueryable();

            if (supplierId.HasValue)
                goodsReplaceQuery = goodsReplaceQuery.Where(gri => gri.Replace.SupplierId == supplierId.Value);

            var replaceItems = await goodsReplaceQuery.ToListAsync();

            returns.AddRange(replaceItems.Select(gri => new GoodsReturnReportDto
            {
                ReturnId = gri.ReplaceId,
                ReturnNo = gri.Replace.ReplaceNo,
                Date = gri.Replace.ReplaceDate,
                Type = "Replace",
                Supplier = gri.Replace.Supplier?.CompanyName ?? "-",
                ProductCode = gri.Product?.Code ?? "-",
                Product = gri.Product?.Name ?? "-",
                Qty = gri.Quantity,
                Unit = "Unit",
                Reason = gri.Replace.Remarks ?? "-",
                Amount = gri.Amount,
                ReplacementStatus = gri.ReferenceReturnItemId.HasValue ? "Replaced" : "Pending",
                Status = gri.Replace.Status,
                Note = gri.Replace.Remarks
            }));

            // Filter by type if specified
            if (!string.IsNullOrEmpty(type))
                returns = returns.Where(r => r.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();

            return returns.OrderByDescending(r => r.Date);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating goods return report: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Report 10: Inventory Summary by Category
    /// </summary>
    public async Task<IEnumerable<InventorySummaryDto>> GetInventorySummaryByCategoryAsync()
    {
        try
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();

            var summaryList = new List<InventorySummaryDto>();

            foreach (var category in categories)
            {
                var categoryProducts = category.Products?.ToList() ?? new List<Domain.Entities.Product>();
                
                var noOfItems = categoryProducts.Count;
                var totalQty = categoryProducts.Sum(p => p.StockQuantity);
                var totalValue = categoryProducts.Sum(p => p.StockQuantity * p.Cost);
                var avgCost = noOfItems > 0 ? categoryProducts.Average(p => p.Cost) : 0;
                var lowStockItems = categoryProducts.Count(p => p.StockQuantity > 0 && p.StockQuantity <= p.ReorderLevel);
                var outOfStockItems = categoryProducts.Count(p => p.StockQuantity <= 0);

                summaryList.Add(new InventorySummaryDto
                {
                    CategoryId = category.Id,
                    Category = category.Name,
                    NoOfItems = noOfItems,
                    TotalQty = totalQty,
                    TotalValue = totalValue,
                    AvgCost = avgCost,
                    PercentageOfTotal = 0, // Will be calculated
                    LowStockItems = lowStockItems,
                    OutOfStockItems = outOfStockItems
                });
            }

            // Calculate percentage of total
            var grandTotal = summaryList.Sum(s => s.TotalValue);
            if (grandTotal > 0)
            {
                foreach (var summary in summaryList)
                {
                    summary.PercentageOfTotal = (summary.TotalValue / grandTotal) * 100;
                }
            }

            return summaryList.OrderByDescending(s => s.TotalValue);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error generating inventory summary report: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Export to Excel
    /// </summary>
    public async Task<byte[]> ExportToExcelAsync(InventoryReportFilterDto filter)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Inventory Report");
        
        // Add header
        sheet.Cell("A1").Value = "Inventory Report";
        sheet.Cell("A1").Style.Font.Bold = true;
        sheet.Cell("A1").Style.Font.FontSize = 16;
        sheet.Range("A1:H1").Merge();
        
        sheet.Cell("A2").Value = $"Report Type: {filter.ReportType}";
        sheet.Cell("A3").Value = $"Date Range: {filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}";
        sheet.Cell("A4").Value = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        
        int currentRow = 6;
        
        // Export based on report type
        switch (filter.ReportType)
        {
            case "StockSummary":
                await ExportStockSummaryToExcel(sheet, currentRow, filter);
                break;
            case "StockLedger":
                await ExportStockLedgerToExcel(sheet, currentRow, filter);
                break;
            case "StockValuation":
                await ExportStockValuationToExcel(sheet, currentRow, filter);
                break;
            case "LowStock":
                await ExportLowStockToExcel(sheet, currentRow, filter);
                break;
            case "GoodsReceived":
                await ExportGoodsReceivedToExcel(sheet, currentRow, filter);
                break;
            case "StockAdjustment":
                await ExportStockAdjustmentToExcel(sheet, currentRow, filter);
                break;
            case "StockTransfer":
                await ExportStockTransferToExcel(sheet, currentRow, filter);
                break;
            case "StockAging":
                await ExportStockAgingToExcel(sheet, currentRow, filter);
                break;
            case "GoodsReturn":
                await ExportGoodsReturnToExcel(sheet, currentRow, filter);
                break;
            case "CategorySummary":
                await ExportCategorySummaryToExcel(sheet, currentRow, filter);
                break;
        }
        
        // Auto-fit columns
        sheet.Columns().AdjustToContents();
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportToPdfAsync(InventoryReportFilterDto filter)
    {
        var document = new PdfDocument();
        document.Info.Title = "Inventory Report";
        document.Info.Subject = $"Inventory Report - {filter.ReportType}";
        document.Info.Creator = "ChronoPos";

        var page = document.AddPage();
        page.Size = PdfSharpCore.PageSize.A4;
        var gfx = XGraphics.FromPdfPage(page);
        
        var titleFont = new XFont("Arial", 20, XFontStyle.Bold);
        var headerFont = new XFont("Arial", 12, XFontStyle.Bold);
        var normalFont = new XFont("Arial", 10, XFontStyle.Regular);
        var smallFont = new XFont("Arial", 8, XFontStyle.Regular);

        double yPosition = 40;
        double leftMargin = 40;
        double pageWidth = page.Width - 80;

        // Title
        gfx.DrawString("Inventory Report", titleFont, XBrushes.DarkBlue, new XRect(leftMargin, yPosition, pageWidth, 30), XStringFormats.TopLeft);
        yPosition += 40;

        // Report Info
        gfx.DrawString($"Report Type: {filter.ReportType}", normalFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 20;
        gfx.DrawString($"Period: {filter.StartDate:dd-MMM-yyyy} to {filter.EndDate:dd-MMM-yyyy}", normalFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 20;
        gfx.DrawString($"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}", normalFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 40;

        // Export based on report type
        switch (filter.ReportType)
        {
            case "StockSummary":
                var stockSummary = await GetStockSummaryAsync(filter.StartDate, filter.EndDate, filter.ProductId, filter.CategoryId);
                ExportStockSummaryToPdf(gfx, ref yPosition, leftMargin, pageWidth, page, stockSummary.Take(15), headerFont, smallFont);
                break;
            case "StockLedger":
                if (filter.ProductId.HasValue)
                {
                    var ledgerData = await GetStockLedgerAsync(filter.ProductId.Value, filter.StartDate, filter.EndDate);
                    ExportStockLedgerToPdf(gfx, ref yPosition, leftMargin, pageWidth, page, ledgerData.Take(20), headerFont, smallFont);
                }
                break;
            case "StockValuation":
                var valuationData = await GetStockValuationAsync(filter.CategoryId);
                ExportStockValuationToPdf(gfx, ref yPosition, leftMargin, pageWidth, page, valuationData.Take(20), headerFont, smallFont);
                break;
            case "LowStock":
                var lowStockData = await GetLowStockReportAsync(filter.CategoryId);
                ExportLowStockToPdf(gfx, ref yPosition, leftMargin, pageWidth, page, lowStockData.Take(20), headerFont, smallFont);
                break;
            case "GoodsReceived":
                var grnData = await GetGoodsReceivedReportAsync(filter.StartDate, filter.EndDate);
                ExportGoodsReceivedToPdf(gfx, ref yPosition, leftMargin, pageWidth, page, grnData.Take(20), headerFont, smallFont);
                break;
            case "StockAdjustment":
                var adjData = await GetStockAdjustmentReportAsync(filter.StartDate, filter.EndDate);
                ExportStockAdjustmentToPdf(gfx, ref yPosition, leftMargin, pageWidth, page, adjData.Take(20), headerFont, smallFont);
                break;
            case "StockTransfer":
                var transferData = await GetStockTransferReportAsync(filter.StartDate, filter.EndDate);
                ExportStockTransferToPdf(gfx, ref yPosition, leftMargin, pageWidth, page, transferData.Take(20), headerFont, smallFont);
                break;
            case "StockAging":
                var agingData = await GetStockAgingReportAsync(filter.CategoryId);
                ExportStockAgingToPdf(gfx, ref yPosition, leftMargin, pageWidth, page, agingData.Take(20), headerFont, smallFont);
                break;
            case "GoodsReturn":
                var returnData = await GetGoodsReturnReportAsync(filter.StartDate, filter.EndDate);
                ExportGoodsReturnToPdf(gfx, ref yPosition, leftMargin, pageWidth, page, returnData.Take(20), headerFont, smallFont);
                break;
            case "CategorySummary":
                var categoryData = await GetInventorySummaryByCategoryAsync();
                ExportCategorySummaryToPdf(gfx, ref yPosition, leftMargin, pageWidth, page, categoryData.Take(15), headerFont, smallFont);
                break;
        }

        // Footer
        gfx.DrawString("Page 1", smallFont, XBrushes.Black, new XRect(leftMargin, page.Height - 40, pageWidth, 20), XStringFormats.Center);

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportToCsvAsync(InventoryReportFilterDto filter)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Inventory Report");
        csv.AppendLine($"Report Type,{filter.ReportType}");
        csv.AppendLine($"Date Range,{filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}");
        csv.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine();
        
        // Export based on report type
        switch (filter.ReportType)
        {
            case "StockSummary":
                await ExportStockSummaryToCsv(csv, filter);
                break;
            case "StockLedger":
                await ExportStockLedgerToCsv(csv, filter);
                break;
            case "StockValuation":
                await ExportStockValuationToCsv(csv, filter);
                break;
            case "LowStock":
                await ExportLowStockToCsv(csv, filter);
                break;
            case "GoodsReceived":
                await ExportGoodsReceivedToCsv(csv, filter);
                break;
            case "StockAdjustment":
                await ExportStockAdjustmentToCsv(csv, filter);
                break;
            case "StockTransfer":
                await ExportStockTransferToCsv(csv, filter);
                break;
            case "StockAging":
                await ExportStockAgingToCsv(csv, filter);
                break;
            case "GoodsReturn":
                await ExportGoodsReturnToCsv(csv, filter);
                break;
            case "CategorySummary":
                await ExportCategorySummaryToCsv(csv, filter);
                break;
        }
        
        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    // Helper methods
    private static bool IsInwardMovement(StockMovementType type)
    {
        return type == StockMovementType.Purchase ||
               type == StockMovementType.TransferIn ||
               type == StockMovementType.Return ||
               type == StockMovementType.Opening;
    }

    private static bool IsOutwardMovement(StockMovementType type)
    {
        return type == StockMovementType.Sale ||
               type == StockMovementType.TransferOut ||
               type == StockMovementType.Waste;
    }

    #region Excel Export Helpers

    private async Task ExportStockSummaryToExcel(IXLWorksheet sheet, int startRow, InventoryReportFilterDto filter)
    {
        var data = await GetStockSummaryAsync(filter.StartDate, filter.EndDate, filter.ProductId, filter.CategoryId);
        
        // Headers
        sheet.Cell(startRow, 1).Value = "Product Code";
        sheet.Cell(startRow, 2).Value = "Product Name";
        sheet.Cell(startRow, 3).Value = "Category";
        sheet.Cell(startRow, 4).Value = "Opening Stock";
        sheet.Cell(startRow, 5).Value = "Inward Qty";
        sheet.Cell(startRow, 6).Value = "Outward Qty";
        sheet.Cell(startRow, 7).Value = "Closing Stock";
        sheet.Cell(startRow, 8).Value = "Stock Value";
        sheet.Range(startRow, 1, startRow, 8).Style.Font.Bold = true;
        sheet.Range(startRow, 1, startRow, 8).Style.Fill.BackgroundColor = XLColor.LightGray;
        
        // Data
        int row = startRow + 1;
        foreach (var item in data)
        {
            sheet.Cell(row, 1).Value = item.ProductCode;
            sheet.Cell(row, 2).Value = item.ProductName;
            sheet.Cell(row, 3).Value = item.Category;
            sheet.Cell(row, 4).Value = item.OpeningStock;
            sheet.Cell(row, 5).Value = item.InwardQty;
            sheet.Cell(row, 6).Value = item.OutwardQty;
            sheet.Cell(row, 7).Value = item.ClosingStock;
            sheet.Cell(row, 8).Value = item.StockValue;
            row++;
        }
    }

    private async Task ExportStockLedgerToExcel(IXLWorksheet sheet, int startRow, InventoryReportFilterDto filter)
    {
        if (!filter.ProductId.HasValue) return;
        
        var data = await GetStockLedgerAsync(filter.ProductId.Value, filter.StartDate, filter.EndDate);
        
        // Headers
        sheet.Cell(startRow, 1).Value = "Date";
        sheet.Cell(startRow, 2).Value = "Ref No";
        sheet.Cell(startRow, 3).Value = "Transaction Type";
        sheet.Cell(startRow, 4).Value = "In Qty";
        sheet.Cell(startRow, 5).Value = "Out Qty";
        sheet.Cell(startRow, 6).Value = "Balance";
        sheet.Range(startRow, 1, startRow, 6).Style.Font.Bold = true;
        sheet.Range(startRow, 1, startRow, 6).Style.Fill.BackgroundColor = XLColor.LightGray;
        
        // Data
        int row = startRow + 1;
        foreach (var item in data)
        {
            sheet.Cell(row, 1).Value = item.Date.ToString("yyyy-MM-dd");
            sheet.Cell(row, 2).Value = item.RefNo;
            sheet.Cell(row, 3).Value = item.TransactionType.ToString();
            sheet.Cell(row, 4).Value = item.InQty;
            sheet.Cell(row, 5).Value = item.OutQty;
            sheet.Cell(row, 6).Value = item.Balance;
            row++;
        }
    }

    private async Task ExportStockValuationToExcel(IXLWorksheet sheet, int startRow, InventoryReportFilterDto filter)
    {
        var data = await GetStockValuationAsync(filter.CategoryId);
        
        // Headers
        sheet.Cell(startRow, 1).Value = "Product Code";
        sheet.Cell(startRow, 2).Value = "Product";
        sheet.Cell(startRow, 3).Value = "Category";
        sheet.Cell(startRow, 4).Value = "Quantity";
        sheet.Cell(startRow, 5).Value = "Unit Cost";
        sheet.Cell(startRow, 6).Value = "Total Value";
        sheet.Range(startRow, 1, startRow, 6).Style.Font.Bold = true;
        sheet.Range(startRow, 1, startRow, 6).Style.Fill.BackgroundColor = XLColor.LightGray;
        
        // Data
        int row = startRow + 1;
        foreach (var item in data)
        {
            sheet.Cell(row, 1).Value = item.ProductCode;
            sheet.Cell(row, 2).Value = item.Product;
            sheet.Cell(row, 3).Value = item.Category;
            sheet.Cell(row, 4).Value = item.Quantity;
            sheet.Cell(row, 5).Value = item.UnitCost;
            sheet.Cell(row, 6).Value = item.TotalValue;
            row++;
        }
    }

    private async Task ExportLowStockToExcel(IXLWorksheet sheet, int startRow, InventoryReportFilterDto filter)
    {
        var data = await GetLowStockReportAsync(filter.CategoryId);
        
        // Headers
        sheet.Cell(startRow, 1).Value = "Product Code";
        sheet.Cell(startRow, 2).Value = "Product";
        sheet.Cell(startRow, 3).Value = "Category";
        sheet.Cell(startRow, 4).Value = "Current Qty";
        sheet.Cell(startRow, 5).Value = "Reorder Level";
        sheet.Cell(startRow, 6).Value = "Status";
        sheet.Range(startRow, 1, startRow, 6).Style.Font.Bold = true;
        sheet.Range(startRow, 1, startRow, 6).Style.Fill.BackgroundColor = XLColor.LightGray;
        
        // Data
        int row = startRow + 1;
        foreach (var item in data)
        {
            sheet.Cell(row, 1).Value = item.ProductCode;
            sheet.Cell(row, 2).Value = item.Product;
            sheet.Cell(row, 3).Value = item.Category;
            sheet.Cell(row, 4).Value = item.CurrentQty;
            sheet.Cell(row, 5).Value = item.ReorderLevel;
            sheet.Cell(row, 6).Value = item.StatusText;
            row++;
        }
    }

    private async Task ExportGoodsReceivedToExcel(IXLWorksheet sheet, int startRow, InventoryReportFilterDto filter)
    {
        var data = await GetGoodsReceivedReportAsync(filter.StartDate, filter.EndDate);
        
        // Headers
        sheet.Cell(startRow, 1).Value = "Date";
        sheet.Cell(startRow, 2).Value = "GRN No";
        sheet.Cell(startRow, 3).Value = "Supplier";
        sheet.Cell(startRow, 4).Value = "Product";
        sheet.Cell(startRow, 5).Value = "Qty";
        sheet.Cell(startRow, 6).Value = "Unit Price";
        sheet.Cell(startRow, 7).Value = "Total";
        sheet.Range(startRow, 1, startRow, 7).Style.Font.Bold = true;
        sheet.Range(startRow, 1, startRow, 7).Style.Fill.BackgroundColor = XLColor.LightGray;
        
        // Data
        int row = startRow + 1;
        foreach (var item in data)
        {
            sheet.Cell(row, 1).Value = item.Date.ToString("yyyy-MM-dd");
            sheet.Cell(row, 2).Value = item.GrnNo;
            sheet.Cell(row, 3).Value = item.Supplier;
            sheet.Cell(row, 4).Value = item.Product;
            sheet.Cell(row, 5).Value = item.Qty;
            sheet.Cell(row, 6).Value = item.UnitPrice;
            sheet.Cell(row, 7).Value = item.Total;
            row++;
        }
    }

    private async Task ExportStockAdjustmentToExcel(IXLWorksheet sheet, int startRow, InventoryReportFilterDto filter)
    {
        var data = await GetStockAdjustmentReportAsync(filter.StartDate, filter.EndDate);
        
        // Headers
        sheet.Cell(startRow, 1).Value = "Date";
        sheet.Cell(startRow, 2).Value = "Adjustment No";
        sheet.Cell(startRow, 3).Value = "Product";
        sheet.Cell(startRow, 4).Value = "Previous Qty";
        sheet.Cell(startRow, 5).Value = "New Qty";
        sheet.Cell(startRow, 6).Value = "Difference";
        sheet.Cell(startRow, 7).Value = "Reason";
        sheet.Range(startRow, 1, startRow, 7).Style.Font.Bold = true;
        sheet.Range(startRow, 1, startRow, 7).Style.Fill.BackgroundColor = XLColor.LightGray;
        
        // Data
        int row = startRow + 1;
        foreach (var item in data)
        {
            sheet.Cell(row, 1).Value = item.Date.ToString("yyyy-MM-dd");
            sheet.Cell(row, 2).Value = item.AdjustmentNo;
            sheet.Cell(row, 3).Value = item.Product;
            sheet.Cell(row, 4).Value = item.PreviousQty;
            sheet.Cell(row, 5).Value = item.NewQty;
            sheet.Cell(row, 6).Value = item.Difference;
            sheet.Cell(row, 7).Value = item.Reason;
            row++;
        }
    }

    private async Task ExportStockTransferToExcel(IXLWorksheet sheet, int startRow, InventoryReportFilterDto filter)
    {
        var data = await GetStockTransferReportAsync(filter.StartDate, filter.EndDate);
        
        // Headers
        sheet.Cell(startRow, 1).Value = "Date";
        sheet.Cell(startRow, 2).Value = "Transfer No";
        sheet.Cell(startRow, 3).Value = "Product";
        sheet.Cell(startRow, 4).Value = "From Location";
        sheet.Cell(startRow, 5).Value = "To Location";
        sheet.Cell(startRow, 6).Value = "Qty";
        sheet.Range(startRow, 1, startRow, 6).Style.Font.Bold = true;
        sheet.Range(startRow, 1, startRow, 6).Style.Fill.BackgroundColor = XLColor.LightGray;
        
        // Data
        int row = startRow + 1;
        foreach (var item in data)
        {
            sheet.Cell(row, 1).Value = item.Date.ToString("yyyy-MM-dd");
            sheet.Cell(row, 2).Value = item.TransferNo;
            sheet.Cell(row, 3).Value = item.Product;
            sheet.Cell(row, 4).Value = item.FromLocation;
            sheet.Cell(row, 5).Value = item.ToLocation;
            sheet.Cell(row, 6).Value = item.Qty;
            row++;
        }
    }

    private async Task ExportStockAgingToExcel(IXLWorksheet sheet, int startRow, InventoryReportFilterDto filter)
    {
        var data = await GetStockAgingReportAsync(filter.CategoryId);
        
        // Headers
        sheet.Cell(startRow, 1).Value = "Product Code";
        sheet.Cell(startRow, 2).Value = "Product";
        sheet.Cell(startRow, 3).Value = "Batch No";
        sheet.Cell(startRow, 4).Value = "Days In Stock";
        sheet.Cell(startRow, 5).Value = "Quantity";
        sheet.Cell(startRow, 6).Value = "Age Category";
        sheet.Range(startRow, 1, startRow, 6).Style.Font.Bold = true;
        sheet.Range(startRow, 1, startRow, 6).Style.Fill.BackgroundColor = XLColor.LightGray;
        
        // Data
        int row = startRow + 1;
        foreach (var item in data)
        {
            sheet.Cell(row, 1).Value = item.ProductCode;
            sheet.Cell(row, 2).Value = item.Product;
            sheet.Cell(row, 3).Value = item.BatchNo ?? "N/A";
            sheet.Cell(row, 4).Value = item.DaysInStock;
            sheet.Cell(row, 5).Value = item.Quantity;
            sheet.Cell(row, 6).Value = item.AgeCategory;
            row++;
        }
    }

    private async Task ExportGoodsReturnToExcel(IXLWorksheet sheet, int startRow, InventoryReportFilterDto filter)
    {
        var data = await GetGoodsReturnReportAsync(filter.StartDate, filter.EndDate);
        
        // Headers
        sheet.Cell(startRow, 1).Value = "Date";
        sheet.Cell(startRow, 2).Value = "Return No";
        sheet.Cell(startRow, 3).Value = "Supplier";
        sheet.Cell(startRow, 4).Value = "Product";
        sheet.Cell(startRow, 5).Value = "Qty";
        sheet.Cell(startRow, 6).Value = "Reason";
        sheet.Range(startRow, 1, startRow, 6).Style.Font.Bold = true;
        sheet.Range(startRow, 1, startRow, 6).Style.Fill.BackgroundColor = XLColor.LightGray;
        
        // Data
        int row = startRow + 1;
        foreach (var item in data)
        {
            sheet.Cell(row, 1).Value = item.Date.ToString("yyyy-MM-dd");
            sheet.Cell(row, 2).Value = item.ReturnNo;
            sheet.Cell(row, 3).Value = item.Supplier;
            sheet.Cell(row, 4).Value = item.Product;
            sheet.Cell(row, 5).Value = item.Qty;
            sheet.Cell(row, 6).Value = item.Reason;
            row++;
        }
    }

    private async Task ExportCategorySummaryToExcel(IXLWorksheet sheet, int startRow, InventoryReportFilterDto filter)
    {
        var data = await GetInventorySummaryByCategoryAsync();
        
        // Headers
        sheet.Cell(startRow, 1).Value = "Category";
        sheet.Cell(startRow, 2).Value = "No of Items";
        sheet.Cell(startRow, 3).Value = "Total Qty";
        sheet.Cell(startRow, 4).Value = "Total Value";
        sheet.Cell(startRow, 5).Value = "Low Stock Items";
        sheet.Cell(startRow, 6).Value = "Out of Stock";
        sheet.Range(startRow, 1, startRow, 6).Style.Font.Bold = true;
        sheet.Range(startRow, 1, startRow, 6).Style.Fill.BackgroundColor = XLColor.LightGray;
        
        // Data
        int row = startRow + 1;
        foreach (var item in data)
        {
            sheet.Cell(row, 1).Value = item.Category;
            sheet.Cell(row, 2).Value = item.NoOfItems;
            sheet.Cell(row, 3).Value = item.TotalQty;
            sheet.Cell(row, 4).Value = item.TotalValue;
            sheet.Cell(row, 5).Value = item.LowStockItems;
            sheet.Cell(row, 6).Value = item.OutOfStockItems;
            row++;
        }
    }

    #endregion

    #region CSV Export Helpers

    private async Task ExportStockSummaryToCsv(StringBuilder csv, InventoryReportFilterDto filter)
    {
        var data = await GetStockSummaryAsync(filter.StartDate, filter.EndDate, filter.ProductId, filter.CategoryId);
        
        csv.AppendLine("Product Code,Product Name,Category,Opening Stock,Inward Qty,Outward Qty,Closing Stock,Stock Value");
        foreach (var item in data)
        {
            csv.AppendLine($"{item.ProductCode},{item.ProductName},{item.Category},{item.OpeningStock},{item.InwardQty},{item.OutwardQty},{item.ClosingStock},{item.StockValue}");
        }
    }

    private async Task ExportStockLedgerToCsv(StringBuilder csv, InventoryReportFilterDto filter)
    {
        if (!filter.ProductId.HasValue) return;
        
        var data = await GetStockLedgerAsync(filter.ProductId.Value, filter.StartDate, filter.EndDate);
        
        csv.AppendLine("Date,Ref No,Transaction Type,In Qty,Out Qty,Balance");
        foreach (var item in data)
        {
            csv.AppendLine($"{item.Date:yyyy-MM-dd},{item.RefNo},{item.TransactionType},{item.InQty},{item.OutQty},{item.Balance}");
        }
    }

    private async Task ExportStockValuationToCsv(StringBuilder csv, InventoryReportFilterDto filter)
    {
        var data = await GetStockValuationAsync(filter.CategoryId);
        
        csv.AppendLine("Product Code,Product,Category,Quantity,Unit Cost,Total Value");
        foreach (var item in data)
        {
            csv.AppendLine($"{item.ProductCode},{item.Product},{item.Category},{item.Quantity},{item.UnitCost},{item.TotalValue}");
        }
    }

    private async Task ExportLowStockToCsv(StringBuilder csv, InventoryReportFilterDto filter)
    {
        var data = await GetLowStockReportAsync(filter.CategoryId);
        
        csv.AppendLine("Product Code,Product,Category,Current Qty,Reorder Level,Status");
        foreach (var item in data)
        {
            csv.AppendLine($"{item.ProductCode},{item.Product},{item.Category},{item.CurrentQty},{item.ReorderLevel},{item.StatusText}");
        }
    }

    private async Task ExportGoodsReceivedToCsv(StringBuilder csv, InventoryReportFilterDto filter)
    {
        var data = await GetGoodsReceivedReportAsync(filter.StartDate, filter.EndDate);
        
        csv.AppendLine("Date,GRN No,Supplier,Product,Qty,Unit Price,Total");
        foreach (var item in data)
        {
            csv.AppendLine($"{item.Date:yyyy-MM-dd},{item.GrnNo},{item.Supplier},{item.Product},{item.Qty},{item.UnitPrice},{item.Total}");
        }
    }

    private async Task ExportStockAdjustmentToCsv(StringBuilder csv, InventoryReportFilterDto filter)
    {
        var data = await GetStockAdjustmentReportAsync(filter.StartDate, filter.EndDate);
        
        csv.AppendLine("Date,Adjustment No,Product,Previous Qty,New Qty,Difference,Reason");
        foreach (var item in data)
        {
            csv.AppendLine($"{item.Date:yyyy-MM-dd},{item.AdjustmentNo},{item.Product},{item.PreviousQty},{item.NewQty},{item.Difference},{item.Reason}");
        }
    }

    private async Task ExportStockTransferToCsv(StringBuilder csv, InventoryReportFilterDto filter)
    {
        var data = await GetStockTransferReportAsync(filter.StartDate, filter.EndDate);
        
        csv.AppendLine("Date,Transfer No,Product,From Location,To Location,Qty");
        foreach (var item in data)
        {
            csv.AppendLine($"{item.Date:yyyy-MM-dd},{item.TransferNo},{item.Product},{item.FromLocation},{item.ToLocation},{item.Qty}");
        }
    }

    private async Task ExportStockAgingToCsv(StringBuilder csv, InventoryReportFilterDto filter)
    {
        var data = await GetStockAgingReportAsync(filter.CategoryId);
        
        csv.AppendLine("Product Code,Product,Batch No,Days In Stock,Quantity,Age Category");
        foreach (var item in data)
        {
            var batchNo = item.BatchNo ?? "N/A";
            csv.AppendLine($"{item.ProductCode},{item.Product},{batchNo},{item.DaysInStock},{item.Quantity},{item.AgeCategory}");
        }
    }

    private async Task ExportGoodsReturnToCsv(StringBuilder csv, InventoryReportFilterDto filter)
    {
        var data = await GetGoodsReturnReportAsync(filter.StartDate, filter.EndDate);
        
        csv.AppendLine("Date,Return No,Supplier,Product,Qty,Reason");
        foreach (var item in data)
        {
            csv.AppendLine($"{item.Date:yyyy-MM-dd},{item.ReturnNo},{item.Supplier},{item.Product},{item.Qty},{item.Reason}");
        }
    }

    private async Task ExportCategorySummaryToCsv(StringBuilder csv, InventoryReportFilterDto filter)
    {
        var data = await GetInventorySummaryByCategoryAsync();
        
        csv.AppendLine("Category,No of Items,Total Qty,Total Value,Low Stock Items,Out of Stock");
        foreach (var item in data)
        {
            csv.AppendLine($"{item.Category},{item.NoOfItems},{item.TotalQty},{item.TotalValue},{item.LowStockItems},{item.OutOfStockItems}");
        }
    }

    #endregion

    #region PDF Export Helpers

    private void ExportStockSummaryToPdf(XGraphics gfx, ref double yPosition, double leftMargin, double pageWidth, PdfPage page, IEnumerable<StockSummaryDto> data, XFont headerFont, XFont smallFont)
    {
        gfx.DrawString("Stock Summary", headerFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 25;

        // Table Header
        double[] colWidths = { 80, 150, 80, 60, 60, 60, 80 };
        gfx.DrawRectangle(XBrushes.LightBlue, leftMargin, yPosition, pageWidth, 20);
        gfx.DrawString("Code", smallFont, XBrushes.Black, new XRect(leftMargin, yPosition + 5, colWidths[0], 20), XStringFormats.TopLeft);
        gfx.DrawString("Product", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition + 5, colWidths[1], 20), XStringFormats.TopLeft);
        gfx.DrawString("Category", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition + 5, colWidths[2], 20), XStringFormats.TopLeft);
        gfx.DrawString("Opening", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition + 5, colWidths[3], 20), XStringFormats.TopLeft);
        gfx.DrawString("Inward", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition + 5, colWidths[4], 20), XStringFormats.TopLeft);
        gfx.DrawString("Outward", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition + 5, colWidths[5], 20), XStringFormats.TopLeft);
        gfx.DrawString("Closing", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4] + colWidths[5], yPosition + 5, colWidths[6], 20), XStringFormats.TopLeft);
        yPosition += 22;

        foreach (var item in data)
        {
            if (yPosition > page.Height - 80) break;
            
            var code = item.ProductCode.Length > 10 ? item.ProductCode.Substring(0, 10) : item.ProductCode;
            var name = item.ProductName.Length > 20 ? item.ProductName.Substring(0, 17) + "..." : item.ProductName;
            var cat = item.Category.Length > 10 ? item.Category.Substring(0, 10) : item.Category;
            
            gfx.DrawString(code, smallFont, XBrushes.Black, new XRect(leftMargin, yPosition, colWidths[0], 15), XStringFormats.TopLeft);
            gfx.DrawString(name, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition, colWidths[1], 15), XStringFormats.TopLeft);
            gfx.DrawString(cat, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition, colWidths[2], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.OpeningStock.ToString("N0"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition, colWidths[3], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.InwardQty.ToString("N0"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition, colWidths[4], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.OutwardQty.ToString("N0"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition, colWidths[5], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.ClosingStock.ToString("N0"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4] + colWidths[5], yPosition, colWidths[6], 15), XStringFormats.TopLeft);
            yPosition += 16;
        }
    }

    private void ExportStockLedgerToPdf(XGraphics gfx, ref double yPosition, double leftMargin, double pageWidth, PdfPage page, IEnumerable<StockLedgerReportDto> data, XFont headerFont, XFont smallFont)
    {
        gfx.DrawString("Stock Ledger", headerFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 25;

        // Table Header
        double[] colWidths = { 80, 80, 120, 60, 60, 60 };
        gfx.DrawRectangle(XBrushes.LightBlue, leftMargin, yPosition, pageWidth, 20);
        gfx.DrawString("Date", smallFont, XBrushes.Black, new XRect(leftMargin, yPosition + 5, colWidths[0], 20), XStringFormats.TopLeft);
        gfx.DrawString("Ref No", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition + 5, colWidths[1], 20), XStringFormats.TopLeft);
        gfx.DrawString("Type", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition + 5, colWidths[2], 20), XStringFormats.TopLeft);
        gfx.DrawString("In Qty", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition + 5, colWidths[3], 20), XStringFormats.TopLeft);
        gfx.DrawString("Out Qty", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition + 5, colWidths[4], 20), XStringFormats.TopLeft);
        gfx.DrawString("Balance", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition + 5, colWidths[5], 20), XStringFormats.TopLeft);
        yPosition += 22;

        foreach (var item in data)
        {
            if (yPosition > page.Height - 80) break;
            
            gfx.DrawString(item.Date.ToString("dd-MMM-yy"), smallFont, XBrushes.Black, new XRect(leftMargin, yPosition, colWidths[0], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.RefNo, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition, colWidths[1], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.TransactionType.ToString(), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition, colWidths[2], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.InQty.ToString("N0"), smallFont, XBrushes.Green, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition, colWidths[3], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.OutQty.ToString("N0"), smallFont, XBrushes.Red, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition, colWidths[4], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.Balance.ToString("N0"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition, colWidths[5], 15), XStringFormats.TopLeft);
            yPosition += 16;
        }
    }

    private void ExportStockValuationToPdf(XGraphics gfx, ref double yPosition, double leftMargin, double pageWidth, PdfPage page, IEnumerable<StockValuationDto> data, XFont headerFont, XFont smallFont)
    {
        gfx.DrawString("Stock Valuation", headerFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 25;

        // Table Header
        double[] colWidths = { 80, 180, 90, 70, 80, 80 };
        gfx.DrawRectangle(XBrushes.LightBlue, leftMargin, yPosition, pageWidth, 20);
        gfx.DrawString("Code", smallFont, XBrushes.Black, new XRect(leftMargin, yPosition + 5, colWidths[0], 20), XStringFormats.TopLeft);
        gfx.DrawString("Product", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition + 5, colWidths[1], 20), XStringFormats.TopLeft);
        gfx.DrawString("Category", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition + 5, colWidths[2], 20), XStringFormats.TopLeft);
        gfx.DrawString("Quantity", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition + 5, colWidths[3], 20), XStringFormats.TopLeft);
        gfx.DrawString("Unit Cost", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition + 5, colWidths[4], 20), XStringFormats.TopLeft);
        gfx.DrawString("Total Value", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition + 5, colWidths[5], 20), XStringFormats.TopLeft);
        yPosition += 22;

        foreach (var item in data)
        {
            if (yPosition > page.Height - 80) break;
            
            var code = item.ProductCode.Length > 10 ? item.ProductCode.Substring(0, 10) : item.ProductCode;
            var name = item.Product.Length > 25 ? item.Product.Substring(0, 22) + "..." : item.Product;
            var cat = item.Category.Length > 12 ? item.Category.Substring(0, 12) : item.Category;
            
            gfx.DrawString(code, smallFont, XBrushes.Black, new XRect(leftMargin, yPosition, colWidths[0], 15), XStringFormats.TopLeft);
            gfx.DrawString(name, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition, colWidths[1], 15), XStringFormats.TopLeft);
            gfx.DrawString(cat, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition, colWidths[2], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.Quantity.ToString("N0"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition, colWidths[3], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.UnitCost.ToString("N2"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition, colWidths[4], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.TotalValue.ToString("N2"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition, colWidths[5], 15), XStringFormats.TopLeft);
            yPosition += 16;
        }
    }

    private void ExportLowStockToPdf(XGraphics gfx, ref double yPosition, double leftMargin, double pageWidth, PdfPage page, IEnumerable<LowStockDto> data, XFont headerFont, XFont smallFont)
    {
        gfx.DrawString("Low Stock Alert", headerFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 25;

        // Table Header
        double[] colWidths = { 80, 180, 90, 70, 80, 90 };
        gfx.DrawRectangle(XBrushes.LightBlue, leftMargin, yPosition, pageWidth, 20);
        gfx.DrawString("Code", smallFont, XBrushes.Black, new XRect(leftMargin, yPosition + 5, colWidths[0], 20), XStringFormats.TopLeft);
        gfx.DrawString("Product", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition + 5, colWidths[1], 20), XStringFormats.TopLeft);
        gfx.DrawString("Category", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition + 5, colWidths[2], 20), XStringFormats.TopLeft);
        gfx.DrawString("Qty", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition + 5, colWidths[3], 20), XStringFormats.TopLeft);
        gfx.DrawString("Reorder Lvl", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition + 5, colWidths[4], 20), XStringFormats.TopLeft);
        gfx.DrawString("Status", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition + 5, colWidths[5], 20), XStringFormats.TopLeft);
        yPosition += 22;

        foreach (var item in data)
        {
            if (yPosition > page.Height - 80) break;
            
            var code = item.ProductCode.Length > 10 ? item.ProductCode.Substring(0, 10) : item.ProductCode;
            var name = item.Product.Length > 25 ? item.Product.Substring(0, 22) + "..." : item.Product;
            var cat = item.Category.Length > 12 ? item.Category.Substring(0, 12) : item.Category;
            
            var color = item.CurrentQty <= 0 ? XBrushes.DarkRed : XBrushes.OrangeRed;
            
            gfx.DrawString(code, smallFont, XBrushes.Black, new XRect(leftMargin, yPosition, colWidths[0], 15), XStringFormats.TopLeft);
            gfx.DrawString(name, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition, colWidths[1], 15), XStringFormats.TopLeft);
            gfx.DrawString(cat, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition, colWidths[2], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.CurrentQty.ToString("N0"), smallFont, color, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition, colWidths[3], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.ReorderLevel.ToString("N0"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition, colWidths[4], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.StatusText, smallFont, color, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition, colWidths[5], 15), XStringFormats.TopLeft);
            yPosition += 16;
        }
    }

    private void ExportGoodsReceivedToPdf(XGraphics gfx, ref double yPosition, double leftMargin, double pageWidth, PdfPage page, IEnumerable<GoodsReceivedReportDto> data, XFont headerFont, XFont smallFont)
    {
        gfx.DrawString("Goods Received", headerFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 25;

        // Table Header
        double[] colWidths = { 70, 70, 120, 150, 60, 70 };
        gfx.DrawRectangle(XBrushes.LightBlue, leftMargin, yPosition, pageWidth, 20);
        gfx.DrawString("Date", smallFont, XBrushes.Black, new XRect(leftMargin, yPosition + 5, colWidths[0], 20), XStringFormats.TopLeft);
        gfx.DrawString("GRN No", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition + 5, colWidths[1], 20), XStringFormats.TopLeft);
        gfx.DrawString("Supplier", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition + 5, colWidths[2], 20), XStringFormats.TopLeft);
        gfx.DrawString("Product", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition + 5, colWidths[3], 20), XStringFormats.TopLeft);
        gfx.DrawString("Qty", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition + 5, colWidths[4], 20), XStringFormats.TopLeft);
        gfx.DrawString("Total", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition + 5, colWidths[5], 20), XStringFormats.TopLeft);
        yPosition += 22;

        foreach (var item in data)
        {
            if (yPosition > page.Height - 80) break;
            
            var supplier = item.Supplier.Length > 15 ? item.Supplier.Substring(0, 12) + "..." : item.Supplier;
            var product = item.Product.Length > 20 ? item.Product.Substring(0, 17) + "..." : item.Product;
            
            gfx.DrawString(item.Date.ToString("dd-MMM-yy"), smallFont, XBrushes.Black, new XRect(leftMargin, yPosition, colWidths[0], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.GrnNo, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition, colWidths[1], 15), XStringFormats.TopLeft);
            gfx.DrawString(supplier, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition, colWidths[2], 15), XStringFormats.TopLeft);
            gfx.DrawString(product, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition, colWidths[3], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.Qty.ToString("N0"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition, colWidths[4], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.Total.ToString("N2"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition, colWidths[5], 15), XStringFormats.TopLeft);
            yPosition += 16;
        }
    }

    private void ExportStockAdjustmentToPdf(XGraphics gfx, ref double yPosition, double leftMargin, double pageWidth, PdfPage page, IEnumerable<StockAdjustmentReportDto> data, XFont headerFont, XFont smallFont)
    {
        gfx.DrawString("Stock Adjustments", headerFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 25;

        // Table Header
        double[] colWidths = { 70, 90, 180, 80, 70, 100 };
        gfx.DrawRectangle(XBrushes.LightBlue, leftMargin, yPosition, pageWidth, 20);
        gfx.DrawString("Date", smallFont, XBrushes.Black, new XRect(leftMargin, yPosition + 5, colWidths[0], 20), XStringFormats.TopLeft);
        gfx.DrawString("Adj No", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition + 5, colWidths[1], 20), XStringFormats.TopLeft);
        gfx.DrawString("Product", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition + 5, colWidths[2], 20), XStringFormats.TopLeft);
        gfx.DrawString("Previous", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition + 5, colWidths[3], 20), XStringFormats.TopLeft);
        gfx.DrawString("New", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition + 5, colWidths[4], 20), XStringFormats.TopLeft);
        gfx.DrawString("Reason", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition + 5, colWidths[5], 20), XStringFormats.TopLeft);
        yPosition += 22;

        foreach (var item in data)
        {
            if (yPosition > page.Height - 80) break;
            
            var product = item.Product.Length > 25 ? item.Product.Substring(0, 22) + "..." : item.Product;
            var reason = item.Reason.Length > 15 ? item.Reason.Substring(0, 12) + "..." : item.Reason;
            
            gfx.DrawString(item.Date.ToString("dd-MMM-yy"), smallFont, XBrushes.Black, new XRect(leftMargin, yPosition, colWidths[0], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.AdjustmentNo, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition, colWidths[1], 15), XStringFormats.TopLeft);
            gfx.DrawString(product, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition, colWidths[2], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.PreviousQty.ToString("N0"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition, colWidths[3], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.NewQty.ToString("N0"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition, colWidths[4], 15), XStringFormats.TopLeft);
            gfx.DrawString(reason, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition, colWidths[5], 15), XStringFormats.TopLeft);
            yPosition += 16;
        }
    }

    private void ExportStockTransferToPdf(XGraphics gfx, ref double yPosition, double leftMargin, double pageWidth, PdfPage page, IEnumerable<StockTransferReportDto> data, XFont headerFont, XFont smallFont)
    {
        gfx.DrawString("Stock Transfers", headerFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 25;

        // Table Header
        double[] colWidths = { 70, 90, 150, 90, 90, 60 };
        gfx.DrawRectangle(XBrushes.LightBlue, leftMargin, yPosition, pageWidth, 20);
        gfx.DrawString("Date", smallFont, XBrushes.Black, new XRect(leftMargin, yPosition + 5, colWidths[0], 20), XStringFormats.TopLeft);
        gfx.DrawString("Transfer No", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition + 5, colWidths[1], 20), XStringFormats.TopLeft);
        gfx.DrawString("Product", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition + 5, colWidths[2], 20), XStringFormats.TopLeft);
        gfx.DrawString("From", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition + 5, colWidths[3], 20), XStringFormats.TopLeft);
        gfx.DrawString("To", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition + 5, colWidths[4], 20), XStringFormats.TopLeft);
        gfx.DrawString("Qty", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition + 5, colWidths[5], 20), XStringFormats.TopLeft);
        yPosition += 22;

        foreach (var item in data)
        {
            if (yPosition > page.Height - 80) break;
            
            var product = item.Product.Length > 20 ? item.Product.Substring(0, 17) + "..." : item.Product;
            var from = item.FromLocation.Length > 12 ? item.FromLocation.Substring(0, 10) + "..." : item.FromLocation;
            var to = item.ToLocation.Length > 12 ? item.ToLocation.Substring(0, 10) + "..." : item.ToLocation;
            
            gfx.DrawString(item.Date.ToString("dd-MMM-yy"), smallFont, XBrushes.Black, new XRect(leftMargin, yPosition, colWidths[0], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.TransferNo, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition, colWidths[1], 15), XStringFormats.TopLeft);
            gfx.DrawString(product, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition, colWidths[2], 15), XStringFormats.TopLeft);
            gfx.DrawString(from, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition, colWidths[3], 15), XStringFormats.TopLeft);
            gfx.DrawString(to, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition, colWidths[4], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.Qty.ToString("N0"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition, colWidths[5], 15), XStringFormats.TopLeft);
            yPosition += 16;
        }
    }

    private void ExportStockAgingToPdf(XGraphics gfx, ref double yPosition, double leftMargin, double pageWidth, PdfPage page, IEnumerable<StockAgingDto> data, XFont headerFont, XFont smallFont)
    {
        gfx.DrawString("Stock Aging", headerFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 25;

        // Table Header
        double[] colWidths = { 80, 180, 80, 70, 70, 100 };
        gfx.DrawRectangle(XBrushes.LightBlue, leftMargin, yPosition, pageWidth, 20);
        gfx.DrawString("Code", smallFont, XBrushes.Black, new XRect(leftMargin, yPosition + 5, colWidths[0], 20), XStringFormats.TopLeft);
        gfx.DrawString("Product", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition + 5, colWidths[1], 20), XStringFormats.TopLeft);
        gfx.DrawString("Days", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition + 5, colWidths[2], 20), XStringFormats.TopLeft);
        gfx.DrawString("Quantity", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition + 5, colWidths[3], 20), XStringFormats.TopLeft);
        gfx.DrawString("Value", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition + 5, colWidths[4], 20), XStringFormats.TopLeft);
        gfx.DrawString("Category", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition + 5, colWidths[5], 20), XStringFormats.TopLeft);
        yPosition += 22;

        foreach (var item in data)
        {
            if (yPosition > page.Height - 80) break;
            
            var code = item.ProductCode.Length > 10 ? item.ProductCode.Substring(0, 10) : item.ProductCode;
            var name = item.Product.Length > 25 ? item.Product.Substring(0, 22) + "..." : item.Product;
            
            var color = item.DaysInStock > 180 ? XBrushes.Red : item.DaysInStock > 90 ? XBrushes.OrangeRed : XBrushes.Black;
            
            gfx.DrawString(code, smallFont, XBrushes.Black, new XRect(leftMargin, yPosition, colWidths[0], 15), XStringFormats.TopLeft);
            gfx.DrawString(name, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition, colWidths[1], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.DaysInStock.ToString(), smallFont, color, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition, colWidths[2], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.Quantity.ToString("N0"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition, colWidths[3], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.TotalValue.ToString("N2"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition, colWidths[4], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.AgeCategory, smallFont, color, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition, colWidths[5], 15), XStringFormats.TopLeft);
            yPosition += 16;
        }
    }

    private void ExportGoodsReturnToPdf(XGraphics gfx, ref double yPosition, double leftMargin, double pageWidth, PdfPage page, IEnumerable<GoodsReturnReportDto> data, XFont headerFont, XFont smallFont)
    {
        gfx.DrawString("Goods Returns", headerFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 25;

        // Table Header
        double[] colWidths = { 70, 80, 120, 150, 60, 110 };
        gfx.DrawRectangle(XBrushes.LightBlue, leftMargin, yPosition, pageWidth, 20);
        gfx.DrawString("Date", smallFont, XBrushes.Black, new XRect(leftMargin, yPosition + 5, colWidths[0], 20), XStringFormats.TopLeft);
        gfx.DrawString("Return No", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition + 5, colWidths[1], 20), XStringFormats.TopLeft);
        gfx.DrawString("Supplier", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition + 5, colWidths[2], 20), XStringFormats.TopLeft);
        gfx.DrawString("Product", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition + 5, colWidths[3], 20), XStringFormats.TopLeft);
        gfx.DrawString("Qty", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition + 5, colWidths[4], 20), XStringFormats.TopLeft);
        gfx.DrawString("Reason", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition + 5, colWidths[5], 20), XStringFormats.TopLeft);
        yPosition += 22;

        foreach (var item in data)
        {
            if (yPosition > page.Height - 80) break;
            
            var supplier = item.Supplier.Length > 15 ? item.Supplier.Substring(0, 12) + "..." : item.Supplier;
            var product = item.Product.Length > 20 ? item.Product.Substring(0, 17) + "..." : item.Product;
            var reason = item.Reason.Length > 15 ? item.Reason.Substring(0, 12) + "..." : item.Reason;
            
            gfx.DrawString(item.Date.ToString("dd-MMM-yy"), smallFont, XBrushes.Black, new XRect(leftMargin, yPosition, colWidths[0], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.ReturnNo, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition, colWidths[1], 15), XStringFormats.TopLeft);
            gfx.DrawString(supplier, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition, colWidths[2], 15), XStringFormats.TopLeft);
            gfx.DrawString(product, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition, colWidths[3], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.Qty.ToString("N0"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition, colWidths[4], 15), XStringFormats.TopLeft);
            gfx.DrawString(reason, smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition, colWidths[5], 15), XStringFormats.TopLeft);
            yPosition += 16;
        }
    }

    private void ExportCategorySummaryToPdf(XGraphics gfx, ref double yPosition, double leftMargin, double pageWidth, PdfPage page, IEnumerable<InventorySummaryDto> data, XFont headerFont, XFont smallFont)
    {
        gfx.DrawString("Category Summary", headerFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 25;

        // Table Header
        double[] colWidths = { 150, 70, 80, 90, 90, 90 };
        gfx.DrawRectangle(XBrushes.LightBlue, leftMargin, yPosition, pageWidth, 20);
        gfx.DrawString("Category", smallFont, XBrushes.Black, new XRect(leftMargin, yPosition + 5, colWidths[0], 20), XStringFormats.TopLeft);
        gfx.DrawString("Items", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition + 5, colWidths[1], 20), XStringFormats.TopLeft);
        gfx.DrawString("Total Qty", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition + 5, colWidths[2], 20), XStringFormats.TopLeft);
        gfx.DrawString("Total Value", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition + 5, colWidths[3], 20), XStringFormats.TopLeft);
        gfx.DrawString("Low Stock", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition + 5, colWidths[4], 20), XStringFormats.TopLeft);
        gfx.DrawString("Out Stock", smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition + 5, colWidths[5], 20), XStringFormats.TopLeft);
        yPosition += 22;

        foreach (var item in data)
        {
            if (yPosition > page.Height - 80) break;
            
            var cat = item.Category.Length > 20 ? item.Category.Substring(0, 17) + "..." : item.Category;
            
            gfx.DrawString(cat, smallFont, XBrushes.Black, new XRect(leftMargin, yPosition, colWidths[0], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.NoOfItems.ToString(), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0], yPosition, colWidths[1], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.TotalQty.ToString("N0"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1], yPosition, colWidths[2], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.TotalValue.ToString("N2"), smallFont, XBrushes.Black, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2], yPosition, colWidths[3], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.LowStockItems.ToString(), smallFont, XBrushes.OrangeRed, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3], yPosition, colWidths[4], 15), XStringFormats.TopLeft);
            gfx.DrawString(item.OutOfStockItems.ToString(), smallFont, XBrushes.Red, new XRect(leftMargin + colWidths[0] + colWidths[1] + colWidths[2] + colWidths[3] + colWidths[4], yPosition, colWidths[5], 15), XStringFormats.TopLeft);
            yPosition += 16;
        }
    }

    #endregion
}
