using Koi_Web_BE.Database;
using Koi_Web_BE.Exceptions;
using Koi_Web_BE.Models.Entities;
using Koi_Web_BE.Models.Primitives;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Koi_Web_BE.Utils;


public class ChatHub(IApplicationDbContext context, CurrentUser currentUser) : Hub
{
    public record ChatMessageResponse(
        string AvatarUrl,
        string Username,
        string Content,
        DateTimeOffset CreatedAt
    )
    {
        public static ChatMessageResponse MapToChatMessageResponse(ChatMessage chatMessage) =>
            new(
                AvatarUrl: chatMessage.Sender.AvatarUrl,
                Username: chatMessage.Sender.Username,
                Content: chatMessage.Content,
                CreatedAt: chatMessage.CreatedAt
            );
    }

    public async Task<IEnumerable<ChatMessageResponse>> GetMessages(Guid roomId)
    {
        var chatMessages = await context.ChatMessages
            .AsNoTracking()
            .Include(cm => cm.Sender)
            .Where(cm => cm.ChatRoomId == roomId)
            .OrderBy(cm => cm.CreatedAt)
            .ToListAsync();
        return chatMessages.Select(ChatMessageResponse.MapToChatMessageResponse);
    }

    public async Task JoinRoom(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        // Check if the user connection exists
        var userConn = await context.UserConnections
            .AsNoTracking()
            .Where(cr => cr.UserId == currentUser.User!.Id)
            .Where(cr => cr.ChatRoom.RoomName == roomName)
            .FirstOrDefaultAsync();
        //check if chat room exists
        var chatRoom = await context.ChatRooms
            .AsNoTracking()
            .FirstOrDefaultAsync(cr => cr.RoomName == roomName);
        if (userConn == null)
        {
            // Add the user to the chat room
            var userConnection = new UserConnection
            {
                UserId = currentUser.User!.Id,
                ChatRoom = chatRoom ?? new ChatRoom { RoomName = roomName }
            };
        }

        context.UserConnections.Add(userConn!);
        await context.SaveChangesAsync();
        //send message to the room
        await Clients.Group(roomName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} has joined the room {roomName}");
    }

    public async Task LeaveRoom(string roomName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        // Check if the user connection exists
        var userConn = await context.UserConnections
            .AsNoTracking()
            .Where(cr => cr.UserId == currentUser.User!.Id)
            .Where(cr => cr.ChatRoom.RoomName == roomName)
            .FirstOrDefaultAsync();
        if (userConn != null)
        {
            context.UserConnections.Remove(userConn);
            await context.SaveChangesAsync();
        }
        //send message to the room
        await Clients.Group(roomName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} has left the room {roomName}");
    }

    public async Task SendMessageToRoom(string roomName, string message)
    {
        //check if the room does not exist
        var chatRoom = await context.ChatRooms
            .AsNoTracking()
            .FirstOrDefaultAsync(cr => cr.RoomName == roomName);
        if (chatRoom is null)
        {
            throw new NotFoundException("The chat room does not exist");
        }
        var chatMessage = new ChatMessage
        {
            SenderId = currentUser.User!.Id,
            ChatRoom = chatRoom,
            Content = message
        };
        await Clients.Group(roomName).SendAsync("ReceiveMessage", currentUser.User!.Username, message);
    }
}