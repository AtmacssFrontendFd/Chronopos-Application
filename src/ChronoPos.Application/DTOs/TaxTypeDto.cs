namespace ChronoPos.Application.DTOs;

public class TaxTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Value { get; set; }
    public bool IsPercentage { get; set; }
    public bool IncludedInPrice { get; set; }
    public bool AppliesToBuying { get; set; }
    public bool AppliesToSelling { get; set; }
    public int CalculationOrder { get; set; }
    public bool IsActive { get; set; }
}
