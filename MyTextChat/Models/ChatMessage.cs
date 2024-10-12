namespace MyTextChat.Models;

public class ChatMessage
{
    public string Source { get; set; }  // Имя отправителя
    public string? Target { get; set; }  // Имя получателя
    public string Text { get; set; }    // Текст сообщения
    public DateTime Timestamp { get; set; } = DateTime.Now; // Время отправки
}
