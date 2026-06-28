using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.DTOs.Auth;
using FluentAssertions;
using Xunit;

namespace ECommerce.Tests.Integration;

public class AuthIntegrationTests : IClassFixture<ECommerceWebAppFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(ECommerceWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        var dto = new RegisterDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = $"jane{Guid.NewGuid():N}@test.com",
            Password = "Test123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns400()
    {
        var dto = new RegisterDto
        {
            FirstName = "Dup",
            LastName = "User",
            Email = "customer@shop.com",
            Password = "Customer123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithTokens()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "customer@shop.com",
            Password = "Customer123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "customer@shop.com",
            Password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "nobody@nowhere.com",
            Password = "Whatever1!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
