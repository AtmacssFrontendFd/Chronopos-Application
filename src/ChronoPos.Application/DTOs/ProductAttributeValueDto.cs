using System;

namespace ChronoPos.Application.DTOs
{
    public class ProductAttributeValueDto
    {
        public int Id { get; set; }
        public int AttributeId { get; set; }
        public string Value { get; set; } = string.Empty;
        public string? ValueAr { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Parent Attribute Information (for display purposes)
        public string AttributeName { get; set; } = string.Empty;
        public string? AttributeNameAr { get; set; }
        public string AttributeType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
    }
}