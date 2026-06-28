using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.DTOs.Product;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public interface IECommerceMapper
{
    ProductDto ProductToDto(Product product);
    List<ProductDto> ProductsToDto(IEnumerable<Product> products);

    Product CreateDtoToProduct(CreateProductDto dto);
    void UpdateDtoToProduct(UpdateProductDto dto, Product product);

    CategoryDto CategoryToDto(Category category);
    List<CategoryDto> CategoriesToDto(IEnumerable<Category> categories);
    CategoryWithProductsDto CategoryToWithProductsDto(Category category);

    Category CreateDtoToCategory(CreateCategoryDto dto);
    void UpdateDtoToCategory(CreateCategoryDto dto, Category category);

    CartDto CartToDto(Cart cart);
    CartItemDto CartItemToDto(CartItem item);

    OrderDto OrderToDto(Order order);
    List<OrderDto> OrdersToDto(IEnumerable<Order> orders);
    OrderItemDto OrderItemToDto(OrderItem item);
    ShippingAddress ShippingAddressDtoToEntity(ShippingAddressDto dto);
}
