﻿using System.Text.Json.Serialization;

namespace MagniseTestTask.Token;

public class TokenResponse
{
    [JsonPropertyName("access_token")] 
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")] 
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
}