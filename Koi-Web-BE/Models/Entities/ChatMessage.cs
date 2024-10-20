using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class ChatMessage : BaseEntity
{
    public string SenderId { get; set; } = null!;
    public Guid ChatRoomId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    //relations
    public User Sender { get; set; } = null!;
    public ChatRoom ChatRoom { get; set; } = null!;
}