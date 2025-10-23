using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for displaying goods received item information
/// </summary>
public class GoodsReceivedItemDto
{
    public int Id { get; set; }
    public int GrnId { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public int? BatchId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public long UomId { get; set; }
    public string? UomName { get; set; }
    public decimal CostPrice { get; set; }
    public decimal? LandedCost { get; set; }
    public decimal LineTotal { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new goods received item
/// </summary>
public class CreateGoodsReceivedItemDto
{
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
    [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }
    
    [Required]
    public long UomId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Cost price must be greater than 0")]
    public decimal CostPrice { get; set; }
    
    public decimal? LandedCost { get; set; }
}

/// <summary>
/// DTO for updating goods received item
/// </summary>
public class UpdateGoodsReceivedItemDto
{
    [Required]
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
    [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }
    
    [Required]
    public long UomId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Cost price must be greater than 0")]
    public decimal CostPrice { get; set; }
    
    public decimal? LandedCost { get; set; }
}