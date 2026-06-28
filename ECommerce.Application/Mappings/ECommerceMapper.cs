using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.DTOs.Product;
using ECommerce.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace ECommerce.Application.Mappings;

[Mapper]
public partial class ECommerceMapper : IECommerceMapper
{
    [MapProperty(nameof(Product.Category) + "." + nameof(Category.Name), nameof(ProductDto.CategoryName))]
    public partial ProductDto ProductToDto(Product product);

    public List<ProductDto> ProductsToDto(IEnumerable<Product> products) =>
        products.Select(ProductToDto).ToList();

    [MapperIgnoreTarget(nameof(Product.Id))]
    [MapperIgnoreTarget(nameof(Product.CreatedAt))]
    [MapperIgnoreTarget(nameof(Product.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Product.IsActive))]
    [MapperIgnoreTarget(nameof(Product.Category))]
    public partial Product CreateDtoToProduct(CreateProductDto dto);

    [MapperIgnoreTarget(nameof(Product.Id))]
    [MapperIgnoreTarget(nameof(Product.CreatedAt))]
    [MapperIgnoreTarget(nameof(Product.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Product.IsActive))]
    [MapperIgnoreTarget(nameof(Product.Category))]
    public partial void UpdateDtoToProduct(UpdateProductDto dto, Product product);

    public partial CategoryDto CategoryToDto(Category category);

    public List<CategoryDto> CategoriesToDto(IEnumerable<Category> categories) =>
        categories.Select(CategoryToDto).ToList();

    public partial CategoryWithProductsDto CategoryToWithProductsDto(Category category);

    [MapperIgnoreTarget(nameof(Category.Id))]
    [MapperIgnoreTarget(nameof(Category.Products))]
    public partial Category CreateDtoToCategory(CreateCategoryDto dto);

    [MapperIgnoreTarget(nameof(Category.Id))]
    [MapperIgnoreTarget(nameof(Category.Products))]
    public partial void UpdateDtoToCategory(CreateCategoryDto dto, Category category);

    [MapProperty(nameof(Cart.CartItems), nameof(CartDto.Items))]
    public partial CartDto CartToDto(Cart cart);

    [MapProperty(nameof(CartItem.Product) + "." + nameof(Product.Name), nameof(CartItemDto.ProductName))]
    [MapProperty(nameof(CartItem.Product) + "." + nameof(Product.ImageUrl), nameof(CartItemDto.ProductImageUrl))]
    public partial CartItemDto CartItemToDto(CartItem item);

    [MapProperty(nameof(Order.OrderItems), nameof(OrderDto.Items))]
    public partial OrderDto OrderToDto(Order order);

    public List<OrderDto> OrdersToDto(IEnumerable<Order> orders) =>
        orders.Select(OrderToDto).ToList();

    public partial OrderItemDto OrderItemToDto(OrderItem item);

    public partial ShippingAddress ShippingAddressDtoToEntity(ShippingAddressDto dto);
}
