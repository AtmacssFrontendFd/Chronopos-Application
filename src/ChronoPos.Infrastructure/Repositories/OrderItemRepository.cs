using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for OrderItem entity operations
/// </summary>
public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all items for a specific order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Collection of order items</returns>
    public async Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(int orderId)
    {
        return await _context.Set<OrderItem>()
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all items for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of order items</returns>
    public async Task<IEnumerable<OrderItem>> GetItemsByProductIdAsync(int productId)
    {
        return await _context.Set<OrderItem>()
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
            .Where(oi => oi.ProductId == productId)
            .OrderByDescending(oi => oi.Order!.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all items by status
    /// </summary>
    /// <param name="status">Order item status</param>
    /// <returns>Collection of order items</returns>
    public async Task<IEnumerable<OrderItem>> GetItemsByStatusAsync(string status)
    {
        return await _context.Set<OrderItem>()
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
            .Where(oi => oi.Status.ToLower() == status.ToLower())
            .OrderByDescending(oi => oi.Order!.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets order item with related data (product, order)
    /// </summary>
    /// <param name="id">Order item ID</param>
    /// <returns>Order item with navigation properties</returns>
    public async Task<OrderItem?> GetItemWithDetailsAsync(int id)
    {
        return await _context.Set<OrderItem>()
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
                .ThenInclude(o => o!.Customer)
            .Include(oi => oi.Order)
                .ThenInclude(o => o!.Table)
            .FirstOrDefaultAsync(oi => oi.Id == id);
    }

    /// <summary>
    /// Gets all order items with related data
    /// </summary>
    /// <returns>Collection of order items with navigation properties</returns>
    public async Task<IEnumerable<OrderItem>> GetAllItemsWithDetailsAsync()
    {
        return await _context.Set<OrderItem>()
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
                .ThenInclude(o => o!.Customer)
            .Include(oi => oi.Order)
                .ThenInclude(o => o!.Table)
            .OrderByDescending(oi => oi.Order!.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Deletes all items for a specific order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteItemsByOrderIdAsync(int orderId)
    {
        var items = await _context.Set<OrderItem>()
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();

        if (!items.Any())
        {
            return false;
        }

        _context.Set<OrderItem>().RemoveRange(items);
        return true;
    }

    /// <summary>
    /// Gets all order items including related data
    /// </summary>
    /// <returns>Collection of order items with navigation properties</returns>
    public override async Task<IEnumerable<OrderItem>> GetAllAsync()
    {
        return await GetAllItemsWithDetailsAsync();
    }

    /// <summary>
    /// Gets order item by ID including related data
    /// </summary>
    /// <param name="id">Order item ID</param>
    /// <returns>Order item with navigation properties</returns>
    public override async Task<OrderItem?> GetByIdAsync(int id)
    {
        return await GetItemWithDetailsAsync(id);
    }
}
