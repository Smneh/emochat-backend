namespace Contract.Events;

public class GroupMemberRemovedEvent
{
    public string GroupId { get; set; }
    public string Member { get; set; }
}