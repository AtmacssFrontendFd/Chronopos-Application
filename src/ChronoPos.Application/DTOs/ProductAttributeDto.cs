using System;
using System.Collections.Generic;

namespace ChronoPos.Application.DTOs
{
    public class ProductAttributeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameAr { get; set; }
        public bool IsRequired { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public List<ProductAttributeValueDto>? Values { get; set; }
        public int ValuesCount { get; set; }
    }
}