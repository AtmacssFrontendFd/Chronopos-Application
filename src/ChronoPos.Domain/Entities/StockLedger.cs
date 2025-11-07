using ChronoPos.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Stock movement history ledger
/// </summary>
[Table("stock_ledger")]
public class StockLedger
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("unit_id")]
    public int? UnitId { get; set; }

    [Required]
    [Column("movement_type")]
    public StockMovementType MovementType { get; set; }

    [Required]
    [Column("qty", TypeName = "decimal(10,2)")]
    public decimal Qty { get; set; }

    [Required]
    [Column("balance", TypeName = "decimal(10,2)")]
    public decimal Balance { get; set; }

    [Column("location")]
    [MaxLength(200)]
    public string? Location { get; set; }

    [Column("reference_type")]
    public StockReferenceType? ReferenceType { get; set; }

    [Column("reference_id")]
    public int? ReferenceId { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("note")]
    public string? Note { get; set; }

    // Navigation Properties
    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }

    [ForeignKey("UnitId")]
    public virtual ProductUnit? Unit { get; set; }
}
