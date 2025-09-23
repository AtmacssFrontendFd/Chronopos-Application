using ChronoPos.Application.DTOs;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for ProductUnit operations
/// </summary>
public interface IProductUnitService
{
    /// <summary>
    /// Creates a new product unit
    /// </summary>
    /// <param name="createDto">Product unit creation data</param>
    /// <returns>Created product unit</returns>
    Task<ProductUnitDto> CreateAsync(CreateProductUnitDto createDto);
    
    /// <summary>
    /// Updates an existing product unit
    /// </summary>
    /// <param name="updateDto">Product unit update data</param>
    /// <returns>Updated product unit</returns>
    Task<ProductUnitDto> UpdateAsync(UpdateProductUnitDto updateDto);
    
    /// <summary>
    /// Deletes a product unit by ID
    /// </summary>
    /// <param name="id">Product unit ID</param>
    /// <returns>Task</returns>
    Task DeleteAsync(int id);
    
    /// <summary>
    /// Gets a product unit by ID
    /// </summary>
    /// <param name="id">Product unit ID</param>
    /// <returns>Product unit or null if not found</returns>
    Task<ProductUnitDto?> GetByIdAsync(int id);
    
    /// <summary>
    /// Gets all product units for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of product units</returns>
    Task<IEnumerable<ProductUnitDto>> GetByProductIdAsync(int productId);
    
    /// <summary>
    /// Gets the base unit for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Base product unit or null if not found</returns>
    Task<ProductUnitDto?> GetBaseUnitByProductIdAsync(int productId);
    
    /// <summary>
    /// Creates multiple product units for a product (typically used when creating a new product)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="productUnits">List of product units to create</param>
    /// <returns>List of created product units</returns>
    Task<IEnumerable<ProductUnitDto>> CreateMultipleAsync(int productId, IEnumerable<CreateProductUnitDto> productUnits);
    
    /// <summary>
    /// Updates all product units for a product (replaces existing ones)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="productUnits">New list of product units</param>
    /// <returns>List of updated product units</returns>
    Task<IEnumerable<ProductUnitDto>> UpdateAllForProductAsync(int productId, IEnumerable<CreateProductUnitDto> productUnits);
    
    /// <summary>
    /// Deletes all product units for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Task</returns>
    Task DeleteByProductIdAsync(int productId);
    
    /// <summary>
    /// Updates the base unit designation for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="newBaseUnitId">New base unit ID</param>
    /// <returns>Task</returns>
    Task UpdateBaseUnitAsync(int productId, long newBaseUnitId);
    
    /// <summary>
    /// Validates a list of product units for business rules
    /// </summary>
    /// <param name="productUnits">Product units to validate</param>
    /// <returns>Validation result with any errors</returns>
    Task<ValidationResult> ValidateProductUnitsAsync(IEnumerable<CreateProductUnitDto> productUnits);
    
    /// <summary>
    /// Gets product unit summaries for a product (lightweight version for display)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of product unit summaries</returns>
    Task<IEnumerable<ProductUnitSummaryDto>> GetSummariesByProductIdAsync(int productId);
    
    /// <summary>
    /// Gets all product units in the system
    /// </summary>
    /// <returns>List of all product units</returns>
    Task<IEnumerable<ProductUnitDto>> GetAllAsync();
    
    /// <summary>
    /// Gets all product units with navigation properties (for UI display)
    /// </summary>
    /// <returns>List of all product units with complete information</returns>
    Task<IEnumerable<ProductUnit>> GetAllWithNavigationAsync();
}

/// <summary>
/// Validation result for product unit operations
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
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