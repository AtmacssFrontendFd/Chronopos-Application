using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for OrderItem entity operations
/// </summary>
public interface IOrderItemRepository : IRepository<OrderItem>
{
    /// <summary>
    /// Gets all items for a specific order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Collection of order items</returns>
    Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(int orderId);

    /// <summary>
    /// Gets all items for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of order items</returns>
    Task<IEnumerable<OrderItem>> GetItemsByProductIdAsync(int productId);

    /// <summary>
    /// Gets all items by status
    /// </summary>
    /// <param name="status">Order item status</param>
    /// <returns>Collection of order items</returns>
    Task<IEnumerable<OrderItem>> GetItemsByStatusAsync(string status);

    /// <summary>
    /// Gets order item with related data (product, order)
    /// </summary>
    /// <param name="id">Order item ID</param>
    /// <returns>Order item with navigation properties</returns>
    Task<OrderItem?> GetItemWithDetailsAsync(int id);

    /// <summary>
    /// Gets all order items with related data
    /// </summary>
    /// <returns>Collection of order items with navigation properties</returns>
    Task<IEnumerable<OrderItem>> GetAllItemsWithDetailsAsync();

    /// <summary>
    /// Deletes all items for a specific order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteItemsByOrderIdAsync(int orderId);
}
