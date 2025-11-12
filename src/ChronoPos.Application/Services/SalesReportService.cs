using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Logging;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ClosedXML.Excel;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Sales Report operations
/// </summary>
public class SalesReportService : ISalesReportService
{
    private readonly IChronoPosDbContext _context;

    public SalesReportService(IChronoPosDbContext context)
    {
        _context = context;
    }

    public async Task<SalesReportDto> GenerateSalesReportAsync(SalesReportFilterDto filter)
    {
        try
        {
            AppLogger.LogInfo("Generating sales report with filters", filename: "sales_report");
            
            var startDate = filter.StartDate ?? DateTime.Today;
            var endDate = filter.EndDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

            AppLogger.LogInfo($"Report date range: {startDate:yyyy-MM-dd HH:mm:ss} to {endDate:yyyy-MM-dd HH:mm:ss}", filename: "sales_report");

            var report = new SalesReportDto
            {
                Summary = await GetSalesSummaryAsync(startDate, endDate),
                TopProducts = await GetTopProductsAsync(startDate, endDate, 10),
                CategoryPerformance = await GetCategoryPerformanceAsync(startDate, endDate),
                PaymentBreakdown = await GetPaymentMethodBreakdownAsync(startDate, endDate),
                DailyTrend = await GetDailySalesTrendAsync(startDate, endDate),
                HourlyDistribution = await GetHourlySalesDistributionAsync(startDate)
            };

            var (transactions, totalCount) = await GetSalesTransactionsAsync(filter);
            report.Transactions = transactions;
            report.TotalRecords = totalCount;

            AppLogger.LogInfo($"Sales report generated successfully with {totalCount} transactions", filename: "sales_report");
            return report;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error generating sales report: {ex.Message}", ex, filename: "sales_report");
            throw;
        }
    }

