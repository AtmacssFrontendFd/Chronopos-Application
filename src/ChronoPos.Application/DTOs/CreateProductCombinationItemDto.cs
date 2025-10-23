using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for creating ProductCombinationItem
/// </summary>
public class CreateProductCombinationItemDto
{
    [Required]
    public int ProductUnitId { get; set; }
    
    [Required]
    public int AttributeValueId { get; set; }
}