using Azure.Storage.Blobs;
using GroomerManager.Application.Common.Interfaces;
using GroomerManager.Infrastructure.Auth;
using GroomerManager.Infrastructure.Email;
using GroomerManager.Infrastructure.Persistence.Configurations;
using GroomerManager.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GroomerManager.Infrastructure;

public static class Extension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseConfiguration(configuration.GetConnectionString("GroomerManagerStore")!);
        services.AddSingleton(_ => new BlobServiceClient(configuration.GetConnectionString("BlobStorage")));
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<ConfirmEmailSettings>(configuration.GetSection("ConfirmEmailSettings"));
        services.AddTransient<IDateTime, DataTimeService>();
        services.AddTransient<IConfirmEmail, ConfirmEmail>();
        services.AddTransient<IEmailSchema, EmailSchema>();
        services.AddJwt(configuration);
        services.AddScoped(typeof(IPasswordHasher<>), typeof(PasswordHasher<>));
        services.AddScoped<IPasswordManager, PasswordManager>();
        services.AddSingleton<IBlobService, BlobService>();
        services.AddTransient<IEmailSender, EmailSender>();
        return services;
    }
}