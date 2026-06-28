using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Order;

namespace ECommerce.Application.Interfaces.Services;

public interface IOrderService
{
    Task<OrderDto> CheckoutAsync(string userId, CheckoutDto dto);
    Task<PagedResult<OrderDto>> GetUserOrdersAsync(string userId, int page, int pageSize);
    Task<OrderDto> GetByIdAsync(string userId, Guid id);
    Task<OrderDto> CancelOrderAsync(string userId, Guid id);
    Task<PagedResult<OrderDto>> GetAllOrdersAsync(int page, int pageSize);
    Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto);
}
