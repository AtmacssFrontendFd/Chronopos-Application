using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Order entity operations
/// </summary>
public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all orders for a specific table
    /// </summary>
    /// <param name="tableId">Table ID</param>
    /// <returns>Collection of orders</returns>
    public async Task<IEnumerable<Order>> GetOrdersByTableIdAsync(int tableId)
    {
        return await _context.Set<Order>()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Customer)
            .Include(o => o.Table)
            .Include(o => o.PaymentType)
            .Include(o => o.Reservation)
            .Where(o => o.TableId == tableId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all orders for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Collection of orders</returns>
    public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId)
    {
        return await _context.Set<Order>()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Customer)
            .Include(o => o.Table)
            .Include(o => o.PaymentType)
            .Include(o => o.Reservation)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all orders for a specific reservation
    /// </summary>
    /// <param name="reservationId">Reservation ID</param>
    /// <returns>Collection of orders</returns>
    public async Task<IEnumerable<Order>> GetOrdersByReservationIdAsync(int reservationId)
    {
        return await _context.Set<Order>()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Customer)
            .Include(o => o.Table)
            .Include(o => o.PaymentType)
            .Include(o => o.Reservation)
            .Where(o => o.ReservationId == reservationId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all orders by status
    /// </summary>
    /// <param name="status">Order status</param>
    /// <returns>Collection of orders</returns>
    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status)
    {
        return await _context.Set<Order>()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Customer)
            .Include(o => o.Table)
            .Include(o => o.PaymentType)
            .Include(o => o.Reservation)
            .Where(o => o.Status.ToLower() == status.ToLower())
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets orders within a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of orders</returns>
    public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Set<Order>()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Customer)
            .Include(o => o.Table)
            .Include(o => o.PaymentType)
            .Include(o => o.Reservation)
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets order with all related data (items, customer, table, etc.)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order with navigation properties</returns>
    public async Task<Order?> GetOrderWithDetailsAsync(int id)
    {
        return await _context.Set<Order>()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Customer)
            .Include(o => o.Table)
            .Include(o => o.PaymentType)
            .Include(o => o.Reservation)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    /// <summary>
    /// Gets all orders with related data
    /// </summary>
    /// <returns>Collection of orders with navigation properties</returns>
    public async Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync()
    {
        return await _context.Set<Order>()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Customer)
            .Include(o => o.Table)
            .Include(o => o.PaymentType)
            .Include(o => o.Reservation)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets pending orders
    /// </summary>
    /// <returns>Collection of pending orders</returns>
    public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
    {
        return await GetOrdersByStatusAsync("pending");
    }

    /// <summary>
    /// Gets active orders (pending, in_progress, served)
    /// </summary>
    /// <returns>Collection of active orders</returns>
    public async Task<IEnumerable<Order>> GetActiveOrdersAsync()
    {
        var activeStatuses = new[] { "pending", "in_progress", "served" };
        
        return await _context.Set<Order>()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Customer)
            .Include(o => o.Table)
            .Include(o => o.PaymentType)
            .Include(o => o.Reservation)
            .Where(o => activeStatuses.Contains(o.Status.ToLower()))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all orders including related data
    /// </summary>
    /// <returns>Collection of orders with navigation properties</returns>
    public override async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await GetAllOrdersWithDetailsAsync();
    }

    /// <summary>
    /// Gets order by ID including related data
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order with navigation properties</returns>
    public override async Task<Order?> GetByIdAsync(int id)
    {
        return await GetOrderWithDetailsAsync(id);
    }
}
