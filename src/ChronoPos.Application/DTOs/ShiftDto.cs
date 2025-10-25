using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Shift operations
/// </summary>
public class ShiftDto
{
    public int ShiftId { get; set; }
    public int? UserId { get; set; }
    public int? ShopLocationId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal? ClosingCash { get; set; }
    public decimal? ExpectedCash { get; set; }
    public decimal? CashDifference { get; set; }
    public string Status { get; set; } = "Open";
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public string? UserName { get; set; }
    public string? ShopLocationName { get; set; }
    
    // Calculated properties
    public int TransactionCount { get; set; }
    public decimal TotalSales { get; set; }
    
    // Display properties
    public string DisplayInfo => $"Shift #{ShiftId} - {StartTime:dd/MM/yyyy HH:mm}";
    public string? Duration
    {
        get
        {
            if (EndTime.HasValue)
            {
                var duration = EndTime.Value - StartTime;
                return $"{duration.Hours}h {duration.Minutes}m";
            }
            return "In Progress";
        }
    }
    public string StatusDisplay => Status == "Open" ? "Open" : "Closed";
}

/// <summary>
/// DTO for creating a new shift
/// </summary>
public class CreateShiftDto
{
    public int? UserId { get; set; }
    
    public int? ShopLocationId { get; set; }
    
    public decimal? OpeningCash { get; set; }
    
    public string? Note { get; set; }
}

/// <summary>
/// DTO for updating an existing shift
/// </summary>
public class UpdateShiftDto
{
    public decimal? OpeningCash { get; set; }
    
    public string? Note { get; set; }
}

/// <summary>
/// DTO for closing a shift
/// </summary>
public class CloseShiftDto
{
    [Required]
    public decimal ClosingCash { get; set; }
    
    public string? Note { get; set; }
}

/// <summary>
/// DTO for shift summary with transaction statistics
/// </summary>
public class ShiftSummaryDto
{
    public int ShiftId { get; set; }
    public int? UserId { get; set; }
    
    [Display(Name = "User")]
    public string? UserName { get; set; }

    [Display(Name = "Start Time")]
    public DateTime StartTime { get; set; }

    [Display(Name = "End Time")]
    public DateTime? EndTime { get; set; }

    [Display(Name = "Duration")]
    public string? Duration { get; set; }

    [Display(Name = "Status")]
    public string Status { get; set; } = "Open";

    [Display(Name = "Opening Cash")]
    public decimal OpeningCash { get; set; }

    [Display(Name = "Closing Cash")]
    public decimal? ClosingCash { get; set; }

    [Display(Name = "Expected Cash")]
    public decimal? ExpectedCash { get; set; }

    [Display(Name = "Cash Difference")]
    public decimal? CashDifference { get; set; }

    // Transaction statistics
    [Display(Name = "Total Transactions")]
    public int TotalTransactions { get; set; }

    [Display(Name = "Total Sales")]
    public decimal TotalSales { get; set; }

    [Display(Name = "Cash Sales")]
    public decimal TotalCashSales { get; set; }

    [Display(Name = "Card Sales")]
    public decimal TotalCardSales { get; set; }

    [Display(Name = "Credit Sales")]
    public decimal TotalCreditSales { get; set; }

    [Display(Name = "Total VAT")]
    public decimal TotalVat { get; set; }

    [Display(Name = "Total Discount")]
    public decimal TotalDiscount { get; set; }

    // Transaction status breakdown
    [Display(Name = "Draft")]
    public int DraftTransactions { get; set; }

    [Display(Name = "Hold")]
    public int HoldTransactions { get; set; }

    [Display(Name = "Billed")]
    public int BilledTransactions { get; set; }

    [Display(Name = "Settled")]
    public int SettledTransactions { get; set; }

    [Display(Name = "Cancelled")]
    public int CancelledTransactions { get; set; }

    [Display(Name = "Pending Payment")]
    public int PendingPaymentTransactions { get; set; }

    [Display(Name = "Partial Payment")]
    public int PartialPaymentTransactions { get; set; }
}