    public async Task<SalesSummaryDto> GetSalesSummaryAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            AppLogger.LogInfo($"Getting sales summary from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

            var transactions = await _context.Transactions
                .Where(t => t.SellingTime >= startDate && t.SellingTime <= endDate)
                .Include(t => t.TransactionProducts)
                .ThenInclude(tp => tp.Product)
                .ToListAsync();

            AppLogger.LogInfo($"Retrieved {transactions.Count} transaction records");

            // Include settled, billed, pending_payment, and partial_payment - exclude only draft, hold, and cancelled
            var completedTransactions = transactions.Where(t => 
                t.Status == "settled" || 
                t.Status == "billed" ||
                t.Status == "pending_payment" ||
                t.Status == "partial_payment").ToList();
            var cancelledTransactions = transactions.Where(t => t.Status == "cancelled").ToList();

            AppLogger.LogInfo($"Completed transactions (Settled/Billed/Pending/Partial): {completedTransactions.Count}, Cancelled: {cancelledTransactions.Count}");

            var totalItemsSold = (int)completedTransactions.SelectMany(t => t.TransactionProducts).Sum(tp => tp.Quantity);
            var grossProfit = completedTransactions.SelectMany(t => t.TransactionProducts)
                .Sum(tp => (tp.SellingPrice - (tp.Product?.Cost ?? 0)) * tp.Quantity);

            var summary = new SalesSummaryDto
            {
                TotalSalesAmount = completedTransactions.Sum(t => t.TotalAmount),
                TotalTransactions = completedTransactions.Count,
                AverageTransactionValue = completedTransactions.Any() 
                    ? completedTransactions.Average(t => t.TotalAmount) 
                    : 0,
                TotalItemsSold = totalItemsSold,
                TotalDiscountGiven = completedTransactions.Sum(t => t.TotalDiscount),
                TotalTaxCollected = completedTransactions.Sum(t => t.TotalVat),
                GrossProfit = grossProfit,
                NetRevenue = completedTransactions.Sum(t => t.TotalAmount),
                RefundAmount = 0, // Refunds tracked separately in transaction system
                RefundCount = 0,
                StartDate = startDate,
                EndDate = endDate
            };

            AppLogger.LogInfo($"Summary calculated: {completedTransactions.Count} completed transactions, Total: ${summary.TotalSalesAmount:F2}");
            return summary;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error getting sales summary: {ex.Message}", ex, filename: "sales_report");
            throw;
        }
    }

    public async Task<List<ProductPerformanceDto>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int topCount = 10)
    {
        try
        {
            AppLogger.LogInfo($"Getting top {topCount} products from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

            // First, get all relevant transaction products with their related data
            var transactionProducts = await _context.TransactionProducts
                .Include(tp => tp.Transaction)
                .Include(tp => tp.Product)
                .ThenInclude(p => p.Category)
                .Where(tp => tp.Transaction.SellingTime >= startDate 
                          && tp.Transaction.SellingTime <= endDate
                          && (tp.Transaction.Status == "settled" || tp.Transaction.Status == "billed" || 
                              tp.Transaction.Status == "pending_payment" || tp.Transaction.Status == "partial_payment"))
                .ToListAsync();

            AppLogger.LogInfo($"Retrieved {transactionProducts.Count} transaction products");

            // Group and aggregate in memory to avoid EF Core translation issues
            var productPerformance = transactionProducts
                .GroupBy(tp => tp.ProductId)
                .Select(g =>
                {
                    var firstItem = g.First();
                    var product = firstItem.Product;
                    var categoryName = product?.Category?.Name ?? "Uncategorized";
                    
                    return new ProductPerformanceDto
                    {
                        ProductId = g.Key,
                        ProductName = product?.Name ?? "Unknown",
                        SKU = product?.SKU ?? "",
                        CategoryName = categoryName,
                        QuantitySold = (int)g.Sum(tp => tp.Quantity),
                        TotalRevenue = g.Sum(tp => tp.LineTotal),
                        TotalCost = g.Sum(tp => (product?.Cost ?? 0) * tp.Quantity),
                        TransactionCount = g.Select(tp => tp.TransactionId).Distinct().Count(),
                        AveragePrice = g.Average(tp => tp.SellingPrice)
                    };
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(topCount)
                .ToList();

            // Calculate derived fields
            foreach (var product in productPerformance)
            {
                product.GrossProfit = product.TotalRevenue - product.TotalCost;
                product.ProfitMargin = product.TotalRevenue > 0 
                    ? (product.GrossProfit / product.TotalRevenue) * 100 
                    : 0;
            }

            AppLogger.LogInfo($"Calculated performance for {productPerformance.Count} products");
            return productPerformance;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error getting top products: {ex.Message}", ex, filename: "sales_report");
            throw;
        }
    }

    public async Task<List<CategoryPerformanceDto>> GetCategoryPerformanceAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            AppLogger.LogInfo($"Getting category performance from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

            // Get all relevant transaction products with their related data
            var transactionProducts = await _context.TransactionProducts
                .Include(tp => tp.Transaction)
                .Include(tp => tp.Product)
                .ThenInclude(p => p.Category)
                .Where(tp => tp.Transaction.SellingTime >= startDate 
                          && tp.Transaction.SellingTime <= endDate
                          && (tp.Transaction.Status == "settled" || tp.Transaction.Status == "billed" ||
                              tp.Transaction.Status == "pending_payment" || tp.Transaction.Status == "partial_payment"))
                .ToListAsync();

            AppLogger.LogInfo($"Retrieved {transactionProducts.Count} transaction products for category performance");

            // Group and aggregate in memory
            var categoryPerformance = transactionProducts
                .GroupBy(tp => new 
                { 
                    CategoryId = tp.Product?.CategoryId,
                    CategoryName = tp.Product?.Category?.Name ?? "Uncategorized"
                })
                .Select(g => new CategoryPerformanceDto
                {
                    CategoryId = g.Key.CategoryId ?? 0,
                    CategoryName = g.Key.CategoryName,
                    ProductCount = g.Select(tp => tp.ProductId).Distinct().Count(),
                    QuantitySold = (int)g.Sum(tp => tp.Quantity),
                    TotalRevenue = g.Sum(tp => tp.LineTotal),
                    TotalCost = g.Sum(tp => (tp.Product?.Cost ?? 0) * tp.Quantity),
                    TransactionCount = g.Select(tp => tp.TransactionId).Distinct().Count()
                })
                .OrderByDescending(c => c.TotalRevenue)
                .ToList();

            var totalRevenue = categoryPerformance.Sum(c => c.TotalRevenue);

            // Calculate derived fields
            foreach (var category in categoryPerformance)
            {
                category.GrossProfit = category.TotalRevenue - category.TotalCost;
                category.ProfitMargin = category.TotalRevenue > 0 
                    ? (category.GrossProfit / category.TotalRevenue) * 100 
                    : 0;
                category.RevenuePercentage = totalRevenue > 0 
                    ? (category.TotalRevenue / totalRevenue) * 100 
                    : 0;
            }

            AppLogger.LogInfo($"Calculated performance for {categoryPerformance.Count} categories");
            return categoryPerformance;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error getting category performance: {ex.Message}", ex, filename: "sales_report");
            throw;
        }
    }

    public async Task<List<PaymentMethodBreakdownDto>> GetPaymentMethodBreakdownAsync(DateTime startDate, DateTime endDate)
    {
        // Transaction doesn't have PaymentMethod enum, but has AmountPaidCash and AmountCreditRemaining
        // Simplify to Cash vs Credit breakdown
        var transactions = await _context.Transactions
            .Where(t => t.SellingTime >= startDate 
                     && t.SellingTime <= endDate
                     && (t.Status == "settled" || t.Status == "billed" || 
                         t.Status == "pending_payment" || t.Status == "partial_payment"))
            .ToListAsync();

        var cashTransactions = transactions.Where(t => t.AmountPaidCash > 0).ToList();
        var creditTransactions = transactions.Where(t => t.AmountCreditRemaining > 0).ToList();
        
        var paymentBreakdown = new List<PaymentMethodBreakdownDto>();
        
        if (cashTransactions.Any())
        {
            paymentBreakdown.Add(new PaymentMethodBreakdownDto
            {
                PaymentMethod = PaymentMethod.Cash,
                PaymentMethodName = "Cash",
                TransactionCount = cashTransactions.Count,
                TotalAmount = cashTransactions.Sum(t => t.AmountPaidCash),
                AverageTransactionValue = cashTransactions.Average(t => t.AmountPaidCash)
            });
        }
        
        if (creditTransactions.Any())
        {
            paymentBreakdown.Add(new PaymentMethodBreakdownDto
            {
                PaymentMethod = PaymentMethod.CreditCard, // Using CreditCard for credit
                PaymentMethodName = "Credit",
                TransactionCount = creditTransactions.Count,
                TotalAmount = creditTransactions.Sum(t => t.AmountCreditRemaining),
                AverageTransactionValue = creditTransactions.Average(t => t.AmountCreditRemaining)
            });
        }

        var totalAmount = paymentBreakdown.Sum(p => p.TotalAmount);

        // Calculate percentage
        foreach (var payment in paymentBreakdown)
        {
            payment.Percentage = totalAmount > 0 
                ? (payment.TotalAmount / totalAmount) * 100 
                : 0;
        }

        return paymentBreakdown.OrderByDescending(p => p.TotalAmount).ToList();
    }

    public async Task<List<DailySalesDto>> GetDailySalesTrendAsync(DateTime startDate, DateTime endDate)
    {
        var dailySales = await _context.Transactions
            .Where(t => t.SellingTime >= startDate 
                     && t.SellingTime <= endDate
                     && (t.Status == "settled" || t.Status == "billed" || 
                         t.Status == "pending_payment" || t.Status == "partial_payment"))
            .Include(t => t.TransactionProducts)
            .GroupBy(t => t.SellingTime.Date)
            .Select(g => new DailySalesDto
            {
                Date = g.Key,
                DateLabel = g.Key.ToString("MMM dd"),
                TotalSales = g.Sum(t => t.TotalAmount),
                TransactionCount = g.Count(),
                ItemsSold = (int)g.SelectMany(t => t.TransactionProducts).Sum(tp => tp.Quantity),
                AverageValue = g.Average(t => t.TotalAmount)
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        return dailySales;
    }

    public async Task<List<HourlySalesDto>> GetHourlySalesDistributionAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddSeconds(-1);

        var hourlySales = await _context.Transactions
            .Where(t => t.SellingTime >= startOfDay 
                     && t.SellingTime <= endOfDay
                     && (t.Status == "settled" || t.Status == "billed" || 
                         t.Status == "pending_payment" || t.Status == "partial_payment"))
            .GroupBy(t => t.SellingTime.Hour)
            .Select(g => new HourlySalesDto
            {
                Hour = g.Key,
                Sales = g.Sum(t => t.TotalAmount),
                TransactionCount = g.Count()
            })
            .ToListAsync();

        // Fill in missing hours with zero values
        var allHours = Enumerable.Range(0, 24).Select(hour =>
        {
            var existing = hourlySales.FirstOrDefault(h => h.Hour == hour);
            return existing ?? new HourlySalesDto
            {
                Hour = hour,
                HourLabel = DateTime.Today.AddHours(hour).ToString("hh:mm tt"),
                Sales = 0,
                TransactionCount = 0
            };
        }).ToList();

        // Set hour labels
        foreach (var hourData in allHours)
        {
            hourData.HourLabel = DateTime.Today.AddHours(hourData.Hour).ToString("hh:mm tt");
        }

        return allHours;
    }

    public async Task<List<TopCustomerDto>> GetTopCustomersAsync(DateTime startDate, DateTime endDate, int topCount = 10)
    {
        var topCustomers = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
            .Where(s => s.SaleDate >= startDate 
                     && s.SaleDate <= endDate
                     && s.Status == SaleStatus.Settled
                     && s.CustomerId != null)
            .GroupBy(s => new 
            { 
                s.CustomerId, 
                s.Customer!.CustomerFullName,
                s.Customer.MobileNo
            })
            .Select(g => new TopCustomerDto
            {
                CustomerId = g.Key.CustomerId!.Value,
                CustomerName = g.Key.CustomerFullName,
                PhoneNumber = g.Key.MobileNo,
                TransactionCount = g.Count(),
                TotalSpent = g.Sum(s => s.TotalAmount),
                AverageTransactionValue = g.Average(s => s.TotalAmount),
                LastTransactionDate = g.Max(s => s.SaleDate)
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(topCount)
            .ToListAsync();

        return topCustomers;
    }

    public async Task<(List<SaleTransactionDto> Transactions, int TotalCount)> GetSalesTransactionsAsync(SalesReportFilterDto filter)
    {
        var query = _context.Transactions
            .Include(t => t.Customer)
            .Include(t => t.TransactionProducts)
            .AsQueryable();

        // Apply filters
        if (filter.StartDate.HasValue)
            query = query.Where(t => t.SellingTime >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(t => t.SellingTime <= filter.EndDate.Value);

        if (filter.CustomerId.HasValue)
            query = query.Where(t => t.CustomerId == filter.CustomerId.Value);

        // Note: Transaction doesn't have PaymentMethod enum, it uses AmountPaidCash/AmountCreditRemaining
        // Skip payment method filter for now
        
        if (filter.Status.HasValue)
        {
            // Map SaleStatus enum to Transaction status strings
            var statusString = filter.Status.Value switch
            {
                SaleStatus.Draft => "draft",
                SaleStatus.Billed => "billed",
                SaleStatus.Settled => "settled",
                SaleStatus.Hold => "hold",
                SaleStatus.Cancelled => "cancelled",
                SaleStatus.PendingPayment => "pending_payment",
                SaleStatus.PartialPayment => "partial_payment",
                _ => "settled"
            };
            query = query.Where(t => t.Status == statusString);
        }
        else
            // When no specific status is filtered, include only completed sales
            query = query.Where(t => 
                t.Status == "settled" || 
                t.Status == "billed" ||
                t.Status == "pending_payment" ||
                t.Status == "partial_payment");

        if (filter.MinimumAmount.HasValue)
            query = query.Where(t => t.TotalAmount >= filter.MinimumAmount.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.TransactionProducts.Any(tp => tp.Product.CategoryId == filter.CategoryId.Value));

        if (filter.ProductId.HasValue)
            query = query.Where(t => t.TransactionProducts.Any(tp => tp.ProductId == filter.ProductId.Value));

        var totalCount = await query.CountAsync();

        // Apply sorting
        query = filter.SortBy.ToLower() switch
        {
            "saledate" => filter.SortDescending 
                ? query.OrderByDescending(t => t.SellingTime) 
                : query.OrderBy(t => t.SellingTime),
            "totalamount" => filter.SortDescending 
                ? query.OrderByDescending(t => t.TotalAmount) 
                : query.OrderBy(t => t.TotalAmount),
            "customer" => filter.SortDescending 
                ? query.OrderByDescending(t => t.Customer!.CustomerFullName) 
                : query.OrderBy(t => t.Customer!.CustomerFullName),
            _ => query.OrderByDescending(t => t.SellingTime)
        };

        // Apply pagination
        var transactions = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // Map to DTOs after retrieving from database
        var transactionDtos = transactions.Select(t => new SaleTransactionDto
        {
            Id = t.Id,
            InvoiceNumber = t.InvoiceNumber ?? "",
            SaleDate = t.SellingTime,
            CustomerName = t.Customer != null ? t.Customer.CustomerFullName : "Walk-in Customer",
            ItemsCount = t.TransactionProducts.Count,
            SubTotal = t.TotalAmount - t.TotalVat + t.TotalDiscount,
            DiscountAmount = t.TotalDiscount,
            TaxAmount = t.TotalVat,
            TotalAmount = t.TotalAmount,
            PaymentMethod = t.AmountPaidCash > 0 ? PaymentMethod.Cash : PaymentMethod.CreditCard,
            PaymentMethodName = t.AmountPaidCash > 0 ? "Cash" : "Credit",
            Status = MapStatusToEnum(t.Status),
            StatusName = t.StatusDisplay,
            Notes = t.DiscountNote ?? ""
        }).ToList();

        return (transactionDtos, totalCount);
    }

    public async Task<byte[]> ExportToExcelAsync(SalesReportFilterDto filter)
    {
        // For exports, ensure we get all records by setting page number to 1 and page size to max
        filter.PageNumber = 1;
        filter.PageSize = int.MaxValue;
        
        AppLogger.LogInfo($"ExportToExcelAsync called with date range: {filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}", filename: "sales_report");
        
        var report = await GenerateSalesReportAsync(filter);
        var currencySymbol = filter.CurrencySymbol ?? "€";
        
        AppLogger.LogInfo($"Exporting {report.Transactions?.Count ?? 0} transactions to Excel", filename: "sales_report");
        
        if (report.Transactions == null || report.Transactions.Count == 0)
        {
            AppLogger.LogWarning("No transactions found in report.Transactions collection!", filename: "sales_report");
        }
        else
        {
            AppLogger.LogInfo($"First transaction: {report.Transactions[0].InvoiceNumber} - {report.Transactions[0].CustomerName}", filename: "sales_report");
        }
        
        using var workbook = new XLWorkbook();
        
        // Summary Sheet
        var summarySheet = workbook.Worksheets.Add("Summary");
        summarySheet.Cell("A1").Value = "Sales Report Summary";
        summarySheet.Cell("A1").Style.Font.FontSize = 16;
        summarySheet.Cell("A1").Style.Font.Bold = true;
        
        summarySheet.Cell("A3").Value = "Period";
        summarySheet.Cell("B3").Value = $"{filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}";
        
        summarySheet.Cell("A5").Value = "Total Sales";
        summarySheet.Cell("B5").Value = report.Summary.TotalSalesAmount;
        summarySheet.Cell("B5").Style.NumberFormat.Format = $"\"{currencySymbol}\"#,##0.00";
        
        summarySheet.Cell("A6").Value = "Total Transactions";
        summarySheet.Cell("B6").Value = report.Summary.TotalTransactions;
        
        summarySheet.Cell("A7").Value = "Average Transaction";
        summarySheet.Cell("B7").Value = report.Summary.AverageTransactionValue;
        summarySheet.Cell("B7").Style.NumberFormat.Format = $"\"{currencySymbol}\"#,##0.00";
        
        summarySheet.Cell("A8").Value = "Items Sold";
        summarySheet.Cell("B8").Value = report.Summary.TotalItemsSold;
        
        summarySheet.Cell("A9").Value = "Gross Profit";
        summarySheet.Cell("B9").Value = report.Summary.GrossProfit;
        summarySheet.Cell("B9").Style.NumberFormat.Format = $"\"{currencySymbol}\"#,##0.00";
        
        // Transactions Sheet
        var transSheet = workbook.Worksheets.Add("Transactions");
        transSheet.Cell("A1").Value = "Invoice #";
        transSheet.Cell("B1").Value = "Date";
        transSheet.Cell("C1").Value = "Customer";
        transSheet.Cell("D1").Value = "Items";
        transSheet.Cell("E1").Value = "Subtotal";
        transSheet.Cell("F1").Value = "Discount";
        transSheet.Cell("G1").Value = "Tax";
        transSheet.Cell("H1").Value = "Total";
        transSheet.Cell("I1").Value = "Payment";
        transSheet.Cell("J1").Value = "Status";
        
        transSheet.Range("A1:J1").Style.Font.Bold = true;
        transSheet.Range("A1:J1").Style.Fill.BackgroundColor = XLColor.LightBlue;
        
        AppLogger.LogInfo($"Starting to write {report.Transactions?.Count ?? 0} transactions to Excel", filename: "sales_report");
        
        if (report.Transactions != null && report.Transactions.Count > 0)
        {
            int row = 2;
            foreach (var trans in report.Transactions)
            {
                try
                {
                    transSheet.Cell(row, 1).Value = trans.InvoiceNumber ?? "";
                    transSheet.Cell(row, 2).Value = trans.SaleDate.ToString("yyyy-MM-dd HH:mm");
                    transSheet.Cell(row, 3).Value = trans.CustomerName ?? "";
                    transSheet.Cell(row, 4).Value = trans.ItemsCount;
                    transSheet.Cell(row, 5).Value = trans.SubTotal;
                    transSheet.Cell(row, 5).Style.NumberFormat.Format = $"\"{currencySymbol}\"#,##0.00";
                    transSheet.Cell(row, 6).Value = trans.DiscountAmount;
                    transSheet.Cell(row, 6).Style.NumberFormat.Format = $"\"{currencySymbol}\"#,##0.00";
                    transSheet.Cell(row, 7).Value = trans.TaxAmount;
                    transSheet.Cell(row, 7).Style.NumberFormat.Format = $"\"{currencySymbol}\"#,##0.00";
                    transSheet.Cell(row, 8).Value = trans.TotalAmount;
                    transSheet.Cell(row, 8).Style.NumberFormat.Format = $"\"{currencySymbol}\"#,##0.00";
                    transSheet.Cell(row, 9).Value = trans.PaymentMethodName ?? "";
                    transSheet.Cell(row, 10).Value = trans.StatusName ?? "";
                    row++;
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"Error writing transaction row {row}: {ex.Message}", ex, filename: "sales_report");
                }
            }
            AppLogger.LogInfo($"Finished writing {row - 2} transactions to Excel", filename: "sales_report");
        }
        else
        {
            AppLogger.LogWarning("No transactions to write to Excel!", filename: "sales_report");
        }
        
        transSheet.Columns().AdjustToContents();
        summarySheet.Columns().AdjustToContents();
        
        // Reorder worksheets - Transactions first, Summary second
        workbook.Worksheet("Transactions").Position = 1;
        workbook.Worksheet("Summary").Position = 2;
        
        AppLogger.LogInfo($"Workbook has {workbook.Worksheets.Count} worksheets: {string.Join(", ", workbook.Worksheets.Select(w => w.Name))}", filename: "sales_report");
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportToCsvAsync(SalesReportFilterDto filter)
    {
        // For exports, ensure we get all records
        filter.PageNumber = 1;
        filter.PageSize = int.MaxValue;
        
        var (transactions, _) = await GetSalesTransactionsAsync(filter);
        
        AppLogger.LogInfo($"Exporting {transactions.Count} transactions to CSV", filename: "sales_report");
        
        var csv = new StringBuilder();
        csv.AppendLine("Invoice Number,Date,Customer,Items,Subtotal,Discount,Tax,Total,Payment Method,Status");
        
        foreach (var transaction in transactions)
        {
            csv.AppendLine($"{transaction.InvoiceNumber}," +
                          $"{transaction.SaleDate:yyyy-MM-dd HH:mm}," +
                          $"{transaction.CustomerName}," +
                          $"{transaction.ItemsCount}," +
                          $"{transaction.SubTotal:F2}," +
                          $"{transaction.DiscountAmount:F2}," +
                          $"{transaction.TaxAmount:F2}," +
                          $"{transaction.TotalAmount:F2}," +
                          $"{transaction.PaymentMethodName}," +
                          $"{transaction.StatusName}");
        }
        
        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportToPdfAsync(SalesReportFilterDto filter)
    {
        // For exports, ensure we get all records
        filter.PageNumber = 1;
        filter.PageSize = int.MaxValue;
        
        var report = await GenerateSalesReportAsync(filter);
        var currencySymbol = filter.CurrencySymbol ?? "€";
        
        AppLogger.LogInfo($"Exporting {report.Transactions.Count} transactions to PDF", filename: "sales_report");

        var document = new PdfDocument();
        document.Info.Title = "Sales Report";
        document.Info.Subject = $"Sales Report from {filter.StartDate:dd-MMM-yyyy} to {filter.EndDate:dd-MMM-yyyy}";
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
        gfx.DrawString("Sales Report", titleFont, XBrushes.DarkBlue, new XRect(leftMargin, yPosition, pageWidth, 30), XStringFormats.TopLeft);
        yPosition += 40;

        // Report Period
        gfx.DrawString($"Period: {filter.StartDate:dd-MMM-yyyy} to {filter.EndDate:dd-MMM-yyyy}", normalFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 20;
        gfx.DrawString($"Generated: {DateTime.Now:dd-MMM-yyyy HH:mm}", normalFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 40;

        // Summary Section
        gfx.DrawString("Summary", headerFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 25;

        var summaryData = new[]
        {
            ("Total Sales", $"{currencySymbol}{report.Summary.TotalSalesAmount:N2}"),
            ("Total Transactions", report.Summary.TotalTransactions.ToString()),
            ("Average Transaction", $"{currencySymbol}{report.Summary.AverageTransactionValue:N2}"),
            ("Items Sold", report.Summary.TotalItemsSold.ToString()),
            ("Gross Profit", $"{currencySymbol}{report.Summary.GrossProfit:N2}")
        };

        foreach (var (label, value) in summaryData)
        {
            gfx.DrawString(label + ":", normalFont, XBrushes.Black, new XRect(leftMargin, yPosition, 200, 20), XStringFormats.TopLeft);
            gfx.DrawString(value, normalFont, XBrushes.Black, new XRect(leftMargin + 200, yPosition, 200, 20), XStringFormats.TopLeft);
            yPosition += 20;
        }

        yPosition += 20;

        // Transactions Section
        gfx.DrawString("Recent Transactions", headerFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 25;

        // Table Header
        double colWidth = pageWidth / 5;
        gfx.DrawRectangle(XBrushes.LightBlue, leftMargin, yPosition, pageWidth, 20);
        gfx.DrawString("Date", normalFont, XBrushes.Black, new XRect(leftMargin, yPosition, colWidth, 20), XStringFormats.TopLeft);
        gfx.DrawString("Invoice", normalFont, XBrushes.Black, new XRect(leftMargin + colWidth, yPosition, colWidth, 20), XStringFormats.TopLeft);
        gfx.DrawString("Customer", normalFont, XBrushes.Black, new XRect(leftMargin + colWidth * 2, yPosition, colWidth, 20), XStringFormats.TopLeft);
        gfx.DrawString("Amount", normalFont, XBrushes.Black, new XRect(leftMargin + colWidth * 3, yPosition, colWidth, 20), XStringFormats.TopLeft);
        gfx.DrawString("Status", normalFont, XBrushes.Black, new XRect(leftMargin + colWidth * 4, yPosition, colWidth, 20), XStringFormats.TopLeft);
        yPosition += 22;

        // Table Rows (limit to 20 transactions to fit on page)
        int count = 0;
        foreach (var transaction in report.Transactions.Take(20))
        {
            if (yPosition > page.Height - 100)
                break;

            gfx.DrawString(transaction.SaleDate.ToString("dd-MMM-yyyy"), smallFont, XBrushes.Black, new XRect(leftMargin, yPosition, colWidth, 15), XStringFormats.TopLeft);
            gfx.DrawString(transaction.InvoiceNumber ?? "-", smallFont, XBrushes.Black, new XRect(leftMargin + colWidth, yPosition, colWidth, 15), XStringFormats.TopLeft);
            
            string customerName = transaction.CustomerName ?? "Walk-in";
            if (customerName.Length > 15) customerName = customerName.Substring(0, 12) + "...";
            gfx.DrawString(customerName, smallFont, XBrushes.Black, new XRect(leftMargin + colWidth * 2, yPosition, colWidth, 15), XStringFormats.TopLeft);
            
            gfx.DrawString($"{currencySymbol}{transaction.TotalAmount:N2}", smallFont, XBrushes.Black, new XRect(leftMargin + colWidth * 3, yPosition, colWidth, 15), XStringFormats.TopLeft);
            gfx.DrawString(transaction.StatusName, smallFont, XBrushes.Black, new XRect(leftMargin + colWidth * 4, yPosition, colWidth, 15), XStringFormats.TopLeft);
            
            yPosition += 18;
            count++;
        }

        if (report.TotalRecords > 20)
        {
            yPosition += 10;
            gfx.DrawString($"Showing first 20 of {report.TotalRecords} transactions", smallFont, XBrushes.Gray, new XRect(leftMargin, yPosition, pageWidth, 15), XStringFormats.TopLeft);
        }

        // Footer
        gfx.DrawString($"Page 1", smallFont, XBrushes.Black, new XRect(leftMargin, page.Height - 40, pageWidth, 20), XStringFormats.Center);

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }
    
    // Helper method to map Transaction status string to SaleStatus enum
    private SaleStatus MapStatusToEnum(string status)
    {
        return status?.ToLower() switch
        {
            "draft" => SaleStatus.Draft,
            "billed" => SaleStatus.Billed,
            "settled" => SaleStatus.Settled,
            "hold" => SaleStatus.Hold,
            "cancelled" => SaleStatus.Cancelled,
            "pending_payment" => SaleStatus.PendingPayment,
            "partial_payment" => SaleStatus.PartialPayment,
            _ => SaleStatus.Settled
        };
    }
}
