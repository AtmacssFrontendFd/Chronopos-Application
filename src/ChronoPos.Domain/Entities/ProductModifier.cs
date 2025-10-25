using System;

namespace ChronoPos.Domain.Entities
{
    /// <summary>
    /// Represents an individual product modifier/add-on
    /// </summary>
    public class ProductModifier
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public decimal Price { get; set; }
        
        public decimal Cost { get; set; }
        
        public string? Sku { get; set; }
        
        public string? Barcode { get; set; }
        
        public int? TaxTypeId { get; set; }
        
        public string Status { get; set; } = "Active";
        
        public int CreatedBy { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual TaxType? TaxType { get; set; }
        
        public virtual User? Creator { get; set; }
        
        public virtual ICollection<ProductModifierGroupItem>? ModifierGroupItems { get; set; }
    }
}
