using MagniseTestTask.DTOs;

namespace MagniseTestTask.Interfaces;

public interface IAssetService
{
    Task SaveAssetsAsync();
    Task<IEnumerable<AssetDto>> GetAssetsAsync();
    Task<List<object>> GetAssetPricesAsync(string assetId);
}