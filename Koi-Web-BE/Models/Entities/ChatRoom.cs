using Koi_Web_BE.Models.Primitives;

namespace Koi_Web_BE.Models.Entities;

public class ChatRoom : BaseEntity
{
    public string RoomName { get; set; } = string.Empty;
    //relations
    public IList<UserConnection> userConnections = [];
    public IList<ChatMessage> chatMessages = [];
}