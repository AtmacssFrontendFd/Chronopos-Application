namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for ProductModifierLink
/// </summary>
public class ProductModifierLinkDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ModifierGroupId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Additional properties for display
    public string? ProductName { get; set; }
    public string? ProductSku { get; set; }
    public string? ModifierGroupName { get; set; }
}

/// <summary>
/// DTO for creating ProductModifierLink
/// </summary>
public class CreateProductModifierLinkDto
{
    public int ProductId { get; set; }
    public int ModifierGroupId { get; set; }
}

/// <summary>
/// DTO for updating ProductModifierLink
/// </summary>
public class UpdateProductModifierLinkDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ModifierGroupId { get; set; }
}
