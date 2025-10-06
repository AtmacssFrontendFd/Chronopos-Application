using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for displaying goods received information
/// </summary>
public class GoodsReceivedDto
{
    public int Id { get; set; }
    public string GrnNo { get; set; } = string.Empty;
    public long SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int StoreId { get; set; }
    public string? StoreName { get; set; }
    public string? InvoiceNo { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime ReceivedDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Remarks { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public List<GoodsReceivedItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for creating a new goods received transaction
/// </summary>
public class CreateGoodsReceivedDto
{
    [Required]
    [StringLength(50)]
    public string GrnNo { get; set; } = string.Empty;
    
    [Required]
    public long SupplierId { get; set; }
    
    [Required]
    public int StoreId { get; set; }
    
    [StringLength(50)]
    public string? InvoiceNo { get; set; }
    
    public DateTime? InvoiceDate { get; set; }
    
    [Required]
    public DateTime ReceivedDate { get; set; } = DateTime.Today;
    
    public decimal TotalAmount { get; set; } = 0;
    
    [StringLength(255)]
    public string? Remarks { get; set; }
    
    [StringLength(20)]
    public string Status { get; set; } = "Pending";
    
    public List<CreateGoodsReceivedItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for updating goods received transaction
/// </summary>
public class UpdateGoodsReceivedDto
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string GrnNo { get; set; } = string.Empty;
    
    [Required]
    public long SupplierId { get; set; }
    
    [Required]
    public int StoreId { get; set; }
    
    [StringLength(50)]
    public string? InvoiceNo { get; set; }
    
    public DateTime? InvoiceDate { get; set; }
    
    [Required]
    public DateTime ReceivedDate { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    [StringLength(255)]
    public string? Remarks { get; set; }
    
    [StringLength(20)]
    public string Status { get; set; } = "Pending";
    
    public List<UpdateGoodsReceivedItemDto> Items { get; set; } = new();
}