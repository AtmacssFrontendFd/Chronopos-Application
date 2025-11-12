namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Sales Analytics/Chart Data
/// </summary>
public class SalesAnalyticsDto
{
    /// <summary>
    /// Date or time period label
    /// </summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>
    /// Date value for the analytics point
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Total sales amount for the period
    /// </summary>
    public decimal Sales { get; set; }

    /// <summary>
    /// Number of transactions in the period
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Average transaction value for the period
    /// </summary>
    public decimal AverageValue { get; set; }

    /// <summary>
    /// Number of items sold in the period
    /// </summary>
    public int ItemsSold { get; set; }

    /// <summary>
    /// Profit amount (if applicable)
    /// </summary>
    public decimal? Profit { get; set; }

    /// <summary>
    /// Growth percentage compared to previous period
    /// </summary>
    public decimal? GrowthPercentage { get; set; }
}

/// <summary>
/// Data Transfer Object for Hourly Sales Distribution
/// </summary>
public class HourlySalesDto
{
    /// <summary>
    /// Hour of the day (0-23)
    /// </summary>
    public int Hour { get; set; }

    /// <summary>
    /// Display label (e.g., "9:00 AM")
    /// </summary>
    public string HourLabel { get; set; } = string.Empty;

    /// <summary>
    /// Total sales for this hour
    /// </summary>
    public decimal Sales { get; set; }

    /// <summary>
    /// Number of transactions in this hour
    /// </summary>
    public int TransactionCount { get; set; }
}
