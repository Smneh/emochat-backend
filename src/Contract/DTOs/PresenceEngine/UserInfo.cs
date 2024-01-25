using Contract.Enums;

namespace Contract.DTOs.PresenceEngine;

public class UserInfo
{
    public string Username { get; set; }
    public ClientType ClientType { get; set; }
    public string SessionId { get; set; }
    public string ClientUniqueId { get; set; }
}