namespace Multitenant.Web.Client.Constants;

/// <summary>
/// Claves de localStorage. Prefijo "multitenant_" para no chocar con otras apps en el mismo origen.
/// </summary>
public static class StorageKeys
{
    public const string AccessToken = "multitenant_access_token";
    public const string TenantId = "multitenant_tenant_id";
    public const string TokenExpiry = "multitenant_token_expiry";
}
