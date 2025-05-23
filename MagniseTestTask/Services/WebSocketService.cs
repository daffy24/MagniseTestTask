using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MagniseTestTask.Interfaces;
using MagniseTestTask.Token;

namespace MagniseTestTask.Services;

public class WebSocketService : IWebSocketService
{
    private readonly ConcurrentDictionary<string, object> _latestUpdates = new();
    private readonly Uri _webSocketUri;

    public WebSocketService(TokenManager tokenManager)
    {
        var accessToken = tokenManager.GetAccessTokenAsync("r_test@fintatech.com", "kisfiz-vUnvy9-sopnyv").Result;

        _webSocketUri = new Uri($"wss://platform.fintacharts.com/api/streaming/ws/v1/realtime?token={accessToken}");
    }

    public async Task StartListeningAsync(string id)
    {
        using var webSocket = new ClientWebSocket();

        try
        {
            await webSocket.ConnectAsync(_webSocketUri, CancellationToken.None);
            Console.WriteLine("WebSocket connection established.");

            await SendSubscriptionMessageAsync(webSocket, id);

            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("WebSocket connection closed.");
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client",
                        CancellationToken.None);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ProcessMessage(message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket error: {ex.Message}");
        }
    }

    public async Task SendSubscriptionMessageAsync(ClientWebSocket webSocket, string id)
    {
        var subscriptionMessage = new
        {
            type = "l1-subscription",
            id = "1",
            instrumentId = id,
            provider = "simulation",
            subscribe = true,
            kinds = new[] { "ask", "bid", "last" }
        };

        var messageJson = JsonSerializer.Serialize(subscriptionMessage);
        var messageBytes = Encoding.UTF8.GetBytes(messageJson);

        await webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true,
            CancellationToken.None);
        Console.WriteLine("Subscription message sent.");
    }


    public void ProcessMessage(string message)
    {
        try
        {
            var jsonData = JsonDocument.Parse(message);
            var type = jsonData.RootElement.GetProperty("type").GetString();

            if (type == "response")
            {
                Console.WriteLine("Subscription response received.");
            }

            if (type == "l1-update")
            {
                if (jsonData.RootElement.TryGetProperty("ask", out var askElement))
                {
                    var price = askElement.GetProperty("price").GetDecimal();
                    var timestamp = askElement.GetProperty("timestamp").GetString();

                    _latestUpdates["ask"] = new
                    {
                        Price = price,
                        Timestamp = timestamp
                    };

                    Console.WriteLine($"Ask Price: {price}, Timestamp: {timestamp}");
                }
                else if (jsonData.RootElement.TryGetProperty("bid", out var bidElement))
                {
                    var price = bidElement.GetProperty("price").GetDecimal();
                    var timestamp = bidElement.GetProperty("timestamp").GetString();

                    _latestUpdates["bid"] = new
                    {
                        Price = price,
                        Timestamp = timestamp
                    };

                    Console.WriteLine($"Bid Price: {price}, Timestamp: {timestamp}");
                }
                else if (jsonData.RootElement.TryGetProperty("last", out var lastElement))
                {
                    var price = lastElement.GetProperty("price").GetDecimal();
                    var timestamp = lastElement.GetProperty("timestamp").GetString();

                    _latestUpdates["last"] = new
                    {
                        Price = price,
                        Timestamp = timestamp
                    };

                    Console.WriteLine($"Last Price: {price}, Timestamp: {timestamp}");
                }
                else
                {
                    Console.WriteLine("Unexpected update type or missing data.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
        }
    }

    public object GetLatestUpdates()
    {
        return _latestUpdates;
    }
}