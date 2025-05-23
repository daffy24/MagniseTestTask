namespace MagniseTestTask.Interfaces;

public interface IWebSocketService
{
    Task StartListeningAsync(string id);
    Object GetLatestUpdates();
}