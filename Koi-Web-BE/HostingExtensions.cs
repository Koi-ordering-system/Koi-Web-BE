using System.Reflection;
using CloudinaryDotNet;
using FluentValidation;
using Koi_Web_BE.Behaviors;
using Koi_Web_BE.Database;
using Koi_Web_BE.Database.Interceptors;
using Koi_Web_BE.Middlewares;
using Koi_Web_BE.Models.ExternalEntities;
using Koi_Web_BE.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Net.payOS;

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

        builder.Services.AddScoped(s => new Cloudinary(
            Environment.GetEnvironmentVariable("CLOUDINARY_URL")
        ));

        builder.Services.AddPayOSService(configuration);
        builder.Services.AddUrlSettings(configuration);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.Configure<RouteHandlerOptions>(opt => opt.ThrowOnBadRequest = true);
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSingleton<ExceptionMiddleware>();

        var connectionString =
            configuration["CONNECTION_STRING"]
            ?? throw new InvalidOperationException("DefaultConnection are missing");

        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, UpdateAuditableInterceptor>();
        builder.Services.AddScoped<IImageService, ImageService>();
        builder.Services.AddScoped<IPayOSServices, PayOSServices>();

        builder.Services.AddDbContext<ApplicationDbContext>(
            (sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.EnableSensitiveDataLogging();

                options.UseNpgsql(connectionString);
                // .UseSnakeCaseNamingConvention();
            }
        );

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

    public static IServiceCollection AddPayOSService(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        string clientId =
            configuration["PAYOS_CLIENT_ID"] ?? throw new Exception("PAYOS_CLIENT_ID is missing");
        string apiKey =
            configuration["PAYOS_API_KEY"] ?? throw new Exception("PAYOS_API_KEY is missing");
        string checkSumKey =
            configuration["PAYOS_CHECKSUM_KEY"]
            ?? throw new Exception("PAYOS_CHECKSUM_KEY is missing");

        PayOS payOS = new(clientId, apiKey, checkSumKey);
        services.AddScoped(s => payOS);

        return services;
    }

    public static IServiceCollection AddUrlSettings(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        string returnUrl =
            configuration["RETURN_URL"] ?? throw new Exception("RETURN_URL is missing");
        string cancelUrl =
            configuration["CANCEL_URL"] ?? throw new Exception("CANCEL_URL is missing");

        UrlSettings urlSettings = new() { ReturnUrl = returnUrl, CancelUrl = cancelUrl };
        services.AddScoped(s => urlSettings);

        return services;
    }
}
