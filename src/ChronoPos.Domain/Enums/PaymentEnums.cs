namespace ChronoPos.Domain.Enums;

/// <summary>
/// Represents the payment method used for a sale
/// </summary>
public enum PaymentMethod
{
    Cash = 1,
    CreditCard = 2,
    DebitCard = 3,
    DigitalWallet = 4,
    BankTransfer = 5,
    Check = 6
}

/// <summary>
/// Represents the status of a sale transaction
/// </summary>
public enum SaleStatus
{
    Pending = 1,
    Completed = 2,
    Cancelled = 3,
    Refunded = 4
}
