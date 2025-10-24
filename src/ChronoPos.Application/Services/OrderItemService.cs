using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for OrderItem operations
/// </summary>
public class OrderItemService : IOrderItemService
{
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderItemService(
        IOrderItemRepository orderItemRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all order items
    /// </summary>
    /// <returns>Collection of order item DTOs</returns>
    public async Task<IEnumerable<OrderItemDto>> GetAllAsync()
    {
        var orderItems = await _orderItemRepository.GetAllAsync();
        return orderItems.Select(MapToDto);
    }

    /// <summary>
    /// Gets order item by ID
    /// </summary>
    /// <param name="id">Order item ID</param>
    /// <returns>Order item DTO if found</returns>
    public async Task<OrderItemDto?> GetByIdAsync(int id)
    {
        var orderItem = await _orderItemRepository.GetByIdAsync(id);
        return orderItem != null ? MapToDto(orderItem) : null;
    }

    /// <summary>
    /// Gets all items for a specific order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Collection of order item DTOs</returns>
    public async Task<IEnumerable<OrderItemDto>> GetItemsByOrderIdAsync(int orderId)
    {
        var orderItems = await _orderItemRepository.GetItemsByOrderIdAsync(orderId);
        return orderItems.Select(MapToDto);
    }

    /// <summary>
    /// Gets all items for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of order item DTOs</returns>
    public async Task<IEnumerable<OrderItemDto>> GetItemsByProductIdAsync(int productId)
    {
        var orderItems = await _orderItemRepository.GetItemsByProductIdAsync(productId);
        return orderItems.Select(MapToDto);
    }

    /// <summary>
    /// Gets all items by status
    /// </summary>
    /// <param name="status">Order item status</param>
    /// <returns>Collection of order item DTOs</returns>
    public async Task<IEnumerable<OrderItemDto>> GetItemsByStatusAsync(string status)
    {
        var orderItems = await _orderItemRepository.GetItemsByStatusAsync(status);
        return orderItems.Select(MapToDto);
    }

    /// <summary>
    /// Creates a new order item
    /// </summary>
    /// <param name="createOrderItemDto">Order item data</param>
    /// <returns>Created order item DTO</returns>
    public async Task<OrderItemDto> CreateAsync(CreateOrderItemDto createOrderItemDto)
    {
        // Validate product exists if ProductId is provided
        if (createOrderItemDto.ProductId.HasValue)
        {
            var product = await _productRepository.GetByIdAsync(createOrderItemDto.ProductId.Value);
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {createOrderItemDto.ProductId} not found");
            }
        }

        var orderItem = new OrderItem
        {
            OrderId = createOrderItemDto.OrderId,
            ProductId = createOrderItemDto.ProductId,
            MenuItemId = createOrderItemDto.MenuItemId,
            Quantity = createOrderItemDto.Quantity,
            Price = createOrderItemDto.Price,
            Notes = createOrderItemDto.Notes,
            Status = createOrderItemDto.Status?.ToLower() ?? "pending"
        };

        await _orderItemRepository.AddAsync(orderItem);
        await _unitOfWork.SaveChangesAsync();

        // Reload order item with all related data
        var createdOrderItem = await _orderItemRepository.GetByIdAsync(orderItem.Id);
        return MapToDto(createdOrderItem!);
    }

    /// <summary>
    /// Updates an existing order item
    /// </summary>
    /// <param name="id">Order item ID</param>
    /// <param name="updateOrderItemDto">Updated order item data</param>
    /// <returns>Updated order item DTO</returns>
    public async Task<OrderItemDto> UpdateAsync(int id, UpdateOrderItemDto updateOrderItemDto)
    {
        var orderItem = await _orderItemRepository.GetByIdAsync(id);
        if (orderItem == null)
        {
            throw new ArgumentException($"Order item with ID {id} not found");
        }

        // Validate product exists if ProductId is provided
        if (updateOrderItemDto.ProductId.HasValue)
        {
            var product = await _productRepository.GetByIdAsync(updateOrderItemDto.ProductId.Value);
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {updateOrderItemDto.ProductId} not found");
            }
        }

        orderItem.ProductId = updateOrderItemDto.ProductId;
        orderItem.MenuItemId = updateOrderItemDto.MenuItemId;
        orderItem.Quantity = updateOrderItemDto.Quantity;
        orderItem.Price = updateOrderItemDto.Price;
        orderItem.Notes = updateOrderItemDto.Notes;
        orderItem.Status = updateOrderItemDto.Status?.ToLower() ?? orderItem.Status;

        await _orderItemRepository.UpdateAsync(orderItem);
        await _unitOfWork.SaveChangesAsync();

        // Reload order item with all related data
        var updatedOrderItem = await _orderItemRepository.GetByIdAsync(id);
        return MapToDto(updatedOrderItem!);
    }

    /// <summary>
    /// Updates order item status
    /// </summary>
    /// <param name="id">Order item ID</param>
    /// <param name="status">New status</param>
    /// <returns>Updated order item DTO</returns>
    public async Task<OrderItemDto> UpdateStatusAsync(int id, string status)
    {
        var orderItem = await _orderItemRepository.GetByIdAsync(id);
        if (orderItem == null)
        {
            throw new ArgumentException($"Order item with ID {id} not found");
        }

        orderItem.Status = status.ToLower();

        await _orderItemRepository.UpdateAsync(orderItem);
        await _unitOfWork.SaveChangesAsync();

        // Reload order item with all related data
        var updatedOrderItem = await _orderItemRepository.GetByIdAsync(id);
        return MapToDto(updatedOrderItem!);
    }

    /// <summary>
    /// Deletes an order item
    /// </summary>
    /// <param name="id">Order item ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var orderItem = await _orderItemRepository.GetByIdAsync(id);
        if (orderItem == null)
        {
            return false;
        }

        await _orderItemRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Deletes all items for a specific order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteItemsByOrderIdAsync(int orderId)
    {
        var result = await _orderItemRepository.DeleteItemsByOrderIdAsync(orderId);
        if (result)
        {
            await _unitOfWork.SaveChangesAsync();
        }
        return result;
    }

    /// <summary>
    /// Cancels an order item
    /// </summary>
    /// <param name="id">Order item ID</param>
    /// <returns>Updated order item DTO</returns>
    public async Task<OrderItemDto> CancelItemAsync(int id)
    {
        return await UpdateStatusAsync(id, "cancelled");
    }

    /// <summary>
    /// Maps OrderItem entity to OrderItemDto
    /// </summary>
    /// <param name="orderItem">OrderItem entity</param>
    /// <returns>OrderItem DTO</returns>
    private static OrderItemDto MapToDto(OrderItem orderItem)
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
