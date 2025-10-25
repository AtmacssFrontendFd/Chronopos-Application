using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a work shift for tracking sales transactions
/// </summary>
public class Shift
{
    public int ShiftId { get; set; }
    
    public int? UserId { get; set; }
    
    public int? ShopLocationId { get; set; }
    
    [Required]
    public DateTime StartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    public decimal OpeningCash { get; set; } = 0;
    
    public decimal ClosingCash { get; set; } = 0;
    
    public decimal ExpectedCash { get; set; } = 0;
    
    public decimal CashDifference { get; set; } = 0;
    
    [StringLength(20)]
    public string Status { get; set; } = "Open"; // Open, Closed
    
    public string? Notes { get; set; }
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual User? User { get; set; }
    
    public virtual ShopLocation? ShopLocation { get; set; }
    
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    
    public virtual ICollection<RefundTransaction> RefundTransactions { get; set; } = new List<RefundTransaction>();
    
    public virtual ICollection<ExchangeTransaction> ExchangeTransactions { get; set; } = new List<ExchangeTransaction>();
    
    /// <summary>
    /// Returns the shift display information
    /// </summary>
    public string DisplayInfo => $"Shift #{ShiftId} - {StartTime:dd/MM/yyyy HH:mm}";
    
    /// <summary>
    /// Returns the shift duration
    /// </summary>
    public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
    
    /// <summary>
    /// Returns formatted duration
    /// </summary>
    public string DurationDisplay => Duration.HasValue ? $"{Duration.Value.Hours}h {Duration.Value.Minutes}m" : "In Progress";
}
