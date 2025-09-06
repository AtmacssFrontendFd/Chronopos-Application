namespace ChronoPos.Application.DTOs
{
    /// <summary>
    /// DTO for product stock information used in adjustments
    /// </summary>
    public class ProductStockDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public int UomId { get; set; }
        public string UomName { get; set; } = string.Empty;
        public decimal UnitCost { get; set; }
        public decimal SalePrice { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
