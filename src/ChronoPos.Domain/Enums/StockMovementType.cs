namespace ChronoPos.Domain.Enums;

/// <summary>
/// Movement Type - What action caused stock change
/// </summary>
public enum StockMovementType
{
    /// <summary>
    /// Inward from Supplier (GRN)
    /// </summary>
    Purchase = 1,

    /// <summary>
    /// Outward to Customer
    /// </summary>
    Sale = 2,

    /// <summary>
    /// Manual Stock Correction
    /// </summary>
    Adjustment = 3,

    /// <summary>
    /// Received from another branch/location
    /// </summary>
    TransferIn = 4,

    /// <summary>
    /// Sent to another branch/location
    /// </summary>
    TransferOut = 5,

    /// <summary>
    /// Goods Returned to Supplier
    /// </summary>
    Return = 6,

    /// <summary>
    /// Goods Replaced
    /// </summary>
    Replace = 7,

    /// <summary>
    /// Damaged / Expired / Lost
    /// </summary>
    Waste = 8,

    /// <summary>
    /// Initial Stock Balance
    /// </summary>
    Opening = 9,

    /// <summary>
    /// Closing Balance Snapshot
    /// </summary>
    Closing = 10
}
