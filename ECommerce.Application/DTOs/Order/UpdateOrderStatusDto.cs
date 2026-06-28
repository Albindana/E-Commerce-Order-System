using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Order;

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}
