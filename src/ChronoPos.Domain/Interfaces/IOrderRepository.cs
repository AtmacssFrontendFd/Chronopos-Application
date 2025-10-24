using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for Order entity operations
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Gets all orders for a specific table
    /// </summary>
    /// <param name="tableId">Table ID</param>
    /// <returns>Collection of orders</returns>
    Task<IEnumerable<Order>> GetOrdersByTableIdAsync(int tableId);

    /// <summary>
    /// Gets all orders for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Collection of orders</returns>
    Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);

    /// <summary>
    /// Gets all orders for a specific reservation
    /// </summary>
    /// <param name="reservationId">Reservation ID</param>
    /// <returns>Collection of orders</returns>
    Task<IEnumerable<Order>> GetOrdersByReservationIdAsync(int reservationId);

    /// <summary>
    /// Gets all orders by status
    /// </summary>
    /// <param name="status">Order status</param>
    /// <returns>Collection of orders</returns>
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);

    /// <summary>
    /// Gets orders within a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of orders</returns>
    Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets order with all related data (items, customer, table, etc.)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order with navigation properties</returns>
    Task<Order?> GetOrderWithDetailsAsync(int id);

    /// <summary>
    /// Gets all orders with related data
    /// </summary>
    /// <returns>Collection of orders with navigation properties</returns>
    Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync();

    /// <summary>
    /// Gets pending orders
    /// </summary>
    /// <returns>Collection of pending orders</returns>
    Task<IEnumerable<Order>> GetPendingOrdersAsync();

    /// <summary>
    /// Gets active orders (pending, in_progress, served)
    /// </summary>
    /// <returns>Collection of active orders</returns>
    Task<IEnumerable<Order>> GetActiveOrdersAsync();
}
