using AutoMapper;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.DTOs.Product;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty));

        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();

        CreateMap<Category, CategoryDto>();
        CreateMap<Category, CategoryWithProductsDto>()
            .ForMember(d => d.Products, o => o.MapFrom(s => s.Products));
        CreateMap<CreateCategoryDto, Category>();

        CreateMap<Cart, CartDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.CartItems));
        CreateMap<CartItem, CartItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.ProductImageUrl, o => o.MapFrom(s => s.Product != null ? s.Product.ImageUrl : null));

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.OrderItems));
        CreateMap<OrderItem, OrderItemDto>();
        CreateMap<ShippingAddress, ShippingAddressDto>();
        CreateMap<ShippingAddressDto, ShippingAddress>();
    }
}
