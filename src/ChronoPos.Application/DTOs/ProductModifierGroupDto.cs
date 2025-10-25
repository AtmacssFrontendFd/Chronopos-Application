using System;

namespace ChronoPos.Application.DTOs
{
    public class ProductModifierGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SelectionType { get; set; } = "Multiple";
        public bool Required { get; set; }
        public int MinSelections { get; set; }
        public int? MaxSelections { get; set; }
        public string Status { get; set; } = "Active";
        public int CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ItemCount { get; set; }
    }

    public class CreateProductModifierGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SelectionType { get; set; } = "Multiple";
        public bool Required { get; set; }
        public int MinSelections { get; set; }
        public int? MaxSelections { get; set; }
        public string Status { get; set; } = "Active";
        public int CreatedBy { get; set; }
    }

    public class UpdateProductModifierGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SelectionType { get; set; } = "Multiple";
        public bool Required { get; set; }
        public int MinSelections { get; set; }
        public int? MaxSelections { get; set; }
        public string Status { get; set; } = "Active";
    }
}
