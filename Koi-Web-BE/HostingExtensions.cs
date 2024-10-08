using System.Reflection;
using FluentValidation;
using Koi_Web_BE.Behaviors;
using Koi_Web_BE.Database;
using Koi_Web_BE.Database.Interceptors;
using Koi_Web_BE.Middlewares;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Koi_Web_BE;

public static class HostingExtensions
{
    /// <summary>
    /// ConfigureServices manages services registrations and
    /// handles core cross-cutting concerns' configurations
    /// like authentication, authorization,...
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.LoadEnv();
        var configuration = builder.Configuration;

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.Configure<RouteHandlerOptions>(opt => opt.ThrowOnBadRequest = true);
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSingleton<ExceptionMiddleware>();

        var connectionString = configuration["CONNECTION_STRING"]
                               ?? throw new InvalidOperationException("DefaultConnection are missing");

        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, UpdateAuditableInterceptor>();
        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.EnableSensitiveDataLogging();

            options.UseNpgsql(connectionString);
            // .UseSnakeCaseNamingConvention();
        });

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
            config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        return builder.Build();
    }

    /// <summary>
    /// LoadEnv loads environment variables from .env file corresponding
    /// to the application environment name and add to configuration 
    /// </summary>
    /// <param name="builder"></param>
    private static void LoadEnv(this WebApplicationBuilder builder)
    {
        DotNetEnv.Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
        builder.Configuration.AddEnvironmentVariables();
    }
}