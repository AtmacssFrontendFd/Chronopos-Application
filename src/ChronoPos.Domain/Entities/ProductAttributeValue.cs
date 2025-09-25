using System;

namespace ChronoPos.Domain.Entities
{
    public class ProductAttributeValue
    {
        public int Id { get; set; }
        public int AttributeId { get; set; }
        public string Value { get; set; } = string.Empty;
        public string? ValueAr { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ProductAttribute? Attribute { get; set; }
    }
}