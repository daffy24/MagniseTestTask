using System.Text.Json;
using MagniseTestTask.Token;
using MagniseTestTask.DTOs;
using MagniseTestTask.Interfaces;
using MagniseTestTask.Models;

namespace MagniseTestTask.Services;

public class AssetService(IAssetRepository repository, IHttpClientFactory httpClientFactory, TokenManager tokenManager)
    : IAssetService
{
    private async Task<HttpClient> GetHttpClientWithTokenAsync()
    {
        var httpClient = httpClientFactory.CreateClient();
        var accessToken = await tokenManager.GetAccessTokenAsync("r_test@fintatech.com", "kisfiz-vUnvy9-sopnyv");
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        return httpClient;
    }

    public async Task SaveAssetsAsync()
    {
        if (await repository.HasAnyAssetsAsync())
        {
            Console.WriteLine("Database already contains assets. Skipping data fetching.");
            return;
        }

        var httpClient = await GetHttpClientWithTokenAsync();

        var initialResponse =
            await httpClient.GetAsync("https://platform.fintacharts.com/api/instruments/v1/instruments?page=1");
        initialResponse.EnsureSuccessStatusCode();

        var initialContent = await initialResponse.Content.ReadAsStringAsync();
        var initialJsonData = JsonDocument.Parse(initialContent);

        var paging = initialJsonData.RootElement.GetProperty("paging");
        int totalPages = paging.GetProperty("pages").GetInt32();

        var tasks = Enumerable.Range(1, totalPages).Select(async page =>
        {
            var response =
                await httpClient.GetAsync(
                    $"https://platform.fintacharts.com/api/instruments/v1/instruments?page={page}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jsonData = JsonDocument.Parse(content);

            return jsonData.RootElement.GetProperty("data")
                .EnumerateArray()
                .Select(a => new Asset
                {
                    Id = a.GetProperty("id").GetString(),
                    Name = a.GetProperty("profile").GetProperty("name").GetString(),
                    Symbol = a.GetProperty("symbol").GetString(),
                    Provider = string.Join(
                        ",",
                        a.GetProperty("mappings").EnumerateObject().Select(p => p.Name)
                    )
                }).ToList();
        });

        var assetsPerPage = await Task.WhenAll(tasks);
        var allAssets = assetsPerPage.SelectMany(x => x).ToList();

        foreach (var asset in allAssets)
        {
            await repository.AddAsync(asset);
        }

        await repository.SaveChangesAsync();
        Console.WriteLine("Assets have been saved successfully.");
    }


    public async Task<List<object>> GetAssetPricesAsync(string assetId)
{
    var asset = await repository.GetByIdAsync(assetId);
    if (asset == null || string.IsNullOrWhiteSpace(asset.Provider))
    {
        throw new Exception($"Asset with ID {assetId} does not exist or has no providers.");
    }

    var providers = asset.Provider.Split(',').Select(p => p.Trim());
    var name = asset.Name;
    var httpClient = await GetHttpClientWithTokenAsync();

    var results = new List<object>();

    foreach (var provider in providers)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1000));

            var url =
                $"https://platform.fintacharts.com/api/bars/v1/bars/count-back?instrumentId={assetId}&provider={provider}&interval=1&periodicity=minute&barsCount=1";
            var responseTask = httpClient.GetAsync(url, cts.Token);

            var response = await responseTask;

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonData = JsonDocument.Parse(content);

                var price = jsonData.RootElement.GetProperty("data").EnumerateArray().First();

                results.Add(new
                {
                    Name = name,
                    Provider = provider,
                    UpdateTime = price.GetProperty("t").GetString(),
                    Price = price.GetProperty("c").GetDecimal()
                });
            }
            else
            {
                Console.WriteLine($"Failed to fetch price for provider {provider}. StatusCode: {response.StatusCode}");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Request for provider {provider} timed out.");
            results.Add(new
            {
                Name = name,
                Provider = provider,
                Message = "This asset is not supported."
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching price for asset {assetId} from provider {provider}: {ex.Message}");
        }
    }

    if (!results.Any())
    {
        throw new Exception($"Failed to fetch price for asset {assetId} from all providers.");
    }

    return results;
}

    public async Task<IEnumerable<AssetDto>> GetAssetsAsync()
    {
        var assetsFromDatabase = await repository.GetAllAsync();

        return assetsFromDatabase.Select(asset => new AssetDto
        {
            Id = asset.Id,
            Name = asset.Name,
            Symbol = asset.Symbol,
        }).ToList();
    }

   
}