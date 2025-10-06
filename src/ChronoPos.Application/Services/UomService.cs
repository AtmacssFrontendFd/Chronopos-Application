using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Unit of Measurement business logic
/// </summary>
public class UomService : IUomService
{
    private readonly IUomRepository _uomRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggingService _loggingService;

    public UomService(
        IUomRepository uomRepository,
        IUnitOfWork unitOfWork,
        ILoggingService loggingService)
    {
        _uomRepository = uomRepository;
        _unitOfWork = unitOfWork;
        _loggingService = loggingService;
    }

    /// <summary>
    /// Gets all UOMs asynchronously
    /// </summary>
    public async Task<IEnumerable<UnitOfMeasurementDto>> GetAllUomsAsync()
    {
        try
        {
            var uoms = await _uomRepository.GetAllAsync();
            return uoms.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error getting all UOMs: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets all active UOMs asynchronously
    /// </summary>
    public async Task<IEnumerable<UnitOfMeasurementDto>> GetActiveUomsAsync()
    {
        try
        {
            var uoms = await _uomRepository.GetActiveUomsAsync();
            return uoms.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error getting active UOMs: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets a UOM by ID asynchronously
    /// </summary>
    /// <param name="id">UOM ID</param>
    public async Task<UnitOfMeasurementDto?> GetUomByIdAsync(long id)
    {
        try
        {
            var uom = await _uomRepository.GetByIdAsync((int)id);
            return uom != null ? MapToDto(uom) : null;
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error getting UOM by ID {id}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets UOMs by type (Base or Derived) asynchronously
    /// </summary>
    /// <param name="type">UOM type (Base or Derived)</param>
    public async Task<IEnumerable<UnitOfMeasurementDto>> GetUomsByTypeAsync(string type)
    {
        try
        {
            var uoms = await _uomRepository.GetUomsByTypeAsync(type);
            return uoms.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error getting UOMs by type {type}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets UOMs by category title asynchronously
    /// </summary>
    /// <param name="categoryTitle">Category title</param>
    public async Task<IEnumerable<UnitOfMeasurementDto>> GetUomsByCategoryAsync(string categoryTitle)
    {
        try
        {
            var uoms = await _uomRepository.GetUomsByCategoryAsync(categoryTitle);
            return uoms.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error getting UOMs by category {categoryTitle}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets all derived UOMs for a base UOM asynchronously
    /// </summary>
    /// <param name="baseUomId">Base UOM ID</param>
    public async Task<IEnumerable<UnitOfMeasurementDto>> GetDerivedUomsAsync(long baseUomId)
    {
        try
        {
            var uoms = await _uomRepository.GetDerivedUomsAsync(baseUomId);
            return uoms.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error getting derived UOMs for base UOM {baseUomId}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets a UOM by its abbreviation asynchronously
    /// </summary>
    /// <param name="abbreviation">UOM abbreviation</param>
    public async Task<UnitOfMeasurementDto?> GetByAbbreviationAsync(string abbreviation)
    {
        try
        {
            var uom = await _uomRepository.GetByAbbreviationAsync(abbreviation);
            return uom != null ? MapToDto(uom) : null;
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error getting UOM by abbreviation {abbreviation}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Creates a new UOM asynchronously
    /// </summary>
    /// <param name="createDto">UOM creation data</param>
    /// <param name="createdBy">User ID creating the UOM</param>
    public async Task<UnitOfMeasurementDto> CreateUomAsync(CreateUomDto createDto, int createdBy)
    {
        try
        {
            // Validate input
            var validationResult = await ValidateUomAsync(
                createDto.Name, 
                createDto.Abbreviation, 
                createDto.Type, 
                createDto.BaseUomId, 
                createDto.ConversionFactor);

            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            // Create entity
            var uom = new UnitOfMeasurement
            {
                Name = createDto.Name,
                Abbreviation = createDto.Abbreviation,
                Type = createDto.Type,
                CategoryTitle = createDto.CategoryTitle,
                BaseUomId = createDto.BaseUomId,
                ConversionFactor = createDto.ConversionFactor,
                Status = createDto.Status,
                IsActive = createDto.IsActive,
                CreatedBy = createdBy
            };

            // Save to database
            var createdUom = await _uomRepository.AddAsync(uom);
            await _unitOfWork.SaveChangesAsync();

            _loggingService.Log($"UOM created: {createdUom.Name} (ID: {createdUom.Id}) by user {createdBy}");

            // Return DTO
            return MapToDto(createdUom);
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error creating UOM: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing UOM asynchronously
    /// </summary>
    /// <param name="id">UOM ID</param>
    /// <param name="updateDto">UOM update data</param>
    /// <param name="updatedBy">User ID updating the UOM</param>
    public async Task<UnitOfMeasurementDto> UpdateUomAsync(long id, UpdateUomDto updateDto, int updatedBy)
    {
        try
        {
            // Get existing UOM
            var existingUom = await _uomRepository.GetByIdAsync((int)id);
            if (existingUom == null)
            {
                throw new ArgumentException($"UOM with ID {id} not found");
            }

            // Validate input
            var validationResult = await ValidateUomAsync(
                updateDto.Name, 
                updateDto.Abbreviation, 
                updateDto.Type, 
                updateDto.BaseUomId, 
                updateDto.ConversionFactor, 
                id);

            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            // Update entity
            existingUom.Name = updateDto.Name;
            existingUom.Abbreviation = updateDto.Abbreviation;
            existingUom.Type = updateDto.Type;
            existingUom.CategoryTitle = updateDto.CategoryTitle;
            existingUom.BaseUomId = updateDto.BaseUomId;
            existingUom.ConversionFactor = updateDto.ConversionFactor;
            existingUom.Status = updateDto.Status;
            existingUom.IsActive = updateDto.IsActive;
            existingUom.UpdatedBy = updatedBy;

            // Save to database
            await _uomRepository.UpdateAsync(existingUom);
            await _unitOfWork.SaveChangesAsync();

            _loggingService.Log($"UOM updated: {existingUom.Name} (ID: {existingUom.Id}) by user {updatedBy}");

            // Return DTO
            return MapToDto(existingUom);
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error updating UOM {id}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Soft deletes a UOM asynchronously
    /// </summary>
    /// <param name="id">UOM ID</param>
    /// <param name="deletedBy">User ID performing the deletion</param>
    public async Task<bool> DeleteUomAsync(long id, int deletedBy)
    {
        try
        {
            // Check if UOM can be deleted
            var canDelete = await CanDeleteUomAsync(id);
            if (!canDelete)
            {
                throw new InvalidOperationException("UOM cannot be deleted as it is referenced by other entities");
            }

            // Soft delete
            await _uomRepository.SoftDeleteAsync(id, deletedBy);
            await _unitOfWork.SaveChangesAsync();

            _loggingService.Log($"UOM soft deleted: ID {id} by user {deletedBy}");

            return true;
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error deleting UOM {id}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Restores a soft-deleted UOM asynchronously
    /// </summary>
    /// <param name="id">UOM ID</param>
    /// <param name="restoredBy">User ID performing the restoration</param>
    public async Task<bool> RestoreUomAsync(long id, int restoredBy)
    {
        try
        {
            await _uomRepository.RestoreAsync(id, restoredBy);
            await _unitOfWork.SaveChangesAsync();

            _loggingService.Log($"UOM restored: ID {id} by user {restoredBy}");

            return true;
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error restoring UOM {id}: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Gets UOMs with pagination support asynchronously
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="searchTerm">Optional search term</param>
    /// <param name="includeInactive">Include inactive UOMs</param>
    public async Task<PagedUomResultDto> GetPagedUomsAsync(
        int pageNumber, 
        int pageSize, 
        string? searchTerm = null, 
        bool includeInactive = false)
    {
        try
        {
            var (items, totalCount) = await _uomRepository.GetPagedAsync(
                pageNumber, 
                pageSize, 
                searchTerm, 
                includeInactive);

            return new PagedUomResultDto
            {
                Items = items.Select(MapToDto),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error getting paged UOMs: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// Validates UOM data before create/update operations
    /// </summary>
    /// <param name="name">UOM name</param>
    /// <param name="abbreviation">UOM abbreviation</param>
    /// <param name="type">UOM type</param>
    /// <param name="baseUomId">Base UOM ID (for derived UOMs)</param>
    /// <param name="conversionFactor">Conversion factor (for derived UOMs)</param>
    /// <param name="excludeId">ID to exclude from validation (for updates)</param>
    public async Task<UomValidationResult> ValidateUomAsync(
        string name, 
        string? abbreviation, 
        string type, 
        long? baseUomId, 
        decimal? conversionFactor, 
        long? excludeId = null)
    {
        var result = new UomValidationResult { IsValid = true };

        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(name))
            {
                result.AddError("Name is required");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                result.AddError("Type is required");
            }
            else if (type != "Base" && type != "Derived")
            {
                result.AddError("Type must be either 'Base' or 'Derived'");
            }

            // Validate derived UOM requirements
            if (type == "Derived")
            {
                if (!baseUomId.HasValue)
                {
                    result.AddError("Base UOM is required for derived UOMs");
                }

                if (!conversionFactor.HasValue || conversionFactor <= 0)
                {
                    result.AddError("Conversion factor must be greater than 0 for derived UOMs");
                }
            }

            // Check for duplicate name
            if (!string.IsNullOrWhiteSpace(name))
            {
                var nameExists = await _uomRepository.ExistsByNameAsync(name, excludeId);
                if (nameExists)
                {
                    result.AddError($"UOM with name '{name}' already exists");
                }
            }

            // Check for duplicate abbreviation
            if (!string.IsNullOrWhiteSpace(abbreviation))
            {
                var abbreviationExists = await _uomRepository.ExistsByAbbreviationAsync(abbreviation, excludeId);
                if (abbreviationExists)
                {
                    result.AddError($"UOM with abbreviation '{abbreviation}' already exists");
                }
            }

            // Validate base UOM exists (for derived UOMs)
            if (baseUomId.HasValue)
            {
                var baseUom = await _uomRepository.GetByIdAsync(baseUomId.Value);
                if (baseUom == null)
                {
                    result.AddError($"Base UOM with ID {baseUomId} does not exist");
                }
                else if (baseUom.Type != "Base")
                {
                    result.AddError("Base UOM must be of type 'Base'");
                }
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error during UOM validation: {ex.Message}", ex);
            result.AddError("An error occurred during validation");
        }

        return result;
    }

    /// <summary>
    /// Converts quantity from one UOM to another
    /// </summary>
    /// <param name="quantity">Quantity to convert</param>
    /// <param name="fromUomId">Source UOM ID</param>
    /// <param name="toUomId">Target UOM ID</param>
    public async Task<decimal?> ConvertQuantityAsync(decimal quantity, long fromUomId, long toUomId)
    {
        try
        {
            if (fromUomId == toUomId)
            {
                return quantity;
            }

            var fromUom = await _uomRepository.GetByIdAsync((int)fromUomId);
            var toUom = await _uomRepository.GetByIdAsync((int)toUomId);

            if (fromUom == null || toUom == null)
            {
                return null;
            }

            // Both must have the same base UOM or one must be the base of the other
            if (fromUom.Type == "Base" && toUom.BaseUomId == fromUom.Id)
            {
                // Converting from base to derived
                return toUom.ConversionFactor.HasValue ? quantity * toUom.ConversionFactor.Value : null;
            }
            else if (toUom.Type == "Base" && fromUom.BaseUomId == toUom.Id)
            {
                // Converting from derived to base
                return fromUom.ConversionFactor.HasValue ? quantity / fromUom.ConversionFactor.Value : null;
            }
            else if (fromUom.BaseUomId == toUom.BaseUomId && fromUom.BaseUomId.HasValue)
            {
                // Converting between derived UOMs with the same base
                if (fromUom.ConversionFactor.HasValue && toUom.ConversionFactor.HasValue)
                {
                    // Convert to base first, then to target
                    var baseQuantity = quantity / fromUom.ConversionFactor.Value;
                    return baseQuantity * toUom.ConversionFactor.Value;
                }
            }

            // No conversion possible
            return null;
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error converting quantity from UOM {fromUomId} to {toUomId}: {ex.Message}", ex);
            return null;
        }
    }

    /// <summary>
    /// Checks if a UOM can be safely deleted (not referenced by other entities)
    /// </summary>
    /// <param name="id">UOM ID</param>
    public async Task<bool> CanDeleteUomAsync(long id)
    {
        try
        {
            // Check if UOM is used as base UOM for other UOMs
            var derivedUoms = await _uomRepository.GetDerivedUomsAsync(id);
            if (derivedUoms.Any())
            {
                return false;
            }

            // TODO: Add checks for other entities that might reference UOM
            // For example: Products, Stock movements, etc.
            // This would require additional repository methods or database queries

            return true;
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error checking if UOM {id} can be deleted: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// Maps UnitOfMeasurement entity to DTO
    /// </summary>
    /// <param name="uom">UnitOfMeasurement entity</param>
    /// <returns>UnitOfMeasurementDto</returns>
    private static UnitOfMeasurementDto MapToDto(UnitOfMeasurement uom)
    {
        return new UnitOfMeasurementDto
        {
            Id = uom.Id,
            Name = uom.Name,
            Abbreviation = uom.Abbreviation,
            Type = uom.Type,
            CategoryTitle = uom.CategoryTitle,
            BaseUomId = uom.BaseUomId,
            BaseUomName = uom.BaseUom?.Name,
            ConversionFactor = uom.ConversionFactor,
            Status = uom.Status,
            IsActive = uom.IsActive,
            CreatedBy = uom.CreatedBy,
            CreatedByName = uom.Creator?.FullName,
            CreatedAt = uom.CreatedAt,
            UpdatedBy = uom.UpdatedBy,
            UpdatedByName = uom.Updater?.FullName,
            UpdatedAt = uom.UpdatedAt,
            DeletedAt = uom.DeletedAt,
            DeletedBy = uom.DeletedBy,
            DeletedByName = uom.Deleter?.FullName
        };
    }

    #region Convenience Methods for ViewModel Compatibility

    public async Task<IEnumerable<UnitOfMeasurementDto>> GetAllAsync()
    {
        return await GetAllUomsAsync();
    }

    public async Task<UnitOfMeasurementDto?> GetByIdAsync(long id)
    {
        return await GetUomByIdAsync(id);
    }

    public async Task<UnitOfMeasurementDto> CreateAsync(CreateUomDto createDto)
    {
        // Use default user ID (1) for convenience method
        const int defaultUserId = 1;
        return await CreateUomAsync(createDto, defaultUserId);
    }

    public async Task<UnitOfMeasurementDto> UpdateAsync(long id, UpdateUomDto updateDto)
    {
        // Use default user ID (1) for convenience method
        const int defaultUserId = 1;
        return await UpdateUomAsync(id, updateDto, defaultUserId);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        // Use default user ID (1) for convenience method
        const int defaultUserId = 1;
        return await DeleteUomAsync(id, defaultUserId);
    }

    #endregion
}