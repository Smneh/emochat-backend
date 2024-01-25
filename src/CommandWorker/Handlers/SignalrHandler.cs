using Aerospike.Client;
using CommandWorker.Interfaces;
using Contract.Commands.PresenceEngine;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommandWorker.Handlers;

public class SignalrHandler : INotificationHandler<SignalrCommand>
{
    private readonly IPushNotificationService _pushNotificationService;

    public SignalrHandler(IPushNotificationService pushNotificationService)
    {
        _pushNotificationService = pushNotificationService;
    }

    public async Task Handle(SignalrCommand data, CancellationToken cancellationToken)
    {
        await PushData(data);
    }

    private async Task PushData(SignalrCommand data)
    {
        foreach (var record in data.Receivers)
        {
            var connectionsInfo = GetConnectionsInfo(record);
            var activeSessionIds = GetActiveSessionIds(connectionsInfo);
            var connectionIds = GetConnectionIds(connectionsInfo,activeSessionIds);
            
            var messageData = JObject.Parse((string) data.Message);
            
            await SendPushNotification(connectionIds, data.Method, messageData);
        }
    }
    

    private async Task SendPushNotification(IReadOnlyList<string> connectionIds, string method, JObject messageData)
    {
        var dataToSend = JsonConvert.SerializeObject(messageData);
        await _pushNotificationService.Send(connectionIds, method, dataToSend);
    }

    private List<string> GetActiveSessionIds(IEnumerable<Dictionary<string, string>> activeSessions)
    {
        return activeSessions.Select(tokenAndIp => tokenAndIp["sessionId"]).ToList();
    }
    
    private List<string> GetConnectionIds(IEnumerable<Dictionary<string, string>> connections, List<string> sessionIds)
    {
        var connectionIds = connections
            .Where(session => sessionIds.Contains(session["sessionId"]))
            .Select(tokenAndIp => tokenAndIp["connectionId"])
            .ToList();
        return connectionIds;
    }
    


    private List<string> GetIp(IEnumerable<Dictionary<string, string>> connections)
    {
        return connections.Select(tokenAndIp => tokenAndIp["ip"]).ToList();
    }

    private List<string> GetTokens(IEnumerable<Dictionary<string, string>> connections)
    {
        return connections.Select(tokenAndIp => tokenAndIp["token"]).ToList();
    }

    private List<Dictionary<string, string>> GetConnectionsInfo(Record record)
    {
        var bins = record?.bins.ToDictionary(bin => bin.Key, bin => bin.Value);

        var tokensAndIpsObjects = (List<object>) bins!["connectionsInfo"];
        var tokensAndIps = new List<Dictionary<string, string>>();

        for (var i = 0; i < tokensAndIpsObjects.Count; i++)
        {
            var tokenAndIp = tokensAndIpsObjects[i] as Dictionary<object, object>;
            var tokenAndIpString = tokenAndIp!.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value.ToString());

            tokensAndIps.Add(tokenAndIpString!);
        }

        return tokensAndIps;
    }
}