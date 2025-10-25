using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for Shift entity operations
/// </summary>
public interface IShiftRepository : IRepository<Shift>
{
    /// <summary>
    /// Gets shifts by user ID
    /// </summary>
    Task<IEnumerable<Shift>> GetByUserIdAsync(int userId);
    
    /// <summary>
    /// Gets shifts by shop location ID
    /// </summary>
    Task<IEnumerable<Shift>> GetByShopLocationIdAsync(int shopLocationId);
    
    /// <summary>
    /// Gets shifts by status
    /// </summary>
    Task<IEnumerable<Shift>> GetByStatusAsync(string status);
    
    /// <summary>
    /// Gets the current open shift for a user
    /// </summary>
    Task<Shift?> GetCurrentOpenShiftAsync(int userId);
    
    /// <summary>
    /// Gets shifts for a specific date range
    /// </summary>
    Task<IEnumerable<Shift>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets shift with all transactions
    /// </summary>
    Task<Shift?> GetWithTransactionsAsync(int shiftId);
    
    /// <summary>
    /// Gets the active shift for a user
    /// </summary>
    Task<Shift?> GetActiveShiftForUserAsync(int userId);
    
    /// <summary>
    /// Updates a shift
    /// </summary>
    void Update(Shift shift);
    
    /// <summary>
    /// Deletes a shift
    /// </summary>
    void Delete(Shift shift);
}
