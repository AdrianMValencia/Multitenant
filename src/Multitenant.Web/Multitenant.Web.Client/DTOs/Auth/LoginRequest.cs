namespace Multitenant.Web.Client.DTOs.Auth;

/// <summary>Cuerpo POST de login. TenantId elige la empresa.</summary>
public sealed record LoginRequest(Guid TenantId, string Email, string Password);
