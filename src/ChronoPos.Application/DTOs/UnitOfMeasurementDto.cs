namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for Unit of Measurement display and transfer
/// </summary>
public class UnitOfMeasurementDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public int? BaseUomId { get; set; }
    public string? BaseUomName { get; set; }
    public decimal? ConversionFactor { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Display property for UI binding
    public string DisplayName => $"{Name} ({Abbreviation})";
    
    // Override ToString for ComboBox display
    public override string ToString()
    {
        return DisplayName;
    }
}
