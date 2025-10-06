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

    // Display properties for UI binding
    public string Status => IsActive ? "Active" : "Inactive";
    public string ValueDisplay => IsPercentage ? $"{Value:F2}%" : $"{Value:C}";
    public string TypeDisplay => IsPercentage ? "Percentage" : "Fixed Amount";
    public string AppliesToDisplay
    {
        get
        {
            if (AppliesToBuying && AppliesToSelling)
                return "Buying & Selling";
            else if (AppliesToBuying)
                return "Buying Only";
            else if (AppliesToSelling)
                return "Selling Only";
            else
                return "None";
        }
    }
}
