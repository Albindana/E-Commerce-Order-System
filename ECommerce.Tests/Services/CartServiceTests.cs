using AutoMapper;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Mappings;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace ECommerce.Tests.Services;

public class CartServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly IMapper _mapper;
    private readonly CartService _sut;

    public CartServiceTests()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
            .CreateMapper();
        _sut = new CartService(_uow.Object, _mapper);
    }

    [Fact]
    public async Task AddItemAsync_WhenCartDoesNotExist_CreatesNewCart()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "Keyboard", Price = 89.99m, StockQuantity = 10 };
        var userId = "user-1";

        _uow.Setup(u => u.Products.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _uow.Setup(u => u.Carts.GetByUserIdWithItemsAsync(userId)).ReturnsAsync((Cart?)null);
        _uow.Setup(u => u.Carts.AddAsync(It.IsAny<Cart>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.AddItemAsync(userId, new AddCartItemDto { ProductId = product.Id, Quantity = 1 });

        _uow.Verify(u => u.Carts.AddAsync(It.IsAny<Cart>()), Times.Once);
        result.Items.Should().HaveCount(1);
        result.Items[0].UnitPrice.Should().Be(89.99m);
    }

    [Fact]
    public async Task AddItemAsync_WhenProductAlreadyInCart_IncreasesQuantity()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "Headphones", Price = 149.99m, StockQuantity = 20 };
        var userId = "user-2";

        var existingItem = new CartItem
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Quantity = 1,
            UnitPrice = product.Price,
            Product = product
        };

        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId, CartItems = new List<CartItem> { existingItem } };

        _uow.Setup(u => u.Products.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _uow.Setup(u => u.Carts.GetByUserIdWithItemsAsync(userId)).ReturnsAsync(cart);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.AddItemAsync(userId, new AddCartItemDto { ProductId = product.Id, Quantity = 2 });

        existingItem.Quantity.Should().Be(3);
        _uow.Verify(u => u.Carts.AddAsync(It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task AddItemAsync_WhenStockInsufficient_ThrowsConflictException()
    {
        var product = new Product { Id = Guid.NewGuid(), Price = 10m, StockQuantity = 1 };
        _uow.Setup(u => u.Products.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var act = () => _sut.AddItemAsync("user-3", new AddCartItemDto { ProductId = product.Id, Quantity = 5 });

        await act.Should().ThrowAsync<ConflictException>();
    }
}
