using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Logging;
using ChronoPos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ClosedXML.Excel;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Customer Report operations
/// </summary>
public class CustomerReportService : ICustomerReportService
{
    private readonly IChronoPosDbContext _context;

    public CustomerReportService(IChronoPosDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerReportDto> GenerateCustomerReportAsync(CustomerReportFilterDto filter)
    {
        try
        {
            AppLogger.LogInfo("[CustomerReport] ===== STARTING CUSTOMER REPORT GENERATION =====", filename: "customer_report");

            var startDate = filter.StartDate ?? DateTime.Today.AddMonths(-1);
            var endDate = filter.EndDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

            AppLogger.LogInfo($"[CustomerReport] Report date range: {startDate:yyyy-MM-dd HH:mm:ss} to {endDate:yyyy-MM-dd HH:mm:ss}", filename: "customer_report");

            var report = new CustomerReportDto();

            AppLogger.LogInfo("[CustomerReport] Step 1/5: Getting customer summary...", filename: "customer_report");
            report.Summary = await GetCustomerSummaryAsync(startDate, endDate);
            AppLogger.LogInfo("[CustomerReport] Step 1/5: Customer summary completed", filename: "customer_report");

            AppLogger.LogInfo("[CustomerReport] Step 2/5: Getting top customers by revenue...", filename: "customer_report");
            report.TopCustomersByRevenue = await GetTopCustomersByRevenueAsync(startDate, endDate, 10);
            AppLogger.LogInfo("[CustomerReport] Step 2/5: Top customers by revenue completed", filename: "customer_report");

            AppLogger.LogInfo("[CustomerReport] Step 3/5: Getting top customers by purchases...", filename: "customer_report");
            report.TopCustomersByPurchases = await GetTopCustomersByPurchasesAsync(startDate, endDate, 10);
            AppLogger.LogInfo("[CustomerReport] Step 3/5: Top customers by purchases completed", filename: "customer_report");

            AppLogger.LogInfo("[CustomerReport] Step 4/5: Getting customer growth trend...", filename: "customer_report");
            report.CustomerGrowthTrend = await GetCustomerGrowthTrendAsync(startDate, endDate);
            AppLogger.LogInfo("[CustomerReport] Step 4/5: Customer growth trend completed", filename: "customer_report");

            AppLogger.LogInfo("[CustomerReport] Step 5/5: Getting customer segments...", filename: "customer_report");
            report.CustomerSegments = await GetCustomerSegmentsAsync(startDate, endDate);
            AppLogger.LogInfo("[CustomerReport] Step 5/5: Customer segments completed", filename: "customer_report");

            AppLogger.LogInfo("[CustomerReport] Getting customer analysis (paginated)...", filename: "customer_report");
            var (customers, totalCount) = await GetCustomerAnalysisAsync(filter);
            report.Customers = customers;
            report.TotalRecords = totalCount;

            AppLogger.LogInfo($"[CustomerReport] ===== CUSTOMER REPORT GENERATION COMPLETE: {totalCount} customers =====", filename: "customer_report");
            return report;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"[CustomerReport] ===== ERROR IN REPORT GENERATION: {ex.Message} =====", ex, filename: "customer_report");
            throw;
        }
    }

    public async Task<CustomerSummaryDto> GetCustomerSummaryAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            AppLogger.LogInfo($"[CustomerReport] Getting customer summary from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", filename: "customer_report");

            AppLogger.LogInfo("[CustomerReport] Fetching total customer count...", filename: "customer_report");
            var allCustomers = await _context.Customers.CountAsync();
            AppLogger.LogInfo($"[CustomerReport] Total customers: {allCustomers}", filename: "customer_report");
            
            // New customers in period
            AppLogger.LogInfo("[CustomerReport] Fetching new customers count...", filename: "customer_report");
            var newCustomers = await _context.Customers
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .CountAsync();
            AppLogger.LogInfo($"[CustomerReport] New customers: {newCustomers}", filename: "customer_report");

            // Get transactions in period
            AppLogger.LogInfo("[CustomerReport] Fetching transactions in period...", filename: "customer_report");
            var transactions = await _context.Transactions
                .Where(t => t.SellingTime >= startDate && t.SellingTime <= endDate
                         && (t.Status == "settled" || t.Status == "billed" || 
                             t.Status == "pending_payment" || t.Status == "partial_payment"))
                .ToListAsync();
            AppLogger.LogInfo($"[CustomerReport] Found {transactions.Count} transactions", filename: "customer_report");
            AppLogger.LogInfo($"[CustomerReport] Found {transactions.Count} transactions", filename: "customer_report");

            // Active customers (made at least one purchase in period)
            AppLogger.LogInfo("[CustomerReport] Calculating active customers...", filename: "customer_report");
            var activeCustomerIds = transactions.Select(t => t.CustomerId).Distinct().Count();
            AppLogger.LogInfo($"[CustomerReport] Active customers: {activeCustomerIds}", filename: "customer_report");

