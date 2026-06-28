using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Mappings;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace ECommerce.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly IECommerceMapper _mapper = new ECommerceMapper();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_uow.Object, _mapper);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductNotFound_ThrowsNotFoundException()
    {
        _uow.Setup(u => u.Products.GetByIdWithCategoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Product?)null);

        var act = () => _sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProductDto()
    {
        var categoryId = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Desc",
            Price = 9.99m,
            CategoryId = categoryId,
            Category = new Category { Id = categoryId, Name = "Electronics" }
        };

        _uow.Setup(u => u.Products.GetByIdWithCategoryAsync(product.Id))
            .ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(product.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(product.Id);
        result.Name.Should().Be("Test Product");
        result.CategoryName.Should().Be("Electronics");
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryNotFound_ThrowsNotFoundException()
    {
        _uow.Setup(u => u.Categories.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Category?)null);

        var act = () => _sut.CreateAsync(new CreateProductDto
        {
            Name = "X", Price = 1, CategoryId = Guid.NewGuid()
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_AddsProductAndReturnsDto()
    {
        var category = new Category { Id = Guid.NewGuid(), Name = "Books" };

        _uow.Setup(u => u.Categories.GetByIdAsync(category.Id)).ReturnsAsync(category);
        _uow.Setup(u => u.Products.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.CreateAsync(new CreateProductDto
        {
            Name = "Clean Code",
            Description = "A great book",
            Price = 34.99m,
            StockQuantity = 10,
            CategoryId = category.Id
        });

        result.Name.Should().Be("Clean Code");
        result.Price.Should().Be(34.99m);
        _uow.Verify(u => u.Products.AddAsync(It.IsAny<Product>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_SetsIsActiveFalse_InsteadOfHardDelete()
    {
        var product = new Product { Id = Guid.NewGuid(), IsActive = true };
        _uow.Setup(u => u.Products.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _uow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        await _sut.DeleteAsync(product.Id);

        product.IsActive.Should().BeFalse();
        _uow.Verify(u => u.Products.Update(product), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
