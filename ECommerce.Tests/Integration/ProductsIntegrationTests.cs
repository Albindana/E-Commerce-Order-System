using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Product;
using FluentAssertions;
using Xunit;

namespace ECommerce.Tests.Integration;

public class ProductsIntegrationTests : IClassFixture<ECommerceWebAppFactory>
{
    private readonly HttpClient _client;

    public ProductsIntegrationTests(ECommerceWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "admin@shop.com",
            Password = "Admin123!"
        });
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return body!.AccessToken;
    }

    [Fact]
    public async Task GetProducts_Anonymous_Returns200WithPagedResult()
    {
        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
        body!.Items.Should().NotBeEmpty();
        body.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetProductById_WithValidId_Returns200()
    {
        var listResponse = await _client.GetAsync("/api/products");
        var list = await listResponse.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
        var firstId = list!.Items.First().Id;

        var response = await _client.GetAsync($"/api/products/{firstId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProductDto>();
        body!.Id.Should().Be(firstId);
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_Returns404()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_AsAdmin_Returns201()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var categoryResponse = await _client.GetAsync("/api/categories");
        var categories = await categoryResponse.Content.ReadFromJsonAsync<List<ECommerce.Application.DTOs.Category.CategoryDto>>();
        var categoryId = categories!.First().Id;

        var dto = new CreateProductDto
        {
            Name = "Integration Test Product",
            Description = "Created by integration test",
            Price = 49.99m,
            StockQuantity = 5,
            CategoryId = categoryId
        };

        var response = await _client.PostAsJsonAsync("/api/products", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ProductDto>();
        body!.Name.Should().Be("Integration Test Product");
        body.Price.Should().Be(49.99m);

        _client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task CreateProduct_Unauthenticated_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new CreateProductDto
        {
            Name = "X", Price = 1, CategoryId = Guid.NewGuid()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
