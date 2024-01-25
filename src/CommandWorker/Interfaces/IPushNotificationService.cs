namespace CommandWorker.Interfaces;

public interface IPushNotificationService
{
    public Task Send(IReadOnlyList<string> connectionIds, string method, object data);
}