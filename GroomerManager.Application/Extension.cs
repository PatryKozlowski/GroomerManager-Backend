using Microsoft.Extensions.DependencyInjection;

namespace GroomerManager.Application;

public static class Extension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}