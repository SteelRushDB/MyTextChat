using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MyTextChat.Models;

namespace MyTextChat.Services;

public class WebSocketHandler
{
    private static readonly List<WebSocket> Clients = new();
    private static List<ChatMessage> _messageHistory = new List<ChatMessage>();
    private static Dictionary<string, WebSocket> _connectedUsers = new Dictionary<string, WebSocket>();
    
    public async Task HandleWebSocket(HttpContext context, WebSocket webSocket)
    {
        Clients.Add(webSocket);
        Console.WriteLine("+client");
        
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        
        while (!result.CloseStatus.HasValue)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var request = JsonSerializer.Deserialize<ChatRequest>(message);
            
            if (request.Command == "joinChat")
            {
                var joinChatRequest= JsonSerializer.Deserialize<JoinChatRequest>(message);
                await HandleJoinChat(joinChatRequest, webSocket);
            }
            else if (request.Command == "sendPublicMessage")
            {
                var MessageRequest= JsonSerializer.Deserialize<SendPublicMessageRequest>(message);
                await HandleSendPublicMessage(MessageRequest, webSocket);
            }
            else if (request.Command == "sendPrivateMessage")
            {
                var MessageRequest= JsonSerializer.Deserialize<SendPrivateMessageRequest>(message);
                await HandleSendPrivateMessage(MessageRequest, webSocket);
            }
            
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }
        
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        
        Clients.Remove(webSocket);
        Console.WriteLine("-client");
    }


    public async Task HandleJoinChat(JoinChatRequest request, WebSocket webSocket)
    {
        var name = request.Name;
        if (name == null) await SendError("Bad Request: Не был введен UserName", "joinChat", webSocket);
        
        if (!_connectedUsers.ContainsKey(name))
        {
            _connectedUsers[name] = webSocket;

            var response = new JoinChatResponse
            {
                ChatHistory = _messageHistory
            };
            var historyMessageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
            await webSocket.SendAsync(new ArraySegment<byte>(historyMessageBytes), 
                WebSocketMessageType.Text, true, CancellationToken.None);
        }
        else
        {
            await SendError("В чате уже есть пользователь с таким именем", "joinChat", webSocket);
        }
    }
    
    public async Task HandleSendPrivateMessage(SendPrivateMessageRequest request, WebSocket webSocket)
    {
        //TODO сделать добавление приватных сообщений в историю 
        
        var response = new SendMessageResponse()
        {
            Text = request.ChatMessage.Text,
            Source = request.ChatMessage.Source,
            
            Target = request.ChatMessage.Target
        };
        
        if (response.Target != null)
        {
            var targetClient = _connectedUsers[response.Target];
            if (targetClient == null)
            {
                await SendError($"В чате нет пользователя с именем {response.Target}", "sendPrivateMessage",webSocket);
            }
            else
            {
                var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
                await targetClient.SendAsync(new ArraySegment<byte>(messageBytes), 
                    WebSocketMessageType.Text, true, CancellationToken.None); 
                // !Важно! Отправляем ответ конкретному targetClient
            }
        }
        else
        {
            await SendError($"Bad Request: В приватном сообщении не оказалось получателя", "sendPrivateMessage", webSocket);
        }
    }

    public async Task HandleSendPublicMessage(SendPublicMessageRequest request, WebSocket webSocket)
    {
        _messageHistory.Add(request.ChatMessage);
        
        var response = new SendMessageResponse()
        {
            Text = request.ChatMessage.Text,
            Source = request.ChatMessage.Source,
        };
        
        foreach (var client in Clients)
        {
            if (client.State == WebSocketState.Open)
            {
                var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
                await client.SendAsync(new ArraySegment<byte>(messageBytes), 
                    WebSocketMessageType.Text, true, CancellationToken.None);
                // !Важно! Отправляем ответ каждому client
            }
        }
    }
    
    //Метод отправки ошибки
    public async Task SendError(string text, string sourceCommand, WebSocket webSocket)
    {
        var errorResponse = new ErrorResponse
        {
            ErrorText = text,
            SourceCommand = sourceCommand
        };
        var errorMessageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(errorResponse));
        await webSocket.SendAsync(new ArraySegment<byte>(errorMessageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}