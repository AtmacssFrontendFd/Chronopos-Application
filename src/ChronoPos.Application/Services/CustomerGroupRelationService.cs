using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for CustomerGroupRelation operations
/// </summary>
public class CustomerGroupRelationService : ICustomerGroupRelationService
{
    private readonly ICustomerGroupRelationRepository _relationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerGroupRelationService(
        ICustomerGroupRelationRepository relationRepository, 
        IUnitOfWork unitOfWork)
    {
        _relationRepository = relationRepository ?? throw new ArgumentNullException(nameof(relationRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all customer group relations
    /// </summary>
    /// <returns>Collection of customer group relation DTOs</returns>
    public async Task<IEnumerable<CustomerGroupRelationDto>> GetAllAsync()
    {
        var relations = await _relationRepository.GetAllAsync();
        return relations.Select(MapToDto);
    }

    /// <summary>
    /// Gets all active customer group relations
    /// </summary>
    /// <returns>Collection of active customer group relation DTOs</returns>
    public async Task<IEnumerable<CustomerGroupRelationDto>> GetActiveRelationsAsync()
    {
        var relations = await _relationRepository.GetActiveRelationsAsync();
        return relations.Select(MapToDto);
    }

    /// <summary>
    /// Gets customer group relation by ID
    /// </summary>
    /// <param name="id">Relation ID</param>
    /// <returns>Customer group relation DTO if found</returns>
    public async Task<CustomerGroupRelationDto?> GetByIdAsync(int id)
    {
        var relation = await _relationRepository.GetByIdAsync(id);
        return relation != null ? MapToDto(relation) : null;
    }

    /// <summary>
    /// Gets all relations for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Collection of customer group relation DTOs</returns>
    public async Task<IEnumerable<CustomerGroupRelationDto>> GetByCustomerIdAsync(int customerId)
    {
        var relations = await _relationRepository.GetByCustomerIdAsync(customerId);
        return relations.Select(MapToDto);
    }

    /// <summary>
    /// Gets all relations for a specific customer group
    /// </summary>
    /// <param name="customerGroupId">Customer group ID</param>
    /// <returns>Collection of customer group relation DTOs</returns>
    public async Task<IEnumerable<CustomerGroupRelationDto>> GetByCustomerGroupIdAsync(int customerGroupId)
    {
        var relations = await _relationRepository.GetByCustomerGroupIdAsync(customerGroupId);
        return relations.Select(MapToDto);
    }

    /// <summary>
    /// Creates a new customer group relation
    /// </summary>
    /// <param name="createRelationDto">Relation data</param>
    /// <returns>Created customer group relation DTO</returns>
    public async Task<CustomerGroupRelationDto> CreateAsync(CreateCustomerGroupRelationDto createRelationDto)
    {
        // Check if relation already exists
        if (await _relationRepository.RelationExistsAsync(createRelationDto.CustomerId, createRelationDto.CustomerGroupId))
        {
            throw new InvalidOperationException($"Relation between customer {createRelationDto.CustomerId} and group {createRelationDto.CustomerGroupId} already exists");
        }

        var relation = new CustomerGroupRelation
        {
            CustomerId = createRelationDto.CustomerId,
            CustomerGroupId = createRelationDto.CustomerGroupId,
            Status = createRelationDto.Status ?? "Active",
            CreatedBy = createRelationDto.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _relationRepository.AddAsync(relation);
        await _unitOfWork.SaveChangesAsync();

        // Reload with navigation properties
        var createdRelation = await _relationRepository.GetByIdAsync(relation.Id);
        return MapToDto(createdRelation!);
    }

    /// <summary>
    /// Updates an existing customer group relation
    /// </summary>
    /// <param name="id">Relation ID</param>
    /// <param name="updateRelationDto">Updated relation data</param>
    /// <returns>Updated customer group relation DTO</returns>
    public async Task<CustomerGroupRelationDto> UpdateAsync(int id, UpdateCustomerGroupRelationDto updateRelationDto)
    {
        var relation = await _relationRepository.GetByIdAsync(id);
        if (relation == null)
        {
            throw new ArgumentException($"CustomerGroupRelation with ID {id} not found");
        }

        // Check if relation already exists (excluding current relation)
        if (updateRelationDto.CustomerId.HasValue && updateRelationDto.CustomerGroupId.HasValue)
        {
            if (await _relationRepository.RelationExistsAsync(
                updateRelationDto.CustomerId.Value, 
                updateRelationDto.CustomerGroupId.Value, 
                id))
            {
                throw new InvalidOperationException($"Relation between customer {updateRelationDto.CustomerId} and group {updateRelationDto.CustomerGroupId} already exists");
            }
        }

        if (updateRelationDto.CustomerId.HasValue)
            relation.CustomerId = updateRelationDto.CustomerId.Value;
        
        if (updateRelationDto.CustomerGroupId.HasValue)
            relation.CustomerGroupId = updateRelationDto.CustomerGroupId.Value;
        
        if (updateRelationDto.Status != null)
            relation.Status = updateRelationDto.Status;
        
        relation.UpdatedBy = updateRelationDto.UpdatedBy;
        relation.UpdatedAt = DateTime.UtcNow;

        await _relationRepository.UpdateAsync(relation);
        await _unitOfWork.SaveChangesAsync();

        // Reload with navigation properties
        var updatedRelation = await _relationRepository.GetByIdAsync(id);
        return MapToDto(updatedRelation!);
    }

    /// <summary>
    /// Deletes a customer group relation (soft delete)
    /// </summary>
    /// <param name="id">Relation ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var relation = await _relationRepository.GetByIdAsync(id);
        if (relation == null)
        {
            return false;
        }

        // Soft delete
        relation.DeletedAt = DateTime.UtcNow;
        relation.UpdatedAt = DateTime.UtcNow;

        await _relationRepository.UpdateAsync(relation);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Checks if a relation exists between customer and customer group
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="customerGroupId">Customer group ID</param>
    /// <param name="excludeId">Relation ID to exclude from check</param>
    /// <returns>True if relation exists</returns>
    public async Task<bool> RelationExistsAsync(int customerId, int customerGroupId, int? excludeId = null)
    {
        return await _relationRepository.RelationExistsAsync(customerId, customerGroupId, excludeId);
    }

    /// <summary>
    /// Gets all relations with customer and group details
    /// </summary>
    /// <returns>Collection of relations with details</returns>
    public async Task<IEnumerable<CustomerGroupRelationDto>> GetAllWithDetailsAsync()
    {
        var relations = await _relationRepository.GetAllWithDetailsAsync();
        return relations.Select(MapToDto);
    }

    /// <summary>
    /// Maps CustomerGroupRelation entity to CustomerGroupRelationDto
    /// </summary>
    /// <param name="relation">CustomerGroupRelation entity</param>
    /// <returns>CustomerGroupRelation DTO</returns>
    private static CustomerGroupRelationDto MapToDto(CustomerGroupRelation relation)
    {
        return new CustomerGroupRelationDto
        {
            Id = relation.Id,
            CustomerId = relation.CustomerId,
            CustomerGroupId = relation.CustomerGroupId,
            Status = relation.Status,
            CreatedBy = relation.CreatedBy,
            CreatedAt = relation.CreatedAt,
            UpdatedBy = relation.UpdatedBy,
            UpdatedAt = relation.UpdatedAt,
            DeletedAt = relation.DeletedAt,
            DeletedBy = relation.DeletedBy,
            CustomerName = relation.Customer?.DisplayName,
            CustomerGroupName = relation.CustomerGroup?.Name
        };
    }
}
