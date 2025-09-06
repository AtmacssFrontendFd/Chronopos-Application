using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Stock Level operations
/// </summary>
public class StockLevelDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public int ProductId { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal ReservedStock { get; set; }
    public decimal AvailableStock => CurrentStock - ReservedStock;
    public decimal AverageCost { get; set; }
    public decimal LastCost { get; set; }
    public DateTime LastUpdated { get; set; }
    public string StoreName { get; set; } = string.Empty;
}

/// <summary>
/// Data Transfer Object for Stock Movement operations
/// </summary>
public class StockMovementDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public int ProductId { get; set; }
    public string MovementType { get; set; } = string.Empty; // IN, OUT, ADJUSTMENT
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ReferenceType { get; set; } // INITIAL, PURCHASE, SALE, ADJUSTMENT
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public decimal PreviousStock { get; set; }
    public decimal NewStock { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime Created { get; set; }
    public string StoreName { get; set; } = string.Empty;
}

/// <summary>
/// Result class for stock save operations
/// </summary>
public class StockSaveResult
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public List<string> Warnings { get; set; } = new List<string>();
    public int? CreatedStockLevelId { get; set; }
    public int? CreatedMovementId { get; set; }
}

/// <summary>
/// Stock control validation result
/// </summary>
public class StockValidationResult
{
    public bool IsValid => !Errors.Any();
    public List<string> Errors { get; set; } = new List<string>();
    public List<string> Warnings { get; set; } = new List<string>();
}