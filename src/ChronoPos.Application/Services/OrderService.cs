using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Order operations
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all orders
    /// </summary>
    /// <returns>Collection of order DTOs</returns>
    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Select(MapToDto);
    }

    /// <summary>
    /// Gets order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order DTO if found</returns>
    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        return order != null ? MapToDto(order) : null;
    }

    /// <summary>
    /// Gets all orders for a specific table
    /// </summary>
    /// <param name="tableId">Table ID</param>
    /// <returns>Collection of order DTOs</returns>
    public async Task<IEnumerable<OrderDto>> GetOrdersByTableIdAsync(int tableId)
    {
        var orders = await _orderRepository.GetOrdersByTableIdAsync(tableId);
        return orders.Select(MapToDto);
    }

    /// <summary>
    /// Gets all orders for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Collection of order DTOs</returns>
    public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId)
    {
        var orders = await _orderRepository.GetOrdersByCustomerIdAsync(customerId);
        return orders.Select(MapToDto);
    }

    /// <summary>
    /// Gets all orders for a specific reservation
    /// </summary>
    /// <param name="reservationId">Reservation ID</param>
    /// <returns>Collection of order DTOs</returns>
    public async Task<IEnumerable<OrderDto>> GetOrdersByReservationIdAsync(int reservationId)
    {
        var orders = await _orderRepository.GetOrdersByReservationIdAsync(reservationId);
        return orders.Select(MapToDto);
    }

    /// <summary>
    /// Gets all orders by status
    /// </summary>
    /// <param name="status">Order status</param>
    /// <returns>Collection of order DTOs</returns>
    public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(string status)
    {
        var orders = await _orderRepository.GetOrdersByStatusAsync(status);
        return orders.Select(MapToDto);
    }

    /// <summary>
    /// Gets orders within a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of order DTOs</returns>
    public async Task<IEnumerable<OrderDto>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var orders = await _orderRepository.GetOrdersByDateRangeAsync(startDate, endDate);
        return orders.Select(MapToDto);
    }

    /// <summary>
    /// Gets pending orders
    /// </summary>
    /// <returns>Collection of pending order DTOs</returns>
    public async Task<IEnumerable<OrderDto>> GetPendingOrdersAsync()
    {
        var orders = await _orderRepository.GetPendingOrdersAsync();
        return orders.Select(MapToDto);
    }

    /// <summary>
    /// Gets active orders (pending, in_progress, served)
    /// </summary>
    /// <returns>Collection of active order DTOs</returns>
    public async Task<IEnumerable<OrderDto>> GetActiveOrdersAsync()
    {
        var orders = await _orderRepository.GetActiveOrdersAsync();
        return orders.Select(MapToDto);
    }

    /// <summary>
    /// Creates a new order
    /// </summary>
    /// <param name="createOrderDto">Order data</param>
    /// <returns>Created order DTO</returns>
    public async Task<OrderDto> CreateAsync(CreateOrderDto createOrderDto)
    {
        var order = new Order
        {
            TableId = createOrderDto.TableId,
            CustomerId = createOrderDto.CustomerId,
            ReservationId = createOrderDto.ReservationId,
            TotalAmount = createOrderDto.TotalAmount,
            Discount = createOrderDto.Discount,
            PaymentTypeId = createOrderDto.PaymentTypeId,
            Status = createOrderDto.Status?.ToLower() ?? "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Add order items if provided
        if (createOrderDto.OrderItems != null && createOrderDto.OrderItems.Any())
        {
            foreach (var itemDto in createOrderDto.OrderItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = itemDto.ProductId,
                    MenuItemId = itemDto.MenuItemId,
                    Quantity = itemDto.Quantity,
                    Price = itemDto.Price,
                    Notes = itemDto.Notes,
                    Status = itemDto.Status?.ToLower() ?? "pending"
                };

                await _orderItemRepository.AddAsync(orderItem);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        // Reload order with all related data
        var createdOrder = await _orderRepository.GetByIdAsync(order.Id);
        return MapToDto(createdOrder!);
    }

    /// <summary>
    /// Updates an existing order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="updateOrderDto">Updated order data</param>
    /// <returns>Updated order DTO</returns>
    public async Task<OrderDto> UpdateAsync(int id, UpdateOrderDto updateOrderDto)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            throw new ArgumentException($"Order with ID {id} not found");
        }

        order.TableId = updateOrderDto.TableId;
        order.CustomerId = updateOrderDto.CustomerId;
        order.ReservationId = updateOrderDto.ReservationId;
        order.TotalAmount = updateOrderDto.TotalAmount;
        order.Discount = updateOrderDto.Discount;
        order.PaymentTypeId = updateOrderDto.PaymentTypeId;
        order.Status = updateOrderDto.Status?.ToLower() ?? order.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Reload order with all related data
        var updatedOrder = await _orderRepository.GetByIdAsync(id);
        return MapToDto(updatedOrder!);
    }

    /// <summary>
    /// Updates order status
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="status">New status</param>
    /// <returns>Updated order DTO</returns>
    public async Task<OrderDto> UpdateStatusAsync(int id, string status)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            throw new ArgumentException($"Order with ID {id} not found");
        }

        order.Status = status.ToLower();
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Reload order with all related data
        var updatedOrder = await _orderRepository.GetByIdAsync(id);
        return MapToDto(updatedOrder!);
    }

    /// <summary>
    /// Deletes an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return false;
        }

        // Delete all order items first
        await _orderItemRepository.DeleteItemsByOrderIdAsync(id);

        // Delete the order
        await _orderRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Cancels an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Updated order DTO</returns>
    public async Task<OrderDto> CancelOrderAsync(int id)
    {
        return await UpdateStatusAsync(id, "cancelled");
    }

    /// <summary>
    /// Completes an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Updated order DTO</returns>
    public async Task<OrderDto> CompleteOrderAsync(int id)
    {
        return await UpdateStatusAsync(id, "completed");
    }

    /// <summary>
    /// Maps Order entity to OrderDto
    /// </summary>
    /// <param name="order">Order entity</param>
    /// <returns>Order DTO</returns>
    private static OrderDto MapToDto(Order order)
    {
        var dto = new OrderDto
        {
            Id = order.Id,
            TableId = order.TableId,
            TableName = order.Table?.TableNumber,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer?.DisplayName,
            ReservationId = order.ReservationId,
            TotalAmount = order.TotalAmount,
            Discount = order.Discount,
            FinalAmount = order.FinalAmount,
            PaymentTypeId = order.PaymentTypeId,
            PaymentTypeName = order.PaymentType?.Name,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderItems = order.OrderItems?.Select(MapOrderItemToDto).ToList() ?? new List<OrderItemDto>()
        };

        return dto;
    }

    /// <summary>
    /// Maps OrderItem entity to OrderItemDto
    /// </summary>
    /// <param name="orderItem">OrderItem entity</param>
    /// <returns>OrderItem DTO</returns>
    private static OrderItemDto MapOrderItemToDto(OrderItem orderItem)
    {
        return new OrderItemDto
        {
            Id = orderItem.Id,
            OrderId = orderItem.OrderId,
            ProductId = orderItem.ProductId,
            ProductName = orderItem.Product?.Name,
            MenuItemId = orderItem.MenuItemId,
            MenuItemName = null, // Set when MenuItem entity is available
            Quantity = orderItem.Quantity,
            Price = orderItem.Price,
            Notes = orderItem.Notes,
            Status = orderItem.Status
        };
    }
}
