using Koi_Web_BE.Models.Enums;

namespace Koi_Web_BE.Models.Entities;

public class User : BaseEntity
{
    public new required string Id { get; set; }
    public required string Username { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required string PhoneNumber { get; set; }
    public RoleEnum Role { get; set; } = RoleEnum.Customer;
}