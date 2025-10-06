using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents individual items in a goods received transaction
/// </summary>
public class GoodsReceivedItem
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int GrnId { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    public int? BatchId { get; set; }
    
    [StringLength(50)]
    public string? BatchNo { get; set; }
    
    public DateTime? ManufactureDate { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(12,4)")]
    public decimal Quantity { get; set; }
    
    [Required]
    public long UomId { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(12,2)")]
    public decimal CostPrice { get; set; }
    
    [Column(TypeName = "decimal(12,2)")]
    public decimal? LandedCost { get; set; }
    
    [Column(TypeName = "decimal(12,2)")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal LineTotal { get; private set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual GoodsReceived? GoodsReceived { get; set; }
    public virtual Product? Product { get; set; }
    public virtual ProductBatch? ProductBatch { get; set; }
    public virtual UnitOfMeasurement? UnitOfMeasurement { get; set; }
}