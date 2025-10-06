namespace ChronoPos.Application.DTOs;

/// <summary>
/// Support DTOs for stock adjustment operations
/// </summary>
public class StockAdjustmentSupportDto
{
    /// <summary>
    /// DTO for stock adjustment reasons
    /// </summary>
    public class ReasonDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for store locations
    /// </summary>
    public class LocationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}

/// <summary>
/// DTO for paginated results
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
