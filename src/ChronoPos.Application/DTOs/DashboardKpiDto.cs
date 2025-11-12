namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Dashboard Key Performance Indicators
/// </summary>
public class DashboardKpiDto
{
    /// <summary>
    /// Total sales for the current day
    /// </summary>
    public decimal TodaysSales { get; set; }

    /// <summary>
    /// Total sales for the current month
    /// </summary>
    public decimal MonthlySales { get; set; }

    /// <summary>
    /// Growth percentage compared to previous period
    /// </summary>
    public decimal GrowthPercentage { get; set; }

    /// <summary>
    /// Number of currently active/occupied tables (restaurant mode)
    /// </summary>
    public int ActiveTables { get; set; }

    /// <summary>
    /// Number of pending orders (draft/billed status)
    /// </summary>
    public int PendingOrders { get; set; }

    /// <summary>
    /// Number of products below reorder level
    /// </summary>
    public int LowStockItems { get; set; }

    /// <summary>
    /// Total number of active customers
    /// </summary>
    public int TotalCustomers { get; set; }

    /// <summary>
    /// Average transaction value (total sales / number of transactions)
    /// </summary>
    public decimal AverageTransactionValue { get; set; }

    /// <summary>
    /// Total number of transactions today
    /// </summary>
    public int TodaysTransactionCount { get; set; }

    /// <summary>
    /// Sales for yesterday (for comparison)
    /// </summary>
    public decimal YesterdaysSales { get; set; }

    /// <summary>
    /// Sales for last month (for comparison)
    /// </summary>
    public decimal LastMonthSales { get; set; }
}
