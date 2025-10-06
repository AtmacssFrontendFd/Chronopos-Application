namespace ChronoPos.Domain.Enums
{
    /// <summary>
    /// Represents the mode of stock adjustment
    /// </summary>
    public enum StockAdjustmentMode
    {
        /// <summary>
        /// Adjust stock directly on the product
        /// </summary>
        Product = 0,
        
        /// <summary>
        /// Adjust stock on a product unit with conversion factor calculation
        /// </summary>
        ProductUnit = 1
    }
}