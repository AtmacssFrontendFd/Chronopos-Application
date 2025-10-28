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
    Draft = 1,              // Sale in progress / saved only
    Billed = 2,             // Bill printed, awaiting payment
    Settled = 3,            // Fully paid and closed
    Hold = 4,               // Temporarily parked (table order, paused cart)
    Cancelled = 5,          // Order cancelled before settlement
    PendingPayment = 6,     // Customer took goods/service but will pay later (credit sale)
    PartialPayment = 7      // Some payment done, balance pending
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
    Shop = 5 // Store/Shop-level discount
}


