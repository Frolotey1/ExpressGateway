using ExpressGateway.Services;

namespace ExpressGateway.Models;

public class ChatListResponse {
    public List<ChatInfo> Chats {get; set;} = new();
    public int Total {get; set;}
}