using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for ProductUnit entity
/// </summary>
public class ProductUnitDto : INotifyPropertyChanged
{
    private decimal _costOfUnit;
    private decimal _priceOfUnit;
    private int _qtyInUnit = 1;
    private long _unitId;

    public int Id { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public long UnitId 
    { 
        get => _unitId;
        set
        {
            _unitId = value;
            OnPropertyChanged();
        }
    }

    [StringLength(100)]
    public string Sku { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity in unit must be at least 1")]
    public int QtyInUnit 
    { 
        get => _qtyInUnit;
        set
        {
            _qtyInUnit = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Cost of unit cannot be negative")]
    public decimal CostOfUnit 
    { 
        get => _costOfUnit;
        set
        {
            _costOfUnit = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CostDisplay));
            OnPropertyChanged(nameof(Markup));
            OnPropertyChanged(nameof(MarkupDisplay));
            OnPropertyChanged(nameof(HasValidPricing));
            OnPropertyChanged(nameof(IsMarkupValid));
        }
    }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price of unit cannot be negative")]
    public decimal PriceOfUnit 
    { 
        get => _priceOfUnit;
        set
        {
            _priceOfUnit = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PriceDisplay));
            OnPropertyChanged(nameof(Markup));
            OnPropertyChanged(nameof(MarkupDisplay));
            OnPropertyChanged(nameof(HasValidPricing));
            OnPropertyChanged(nameof(IsMarkupValid));
        }
    }
    
    public int? SellingPriceId { get; set; }
    
    [StringLength(50)]
    public string PriceType { get; set; } = "Retail";
    
    public bool DiscountAllowed { get; set; } = false;
    
    public bool IsBase { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Display Properties
    public string? UnitName { get; set; }
    public string? UnitAbbreviation { get; set; }
    public string? ProductName { get; set; }
    
    // Computed Properties
    public decimal Markup => CostOfUnit > 0 ? ((PriceOfUnit - CostOfUnit) / CostOfUnit) * 100 : 0;
    public string DisplayName => $"{UnitName} ({QtyInUnit})";
    public string PriceDisplay => $"{PriceOfUnit:C}";
    public string CostDisplay => $"{CostOfUnit:C}";
    public string MarkupDisplay => $"{Markup:F1}%";
    public bool HasValidPricing => PriceOfUnit > 0 && CostOfUnit >= 0;
    public bool IsMarkupValid => Markup >= 0;
    
    // For UI Binding
    public override string ToString()
    {
        return DisplayName;
    }

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// DTO for creating a new ProductUnit
/// </summary>
public class CreateProductUnitDto
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public long UnitId { get; set; }
    
    [StringLength(100)]
    public string Sku { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity in unit must be at least 1")]
    public int QtyInUnit { get; set; } = 1;
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Cost of unit cannot be negative")]
    public decimal CostOfUnit { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price of unit cannot be negative")]
    public decimal PriceOfUnit { get; set; }
    
    public int? SellingPriceId { get; set; }
    
    [StringLength(50)]
    public string PriceType { get; set; } = "Retail";
    
    public bool DiscountAllowed { get; set; } = false;
    
    public bool IsBase { get; set; } = false;
}

/// <summary>
/// DTO for updating an existing ProductUnit
/// </summary>
public class UpdateProductUnitDto
{
    [Required]
    public int Id { get; set; }
    
    [StringLength(100)]
    public string Sku { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity in unit must be at least 1")]
    public int QtyInUnit { get; set; } = 1;
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Cost of unit cannot be negative")]
    public decimal CostOfUnit { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price of unit cannot be negative")]
    public decimal PriceOfUnit { get; set; }
    
    public int? SellingPriceId { get; set; }
    
    [StringLength(50)]
    public string PriceType { get; set; } = "Retail";
    
    public bool DiscountAllowed { get; set; } = false;
    
    public bool IsBase { get; set; } = false;
}

/// <summary>
/// DTO for ProductUnit summary information
/// </summary>
public class ProductUnitSummaryDto
{
    public int Id { get; set; }
    public long UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string UnitAbbreviation { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int QtyInUnit { get; set; }
    public decimal PriceOfUnit { get; set; }
    public decimal CostOfUnit { get; set; }
    public bool IsBase { get; set; }
    public bool DiscountAllowed { get; set; }
    public string PriceType { get; set; } = "Retail";
    
    // Display Properties
    public string DisplayName => $"{UnitName} ({QtyInUnit})";
    public string PriceDisplay => $"{PriceOfUnit:C}";
    public decimal Markup => CostOfUnit > 0 ? ((PriceOfUnit - CostOfUnit) / CostOfUnit) * 100 : 0;
}