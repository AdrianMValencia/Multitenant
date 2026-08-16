using Microsoft.AspNetCore.Mvc;
using Multitenant.Application.Abstractions.Security;

namespace Multitenant.Api.Controllers;

/// <summary>
/// Login / refresh / logout. El access token va en el JSON; el refresh token en cookie HttpOnly.
/// </summary>
[Route("api/[controller]")] // → api/auth
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request.TenantId, request.Email, request.Password, cancellationToken);
        if (result is null)
        {
            return Unauthorized();
        }

        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);
        SetCsrfCookie();

        return Ok(new AuthResponse(result.AccessToken, result.AccessTokenExpiresAtUtc));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue("refresh_token", out var cookieRefreshToken) || string.IsNullOrWhiteSpace(cookieRefreshToken))
        {
            return Unauthorized();
        }

        if (!Request.Headers.TryGetValue("X-CSRF-TOKEN", out var csrfHeader) || csrfHeader != Request.Cookies["csrf_token"])
        {
            return Unauthorized("CSRF validation failed.");
        }

        var refreshToken = string.IsNullOrWhiteSpace(request.RefreshToken) ? cookieRefreshToken : request.RefreshToken;
        var result = await authService.RefreshAsync(request.TenantId, refreshToken, cancellationToken);
        if (result is null)
        {
            return Unauthorized();
        }

        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAtUtc);
        SetCsrfCookie();

        return Ok(new AuthResponse(result.AccessToken, result.AccessTokenExpiresAtUtc));
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("refresh_token");
        Response.Cookies.Delete("csrf_token");
        return NoContent();
    }

    private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAtUtc)
    {
        // Cookie ilegible por JavaScript: reduce robo del refresh token por XSS.
        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAtUtc,
            IsEssential = true
        });
    }

    private void SetCsrfCookie()
    {
        var csrfToken = Guid.NewGuid().ToString("N");
        // HttpOnly = false: el front puede copiar este valor al header X-CSRF-TOKEN.
        Response.Cookies.Append("csrf_token", csrfToken, new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            IsEssential = true
        });
    }
}

public sealed record LoginRequest(Guid TenantId, string Email, string Password);
public sealed record RefreshTokenRequest(Guid TenantId, string? RefreshToken);
public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc);