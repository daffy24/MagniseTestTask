namespace MagniseTestTask.Interfaces;

public interface ITokenManager
{
    Task<string> GetAccessTokenAsync(string username, string password);
}