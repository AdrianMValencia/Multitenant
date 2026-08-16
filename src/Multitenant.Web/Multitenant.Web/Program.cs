using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using Multitenant.Web.Client.Auth;
using Multitenant.Web.Client.Extensions;
using Multitenant.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Recuperamos la URL base de la API desde la configuración (appsettings.json)
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl no está configurado.");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Autenticación HTTP mínima (evita el error de IAuthenticationService).
// El JWT de la API se guarda en localStorage; este esquema solo cubre el host Blazor.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.Cookie.Name = "Multitenant.Web.Auth";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Registro del proveedor de identidad personalizado para Blazor
builder.Services.AddScoped<MultitenantAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<MultitenantAuthStateProvider>());

// Inicialización del framework de UI MudBlazor
builder.Services.AddMudServices();
// Registro de servicios de negocio personalizados del cliente (ApiClient, etc.)
builder.Services.AddMultitenantClientServices(apiBaseUrl);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Habilitar protección contra ataques CSRF/XSRF
app.UseAntiforgery();

// Mapeo automático de archivos estáticos (CSS, JS, Img)
app.MapStaticAssets();

// Mapeo del componente raíz y configuración de los modos de renderizado híbrido
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    // Registramos el ensamblado del cliente para que Blazor encuentre las páginas WASM
    .AddAdditionalAssemblies(typeof(Multitenant.Web.Client._Imports).Assembly);

app.Run();
