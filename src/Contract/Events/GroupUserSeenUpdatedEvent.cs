namespace Contract.Events;

public class GroupUserSeenUpdatedEvent
{
    public string GroupId { get; set; }
    public long LastMessageId { get; set; }
}