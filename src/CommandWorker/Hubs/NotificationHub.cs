using System.Collections;
using System.Globalization;
using Contract.Commands.PresenceEngine;
using Contract.DTOs.PresenceEngine;
using Contract.Enums;
using Core.Extensions;
using Core.Settings;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Repository.AeroSpike;
using ClientType = Contract.Enums.ClientType;

namespace CommandWorker.Hubs;

public class NotificationHub : Hub
{
    private readonly AerospikeRepository _aerospikeRepository;
    private readonly string _setName;
    private readonly ISender _sender;

    public NotificationHub(AerospikeRepository aerospikeRepository, ISender sender)
    {
        _aerospikeRepository = aerospikeRepository;
        _setName = Settings.AllSettings.AerospikeSettings.SetName;
        _sender = sender;
    }

    public override async Task OnConnectedAsync() {
        var token = _getToken();

        if (!string.IsNullOrEmpty(token))
        {
            var userInfo = _getUserInfo(token);

            var record = await _aerospikeRepository.ReadAsync(_setName, userInfo.Username);
            var currentTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            var ip = GetClientIpAddress();
            var tokenEntry = CreateTokenEntry(ip, token, userInfo.SessionId);
            var tokensAndIps = RetrieveUserConnections(record);
            var inCallWithUsernames = RetrieveInCallWith(record);
            var updatedConnection = AddConnectedToken(tokenEntry, tokensAndIps);
            var status = CheckUserStatus(updatedConnection);
            var callStatus = CheckCallStatus(inCallWithUsernames, tokensAndIps);
            await UpdateConnectedUsersInfo(record, tokensAndIps, status, currentTime, callStatus, userInfo);
            await SendUserStatus(currentTime, status, userInfo);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var token = _getToken();

        if (!string.IsNullOrEmpty(token))
        {
            var userInfo = _getUserInfo(token);

            var record = await _aerospikeRepository.ReadAsync(_setName, userInfo.Username);
            var currentTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            var tokensAndIps = RetrieveUserConnections(record);
            var inCallWithUsernames = RetrieveInCallWith(record);
            var updatedConnection = RemoveDisconnectedToken(token, tokensAndIps);
            var status = CheckUserStatus(updatedConnection);
            var callStatus = CheckCallStatus(inCallWithUsernames, tokensAndIps);
            await UpdateDisconnectedUserInfo(tokensAndIps, status, currentTime, callStatus, userInfo);
            await SendUserStatus(currentTime, status, userInfo);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private string GetClientIpAddress()
    {
        var httpContext = Context.GetHttpContext();
        return httpContext.Connection.RemoteIpAddress.ToString();
    }

    private Dictionary<string, string> CreateTokenEntry(string ip, string token , string sessionId)
    {
        return new Dictionary<string, string>
            { { "token", token }, { "ip", ip }, { "connectionId", Context.ConnectionId } ,{ "sessionId", sessionId.ToString() }};
    }


    private List<Dictionary<string, string>> RetrieveUserConnections(IReadOnlyDictionary<string, object> record)
    {
        var tokensAndIps = new List<Dictionary<string, string>>();
        if (!record.ContainsKey("connectionsInfo")) return tokensAndIps;
        var tokensAndIpsObjects = (List<object>)record["connectionsInfo"];

        // Convert each object in the list to a Dictionary<string, string>
        for (var index = 0; index < tokensAndIpsObjects.Count; index++)
        {
            var tokenAndIpObject = tokensAndIpsObjects[index];
            var tokenAndIp = tokenAndIpObject as Dictionary<object, object>;
            var tokenAndIpString = tokenAndIp!.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value.ToString());

            tokensAndIps.Add(tokenAndIpString!);
        }

        return tokensAndIps;
    }

    private List<string> RetrieveInCallWith(IReadOnlyDictionary<string, object> record)
    {
        var inCallWithUsernames = new List<string>();
        if (!record.ContainsKey("inCallWith")) return inCallWithUsernames;

        var inCallWithUsernamesObject = (List<object>)record["inCallWith"];

        // Convert each object in the list to a Dictionary<string, string>
        for (var index = 0; index < inCallWithUsernamesObject.Count; index++)
        {
            var username = inCallWithUsernamesObject[index];
            inCallWithUsernames.Add((string)username);
        }

        return inCallWithUsernames;
    }

    private List<Dictionary<string, string>> AddConnectedToken(Dictionary<string, string> tokenEntry,
        List<Dictionary<string, string>> tokensAndIps)
    {
        tokensAndIps.Add(tokenEntry);
        return tokensAndIps;
    }

    private List<Dictionary<string, string>> RemoveDisconnectedToken(string token,
        List<Dictionary<string, string>> tokensAndIps)
    {
        tokensAndIps.RemoveAll(tokenAndIp => tokenAndIp["token"] == token);
        return tokensAndIps;
    }

    private string CheckUserStatus(ICollection tokensAndIps)
    {
        return tokensAndIps.Count > 0 ? "online" : "offline";
    }

    private CallStatus CheckCallStatus(List<string> inCallWithUsernames, List<Dictionary<string, string>> tokensAndIps)
    {
        return tokensAndIps.Count switch
        {
            // if user connected more than one device and if user is in call with another user
            > 1 when inCallWithUsernames.Count == 2 => CallStatus.Busy,
            0 => CallStatus.NotAvailable,
            _ => CallStatus.Available
        };
    }

    private async Task UpdateConnectedUsersInfo(IReadOnlyDictionary<string, object> record,
        ICollection<Dictionary<string, string>> tokensAndIps, string status, string currentTime, CallStatus callStatus,
        UserInfo userInfo)
    {
        Dictionary<string, object> bins;
        if (record.Count == 0)
        {
            bins = new Dictionary<string, object>
            {
                { "username", userInfo.Username },
                { "status", status },
                { "connectionType", 1 },
                { "clientType", (int)userInfo.ClientType },
                { "clientUniqueId", userInfo.ClientUniqueId },
                { "lastActionTime", currentTime },
                { "connectionsInfo", tokensAndIps },
                { "callStatus", (int)CallStatus.Available },
                { "inCallWith", new List<string>() }
            };
        }
        else
        {
            bins = new Dictionary<string, object>
            {
                { "status", status },
                { "lastActionTime", currentTime },
                { "connectionsInfo", tokensAndIps },
                { "callStatus", (int)callStatus },
            };
        }


        await _aerospikeRepository.WriteAsync(_setName, userInfo.Username, bins);
    }

    private async Task UpdateDisconnectedUserInfo(List<Dictionary<string, string>> tokensAndIps, string status,
        string currentTime, CallStatus callStatus, UserInfo userInfo)
    {
        var bins = new Dictionary<string, object>
        {
            { "status", status },
            { "lastActionTime", currentTime },
            { "connectionsInfo", tokensAndIps },
            { "callStatus", (int)callStatus },
        };

        await _aerospikeRepository.WriteAsync(_setName, userInfo.Username, bins);
    }


    private async Task SendUserStatus(string currentTime, string status, UserInfo userInfo)
    {
        await _sender.Send(new SendUserStatusCommand
        {
            Username = userInfo.Username,
            Status = status,
            LastActionTime = currentTime
        });
    }


    private string _getToken()
    {
        try
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null)
                throw new Exception("...");

            var query = httpContext.Request.Query;

            return query["access_token"]!;
        }
        catch (Exception)
        {
            return "";
        }
    }

    private UserInfo _getUserInfo(string token)
    {
        var username = GetCurrentUsername(token);

        if (username == "") return new UserInfo();

        var sessionId = token.GetClaimById<string>("SI");

        return new UserInfo
        {
            Username = username,
            ClientType = ClientType.Web,
            ClientUniqueId = "",
            SessionId = sessionId,
        };
    }

    public static string GetCurrentUsername(string token)
    {
        try
        {
            var username = token.GetClaimById<string>("UN");
            return !string.IsNullOrWhiteSpace(username) ? username : "";
        }
        catch (Exception)
        {
            return "";
        }
    }

    public static ClientType GetClientType(string clientToken)
    {
        return clientToken switch
        {
            "M" => ClientType.Mobile,
            "W" => ClientType.Web,
            _ => ClientType.None
        };
    }
}