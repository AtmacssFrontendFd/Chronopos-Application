namespace ChronoPos.Application.DTOs
{
    /// <summary>
    /// DTO for stock adjustment reason
    /// </summary>
    public class StockAdjustmentReasonDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
