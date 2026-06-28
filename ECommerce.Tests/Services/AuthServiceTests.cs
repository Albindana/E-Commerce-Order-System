using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace ECommerce.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<AppUser>> _userManager;
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userManager = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

        _sut = new AuthService(_userManager.Object, _tokenService.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        var user = new AppUser { Id = "u1", Email = "test@test.com", FirstName = "John", LastName = "Doe" };

        _userManager.Setup(m => m.FindByEmailAsync("test@test.com")).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "Password1!")).ReturnsAsync(true);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Customer" });
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        _tokenService.Setup(t => t.GenerateAccessToken(user, It.IsAny<IList<string>>())).Returns("access-token");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");
        _tokenService.Setup(t => t.GetRefreshTokenExpiry()).Returns(DateTime.UtcNow.AddDays(7));

        var result = await _sut.LoginAsync(new LoginDto { Email = "test@test.com", Password = "Password1!" });

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorizedException()
    {
        var user = new AppUser { Email = "test@test.com" };

        _userManager.Setup(m => m.FindByEmailAsync("test@test.com")).ReturnsAsync(user);
        _userManager.Setup(m => m.CheckPasswordAsync(user, "wrong")).ReturnsAsync(false);

        var act = () => _sut.LoginAsync(new LoginDto { Email = "test@test.com", Password = "wrong" });

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsUnauthorizedException()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AppUser?)null);

        var act = () => _sut.LoginAsync(new LoginDto { Email = "ghost@test.com", Password = "any" });

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsBadRequestException()
    {
        _userManager.Setup(m => m.FindByEmailAsync("taken@test.com"))
            .ReturnsAsync(new AppUser());

        var act = () => _sut.RegisterAsync(new RegisterDto
        {
            Email = "taken@test.com", Password = "Pass1!",
            FirstName = "Jane", LastName = "Doe"
        });

        await act.Should().ThrowAsync<BadRequestException>();
    }
}
