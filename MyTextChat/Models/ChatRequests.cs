namespace MyTextChat.Models;


public class ChatRequest
{
    public string Command { get; set; }  // Поле команды (например, joinChat)
}

public class JoinChatRequest
{
    public string Command { get; set; } = "joinChat";
    public string Name { get; set; }  // Имя пользователя
}

public class SendPublicMessageRequest
{
    public string Command { get; set; } = "sendPublicMessage";
    public ChatMessage ChatMessage { get; set; }
}

public class SendPrivateMessageRequest
{
    public string Command { get; set; } = "sendPrivateMessage";
    public ChatMessage ChatMessage { get; set; }
}
