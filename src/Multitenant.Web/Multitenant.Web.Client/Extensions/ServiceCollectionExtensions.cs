using Multitenant.Web.Client.Auth;
using Multitenant.Web.Client.Services;
using Multitenant.Web.Client.Services.Contracts;

namespace Multitenant.Web.Client.Extensions;

/// <summary>Registro DI compartido por el host Blazor Server y el cliente WASM.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMultitenantClientServices(
        this IServiceCollection services,
        string apiBaseUrl)
    {
        services.AddScoped<ITokenStorage, LocalStorageTokenStorage>();
        services.AddScoped<IAuthApiService, AuthApiService>();
        services.AddScoped<ICustomerApiService, CustomerApiService>();

        // Timeout: si la API está caída, la UI no se queda en "Cargando..." para siempre.
        services.AddScoped(_ => new HttpClient
        {
            BaseAddress = new Uri(apiBaseUrl),
            Timeout = TimeSpan.FromSeconds(15)
        });

        return services;
    }
}
