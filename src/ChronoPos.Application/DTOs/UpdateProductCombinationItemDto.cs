using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for updating ProductCombinationItem
/// </summary>
public class UpdateProductCombinationItemDto
{
    public int Id { get; set; }
    
    [Required]
    public int ProductUnitId { get; set; }
    
    [Required]
    public int AttributeValueId { get; set; }
}