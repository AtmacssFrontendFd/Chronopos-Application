using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Dashboard operations and analytics
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRestaurantTableRepository _restaurantTableRepository;
    private readonly ITransactionProductRepository _transactionProductRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(
        ITransactionRepository transactionRepository,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IRepository<Customer> customerRepository,
        IRestaurantTableRepository restaurantTableRepository,
        ITransactionProductRepository transactionProductRepository,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _customerRepository = customerRepository;
        _restaurantTableRepository = restaurantTableRepository;
        _transactionProductRepository = transactionProductRepository;
        _unitOfWork = unitOfWork;
    }

    #region Helper Methods

    /// <summary>
    /// Filters transactions to only include valid ones (excludes cancelled, draft, hold)
    /// If no transactions match the filter, returns all active transactions (for compatibility with existing data)
    /// </summary>
    private List<Transaction> FilterValidTransactions(IEnumerable<Transaction> transactions)
    {
        var transactionsList = transactions.ToList();
        var validTransactions = transactionsList
            .Where(t => !t.DeletedAt.HasValue && 
                       t.Status != "cancelled" && 
                       t.Status != "draft" && 
                       t.Status != "hold")
            .ToList();

        // If no transactions with proper status, consider all active transactions
        if (!validTransactions.Any() && transactionsList.Any())
        {
            validTransactions = transactionsList.Where(t => !t.DeletedAt.HasValue).ToList();
        }

        return validTransactions;
    }

    #endregion

    #region KPI Methods

    public async Task<DashboardKpiDto> GetDashboardKpisAsync()
    {
        try
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddDays(-1);

            // Get all transactions for calculations
            var allTransactions = await _transactionRepository.GetAllAsync();
            var validTransactions = FilterValidTransactions(allTransactions);

            Console.WriteLine($"[DashboardService] GetDashboardKpisAsync:");
            Console.WriteLine($"  Total transactions in DB: {allTransactions.Count()}");
            Console.WriteLine($"  Valid transactions (after filter): {validTransactions.Count}");
            Console.WriteLine($"  Today: {today:yyyy-MM-dd}");
            Console.WriteLine($"  Start of month: {startOfMonth:yyyy-MM-dd}");

            // Calculate today's sales
            var todaysSales = validTransactions
                .Where(t => t.SellingTime.Date == today)
                .Sum(t => t.TotalAmount);

            Console.WriteLine($"  Today's sales: ${todaysSales} ({validTransactions.Count(t => t.SellingTime.Date == today)} transactions)");

            // Calculate yesterday's sales
            var yesterdaysSales = validTransactions
                .Where(t => t.SellingTime.Date == yesterday)
                .Sum(t => t.TotalAmount);

            // Calculate monthly sales
            var monthlySales = validTransactions
                .Where(t => t.SellingTime >= startOfMonth)
                .Sum(t => t.TotalAmount);

            Console.WriteLine($"  Monthly sales: ${monthlySales} ({validTransactions.Count(t => t.SellingTime >= startOfMonth)} transactions)");

            // Calculate last month's sales
            var lastMonthSales = validTransactions
                .Where(t => t.SellingTime >= startOfLastMonth && t.SellingTime <= endOfLastMonth)
                .Sum(t => t.TotalAmount);

            // Calculate growth percentage (comparing current month to last month)
            var growthPercentage = lastMonthSales > 0 
                ? ((monthlySales - lastMonthSales) / lastMonthSales) * 100 
                : 0;

            // Get today's transaction count
            var todaysTransactionCount = validTransactions
                .Count(t => t.SellingTime.Date == today);

            // Calculate average transaction value
            var averageTransactionValue = todaysTransactionCount > 0 
                ? todaysSales / todaysTransactionCount 
                : 0;

            // Get active tables count
            var activeTables = await GetActiveTablesCountAsync();

            // Get pending orders count
            var pendingOrders = await GetPendingOrdersCountAsync();

            // Get low stock items count
            var lowStockItems = await GetLowStockItemsCountAsync();

            // Get total active customers
            var allCustomers = await _customerRepository.GetAllAsync();
            var totalCustomers = allCustomers.Count(c => !c.DeletedAt.HasValue);

            return new DashboardKpiDto
            {
                TodaysSales = todaysSales,
                MonthlySales = monthlySales,
                GrowthPercentage = growthPercentage,
                ActiveTables = activeTables,
                PendingOrders = pendingOrders,
                LowStockItems = lowStockItems,
                TotalCustomers = totalCustomers,
                AverageTransactionValue = averageTransactionValue,
                TodaysTransactionCount = todaysTransactionCount,
                YesterdaysSales = yesterdaysSales,
                LastMonthSales = lastMonthSales
            };
        }
        catch (Exception ex)
        {
            // Log error and return default values
            Console.WriteLine($"Error in GetDashboardKpisAsync: {ex.Message}");
            return new DashboardKpiDto();
        }
    }

    public async Task<decimal> GetTodaysSalesAsync()
    {
        try
        {
            var today = DateTime.Today;
            var transactions = await _transactionRepository.GetByDateRangeAsync(today, today.AddDays(1));
            var validTransactions = FilterValidTransactions(transactions);
            return validTransactions.Sum(t => t.TotalAmount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetTodaysSalesAsync: {ex.Message}");
            return 0;
        }
    }

    public async Task<decimal> GetMonthlySalesAsync()
    {
        try
        {
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var transactions = await _transactionRepository.GetByDateRangeAsync(startOfMonth, DateTime.Now);
            var validTransactions = FilterValidTransactions(transactions);
            return validTransactions.Sum(t => t.TotalAmount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetMonthlySalesAsync: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> GetActiveTablesCountAsync()
    {
        try
        {
            var tables = await _restaurantTableRepository.GetAllAsync();
            return tables.Count(t => t.Status == "occupied" || t.Status == "reserved");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetActiveTablesCountAsync: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> GetLowStockItemsCountAsync()
    {
        try
        {
            var products = await _productRepository.GetAllAsync();
            return products.Count(p => p.StockQuantity <= p.ReorderLevel && p.ReorderLevel > 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetLowStockItemsCountAsync: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> GetPendingOrdersCountAsync()
    {
        try
        {
            var transactions = await _transactionRepository.GetAllAsync();
            return transactions.Count(t => t.Status == "draft" || t.Status == "billed" || t.Status == "pending_payment");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetPendingOrdersCountAsync: {ex.Message}");
            return 0;
        }
    }

    #endregion

    #region Popular Products

    public async Task<List<ProductSalesDto>> GetPopularProductsAsync(int count = 6, int days = 7)
    {
        try
        {
            var startDate = DateTime.Now.AddDays(-days);
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, DateTime.Now);
            var validTransactions = FilterValidTransactions(transactions);

            Console.WriteLine($"DashboardService.GetPopularProducts: Found {validTransactions.Count} valid transactions");

            // Get all transaction products
            var allTransactionProducts = await _transactionProductRepository.GetAllAsync();
            var transactionIds = validTransactions.Select(t => t.Id).ToList();
            
            var transactionProducts = allTransactionProducts
                .Where(tp => transactionIds.Contains(tp.TransactionId))
                .ToList();

            Console.WriteLine($"DashboardService.GetPopularProducts: Found {transactionProducts.Count} transaction products");

            // Group by product and calculate totals
            var productSales = transactionProducts
                .GroupBy(tp => tp.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    UnitsSold = (int)g.Sum(tp => tp.Quantity),
                    Revenue = g.Sum(tp => tp.LineTotal)
                })
                .OrderByDescending(ps => ps.UnitsSold)
                .Take(count)
                .ToList();

            var result = new List<ProductSalesDto>();

            foreach (var ps in productSales)
            {
                var product = await _productRepository.GetByIdAsync(ps.ProductId);
                if (product != null)
                {
                    var stockStatus = GetStockStatus(product.StockQuantity, (int)product.ReorderLevel);
                    var totalRevenue = validTransactions.Sum(t => t.TotalAmount);
                    var salesPercentage = totalRevenue > 0 ? (ps.Revenue / totalRevenue) * 100 : 0;

                    result.Add(new ProductSalesDto
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Sku = product.SKU,
                        ImagePath = product.ImagePath,
                        UnitsSold = ps.UnitsSold,
                        Revenue = ps.Revenue,
                        StockQuantity = product.StockQuantity,
                        StockStatus = stockStatus,
                        Color = product.Color,
                        CategoryName = product.Category?.Name,
                        UnitPrice = product.Price,
                        SalesPercentage = salesPercentage
                    });
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetPopularProductsAsync: {ex.Message}");
            return new List<ProductSalesDto>();
        }
    }

    private string GetStockStatus(int stockQuantity, int reorderLevel)
    {
        if (stockQuantity <= 0)
            return "Out of Stock";
        if (stockQuantity <= reorderLevel && reorderLevel > 0)
            return "Low Stock";
        return "In Stock";
    }

    #endregion

    #region Recent Sales

    public async Task<List<RecentSaleDto>> GetRecentSalesAsync(int count = 10)
    {
        try
        {
            var transactions = await _transactionRepository.GetAllWithDetailsAsync();
            var recentTransactions = transactions
                .OrderByDescending(t => t.SellingTime)
                .Take(count)
                .ToList();

            var result = new List<RecentSaleDto>();

            foreach (var transaction in recentTransactions)
            {
                var productNames = transaction.TransactionProducts
                    .Select(tp => tp.Product?.Name ?? "Unknown")
                    .ToList();

                var customerName = transaction.Customer != null
                    ? transaction.Customer.DisplayName
                    : "Walk-in Customer";

                result.Add(new RecentSaleDto
                {
                    TransactionId = transaction.Id,
                    InvoiceNumber = transaction.InvoiceNumber ?? $"INV-{transaction.Id}",
                    SellingTime = transaction.SellingTime,
                    CustomerName = customerName,
                    CustomerId = transaction.CustomerId,
                    ProductNames = productNames,
                    ItemCount = transaction.TransactionProducts.Count,
                    TotalAmount = transaction.TotalAmount,
                    Status = transaction.Status,
                    PaymentMethod = transaction.AmountPaidCash > 0 ? "Cash" : "Credit",
                    CreatedBy = transaction.Creator?.Username ?? "Unknown"
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetRecentSalesAsync: {ex.Message}");
            return new List<RecentSaleDto>();
        }
    }

    #endregion

    #region Sales Analytics

    public async Task<List<SalesAnalyticsDto>> GetDailySalesAnalyticsAsync(int days = 30)
    {
        try
        {
            var startDate = DateTime.Now.AddDays(-days).Date;
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, DateTime.Now);
            var validTransactions = FilterValidTransactions(transactions);

            Console.WriteLine($"[DashboardService] GetDailySalesAnalyticsAsync: Queried transactions = {transactions.Count()}");
            Console.WriteLine($"[DashboardService] GetDailySalesAnalyticsAsync: Valid transactions after filter = {validTransactions.Count}");

            var dailySales = validTransactions
                .GroupBy(t => t.SellingTime.Date)
                .Select(g => new SalesAnalyticsDto
                {
                    Period = g.Key.ToString("MMM dd"),
                    Date = g.Key,
                    Sales = g.Sum(t => t.TotalAmount),
                    TransactionCount = g.Count(),
                    AverageValue = g.Count() > 0 ? g.Sum(t => t.TotalAmount) / g.Count() : 0,
                    ItemsSold = (int)g.SelectMany(t => t.TransactionProducts).Sum(tp => tp.Quantity)
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Log non-zero days for debugging
            foreach (var ds in dailySales.Where(d => d.Sales > 0))
            {
                Console.WriteLine($"  Non-zero day: {ds.Date:yyyy-MM-dd} Sales={ds.Sales} Transactions={ds.TransactionCount}");
            }

            // Fill in missing days with zero values
            var result = new List<SalesAnalyticsDto>();
            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var existing = dailySales.FirstOrDefault(ds => ds.Date.Date == date);
                
                if (existing != null)
                {
                    result.Add(existing);
                }
                else
                {
                    result.Add(new SalesAnalyticsDto
                    {
                        Period = date.ToString("MMM dd"),
                        Date = date,
                        Sales = 0,
                        TransactionCount = 0,
                        AverageValue = 0,
                        ItemsSold = 0
                    });
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetDailySalesAnalyticsAsync: {ex.Message}");
            return new List<SalesAnalyticsDto>();
        }
    }

    public async Task<List<HourlySalesDto>> GetHourlySalesDistributionAsync()
    {
        try
        {
            var today = DateTime.Today;
            var transactions = await _transactionRepository.GetByDateRangeAsync(today, today.AddDays(1));
            var validTransactions = FilterValidTransactions(transactions);

            var hourlySales = validTransactions
                .GroupBy(t => t.SellingTime.Hour)
                .Select(g => new HourlySalesDto
                {
                    Hour = g.Key,
                    HourLabel = $"{g.Key:D2}:00",
                    Sales = g.Sum(t => t.TotalAmount),
                    TransactionCount = g.Count()
                })
                .OrderBy(h => h.Hour)
                .ToList();

            return hourlySales;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetHourlySalesDistributionAsync: {ex.Message}");
            return new List<HourlySalesDto>();
        }
    }

    public async Task<List<SalesAnalyticsDto>> GetWeeklySalesAnalyticsAsync(int weeks = 4)
    {
        try
        {
            var startDate = DateTime.Now.AddDays(-weeks * 7).Date;
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, DateTime.Now);
            var validTransactions = FilterValidTransactions(transactions);

            var weeklySales = validTransactions
                .GroupBy(t => GetWeekNumber(t.SellingTime))
                .Select(g => new SalesAnalyticsDto
                {
                    Period = $"Week {g.Key}",
                    Date = g.Min(t => t.SellingTime.Date),
                    Sales = g.Sum(t => t.TotalAmount),
                    TransactionCount = g.Count(),
                    AverageValue = g.Count() > 0 ? g.Sum(t => t.TotalAmount) / g.Count() : 0,
                    ItemsSold = (int)g.SelectMany(t => t.TransactionProducts).Sum(tp => tp.Quantity)
                })
                .OrderBy(w => w.Date)
                .ToList();

            return weeklySales;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetWeeklySalesAnalyticsAsync: {ex.Message}");
            return new List<SalesAnalyticsDto>();
        }
    }

    public async Task<List<SalesAnalyticsDto>> GetMonthlySalesAnalyticsAsync(int months = 12)
    {
        try
        {
            var startDate = DateTime.Now.AddMonths(-months).Date;
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, DateTime.Now);
            var validTransactions = FilterValidTransactions(transactions);

            var monthlySales = validTransactions
                .GroupBy(t => new { t.SellingTime.Year, t.SellingTime.Month })
                .Select(g => new SalesAnalyticsDto
                {
                    Period = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Sales = g.Sum(t => t.TotalAmount),
                    TransactionCount = g.Count(),
                    AverageValue = g.Count() > 0 ? g.Sum(t => t.TotalAmount) / g.Count() : 0,
                    ItemsSold = (int)g.SelectMany(t => t.TransactionProducts).Sum(tp => tp.Quantity)
                })
                .OrderBy(m => m.Date)
                .ToList();

            return monthlySales;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetMonthlySalesAnalyticsAsync: {ex.Message}");
            return new List<SalesAnalyticsDto>();
        }
    }

    private int GetWeekNumber(DateTime date)
    {
        var calendar = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    #endregion

    #region Category Performance

    public async Task<List<CategorySalesDto>> GetTopCategoriesAsync(int count = 5, int days = 30)
    {
        try
        {
            var startDate = DateTime.Now.AddDays(-days);
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, DateTime.Now);
            var settledTransactions = transactions.Where(t => t.Status == "settled").ToList();

            var allTransactionProducts = await _transactionProductRepository.GetAllAsync();
            var transactionIds = settledTransactions.Select(t => t.Id).ToList();
            
            var transactionProducts = allTransactionProducts
                .Where(tp => transactionIds.Contains(tp.TransactionId))
                .ToList();

            // Get previous period for comparison
            var previousStartDate = startDate.AddDays(-days);
            var previousTransactions = await _transactionRepository.GetByDateRangeAsync(previousStartDate, startDate);
            var previousSettledTransactions = previousTransactions.Where(t => t.Status == "settled").ToList();
            var previousTransactionIds = previousSettledTransactions.Select(t => t.Id).ToList();
            
            var previousTransactionProducts = allTransactionProducts
                .Where(tp => previousTransactionIds.Contains(tp.TransactionId))
                .ToList();

            // Group by category
            var categorySales = transactionProducts
                .Where(tp => tp.Product != null && tp.Product.CategoryId > 0)
                .GroupBy(tp => tp.Product!.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Revenue = g.Sum(tp => tp.LineTotal),
                    UnitsSold = (int)g.Sum(tp => tp.Quantity),
                    ProductCount = g.Select(tp => tp.ProductId).Distinct().Count()
                })
                .OrderByDescending(cs => cs.Revenue)
                .Take(count)
                .ToList();

            var result = new List<CategorySalesDto>();
            var totalRevenue = transactionProducts.Sum(tp => tp.LineTotal);

            foreach (var cs in categorySales)
            {
                var category = await _categoryRepository.GetByIdAsync(cs.CategoryId);
                if (category != null)
                {
                    var previousRevenue = previousTransactionProducts
                        .Where(tp => tp.Product != null && tp.Product.CategoryId == cs.CategoryId)
                        .Sum(tp => tp.LineTotal);

                    var growthPercentage = previousRevenue > 0 
                        ? ((cs.Revenue - previousRevenue) / previousRevenue) * 100 
                        : 0;

                    var salesPercentage = totalRevenue > 0 ? (cs.Revenue / totalRevenue) * 100 : 0;

                    var averagePrice = cs.UnitsSold > 0 ? cs.Revenue / cs.UnitsSold : 0;

                    result.Add(new CategorySalesDto
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name,
                        Revenue = cs.Revenue,
                        SalesPercentage = salesPercentage,
                        ProductCount = cs.ProductCount,
                        UnitsSold = cs.UnitsSold,
                        GrowthPercentage = growthPercentage,
                        AveragePrice = averagePrice,
                        Color = null, // Category doesn't have a Color property
                        PreviousPeriodRevenue = previousRevenue
                    });
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetTopCategoriesAsync: {ex.Message}");
            return new List<CategorySalesDto>();
        }
    }

    #endregion

    #region Customer Insights

    public async Task<CustomerInsightsDto> GetCustomerInsightsAsync()
    {
        try
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var allCustomers = await _customerRepository.GetAllAsync();
            var activeCustomers = allCustomers.Where(c => !c.DeletedAt.HasValue).ToList();

            var newCustomersToday = activeCustomers.Count(c => c.CreatedAt.Date == today);
            var newCustomersThisWeek = activeCustomers.Count(c => c.CreatedAt >= startOfWeek);
            var newCustomersThisMonth = activeCustomers.Count(c => c.CreatedAt >= startOfMonth);

            // Get top customers
            var topCustomers = await GetTopCustomersAsync(5, 30);

            // Calculate returning customers (customers with more than 1 transaction)
            var allTransactions = await _transactionRepository.GetAllAsync();
            var settledTransactions = allTransactions.Where(t => t.Status == "settled" && t.CustomerId.HasValue).ToList();
            
            var customerTransactionCounts = settledTransactions
                .GroupBy(t => t.CustomerId)
                .Select(g => new { CustomerId = g.Key, Count = g.Count() })
                .ToList();

            var returningCustomerCount = customerTransactionCounts.Count(ct => ct.Count > 1);
            var totalCustomersWithTransactions = customerTransactionCounts.Count;
            
            var returningCustomerPercentage = totalCustomersWithTransactions > 0 
                ? ((decimal)returningCustomerCount / totalCustomersWithTransactions) * 100 
                : 0;

            // Calculate customer growth
            var lastMonth = today.AddMonths(-1);
            var customersLastMonth = activeCustomers.Count(c => c.CreatedAt < startOfMonth);
            var customerGrowthPercentage = customersLastMonth > 0 
                ? ((decimal)(activeCustomers.Count - customersLastMonth) / customersLastMonth) * 100 
                : 0;

            // Calculate average customer value
            var customerValues = settledTransactions
                .GroupBy(t => t.CustomerId)
                .Select(g => g.Sum(t => t.TotalAmount))
                .ToList();
            
            var averageCustomerValue = customerValues.Any() ? customerValues.Average() : 0;

            // Calculate average transactions per customer
            var averageTransactionsPerCustomer = totalCustomersWithTransactions > 0 
                ? (decimal)settledTransactions.Count / totalCustomersWithTransactions 
                : 0;

            return new CustomerInsightsDto
            {
                NewCustomersToday = newCustomersToday,
                NewCustomersThisWeek = newCustomersThisWeek,
                NewCustomersThisMonth = newCustomersThisMonth,
                TotalActiveCustomers = activeCustomers.Count,
                ReturningCustomerPercentage = returningCustomerPercentage,
                ReturningCustomerCount = returningCustomerCount,
                TopCustomers = topCustomers,
                CustomerGrowthPercentage = customerGrowthPercentage,
                AverageCustomerValue = averageCustomerValue,
                AverageTransactionsPerCustomer = averageTransactionsPerCustomer
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCustomerInsightsAsync: {ex.Message}");
            return new CustomerInsightsDto();
        }
    }

    public async Task<List<TopCustomerDto>> GetTopCustomersAsync(int count = 5, int days = 30)
    {
        try
        {
            var startDate = DateTime.Now.AddDays(-days);
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, DateTime.Now);
            var settledTransactions = transactions
                .Where(t => t.Status == "settled" && t.CustomerId.HasValue)
                .ToList();

            var customerSales = settledTransactions
                .GroupBy(t => t.CustomerId!.Value)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    TotalSpent = g.Sum(t => t.TotalAmount),
                    TransactionCount = g.Count(),
                    LastTransactionDate = g.Max(t => t.SellingTime),
                    AverageTransactionValue = g.Average(t => t.TotalAmount)
                })
                .OrderByDescending(cs => cs.TotalSpent)
                .Take(count)
                .ToList();

            var result = new List<TopCustomerDto>();

            foreach (var cs in customerSales)
            {
                var customer = await _customerRepository.GetByIdAsync(cs.CustomerId);
                if (customer != null)
                {
                    result.Add(new TopCustomerDto
                    {
                        CustomerId = customer.Id,
                        CustomerName = customer.DisplayName,
                        PhoneNumber = customer.PrimaryMobile,
                        TotalSpent = cs.TotalSpent,
                        TransactionCount = cs.TransactionCount,
                        LastTransactionDate = cs.LastTransactionDate,
                        LoyaltyPoints = null, // Not available in current Customer entity
                        AverageTransactionValue = cs.AverageTransactionValue
                    });
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetTopCustomersAsync: {ex.Message}");
            return new List<TopCustomerDto>();
        }
    }

    #endregion

    #region Quick Stats

    public async Task<Dictionary<string, object>> GetQuickStatsAsync()
    {
        try
        {
            var stats = new Dictionary<string, object>();
            var kpis = await GetDashboardKpisAsync();

            stats["TodaysSales"] = kpis.TodaysSales;
            stats["MonthlySales"] = kpis.MonthlySales;
            stats["ActiveTables"] = kpis.ActiveTables;
            stats["PendingOrders"] = kpis.PendingOrders;
            stats["LowStockItems"] = kpis.LowStockItems;
            stats["TotalCustomers"] = kpis.TotalCustomers;
            stats["AverageTransactionValue"] = kpis.AverageTransactionValue;
            stats["TodaysTransactionCount"] = kpis.TodaysTransactionCount;

            return stats;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetQuickStatsAsync: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    #endregion
}
