using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Models;
using System.Windows.Media;

namespace ChronoPos.Desktop.Helpers;

/// <summary>
/// Helper class for managing time slot grid calculations and operations
/// </summary>
public static class TimeSlotGridHelper
{
    // Default business hours: 10:00 AM to 10:00 PM
    public const int DefaultStartHour = 10;
    public const int DefaultEndHour = 22;
    public const int DefaultReservationDurationHours = 2;

    /// <summary>
    /// Generates time slot headers for the timeline grid
    /// </summary>
    /// <param name="startHour">Start hour (24-hour format, default 10)</param>
    /// <param name="endHour">End hour (24-hour format, default 22)</param>
    /// <returns>List of time slot headers</returns>
    public static List<TimeSlotHeader> GenerateTimeSlots(int startHour = DefaultStartHour, int endHour = DefaultEndHour)
    {
        var slots = new List<TimeSlotHeader>();
        
        for (int hour = startHour; hour <= endHour; hour++)
        {
            var timeSlot = new TimeSlotHeader
            {
                Hour = hour,
                DisplayTime = FormatHourTo12Hour(hour),
                ColumnIndex = hour - startHour
            };
            slots.Add(timeSlot);
        }

        return slots;
    }

    /// <summary>
    /// Converts 24-hour time to 12-hour format with AM/PM
    /// </summary>
    /// <param name="hour">Hour in 24-hour format</param>
    /// <returns>Formatted time string (e.g., "10:00 AM")</returns>
    public static string FormatHourTo12Hour(int hour)
    {
        if (hour == 0) return "12:00 AM";
        if (hour < 12) return $"{hour}:00 AM";
        if (hour == 12) return "12:00 PM";
        return $"{hour - 12}:00 PM";
    }

    /// <summary>
    /// Converts a TimeSpan to grid column index
    /// </summary>
    /// <param name="time">Reservation time</param>
    /// <param name="startHour">Grid start hour (default 10)</param>
    /// <returns>Column index (0-based)</returns>
    public static int TimeToColumnIndex(TimeSpan time, int startHour = DefaultStartHour)
    {
        var hour = time.Hours;
        var columnIndex = hour - startHour;
        
        // If time has minutes, we might want to adjust positioning
        // For now, we'll place it at the hour column
        return Math.Max(0, columnIndex);
    }

    /// <summary>
    /// Calculates column span based on duration
    /// Defaults to 2 hours if not specified
    /// </summary>
    /// <param name="durationHours">Duration in hours</param>
    /// <returns>Number of columns to span</returns>
    public static int DurationToColumnSpan(double durationHours = DefaultReservationDurationHours)
    {
        return Math.Max(1, (int)Math.Ceiling(durationHours));
    }

    /// <summary>
    /// Checks if two reservations overlap in time
    /// </summary>
    /// <param name="r1">First reservation</param>
    /// <param name="r2">Second reservation</param>
    /// <param name="durationHours">Duration in hours (default 2)</param>
    /// <returns>True if reservations overlap</returns>
    public static bool IsTimeSlotOverlapping(ReservationDto r1, ReservationDto r2, double durationHours = DefaultReservationDurationHours)
    {
        if (r1.TableId != r2.TableId) return false;
        if (r1.ReservationDate.Date != r2.ReservationDate.Date) return false;

        var r1Start = r1.ReservationTime;
        var r1End = r1Start.Add(TimeSpan.FromHours(durationHours));
        var r2Start = r2.ReservationTime;
        var r2End = r2Start.Add(TimeSpan.FromHours(durationHours));

        // Check for overlap
        return r1Start < r2End && r2Start < r1End;
    }

    /// <summary>
    /// Gets the status color based on reservation status
    /// </summary>
    /// <param name="status">Reservation status</param>
    /// <returns>Color brush for the status</returns>
    public static Brush GetStatusColor(string status)
    {
        return status?.ToLower() switch
        {
            "confirmed" => new SolidColorBrush(Color.FromRgb(255, 215, 0)),      // Yellow
            "checked_in" => new SolidColorBrush(Color.FromRgb(255, 215, 0)),     // Yellow
            "waiting" => new SolidColorBrush(Color.FromRgb(204, 204, 204)),      // Grey
            "cancelled" => new SolidColorBrush(Color.FromRgb(255, 107, 107)),    // Red
            "completed" => new SolidColorBrush(Color.FromRgb(78, 205, 196)),     // Green/Teal
            _ => Brushes.LightGray
        };
    }

