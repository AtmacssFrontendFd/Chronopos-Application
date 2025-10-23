using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for Reservation entity operations
/// </summary>
public interface IReservationRepository : IRepository<Reservation>
{
    /// <summary>
    /// Gets reservations by customer ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Collection of reservations for the customer</returns>
    Task<IEnumerable<Reservation>> GetByCustomerIdAsync(int customerId);

    /// <summary>
    /// Gets reservations by table ID
    /// </summary>
    /// <param name="tableId">Table ID</param>
    /// <returns>Collection of reservations for the table</returns>
    Task<IEnumerable<Reservation>> GetByTableIdAsync(int tableId);

    /// <summary>
    /// Gets reservations by status
    /// </summary>
    /// <param name="status">Reservation status</param>
    /// <returns>Collection of reservations with specified status</returns>
    Task<IEnumerable<Reservation>> GetByStatusAsync(string status);

    /// <summary>
    /// Gets reservations for a specific date
    /// </summary>
    /// <param name="date">Reservation date</param>
    /// <returns>Collection of reservations for the date</returns>
    Task<IEnumerable<Reservation>> GetByDateAsync(DateTime date);

    /// <summary>
    /// Gets reservations for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of reservations within the date range</returns>
    Task<IEnumerable<Reservation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets active (non-deleted) reservations
    /// </summary>
    /// <returns>Collection of active reservations</returns>
    Task<IEnumerable<Reservation>> GetActiveReservationsAsync();

    /// <summary>
    /// Gets upcoming reservations (future dates)
    /// </summary>
    /// <returns>Collection of upcoming reservations</returns>
    Task<IEnumerable<Reservation>> GetUpcomingReservationsAsync();

    /// <summary>
    /// Gets reservations for today
    /// </summary>
    /// <returns>Collection of today's reservations</returns>
    Task<IEnumerable<Reservation>> GetTodayReservationsAsync();

    /// <summary>
    /// Gets reservations by table and date (for checking conflicts)
    /// </summary>
    /// <param name="tableId">Table ID</param>
    /// <param name="date">Reservation date</param>
    /// <param name="excludeId">Reservation ID to exclude (for updates)</param>
    /// <returns>Collection of reservations for the table on the date</returns>
    Task<IEnumerable<Reservation>> GetTableReservationsByDateAsync(int tableId, DateTime date, int? excludeId = null);

    /// <summary>
    /// Checks if a time slot is available for a table
    /// </summary>
    /// <param name="tableId">Table ID</param>
    /// <param name="date">Reservation date</param>
    /// <param name="time">Reservation time</param>
    /// <param name="excludeId">Reservation ID to exclude (for updates)</param>
    /// <returns>True if time slot is available</returns>
    Task<bool> IsTimeSlotAvailableAsync(int tableId, DateTime date, TimeSpan time, int? excludeId = null);

    /// <summary>
    /// Soft deletes a reservation (sets IsDeleted flag)
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> SoftDeleteAsync(int id);

    /// <summary>
    /// Updates reservation status
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <param name="status">New status</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateReservationStatusAsync(int id, string status);

    /// <summary>
    /// Gets reservations with related entities (Customer, Table, PaymentType)
    /// </summary>
    /// <returns>Collection of reservations with navigation properties loaded</returns>
    Task<IEnumerable<Reservation>> GetReservationsWithDetailsAsync();
}
