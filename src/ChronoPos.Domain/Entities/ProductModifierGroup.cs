using System;

namespace ChronoPos.Domain.Entities
{
    /// <summary>
    /// Represents a group of modifiers (e.g., Pizza Toppings, Drink Sizes)
    /// </summary>
    public class ProductModifierGroup
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        /// <summary>
        /// Selection type: 'Single' or 'Multiple'
        /// </summary>
        public string SelectionType { get; set; } = "Multiple";
        
        public bool Required { get; set; }
        
        public int MinSelections { get; set; }
        
        public int? MaxSelections { get; set; }
        
        public string Status { get; set; } = "Active";
        
        public int CreatedBy { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual User? Creator { get; set; }
        
        public virtual ICollection<ProductModifierGroupItem>? GroupItems { get; set; }
    }
}
