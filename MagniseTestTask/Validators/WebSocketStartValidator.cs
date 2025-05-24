using MagniseTestTask.Interfaces;

namespace MagniseTestTask.Validators;

public class WebSocketStartValidator(IServiceProvider serviceProvider) : IWebSocketValidator
{
    public async Task ValidateAsync(string id)
    {
        using var scope = serviceProvider.CreateScope();
        var assetRepository = scope.ServiceProvider.GetRequiredService<IAssetRepository>();
        var asset = await assetRepository.GetByIdAsync(id);

        if (asset == null)
        {
            throw new ArgumentException("Invalid asset ID provided for starting WebSocket.");
        }
        
        var providers = asset.Provider.Split(',').Select(p => p.Trim());
        if (providers.All(provider => provider != "simulation"))
        {
            throw new InvalidOperationException(
                $"Only assets with provider 'simulation' are supported. Provider: {asset.Provider}");
        }
    }
}