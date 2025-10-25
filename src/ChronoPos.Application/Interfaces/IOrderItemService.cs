using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for OrderItem operations
/// </summary>
public interface IOrderItemService
{
    /// <summary>
    /// Gets all order items
    /// </summary>
    /// <returns>Collection of order item DTOs</returns>
    Task<IEnumerable<OrderItemDto>> GetAllAsync();

    /// <summary>
    /// Gets order item by ID
    /// </summary>
    /// <param name="id">Order item ID</param>
    /// <returns>Order item DTO if found</returns>
    Task<OrderItemDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all items for a specific order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Collection of order item DTOs</returns>
    Task<IEnumerable<OrderItemDto>> GetItemsByOrderIdAsync(int orderId);

    /// <summary>
    /// Gets all items for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of order item DTOs</returns>
    Task<IEnumerable<OrderItemDto>> GetItemsByProductIdAsync(int productId);

    /// <summary>
    /// Gets all items by status
    /// </summary>
    /// <param name="status">Order item status</param>
    /// <returns>Collection of order item DTOs</returns>
    Task<IEnumerable<OrderItemDto>> GetItemsByStatusAsync(string status);

    /// <summary>
    /// Creates a new order item
    /// </summary>
    /// <param name="createOrderItemDto">Order item data</param>
    /// <returns>Created order item DTO</returns>
    Task<OrderItemDto> CreateAsync(CreateOrderItemDto createOrderItemDto);

    /// <summary>
    /// Updates an existing order item
    /// </summary>
    /// <param name="id">Order item ID</param>
    /// <param name="updateOrderItemDto">Updated order item data</param>
    /// <returns>Updated order item DTO</returns>
    Task<OrderItemDto> UpdateAsync(int id, UpdateOrderItemDto updateOrderItemDto);

    /// <summary>
    /// Updates order item status
    /// </summary>
    /// <param name="id">Order item ID</param>
    /// <param name="status">New status</param>
    /// <returns>Updated order item DTO</returns>
    Task<OrderItemDto> UpdateStatusAsync(int id, string status);

    /// <summary>
    /// Deletes an order item
    /// </summary>
    /// <param name="id">Order item ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Deletes all items for a specific order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteItemsByOrderIdAsync(int orderId);

    /// <summary>
    /// Cancels an order item
    /// </summary>
    /// <param name="id">Order item ID</param>
    /// <returns>Updated order item DTO</returns>
    Task<OrderItemDto> CancelItemAsync(int id);
}
