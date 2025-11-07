namespace ChronoPos.Domain.Enums;

/// <summary>
/// Reference Type - Which document/source triggered movement
/// </summary>
public enum StockReferenceType
{
    /// <summary>
    /// Goods Received Note
    /// </summary>
    GRN = 1,

    /// <summary>
    /// Sales Transaction
    /// </summary>
    Sale = 2,

    /// <summary>
    /// Stock Adjustment Entry
    /// </summary>
    Adjustment = 3,

    /// <summary>
    /// Stock Transfer Record
    /// </summary>
    Transfer = 4,

    /// <summary>
    /// Goods Return to Supplier
    /// </summary>
    Return = 5,

    /// <summary>
    /// Goods Replacement Entry
    /// </summary>
    Replace = 6,

    /// <summary>
    /// Opening Stock Entry
    /// </summary>
    Opening = 7,

    /// <summary>
    /// Closing Stock Entry
    /// </summary>
    Closing = 8,

    /// <summary>
    /// Manual Entry or Correction
    /// </summary>
    Manual = 9,

    /// <summary>
    /// Refund Transaction (Customer returning products)
    /// </summary>
    Refund = 10,

    /// <summary>
    /// Exchange Transaction (Customer exchanging products)
    /// </summary>
    Exchange = 11
}
