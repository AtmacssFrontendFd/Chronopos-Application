/* This C# code snippet defines a class named `StockTransfer` within the `ChronoPos.Domain.Entities`
namespace. Here's a breakdown of what the code is doing: */
using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a stock transfer between locations in the system
/// </summary>
public class StockTransfer
{
    public int TransferId { get; set; }
    
    [Required]
    [StringLength(30)]
    public string TransferNo { get; set; } = string.Empty;
    
    [Required]
    public DateTime TransferDate { get; set; }
    
    [Required]
    public int FromStoreId { get; set; }
    
    [Required]
    public int ToStoreId { get; set; }
    
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, In-Transit, Completed, Cancelled
    
    public string? Remarks { get; set; }
    
    [Required]
    public int CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual Store? FromStore { get; set; }
    public virtual Store? ToStore { get; set; }
    public virtual User? Creator { get; set; }
    public virtual User? Updater { get; set; }
    public virtual ICollection<StockTransferItem> Items { get; set; } = new List<StockTransferItem>();
}
