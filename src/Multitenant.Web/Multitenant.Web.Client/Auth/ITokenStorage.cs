namespace Multitenant.Web.Client.Auth;

/// <summary>
/// Dónde vive la sesión del navegador (JWT + tenant). Implementación: localStorage.
/// </summary>
public interface ITokenStorage
{
    Task<string?> GetAccessTokenAsync();
    Task<Guid?> GetTenantIdAsync();
    Task SaveAsync(string accessToken, Guid tenantId, DateTime expiry);
    Task ClearAsync();
}
