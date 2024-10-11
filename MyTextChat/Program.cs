using System.Net.WebSockets;
using System.Text;
using MyTextChat.Services;

namespace MyTextChat;

public class Program
{
    
    
    public static void Main(string[] args)
    {
        
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        
        app.UseWebSockets();
        // Вызов обработчика для маршрута "/ws"
        app.Map("/ws", async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var webSocketHandler = new WebSocketHandler();
                await webSocketHandler.HandleWebSocket(context, webSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        });
        
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}