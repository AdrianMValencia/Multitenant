namespace Multitenant.Web.Client.DTOs.Auth;

/// <summary>Respuesta de login/refresh. El refresh token no viene aquí: va en cookie.</summary>
public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc);
