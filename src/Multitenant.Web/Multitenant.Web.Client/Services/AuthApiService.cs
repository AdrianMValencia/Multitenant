using Multitenant.Web.Client.Auth;
using Multitenant.Web.Client.Constants;
using Multitenant.Web.Client.DTOs.Auth;
using Multitenant.Web.Client.Services.Contracts;
using System.Net.Http.Json;

namespace Multitenant.Web.Client.Services;

/// <summary>Habla con api/auth. Tras login guarda JWT en localStorage y notifica a Blazor.</summary>
public class AuthApiService(
    HttpClient httpClient,
    ITokenStorage tokenStorage,
    MultitenantAuthStateProvider authStateProvider) : IAuthApiService
{
    public async Task<(bool Success, string? Error)> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(ApiEndpoints.Auth.Login, request);
            if (!response.IsSuccessStatusCode)
                return (false, "Credenciales incorrectas");

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (result is null)
                return (false, "Respuesta inesperada del servidor");

            await tokenStorage.SaveAsync(result.AccessToken, request.TenantId, result.ExpiresAtUtc);
            authStateProvider.NotifyStateChanged();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await httpClient.PostAsync(ApiEndpoints.Auth.Logout, null);
        }
        catch
        {
            // si el API no responde, igual limpiamos local
        }

        await tokenStorage.ClearAsync();
        authStateProvider.NotifyStateChanged();
    }
}
