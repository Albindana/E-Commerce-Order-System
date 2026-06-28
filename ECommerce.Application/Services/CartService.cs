using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Mappings;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _uow;
    private readonly IECommerceMapper _mapper;

    public CartService(IUnitOfWork uow, IECommerceMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<CartDto> GetCartAsync(string userId)
    {
        var cart = await _uow.Carts.GetByUserIdWithItemsAsync(userId);
        if (cart == null)
            return new CartDto();

        return _mapper.CartToDto(cart);
    }

    public async Task<CartDto> AddItemAsync(string userId, AddCartItemDto dto)
    {
        var product = await _uow.Products.GetByIdAsync(dto.ProductId)
            ?? throw new NotFoundException($"Product {dto.ProductId} not found.");

        if (product.StockQuantity < dto.Quantity)
            throw new ConflictException("Insufficient stock.");

        var cart = await _uow.Carts.GetByUserIdWithItemsAsync(userId);
        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            await _uow.Carts.AddAsync(cart);
        }

        var existing = cart.CartItems.FirstOrDefault(i => i.ProductId == dto.ProductId);
        if (existing != null)
        {
            existing.Quantity += dto.Quantity;
        }
        else
        {
            cart.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = product.Price,
                Product = product
            });
        }

        await _uow.SaveChangesAsync();
        return _mapper.CartToDto(cart);
    }

    public async Task<CartDto> UpdateItemAsync(string userId, Guid itemId, UpdateCartItemDto dto)
    {
        var cart = await _uow.Carts.GetByUserIdWithItemsAsync(userId)
            ?? throw new NotFoundException("Cart not found.");

        var item = cart.CartItems.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException("Cart item not found.");

        if (dto.Quantity <= 0)
            cart.CartItems.Remove(item);
        else
            item.Quantity = dto.Quantity;

        await _uow.SaveChangesAsync();
        return _mapper.CartToDto(cart);
    }

    public async Task RemoveItemAsync(string userId, Guid itemId)
    {
        var cart = await _uow.Carts.GetByUserIdWithItemsAsync(userId)
            ?? throw new NotFoundException("Cart not found.");

        var item = cart.CartItems.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException("Cart item not found.");

        cart.CartItems.Remove(item);
        await _uow.SaveChangesAsync();
    }

    public async Task ClearCartAsync(string userId)
    {
        var cart = await _uow.Carts.GetByUserIdWithItemsAsync(userId);
        if (cart == null) return;

        cart.CartItems.Clear();
        await _uow.SaveChangesAsync();
    }
}
