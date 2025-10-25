using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for OrderItem operations
/// </summary>
public class OrderItemDto
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int? ProductId { get; set; }
    public string? ProductName { get; set; }

    public int? MenuItemId { get; set; }
    public string? MenuItemName { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public string? Notes { get; set; }

    [Required]
    public string Status { get; set; } = "pending";

    // Display Properties
    public decimal LineTotal => Quantity * Price;
    public string StatusDisplay => Status switch
    {
        "pending" => "Pending",
        "preparing" => "Preparing",
        "served" => "Served",
        "cancelled" => "Cancelled",
        _ => Status
    };
    public string ProductDisplay => ProductName ?? MenuItemName ?? "-";
    public string PriceDisplay => $"{Price:C2}";
    public string LineTotalDisplay => $"{LineTotal:C2}";
    public string NotesDisplay => Notes ?? "-";
}

/// <summary>
/// DTO for creating a new order item
/// </summary>
public class CreateOrderItemDto
{
    public int OrderId { get; set; }
    public int? ProductId { get; set; }
    public int? MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "pending";
}

/// <summary>
/// DTO for updating an existing order item
/// </summary>
public class UpdateOrderItemDto
{
    public int? ProductId { get; set; }
    public int? MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "pending";
}
