using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Dashboard operations and analytics
/// </summary>
public interface IDashboardService
{
    #region KPI Methods

    /// <summary>
    /// Get all Key Performance Indicators for the dashboard
    /// </summary>
    /// <returns>Dashboard KPI data</returns>
    Task<DashboardKpiDto> GetDashboardKpisAsync();

    /// <summary>
    /// Get total sales for today
    /// </summary>
    /// <returns>Today's sales amount</returns>
    Task<decimal> GetTodaysSalesAsync();

    /// <summary>
    /// Get total sales for the current month
    /// </summary>
    /// <returns>Monthly sales amount</returns>
    Task<decimal> GetMonthlySalesAsync();

    /// <summary>
    /// Get number of active/occupied restaurant tables
    /// </summary>
    /// <returns>Active tables count</returns>
    Task<int> GetActiveTablesCountAsync();

    /// <summary>
    /// Get number of products below reorder level
    /// </summary>
    /// <returns>Low stock items count</returns>
    Task<int> GetLowStockItemsCountAsync();

    /// <summary>
    /// Get number of pending orders (draft/billed status)
    /// </summary>
    /// <returns>Pending orders count</returns>
    Task<int> GetPendingOrdersCountAsync();

    #endregion

    #region Popular Products

    /// <summary>
    /// Get list of popular/best-selling products
    /// </summary>
    /// <param name="count">Number of products to return (default 6)</param>
    /// <param name="days">Number of days to look back (default 7)</param>
    /// <returns>List of popular products with sales data</returns>
    Task<List<ProductSalesDto>> GetPopularProductsAsync(int count = 6, int days = 7);

    #endregion

    #region Recent Sales

    /// <summary>
    /// Get recent sales/transactions
    /// </summary>
    /// <param name="count">Number of recent sales to return (default 10)</param>
    /// <returns>List of recent sales</returns>
    Task<List<RecentSaleDto>> GetRecentSalesAsync(int count = 10);

    #endregion

    #region Sales Analytics

    /// <summary>
    /// Get sales analytics data for charts
    /// </summary>
    /// <param name="days">Number of days to look back (default 30)</param>
    /// <returns>Daily sales analytics data</returns>
    Task<List<SalesAnalyticsDto>> GetDailySalesAnalyticsAsync(int days = 30);

    /// <summary>
    /// Get hourly sales distribution for today
    /// </summary>
    /// <returns>Hourly sales data</returns>
    Task<List<HourlySalesDto>> GetHourlySalesDistributionAsync();

    /// <summary>
    /// Get weekly sales analytics
    /// </summary>
    /// <param name="weeks">Number of weeks to look back (default 4)</param>
    /// <returns>Weekly sales analytics data</returns>
    Task<List<SalesAnalyticsDto>> GetWeeklySalesAnalyticsAsync(int weeks = 4);

    /// <summary>
    /// Get monthly sales analytics
    /// </summary>
    /// <param name="months">Number of months to look back (default 12)</param>
    /// <returns>Monthly sales analytics data</returns>
    Task<List<SalesAnalyticsDto>> GetMonthlySalesAnalyticsAsync(int months = 12);

    #endregion

    #region Category Performance

    /// <summary>
    /// Get top performing categories
    /// </summary>
    /// <param name="count">Number of categories to return (default 5)</param>
    /// <param name="days">Number of days to look back (default 30)</param>
    /// <returns>List of top categories with sales data</returns>
    Task<List<CategorySalesDto>> GetTopCategoriesAsync(int count = 5, int days = 30);

    #endregion

    #region Customer Insights

    /// <summary>
    /// Get customer insights and analytics
    /// </summary>
    /// <returns>Customer insights data</returns>
    Task<CustomerInsightsDto> GetCustomerInsightsAsync();

    /// <summary>
    /// Get top spending customers
    /// </summary>
    /// <param name="count">Number of customers to return (default 5)</param>
    /// <param name="days">Number of days to look back (default 30)</param>
    /// <returns>List of top customers</returns>
    Task<List<TopCustomerDto>> GetTopCustomersAsync(int count = 5, int days = 30);

    #endregion

    #region Quick Stats

    /// <summary>
    /// Get quick statistics for dashboard summary
    /// </summary>
    /// <returns>Dictionary of stat name to value</returns>
    Task<Dictionary<string, object>> GetQuickStatsAsync();

    #endregion
}
