using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents an item in an order (matches order_items table)
/// </summary>
public class OrderItem
{
    public int Id { get; set; }

    // Order Reference
    public int OrderId { get; set; }
    public virtual Order? Order { get; set; }

    // Product/Menu Item Reference
    public int? ProductId { get; set; }
    public virtual Product? Product { get; set; }

    public int? MenuItemId { get; set; }
    // Note: MenuItem entity doesn't exist yet, so we'll keep this as int? for now
    // If you create a MenuItem entity later, add: public virtual MenuItem? MenuItem { get; set; }

    // Item Details
    public int Quantity { get; set; }

    public decimal Price { get; set; }

    [StringLength(255)]
    public string? Notes { get; set; }

    // Item Status
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "pending"; // pending, preparing, served, cancelled

    // Computed Properties
    public decimal LineTotal => Quantity * Price;

    public string StatusDisplay => Status switch
    {
        "pending" => "Pending",
        "preparing" => "Preparing",
        "served" => "Served",
        "cancelled" => "Cancelled",
        _ => Status
    };

    public bool IsPending => Status?.ToLower() == "pending";

    public bool IsPreparing => Status?.ToLower() == "preparing";

    public bool IsServed => Status?.ToLower() == "served";

    public bool IsCancelled => Status?.ToLower() == "cancelled";

    public string ProductName => Product?.Name ?? $"Item #{ProductId}";
}
