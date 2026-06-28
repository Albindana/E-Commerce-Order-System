using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Mappings;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace ECommerce.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly IECommerceMapper _mapper = new ECommerceMapper();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(_uow.Object, _mapper);
    }

    private static CheckoutDto ValidCheckout() => new()
    {
        ShippingAddress = new ShippingAddressDto
        {
            Street = "123 Main St", City = "Springfield",
            Country = "US", ZipCode = "12345"
        }
    };

    [Fact]
    public async Task CheckoutAsync_WhenCartIsNull_ThrowsBadRequestException()
    {
        _uow.Setup(u => u.Carts.GetByUserIdWithItemsAsync("user-1")).ReturnsAsync((Cart?)null);

        var act = () => _sut.CheckoutAsync("user-1", ValidCheckout());

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*empty*");
    }

    [Fact]
    public async Task CheckoutAsync_WhenCartIsEmpty_ThrowsBadRequestException()
    {
        var cart = new Cart { UserId = "user-2", CartItems = new List<CartItem>() };
        _uow.Setup(u => u.Carts.GetByUserIdWithItemsAsync("user-2")).ReturnsAsync(cart);

        var act = () => _sut.CheckoutAsync("user-2", ValidCheckout());

        await act.Should().ThrowAsync<BadRequestException>().WithMessage("*empty*");
    }

    [Fact]
    public async Task CheckoutAsync_WhenStockInsufficient_ThrowsConflictException()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "Widget", StockQuantity = 1 };
        var cartItem = new CartItem { ProductId = product.Id, Quantity = 5, UnitPrice = 10m };
        var cart = new Cart { UserId = "user-3", CartItems = new List<CartItem> { cartItem } };

        _uow.Setup(u => u.Carts.GetByUserIdWithItemsAsync("user-3")).ReturnsAsync(cart);
        _uow.Setup(u => u.Products.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var act = () => _sut.CheckoutAsync("user-3", ValidCheckout());

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CheckoutAsync_WithValidCart_DeductsStockAndClearsCart()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "Widget", StockQuantity = 10 };
        var cartItem = new CartItem { ProductId = product.Id, Quantity = 3, UnitPrice = 19.99m };
        var cart = new Cart { UserId = "user-4", CartItems = new List<CartItem> { cartItem } };

        _uow.Setup(u => u.Carts.GetByUserIdWithItemsAsync("user-4")).ReturnsAsync(cart);
        _uow.Setup(u => u.Products.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _uow.Setup(u => u.Orders.GetNextOrderSequenceAsync()).ReturnsAsync(1);
        _uow.Setup(u => u.Orders.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.CheckoutAsync("user-4", ValidCheckout());

        product.StockQuantity.Should().Be(7);
        cart.CartItems.Should().BeEmpty();
        result.TotalAmount.Should().Be(3 * 19.99m);
        result.OrderNumber.Should().StartWith("ORD-");
    }
}
