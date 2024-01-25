namespace Contract.Events;

public class GroupMemberAddedEvent
{
    public string GroupId { get; set; }
    public string Member { get; set; }
}