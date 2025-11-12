using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Order operations
/// </summary>
public class OrderDto
{
    public int Id { get; set; }

    public int? TableId { get; set; }
    public string? TableName { get; set; }

    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }

    public int? ReservationId { get; set; }

    public decimal TotalAmount { get; set; } = 0.00m;

    public decimal Discount { get; set; } = 0.00m;

    public decimal FinalAmount { get; set; } = 0.00m;

    public int? PaymentTypeId { get; set; }
    public string? PaymentTypeName { get; set; }

    [Required]
    public string Status { get; set; } = "pending";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Related Data
    public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();

    // Display Properties
    public int ItemCount => OrderItems?.Count ?? 0;
    public string StatusDisplay => Status switch
    {
        "pending" => "Pending",
        "in_progress" => "In Progress",
        "served" => "Served",
        "completed" => "Completed",
        "cancelled" => "Cancelled",
        _ => Status
    };
    public string TableDisplay => TableName ?? "-";
    public string CustomerDisplay => CustomerName ?? "-";
    public string PaymentTypeDisplay => PaymentTypeName ?? "-";
    public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    public string UpdatedAtFormatted => UpdatedAt.ToString("dd/MM/yyyy HH:mm");
    public string TotalAmountDisplay => $"{TotalAmount:C2}";
    public string DiscountDisplay => $"{Discount:C2}";
    public string FinalAmountDisplay => $"{FinalAmount:C2}";
}

/// <summary>
/// DTO for creating a new order
/// </summary>
public class CreateOrderDto
{
    public int? TableId { get; set; }
    public int? CustomerId { get; set; }
    public int? ReservationId { get; set; }
    public decimal TotalAmount { get; set; } = 0.00m;
    public decimal Discount { get; set; } = 0.00m;
    public int? PaymentTypeId { get; set; }
    public string Status { get; set; } = "pending";
    public List<CreateOrderItemDto> OrderItems { get; set; } = new List<CreateOrderItemDto>();
}

/// <summary>
/// DTO for updating an existing order
/// </summary>
public class UpdateOrderDto
{
    public int? TableId { get; set; }
    public int? CustomerId { get; set; }
    public int? ReservationId { get; set; }
    public decimal TotalAmount { get; set; } = 0.00m;
    public decimal Discount { get; set; } = 0.00m;
    public int? PaymentTypeId { get; set; }
    public string Status { get; set; } = "pending";
}
