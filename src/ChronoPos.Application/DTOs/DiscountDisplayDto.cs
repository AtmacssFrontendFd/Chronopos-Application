namespace ChronoPos.Application.DTOs;

/// <summary>
/// Lightweight discount information for display purposes in product tables
/// </summary>
public class DiscountDisplayDto
{
    public int Id { get; set; }
    public string DiscountName { get; set; } = string.Empty;
    public string DiscountCode { get; set; } = string.Empty;
    public string FormattedDiscountValue { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsCurrentlyActive { get; set; }
    public bool IsStackable { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Short display text for product table
    /// </summary>
    public string ShortDisplay => $"{DiscountCode} ({FormattedDiscountValue})";
    
    /// <summary>
    /// Compact display for product tables showing first discount and count
    /// </summary>
    public static string GetCompactDisplay(List<DiscountDisplayDto> discounts)
    {
        if (discounts == null || !discounts.Any())
            return "No discounts";
            
        if (discounts.Count == 1)
            return discounts.First().DiscountCode;
            
        var firstDiscount = discounts.First().DiscountCode;
        var additionalCount = discounts.Count - 1;
        return $"{firstDiscount} +{additionalCount}";
    }
}