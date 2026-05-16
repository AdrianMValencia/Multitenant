using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Multitenant.Application.Abstractions.Messaging;
using Multitenant.Application.Commons.Behaviours;

namespace Multitenant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddScoped<HandlerExecutor>();
        services.AddScoped<IValidationService, ValidationService>();
        // 1. Escaneo automático y registro de todos los Handlers del proyecto
        RegisterHandlers(services);
        // 2. Escaneo automático y registro de todos los Validadores disponibles
        RegisterValidators(services);

        return services;
    }

    // Lógica reflectiva para encontrar e inyectar CommandHandlers y QueryHandlers
    private static void RegisterHandlers(IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        // Filtramos clases no abstractas que cumplan con la interfaz de handler
        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false });

        foreach (var handlerType in handlerTypes)
        {
            // Buscamos interfaces genéricas que coincidan con el patrón CQRS
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Where(i =>
                {
                    var generic = i.GetGenericTypeDefinition();
                    return generic == typeof(ICommandHandler<,>) || generic == typeof(IQueryHandler<,>);
                });

            // Registramos el handler para cada interfaz que implementa (Scoped)
            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, handlerType);
            }
        }
    }

    // Lógica para registrar validadores de comandos/consultas automáticamente
    private static void RegisterValidators(IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        var validatorTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false });

        foreach (var validatorType in validatorTypes)
        {
            // Buscamos implementaciones de AbstractValidator<T> (FluentValidation)
            var interfaces = validatorType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));

            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, validatorType);
            }
        }
    }
}
