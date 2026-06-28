namespace ECommerce.Application.DTOs.Cart;

public class AddCartItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
