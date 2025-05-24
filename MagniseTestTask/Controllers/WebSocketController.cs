using MagniseTestTask.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MagniseTestTask.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebSocketController(IWebSocketService webSocketService, IWebSocketValidator webSocketValidator)
    : ControllerBase
{
    [HttpGet("{id}/start")]
    public async Task<IActionResult> StartWebSocket(string id)
    {
        try
        {
            await webSocketValidator.ValidateAsync(id);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                Error = ex.Message
            });
        }

        _ = Task.Run(() => webSocketService.StartListeningAsync(id));
        return Ok(new
        {
            message = "WebSocket listening started. Check updates for price.",
            assetId = id,
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("updates")]
    public IActionResult GetUpdates()
    {
        var updates = webSocketService.GetLatestUpdates();
        return Ok(updates);
    }
    [HttpGet("stop")]
    public async Task<IActionResult> StopWebSocket()
    {
        try
        {
            await webSocketService.StopListeningAsync();
        }
        catch (Exception e)
        {
            return Ok(new
            {
                message = "WebSocket listening stoped.",
                timestamp = DateTime.UtcNow
            });
        }

        return BadRequest(new
        {
            message = "No active WebSocket connection to stop",
            timestamp = DateTime.UtcNow
        });
    }
}