using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class UserConnection : BaseEntity
{
    public string UserId { get; set; } = null!;
    public Guid ChatRoomId { get; set; }
    //relations
    public User User { get; set; } = null!;
    public ChatRoom ChatRoom { get; set; } = null!;
}