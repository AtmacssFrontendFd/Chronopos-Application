using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for RestaurantTable operations
/// </summary>
public interface IRestaurantTableService
{
    /// <summary>
    /// Gets all restaurant tables
    /// </summary>
    /// <returns>Collection of restaurant table DTOs</returns>
    Task<IEnumerable<RestaurantTableDto>> GetAllAsync();

    /// <summary>
    /// Gets all available tables
    /// </summary>
    /// <returns>Collection of available table DTOs</returns>
    Task<IEnumerable<RestaurantTableDto>> GetAvailableTablesAsync();

    /// <summary>
    /// Gets table by ID
    /// </summary>
    /// <param name="id">Table ID</param>
    /// <returns>Restaurant table DTO if found</returns>
    Task<RestaurantTableDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets table by table number
    /// </summary>
    /// <param name="tableNumber">Table number</param>
    /// <returns>Restaurant table DTO if found</returns>
    Task<RestaurantTableDto?> GetByTableNumberAsync(string tableNumber);

    /// <summary>
    /// Gets tables by status
    /// </summary>
    /// <param name="status">Table status</param>
    /// <returns>Collection of table DTOs with specified status</returns>
    Task<IEnumerable<RestaurantTableDto>> GetTablesByStatusAsync(string status);

    /// <summary>
    /// Gets tables by location
    /// </summary>
    /// <param name="location">Location name</param>
    /// <returns>Collection of table DTOs in specified location</returns>
    Task<IEnumerable<RestaurantTableDto>> GetTablesByLocationAsync(string location);

    /// <summary>
    /// Gets tables with minimum capacity
    /// </summary>
    /// <param name="capacity">Minimum capacity required</param>
    /// <returns>Collection of table DTOs with at least the specified capacity</returns>
    Task<IEnumerable<RestaurantTableDto>> GetTablesByMinCapacityAsync(int capacity);

    /// <summary>
    /// Creates a new restaurant table
    /// </summary>
    /// <param name="createTableDto">Table data</param>
    /// <returns>Created table DTO</returns>
    Task<RestaurantTableDto> CreateAsync(CreateRestaurantTableDto createTableDto);

    /// <summary>
    /// Updates an existing restaurant table
    /// </summary>
    /// <param name="id">Table ID</param>
    /// <param name="updateTableDto">Updated table data</param>
    /// <returns>Updated table DTO</returns>
    Task<RestaurantTableDto> UpdateAsync(int id, UpdateRestaurantTableDto updateTableDto);

    /// <summary>
    /// Deletes a restaurant table
    /// </summary>
    /// <param name="id">Table ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if table number exists
    /// </summary>
    /// <param name="tableNumber">Table number</param>
    /// <param name="excludeId">Table ID to exclude from check</param>
    /// <returns>True if table number exists</returns>
    Task<bool> TableNumberExistsAsync(string tableNumber, int? excludeId = null);

    /// <summary>
    /// Gets tables with their reservation count
    /// </summary>
    /// <returns>Collection of tables with reservation counts</returns>
    Task<IEnumerable<RestaurantTableDto>> GetTablesWithReservationCountAsync();

    /// <summary>
    /// Updates table status
    /// </summary>
    /// <param name="id">Table ID</param>
    /// <param name="status">New status</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateTableStatusAsync(int id, string status);
}
