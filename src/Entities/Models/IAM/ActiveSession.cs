namespace Entities.Models.IAM;

public class ActiveSession
{
    public string Username { get; set; }
    public string Ip { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime ExpireTime { get; set; }
    public string UniqueId { get; set; }
    public bool IsSuccessor { get; set; }
    public bool IsSetToKillWebSessions { get; set; }
}