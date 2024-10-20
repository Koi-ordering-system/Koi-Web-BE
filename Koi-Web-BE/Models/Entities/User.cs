using Koi_Web_BE.Models.Enums;
using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class User : BaseEntity
{
    public new string Id { get; set; } = string.Empty;
    public required string Username { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required string PhoneNumber { get; set; }
    public RoleEnum Role { get; set; } = RoleEnum.Customer;
    // Relations
    public IList<Order> Orders { get; set; } = [];
    // public Cart Carts { get; set; } = null!;
    public IList<Review> Reviews { get; set; } = [];
    public IList<UserConnection> UserConnections { get; set; } = [];

    public bool IsAdmin() => Role.Equals(RoleEnum.Admin);
    public bool IsManager() => Role.Equals(RoleEnum.Manager);
}