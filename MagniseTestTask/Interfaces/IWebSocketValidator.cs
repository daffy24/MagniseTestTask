namespace MagniseTestTask.Interfaces;

public interface IWebSocketValidator
{
    Task ValidateAsync(string id);
}