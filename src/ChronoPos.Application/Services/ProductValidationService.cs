using ChronoPos.Domain.Entities;
using ChronoPos.Application.Interfaces;
using System.Text.RegularExpressions;

namespace ChronoPos.Application.Services;

/// <summary>
/// Business validation rules for products
/// </summary>
public class ProductValidationService
{
    private readonly IProductService _productService;

    public ProductValidationService(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Validates product code
    /// </summary>
    public ValidationResult ValidateCode(string code, int? excludeProductId = null)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(code))
        {
            result.Errors.Add("Product code is required");
            return result;
        }

        if (code.Length > 50)
        {
            result.Errors.Add("Product code cannot exceed 50 characters");
        }

        if (code.Length < 3)
        {
            result.Warnings.Add("Product code should be at least 3 characters");
        }

        // Check for valid characters (alphanumeric, hyphens, underscores)
        if (!Regex.IsMatch(code, @"^[a-zA-Z0-9\-_]+$"))
        {
            result.Errors.Add("Product code can only contain letters, numbers, hyphens, and underscores");
        }

        return result;
    }

    /// <summary>
    /// Validates product name
    /// </summary>
    public ValidationResult ValidateName(string name)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(name))
        {
            result.Errors.Add("Product name is required");
            return result;
        }

        if (name.Length > 100)
        {
            result.Errors.Add("Product name cannot exceed 100 characters");
        }

        if (name.Length < 2)
        {
            result.Warnings.Add("Product name should be at least 2 characters");
        }

        return result;
    }

    /// <summary>
    /// Validates price
    /// </summary>
    public ValidationResult ValidatePrice(decimal price)
    {
        var result = new ValidationResult();

        if (price < 0)
        {
            result.Errors.Add("Price cannot be negative");
        }
        else if (price == 0)
        {
            result.Warnings.Add("Price is zero - confirm this is correct");
        }

        return result;
    }

    /// <summary>
    /// Validates cost price
    /// </summary>
    public ValidationResult ValidateCost(decimal cost, decimal price)
    {
        var result = new ValidationResult();

        if (cost < 0)
        {
            result.Errors.Add("Cost cannot be negative");
        }

        if (cost > 0 && price > 0 && price < cost)
        {
            result.Warnings.Add("Warning: Selling price is lower than cost price");
        }

        return result;
    }

    /// <summary>
    /// Validates barcode format
    /// </summary>
    public ValidationResult ValidateBarcode(string barcode)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(barcode))
        {
            result.Errors.Add("Barcode cannot be empty");
            return result;
        }

        // Trim and validate format
        barcode = barcode.Trim();

        // Check length (typical barcode lengths)
        if (barcode.Length < 4 || barcode.Length > 50)
        {
            result.Warnings.Add("Barcode length should be between 4-50 characters");
        }

        // Check for valid characters (alphanumeric)
        if (!Regex.IsMatch(barcode, @"^[a-zA-Z0-9\-\s]+$"))
        {
            result.Errors.Add("Barcode contains invalid characters");
        }

        return result;
    }

    /// <summary>
    /// Validates list of barcodes
    /// </summary>
    public ValidationResult ValidateBarcodes(List<string> barcodes)
    {
        var result = new ValidationResult();
        var uniqueBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var barcode in barcodes.Where(b => !string.IsNullOrWhiteSpace(b)))
        {
            // Check for duplicates within the list
            if (!uniqueBarcodes.Add(barcode.Trim()))
            {
                result.Errors.Add($"Duplicate barcode in list: '{barcode}'");
                continue;
            }

            // Validate individual barcode
            var barcodeResult = ValidateBarcode(barcode);
            result.Errors.AddRange(barcodeResult.Errors);
            result.Warnings.AddRange(barcodeResult.Warnings);
        }

        return result;
    }

    /// <summary>
    /// Validates discount settings
    /// </summary>
    public ValidationResult ValidateDiscount(decimal maxDiscount, bool isDiscountAllowed)
    {
        var result = new ValidationResult();

        if (isDiscountAllowed)
        {
            if (maxDiscount < 0 || maxDiscount > 100)
            {
                result.Errors.Add("Max discount must be between 0 and 100");
            }
        }

        return result;
    }

    /// <summary>
    /// Validates age restriction
    /// </summary>
    public ValidationResult ValidateAgeRestriction(int? ageRestriction)
    {
        var result = new ValidationResult();

        if (ageRestriction.HasValue)
        {
            if (ageRestriction < 0 || ageRestriction > 150)
            {
                result.Errors.Add("Age restriction must be between 0 and 150");
            }
        }

        return result;
    }

    /// <summary>
    /// Validates entire product
    /// </summary>
    public ValidationResult ValidateProduct(Product product)
    {
        var result = new ValidationResult();

        // Validate basic fields
        var codeResult = ValidateCode(product.Code, product.Id);
        var nameResult = ValidateName(product.Name);
        var priceResult = ValidatePrice(product.Price);
        var costResult = ValidateCost(product.Cost, product.Price);
        var discountResult = ValidateDiscount(product.MaxDiscount, product.IsDiscountAllowed);
        var ageResult = ValidateAgeRestriction(product.AgeRestriction);

        // Combine results
        result.Errors.AddRange(codeResult.Errors);
        result.Errors.AddRange(nameResult.Errors);
        result.Errors.AddRange(priceResult.Errors);
        result.Errors.AddRange(costResult.Errors);
        result.Errors.AddRange(discountResult.Errors);
        result.Errors.AddRange(ageResult.Errors);

        result.Warnings.AddRange(codeResult.Warnings);
        result.Warnings.AddRange(nameResult.Warnings);
        result.Warnings.AddRange(priceResult.Warnings);
        result.Warnings.AddRange(costResult.Warnings);
        result.Warnings.AddRange(discountResult.Warnings);
        result.Warnings.AddRange(ageResult.Warnings);

        return result;
    }
}
