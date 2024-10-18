using Koi_Web_BE.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Koi_Web_BE.Database;

public interface IApplicationDbContext
{
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Color> Colors { get; }
    DbSet<Farm> Farms { get; }
    DbSet<FarmKoi> FarmKois { get; }
    DbSet<FarmImage> FarmImages { get; }
    DbSet<Koi> Kois { get; }
    DbSet<KoiImage> KoiImages { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderKoi> OrderKois { get; }
    DbSet<OrderTrip> OrderTrips { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Species> Species { get; }
    DbSet<User> Users { get; }
    DbSet<Trip> Trips { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}