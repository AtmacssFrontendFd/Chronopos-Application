using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents an order in the restaurant (matches orders table)
/// </summary>
public class Order
{
    public int Id { get; set; }

    // Table and Customer Information
    public int? TableId { get; set; }
    public virtual RestaurantTable? Table { get; set; }

    public int? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    public int? ReservationId { get; set; }
    public virtual Reservation? Reservation { get; set; }

    // Financial Information
    public decimal TotalAmount { get; set; } = 0.00m;

    public decimal Discount { get; set; } = 0.00m;

    // FinalAmount is computed in the database but we can also calculate it here
    public decimal FinalAmount => TotalAmount - Discount;

    // Payment Information
    public int? PaymentTypeId { get; set; }
    public virtual PaymentType? PaymentType { get; set; }

    // Order Status
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "pending"; // pending, in_progress, served, completed, cancelled

    // Audit Fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // Computed Properties
    public int ItemCount => OrderItems?.Count ?? 0;

    public bool HasItems => OrderItems?.Any() == true;

    public string StatusDisplay => Status switch
    {
        "pending" => "Pending",
        "in_progress" => "In Progress",
        "served" => "Served",
        "completed" => "Completed",
        "cancelled" => "Cancelled",
        _ => Status
    };

    public bool IsPending => Status?.ToLower() == "pending";

    public bool IsInProgress => Status?.ToLower() == "in_progress";

    public bool IsServed => Status?.ToLower() == "served";

    public bool IsCompleted => Status?.ToLower() == "completed";

    public bool IsCancelled => Status?.ToLower() == "cancelled";
}
