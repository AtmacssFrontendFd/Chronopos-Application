using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Unit of Measurement business logic
/// </summary>
public interface IUomService
{
    /// <summary>
    /// Gets all UOMs asynchronously
    /// </summary>
    Task<IEnumerable<UnitOfMeasurementDto>> GetAllUomsAsync();
    
    /// <summary>
    /// Gets all active UOMs asynchronously
    /// </summary>
    Task<IEnumerable<UnitOfMeasurementDto>> GetActiveUomsAsync();
    
    /// <summary>
    /// Gets a UOM by ID asynchronously
    /// </summary>
    /// <param name="id">UOM ID</param>
    Task<UnitOfMeasurementDto?> GetUomByIdAsync(long id);
    
    /// <summary>
    /// Gets UOMs by type (Base or Derived) asynchronously
    /// </summary>
    /// <param name="type">UOM type (Base or Derived)</param>
    Task<IEnumerable<UnitOfMeasurementDto>> GetUomsByTypeAsync(string type);
    
    /// <summary>
    /// Gets UOMs by category title asynchronously
    /// </summary>
    /// <param name="categoryTitle">Category title</param>
    Task<IEnumerable<UnitOfMeasurementDto>> GetUomsByCategoryAsync(string categoryTitle);
    
    /// <summary>
    /// Gets all derived UOMs for a base UOM asynchronously
    /// </summary>
    /// <param name="baseUomId">Base UOM ID</param>
    Task<IEnumerable<UnitOfMeasurementDto>> GetDerivedUomsAsync(long baseUomId);
    
    /// <summary>
    /// Gets a UOM by its abbreviation asynchronously
    /// </summary>
    /// <param name="abbreviation">UOM abbreviation</param>
    Task<UnitOfMeasurementDto?> GetByAbbreviationAsync(string abbreviation);
    
    /// <summary>
    /// Creates a new UOM asynchronously
    /// </summary>
    /// <param name="createDto">UOM creation data</param>
    /// <param name="createdBy">User ID creating the UOM</param>
    Task<UnitOfMeasurementDto> CreateUomAsync(CreateUomDto createDto, int createdBy);
    
    /// <summary>
    /// Updates an existing UOM asynchronously
    /// </summary>
    /// <param name="id">UOM ID</param>
    /// <param name="updateDto">UOM update data</param>
    /// <param name="updatedBy">User ID updating the UOM</param>
    Task<UnitOfMeasurementDto> UpdateUomAsync(long id, UpdateUomDto updateDto, int updatedBy);
    
    /// <summary>
    /// Soft deletes a UOM asynchronously
    /// </summary>
    /// <param name="id">UOM ID</param>
    /// <param name="deletedBy">User ID performing the deletion</param>
    Task<bool> DeleteUomAsync(long id, int deletedBy);
    
    /// <summary>
    /// Restores a soft-deleted UOM asynchronously
    /// </summary>
    /// <param name="id">UOM ID</param>
    /// <param name="restoredBy">User ID performing the restoration</param>
    Task<bool> RestoreUomAsync(long id, int restoredBy);
    
    /// <summary>
    /// Gets UOMs with pagination support asynchronously
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="searchTerm">Optional search term</param>
    /// <param name="includeInactive">Include inactive UOMs</param>
    Task<PagedUomResultDto> GetPagedUomsAsync(
        int pageNumber, 
        int pageSize, 
        string? searchTerm = null, 
        bool includeInactive = false);
    
    /// <summary>
    /// Validates UOM data before create/update operations
    /// </summary>
    /// <param name="name">UOM name</param>
    /// <param name="abbreviation">UOM abbreviation</param>
    /// <param name="type">UOM type</param>
    /// <param name="baseUomId">Base UOM ID (for derived UOMs)</param>
    /// <param name="conversionFactor">Conversion factor (for derived UOMs)</param>
    /// <param name="excludeId">ID to exclude from validation (for updates)</param>
    Task<UomValidationResult> ValidateUomAsync(
        string name, 
        string? abbreviation, 
        string type, 
        long? baseUomId, 
        decimal? conversionFactor, 
        long? excludeId = null);
    
    /// <summary>
    /// Converts quantity from one UOM to another
    /// </summary>
    /// <param name="quantity">Quantity to convert</param>
    /// <param name="fromUomId">Source UOM ID</param>
    /// <param name="toUomId">Target UOM ID</param>
    Task<decimal?> ConvertQuantityAsync(decimal quantity, long fromUomId, long toUomId);
    
    /// <summary>
    /// Checks if a UOM can be safely deleted (not referenced by other entities)
    /// </summary>
    /// <param name="id">UOM ID</param>
    Task<bool> CanDeleteUomAsync(long id);

    #region Convenience Methods for ViewModels
    
    /// <summary>
    /// Convenience method - same as GetAllUomsAsync()
    /// </summary>
    Task<IEnumerable<UnitOfMeasurementDto>> GetAllAsync();
    
    /// <summary>
    /// Convenience method - same as GetUomByIdAsync()
    /// </summary>
    Task<UnitOfMeasurementDto?> GetByIdAsync(long id);
    
    /// <summary>
    /// Convenience method - same as CreateUomAsync() with default user ID
    /// </summary>
    Task<UnitOfMeasurementDto> CreateAsync(CreateUomDto createDto);
    
    /// <summary>
    /// Convenience method - same as UpdateUomAsync() with default user ID
    /// </summary>
    Task<UnitOfMeasurementDto> UpdateAsync(long id, UpdateUomDto updateDto);
    
    /// <summary>
    /// Convenience method - same as DeleteUomAsync() with default user ID
    /// </summary>
    Task<bool> DeleteAsync(long id);
    
    #endregion
}