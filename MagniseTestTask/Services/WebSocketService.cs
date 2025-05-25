using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MagniseTestTask.Interfaces;

namespace MagniseTestTask.Services;

public class WebSocketService : IWebSocketService
{
    private readonly ConcurrentDictionary<string, object> _latestUpdates = new();
    private readonly Uri _webSocketUri;
  
    private ClientWebSocket? _activeWebSocket;

    public WebSocketService(ITokenManager tokenManager, IConfiguration configuration)
    {
        var baseUri = configuration.GetSection("WebSocketSettings").GetSection("BaseUri").Value;
        var accessToken = tokenManager.GetAccessTokenAsync().Result;
        _webSocketUri = new Uri($"{baseUri}?token={accessToken}");
    }

    public async Task StartListeningAsync(string id)
    {
        if (_activeWebSocket != null && _activeWebSocket.State == WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is already running. Stop it before starting a new one.");
        }

        _activeWebSocket = new ClientWebSocket();

        try
        {
            await _activeWebSocket.ConnectAsync(_webSocketUri, CancellationToken.None);
            Console.WriteLine("WebSocket connection established.");

            await SendSubscriptionMessageAsync(_activeWebSocket, id);

            var buffer = new byte[1024 * 4];

            while (_activeWebSocket.State == WebSocketState.Open)
            {
                var result = await _activeWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("WebSocket connection closed.");
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ProcessMessage(message);
                }
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Stopping WebSocket connection.");
        }
        finally
        {
            _activeWebSocket?.Dispose();
            _activeWebSocket = null;
        }
    }

    public async Task StopListeningAsync()
    {
        if (_activeWebSocket == null || _activeWebSocket.State != WebSocketState.Open)
        {
            Console.WriteLine("No active WebSocket connection to stop.");
            return;
        }

        try
        {
            await _activeWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Stopped by client", CancellationToken.None);
            Console.WriteLine("WebSocket connection closed by client.");
        }
        catch (Exception)
        {
            Console.WriteLine("WebSocket stoped");
        }
        finally
        {
            _activeWebSocket.Dispose();
            _activeWebSocket = null;
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

        await webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
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

                    _latestUpdates["ask"] = new { Price = price, Timestamp = timestamp };
                    Console.WriteLine($"Ask Price: {price}, Timestamp: {timestamp}");
                }
                else if (jsonData.RootElement.TryGetProperty("bid", out var bidElement))
                {
                    var price = bidElement.GetProperty("price").GetDecimal();
                    var timestamp = bidElement.GetProperty("timestamp").GetString();

                    _latestUpdates["bid"] = new { Price = price, Timestamp = timestamp };
                    Console.WriteLine($"Bid Price: {price}, Timestamp: {timestamp}");
                }
                else if (jsonData.RootElement.TryGetProperty("last", out var lastElement))
                {
                    var price = lastElement.GetProperty("price").GetDecimal();
                    var timestamp = lastElement.GetProperty("timestamp").GetString();

                    _latestUpdates["last"] = new { Price = price, Timestamp = timestamp };
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
