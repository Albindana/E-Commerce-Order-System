using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Product;
using FluentAssertions;
using Xunit;

namespace ECommerce.Tests.Integration;

public class CartIntegrationTests : IClassFixture<ECommerceWebAppFactory>
{
    private readonly HttpClient _client;

    public CartIntegrationTests(ECommerceWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task SetCustomerAuthAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "customer@shop.com",
            Password = "Customer123!"
        });
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);
    }

    private async Task<Guid> GetFirstProductIdAsync()
    {
        var response = await _client.GetAsync("/api/products");
        var list = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
        return list!.Items.First().Id;
    }

    [Fact]
    public async Task GetCart_WhenEmpty_Returns200WithEmptyCart()
    {
        await SetCustomerAuthAsync();

        var response = await _client.GetAsync("/api/cart");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddItem_WithValidProduct_Returns200WithCartItem()
    {
        await SetCustomerAuthAsync();
        var productId = await GetFirstProductIdAsync();

        var response = await _client.PostAsJsonAsync("/api/cart/items", new AddCartItemDto
        {
            ProductId = productId,
            Quantity = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await response.Content.ReadFromJsonAsync<CartDto>();
        cart!.Items.Should().Contain(i => i.ProductId == productId);
    }

    [Fact]
    public async Task AddItem_Unauthenticated_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/cart/items", new AddCartItemDto
        {
            ProductId = Guid.NewGuid(),
            Quantity = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ClearCart_Returns204()
    {
        await SetCustomerAuthAsync();
        var productId = await GetFirstProductIdAsync();
        await _client.PostAsJsonAsync("/api/cart/items", new AddCartItemDto { ProductId = productId, Quantity = 1 });

        var response = await _client.DeleteAsync("/api/cart");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
