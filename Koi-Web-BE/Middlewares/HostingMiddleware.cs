using Microsoft.EntityFrameworkCore;

namespace Koi_Web_BE.Middlewares;

public static class HostingMiddleware
{
    public static IHost MigrateDatabase<TContext>(this IHost host, Func<TContext, IServiceProvider, Task> seeder)
            where TContext : DbContext
    {
        using var scope = host.Services.CreateScope();

        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<TContext>();

        try
        {
            // context.Database.EnsureDeleted();
            context?.Database.Migrate();
            seeder(context!, services).Wait();
        }
        catch (Exception ex)
        {
            // logger.LogError(ex, "An error occured while migrating database!");
            Console.WriteLine(ex);
        }

        return host;
    }
}