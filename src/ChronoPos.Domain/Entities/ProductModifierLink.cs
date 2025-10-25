namespace ChronoPos.Domain.Entities;

/// <summary>
/// Links products to modifier groups
/// </summary>
public class ProductModifierLink
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ModifierGroupId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual Product? Product { get; set; }
    public virtual ProductModifierGroup? ModifierGroup { get; set; }
}