            AppLogger.LogInfo("[CustomerReport] Calculating revenue metrics...", filename: "customer_report");
            var totalRevenue = transactions.Sum(t => t.TotalAmount);
            var totalPurchases = transactions.Count;
            var avgOrderValue = totalPurchases > 0 ? totalRevenue / totalPurchases : 0;
            var avgRevenuePerCustomer = activeCustomerIds > 0 ? totalRevenue / activeCustomerIds : 0;
            AppLogger.LogInfo($"[CustomerReport] Total revenue: ${totalRevenue:F2}, Avg order: ${avgOrderValue:F2}", filename: "customer_report");

            // Calculate retention rate
            AppLogger.LogInfo("[CustomerReport] Calculating retention rate...", filename: "customer_report");
            var previousPeriodStart = startDate.AddDays(-(endDate - startDate).Days);
            var previousCustomers = await _context.Transactions
                .Where(t => t.SellingTime >= previousPeriodStart && t.SellingTime < startDate
                         && (t.Status == "settled" || t.Status == "billed"))
                .Select(t => t.CustomerId)
                .Distinct()
                .ToListAsync();
            AppLogger.LogInfo($"[CustomerReport] Previous period customers: {previousCustomers.Count}", filename: "customer_report");
            AppLogger.LogInfo($"[CustomerReport] Previous period customers: {previousCustomers.Count}", filename: "customer_report");

            var currentCustomers = transactions.Select(t => t.CustomerId).Distinct().ToList();
            var retainedCustomers = previousCustomers.Intersect(currentCustomers).Count();
            var retentionRate = previousCustomers.Count > 0 
                ? (double)retainedCustomers / previousCustomers.Count * 100 
                : 0;
            AppLogger.LogInfo($"[CustomerReport] Retention rate: {retentionRate:F2}%", filename: "customer_report");

            var summary = new CustomerSummaryDto
            {
                TotalCustomers = allCustomers,
                NewCustomersThisPeriod = newCustomers,
                ActiveCustomers = activeCustomerIds,
                InactiveCustomers = allCustomers - activeCustomerIds,
                TotalRevenue = totalRevenue,
                AverageRevenuePerCustomer = avgRevenuePerCustomer,
                AverageOrderValue = avgOrderValue,
                TotalPurchases = totalPurchases,
                CustomerRetentionRate = retentionRate,
                StartDate = startDate,
                EndDate = endDate
            };

