namespace Multitenant.Application.Abstractions.Security;

public interface IAuthService
{
    Task<AuthResult?> LoginAsync(Guid tenantId, string email, string password, CancellationToken cancellationToken = default);
    Task<AuthResult?> RefreshAsync(Guid tenantId, string refreshToken, CancellationToken cancellationToken = default);
}

public sealed record AuthResult(
    string AccessToken, 
    string RefreshToken, 
    DateTime AccessTokenExpiresAtUtc, 
    DateTime RefreshTokenExpiresAtUtc);