using Koi_Web_BE.Models.Entities;

namespace Koi_Web_BE.Models.Primitives;

public class CurrentUser
{
    public User? User { get; set; } = null!;
}