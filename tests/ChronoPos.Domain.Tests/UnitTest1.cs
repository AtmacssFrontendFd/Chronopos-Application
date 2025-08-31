using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Enums;

namespace ChronoPos.Domain.Tests;

/// <summary>
/// Unit tests for Product entity
/// </summary>
public class ProductTests
{
    [Fact]
    public void Product_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var product = new Product
        {
            Name = "Test Product",
            SKU = "TEST001",
            Price = 19.99m,
            CategoryId = 1
        };

        // Assert
        Assert.Equal("Test Product", product.Name);
        Assert.Equal("TEST001", product.SKU);
        Assert.Equal(19.99m, product.Price);
        Assert.Equal(1, product.CategoryId);
        Assert.True(product.IsActive);
        Assert.True(product.CreatedAt <= DateTime.UtcNow);
        Assert.True(product.UpdatedAt <= DateTime.UtcNow);
    }
}

/// <summary>
/// Unit tests for SaleItem entity
/// </summary>
public class SaleItemTests
{
    [Fact]
    public void SaleItem_TotalAmount_ShouldCalculateCorrectly()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = 3,
            UnitPrice = 10.00m,
            DiscountAmount = 5.00m
        };

        // Act
        var totalAmount = saleItem.TotalAmount;

        // Assert
        Assert.Equal(25.00m, totalAmount); // (3 * 10.00) - 5.00 = 25.00
    }

    [Fact]
    public void SaleItem_TotalAmount_WithoutDiscount_ShouldCalculateCorrectly()
    {
        // Arrange
        var saleItem = new SaleItem
        {
            Quantity = 2,
            UnitPrice = 15.50m,
            DiscountAmount = 0m
        };

        // Act
        var totalAmount = saleItem.TotalAmount;

        // Assert
        Assert.Equal(31.00m, totalAmount); // 2 * 15.50 = 31.00
    }
}

/// <summary>
/// Unit tests for Customer entity
/// </summary>
public class CustomerTests
{
    [Fact]
    public void Customer_FullName_ShouldReturnConcatenatedName()
    {
        // Arrange
        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var fullName = customer.FullName;

        // Assert
        Assert.Equal("John Doe", fullName);
    }
}
