using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for RestaurantTable operations
/// </summary>
public class RestaurantTableService : IRestaurantTableService
{
    private readonly IRestaurantTableRepository _restaurantTableRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RestaurantTableService(IRestaurantTableRepository restaurantTableRepository, IUnitOfWork unitOfWork)
    {
        _restaurantTableRepository = restaurantTableRepository ?? throw new ArgumentNullException(nameof(restaurantTableRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all restaurant tables
    /// </summary>
    /// <returns>Collection of restaurant table DTOs</returns>
    public async Task<IEnumerable<RestaurantTableDto>> GetAllAsync()
    {
        var tables = await _restaurantTableRepository.GetAllAsync();
        return tables.Select(MapToDto);
    }

    /// <summary>
    /// Gets all available tables
    /// </summary>
    /// <returns>Collection of available table DTOs</returns>
    public async Task<IEnumerable<RestaurantTableDto>> GetAvailableTablesAsync()
    {
        var tables = await _restaurantTableRepository.GetAvailableTablesAsync();
        return tables.Select(MapToDto);
    }

    /// <summary>
    /// Gets table by ID
    /// </summary>
    /// <param name="id">Table ID</param>
    /// <returns>Restaurant table DTO if found</returns>
    public async Task<RestaurantTableDto?> GetByIdAsync(int id)
    {
        var table = await _restaurantTableRepository.GetByIdAsync(id);
        return table != null ? MapToDto(table) : null;
    }

    /// <summary>
    /// Gets table by table number
    /// </summary>
    /// <param name="tableNumber">Table number</param>
    /// <returns>Restaurant table DTO if found</returns>
    public async Task<RestaurantTableDto?> GetByTableNumberAsync(string tableNumber)
    {
        var table = await _restaurantTableRepository.GetByTableNumberAsync(tableNumber);
        return table != null ? MapToDto(table) : null;
    }

    /// <summary>
    /// Gets tables by status
    /// </summary>
    /// <param name="status">Table status</param>
    /// <returns>Collection of table DTOs with specified status</returns>
    public async Task<IEnumerable<RestaurantTableDto>> GetTablesByStatusAsync(string status)
    {
        var tables = await _restaurantTableRepository.GetTablesByStatusAsync(status);
        return tables.Select(MapToDto);
    }

    /// <summary>
    /// Gets tables by location
    /// </summary>
    /// <param name="location">Location name</param>
    /// <returns>Collection of table DTOs in specified location</returns>
    public async Task<IEnumerable<RestaurantTableDto>> GetTablesByLocationAsync(string location)
    {
        var tables = await _restaurantTableRepository.GetTablesByLocationAsync(location);
        return tables.Select(MapToDto);
    }

    /// <summary>
    /// Gets tables with minimum capacity
    /// </summary>
    /// <param name="capacity">Minimum capacity required</param>
    /// <returns>Collection of table DTOs with at least the specified capacity</returns>
    public async Task<IEnumerable<RestaurantTableDto>> GetTablesByMinCapacityAsync(int capacity)
    {
        var tables = await _restaurantTableRepository.GetTablesByMinCapacityAsync(capacity);
        return tables.Select(MapToDto);
    }

    /// <summary>
    /// Creates a new restaurant table
    /// </summary>
    /// <param name="createTableDto">Table data</param>
    /// <returns>Created table DTO</returns>
    public async Task<RestaurantTableDto> CreateAsync(CreateRestaurantTableDto createTableDto)
    {
        // Check if table number already exists
        if (await _restaurantTableRepository.TableNumberExistsAsync(createTableDto.TableNumber))
        {
            throw new InvalidOperationException($"Table with number '{createTableDto.TableNumber}' already exists");
        }

        // Validate status
        var validStatuses = new[] { "available", "reserved", "occupied", "cleaning" };
        if (!validStatuses.Contains(createTableDto.Status.ToLower()))
        {
            throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");
        }

        var table = new RestaurantTable
        {
            TableNumber = createTableDto.TableNumber.Trim(),
            Capacity = createTableDto.Capacity,
            Status = createTableDto.Status.ToLower(),
            Location = createTableDto.Location?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _restaurantTableRepository.AddAsync(table);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(table);
    }

    /// <summary>
    /// Updates an existing restaurant table
    /// </summary>
    /// <param name="id">Table ID</param>
    /// <param name="updateTableDto">Updated table data</param>
    /// <returns>Updated table DTO</returns>
    public async Task<RestaurantTableDto> UpdateAsync(int id, UpdateRestaurantTableDto updateTableDto)
    {
        var table = await _restaurantTableRepository.GetByIdAsync(id);
        if (table == null)
        {
            throw new ArgumentException($"Table with ID {id} not found");
        }

        // Check if table number already exists (excluding current table)
        if (await _restaurantTableRepository.TableNumberExistsAsync(updateTableDto.TableNumber, id))
        {
            throw new InvalidOperationException($"Table with number '{updateTableDto.TableNumber}' already exists");
        }

        // Validate status
        var validStatuses = new[] { "available", "reserved", "occupied", "cleaning" };
        if (!validStatuses.Contains(updateTableDto.Status.ToLower()))
        {
            throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");
        }

        table.TableNumber = updateTableDto.TableNumber.Trim();
        table.Capacity = updateTableDto.Capacity;
        table.Status = updateTableDto.Status.ToLower();
        table.Location = updateTableDto.Location?.Trim();

        await _restaurantTableRepository.UpdateAsync(table);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(table);
    }

    /// <summary>
    /// Deletes a restaurant table
    /// </summary>
    /// <param name="id">Table ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var table = await _restaurantTableRepository.GetByIdAsync(id);
        if (table == null)
        {
            return false;
        }

        // Check if table has associated active reservations
        if (table.Reservations?.Any(r => !r.IsDeleted && r.Status != "cancelled" && r.Status != "completed") == true)
        {
            throw new InvalidOperationException("Cannot delete table that has active reservations");
        }

        await _restaurantTableRepository.DeleteAsync(table.Id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Checks if table number exists
    /// </summary>
    /// <param name="tableNumber">Table number</param>
    /// <param name="excludeId">Table ID to exclude from check</param>
    /// <returns>True if table number exists</returns>
    public async Task<bool> TableNumberExistsAsync(string tableNumber, int? excludeId = null)
    {
        return await _restaurantTableRepository.TableNumberExistsAsync(tableNumber, excludeId);
    }

    /// <summary>
    /// Gets tables with their reservation count
    /// </summary>
    /// <returns>Collection of tables with reservation counts</returns>
    public async Task<IEnumerable<RestaurantTableDto>> GetTablesWithReservationCountAsync()
    {
        var tables = await _restaurantTableRepository.GetTablesWithReservationCountAsync();
        return tables.Select(t => 
        {
            var dto = MapToDto(t);
            dto.ReservationCount = t.Reservations?.Count(r => !r.IsDeleted) ?? 0;
            return dto;
        });
    }

    /// <summary>
    /// Updates table status
    /// </summary>
    /// <param name="id">Table ID</param>
    /// <param name="status">New status</param>
    /// <returns>True if update was successful</returns>
    public async Task<bool> UpdateTableStatusAsync(int id, string status)
    {
        // Validate status
        var validStatuses = new[] { "available", "reserved", "occupied", "cleaning" };
        if (!validStatuses.Contains(status.ToLower()))
        {
            throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");
        }

        var result = await _restaurantTableRepository.UpdateTableStatusAsync(id, status.ToLower());
        if (result)
        {
            await _unitOfWork.SaveChangesAsync();
        }
        return result;
    }

    /// <summary>
    /// Gets all unique floor locations from tables
    /// </summary>
    /// <returns>Collection of distinct location names</returns>
    public async Task<IEnumerable<string>> GetDistinctLocationsAsync()
    {
        var tables = await _restaurantTableRepository.GetAllAsync();
        return tables
            .Where(t => !string.IsNullOrEmpty(t.Location))
            .Select(t => t.Location!)
            .Distinct()
            .OrderBy(l => l)
            .ToList();
    }

    /// <summary>
    /// Maps RestaurantTable entity to RestaurantTableDto
    /// </summary>
    /// <param name="table">RestaurantTable entity</param>
    /// <returns>Restaurant table DTO</returns>
    private static RestaurantTableDto MapToDto(RestaurantTable table)
    {
        return new RestaurantTableDto
        {
            Id = table.Id,
            TableNumber = table.TableNumber,
            Capacity = table.Capacity,
            Status = table.Status,
            Location = table.Location,
            CreatedAt = table.CreatedAt,
            ReservationCount = table.Reservations?.Count(r => !r.IsDeleted) ?? 0
        };
    }
}
