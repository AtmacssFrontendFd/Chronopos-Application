using System;

namespace ChronoPos.Application.DTOs
{
    public class ProductModifierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public int? TaxTypeId { get; set; }
        public string? TaxTypeName { get; set; }
        public string Status { get; set; } = "Active";
        public int CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateProductModifierDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public int? TaxTypeId { get; set; }
        public string Status { get; set; } = "Active";
        public int CreatedBy { get; set; }
    }

    public class UpdateProductModifierDto
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
    }
}
