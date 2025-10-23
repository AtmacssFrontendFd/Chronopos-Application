using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Reservation entity operations
/// </summary>
public class ReservationRepository : Repository<Reservation>, IReservationRepository
{
    public ReservationRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets reservations by customer ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Collection of reservations for the customer</returns>
    public async Task<IEnumerable<Reservation>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.CustomerId == customerId && !r.IsDeleted)
            .OrderByDescending(r => r.ReservationDate)
            .ThenByDescending(r => r.ReservationTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets reservations by table ID
    /// </summary>
    /// <param name="tableId">Table ID</param>
    /// <returns>Collection of reservations for the table</returns>
    public async Task<IEnumerable<Reservation>> GetByTableIdAsync(int tableId)
    {
        return await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.TableId == tableId && !r.IsDeleted)
            .OrderByDescending(r => r.ReservationDate)
            .ThenByDescending(r => r.ReservationTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets reservations by status
    /// </summary>
    /// <param name="status">Reservation status</param>
    /// <returns>Collection of reservations with specified status</returns>
    public async Task<IEnumerable<Reservation>> GetByStatusAsync(string status)
    {
        return await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.Status == status && !r.IsDeleted)
            .OrderByDescending(r => r.ReservationDate)
            .ThenByDescending(r => r.ReservationTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets reservations for a specific date
    /// </summary>
    /// <param name="date">Reservation date</param>
    /// <returns>Collection of reservations for the date</returns>
    public async Task<IEnumerable<Reservation>> GetByDateAsync(DateTime date)
    {
        var dateOnly = date.Date;
        return await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.ReservationDate.Date == dateOnly && !r.IsDeleted)
            .OrderBy(r => r.ReservationTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets reservations for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of reservations within the date range</returns>
    public async Task<IEnumerable<Reservation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var start = startDate.Date;
        var end = endDate.Date;
        
        return await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.ReservationDate.Date >= start && r.ReservationDate.Date <= end && !r.IsDeleted)
            .OrderBy(r => r.ReservationDate)
            .ThenBy(r => r.ReservationTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets active (non-deleted) reservations
    /// </summary>
    /// <returns>Collection of active reservations</returns>
    public async Task<IEnumerable<Reservation>> GetActiveReservationsAsync()
    {
        return await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.ReservationDate)
            .ThenByDescending(r => r.ReservationTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets upcoming reservations (future dates)
    /// </summary>
    /// <returns>Collection of upcoming reservations</returns>
    public async Task<IEnumerable<Reservation>> GetUpcomingReservationsAsync()
    {
        var today = DateTime.UtcNow.Date;
        
        return await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.ReservationDate.Date >= today && !r.IsDeleted)
            .OrderBy(r => r.ReservationDate)
            .ThenBy(r => r.ReservationTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets reservations for today
    /// </summary>
    /// <returns>Collection of today's reservations</returns>
    public async Task<IEnumerable<Reservation>> GetTodayReservationsAsync()
    {
        var today = DateTime.UtcNow.Date;
        
        return await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.ReservationDate.Date == today && !r.IsDeleted)
            .OrderBy(r => r.ReservationTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets reservations by table and date (for checking conflicts)
    /// </summary>
    /// <param name="tableId">Table ID</param>
    /// <param name="date">Reservation date</param>
    /// <param name="excludeId">Reservation ID to exclude (for updates)</param>
    /// <returns>Collection of reservations for the table on the date</returns>
    public async Task<IEnumerable<Reservation>> GetTableReservationsByDateAsync(int tableId, DateTime date, int? excludeId = null)
    {
        var dateOnly = date.Date;
        var query = _context.Set<Reservation>()
            .Where(r => r.TableId == tableId && 
                       r.ReservationDate.Date == dateOnly && 
                       !r.IsDeleted &&
                       r.Status != "cancelled");

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return await query
            .OrderBy(r => r.ReservationTime)
            .ToListAsync();
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
        var dateOnly = date.Date;
        
        // Check for reservations within 2 hours of the requested time
        var timeWindowStart = time.Add(TimeSpan.FromHours(-2));
        var timeWindowEnd = time.Add(TimeSpan.FromHours(2));

        var query = _context.Set<Reservation>()
            .Where(r => r.TableId == tableId && 
                       r.ReservationDate.Date == dateOnly && 
                       !r.IsDeleted &&
                       r.Status != "cancelled" &&
                       r.ReservationTime >= timeWindowStart &&
                       r.ReservationTime <= timeWindowEnd);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    /// <summary>
    /// Soft deletes a reservation (sets IsDeleted flag)
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> SoftDeleteAsync(int id)
    {
        var reservation = await _context.Set<Reservation>().FindAsync(id);
        if (reservation == null)
        {
            return false;
        }

        reservation.IsDeleted = true;
        reservation.UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Updates reservation status
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <param name="status">New status</param>
    /// <returns>True if update was successful</returns>
    public async Task<bool> UpdateReservationStatusAsync(int id, string status)
    {
        var reservation = await _context.Set<Reservation>().FindAsync(id);
        if (reservation == null)
        {
            return false;
        }

        reservation.Status = status;
        reservation.UpdatedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Gets reservations with related entities (Customer, Table, PaymentType)
    /// </summary>
    /// <returns>Collection of reservations with navigation properties loaded</returns>
    public async Task<IEnumerable<Reservation>> GetReservationsWithDetailsAsync()
    {
        return await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.ReservationDate)
            .ThenByDescending(r => r.ReservationTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all reservations including related data
    /// </summary>
    /// <returns>Collection of reservations with navigation properties</returns>
    public override async Task<IEnumerable<Reservation>> GetAllAsync()
    {
        return await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.ReservationDate)
            .ThenByDescending(r => r.ReservationTime)
            .ToListAsync();
    }

    /// <summary>
    /// Gets reservation by ID including related data
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <returns>Reservation with navigation properties</returns>
    public override async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
