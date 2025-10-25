using System;

namespace ChronoPos.Domain.Entities
{
    /// <summary>
    /// Links modifiers to modifier groups with price adjustments and ordering
    /// </summary>
    public class ProductModifierGroupItem
    {
        public int Id { get; set; }
        
        public int GroupId { get; set; }
        
        public int ModifierId { get; set; }
        
        public decimal PriceAdjustment { get; set; }
        
        public int SortOrder { get; set; }
        
        public bool DefaultSelection { get; set; }
        
        public string Status { get; set; } = "Active";
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public virtual ProductModifierGroup? Group { get; set; }
        
        public virtual ProductModifier? Modifier { get; set; }
    }
}
