using Koi_Web_BE.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Koi_Web_BE.Database;

public interface IApplicationDbContext
{
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
    DbSet<User> Users { get; }
    DbSet<Trip> Trips { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<ChatRoom> ChatRooms { get; }
    DbSet<UserConnection> UserConnections { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}