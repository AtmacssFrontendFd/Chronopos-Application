namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Customer Insights and Analytics
/// </summary>
public class CustomerInsightsDto
{
    /// <summary>
    /// Number of new customers created today
    /// </summary>
    public int NewCustomersToday { get; set; }

    /// <summary>
    /// Number of new customers created this week
    /// </summary>
    public int NewCustomersThisWeek { get; set; }

    /// <summary>
    /// Number of new customers created this month
    /// </summary>
    public int NewCustomersThisMonth { get; set; }

    /// <summary>
    /// Total number of active customers
    /// </summary>
    public int TotalActiveCustomers { get; set; }

    /// <summary>
    /// Percentage of returning customers
    /// </summary>
    public decimal ReturningCustomerPercentage { get; set; }

    /// <summary>
    /// Number of returning customers in current period
    /// </summary>
    public int ReturningCustomerCount { get; set; }

    /// <summary>
    /// Top spending customers
    /// </summary>
    public List<TopCustomerDto> TopCustomers { get; set; } = new();

    /// <summary>
    /// Customer growth trend (percentage)
    /// </summary>
    public decimal CustomerGrowthPercentage { get; set; }

    /// <summary>
    /// Average customer lifetime value
    /// </summary>
    public decimal AverageCustomerValue { get; set; }

    /// <summary>
    /// Average number of transactions per customer
    /// </summary>
    public decimal AverageTransactionsPerCustomer { get; set; }
}

/// <summary>
/// Data Transfer Object for Top Customer Information
/// </summary>
public class TopCustomerDto
{
    /// <summary>
    /// Customer identifier
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Customer name
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Customer phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Total amount spent by customer
    /// </summary>
    public decimal TotalSpent { get; set; }

    /// <summary>
    /// Number of transactions made by customer
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Last transaction date
    /// </summary>
    public DateTime? LastTransactionDate { get; set; }

    /// <summary>
    /// Customer loyalty points (if applicable)
    /// </summary>
    public int? LoyaltyPoints { get; set; }

    /// <summary>
    /// Average transaction value for this customer
    /// </summary>
    public decimal AverageTransactionValue { get; set; }
}
