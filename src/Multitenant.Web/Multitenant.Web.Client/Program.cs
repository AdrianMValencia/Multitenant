using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Determinamos la URL de la API (desde configuración o la dirección base del host)
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? builder.HostEnvironment.BaseAddress;

//// 1. Inyectamos la capacidad de manejar estados de autenticación
//builder.Services.AddCascadingAuthenticationState();
//// Registro del proveedor de identidad WASM
//builder.Services.AddScoped<MultitenantAuthStateProvider>();
//// Mapeo automático de la interfaz AuthenticationStateProvider a nuestra clase concreta
//builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
//    sp.GetRequiredService<MultitenantAuthStateProvider>());

builder.Services.AddMudServices();

// Inyección de servicios de cliente y ApiClient unificado
//builder.Services.AddMultitenantClientServices(apiBaseUrl);

await builder.Build().RunAsync();
