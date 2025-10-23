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

/// <summary>
/// Represents the type of discount (percentage or fixed amount)
/// </summary>
public enum DiscountType
{
    Percentage = 1,
    Fixed = 2
}

/// <summary>
/// Represents what the discount is applicable on
/// </summary>
public enum DiscountApplicableOn
{
    Product = 1,
    Category = 2,
    Customer = 3,
    Order = 4
}
