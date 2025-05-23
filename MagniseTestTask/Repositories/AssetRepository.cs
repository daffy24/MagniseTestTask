using MagniseTestTask.DatabaseContext;
using MagniseTestTask.Interfaces;
using MagniseTestTask.Models;
using Microsoft.EntityFrameworkCore;

namespace MagniseTestTask.Repositories
{
    public class AssetRepository(ApplicationDbContext context) : IAssetRepository
    {
        public async Task AddAsync(Asset asset)
        {
            await context.Assets.AddAsync(asset);
        }
        public async Task AddOrUpdateAsync(Asset asset)
        {
            var existingAsset = await context.Assets.FindAsync(asset.Id);
            if (existingAsset != null)
            {
                existingAsset.Name = asset.Name;
                existingAsset.Symbol = asset.Symbol;
                context.Assets.Update(existingAsset);
            }
            else
            {
                await context.Assets.AddAsync(asset);
            }
        }
  
        public async Task<List<Asset>> GetAllAsync()
        {
            return await context.Assets.ToListAsync();
        }

        
        public async Task<bool> HasAnyAssetsAsync()
        {
            return await context.Assets.AnyAsync();
        }
        
        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
        public async Task<Asset?> GetByIdAsync(string id)
        {
            return await context.Assets.FindAsync(id);
        }
    }
}