using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GroomerManager.Infrastructure;

public static class Extension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}