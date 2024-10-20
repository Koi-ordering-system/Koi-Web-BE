using Koi_Web_BE.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Koi_Web_BE.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
{
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
    public DbSet<User> Users => Set<User>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<UserConnection> UserConnections => Set<UserConnection>();
}