using Koi_Web_BE.Extensions;
using Koi_Web_BE.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Koi_Web_BE.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator) : DbContext(options), IApplicationDbContext
{
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Color> Colors => Set<Color>();
    public DbSet<Farm> Farms => Set<Farm>();
    public DbSet<FarmImage> FarmImages => Set<FarmImage>();
    public DbSet<FarmKoi> FarmKois => Set<FarmKoi>();
    public DbSet<Koi> Kois => Set<Koi>();
    public DbSet<KoiImage> KoiImages => Set<KoiImage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderKoi> OrderKois => Set<OrderKoi>();
    public DbSet<OrderTrip> OrderTrips => Set<OrderTrip>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Species> Species => Set<Species>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Cart>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<CartItem>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Color>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Farm>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<FarmImage>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<FarmKoi>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Koi>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<KoiImage>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Order>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<OrderKoi>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<OrderTrip>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Review>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Species>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
    }
    public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        Task task = mediator.DispatchDomainEvents(this);
        UpdateTimestamps();
        await Task.WhenAny(task);
        return await base.SaveChangesAsync(cancellationToken);
    }

    public void UpdateTimestamps()
    {
        // Get all entries tracked by the context
        var modifiedEntries = ChangeTracker.Entries()
            .Where(entry => entry.State == EntityState.Modified ||
                            entry.State == EntityState.Deleted);

        foreach (var entry in modifiedEntries)
        {
            foreach (var property in new[] { "UpdatedAt", "DeletedAt", "IsDeleted" })
            {
                var propertyEntry = entry.Property(property);

                // Handle Modified Entities
                if (entry.State == EntityState.Modified)
                {
                    if (property == "UpdatedAt")
                    {
                        propertyEntry.CurrentValue = DateTimeOffset.UtcNow;
                    }
                }
                // Handle Deleted Entities (Soft Delete)
                if (entry.State == EntityState.Deleted)
                {
                    if (property == "DeletedAt")
                    {
                        propertyEntry.CurrentValue = DateTimeOffset.UtcNow;  // Set the deletion timestamp
                    }
                    if (property == "IsDeleted")
                    {
                        propertyEntry.CurrentValue = true;  // Mark the entity as deleted
                        entry.State = EntityState.Modified;  // Prevent EF from hard deleting it
                    }

                }
            }
        }
    }

}