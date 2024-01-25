using CommandWorker.Hubs;
using CommandWorker.Interfaces;
using Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CommandWorker.Services;

public class PushPushNotificationService : IPushNotificationService, ISingletonDependency
{
    private readonly IHubContext<NotificationHub> _hubContext;


    public PushPushNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Send(IReadOnlyList<string> connectionIds, string method, object data)
    {
        await _hubContext.Clients.Clients(connectionIds).SendAsync(method, data);
    }
}