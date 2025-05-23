using MagniseTestTask.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MagniseTestTask.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebSocketController(IWebSocketService webSocketService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> StartWebSocket(string id)
    {
        _ = Task.Run(() => webSocketService.StartListeningAsync(id));
        return Ok("WebSocket started.");
    }

    [HttpGet("updates")]
    public IActionResult GetUpdates()
    {
        var updates = webSocketService.GetLatestUpdates();
        return Ok(updates);
    }
}