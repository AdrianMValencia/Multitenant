using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Multitenant.Web.Client.Auth;

// Proveedor personalizado de estado de autenticación - Origen de la verdad para la UI (AuthorizeView)
// Inyectamos el ITokenStorage para acceder dinámicamente al JWT almacenado
public class MultitenantAuthStateProvider(ITokenStorage tokenStorage) : AuthenticationStateProvider
{
    // Objeto estático que representa a un usuario no autenticado (anónimo)
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    // Método principal que Blazor invoca para saber quién es el usuario actual
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Intentamos recuperar el Access Token del almacenamiento persistente (ej: LocalStorage)
            var token = await tokenStorage.GetAccessTokenAsync();

            // Si no hay token, devolvemos el estado anónimo de inmediato
            if (string.IsNullOrWhiteSpace(token)) return Anonymous;

            // Decodificamos el JWT para extraer los Claims (Nombre, Roles, TenantId, etc.)
            var claims = JwtParser.ParseClaims(token);

            // Creamos una identidad basada en los Claims extraídos con el esquema "jwt"
            var identity = new ClaimsIdentity(claims, "jwt");

            // Retornamos el estado de autenticación con el Principal (usuario logueado)
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            // Ante cualquier error (ej: token corrupto), tratamos al usuario como anónimo por seguridad
            return Anonymous;
        }
    }

    // Método de utilidad para forzar la actualización de la UI cuando el estado cambia (Login/Logout)
    public void NotifyStateChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
