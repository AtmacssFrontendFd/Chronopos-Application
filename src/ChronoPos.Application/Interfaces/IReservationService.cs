using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Reservation operations
/// </summary>
public interface IReservationService
{
    /// <summary>
    /// Gets all reservations
    /// </summary>
    /// <returns>Collection of reservation DTOs</returns>
    Task<IEnumerable<ReservationDto>> GetAllAsync();

    /// <summary>
    /// Gets all active (non-deleted) reservations
    /// </summary>
    /// <returns>Collection of active reservation DTOs</returns>
    Task<IEnumerable<ReservationDto>> GetActiveReservationsAsync();

    /// <summary>
    /// Gets reservation by ID
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <returns>Reservation DTO if found</returns>
    Task<ReservationDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets reservations by customer ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Collection of reservation DTOs for the customer</returns>
    Task<IEnumerable<ReservationDto>> GetByCustomerIdAsync(int customerId);

    /// <summary>
    /// Gets reservations by table ID
    /// </summary>
    /// <param name="tableId">Table ID</param>
    /// <returns>Collection of reservation DTOs for the table</returns>
    Task<IEnumerable<ReservationDto>> GetByTableIdAsync(int tableId);

    /// <summary>
    /// Gets reservations by status
    /// </summary>
    /// <param name="status">Reservation status</param>
    /// <returns>Collection of reservation DTOs with specified status</returns>
    Task<IEnumerable<ReservationDto>> GetByStatusAsync(string status);

    /// <summary>
    /// Gets reservations for a specific date
    /// </summary>
    /// <param name="date">Reservation date</param>
    /// <returns>Collection of reservation DTOs for the date</returns>
    Task<IEnumerable<ReservationDto>> GetByDateAsync(DateTime date);

    /// <summary>
    /// Gets reservations for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of reservation DTOs within the date range</returns>
    Task<IEnumerable<ReservationDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets upcoming reservations (future dates)
    /// </summary>
    /// <returns>Collection of upcoming reservation DTOs</returns>
    Task<IEnumerable<ReservationDto>> GetUpcomingReservationsAsync();

    /// <summary>
    /// Gets reservations for today
    /// </summary>
    /// <returns>Collection of today's reservation DTOs</returns>
    Task<IEnumerable<ReservationDto>> GetTodayReservationsAsync();

    /// <summary>
    /// Creates a new reservation
    /// </summary>
    /// <param name="createReservationDto">Reservation data</param>
    /// <returns>Created reservation DTO</returns>
    Task<ReservationDto> CreateAsync(CreateReservationDto createReservationDto);

    /// <summary>
    /// Updates an existing reservation
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <param name="updateReservationDto">Updated reservation data</param>
    /// <returns>Updated reservation DTO</returns>
    Task<ReservationDto> UpdateAsync(int id, UpdateReservationDto updateReservationDto);

    /// <summary>
    /// Deletes a reservation (soft delete)
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int id);

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
    /// Updates reservation status
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <param name="status">New status</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateReservationStatusAsync(int id, string status);

    /// <summary>
    /// Confirms a reservation
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <returns>True if confirmation was successful</returns>
    Task<bool> ConfirmReservationAsync(int id);

    /// <summary>
    /// Cancels a reservation
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <returns>True if cancellation was successful</returns>
    Task<bool> CancelReservationAsync(int id);

    /// <summary>
    /// Checks in a reservation
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <returns>True if check-in was successful</returns>
    Task<bool> CheckInReservationAsync(int id);

    /// <summary>
    /// Completes a reservation
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <returns>True if completion was successful</returns>
    Task<bool> CompleteReservationAsync(int id);

    /// <summary>
    /// Gets reservations with detailed information
    /// </summary>
    /// <returns>Collection of reservation DTOs with related entity information</returns>
    Task<IEnumerable<ReservationDto>> GetReservationsWithDetailsAsync();
}
