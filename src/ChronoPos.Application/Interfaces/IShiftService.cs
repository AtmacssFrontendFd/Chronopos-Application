using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Shift operations
/// </summary>
public interface IShiftService
{
    /// <summary>
    /// Gets all shifts
    /// </summary>
    Task<IEnumerable<ShiftDto>> GetAllAsync();
    
    /// <summary>
    /// Gets shift by ID
    /// </summary>
    Task<ShiftDto?> GetByIdAsync(int shiftId);
    
    /// <summary>
    /// Gets shifts by user ID
    /// </summary>
    Task<IEnumerable<ShiftDto>> GetByUserIdAsync(int userId);
    
    /// <summary>
    /// Gets shifts by status
    /// </summary>
    Task<IEnumerable<ShiftDto>> GetByStatusAsync(string status);
    
    /// <summary>
    /// Gets shifts by date range
    /// </summary>
    Task<IEnumerable<ShiftDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets the current open/active shift for a user
    /// </summary>
    Task<ShiftDto?> GetActiveShiftForUserAsync(int userId);
    
    /// <summary>
    /// Creates/Opens a new shift
    /// </summary>
    Task<ShiftDto> OpenShiftAsync(CreateShiftDto createShiftDto);
    
    /// <summary>
    /// Updates an existing shift
    /// </summary>
    Task<ShiftDto> UpdateAsync(int shiftId, UpdateShiftDto updateShiftDto);
    
    /// <summary>
    /// Closes a shift
    /// </summary>
    Task<ShiftDto> CloseShiftAsync(int shiftId, CloseShiftDto closeShiftDto);
    
    /// <summary>
    /// Deletes a shift
    /// </summary>
    Task<bool> DeleteAsync(int shiftId);
    
    /// <summary>
    /// Gets shift summary with transaction statistics
    /// </summary>
    Task<ShiftSummaryDto> GetShiftSummaryAsync(int shiftId);
}
