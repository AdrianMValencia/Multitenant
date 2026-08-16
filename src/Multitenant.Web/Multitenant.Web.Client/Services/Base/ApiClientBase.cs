// Referencias internas para autenticación y modelos de respuesta del cliente
using Multitenant.Web.Client.Auth;
using Multitenant.Web.Client.DTOs.Commons;
// Librerías nativas para cabeceras HTTP y serialización JSON
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Multitenant.Web.Client.Services.Base;

public abstract class ApiClientBase(HttpClient httpClient, ITokenStorage tokenStorage)
{
    // Método genérico para peticiones GET
    protected async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        // Preparamos los headers (JWT y TenantId) antes de llamar
        await PrepareRequestAsync();
        try
        {
            // Ejecutamos la llamada HTTP real
            var response = await httpClient.GetAsync(endpoint);
            // Parseamos el JSON a nuestro modelo ApiResponse estándar
            return await ParseResponseAsync<T>(response);
        }
        catch (Exception ex)
        {
            // Captura errores de red o DNS y devuelve un objeto de error uniforme
            return Fail<T>(ex.Message);
        }
    }

    // Método genérico para peticiones POST (creación de datos)
    protected async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object body)
    {
        // Preparamos los headers de seguridad y contexto
        await PrepareRequestAsync();
        try
        {
            // Serializamos el cuerpo y enviamos la petición JSON
            var response = await httpClient.PostAsJsonAsync(endpoint, body);
            // Procesamos la respuesta del servidor
            return await ParseResponseAsync<T>(response);
        }
        catch (Exception ex) { return Fail<T>(ex.Message); }
    }

    // Método privado para inyectar Token JWT y el TenantId en cada petición saliente
    private async Task PrepareRequestAsync()
    {
        // Intentamos obtener el token actual (con reintentos por latencia de storage)
        var token = await GetTokenWithRetryAsync();
        // Resolvemos el TenantId para asegurar que la API sepa a qué cuenta pertenece la acción
        var tenantId = await GetTenantWithRetryAsync(token);

        // Agregamos el header Authorization si el usuario está logueado
        httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);

        // Limpiamos e inyectamos el header X-Tenant-Id fundamental para el Multitenancy
        httpClient.DefaultRequestHeaders.Remove("X-Tenant-Id");
        if (tenantId.HasValue)
            httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.Value.ToString());
    }

    // Lógica de reintento para obtener el token, útil en Blazor WebAssembly al arrancar
    private async Task<string?> GetTokenWithRetryAsync()
    {
        for (var i = 0; i < 5; i++)
        {
            var token = await tokenStorage.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                return token;
            }

            // Pequeña espera si el storage no está listo (hidratación del estado)
            await Task.Delay(100);
        }

        return null;
    }

    // Lógica de reintento para obtener el TenantId del storage o directamente del Token
    private async Task<Guid?> GetTenantWithRetryAsync(string? token)
    {
        for (var i = 0; i < 5; i++)
        {
            var tenantId = await tokenStorage.GetTenantIdAsync();
            if (tenantId.HasValue)
            {
                return tenantId;
            }

            await Task.Delay(100);
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        // Recuperar el tenant_id desde los Claims del JWT como último recurso
        var claim = JwtParser.ParseClaims(token).FirstOrDefault(x => x.Type == "tenant_id")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    // Convierte el flujo de respuesta HTTP en un objeto ApiResponse tipado
    private static async Task<ApiResponse<T>> ParseResponseAsync<T>(HttpResponseMessage response)
    {
        try
        {
            // Deserialización segura del contenido
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
            if (result is not null) return result;
        }
        catch { /* si falla el parseo, manejamos el estado abajo */ }

        // Devolvemos un estado razonable basado en el código HTTP
        return new ApiResponse<T>
        {
            IsSuccess = response.IsSuccessStatusCode,
            Message = response.IsSuccessStatusCode
                ? "OK"
                : $"Error inesperado del servidor: {(int)response.StatusCode}"
        };
    }

    // Método de utilidad para generar un objeto de fallo ApiResponse
    protected static ApiResponse<T> Fail<T>(string message) => new()
    {
        IsSuccess = false,
        Message = message
    };
}
