using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using ChronoPos.Application.Logging;

namespace ChronoPos.Infrastructure.Repositories;

public class ReservationRepository : Repository<Reservation>, IReservationRepository
{
    public ReservationRepository(ChronoPosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Reservation>> GetByCustomerIdAsync(int customerId)
    {
        var reservations = await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.CustomerId == customerId && r.IsDeleted == false)
            .ToListAsync();
            
        return reservations.OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.ReservationTime).ToList();
    }

    public async Task<IEnumerable<Reservation>> GetByTableIdAsync(int tableId)
    {
        var reservations = await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.TableId == tableId && r.IsDeleted == false)
            .ToListAsync();
            
        return reservations.OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.ReservationTime).ToList();
    }

    public async Task<IEnumerable<Reservation>> GetByStatusAsync(string status)
    {
        var reservations = await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.Status == status && r.IsDeleted == false)
            .ToListAsync();
            
        return reservations.OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.ReservationTime).ToList();
    }

    public async Task<IEnumerable<Reservation>> GetByDateAsync(DateTime date)
    {
        var dateOnly = date.Date;
        AppLogger.LogInfo($"[Repository] GetByDateAsync called with date: {date:yyyy-MM-dd}", filename: "reservation");
        AppLogger.LogInfo($"[Repository] Searching for date: {dateOnly:yyyy-MM-dd}", filename: "reservation");
        
        var reservations = await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.ReservationDate.Date == dateOnly && r.IsDeleted == false)
            .ToListAsync();
        
        AppLogger.LogInfo($"[Repository] Found {reservations.Count} reservations for date {dateOnly:yyyy-MM-dd}", filename: "reservation");
        if (reservations.Any())
        {
            AppLogger.LogInfo($"[Repository] First reservation: ID={reservations[0].Id}, Customer={reservations[0].Customer?.CustomerFullName ?? "NULL"}, Table={reservations[0].Table?.TableNumber ?? "NULL"}, Time={reservations[0].ReservationTime}", filename: "reservation");
        }
        
        return reservations.OrderBy(r => r.ReservationTime).ToList();
    }

    public async Task<IEnumerable<Reservation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var start = startDate.Date;
        var end = endDate.Date;
        
        var reservations = await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.ReservationDate.Date >= start && r.ReservationDate.Date <= end && r.IsDeleted == false)
            .ToListAsync();
            
        return reservations.OrderBy(r => r.ReservationDate).ThenBy(r => r.ReservationTime).ToList();
    }

    public async Task<IEnumerable<Reservation>> GetActiveReservationsAsync()
    {
        var reservations = await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.IsDeleted == false)
            .ToListAsync();
            
        return reservations.OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.ReservationTime).ToList();
    }

    public async Task<IEnumerable<Reservation>> GetUpcomingReservationsAsync()
    {
        var today = DateTime.UtcNow.Date;
        
        var reservations = await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.ReservationDate.Date >= today && r.IsDeleted == false)
            .ToListAsync();
            
        return reservations.OrderBy(r => r.ReservationDate).ThenBy(r => r.ReservationTime).ToList();
    }

    public async Task<IEnumerable<Reservation>> GetTodayReservationsAsync()
    {
        var today = DateTime.UtcNow.Date;
        
        var reservations = await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.ReservationDate.Date == today && r.IsDeleted == false)
            .ToListAsync();
            
        return reservations.OrderBy(r => r.ReservationTime).ToList();
    }

    public async Task<IEnumerable<Reservation>> GetTableReservationsByDateAsync(int tableId, DateTime date, int? excludeId = null)
    {
        var dateOnly = date.Date;
        var query = _context.Set<Reservation>()
            .Where(r => r.TableId == tableId && r.ReservationDate.Date == dateOnly && r.IsDeleted == false && r.Status != "cancelled");

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        var reservations = await query.ToListAsync();
        return reservations.OrderBy(r => r.ReservationTime).ToList();
    }

    public async Task<bool> IsTimeSlotAvailableAsync(int tableId, DateTime date, TimeSpan time, int? excludeId = null)
    {
        var dateOnly = date.Date;
        
        // Fetch all reservations for this table on this date that are not cancelled
        var query = _context.Set<Reservation>()
            .Where(r => r.TableId == tableId 
                && r.ReservationDate.Date == dateOnly 
                && r.IsDeleted == false 
                && r.Status != "cancelled");

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        var reservations = await query.ToListAsync();
        
        // Check for conflicts in memory (TimeSpan comparison doesn't translate to SQL)
        var timeWindowStart = time.Add(TimeSpan.FromHours(-2));
        var timeWindowEnd = time.Add(TimeSpan.FromHours(2));
        
        var hasConflict = reservations.Any(r => 
            r.ReservationTime >= timeWindowStart && r.ReservationTime <= timeWindowEnd);
        
        return !hasConflict;
    }

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

    public async Task<IEnumerable<Reservation>> GetReservationsWithDetailsAsync()
    {
        var reservations = await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.IsDeleted == false)
            .ToListAsync();
            
        return reservations.OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.ReservationTime).ToList();
    }

    public override async Task<IEnumerable<Reservation>> GetAllAsync()
    {
        var reservations = await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .Where(r => r.IsDeleted == false)
            .ToListAsync();
            
        return reservations.OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.ReservationTime).ToList();
    }

    public override async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _context.Set<Reservation>()
            .Include(r => r.Customer)
            .Include(r => r.Table)
            .Include(r => r.PaymentType)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
