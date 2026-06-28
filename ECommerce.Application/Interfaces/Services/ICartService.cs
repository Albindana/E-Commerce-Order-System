using ECommerce.Application.DTOs.Cart;

namespace ECommerce.Application.Interfaces.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync(string userId);
    Task<CartDto> AddItemAsync(string userId, AddCartItemDto dto);
    Task<CartDto> UpdateItemAsync(string userId, Guid itemId, UpdateCartItemDto dto);
    Task RemoveItemAsync(string userId, Guid itemId);
    Task ClearCartAsync(string userId);
}
