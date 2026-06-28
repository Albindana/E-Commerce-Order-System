using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(AppUser user, IList<string> roles);
    string GenerateRefreshToken();
    DateTime GetRefreshTokenExpiry();
}
