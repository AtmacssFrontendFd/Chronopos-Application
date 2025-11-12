namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Category Sales Performance
/// </summary>
public class CategorySalesDto
{
    /// <summary>
    /// Category identifier
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Category name
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Total revenue from this category
    /// </summary>
    public decimal Revenue { get; set; }

    /// <summary>
    /// Percentage contribution to total sales
    /// </summary>
    public decimal SalesPercentage { get; set; }

    /// <summary>
    /// Number of products in this category
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// Total units sold in this category
    /// </summary>
    public int UnitsSold { get; set; }

    /// <summary>
    /// Growth percentage compared to previous period
    /// </summary>
    public decimal GrowthPercentage { get; set; }

    /// <summary>
    /// Average product price in this category
    /// </summary>
    public decimal AveragePrice { get; set; }

    /// <summary>
    /// Category color or icon
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Previous period revenue for comparison
    /// </summary>
    public decimal PreviousPeriodRevenue { get; set; }
}
