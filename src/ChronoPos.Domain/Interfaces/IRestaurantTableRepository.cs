using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for RestaurantTable entity operations
/// </summary>
public interface IRestaurantTableRepository : IRepository<RestaurantTable>
{
    /// <summary>
    /// Gets table by table number
    /// </summary>
    /// <param name="tableNumber">Table number</param>
    /// <returns>RestaurantTable entity if found</returns>
    Task<RestaurantTable?> GetByTableNumberAsync(string tableNumber);

    /// <summary>
    /// Gets all available tables
    /// </summary>
    /// <returns>Collection of available tables</returns>
    Task<IEnumerable<RestaurantTable>> GetAvailableTablesAsync();

    /// <summary>
    /// Gets tables by status
    /// </summary>
    /// <param name="status">Table status (available, reserved, occupied, cleaning)</param>
    /// <returns>Collection of tables with specified status</returns>
    Task<IEnumerable<RestaurantTable>> GetTablesByStatusAsync(string status);

    /// <summary>
    /// Gets tables by location
    /// </summary>
    /// <param name="location">Location name</param>
    /// <returns>Collection of tables in specified location</returns>
    Task<IEnumerable<RestaurantTable>> GetTablesByLocationAsync(string location);

    /// <summary>
    /// Gets tables with minimum capacity
    /// </summary>
    /// <param name="capacity">Minimum capacity required</param>
    /// <returns>Collection of tables with at least the specified capacity</returns>
    Task<IEnumerable<RestaurantTable>> GetTablesByMinCapacityAsync(int capacity);

    /// <summary>
    /// Checks if table number exists (case-insensitive)
    /// </summary>
    /// <param name="tableNumber">Table number to check</param>
    /// <param name="excludeId">Table ID to exclude from check (for updates)</param>
    /// <returns>True if table number exists</returns>
    Task<bool> TableNumberExistsAsync(string tableNumber, int? excludeId = null);

    /// <summary>
    /// Gets tables with their reservation count
    /// </summary>
    /// <returns>Collection of tables with reservation counts</returns>
    Task<IEnumerable<RestaurantTable>> GetTablesWithReservationCountAsync();

    /// <summary>
    /// Updates table status
    /// </summary>
    /// <param name="id">Table ID</param>
    /// <param name="status">New status</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateTableStatusAsync(int id, string status);
}
