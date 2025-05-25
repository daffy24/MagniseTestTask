namespace MagniseTestTask.Interfaces;

public interface ITokenManager
{
    Task<string> GetAccessTokenAsync();
}