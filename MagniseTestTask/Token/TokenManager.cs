
using MagniseTestTask.Interfaces;

namespace MagniseTestTask.Token;

using System.Text.Json;

public class TokenManager(HttpClient httpClient): ITokenManager
{
    private string? _accessToken;
    private string? _refreshToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public async Task<string> GetAccessTokenAsync(string username, string password)
    {
        if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiry > DateTime.UtcNow)
        {
            return _accessToken;
        }

        if (!string.IsNullOrEmpty(_refreshToken))
        {
            try
            {
                return await RefreshAccessTokenAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to refresh token: {ex.Message}");
            }
        }

        return await RequestNewTokenAsync(username, password);
    }

    private async Task<string> RequestNewTokenAsync(string username, string password)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id", "app-cli" },
            { "username", username },
            { "password", password }
        });

        var url = $"https://platform.fintacharts.com/identity/realms/fintatech/protocol/openid-connect/token";
        var response = await httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get access token. Status code: {response.StatusCode}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<TokenResponse>(responseBody);

        if (tokenData == null || string.IsNullOrEmpty(tokenData.AccessToken))
        {
            throw new Exception("Failed to retrieve access token.");
        }

        _accessToken = tokenData.AccessToken;
        _refreshToken = tokenData.RefreshToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);

        return _accessToken;
    }

    private async Task<string> RefreshAccessTokenAsync()
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "client_id", "app-cli" },
            { "refresh_token", _refreshToken! }
        });

        var url = $"https://platform.fintacharts.com/identity/realms/fintatech/protocol/openid-connect/token";
        var response = await httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to refresh access token. Status code: {response.StatusCode}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<TokenResponse>(responseBody);

        if (tokenData == null || string.IsNullOrEmpty(tokenData.AccessToken))
        {
            throw new Exception("Failed to retrieve refreshed access token.");
        }

        _accessToken = tokenData.AccessToken;
        _refreshToken = tokenData.RefreshToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn);

        return _accessToken;
    }
}