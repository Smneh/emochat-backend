using System.Text.Json;
using Aerospike.Client;
using CommandWorker.Interfaces;
using Contract.Commands.PresenceEngine;
using MediatR;
using Newtonsoft.Json;

namespace CommandWorker.Handlers;

public class SignalrUsersStatusHandler : IRequestHandler<SignalrUsersStatus>
{
    private readonly IPushNotificationService _pushNotificationService;

    public SignalrUsersStatusHandler(IPushNotificationService pushNotificationService)
    {
        _pushNotificationService = pushNotificationService;
    }

    public async Task<Unit> Handle(SignalrUsersStatus data, CancellationToken cancellationToken)
    {
       await PushData(data);
       return Unit.Value;
    }

    private async Task PushData(SignalrUsersStatus data)
    {
        await SendUsersDataToUserItSelf(data);
        await SendUserDataToOtherUsers(data);
    }

    private async Task SendUsersDataToUserItSelf(SignalrUsersStatus data)
    {
        var message = JsonDocument.Parse((string) data.Message);
        var status = message.RootElement.GetProperty("status").GetString();
        if (status == "offline") return;

        List<string> connectionIds = new List<string>();
        List<object> otherUsersInfos = new List<object>();

        foreach (var record in data.UsersInfo)
        {
            var receiverUsername = record.GetValue("username").ToString();

            if (receiverUsername == data.Sender)
            {
                connectionIds = GetConnectionIds(record);
            }
            else
            {
                otherUsersInfos.Add(new
                {
                    username = receiverUsername,
                    lastActionTime = record.GetValue("lastActionTime").ToString(),
                    status = record.GetValue("status").ToString()
                });
            }
        }

        var dataToSend = JsonConvert.SerializeObject(otherUsersInfos);
        await _pushNotificationService.Send(connectionIds, data.Method, dataToSend);
    }

    private async Task SendUserDataToOtherUsers(SignalrUsersStatus data)
    {
        List<string> connectionIds = new List<string>();

        var message = JsonDocument.Parse((string) data.Message);
        foreach (var record in data.UsersInfo)
        {
            var receiverUsername = record.GetValue("username").ToString();

            if (receiverUsername == data.Sender) continue;
            
            var connections =  GetConnectionIds(record);
            connectionIds.AddRange(connections);
        }

        var userInfo = new List<object>
        {
            new
            {
                username = message.RootElement.GetProperty("username").GetString(),
                lastActionTime = message.RootElement.GetProperty("lastActionTime").GetString(),
                status = message.RootElement.GetProperty("status").GetString(),
            }
        };
        var dataToSend = JsonConvert.SerializeObject(userInfo);

        await _pushNotificationService.Send(connectionIds, data.Method, dataToSend);
    }

    
    private List<string> GetConnectionIds(Record record)
    {
        var bins = record?.bins.ToDictionary(bin => bin.Key, bin => bin.Value);

        var tokensAndIpsObjects = (List<object>) bins["connectionsInfo"];
        var tokensAndIps = new List<Dictionary<string, string>>();
        for (var i = 0; i < tokensAndIpsObjects.Count; i++)
        {
            var tokenAndIp = tokensAndIpsObjects[i] as Dictionary<object, object>;
            var tokenAndIpString = tokenAndIp!.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value.ToString());

            tokensAndIps.Add(tokenAndIpString!);
        }

        var connectionIds = tokensAndIps.Select(tokenAndIp => tokenAndIp["connectionId"]).ToList();

        return connectionIds;
    }
}