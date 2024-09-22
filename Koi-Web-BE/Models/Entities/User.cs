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
    public IList<Farm> Farms { get; set; } = [];
    public IList<Order> Orders { get; set; } = [];
    public IList<Cart> Carts { get; set; } = [];
    public IList<Koi> Kois { get; set; } = [];
    public IList<Review> Reviews { get; set; } = [];
    public IList<OrderKoi> OrderKois { get; set; } = [];
    public IList<KoiImage> KoiImages { get; set; } = [];
}