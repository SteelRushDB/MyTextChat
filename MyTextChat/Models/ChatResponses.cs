namespace MyTextChat.Models;

public class JoinChatResponse
{
    public string Command { get; set; } = "history";
    public List<ChatMessage> ChatHistory { get; set; } // История сообщений
}

public class SendMessageResponse
{
    public string Command { get; set; } = "receiveMessage";
    public string Text { get; set; }
    public string Source { get; set; }
    public string? Target{ get; set; }
}

public class ErrorResponse
{
    public string Command { get; set; } = "error";
    public string ErrorText { get; set; }
    public string SourceCommand { get; set; }
}