using AutoMapper;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public OrderService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<OrderDto> CheckoutAsync(string userId, CheckoutDto dto)
    {
        var cart = await _uow.Carts.GetByUserIdWithItemsAsync(userId)
            ?? throw new BadRequestException("Cart is empty.");

        if (!cart.CartItems.Any())
            throw new BadRequestException("Cart is empty.");

        foreach (var item in cart.CartItems)
        {
            var product = await _uow.Products.GetByIdAsync(item.ProductId)
                ?? throw new NotFoundException($"Product {item.ProductId} not found.");

            if (product.StockQuantity < item.Quantity)
                throw new ConflictException($"Insufficient stock for '{product.Name}'.");
        }

        var sequence = await _uow.Orders.GetNextOrderSequenceAsync();
        var orderNumber = $"ORD-{DateTime.UtcNow.Year}-{sequence:D5}";

        var order = new Order
        {
            UserId = userId,
            OrderNumber = orderNumber,
            ShippingAddress = _mapper.Map<ShippingAddress>(dto.ShippingAddress)
        };

        foreach (var item in cart.CartItems)
        {
            var product = await _uow.Products.GetByIdAsync(item.ProductId)!;

            order.OrderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = product!.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });

            product.StockQuantity -= item.Quantity;
            _uow.Products.Update(product);
        }

        order.TotalAmount = order.OrderItems.Sum(i => i.UnitPrice * i.Quantity);

        await _uow.Orders.AddAsync(order);

        cart.CartItems.Clear();
        await _uow.SaveChangesAsync();

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<PagedResult<OrderDto>> GetUserOrdersAsync(string userId, int page, int pageSize)
    {
        var (items, total) = await _uow.Orders.GetByUserIdPagedAsync(userId, page, pageSize);
        return new PagedResult<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<OrderDto> GetByIdAsync(string userId, Guid id)
    {
        var order = await _uow.Orders.GetByIdWithItemsAsync(id)
            ?? throw new NotFoundException($"Order {id} not found.");

        if (order.UserId != userId)
            throw new ForbiddenException("Access denied.");

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> CancelOrderAsync(string userId, Guid id)
    {
        var order = await _uow.Orders.GetByIdWithItemsAsync(id)
            ?? throw new NotFoundException($"Order {id} not found.");

        if (order.UserId != userId)
            throw new ForbiddenException("Access denied.");

        if (order.Status != OrderStatus.Pending)
            throw new BadRequestException("Only pending orders can be cancelled.");

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync();

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<PagedResult<OrderDto>> GetAllOrdersAsync(int page, int pageSize)
    {
        var (items, total) = await _uow.Orders.GetAllPagedAsync(page, pageSize);
        return new PagedResult<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(items),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto)
    {
        var order = await _uow.Orders.GetByIdWithItemsAsync(id)
            ?? throw new NotFoundException($"Order {id} not found.");

        order.Status = dto.Status;
        order.UpdatedAt = DateTime.UtcNow;
        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync();

        return _mapper.Map<OrderDto>(order);
    }
}
