using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Multitenant.Api.Security;
using Multitenant.Application;
using Multitenant.Application.Abstractions.Security;
using Multitenant.Infrastructure;
using Multitenant.Infrastructure.Multitenancy;
using Multitenant.Infrastructure.Security;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Contexto para identificar al usuario autenticado en cada petición
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();

// Configuración de Autenticación via JWT (Bearer)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            // Firma criptográfica para validar la autenticidad del token
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey))
        };
    });

// Sistema dinámico de Autorización (RBAC) basada en Permisos
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Configuración de política de cookies segura (HttpOnly + Secure + Strict)
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.Secure = CookieSecurePolicy.Always; // Obliga HTTPS
    options.MinimumSameSitePolicy = SameSiteMode.Strict; // Previene ataques CSRF
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always; // Protege contra XSS
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configuración de CORS permitiendo acceso desde cualquier origen (ajustar en prod)
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CORSPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configuración del Pipeline HTTP (Orden Crítico)
app.UseCors("CORSPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // SEEDING EN DESARROLLO: 2 empresas, usuarios admin y clientes independientes
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<IDevelopmentDataSeeder>();
        await seeder.SeedAsync();
    }
}

app.UseHttpsRedirection();

app.UseCookiePolicy(); // Inyecta reglas de cookies seguras

app.UseAuthentication(); // 1ero. ¿Quién eres? (JWT)

app.UseTenantResolution(); // 2do. ¿A qué empresa perteneces? (Basado en JWT o Header)

app.UseAuthorization(); // 3ero. ¿Qué tienes permitido hacer? (Roles/Permisos)

app.MapControllers();

app.Run();
