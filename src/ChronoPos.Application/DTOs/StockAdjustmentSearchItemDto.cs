using ChronoPos.Domain.Enums;

namespace ChronoPos.Application.DTOs
{
    /// <summary>
    /// Unified DTO for stock adjustment search that can represent either a Product or ProductUnit
    /// </summary>
    public class StockAdjustmentSearchItemDto
    {
        /// <summary>
        /// Unique identifier for the item (ProductId for Product mode, ProductUnitId for ProductUnit mode)
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// The display name of the item
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Additional display information (e.g., unit details for ProductUnit)
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
        
        /// <summary>
        /// The mode this item represents
        /// </summary>
        public StockAdjustmentMode Mode { get; set; }
        
        /// <summary>
        /// Current stock quantity (for Product mode) or quantity in unit (for ProductUnit mode)
        /// </summary>
        public decimal CurrentQuantity { get; set; }
        
        /// <summary>
        /// The product ID (same as Id for Product mode, parent ProductId for ProductUnit mode)
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// For ProductUnit mode: the ProductUnit ID
        /// </summary>
        public int? ProductUnitId { get; set; }
        
        /// <summary>
        /// For ProductUnit mode: the Unit ID from the ProductUnit
        /// </summary>
        public long? UnitId { get; set; }
        
        /// <summary>
        /// For ProductUnit mode: the conversion factor from UnitOfMeasurement
        /// </summary>
        public decimal? ConversionFactor { get; set; }
        
        /// <summary>
        /// For ProductUnit mode: quantity in unit
        /// </summary>
        public int? QtyInUnit { get; set; }
        
        /// <summary>
        /// Product image path
        /// </summary>
        public string? ImagePath { get; set; }
        
        /// <summary>
        /// Unit name for display
        /// </summary>
        public string? UnitName { get; set; }
        
        /// <summary>
        /// Unit abbreviation for display
        /// </summary>
        public string? UnitAbbreviation { get; set; }
        
        /// <summary>
        /// Formatted display text for the search dropdown
        /// </summary>
        public string SearchDisplayText => Mode switch
        {
            StockAdjustmentMode.Product => $"{Name} (Product)",
            StockAdjustmentMode.ProductUnit => $"{Name} - {UnitName} ({QtyInUnit}) (Unit)",
            _ => Name
        };
        
        /// <summary>
        /// Override ToString to return the proper display text for ComboBox binding
        /// </summary>
        public override string ToString()
        {
            return SearchDisplayText;
        }
    }
}