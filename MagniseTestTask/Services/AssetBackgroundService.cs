using MagniseTestTask.Interfaces;

namespace MagniseTestTask.Services;

public class AssetBackgroundService(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Asset background service is starting...");

        using (var scope = serviceScopeFactory.CreateScope())
        {
            var assetService = scope.ServiceProvider.GetRequiredService<IAssetService>();

            try
            {
                await assetService.SaveAssetsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AssetBackgroundService: {ex.Message}");
            }
        }

        Console.WriteLine("Asset background service is stopping.");
    }
}