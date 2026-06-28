using ECommerce.Application.DTOs.Product;

namespace ECommerce.Application.DTOs.Category;

public class CategoryWithProductsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<ProductDto> Products { get; set; } = new();
}
