using MagniseTestTask.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MagniseTestTask.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController(IAssetService service) : ControllerBase
{
    [HttpGet("all")]
    public async Task<IActionResult> GetAllAssets()
    {
        var assets = await service.GetAssetsAsync();
        return Ok(assets);
    }

    [HttpGet("{id}/price")]
    public async Task<IActionResult> GetPrice(string id)
    {
        var price = await service.GetAssetPricesAsync(id);
        return Ok(price);
    }
}