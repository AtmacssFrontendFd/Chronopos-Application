using ChronoPos.Application.DTOs;
using System.Windows.Media;

namespace ChronoPos.Desktop.Models;

/// <summary>
/// Represents a reservation item positioned in the timeline grid
/// </summary>
public class ReservationGridItem
{
    /// <summary>
    /// The reservation data
    /// </summary>
    public ReservationDto Reservation { get; set; } = null!;

    /// <summary>
    /// The table ID this reservation belongs to
    /// </summary>
    public int TableId { get; set; }

    /// <summary>
    /// The table number for display
    /// </summary>
    public string TableNumber { get; set; } = string.Empty;

    /// <summary>
    /// The grid column index where this reservation starts (based on time)
    /// </summary>
    public int StartColumn { get; set; }

    /// <summary>
    /// The number of columns this reservation spans (duration in hours)
    /// Default is 2 hours
    /// </summary>
    public int ColumnSpan { get; set; } = 2;

    /// <summary>
    /// The status color for visual display
    /// </summary>
    public Brush StatusColor { get; set; } = Brushes.LightGray;

    /// <summary>
    /// The status color hex code
    /// </summary>
    public string StatusColorHex { get; set; } = "#CCCCCC";

    /// <summary>
    /// Customer name for display
    /// </summary>
    public string CustomerName => Reservation?.CustomerName ?? "Unknown";

    /// <summary>
    /// Number of persons with icon
    /// </summary>
    public string PersonsDisplay => $"👥 {Reservation?.NumberOfPersons ?? 0:00}";

    /// <summary>
    /// Start time formatted
    /// </summary>
    public string StartTime => Reservation?.ReservationTime.ToString(@"hh\:mm") ?? "00:00";

    /// <summary>
    /// End time calculated (start time + duration)
    /// </summary>
    public string EndTime
    {
        get
        {
            if (Reservation == null) return "00:00";
            var endTime = Reservation.ReservationTime.Add(TimeSpan.FromHours(ColumnSpan));
            return endTime.ToString(@"hh\:mm");
        }
    }

    /// <summary>
    /// Time range display
    /// </summary>
    public string TimeRange => $"{StartTime} - {EndTime}";

    /// <summary>
    /// Tooltip text with full details
    /// </summary>
    public string TooltipText => $"{CustomerName}\n{TimeRange}\n{PersonsDisplay}\nStatus: {Reservation?.StatusDisplay}";
}

/// <summary>
/// Represents a time slot header in the timeline grid
/// </summary>
public class TimeSlotHeader
{
    /// <summary>
    /// Hour in 24-hour format (0-23)
    /// </summary>
    public int Hour { get; set; }

    /// <summary>
    /// Display time in 12-hour format with AM/PM
    /// </summary>
    public string DisplayTime { get; set; } = string.Empty;

    /// <summary>
    /// Display time in 24-hour format
    /// </summary>
    public string DisplayTime24 => $"{Hour:D2}:00";

    /// <summary>
    /// Column index in the grid
    /// </summary>
    public int ColumnIndex { get; set; }
}

/// <summary>
/// Represents a table row in the timeline grid
/// </summary>
public class TableGridRow
{
    /// <summary>
    /// Table information
    /// </summary>
    public RestaurantTableDto Table { get; set; } = null!;

    /// <summary>
    /// Reservations for this table
    /// </summary>
    public List<ReservationGridItem> Reservations { get; set; } = new();

    /// <summary>
    /// Row index in the grid
    /// </summary>
    public int RowIndex { get; set; }

    /// <summary>
    /// Display name with capacity
    /// </summary>
    public string DisplayName => $"{Table?.TableNumber} ({Table?.Capacity}p)";
}
