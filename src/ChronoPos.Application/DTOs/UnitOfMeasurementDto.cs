namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for Unit of Measurement display and transfer
/// </summary>
public class UnitOfMeasurementDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Abbreviation { get; set; }
    public string Type { get; set; } = string.Empty; // Base or Derived
    public string? CategoryTitle { get; set; }
    public long? BaseUomId { get; set; }
    public string? BaseUomName { get; set; }
    public decimal? ConversionFactor { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsActive { get; set; } = true;
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public string? UpdatedByName { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    public string? DeletedByName { get; set; }
    
    // Display property for UI binding
    public string DisplayName => !string.IsNullOrEmpty(Abbreviation) ? $"{Name} ({Abbreviation})" : Name;
    
    // Full display with type information
    public string FullDisplayName => $"{DisplayName} [{Type}]";
    
    // Display properties for DataGrid
    public string AbbreviationDisplay => Abbreviation ?? "-";
    public string ConversionFactorDisplay => ConversionFactor?.ToString("F4") ?? "-";
    public string CategoryDisplay => CategoryTitle ?? "-";
    public string BaseUomDisplay => BaseUomName ?? "-";
    public string StatusDisplay => Status;
    public string CreatedAtFormatted => CreatedAt.ToString("yyyy-MM-dd");
    
    // Override ToString for ComboBox display
    public override string ToString()
    {
        return DisplayName;
    }
}

/// <summary>
/// DTO for creating a new Unit of Measurement
/// </summary>
public class CreateUomDto
{
    public string Name { get; set; } = string.Empty;
    public string? Abbreviation { get; set; }
    public string Type { get; set; } = "Base"; // Base or Derived
    public string? CategoryTitle { get; set; }
    public long? BaseUomId { get; set; }
    public decimal? ConversionFactor { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating an existing Unit of Measurement
/// </summary>
public class UpdateUomDto
{
    public string Name { get; set; } = string.Empty;
    public string? Abbreviation { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? CategoryTitle { get; set; }
    public long? BaseUomId { get; set; }
    public decimal? ConversionFactor { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for paginated UOM results
/// </summary>
public class PagedUomResultDto
{
    public IEnumerable<UnitOfMeasurementDto> Items { get; set; } = new List<UnitOfMeasurementDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// DTO for UOM validation results
/// </summary>
public class UomValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public List<string> Warnings { get; set; } = new List<string>();
    
    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }
    
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
}
