using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Multitenant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {


        return services;
    }
}