            AppLogger.LogInfo($"[CustomerReport] Summary calculated successfully: {activeCustomerIds} active customers, Revenue: ${totalRevenue:F2}", filename: "customer_report");
            return summary;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error getting customer summary: {ex.Message}", ex, filename: "customer_report");
            throw;
        }
    }

    public async Task<(List<CustomerAnalysisDto> Customers, int TotalCount)> GetCustomerAnalysisAsync(CustomerReportFilterDto filter)
    {
        try
        {
            var startDate = filter.StartDate ?? DateTime.Today.AddMonths(-1);
            var endDate = filter.EndDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

            AppLogger.LogInfo($"[CustomerReport] Getting customer analysis from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", filename: "customer_report");

            // Get all customers with their transactions
            AppLogger.LogInfo("[CustomerReport] Building customer query...", filename: "customer_report");
            var query = _context.Customers.AsQueryable();

            // Apply active filter (Status = "Active") - can be done in SQL
            if (filter.IsActive.HasValue)
            {
                AppLogger.LogInfo($"[CustomerReport] Applying active filter: {filter.IsActive.Value}", filename: "customer_report");
                if (filter.IsActive.Value)
                {
                    query = query.Where(c => c.Status == "Active");
                }
                else
                {
                    query = query.Where(c => c.Status != "Active");
                }
            }

            // Load all customers first, then apply search in-memory to avoid EF Core translation issues
            AppLogger.LogInfo("[CustomerReport] Fetching all customers...", filename: "customer_report");
            var allCustomers = await query.ToListAsync();
            AppLogger.LogInfo($"[CustomerReport] Loaded {allCustomers.Count} customers", filename: "customer_report");

            // Apply search filter in-memory
            var customers = allCustomers;
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                AppLogger.LogInfo($"[CustomerReport] Applying search filter: {filter.SearchTerm}", filename: "customer_report");
                var searchLower = filter.SearchTerm.ToLower();
                customers = allCustomers.Where(c => 
                    (c.CustomerFullName?.ToLower().Contains(searchLower) ?? false) ||
                    (c.PrimaryEmail?.ToLower().Contains(searchLower) ?? false) ||
                    (c.PrimaryMobile?.Contains(filter.SearchTerm) ?? false))
                    .ToList();
                AppLogger.LogInfo($"[CustomerReport] After search filter: {customers.Count} customers", filename: "customer_report");
            }

            // Get customer IDs
            var customerIds = customers.Select(c => c.Id).ToList();
            AppLogger.LogInfo($"[CustomerReport] Found {customerIds.Count} customer IDs", filename: "customer_report");

            // Get all transactions for these customers in one query
            AppLogger.LogInfo("[CustomerReport] Fetching transactions with products and categories...", filename: "customer_report");
            var allTransactions = await _context.Transactions
                .Where(t => t.CustomerId.HasValue && customerIds.Contains(t.CustomerId.Value)
                         && t.SellingTime >= startDate
                         && t.SellingTime <= endDate
                         && (t.Status == "settled" || t.Status == "billed" ||
                             t.Status == "pending_payment" || t.Status == "partial_payment"))
                .Include(t => t.TransactionProducts)
                .ThenInclude(tp => tp.Product)
                .ThenInclude(p => p.Category)
                .ToListAsync();
            AppLogger.LogInfo($"[CustomerReport] Loaded {allTransactions.Count} transactions", filename: "customer_report");

            var customerAnalysisList = new List<CustomerAnalysisDto>();

            foreach (var customer in customers)
            {
                var transactions = allTransactions
                    .Where(t => t.CustomerId == customer.Id)
                    .ToList();

                if (transactions.Count == 0 && filter.MinimumPurchases > 0)
                    continue;

                var totalRevenue = transactions.Sum(t => t.TotalAmount);
                
                if (filter.MinimumRevenue.HasValue && totalRevenue < filter.MinimumRevenue.Value)
                    continue;
                if (filter.MaximumRevenue.HasValue && totalRevenue > filter.MaximumRevenue.Value)
                    continue;

                var totalPurchases = transactions.Count;
                var avgOrderValue = totalPurchases > 0 ? totalRevenue / totalPurchases : 0;

                var firstPurchase = transactions.MinBy(t => t.SellingTime)?.SellingTime;
                var lastPurchase = transactions.MaxBy(t => t.SellingTime)?.SellingTime;
                var daysSinceLastPurchase = lastPurchase.HasValue 
                    ? (int)(DateTime.Now - lastPurchase.Value).TotalDays 
                    : 0;

                // Get favorite category
                var categoryPurchases = transactions
                    .SelectMany(t => t.TransactionProducts)
                    .Where(tp => tp.Product?.Category != null)
                    .GroupBy(tp => tp.Product!.Category!.Name)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefault();

                var favoriteCategory = categoryPurchases?.Category ?? "N/A";

                // Get preferred payment method
                var preferredPayment = transactions
                    .GroupBy(t => t.AmountPaidCash > 0 ? "Cash" : "Credit")
                    .Select(g => new { Method = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefault()?.Method ?? "N/A";

                // Calculate purchase frequency
                var purchaseFrequency = "Unknown";
                if (firstPurchase.HasValue && lastPurchase.HasValue && totalPurchases > 1)
                {
                    var daysBetween = (lastPurchase.Value - firstPurchase.Value).TotalDays;
                    var avgDaysBetweenPurchases = daysBetween / (totalPurchases - 1);
                    
                    if (avgDaysBetweenPurchases <= 7) purchaseFrequency = "Weekly";
                    else if (avgDaysBetweenPurchases <= 30) purchaseFrequency = "Monthly";
                    else if (avgDaysBetweenPurchases <= 90) purchaseFrequency = "Quarterly";
                    else purchaseFrequency = "Occasional";
                }

                // Determine segment
                var segment = "Regular";
                if (totalRevenue >= 1000) segment = "VIP";
                else if (totalRevenue >= 500) segment = "Premium";
                else if (totalPurchases >= 10) segment = "Loyal";
                else if (totalPurchases == 1) segment = "New";

                if (!string.IsNullOrEmpty(filter.CustomerSegment) && segment != filter.CustomerSegment)
                    continue;

                var analysis = new CustomerAnalysisDto
                {
                    CustomerId = customer.Id,
                    CustomerName = customer.CustomerFullName,
                    Email = customer.PrimaryEmail ?? "",
                    Phone = customer.PrimaryMobile ?? "",
                    TotalPurchases = totalPurchases,
                    TotalRevenue = totalRevenue,
                    AverageOrderValue = avgOrderValue,
                    FirstPurchaseDate = firstPurchase,
                    LastPurchaseDate = lastPurchase,
                    DaysSinceLastPurchase = daysSinceLastPurchase,
                    FavoriteCategory = favoriteCategory,
                    PreferredPaymentMethod = preferredPayment,
                    CustomerLifetimeValue = totalRevenue,
                    PurchaseFrequency = purchaseFrequency,
                    IsActive = daysSinceLastPurchase <= 90,
                    CustomerSegment = segment
                };

                customerAnalysisList.Add(analysis);
            }

            var totalCount = customerAnalysisList.Count;

            // Apply sorting
            customerAnalysisList = filter.SortBy.ToLower() switch
            {
                "customername" => filter.SortDescending
                    ? customerAnalysisList.OrderByDescending(c => c.CustomerName).ToList()
                    : customerAnalysisList.OrderBy(c => c.CustomerName).ToList(),
                "totalpurchases" => filter.SortDescending
                    ? customerAnalysisList.OrderByDescending(c => c.TotalPurchases).ToList()
                    : customerAnalysisList.OrderBy(c => c.TotalPurchases).ToList(),
                "lastpurchasedate" => filter.SortDescending
                    ? customerAnalysisList.OrderByDescending(c => c.LastPurchaseDate).ToList()
                    : customerAnalysisList.OrderBy(c => c.LastPurchaseDate).ToList(),
                _ => filter.SortDescending
                    ? customerAnalysisList.OrderByDescending(c => c.TotalRevenue).ToList()
                    : customerAnalysisList.OrderBy(c => c.TotalRevenue).ToList()
            };

            // Apply pagination
            var paginatedList = customerAnalysisList
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            AppLogger.LogInfo($"Customer analysis retrieved: {totalCount} total customers, {paginatedList.Count} on page", filename: "customer_report");
            return (paginatedList, totalCount);
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error getting customer analysis: {ex.Message}", ex, filename: "customer_report");
            throw;
        }
    }

    public async Task<List<CustomerRankingDto>> GetTopCustomersByRevenueAsync(DateTime startDate, DateTime endDate, int topCount = 10)
    {
        try
        {
            AppLogger.LogInfo($"[CustomerReport] Getting top {topCount} customers by revenue...", filename: "customer_report");
            
            // Load transactions first, then group in-memory
            var transactions = await _context.Transactions
                .Where(t => t.SellingTime >= startDate && t.SellingTime <= endDate
                         && (t.Status == "settled" || t.Status == "billed" ||
                             t.Status == "pending_payment" || t.Status == "partial_payment"))
                .ToListAsync();

            AppLogger.LogInfo($"[CustomerReport] Loaded {transactions.Count} transactions for revenue analysis", filename: "customer_report");

            // Get customer IDs
            var customerIds = transactions.Where(t => t.CustomerId.HasValue)
                .Select(t => t.CustomerId.Value)
                .Distinct()
                .ToList();

            // Load customers
            var customers = await _context.Customers
                .Where(c => customerIds.Contains(c.Id))
                .ToListAsync();

            AppLogger.LogInfo($"[CustomerReport] Loaded {customers.Count} customers", filename: "customer_report");

            // Group in-memory
            var customerRevenue = transactions
                .Where(t => t.CustomerId.HasValue)
                .GroupBy(t => t.CustomerId.Value)
                .Select(g =>
                {
                    var customer = customers.FirstOrDefault(c => c.Id == g.Key);
                    return new CustomerRankingDto
                    {
                        CustomerId = g.Key,
                        CustomerName = customer?.CustomerFullName ?? "Unknown",
                        Email = customer?.PrimaryEmail ?? "",
                        TotalRevenue = g.Sum(t => t.TotalAmount),
                        TotalPurchases = g.Count()
                    };
                })
                .OrderByDescending(c => c.TotalRevenue)
                .Take(topCount)
                .ToList();

            AppLogger.LogInfo($"[CustomerReport] Top {customerRevenue.Count} customers by revenue calculated", filename: "customer_report");
            return customerRevenue;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error getting top customers by revenue: {ex.Message}", ex, filename: "customer_report");
            throw;
        }
    }

    public async Task<List<CustomerRankingDto>> GetTopCustomersByPurchasesAsync(DateTime startDate, DateTime endDate, int topCount = 10)
    {
        try
        {
            AppLogger.LogInfo($"[CustomerReport] Getting top {topCount} customers by purchases...", filename: "customer_report");
            
            // Load transactions first, then group in-memory
            var transactions = await _context.Transactions
                .Where(t => t.SellingTime >= startDate && t.SellingTime <= endDate
                         && (t.Status == "settled" || t.Status == "billed"))
                .ToListAsync();

            AppLogger.LogInfo($"[CustomerReport] Loaded {transactions.Count} transactions for purchases analysis", filename: "customer_report");

            // Get customer IDs
            var customerIds = transactions.Where(t => t.CustomerId.HasValue)
                .Select(t => t.CustomerId.Value)
                .Distinct()
                .ToList();

            // Load customers
            var customers = await _context.Customers
                .Where(c => customerIds.Contains(c.Id))
                .ToListAsync();

            AppLogger.LogInfo($"[CustomerReport] Loaded {customers.Count} customers", filename: "customer_report");

            // Group in-memory
            var customerPurchases = transactions
                .Where(t => t.CustomerId.HasValue)
                .GroupBy(t => t.CustomerId.Value)
                .Select(g =>
                {
                    var customer = customers.FirstOrDefault(c => c.Id == g.Key);
                    return new CustomerRankingDto
                    {
                        CustomerId = g.Key,
                        CustomerName = customer?.CustomerFullName ?? "Unknown",
                        Email = customer?.PrimaryEmail ?? "",
                        TotalPurchases = g.Count(),
                        TotalRevenue = g.Sum(t => t.TotalAmount),
                        AverageOrderValue = g.Average(t => t.TotalAmount),
                        LastPurchaseDate = g.Max(t => t.SellingTime)
                    };
                })
                .OrderByDescending(c => c.TotalPurchases)
                .Take(topCount)
                .Select((c, index) => new CustomerRankingDto
                {
                    Rank = index + 1,
                    CustomerId = c.CustomerId,
                    CustomerName = c.CustomerName,
                    Email = c.Email,
                    TotalPurchases = c.TotalPurchases,
                    TotalRevenue = c.TotalRevenue,
                    AverageOrderValue = c.AverageOrderValue,
                    LastPurchaseDate = c.LastPurchaseDate
                })
                .ToList();

            AppLogger.LogInfo($"[CustomerReport] Top {customerPurchases.Count} customers by purchases calculated", filename: "customer_report");
            return customerPurchases;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error getting top customers by purchases: {ex.Message}", ex, filename: "customer_report");
            throw;
        }
    }

    public async Task<List<CustomerProductPreferenceDto>> GetCustomerProductPreferencesAsync(int customerId, int topCount = 10)
    {
        try
        {
            var preferences = await _context.TransactionProducts
                .Where(tp => tp.Transaction.CustomerId == customerId)
                .Include(tp => tp.Product)
                .ThenInclude(p => p!.Category)
                .Include(tp => tp.Transaction)
                .GroupBy(tp => new
                {
                    tp.ProductId,
                    tp.Product!.Name,
                    CategoryName = tp.Product.Category != null ? tp.Product.Category.Name : "Uncategorized"
                })
                .Select(g => new CustomerProductPreferenceDto
                {
                    CustomerId = customerId,
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    CategoryName = g.Key.CategoryName,
                    PurchaseCount = g.Sum(tp => (int)tp.Quantity),
                    TotalSpent = g.Sum(tp => tp.LineTotal),
                    LastPurchased = g.Max(tp => tp.Transaction.SellingTime)
                })
                .OrderByDescending(p => p.PurchaseCount)
                .Take(topCount)
                .ToListAsync();

            // Set customer name
            var customer = await _context.Customers.FindAsync(customerId);
            foreach (var pref in preferences)
            {
                pref.CustomerName = customer?.CustomerFullName ?? "";
            }

            return preferences;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error getting customer product preferences: {ex.Message}", ex, filename: "customer_report");
            throw;
        }
    }

    public async Task<List<CustomerPaymentAnalysisDto>> GetCustomerPaymentAnalysisAsync(int customerId)
    {
        try
        {
            var transactions = await _context.Transactions
                .Where(t => t.CustomerId == customerId)
                .ToListAsync();

            var customer = await _context.Customers.FindAsync(customerId);
            var customerName = customer?.CustomerFullName ?? "";

            var paymentAnalysis = transactions
                .GroupBy(t => t.AmountPaidCash > 0 ? "Cash" : "Credit")
                .Select(g => new CustomerPaymentAnalysisDto
                {
                    CustomerId = customerId,
                    CustomerName = customerName,
                    PaymentMethod = g.Key,
                    UsageCount = g.Count(),
                    TotalAmount = g.Sum(t => t.TotalAmount)
                })
                .ToList();

            return paymentAnalysis;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error getting customer payment analysis: {ex.Message}", ex, filename: "customer_report");
            throw;
        }
    }

    public async Task<List<CustomerGrowthDto>> GetCustomerGrowthTrendAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var customers = await _context.Customers
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .ToListAsync();

            var transactions = await _context.Transactions
                .Where(t => t.SellingTime >= startDate && t.SellingTime <= endDate
                         && (t.Status == "settled" || t.Status == "billed"))
                .ToListAsync();

            var growthTrend = new List<CustomerGrowthDto>();
            var currentDate = startDate.Date;
            var runningTotal = await _context.Customers.CountAsync(c => c.CreatedAt < startDate);

            while (currentDate <= endDate.Date)
            {
                var newOnDate = customers.Count(c => c.CreatedAt.Date == currentDate);
                runningTotal += newOnDate;
                var revenueOnDate = transactions
                    .Where(t => t.SellingTime.Date == currentDate)
                    .Sum(t => t.TotalAmount);

                growthTrend.Add(new CustomerGrowthDto
                {
                    Date = currentDate,
                    NewCustomers = newOnDate,
                    TotalCustomers = runningTotal,
                    Revenue = revenueOnDate
                });

                currentDate = currentDate.AddDays(1);
            }

            return growthTrend;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error getting customer growth trend: {ex.Message}", ex, filename: "customer_report");
            throw;
        }
    }

    public async Task<List<CustomerSegmentDto>> GetCustomerSegmentsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var (customers, _) = await GetCustomerAnalysisAsync(new CustomerReportFilterDto
            {
                StartDate = startDate,
                EndDate = endDate,
                PageSize = int.MaxValue
            });

            var segments = customers
                .GroupBy(c => c.CustomerSegment)
                .Select(g => new CustomerSegmentDto
                {
                    SegmentName = g.Key,
                    CustomerCount = g.Count(),
                    TotalRevenue = g.Sum(c => c.TotalRevenue),
                    AverageOrderValue = g.Average(c => c.AverageOrderValue)
                })
                .ToList();

            var totalCustomers = segments.Sum(s => s.CustomerCount);
            foreach (var segment in segments)
            {
                segment.PercentageOfTotal = totalCustomers > 0 
                    ? (double)segment.CustomerCount / totalCustomers * 100 
                    : 0;
            }

            return segments.OrderByDescending(s => s.TotalRevenue).ToList();
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error getting customer segments: {ex.Message}", ex, filename: "customer_report");
            throw;
        }
    }

    public async Task<byte[]> ExportToExcelAsync(CustomerReportFilterDto filter)
    {
        // For exports, ensure we get all records
        filter.PageNumber = 1;
        filter.PageSize = int.MaxValue;

        AppLogger.LogInfo($"ExportToExcelAsync called with date range: {filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}", filename: "customer_report");

        var report = await GenerateCustomerReportAsync(filter);
        var currencySymbol = filter.CurrencySymbol ?? "€";

        AppLogger.LogInfo($"Exporting {report.Customers?.Count ?? 0} customers to Excel", filename: "customer_report");

        using var workbook = new XLWorkbook();

        // Summary Sheet
        var summarySheet = workbook.Worksheets.Add("Summary");
        summarySheet.Cell("A1").Value = "Customer Report Summary";
        summarySheet.Cell("A1").Style.Font.FontSize = 16;
        summarySheet.Cell("A1").Style.Font.Bold = true;

        summarySheet.Cell("A3").Value = "Period";
        summarySheet.Cell("B3").Value = $"{filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}";

        summarySheet.Cell("A5").Value = "Total Customers";
        summarySheet.Cell("B5").Value = report.Summary.TotalCustomers;

        summarySheet.Cell("A6").Value = "Active Customers";
        summarySheet.Cell("B6").Value = report.Summary.ActiveCustomers;

        summarySheet.Cell("A7").Value = "New Customers";
        summarySheet.Cell("B7").Value = report.Summary.NewCustomersThisPeriod;

        summarySheet.Cell("A8").Value = "Total Revenue";
        summarySheet.Cell("B8").Value = report.Summary.TotalRevenue;
        summarySheet.Cell("B8").Style.NumberFormat.Format = $"\"{currencySymbol}\"#,##0.00";

        summarySheet.Cell("A9").Value = "Avg Revenue Per Customer";
        summarySheet.Cell("B9").Value = report.Summary.AverageRevenuePerCustomer;
        summarySheet.Cell("B9").Style.NumberFormat.Format = $"\"{currencySymbol}\"#,##0.00";

        summarySheet.Cell("A10").Value = "Customer Retention Rate";
        summarySheet.Cell("B10").Value = report.Summary.CustomerRetentionRate / 100;
        summarySheet.Cell("B10").Style.NumberFormat.Format = "0.00%";

        // Customers Sheet - Complete Data
        var customersSheet = workbook.Worksheets.Add("Customers");
        customersSheet.Cell("A1").Value = "Customer Name";
        customersSheet.Cell("B1").Value = "Email";
        customersSheet.Cell("C1").Value = "Phone";
        customersSheet.Cell("D1").Value = "Total Purchases";
        customersSheet.Cell("E1").Value = "Total Revenue";
        customersSheet.Cell("F1").Value = "Avg Order Value";
        customersSheet.Cell("G1").Value = "Lifetime Value";
        customersSheet.Cell("H1").Value = "First Purchase";
        customersSheet.Cell("I1").Value = "Last Purchase";
        customersSheet.Cell("J1").Value = "Days Since Last";
        customersSheet.Cell("K1").Value = "Purchase Frequency";
        customersSheet.Cell("L1").Value = "Favorite Category";
        customersSheet.Cell("M1").Value = "Preferred Payment";
        customersSheet.Cell("N1").Value = "Segment";
        customersSheet.Cell("O1").Value = "Status";

        customersSheet.Range("A1:O1").Style.Font.Bold = true;
        customersSheet.Range("A1:O1").Style.Fill.BackgroundColor = XLColor.LightBlue;

        AppLogger.LogInfo($"Starting to write {report.Customers?.Count ?? 0} customers to Excel", filename: "customer_report");

        if (report.Customers != null && report.Customers.Count > 0)
        {
            int row = 2;
            foreach (var customer in report.Customers)
            {
                try
                {
                    customersSheet.Cell(row, 1).Value = customer.CustomerName;
                    customersSheet.Cell(row, 2).Value = customer.Email;
                    customersSheet.Cell(row, 3).Value = customer.Phone;
                    customersSheet.Cell(row, 4).Value = customer.TotalPurchases;
                    customersSheet.Cell(row, 5).Value = customer.TotalRevenue;
                    customersSheet.Cell(row, 5).Style.NumberFormat.Format = $"\"{currencySymbol}\"#,##0.00";
                    customersSheet.Cell(row, 6).Value = customer.AverageOrderValue;
                    customersSheet.Cell(row, 6).Style.NumberFormat.Format = $"\"{currencySymbol}\"#,##0.00";
                    customersSheet.Cell(row, 7).Value = customer.CustomerLifetimeValue;
                    customersSheet.Cell(row, 7).Style.NumberFormat.Format = $"\"{currencySymbol}\"#,##0.00";
                    customersSheet.Cell(row, 8).Value = customer.FirstPurchaseDate?.ToString("yyyy-MM-dd") ?? "N/A";
                    customersSheet.Cell(row, 9).Value = customer.LastPurchaseDate?.ToString("yyyy-MM-dd") ?? "N/A";
                    customersSheet.Cell(row, 10).Value = customer.DaysSinceLastPurchase;
                    customersSheet.Cell(row, 11).Value = customer.PurchaseFrequency;
                    customersSheet.Cell(row, 12).Value = customer.FavoriteCategory;
                    customersSheet.Cell(row, 13).Value = customer.PreferredPaymentMethod;
                    customersSheet.Cell(row, 14).Value = customer.CustomerSegment;
                    customersSheet.Cell(row, 15).Value = customer.IsActive ? "Active" : "Inactive";
                    row++;
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"Error writing customer row {row}: {ex.Message}", ex, filename: "customer_report");
                }
            }
            AppLogger.LogInfo($"Finished writing {row - 2} customers to Excel", filename: "customer_report");
        }

        customersSheet.Columns().AdjustToContents();
        summarySheet.Columns().AdjustToContents();

        // Make Customers sheet first
        customersSheet.Position = 1;
        summarySheet.Position = 2;

        AppLogger.LogInfo($"Workbook has {workbook.Worksheets.Count} worksheets", filename: "customer_report");

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportToCsvAsync(CustomerReportFilterDto filter)
    {
        filter.PageNumber = 1;
        filter.PageSize = int.MaxValue;

        var (customers, _) = await GetCustomerAnalysisAsync(filter);

        AppLogger.LogInfo($"Exporting {customers.Count} customers to CSV", filename: "customer_report");

        var csv = new StringBuilder();
        csv.AppendLine("Customer Name,Email,Phone,Total Purchases,Total Revenue,Avg Order Value,Lifetime Value,First Purchase,Last Purchase,Days Since Last,Purchase Frequency,Favorite Category,Preferred Payment,Segment,Status");

        foreach (var customer in customers)
        {
            csv.AppendLine($"\"{customer.CustomerName}\"," +
                          $"\"{customer.Email}\"," +
                          $"\"{customer.Phone}\"," +
                          $"{customer.TotalPurchases}," +
                          $"{customer.TotalRevenue:F2}," +
                          $"{customer.AverageOrderValue:F2}," +
                          $"{customer.CustomerLifetimeValue:F2}," +
                          $"{customer.FirstPurchaseDate:yyyy-MM-dd}," +
                          $"{customer.LastPurchaseDate:yyyy-MM-dd}," +
                          $"{customer.DaysSinceLastPurchase}," +
                          $"\"{customer.PurchaseFrequency}\"," +
                          $"\"{customer.FavoriteCategory}\"," +
                          $"\"{customer.PreferredPaymentMethod}\"," +
                          $"\"{customer.CustomerSegment}\"," +
                          $"\"{(customer.IsActive ? "Active" : "Inactive")}\"");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportToPdfAsync(CustomerReportFilterDto filter)
    {
        // For exports, ensure we get all records
        filter.PageNumber = 1;
        filter.PageSize = int.MaxValue;

        var report = await GenerateCustomerReportAsync(filter);
        var currencySymbol = filter.CurrencySymbol ?? "€";

        AppLogger.LogInfo($"Exporting {report.Customers.Count} customers to PDF", filename: "customer_report");

        var document = new PdfDocument();
        document.Info.Title = "Customer Report";
        document.Info.Subject = $"Customer Report from {filter.StartDate:dd-MMM-yyyy} to {filter.EndDate:dd-MMM-yyyy}";
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
        gfx.DrawString("Customer Report", titleFont, XBrushes.DarkBlue, new XRect(leftMargin, yPosition, pageWidth, 30), XStringFormats.TopLeft);
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
            ("Total Customers", report.Summary.TotalCustomers.ToString()),
            ("Active Customers", report.Summary.ActiveCustomers.ToString()),
            ("New Customers", report.Summary.NewCustomersThisPeriod.ToString()),
            ("Total Revenue", $"{currencySymbol}{report.Summary.TotalRevenue:N2}"),
            ("Avg Revenue/Customer", $"{currencySymbol}{report.Summary.AverageRevenuePerCustomer:N2}"),
            ("Retention Rate", $"{report.Summary.CustomerRetentionRate:N1}%")
        };

        foreach (var (label, value) in summaryData)
        {
            gfx.DrawString(label + ":", normalFont, XBrushes.Black, new XRect(leftMargin, yPosition, 200, 20), XStringFormats.TopLeft);
            gfx.DrawString(value, normalFont, XBrushes.Black, new XRect(leftMargin + 200, yPosition, 200, 20), XStringFormats.TopLeft);
            yPosition += 20;
        }

        yPosition += 20;

        // Customers Section
        gfx.DrawString("Top Customers", headerFont, XBrushes.Black, new XRect(leftMargin, yPosition, pageWidth, 20), XStringFormats.TopLeft);
        yPosition += 25;

        // Table Header
        double colWidth = pageWidth / 6;
        gfx.DrawRectangle(XBrushes.LightBlue, leftMargin, yPosition, pageWidth, 20);
        gfx.DrawString("Customer", normalFont, XBrushes.Black, new XRect(leftMargin, yPosition, colWidth * 1.5, 20), XStringFormats.TopLeft);
        gfx.DrawString("Phone", normalFont, XBrushes.Black, new XRect(leftMargin + colWidth * 1.5, yPosition, colWidth, 20), XStringFormats.TopLeft);
        gfx.DrawString("Purchases", normalFont, XBrushes.Black, new XRect(leftMargin + colWidth * 2.5, yPosition, colWidth * 0.8, 20), XStringFormats.TopLeft);
        gfx.DrawString("Revenue", normalFont, XBrushes.Black, new XRect(leftMargin + colWidth * 3.3, yPosition, colWidth, 20), XStringFormats.TopLeft);
        gfx.DrawString("Avg Order", normalFont, XBrushes.Black, new XRect(leftMargin + colWidth * 4.3, yPosition, colWidth, 20), XStringFormats.TopLeft);
        gfx.DrawString("Segment", normalFont, XBrushes.Black, new XRect(leftMargin + colWidth * 5.3, yPosition, colWidth * 0.7, 20), XStringFormats.TopLeft);
        yPosition += 22;

        // Table Rows (limit to 20 customers to fit on page)
        int count = 0;
        foreach (var customer in report.Customers.Take(20))
        {
            if (yPosition > page.Height - 100)
                break;

            string customerName = customer.CustomerName ?? "-";
            if (customerName.Length > 20) customerName = customerName.Substring(0, 17) + "...";
            gfx.DrawString(customerName, smallFont, XBrushes.Black, new XRect(leftMargin, yPosition, colWidth * 1.5, 15), XStringFormats.TopLeft);

            string phone = customer.Phone ?? "-";
            if (phone.Length > 12) phone = phone.Substring(0, 9) + "...";
            gfx.DrawString(phone, smallFont, XBrushes.Black, new XRect(leftMargin + colWidth * 1.5, yPosition, colWidth, 15), XStringFormats.TopLeft);

            gfx.DrawString(customer.TotalPurchases.ToString(), smallFont, XBrushes.Black, new XRect(leftMargin + colWidth * 2.5, yPosition, colWidth * 0.8, 15), XStringFormats.TopLeft);
            gfx.DrawString($"{currencySymbol}{customer.TotalRevenue:N2}", smallFont, XBrushes.Black, new XRect(leftMargin + colWidth * 3.3, yPosition, colWidth, 15), XStringFormats.TopLeft);
            gfx.DrawString($"{currencySymbol}{customer.AverageOrderValue:N2}", smallFont, XBrushes.Black, new XRect(leftMargin + colWidth * 4.3, yPosition, colWidth, 15), XStringFormats.TopLeft);
            gfx.DrawString(customer.CustomerSegment, smallFont, XBrushes.Black, new XRect(leftMargin + colWidth * 5.3, yPosition, colWidth * 0.7, 15), XStringFormats.TopLeft);

            yPosition += 18;
            count++;
        }

        if (report.TotalRecords > 20)
        {
            yPosition += 10;
            gfx.DrawString($"Showing first 20 of {report.TotalRecords} customers", smallFont, XBrushes.Gray, new XRect(leftMargin, yPosition, pageWidth, 15), XStringFormats.TopLeft);
        }

        // Footer
        gfx.DrawString($"Page 1", smallFont, XBrushes.Black, new XRect(leftMargin, page.Height - 40, pageWidth, 20), XStringFormats.Center);

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }
}
