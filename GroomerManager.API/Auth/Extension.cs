using GroomerManager.Application.Common.Interfaces;

namespace GroomerManager.API.Auth;

public static class Extension
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CookieSettings>(configuration.GetSection("CookieSettings"));
        services.AddScoped<IAuthenticationDataProvider, JwtAuthenticationDataProvider>();
        return services;
    }
}