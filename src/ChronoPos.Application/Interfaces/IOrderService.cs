using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Order operations
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Gets all orders
    /// </summary>
    /// <returns>Collection of order DTOs</returns>
    Task<IEnumerable<OrderDto>> GetAllAsync();

    /// <summary>
    /// Gets order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order DTO if found</returns>
    Task<OrderDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all orders for a specific table
    /// </summary>
    /// <param name="tableId">Table ID</param>
    /// <returns>Collection of order DTOs</returns>
    Task<IEnumerable<OrderDto>> GetOrdersByTableIdAsync(int tableId);

    /// <summary>
    /// Gets all orders for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Collection of order DTOs</returns>
    Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId);

    /// <summary>
    /// Gets all orders for a specific reservation
    /// </summary>
    /// <param name="reservationId">Reservation ID</param>
    /// <returns>Collection of order DTOs</returns>
    Task<IEnumerable<OrderDto>> GetOrdersByReservationIdAsync(int reservationId);

    /// <summary>
    /// Gets all orders by status
    /// </summary>
    /// <param name="status">Order status</param>
    /// <returns>Collection of order DTOs</returns>
    Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(string status);

    /// <summary>
    /// Gets orders within a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of order DTOs</returns>
    Task<IEnumerable<OrderDto>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets pending orders
    /// </summary>
    /// <returns>Collection of pending order DTOs</returns>
    Task<IEnumerable<OrderDto>> GetPendingOrdersAsync();

    /// <summary>
    /// Gets active orders (pending, in_progress, served)
    /// </summary>
    /// <returns>Collection of active order DTOs</returns>
    Task<IEnumerable<OrderDto>> GetActiveOrdersAsync();

    /// <summary>
    /// Creates a new order
    /// </summary>
    /// <param name="createOrderDto">Order data</param>
    /// <returns>Created order DTO</returns>
    Task<OrderDto> CreateAsync(CreateOrderDto createOrderDto);

    /// <summary>
    /// Updates an existing order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="updateOrderDto">Updated order data</param>
    /// <returns>Updated order DTO</returns>
    Task<OrderDto> UpdateAsync(int id, UpdateOrderDto updateOrderDto);

    /// <summary>
    /// Updates order status
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="status">New status</param>
    /// <returns>Updated order DTO</returns>
    Task<OrderDto> UpdateStatusAsync(int id, string status);

    /// <summary>
    /// Deletes an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Cancels an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Updated order DTO</returns>
    Task<OrderDto> CancelOrderAsync(int id);

    /// <summary>
    /// Completes an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Updated order DTO</returns>
    Task<OrderDto> CompleteOrderAsync(int id);
}
