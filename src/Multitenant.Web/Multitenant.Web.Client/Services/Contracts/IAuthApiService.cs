using Multitenant.Web.Client.DTOs.Auth;

namespace Multitenant.Web.Client.Services.Contracts;

public interface IAuthApiService
{
    /// <returns>Success=false si credenciales inválidas o la API no responde.</returns>
    Task<(bool Success, string? Error)> LoginAsync(LoginRequest request);
    Task LogoutAsync();
}
