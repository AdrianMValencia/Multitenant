// Interoperabilidad con JavaScript para acceder a las APIs nativas del navegador
using Microsoft.JSInterop;
// Constantes que definen las llaves de guardado en el LocalStorage
using Multitenant.Web.Client.Constants;

namespace Multitenant.Web.Client.Auth;

// Implementación del storage para Blazor WebAssembly usando el LocalStorage del navegador
public class LocalStorageTokenStorage(IJSRuntime js) : ITokenStorage
{
    // Caché en memoria para evitar llamadas de red/JS redundantes en el mismo ciclo
    private string? _accessToken;
    private Guid? _tenantId;

    // Recupera el JWT de la sesión actual
    public async Task<string?> GetAccessTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_accessToken))
        {
            return _accessToken;
        }

        try
        {
            _accessToken = await js.InvokeAsync<string?>("localStorage.getItem", StorageKeys.AccessToken);
            return _accessToken;
        }
        catch { return null; }
    }

    // Recupera el ID de la empresa guardado al momento del login
    public async Task<Guid?> GetTenantIdAsync()
    {
        if (_tenantId.HasValue)
        {
            return _tenantId;
        }

        try
        {
            // Leemos el valor en bruto (string) del storage del navegador
            var raw = await js.InvokeAsync<string?>("localStorage.getItem", StorageKeys.TenantId);
            if (!Guid.TryParse(raw, out var id)) return null;

            _tenantId = id; // Guardamos en caché
            return id;
        }
        catch { return null; }
    }

    // Persiste de forma segura los datos de la sesión tras un login exitoso
    public async Task SaveAsync(string accessToken, Guid tenantId, DateTime expiry)
    {
        _accessToken = accessToken;
        _tenantId = tenantId;

        try
        {
            // Guardamos cada valor de forma atómica en el LocalStorage para que sobrevivan a recargas (F5)
            await js.InvokeVoidAsync("localStorage.setItem", StorageKeys.AccessToken, accessToken);
            await js.InvokeVoidAsync("localStorage.setItem", StorageKeys.TenantId, tenantId.ToString());
            await js.InvokeVoidAsync("localStorage.setItem", StorageKeys.TokenExpiry, expiry.ToString("O"));
        }
        catch
        {
            // Manejar posibles errores de cuota excedida o modo incógnito
        }
    }

    // Limpia total de la sesión (usado al cerrar sesión o por expiración)
    public async Task ClearAsync()
    {
        _accessToken = null;
        _tenantId = null;

        try
        {
            // Borramos físicamente los registros del sistema
            await js.InvokeVoidAsync("localStorage.removeItem", StorageKeys.AccessToken);
            await js.InvokeVoidAsync("localStorage.removeItem", StorageKeys.TenantId);
            await js.InvokeVoidAsync("localStorage.removeItem", StorageKeys.TokenExpiry);
        }
        catch
        {
        }
    }
}
