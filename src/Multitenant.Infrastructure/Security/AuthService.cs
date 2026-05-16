using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Multitenant.Application.Abstractions.Security;
using Multitenant.Infrastructure.Persistence.Context;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Multitenant.Infrastructure.Security;

public class AuthService(
    ApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IOptions<JwtOptions> jwtOptions,
    InMemoryRefreshTokenStore refreshTokenStore) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResult?> LoginAsync(Guid tenantId, string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Email == email && x.IsActive == true, cancellationToken);

        if (user?.Password is null || !passwordHasher.Verify(user.Password, password))
        {
            return null;
        }

        if (!IsPbkdf2Format(user.Password))
        {
            user.Password = passwordHasher.Hash(password);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return await GenerateTokensAsync(user.Id, tenantId, email, user.UserRoles.Select(ur => ur.Role?.Name).OfType<string>(), cancellationToken);
    }

    public async Task<AuthResult?> RefreshAsync(Guid tenantId, string refreshToken, CancellationToken cancellationToken = default)
    {
        if (!refreshTokenStore.TryGet(refreshToken, out var tokenData))
        {
            return null;
        }

        if (tokenData.TenantId != tenantId || tokenData.ExpiresAtUtc <= DateTime.UtcNow)
        {
            refreshTokenStore.Remove(refreshToken);
            return null;
        }

        var user = await dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.Id == tokenData.UserId && x.TenantId == tenantId && x.IsActive == true, cancellationToken);

        if (user is null)
        {
            refreshTokenStore.Remove(refreshToken);
            return null;
        }

        refreshTokenStore.Remove(refreshToken);
        return await GenerateTokensAsync(user.Id, tenantId, user.Email ?? string.Empty, user.UserRoles.Select(ur => ur.Role?.Name).OfType<string>(), cancellationToken);
    }

    private static bool IsPbkdf2Format(string passwordHash)
    {
        var parts = passwordHash.Split('.');
        return parts.Length == 3
               && int.TryParse(parts[0], out _)
               && TryFromBase64(parts[1])
               && TryFromBase64(parts[2]);
    }

    private static bool TryFromBase64(string value)
    {
        try
        {
            _ = Convert.FromBase64String(value);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private Task<AuthResult> GenerateTokensAsync(Guid userId, Guid tenantId, string email, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTime.UtcNow;
        var accessTokenExpiration = now.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var refreshTokenExpiration = now.AddDays(_jwtOptions.RefreshTokenDays);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("tenant_id", tenantId.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: accessTokenExpiration,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        refreshTokenStore.Save(refreshToken, userId, tenantId, refreshTokenExpiration);

        return Task.FromResult(new AuthResult(accessToken, refreshToken, accessTokenExpiration, refreshTokenExpiration));
    }
}
