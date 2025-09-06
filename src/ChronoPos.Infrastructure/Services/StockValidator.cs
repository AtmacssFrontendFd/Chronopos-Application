using ChronoPos.Domain.Entities;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Service for validating stock control settings
/// </summary>
public class StockValidator
{
    public ValidationResult ValidateStockSettings(
        bool isStockTracked, 
        decimal initialStock, 
        decimal minimumStock, 
        decimal maximumStock, 
        decimal reorderLevel, 
        decimal reorderQuantity, 
        decimal averageCost,
        bool isUsingSerialNumbers)
    {
        var result = new ValidationResult();
        
        if (!isStockTracked)
            return result; // No validation needed if stock not tracked
        
        // Validate stock levels relationship
        if (maximumStock > 0 && minimumStock > maximumStock)
        {
            result.Errors.Add("Minimum stock cannot exceed maximum stock");
        }
        
        if (reorderLevel > 0 && minimumStock > 0 && reorderLevel < minimumStock)
        {
            result.Warnings.Add("Reorder level is below minimum stock level");
        }
        
        // Validate initial stock
        if (initialStock < 0)
        {
            result.Errors.Add("Initial stock cannot be negative");
        }
        
        // Validate costs
        if (initialStock > 0 && averageCost <= 0)
        {
            result.Warnings.Add("Average cost should be specified when setting initial stock");
        }
        
        // Validate reorder quantity
        if (reorderLevel > 0 && reorderQuantity <= 0)
        {
            result.Warnings.Add("Reorder quantity should be specified when reorder level is set");
        }
        
        return result;
    }
    
    public ValidationResult ValidateSerialNumbers(bool isUsingSerialNumbers, decimal initialStock)
    {
        var result = new ValidationResult();
        
        if (isUsingSerialNumbers && initialStock > 0)
        {
            result.Warnings.Add("Serial numbers must be entered individually. Initial stock will be set to 0.");
        }
        
        return result;
    }
}
