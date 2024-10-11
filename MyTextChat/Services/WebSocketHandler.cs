using System.Net.WebSockets;
using System.Text;

namespace MyTextChat.Services;

public class WebSocketHandler
{
    private static readonly List<WebSocket> Clients = new();
    private static List<string> _messageHistory = new List<string>();
    
    public async Task HandleWebSocket(HttpContext context, WebSocket webSocket)
    {
        Clients.Add(webSocket);
        Console.WriteLine("+client");
        
        // Отправляем историю сообщений новому клиенту сразу после подключения
        foreach (var message in _messageHistory)
        {
            var historyMessageBytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(historyMessageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

       
        
        while (!result.CloseStatus.HasValue)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            _messageHistory.Add(message);
            
            // Отправляем сообщение всем подключенным клиентам
            foreach (var client in Clients)
            {
                if (client.State == WebSocketState.Open)
                {
                    var messageBytes = Encoding.UTF8.GetBytes(message);
                    await client.SendAsync(new ArraySegment<byte>(messageBytes, 0, messageBytes.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                }
            }

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        Clients.Remove(webSocket);
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        Console.WriteLine("-client");
    }
}