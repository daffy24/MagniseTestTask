using MagniseTestTask.Models;

namespace MagniseTestTask.Interfaces;

public interface IAssetRepository
{
    Task AddAsync(Asset asset);

    Task<List<Asset>> GetAllAsync();
    Task<bool> HasAnyAssetsAsync();
    Task SaveChangesAsync();

    Task<Asset?> GetByIdAsync(string id);


}