    /// <summary>
    /// Gets the status color hex code
    /// </summary>
    /// <param name="status">Reservation status</param>
    /// <returns>Hex color code</returns>
    public static string GetStatusColorHex(string status)
    {
        return status?.ToLower() switch
        {
            "confirmed" => "#FFD700",      // Yellow
            "checked_in" => "#FFD700",     // Yellow
            "waiting" => "#CCCCCC",        // Grey
            "cancelled" => "#FF6B6B",      // Red
            "completed" => "#4ECDC4",      // Green/Teal
            _ => "#D3D3D3"                 // Light Gray
        };
    }

    /// <summary>
    /// Converts a reservation to a grid item with position calculations
    /// </summary>
    /// <param name="reservation">Reservation DTO</param>
    /// <param name="startHour">Grid start hour</param>
    /// <param name="durationHours">Reservation duration in hours</param>
    /// <returns>Reservation grid item</returns>
    public static ReservationGridItem ToGridItem(ReservationDto reservation, int startHour = DefaultStartHour, double durationHours = DefaultReservationDurationHours)
    {
        var columnIndex = TimeToColumnIndex(reservation.ReservationTime, startHour);
        var columnSpan = DurationToColumnSpan(durationHours);

        return new ReservationGridItem
        {
            Reservation = reservation,
            TableId = reservation.TableId,
            TableNumber = reservation.TableNumber ?? reservation.TableId.ToString(),
            StartColumn = columnIndex,
            ColumnSpan = columnSpan,
            StatusColor = GetStatusColor(reservation.Status),
            StatusColorHex = GetStatusColorHex(reservation.Status)
        };
    }

    /// <summary>
    /// Checks if a time slot is available for a table
    /// </summary>
    /// <param name="existingReservations">Existing reservations for the table</param>
    /// <param name="newReservationTime">New reservation time</param>
    /// <param name="newReservationDate">New reservation date</param>
    /// <param name="tableId">Table ID</param>
    /// <param name="excludeReservationId">Reservation ID to exclude (for editing)</param>
    /// <param name="durationHours">Duration in hours</param>
    /// <returns>True if slot is available</returns>
    public static bool IsTimeSlotAvailable(
        IEnumerable<ReservationDto> existingReservations,
        TimeSpan newReservationTime,
        DateTime newReservationDate,
        int tableId,
        int? excludeReservationId = null,
        double durationHours = DefaultReservationDurationHours)
    {
        var newReservation = new ReservationDto
        {
            TableId = tableId,
            ReservationTime = newReservationTime,
            ReservationDate = newReservationDate
        };

        foreach (var existing in existingReservations)
        {
            if (existing.Id == excludeReservationId) continue;
            if (existing.Status == "cancelled") continue;
            if (existing.TableId != tableId) continue;

            if (IsTimeSlotOverlapping(newReservation, existing, durationHours))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets suggested available time slots for a table on a specific date
    /// </summary>
    /// <param name="existingReservations">Existing reservations</param>
    /// <param name="date">Date to check</param>
    /// <param name="tableId">Table ID</param>
    /// <param name="startHour">Start hour</param>
    /// <param name="endHour">End hour</param>
    /// <param name="durationHours">Duration</param>
    /// <returns>List of available time slots</returns>
    public static List<TimeSpan> GetAvailableTimeSlots(
        IEnumerable<ReservationDto> existingReservations,
        DateTime date,
        int tableId,
        int startHour = DefaultStartHour,
        int endHour = DefaultEndHour,
        double durationHours = DefaultReservationDurationHours)
    {
        var availableSlots = new List<TimeSpan>();

        for (int hour = startHour; hour < endHour; hour++)
        {
            var timeSlot = new TimeSpan(hour, 0, 0);
            
            if (IsTimeSlotAvailable(existingReservations, timeSlot, date, tableId, null, durationHours))
            {
                availableSlots.Add(timeSlot);
            }
        }

        return availableSlots;
    }
}
