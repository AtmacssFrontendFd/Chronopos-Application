using System;

namespace ChronoPos.Application.DTOs
{
    public class ProductModifierGroupItemDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string? GroupName { get; set; }
        public int ModifierId { get; set; }
        public string? ModifierName { get; set; }
        public decimal ModifierPrice { get; set; }
        public decimal PriceAdjustment { get; set; }
        public decimal FinalPrice { get; set; }
        public int SortOrder { get; set; }
        public bool DefaultSelection { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
    }

    public class CreateProductModifierGroupItemDto
    {
        public int GroupId { get; set; }
        public int ModifierId { get; set; }
        public decimal PriceAdjustment { get; set; }
        public int SortOrder { get; set; }
        public bool DefaultSelection { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class UpdateProductModifierGroupItemDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int ModifierId { get; set; }
        public decimal PriceAdjustment { get; set; }
        public int SortOrder { get; set; }
        public bool DefaultSelection { get; set; }
        public string Status { get; set; } = "Active";
    }
}
