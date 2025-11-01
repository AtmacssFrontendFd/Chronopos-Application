namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Recent Sales Display on Dashboard
/// </summary>
public class RecentSaleDto
{
    /// <summary>
    /// Transaction identifier
    /// </summary>
    public int TransactionId { get; set; }

    /// <summary>
    /// Invoice number
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Transaction/selling time
    /// </summary>
    public DateTime SellingTime { get; set; }

    /// <summary>
    /// Time formatted as HH:mm
    /// </summary>
    public string TimeFormatted => SellingTime.ToString("HH:mm");

    /// <summary>
    /// Date formatted
    /// </summary>
    public string DateFormatted => SellingTime.ToString("dd MMM yyyy");

    /// <summary>
    /// Customer name or "Walk-in Customer"
    /// </summary>
    public string CustomerName { get; set; } = "Walk-in Customer";

    /// <summary>
    /// Customer ID (nullable for walk-in)
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// List of product names in the transaction
    /// </summary>
    public List<string> ProductNames { get; set; } = new();

    /// <summary>
    /// Comma-separated product names for display
    /// </summary>
    public string ProductNamesDisplay => string.Join(", ", ProductNames.Take(3)) + (ProductNames.Count > 3 ? "..." : "");

    /// <summary>
    /// Total number of items
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// Total amount of the transaction
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Transaction status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Payment method/type
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Color code for status display
    /// </summary>
    public string StatusColor
    {
        get
        {
            return Status?.ToLower() switch
            {
                "settled" => "#28A745", // Green
                "billed" => "#FFC107",  // Yellow
                "draft" => "#6C757D",   // Gray
                "cancelled" => "#DC3545", // Red
                "refunded" => "#17A2B8", // Info Blue
                _ => "#6C757D"
            };
        }
    }

    /// <summary>
    /// User who created the transaction
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Time elapsed since transaction (for real-time display)
    /// </summary>
    public string TimeElapsed
    {
        get
        {
            var elapsed = DateTime.Now - SellingTime;
            if (elapsed.TotalMinutes < 1)
                return "Just now";
            if (elapsed.TotalMinutes < 60)
                return $"{(int)elapsed.TotalMinutes} min ago";
            if (elapsed.TotalHours < 24)
                return $"{(int)elapsed.TotalHours} hour{((int)elapsed.TotalHours > 1 ? "s" : "")} ago";
            return $"{(int)elapsed.TotalDays} day{((int)elapsed.TotalDays > 1 ? "s" : "")} ago";
        }
    }
}
