using FluentValidation;
using GroomerManager.Application.Common.Abstraction;
using GroomerManager.Application.Common.Behaviors;
using GroomerManager.Application.Common.Interfaces;
using GroomerManager.Application.Common.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GroomerManager.Application;

public static class Extension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddCQRS();
        services.AddValidators();
        services.AddScoped<ICurrentSalonProvider, CurrentSalonProvider>();
        return services;
    }
    
    private static IServiceCollection AddCQRS(this IServiceCollection service)
    {
        service.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<BaseCommandHandler>();
        });
        return service;
    }
    
    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(BaseQueryHandler));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}