using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Logging;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Reservation operations
/// </summary>
public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRestaurantTableRepository _restaurantTableRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReservationService(
        IReservationRepository reservationRepository, 
        IRestaurantTableRepository restaurantTableRepository,
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository ?? throw new ArgumentNullException(nameof(reservationRepository));
        _restaurantTableRepository = restaurantTableRepository ?? throw new ArgumentNullException(nameof(restaurantTableRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all reservations
    /// </summary>
    /// <returns>Collection of reservation DTOs</returns>
    public async Task<IEnumerable<ReservationDto>> GetAllAsync()
    {
        var reservations = await _reservationRepository.GetAllAsync();
        return reservations.Select(MapToDto);
    }

    /// <summary>
    /// Gets all active (non-deleted) reservations
    /// </summary>
    /// <returns>Collection of active reservation DTOs</returns>
    public async Task<IEnumerable<ReservationDto>> GetActiveReservationsAsync()
    {
        var reservations = await _reservationRepository.GetActiveReservationsAsync();
        return reservations.Select(MapToDto);
    }

    /// <summary>
    /// Gets reservation by ID
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <returns>Reservation DTO if found</returns>
    public async Task<ReservationDto?> GetByIdAsync(int id)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        return reservation != null ? MapToDto(reservation) : null;
    }

    /// <summary>
    /// Gets reservations by customer ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Collection of reservation DTOs for the customer</returns>
    public async Task<IEnumerable<ReservationDto>> GetByCustomerIdAsync(int customerId)
    {
        var reservations = await _reservationRepository.GetByCustomerIdAsync(customerId);
        return reservations.Select(MapToDto);
    }

    /// <summary>
    /// Gets reservations by table ID
    /// </summary>
    /// <param name="tableId">Table ID</param>
    /// <returns>Collection of reservation DTOs for the table</returns>
    public async Task<IEnumerable<ReservationDto>> GetByTableIdAsync(int tableId)
    {
        var reservations = await _reservationRepository.GetByTableIdAsync(tableId);
        return reservations.Select(MapToDto);
    }

    /// <summary>
    /// Gets reservations by status
    /// </summary>
    /// <param name="status">Reservation status</param>
    /// <returns>Collection of reservation DTOs with specified status</returns>
    public async Task<IEnumerable<ReservationDto>> GetByStatusAsync(string status)
    {
        var reservations = await _reservationRepository.GetByStatusAsync(status);
        return reservations.Select(MapToDto);
    }

    /// <summary>
    /// Gets reservations for a specific date
    /// </summary>
    /// <param name="date">Reservation date</param>
    /// <returns>Collection of reservation DTOs for the date</returns>
    public async Task<IEnumerable<ReservationDto>> GetByDateAsync(DateTime date)
    {
        AppLogger.LogInfo($"[Service] GetByDateAsync called with date: {date:yyyy-MM-dd}", filename: "reservation");
        var reservations = await _reservationRepository.GetByDateAsync(date);
        var dtos = reservations.Select(MapToDto).ToList();
        AppLogger.LogInfo($"[Service] Mapped {dtos.Count} reservations to DTOs", filename: "reservation");
        if (dtos.Any())
        {
            AppLogger.LogInfo($"[Service] First DTO: ID={dtos[0].Id}, CustomerName={dtos[0].CustomerName}, TableNumber={dtos[0].TableNumber}, ReservationDateTime={dtos[0].ReservationDateTime}", filename: "reservation");
        }
        return dtos;
    }

    /// <summary>
    /// Gets reservations for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of reservation DTOs within the date range</returns>
    public async Task<IEnumerable<ReservationDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var reservations = await _reservationRepository.GetByDateRangeAsync(startDate, endDate);
        return reservations.Select(MapToDto);
    }

    /// <summary>
    /// Gets upcoming reservations (future dates)
    /// </summary>
    /// <returns>Collection of upcoming reservation DTOs</returns>
    public async Task<IEnumerable<ReservationDto>> GetUpcomingReservationsAsync()
    {
        var reservations = await _reservationRepository.GetUpcomingReservationsAsync();
        return reservations.Select(MapToDto);
    }

    /// <summary>
    /// Gets reservations for today
    /// </summary>
    /// <returns>Collection of today's reservation DTOs</returns>
    public async Task<IEnumerable<ReservationDto>> GetTodayReservationsAsync()
    {
        var reservations = await _reservationRepository.GetTodayReservationsAsync();
        return reservations.Select(MapToDto);
    }

    /// <summary>
    /// Creates a new reservation
    /// </summary>
    /// <param name="createReservationDto">Reservation data</param>
    /// <returns>Created reservation DTO</returns>
    public async Task<ReservationDto> CreateAsync(CreateReservationDto createReservationDto)
    {
        // Validate status
        var validStatuses = new[] { "waiting", "confirmed", "cancelled", "checked_in", "completed" };
        if (!validStatuses.Contains(createReservationDto.Status.ToLower()))
        {
            throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");
        }

        // Validate reservation date is not in the past
        if (createReservationDto.ReservationDate.Date < DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Reservation date cannot be in the past");
        }

        // Check if table exists
        var table = await _restaurantTableRepository.GetByIdAsync(createReservationDto.TableId);
        if (table == null)
        {
            throw new ArgumentException($"Table with ID {createReservationDto.TableId} not found");
        }

        // Check if time slot is available
        if (!await _reservationRepository.IsTimeSlotAvailableAsync(
            createReservationDto.TableId, 
            createReservationDto.ReservationDate, 
            createReservationDto.ReservationTime))
        {
            throw new InvalidOperationException("The selected time slot is not available for this table");
        }

        var reservation = new Reservation
        {
            CustomerId = createReservationDto.CustomerId,
            TableId = createReservationDto.TableId,
            NumberOfPersons = createReservationDto.NumberOfPersons,
            ReservationDate = createReservationDto.ReservationDate,
            ReservationTime = createReservationDto.ReservationTime,
            DepositFee = createReservationDto.DepositFee,
            PaymentTypeId = createReservationDto.PaymentTypeId,
            Status = createReservationDto.Status.ToLower(),
            Notes = createReservationDto.Notes?.Trim(),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _reservationRepository.AddAsync(reservation);
        await _unitOfWork.SaveChangesAsync();

        // Update table status to reserved if reservation is confirmed
        if (reservation.Status == "confirmed")
        {
            await _restaurantTableRepository.UpdateTableStatusAsync(reservation.TableId, "reserved");
            await _unitOfWork.SaveChangesAsync();
        }

        // Reload to get navigation properties
        var createdReservation = await _reservationRepository.GetByIdAsync(reservation.Id);
        return MapToDto(createdReservation!);
    }

    /// <summary>
    /// Updates an existing reservation
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <param name="updateReservationDto">Updated reservation data</param>
    /// <returns>Updated reservation DTO</returns>
    public async Task<ReservationDto> UpdateAsync(int id, UpdateReservationDto updateReservationDto)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null)
        {
            throw new ArgumentException($"Reservation with ID {id} not found");
        }

        if (reservation.IsDeleted)
        {
            throw new InvalidOperationException("Cannot update a deleted reservation");
        }

        // Validate status
        var validStatuses = new[] { "waiting", "confirmed", "cancelled", "checked_in", "completed" };
        if (!validStatuses.Contains(updateReservationDto.Status.ToLower()))
        {
            throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");
        }

        // Validate reservation date is not in the past (only if changing the date)
        if (updateReservationDto.ReservationDate.Date != reservation.ReservationDate.Date &&
            updateReservationDto.ReservationDate.Date < DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Reservation date cannot be in the past");
        }

        // Check if table exists (if changing table)
        if (updateReservationDto.TableId != reservation.TableId)
        {
            var table = await _restaurantTableRepository.GetByIdAsync(updateReservationDto.TableId);
            if (table == null)
            {
                throw new ArgumentException($"Table with ID {updateReservationDto.TableId} not found");
            }
        }

        // Check if time slot is available (if changing table, date, or time)
        if (updateReservationDto.TableId != reservation.TableId ||
            updateReservationDto.ReservationDate.Date != reservation.ReservationDate.Date ||
            updateReservationDto.ReservationTime != reservation.ReservationTime)
        {
            if (!await _reservationRepository.IsTimeSlotAvailableAsync(
                updateReservationDto.TableId, 
                updateReservationDto.ReservationDate, 
                updateReservationDto.ReservationTime,
                id))
            {
                throw new InvalidOperationException("The selected time slot is not available for this table");
            }
        }

        var oldTableId = reservation.TableId;
        var oldStatus = reservation.Status;

        reservation.CustomerId = updateReservationDto.CustomerId;
        reservation.TableId = updateReservationDto.TableId;
        reservation.NumberOfPersons = updateReservationDto.NumberOfPersons;
        reservation.ReservationDate = updateReservationDto.ReservationDate;
        reservation.ReservationTime = updateReservationDto.ReservationTime;
        reservation.DepositFee = updateReservationDto.DepositFee;
        reservation.PaymentTypeId = updateReservationDto.PaymentTypeId;
        reservation.Status = updateReservationDto.Status.ToLower();
        reservation.Notes = updateReservationDto.Notes?.Trim();
        reservation.UpdatedAt = DateTime.UtcNow;

        await _reservationRepository.UpdateAsync(reservation);
        await _unitOfWork.SaveChangesAsync();

        // Update table statuses if needed
        if (oldStatus != reservation.Status || oldTableId != reservation.TableId)
        {
            // If old table no longer has active reservations, set it to available
            if (oldTableId != reservation.TableId)
            {
                var oldTableReservations = await _reservationRepository.GetByTableIdAsync(oldTableId);
                if (!oldTableReservations.Any(r => r.Status == "confirmed" && r.Id != id))
                {
                    await _restaurantTableRepository.UpdateTableStatusAsync(oldTableId, "available");
                }
            }

            // Update new table status based on reservation status
            if (reservation.Status == "confirmed")
            {
                await _restaurantTableRepository.UpdateTableStatusAsync(reservation.TableId, "reserved");
            }
            else if (reservation.Status == "checked_in")
            {
                await _restaurantTableRepository.UpdateTableStatusAsync(reservation.TableId, "occupied");
            }

            await _unitOfWork.SaveChangesAsync();
        }

        // Reload to get navigation properties
        var updatedReservation = await _reservationRepository.GetByIdAsync(reservation.Id);
        return MapToDto(updatedReservation!);
    }

    /// <summary>
    /// Deletes a reservation (soft delete)
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null)
        {
            return false;
        }

        var result = await _reservationRepository.SoftDeleteAsync(id);
        if (result)
        {
            await _unitOfWork.SaveChangesAsync();

            // Update table status to available if no other active reservations
            var tableReservations = await _reservationRepository.GetByTableIdAsync(reservation.TableId);
            if (!tableReservations.Any(r => !r.IsDeleted && r.Status == "confirmed" && r.Id != id))
            {
                await _restaurantTableRepository.UpdateTableStatusAsync(reservation.TableId, "available");
                await _unitOfWork.SaveChangesAsync();
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if a time slot is available for a table
    /// </summary>
    /// <param name="tableId">Table ID</param>
    /// <param name="date">Reservation date</param>
    /// <param name="time">Reservation time</param>
    /// <param name="excludeId">Reservation ID to exclude (for updates)</param>
    /// <returns>True if time slot is available</returns>
    public async Task<bool> IsTimeSlotAvailableAsync(int tableId, DateTime date, TimeSpan time, int? excludeId = null)
    {
        return await _reservationRepository.IsTimeSlotAvailableAsync(tableId, date, time, excludeId);
    }

    /// <summary>
    /// Updates reservation status
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <param name="status">New status</param>
    /// <returns>True if update was successful</returns>
    public async Task<bool> UpdateReservationStatusAsync(int id, string status)
    {
        var validStatuses = new[] { "waiting", "confirmed", "cancelled", "checked_in", "completed" };
        if (!validStatuses.Contains(status.ToLower()))
        {
            throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");
        }

        var reservation = await _reservationRepository.GetByIdAsync(id);
        if (reservation == null)
        {
            return false;
        }

        var result = await _reservationRepository.UpdateReservationStatusAsync(id, status.ToLower());
        if (result)
        {
            await _unitOfWork.SaveChangesAsync();

            // Update table status based on reservation status
            if (status.ToLower() == "confirmed")
            {
                await _restaurantTableRepository.UpdateTableStatusAsync(reservation.TableId, "reserved");
            }
            else if (status.ToLower() == "checked_in")
            {
                await _restaurantTableRepository.UpdateTableStatusAsync(reservation.TableId, "occupied");
            }
            else if (status.ToLower() == "completed" || status.ToLower() == "cancelled")
            {
                // Check if table has other active reservations
                var tableReservations = await _reservationRepository.GetByTableIdAsync(reservation.TableId);
                if (!tableReservations.Any(r => !r.IsDeleted && 
                    (r.Status == "confirmed" || r.Status == "checked_in") && 
                    r.Id != id))
                {
                    await _restaurantTableRepository.UpdateTableStatusAsync(reservation.TableId, "available");
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        return result;
    }

    /// <summary>
    /// Confirms a reservation
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <returns>True if confirmation was successful</returns>
    public async Task<bool> ConfirmReservationAsync(int id)
    {
        return await UpdateReservationStatusAsync(id, "confirmed");
    }

    /// <summary>
    /// Cancels a reservation
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <returns>True if cancellation was successful</returns>
    public async Task<bool> CancelReservationAsync(int id)
    {
        return await UpdateReservationStatusAsync(id, "cancelled");
    }

    /// <summary>
    /// Checks in a reservation
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <returns>True if check-in was successful</returns>
    public async Task<bool> CheckInReservationAsync(int id)
    {
        return await UpdateReservationStatusAsync(id, "checked_in");
    }

    /// <summary>
    /// Completes a reservation
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <returns>True if completion was successful</returns>
    public async Task<bool> CompleteReservationAsync(int id)
    {
        return await UpdateReservationStatusAsync(id, "completed");
    }

    /// <summary>
    /// Gets reservations with detailed information
    /// </summary>
    /// <returns>Collection of reservation DTOs with related entity information</returns>
    public async Task<IEnumerable<ReservationDto>> GetReservationsWithDetailsAsync()
    {
        var reservations = await _reservationRepository.GetReservationsWithDetailsAsync();
        return reservations.Select(MapToDto);
    }

    /// <summary>
    /// Gets reservations for a specific date and location (floor)
    /// </summary>
    /// <param name="date">Reservation date</param>
    /// <param name="location">Table location/floor</param>
    /// <returns>Collection of reservation DTOs for the date and location</returns>
    public async Task<IEnumerable<ReservationDto>> GetByDateAndLocationAsync(DateTime date, string location)
    {
        var reservations = await _reservationRepository.GetByDateAsync(date);
        var filtered = reservations.Where(r => 
            !r.IsDeleted && 
            r.Table?.Location?.Equals(location, StringComparison.OrdinalIgnoreCase) == true);
        return filtered.Select(MapToDto);
    }

    /// <summary>
    /// Gets reservations by date with table details for timeline grid
    /// Includes only active reservations with full table information
    /// </summary>
    /// <param name="date">Reservation date</param>
    /// <param name="location">Optional location/floor filter</param>
    /// <returns>Collection of reservation DTOs optimized for timeline display</returns>
    public async Task<IEnumerable<ReservationDto>> GetReservationsForTimelineAsync(DateTime date, string? location = null)
    {
        AppLogger.LogInfo($"[Service] GetReservationsForTimelineAsync called - Date: {date:yyyy-MM-dd}, Location: {location ?? "All"}", filename: "reservation");
        
        var reservations = await _reservationRepository.GetReservationsWithDetailsAsync();
        AppLogger.LogInfo($"[Service] GetReservationsWithDetailsAsync returned {reservations.Count()} reservations", filename: "reservation");
        
        var filtered = reservations.Where(r => 
            !r.IsDeleted && 
            r.ReservationDate.Date == date.Date &&
            r.Status != "cancelled");
        
        var afterDateFilter = filtered.ToList();
        AppLogger.LogInfo($"[Service] After date filter: {afterDateFilter.Count} reservations for {date.Date:yyyy-MM-dd}", filename: "reservation");

        // Filter by location only if a specific location is provided (not "All Locations")
        if (!string.IsNullOrEmpty(location) && location != "All Locations")
        {
            filtered = afterDateFilter.Where(r => 
                r.Table?.Location?.Equals(location, StringComparison.OrdinalIgnoreCase) == true);
            AppLogger.LogInfo($"[Service] After location filter '{location}': {filtered.Count()} reservations", filename: "reservation");
        }
        else
        {
            filtered = afterDateFilter;
            AppLogger.LogInfo($"[Service] No location filter applied (showing all locations): {afterDateFilter.Count} reservations", filename: "reservation");
        }

        var result = filtered
            .OrderBy(r => r.Table?.TableNumber)
            .ThenBy(r => r.ReservationTime)
            .Select(MapToDto)
            .ToList();
        
        AppLogger.LogInfo($"[Service] Returning {result.Count} reservations for timeline", filename: "reservation");
        if (result.Any())
        {
            AppLogger.LogInfo($"[Service] First timeline reservation: ID={result[0].Id}, Customer={result[0].CustomerName}, Table={result[0].TableNumber}, DateTime={result[0].ReservationDateTime}", filename: "reservation");
        }
        
        return result;
    }

    /// <summary>
    /// Maps Reservation entity to ReservationDto
    /// </summary>
    /// <param name="reservation">Reservation entity</param>
    /// <returns>Reservation DTO</returns>
    private static ReservationDto MapToDto(Reservation reservation)
    {
        return new ReservationDto
        {
            Id = reservation.Id,
            CustomerId = reservation.CustomerId,
            TableId = reservation.TableId,
            NumberOfPersons = reservation.NumberOfPersons,
            ReservationDate = reservation.ReservationDate,
            ReservationTime = reservation.ReservationTime,
            DepositFee = reservation.DepositFee,
            PaymentTypeId = reservation.PaymentTypeId,
            Status = reservation.Status,
            Notes = reservation.Notes,
            IsDeleted = reservation.IsDeleted,
            CreatedAt = reservation.CreatedAt,
            UpdatedAt = reservation.UpdatedAt,
            CustomerName = reservation.Customer?.DisplayName,
            CustomerMobile = reservation.Customer?.PrimaryMobile,
            TableNumber = reservation.Table?.TableNumber,
            PaymentTypeName = reservation.PaymentType?.Name
        };
    }
}
